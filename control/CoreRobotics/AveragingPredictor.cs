using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using Robocup.Utilities;
using Robocup.CoreRobotics;

namespace Robocup.CoreRobotics
{
    /// <summary>
    /// A basic implementation of IPredictor that averages values from multiple cameras
    /// </summary>
    public class AveragingPredictor : IPredictor, Robocup.Core.IVisionInfoAcceptor
    {
        const int NUM_CAMERAS = 2;

        // The state of the field believed in by a camera
        private class FieldState
        {
            // The believed state is kept here
            private BallInfo ball = null;
            private List<RobotInfo>[] robots = new List<RobotInfo>[] { new List<RobotInfo>(), 
                                                                       new List<RobotInfo>() };

            // Tools for velocity measurement
            double ballDtStart;
            BallInfo ballAtDtStart = null;
            List<RobotInfo>[] robotsAtDtStart = new List<RobotInfo>[NUM_CAMERAS] { new List<RobotInfo>(),
                                                                                   new List<RobotInfo>() };
            List<double>[] velocityDtStart = new List<double>[NUM_CAMERAS] { new List<double>(),
                                                                             new List<double>() };
            
            // For assigning IDs to unidentified robots; one per team
            int[] nextID = { -1, -1 };

            // "Constants"
            static double VELOCITY_DT;

            public void LoadConstants()
            {
                VELOCITY_DT = Constants.get<double>("default", "VELOCITY_DT");
            }

            // Update the believed state with new observations
            public void Update(VisionMessage msg)
            {
                double time = HighResTimer.SecondsSinceStart();

                #region Update ball
                if (msg.Ball != null)
                {
                    lock (ball) {
                        // If we see the ball for the fist time, just record it; otherwise update
                        if (ball == null)
                        {
                            ball = new BallInfo(msg.Ball.Position, new Vector2(0, 0)); // Don't know velocity yet
                            ballDtStart = time;
                            ballAtDtStart = new BallInfo(ball);
                        }
                        else
                        {
                            BallInfo newBall = msg.Ball;                            

                            // Update position
                            ball.Position = new Vector2(newBall.Position);

                            // Update velocity if a reasonable interval has passed
                            double dt = time - ballDtStart;
                            if (dt > VELOCITY_DT)
                            {
                                Vector2 d = msg.Ball.Position - ballAtDtStart.Position;
                                ball.Velocity = d / dt;

                                // Reset velocity interval
                                ballDtStart = time;
                                ballAtDtStart = new BallInfo(ball);
                            }

                            // We have just seen the ball                    
                            ball.LastSeen = time;
                        }
                    }
                }
                #endregion

                #region Update robots   
                lock (robots) {
                    foreach (Robocup.Core.VisionMessage.RobotData newRobotData in msg.Robots)
                    {
                        RobotInfo newRobot = new RobotInfo(newRobotData.Position, new Vector2(0, 0), 0,
                            newRobotData.Orientation, newRobotData.ID, -1);

                        // Keep track of nextID
                        if (newRobot.ID > nextID[newRobot.Team])
                        {
                            nextID[newRobot.Team] = newRobot.ID;
                        }

                        // Match with existing info either by ID (if vision gave it one) or by position
                        Predicate<RobotInfo> matchPredicate;
                        if (newRobot.ID < 0)
                        {
                            matchPredicate = new Predicate<RobotInfo>(delegate(RobotInfo robot)
                            {
                                return robot.ID == newRobot.ID;
                            });
                        }
                        else
                        {
                            matchPredicate = new Predicate<RobotInfo>(delegate(RobotInfo robot)
                            {
                                return robot.Position.distanceSq(newRobot.Position) < DELTA_DIST_SQ_MERGE;
                            });
                        }

                        // Find the matching robot
                        int oldRobotIdx = robots[newRobot.Team].FindIndex(matchPredicate);
                        int newRobotIdx = -1;

                        // If never seen this robot before, then add it; otherwise, update
                        if (oldRobotIdx < 0)
                        {
                            // On first frame don't know velocity                            
                            robots[newRobot.Team].Add(new RobotInfo(newRobot));
                            robotsAtDtStart[newRobot.Team].Add(new RobotInfo(newRobot));
                            velocityDtStart[newRobot.Team].Add(time);
                            newRobotIdx = robots[newRobot.Team].Count - 1;
                        }
                        else
                        {
                            RobotInfo oldRobot = robots[newRobot.Team][oldRobotIdx];

                            // Update position and orientation
                            oldRobot.Position = new Vector2(newRobot.Position);
                            oldRobot.Orientation = newRobot.Orientation;                            

                            // Update velocity if a reasonable interval has passed                    
                            double dt = time - velocityDtStart[newRobot.Team][oldRobotIdx];

                            if (dt > VELOCITY_DT)
                            {
                                Vector2 d = newRobot.Position - robotsAtDtStart[newRobot.Team][oldRobotIdx].Position;
                                oldRobot.Velocity = d / dt;

                                // Reset velocity dt interval
                                velocityDtStart[newRobot.Team][oldRobotIdx] = time;
                                robotsAtDtStart[newRobot.Team][oldRobotIdx] = new RobotInfo(oldRobot);
                            }
                            newRobotIdx = oldRobotIdx;
                        }

                        // We have just seen this robot
                        robots[newRobot.Team][newRobotIdx].LastSeen = time;                        
                    }

                    // Assign IDs to any unidentified robots
                    for (int team = 0; team < 2; team++)
                    {
                        foreach (RobotInfo robot in robots[team])
                        {
                            if (robot.ID < 0)
                            {
                                robot.ID = ++nextID[team];                                
                            }
                        }
                    }
                }
                #endregion
            }

