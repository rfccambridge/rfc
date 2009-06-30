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

    	private int team;

        // The state of the field believed in by a camera
        private class FieldState
        {
            // The believed state is kept here
            private BallInfo ball = null;
            private List<RobotInfo>[] robots = new List<RobotInfo>[] { new List<RobotInfo>(), 
                                                                       new List<RobotInfo>() };
 
            // For synching the above
            private object ballLock = new object();
            private object robotsLock = new object();

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
            static double WEIGHT_OLD, WEIGHT_NEW;

            public void LoadConstants()
            {
                VELOCITY_DT = Constants.get<double>("default", "VELOCITY_DT");
                WEIGHT_OLD = Constants.get<double>("default", "WEIGHT_OLD");
                WEIGHT_NEW = Constants.get<double>("default", "WEIGHT_NEW");
            }

            // Update the believed state with new observations
            public void Update(VisionMessage msg)
            {
                double time = HighResTimer.SecondsSinceStart();

                #region Update ball
                if (msg.Ball != null)
                {
                    lock (ballLock) {
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
                            ball.Position = new Vector2(WEIGHT_OLD * ball.Position + WEIGHT_NEW * newBall.Position);

                            // Update velocity if a reasonable interval has passed
                            double dt = time - ballDtStart;
                            if (dt > VELOCITY_DT)
                            {
                                Vector2 d = msg.Ball.Position - ballAtDtStart.Position;
                                ball.Velocity = WEIGHT_OLD * ball.Velocity + WEIGHT_NEW * d / dt;

                                // Reset velocity interval
                                ballDtStart = time;
                                ballAtDtStart = new BallInfo(ball);
                            }
                        }

                        // We have just seen the ball                    
                        ball.LastSeen = time;
                    }
                }
                #endregion

                #region Update robots   
                lock (robotsLock) {
                    foreach (Robocup.Core.VisionMessage.RobotData newRobotData in msg.Robots)
                    {
                        int team = newRobotData.Team == VisionMessage.Team.YELLOW ? 0 : 1;
                        RobotInfo newRobot = new RobotInfo(newRobotData.Position, new Vector2(0, 0), 0,
                            newRobotData.Orientation, team, newRobotData.ID);

                        // Keep track of nextID
                        if (newRobot.ID > nextID[newRobot.Team])
                        {
                            nextID[newRobot.Team] = newRobot.ID;
                        }

                        // Match with existing info either by ID (if vision gave it one) or by position
                        Predicate<RobotInfo> matchByPosPredicate;
                        matchByPosPredicate = new Predicate<RobotInfo>(delegate(RobotInfo robot)
                        {
                            return robot.Position.distanceSq(newRobot.Position) < DELTA_DIST_SQ_MERGE;
                        });
                        
                        // Find the matching robot
                        int oldRobotIdx = -1;
                        oldRobotIdx = robots[newRobot.Team].FindIndex(matchByPosPredicate);
                        if (oldRobotIdx >= 0 && newRobot.ID >= 0 && newRobot.ID != robots[newRobot.Team][oldRobotIdx].ID)
                        {
                                continue;
                        }
                        
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
                            oldRobot.Position = new Vector2(WEIGHT_OLD * oldRobot.Position + WEIGHT_NEW * newRobot.Position);
                            oldRobot.Orientation = WEIGHT_OLD * oldRobot.Orientation + WEIGHT_NEW * newRobot.Orientation;

                            // Update velocity if a reasonable interval has passed                    
                            double dt = time - velocityDtStart[newRobot.Team][oldRobotIdx];

                            if (dt > VELOCITY_DT)
                            {
                                Vector2 d = newRobot.Position - robotsAtDtStart[newRobot.Team][oldRobotIdx].Position;
                                oldRobot.Velocity = WEIGHT_OLD * oldRobot.Velocity + WEIGHT_NEW * d / dt;

                                // Reset velocity dt interval
                                velocityDtStart[newRobot.Team][oldRobotIdx] = time;
                                robotsAtDtStart[newRobot.Team][oldRobotIdx] = new RobotInfo(oldRobot);
                            }
                            newRobotIdx = oldRobotIdx;
                        }

                        // We have just seen this robot
                        robots[newRobot.Team][newRobotIdx].LastSeen = time;                        
                    }
                }
                #endregion
            }

            public BallInfo GetBall()
            {
                BallInfo retBall;
                lock (ballLock) {
                    double time = HighResTimer.SecondsSinceStart();

                    // Reconsider our belief
                    if (ball != null && time - ball.LastSeen > MAX_SECONDS_TO_KEEP_INFO)
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
                lock (robotsLock) {
                    double time = HighResTimer.SecondsSinceStart();

                    // Reconsider our belief
                    List<RobotInfo> tempRobots = new List<RobotInfo>(robots[team].Count);
                    for (int i = 0; i < robots[team].Count; i++)
                    {
                        if (time - robots[team][i].LastSeen < MAX_SECONDS_TO_KEEP_INFO)
                        {
                            tempRobots.Add(robots[team][i]);
                        }                 
                    }
                    robots[team].Clear();
                    robots[team].AddRange(tempRobots);

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
            for (int i = 0; i < NUM_CAMERAS; i++)
            {
                fieldStates[i] = new FieldState();
            }

        	team = Constants.get<int>("configuration", "OUR_TEAM_INT");

            LoadConstants();
        }

        public void LoadConstants()
        {
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
   
            Vector2 avgPosition = null;
            Vector2 avgVelocity = null;
            double avgLastSeen = 0;
            double sum = 0;
            for (int i = 0; i < fieldStates.Length; i++)
            {
                BallInfo ball = fieldStates[i].GetBall();                                
                if (ball != null)
                {
                    //double t = time - ball.LastSeen;
                    double t = 1;
                    // First time, we don't add, just initialize
                    if (avgPosition == null)
                    {
                        avgPosition = t * ball.Position;
                        avgVelocity = t * ball.Velocity;
                        avgLastSeen = t * ball.LastSeen;
                        sum = t;                    
                    }
                    avgPosition += t * ball.Position;
                    avgVelocity += t * ball.Velocity;
                    avgLastSeen += t * ball.LastSeen;
                    sum += t;                    
                }
            }

            BallInfo retBall = null;
            if (avgPosition != null) // if we saw at least one ball
            {
                avgPosition /= sum;
                avgVelocity /= sum;
                avgLastSeen /= sum;
                retBall = new BallInfo(avgPosition, avgVelocity, avgLastSeen);
            }
            return retBall;
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
            // TODO: in the future, the parent predictor could keep it's own state so that paternless
            // ids stay stay steady
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

                    // Match with existing info either by ID (if vision gave it one) or by position
                    Predicate<RobotInfo> matchByIDPredicate, matchByPosPredicate;
                    matchByIDPredicate = new Predicate<RobotInfo>(delegate(RobotInfo robot)
                    {
                        return robot.ID == fsRobot.ID;
                    });
                    matchByPosPredicate = new Predicate<RobotInfo>(delegate(RobotInfo robot)
                    {
                        return robot.Position.distanceSq(fsRobot.Position) < DELTA_DIST_SQ_MERGE;
                    });

                    // Find the matching robot: m*n search
                    int sightingsIdx;
                    bool doNotAdd = false;
                    for (sightingsIdx = 0; sightingsIdx < robotSightings.Count; sightingsIdx++)
                    {
                        int sIdx = -1;
                        sIdx = robotSightings[sightingsIdx].FindIndex(matchByPosPredicate);
                        
                        // If position matches, but ID doesn't, the new robot is on top of one we already saw
                        // In this case, we ignore the new one completely (arbitrary choice -- old is not really 
                        // better than old)
                        if (sIdx >= 0 && fsRobot.ID >= 0 && fsRobot.ID != robotSightings[sightingsIdx][sIdx].ID)
                        {
                            doNotAdd = true;
                            continue;
                        }
             
                        // If at least one sighting was satisfactory, we merge with that list of sightings
                        if (sIdx >= 0)
                        {                            
                            break;
                        }
                    }

                    // Decided to ignore this robot because it was on top of another one
                    if (doNotAdd)
                    {
                        continue;
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
                RobotInfo avgRobot = new RobotInfo(sList[0]);

                // Assign an ID if the robot came without one
                if (avgRobot.ID < 0)
                {
                    avgRobot.ID = ++nextID;
                }

                double t = time - avgRobot.LastSeen;
                t = 1;
                Vector2 avgPosition = t * (new Vector2(avgRobot.Position));
                Vector2 avgVelocity = t * (new Vector2(avgRobot.Velocity));                
                double avgOrientation = t * avgRobot.Orientation;
                double avgAngVel = t * avgRobot.AngularVelocity;
                double avgLastSeen = t * avgRobot.LastSeen;
                double sum = t;                
                for (int i = 1; i < sList.Count; i++)
                {
                    //t = time - sList[i].LastSeen;
                    t = 1;
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
        public List<RobotInfo> GetRobots() {
            List<RobotInfo> combined = new List<RobotInfo>();
            combined.AddRange(GetRobots(0));
            combined.AddRange(GetRobots(1));
            return combined;
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
                //throw new ApplicationException("Cannot mark ball position because no ball is seen.");
                return;
            }
            markedPosition = ball != null ? new Vector2(ball.Position) : null;
            marking = true;
        }

        public void ClearBallMark() {
            marking = false;
            markedPosition = null;
        }

        public bool HasBallMoved() {
            if (!marking) return false;
            BallInfo ball = GetBall();
            bool ret = (ball != null && markedPosition == null) || (ball != null && 
                        markedPosition.distanceSq(ball.Position) > BALL_MOVED_DIST * BALL_MOVED_DIST);
            return ret;
        }

        public void SetPlayType(PlayTypes newPlayType)
        {
			// Do nothing: this method is for assumed ball: returning clever values for the ball
			// based on game state -- i.e. center of field during kick-off
        }

        // TO BE REMOVED
        public BallInfo getBallInfo()
        {
            return GetBall() ?? new BallInfo(new Vector2(0,0));
            //throw new NotImplementedException("unimplemented");
        }
        public List<RobotInfo> getOurTeamInfo()
        {
            //return GetRobots(0);
            throw new NotImplementedException("unimplemented");
        }
        public List<RobotInfo> getTheirTeamInfo()
        {
            //return GetRobots(1);
            throw new NotImplementedException("unimplemented");
        }
        public RobotInfo getCurrentInformation(int id)
        {
            //return GetRobot(0,id);
        	return GetRobot(team, id);
        }
        public List<RobotInfo> getAllInfos()
        {
            //List<RobotInfo> combined = new List<RobotInfo>();
            //combined.AddRange(GetRobots(0));
            //combined.AddRange(GetRobots(1));
            //return combined;
            throw new NotImplementedException("unimplemented");
        }        

    }

}
