using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using MovementModeler = Robocup.CoreRobotics.MovementModeler;

namespace Robocup.Simulation
{
    public class PhysicsEngine : IPredictor, IRobots
    {
        List<RobotInfo> ourinfo;
        List<RobotInfo> theirinfo;
        BallInfo ball_info;
        VirtualRef referee;
        Dictionary<int, MovementModeler> movement_modelers = new Dictionary<int, MovementModeler>();

        public void MoveRobot(int robotID, RobotInfo new_info)
        {
            foreach (RobotInfo info in getOurTeamInfo())
            {
                if (info.ID == robotID)
                {
                    UpdateRobot(info, new_info);
                }
            }
            foreach (RobotInfo info in getTheirTeamInfo())
            {
                if (info.ID == robotID)
                {
                    UpdateRobot(info, new_info);
                }
            }
        }
        public void MoveBall(Vector2 newPosition)
        {
            UpdateBall(new BallInfo(newPosition));
        }

        public PhysicsEngine(VirtualRef referee)
        {
            resetPositions();
            this.referee = referee;
            foreach (RobotInfo info in ourinfo)
                movement_modelers.Add(info.ID, new MovementModeler());
            foreach (RobotInfo info in theirinfo)
                movement_modelers.Add(info.ID, new MovementModeler());
        }

        public void resetPositions()
        {
            ourinfo = new List<RobotInfo>(new RobotInfo[]{
                new RobotInfo(new Vector2(-1.0, -1), 3, 0),
                new RobotInfo(new Vector2(-1.0, 0), 3, 1),
                new RobotInfo(new Vector2(-1.0, 1), 3, 2),
                new RobotInfo(new Vector2(-2f, -1), 3, 3),
                new RobotInfo(new Vector2(-2f, 1), 3, 4),
            });

            theirinfo = new List<RobotInfo>(new RobotInfo[]{
                new RobotInfo(new Vector2(1.0, -1), 3, 5),
                new RobotInfo(new Vector2(1.0, 0), 3, 6),
                new RobotInfo(new Vector2(1.0, 1), 3, 7),
                new RobotInfo(new Vector2(2f, -1), 3, 8),
                new RobotInfo(new Vector2(2f, 1), 3, 9)
            });
            ball_info = new BallInfo(Vector2.ZERO);
        }

        #region IPredictor Members

        public RobotInfo getCurrentInformation(int robotID)
        {
            foreach (RobotInfo info in getOurTeamInfo())
            {
                if (info.ID == robotID)
                {
                    return info;
                }
            }
            foreach (RobotInfo info in getTheirTeamInfo())
            {
                if (info.ID == robotID)
                {
                    return info;
                }
            }
            return null;
        }

        public List<RobotInfo> getOurTeamInfo()
        {
            return new List<RobotInfo>(ourinfo);
        }

        public List<RobotInfo> getTheirTeamInfo()
        {
            return new List<RobotInfo>(theirinfo);
        }

        //not an IPredictor method, but useful
        public List<RobotInfo> getAllInfos()
        {
            List<RobotInfo> infos = new List<RobotInfo>(ourinfo);
            infos.AddRange(theirinfo);
            return infos;
        }

        public BallInfo getBallInfo()
        {
            return ball_info;
        }

        public void setBallMark()
        {
            throw new ApplicationException("PhysicsEngine.setBallMark: not implemented");
        }
        public void clearBallMark()
        {
            throw new ApplicationException("PhysicsEngine.clearBallMark: not implemented");
        }
        public bool hasBallMoved()
        {
            throw new ApplicationException("PhysicsEngine.hasBallMoved: not implemented");
        }
        public void setPlayType(PlayTypes newPlayType)
        {
            throw new ApplicationException("PhysicsEngine.setPlayType: not implemented");
        }
        #endregion