            public BallInfo GetBall()
            {
                BallInfo retBall;
                lock (ball) {
                    // Reconsider our belief
                    if (ball.LastSeen > MAX_SECONDS_TO_KEEP_INFO)
                    {
                        ball = null;
                    }
                    // Copy data for returning
                    retBall = (ball != null) ? new BallInfo(ball) : null;
                }
                // It's ok to return null if we don't know anything about the ball
                return retBall;
            }

            public List<RobotInfo> GetRobots(int team)
            {
                List<RobotInfo> retRobots;
                lock (robots) {
                    // Reconsider our belief
                    List<int> idxToRemove = new List<int>();
                    for (int i = 0; i < robots[team].Count; i++)
                    {
                        if (robots[team][i].LastSeen > MAX_SECONDS_TO_KEEP_INFO)
                        {
                            idxToRemove.Add(i);
                        }
                    }
                    foreach (int idx in idxToRemove) {
                        robots[team].RemoveAt(idx);
                    }

                    // Copy data for returning               
                    retRobots = new List<RobotInfo>(robots[team].Count);
                    foreach (RobotInfo robot in robots[team])
                    {
                        retRobots.Add(new RobotInfo(robot));
                    }
                }
                // It's ok to return an empty list if we see no robots
                return retRobots;
            }
        }

        // Each camera believes in a state
        private FieldState[] fieldStates = new FieldState[NUM_CAMERAS];

        // For marking ball position
        private bool marking = false;          
        private Vector2 markedPosition = null;

        // "Constants"
        private static double DELTA_DIST_SQ_MERGE;
        private static double MAX_SECONDS_TO_KEEP_INFO;
        private static double VELOCITY_DT;
        private static double BALL_MOVED_DIST;        

        public AveragingPredictor()
        {
            LoadConstants();
        }

        public void LoadConstants() {
            MAX_SECONDS_TO_KEEP_INFO = Constants.get<double>("default", "MAX_SECONDS_TO_KEEP_INFO");
            VELOCITY_DT = Constants.get<double>("default", "VELOCITY_DT");            
            BALL_MOVED_DIST = Constants.get<double>("plays", "BALL_MOVED_DIST");
            DELTA_DIST_SQ_MERGE = Constants.get<double>("default", "DELTA_DIST_SQ_MERGE");
            foreach (FieldState fieldState in fieldStates)
            {
                fieldState.LoadConstants();
            }
        }
        

        public void Update(VisionMessage msg)
        {
            fieldStates[msg.CameraID].Update(msg);            
        }

        // Return the average of info from all cameras weighed by the time since 
        // it was last updated
        public BallInfo GetBall()
        {
            double time = HighResTimer.SecondsSinceStart();

            // Return the average from all the cameras that see it weighted 
            // by the time since they last saw it            
            BallInfo ball0 = fieldStates[0].GetBall();
            Vector2 avgPosition = new Vector2(ball0.Position);
            Vector2 avgVelocity = new Vector2(ball0.Velocity);
            double avgLastSeen = ball0.LastSeen;
            double sum = time - ball0.LastSeen;
            for (int i = 1; i < fieldStates.Length; i++)
            {
                BallInfo ball = fieldStates[i].GetBall();                                
                if (ball != null)
                {
                    double t = time - ball.LastSeen;
                    avgPosition += t * ball.Position;
                    avgVelocity += t * ball.Velocity;
                    avgLastSeen += t * ball.LastSeen;
                    sum += t;                    
                }
            }
            avgPosition /= sum;
            avgVelocity /= sum;
            avgLastSeen /= sum; 

            return new BallInfo(avgPosition, avgVelocity, avgLastSeen);
        }

        // For each robot return the average of info from all cameras weighed by
        // the time since it was last updated
        public List<RobotInfo> GetRobots(int team)
        {          
            // Will store resulting list of robots
            List<RobotInfo> avgRobots = new List<RobotInfo>();

            // Infos from cameras    
            List<RobotInfo>[] fieldStateLists = new List<RobotInfo>[NUM_CAMERAS];

            // The outer list is has one entry per physical robot, each entry is a list made up of 
            // infos for that robot believed by different cameras. We later average over the inner list.
            List<List<RobotInfo>> robotSightings = new List<List<RobotInfo>>();            

            // Technically, need to record acquisition time for each camera, but it's ok
            double time = HighResTimer.SecondsSinceStart();
            
            // For assigning IDs to unidentified robots
            int nextID = -1;

            // Acquire the info from all cameras (as concurrently as possible)
            for (int cameraID = 0; cameraID < NUM_CAMERAS; cameraID++) 
            {                   
                fieldStateLists[cameraID] = fieldStates[cameraID].GetRobots(team);
            }

            // Construct the robot sightings list for later averaging
            for (int cameraID = 0; cameraID < NUM_CAMERAS; cameraID++) 
            {                              
                // Iterate over robots seen by the camera
                foreach (RobotInfo fsRobot in fieldStateLists[cameraID])
                {                   
                    // Match with unique info either by ID or by position
                    Predicate<RobotInfo> matchPredicate;
                    if (fsRobot.ID < 0)
                    {
                        matchPredicate = new Predicate<RobotInfo>(delegate(RobotInfo robot)
                        {
                            return robot.ID == fsRobot.ID;
                        });
                    }
                    else
                    {
                        matchPredicate = new Predicate<RobotInfo>(delegate(RobotInfo robot)
                        {
                            return robot.Position.distanceSq(fsRobot.Position) < DELTA_DIST_SQ_MERGE;
                        });
                    }

                    // Find the matching robot: m*n search
                    int sightingsIdx;
                    for (sightingsIdx = 0; sightingsIdx < robotSightings.Count; sightingsIdx++)
                    {
                        int sIdx = robotSightings[sightingsIdx].FindIndex(matchPredicate);
                        // If at least one sighting was satisfactory, we merge with that list of sightings
                        if (sIdx >= 0)
                        {                            
                            break;
                        }
                    }

                    // If not yet seen anywhere else, add the robot; otherwise, add it to the 
                    // sightings for later averaging
                    if (sightingsIdx == robotSightings.Count) {
                        List<RobotInfo> sList = new List<RobotInfo>();
                        sList.Add(fsRobot);                        
                        robotSightings.Add(sList);
                    } else {
                        robotSightings[sightingsIdx].Add(fsRobot);
                    }

                    // Keep track of maximum ID, so that we can assign IDs later
                    if (fsRobot.ID > nextID)
                    {
                        nextID = fsRobot.ID;
                    }
                }
            }

            // Now we have assembled information for each robot, just take the average
            foreach (List<RobotInfo> sList in robotSightings)
            {
                RobotInfo avgRobot= new RobotInfo(sList[0]);

                // Assign an ID if the robot came without one
                if (avgRobot.ID < 0)
                {

                }

                Vector2 avgPosition = new Vector2(avgRobot.Position);
                Vector2 avgVelocity = new Vector2(avgRobot.Velocity);
                double avgOrientation = avgRobot.Orientation;
                double avgAngVel = avgRobot.AngularVelocity;
                double avgLastSeen = avgRobot.LastSeen;
                double sum = time - avgRobot.LastSeen;
                for (int i = 1; i < sList.Count; i++)
                {
                    double t = time - sList[i].LastSeen;
                    avgPosition += t * sList[i].Position;
                    avgVelocity += t * sList[i].Velocity;
                    avgAngVel += t * sList[i].AngularVelocity;
                    avgOrientation += t * sList[i].Orientation;                
                    avgLastSeen += t * sList[i].LastSeen;
                    sum += t;
                }
                avgPosition /= sum;
                avgVelocity /= sum;
                avgAngVel /= sum;
                avgOrientation /= sum;
                avgLastSeen /= sum;

                // Record result for returning
                avgRobots.Add(new RobotInfo(avgPosition, avgVelocity, avgAngVel, avgOrientation, 
                    team, avgRobot.ID, avgLastSeen));
            }

            return avgRobots;
        }