        /// <summary>
        /// Steps forward the given number of seconds
        /// </summary>
        public void step(double dt)
        {
            foreach (RobotInfo info in getOurTeamInfo())
            {
                int id = info.ID;
                if (!speeds.ContainsKey(id))
                    speeds[id] = new WheelSpeeds();
                UpdateRobot(info, movement_modelers[id].ModelWheelSpeeds(info, speeds[id], dt));
            }
            foreach (RobotInfo info in getTheirTeamInfo())
            {
                int id = info.ID;
                if (!speeds.ContainsKey(id))
                    speeds[id] = new WheelSpeeds();
                UpdateRobot(info, movement_modelers[id].ModelWheelSpeeds(info, speeds[id], dt));
            }

            //the speed at which the ball will bounce when it hits another robot
            double ballbounce = .005;
            const double collisionradius = .10;
            //the fraction of the ball velocity that it loses every second
            //well, roughly, because compounding counts, so it's off by about a factor of 2.5
            const double balldecay = .8;

            BallInfo ball = getBallInfo();
            // run one step of tester

            // update ball location
            Vector2 newballlocation = ball.Position + dt * ball.Velocity;
            Vector2 newballvelocity = (1 - dt * balldecay) *  ball.Velocity;

            // check for collisions ball-robot, update ball position
            bool collided = false;
            foreach (RobotInfo r in getTheirTeamInfo())
            {
                Vector2 location = r.Position;
                if (newballlocation.distanceSq(location) <= collisionradius * collisionradius)
                {
                    collided = true;
                    ballbounce = Math.Sqrt((ball.Velocity - r.Velocity).magnitudeSq())*.02;
                    newballvelocity = ballbounce * ((ball.Position - location).normalize());
                    newballlocation = location + (collisionradius + .005) * (ball.Position - location).normalize();
                    break;
                }
            }

            // check for collisions ball-robot, in state.getOurTeamInfo(), update ball position
            if (!collided)
            {
                foreach (RobotInfo r in getOurTeamInfo())
                {
                    Vector2 location = r.Position;
                    if (newballlocation.distanceSq(location) <= collisionradius * collisionradius)
                    {
                        collided = true;
                        ballbounce = Math.Sqrt((ball.Velocity - r.Velocity).magnitudeSq()) * .02;
                        newballvelocity = ballbounce * ((ball.Position - location).normalize());
                        newballlocation = location + .13 * (ball.Position - location).normalize();
                        break;
                    }
                }
            }

            // do wall boundaries
            if (!collided)
            {
                Vector2 ballPos = ball.Position;
                double ballVx = newballvelocity.X;
                double ballVy = newballvelocity.Y;
                if (ballPos.X < -2.45)
                    ballVx = Math.Abs(ballVx);
                else if (ballPos.X > 2.45)
                    ballVx = -Math.Abs(ballVx);
                if (ballPos.Y < -1.7)
                    ballVy = Math.Abs(ballVy);
                else if (ballPos.Y > 1.7)
                    ballVy = -Math.Abs(ballVy);
                newballvelocity = new Vector2(ballVx, ballVy);
                newballlocation = new Vector2(ball.Position.X + ballVx, ball.Position.Y + ballVy);
            }

            List<RobotInfo> allinfos = new List<RobotInfo>(10);
            allinfos.AddRange(getOurTeamInfo());
            allinfos.AddRange(getTheirTeamInfo());

            // fix robot-robot collisions
            for (int i = 0; i < allinfos.Count; i++)
            {
                for (int j = 0; j < allinfos.Count; j++)
                {
                    if (i == j)
                        continue;
                    Vector2 p1 = allinfos[i].Position;
                    Vector2 p2 = allinfos[j].Position;
                    if (p1.distanceSq(p2) <= .2 * .2)
                    {
                        Vector2 t1 = p1 + .01 * (p1 - p2).normalize();
                        Vector2 t2 = p2 + .01 * (p2 - p1).normalize();
                        UpdateRobot(allinfos[i], new RobotInfo(t1, allinfos[i].Orientation, allinfos[i].ID));
                        UpdateRobot(allinfos[j], new RobotInfo(t2, allinfos[j].Orientation, allinfos[j].ID));
                    }
                }
            }

            // friction
            UpdateBall(new BallInfo(newballlocation, newballvelocity));
            referee.RunRef(this, UpdateBall);
        }


        private void UpdateRobot(RobotInfo old_info, RobotInfo new_info)
        {

            int our_index = ourinfo.IndexOf(old_info);
            if (our_index >= 0)
            {
                ourinfo[our_index] = new_info;
                //ourinfo.Remove(old_info);
                //ourinfo.Add(new_info);
            }
            int their_index = theirinfo.IndexOf(old_info);
            if (their_index >= 0)
            {
                theirinfo[their_index] = new_info;
                //theirinfo.Remove(old_info);
                //theirinfo.Add(new_info);
            }
#if DEBUG
            if (old_info.ID != new_info.ID)
                throw new ApplicationException("old robot id and new robot ids dont match!");
            foreach (RobotInfo info in getOurTeamInfo())
            {
                if (info.ID == old_info.ID)
                    return;
            }
            foreach (RobotInfo info in getTheirTeamInfo())
            {
                if (info.ID == old_info.ID)
                    return;
            }
            throw new ApplicationException("no robot found with id " + old_info.ID);
#endif
        }
        private void UpdateBall(BallInfo new_info)
        {
            this.ball_info = new_info;
        }

        #region IRobot members
        const double initial_ball_speed = 0.1f;
        readonly Random r = new Random();
        public void kick(int robotID)
        {
            RobotInfo robot = getCurrentInformation(robotID);
            // add randomness to actual robot location / direction
            //const double randomComponent = initial_ball_speed / 3;            

            double ballVx = (double)(initial_ball_speed) * Math.Cos(robot.Orientation);
            double ballVy = (double)(initial_ball_speed) * Math.Sin(robot.Orientation);

            Console.WriteLine("ORIENTATION: " + robot.Orientation + " X: " + ballVx + " Y: " + ballVy);
            //ballVx += (double)(r.NextDouble() * 2 - 1) * randomComponent;
            //ballVy += (double)(r.NextDouble() * 2 - 1) * randomComponent;
            //RobotInfo prev = robot;
            //const double recoil = .02 / initial_ball_speed; ;
            UpdateBall(new BallInfo(ball_info.Position, new Vector2(ballVx, ballVy)));
            //UpdateRobot(robot, new RobotInfo(prev.Position + (new Vector2(-ballVx * recoil, -ballVy * recoil)), prev.Orientation, prev.ID));
        }
        public void beamKick(int robotID) 
        {
            throw new NotImplementedException("Should be implemented in the simulator as well!!!");
        }
        Dictionary<int, WheelSpeeds> speeds = new Dictionary<int, WheelSpeeds>();
        public void setMotorSpeeds(int robotID, WheelSpeeds speeds)
        {
            this.speeds[robotID] = speeds;
        }
        #endregion
    }
}