        public RobotInfo GetRobot(int team, int id)
        {
            List<RobotInfo> robots = GetRobots(team);            
            RobotInfo robot = robots.Find(new Predicate<RobotInfo>(delegate(RobotInfo r)
            {
                return r.ID == id;
            }));
            if (robot == null)
            {
                throw new ApplicationException("AveragingPredictor.GetRobot: no robot with id=" + 
                    id.ToString() + " found on team " + team.ToString());
            }
            return robot;
        }

        public void SetBallMark() {     
            BallInfo ball = GetBall();
            if (ball == null) {
                throw new ApplicationException("Cannot mark ball position because no ball is seen.");
            }
            markedPosition = new Vector2(ball.Position);
            marking = true;
        }

        public void ClearBallMark() {
            marking = false;
            markedPosition = null;
        }

        public bool HasBallMoved() {
            if (!marking) return false;
            BallInfo ball = GetBall();           
            bool ret = markedPosition.distanceSq(ball.Position) > BALL_MOVED_DIST * BALL_MOVED_DIST;
            return ret;
        }

        public void SetPlayType(PlayTypes newPlayType)
        {
            throw new ApplicationException("StaticPredictor.setPlayType: not implemented");
        }

        // TO BE REMOVED
        public BallInfo getBallInfo()
        {
            throw new NotImplementedException("unimplemented");
        }
        public List<RobotInfo> getOurTeamInfo()
        {
            throw new NotImplementedException("unimplemented");
        }
        public List<RobotInfo> getTheirTeamInfo()
        {
            throw new NotImplementedException("unimplemented");
        }
        public RobotInfo getCurrentInformation(int id)
        {
            throw new NotImplementedException("unimplemented");
        }
        public List<RobotInfo> getAllInfos()
        {
            throw new NotImplementedException("unimplemented");
        }        

    }

}
