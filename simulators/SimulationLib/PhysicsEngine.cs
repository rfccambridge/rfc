using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Robocup.Core;
using MovementModeler = Robocup.CoreRobotics.MovementModeler;
using Robocup.CoreRobotics;
using Robocup.Utilities;
using Robocup.MessageSystem;

namespace Robocup.Simulation
{
    public class PhysicsEngine : IPredictor
    {
        const double INITIAL_BALL_SPEED = 0.6f;

		private bool _marking = false;
		private Vector2 _markedPosition;

		private double BALL_MOVED_DIST;
        static double FIELD_WIDTH;
        static double FIELD_HEIGHT;
        static double FIELD_XMIN;
        static double FIELD_XMAX;
        static double FIELD_YMIN;
        static double FIELD_YMAX;
        static double GOAL_WIDTH;

        private bool running = false;
        private System.Timers.Timer mainTimer = new System.Timers.Timer();
        private int mainTimerSync = 0;
        private int counter = 0;

		private int numYellow;
		private int numBlue;

        public int NumYellow { get { return numYellow; } }
        public int NumBlue { get { return numBlue; } }

        private IMessageReceiver<RobotCommand> cmdReceiver;

        private bool visionStarted = false;
        SSLVision.RoboCupSSLServerManaged sslVisionServer;

        private bool refBoxStarted = false;
        SimpleReferee referee;

        private SimulatedScenario scenario;
        private Object updateScenarioLock = new Object();

		private BallInfo ball = null;
        private Dictionary<Team, List<RobotInfo>> robots = new Dictionary<Team, List<RobotInfo>>();

        private Dictionary<Team, Dictionary<int, MovementModeler>> movement_modelers = new Dictionary<Team, Dictionary<int, MovementModeler>>();
        private Dictionary<Team, Dictionary<int, WheelSpeeds>> speeds = new Dictionary<Team, Dictionary<int, WheelSpeeds>>();
        private Dictionary<Team, Dictionary<int, bool>> break_beams = new Dictionary<Team, Dictionary<int, bool>>();

		public PhysicsEngine()
		{
            foreach (Team team in Enum.GetValues(typeof(Team)))
            {
                robots[team] = new List<RobotInfo>();
                break_beams[team] = new Dictionary<int, bool>();
                movement_modelers[team] = new Dictionary<int, MovementModeler>();
                speeds[team] = new Dictionary<int, WheelSpeeds>();
            }
			
            LoadConstants();

            referee = new SimpleReferee();

            mainTimer.AutoReset = true;
            mainTimer.Elapsed += mainTimer_Elapsed;
		}

        private void InitState()
		{
			foreach (Team team in Enum.GetValues(typeof(Team)))
			{
				break_beams[team].Clear();
				movement_modelers[team].Clear();
				speeds[team].Clear();
			}

            GetScenarioScene();

			foreach (Team team in Enum.GetValues(typeof(Team)))
			{
				foreach (RobotInfo info in robots[team])
				{
					break_beams[team].Add(info.ID, false);
					movement_modelers[team].Add(info.ID, new MovementModeler());
					speeds[team].Add(info.ID, new WheelSpeeds());
				}
			}

            referee.GoalScored += scenario.GoalScored;
		}

        public void LoadConstants()
        {
            BALL_MOVED_DIST = Constants.get<double>("plays", "BALL_MOVED_DIST");

            FIELD_WIDTH = Constants.get<double>("plays", "FIELD_WIDTH");
            FIELD_HEIGHT = Constants.get<double>("plays", "FIELD_HEIGHT");

            GOAL_WIDTH = Constants.get<double>("plays", "GOAL_WIDTH");

            //Leave a goal depth around the field, so that stuff(ball) is allowed inside the goal
            FIELD_XMIN = -FIELD_WIDTH / 2 - GOAL_WIDTH;
            FIELD_XMAX = FIELD_WIDTH / 2 + GOAL_WIDTH;
            FIELD_YMIN = -FIELD_HEIGHT / 2;
            FIELD_YMAX = FIELD_HEIGHT / 2;            
        }

        public void StartCommander(int port)
        {
            if (cmdReceiver != null)
                throw new ApplicationException("Already listening.");
            cmdReceiver = Messages.CreateServerReceiver<RobotCommand>(port);
            if (cmdReceiver == null)
                throw new ApplicationException("Could not listen on port " + port.ToString());
            cmdReceiver.MessageReceived += cmdReceiver_MessageReceived;
        }

        public void StopCommander()
        {
            if (cmdReceiver == null)
                throw new ApplicationException("Not listening.");
            cmdReceiver.Close();
            cmdReceiver.MessageReceived -= cmdReceiver_MessageReceived;
            cmdReceiver = null;
        }

        public void StartVision(string host, int port)
        {
            if (visionStarted)
                throw new ApplicationException("Vision already running.");

            sslVisionServer= new SSLVision.RoboCupSSLServerManaged(port, host, "");
            sslVisionServer.open();

            visionStarted = true;
        }

        public void StopVision()
        {
            if (!visionStarted)
                throw new ApplicationException("Vision not running.");

            sslVisionServer.close();

            visionStarted = false;
        }

        public void StartReferee(string host, int port)
        {
            if (refBoxStarted)
                throw new ApplicationException("Referee already running.");

            referee.Connect(host, port);

            refBoxStarted = true;
        }

        public void StopReferee()
        {
            if (!refBoxStarted)
                throw new ApplicationException("Referee not running.");

            referee.Disconnect();

            refBoxStarted = false;
        }

        public void SetScenario(SimulatedScenario scenario)
        {
            if (running)
                throw new ApplicationException("Cannot change scenario while running");

            if (this.scenario != null)
                referee.GoalScored -= scenario.GoalScored;
            
            this.scenario = scenario;
        }

        public void GetScenarioScene()
        {
            SimulatorScene scene = scenario.GetScene();

            lock (updateScenarioLock)
            {
                ball = scene.Ball;
                foreach (Team team in Enum.GetValues(typeof(Team)))
                {
                    robots[team].Clear();
                    robots[team].AddRange(scene.Robots[team]);
                }
            }
        }

        public void Start(int numYellow, int numBlue)
        {
            if (running)
                throw new ApplicationException("Already running.");

			this.numYellow = numYellow;
			this.numBlue = numBlue;

			InitState();

            double freq = Constants.get<double>("default", "SIM_ENGINE_FREQUENCY");
            double period = 1.0 / freq * 1000; // in ms

            mainTimer.Interval = period;
            mainTimer.Start();

            counter = 0;
            running = true;
        }

        public void Stop()
        {
            if (!running)
                throw new ApplicationException("Not running.");
            mainTimer.Stop();
            running = false;
        }
      
        /// <summary>
        /// Steps forward the given number of seconds
        /// </summary>
        public void Step(double dt)
        {            
            foreach (Team team in Enum.GetValues(typeof(Team)))
                foreach (RobotInfo info in robots[team])
                    UpdateRobot(info, movement_modelers[team][info.ID].ModelWheelSpeeds(info, speeds[team][info.ID], dt));

            //the speed at which the ball will bounce when it hits another robot
            double ballbounce = .005;
            const double collisionradius = .10;
            //the fraction of the ball velocity that it loses every second
            //well, roughly, because compounding counts, so it's off by about a factor of 2.5
            const double balldecay = 6;

            // run one step of tester

            // update ball location
            Vector2 newballlocation = ball.Position + dt * ball.Velocity;
            Vector2 newballvelocity = (1 - dt * balldecay) * ball.Velocity;

            // check for collisions ball-robot, update ball position

            List<RobotInfo> allRobots = new List<RobotInfo>();
            foreach (Team team in Enum.GetValues(typeof(Team)))
                allRobots.AddRange(robots[team]);

            bool collided = false;
            foreach (RobotInfo r in allRobots)
            {
                Vector2 location = r.Position;
                if (newballlocation.distanceSq(location) <= collisionradius * collisionradius)
                {
                    collided = true;
                    ballbounce = Math.Sqrt((ball.Velocity - r.Velocity).magnitudeSq()) * .02;
                    newballvelocity = ballbounce * ((ball.Position - location).normalize());
                    newballlocation = location + (collisionradius + .005) * (ball.Position - location).normalize();
                    break;
                }
            }

            // do wall boundaries
            if (!collided)
            {
                Vector2 ballPos = ball.Position;
                double ballVx = newballvelocity.X;
                double ballVy = newballvelocity.Y;
                if (ballPos.X < FIELD_XMIN)
                    ballVx = Math.Abs(ballVx);
                else if (ballPos.X > FIELD_XMAX)
                    ballVx = -Math.Abs(ballVx);
                if (ballPos.Y < FIELD_YMIN)
                    ballVy = Math.Abs(ballVy);
                else if (ballPos.Y > FIELD_YMAX)
                    ballVy = -Math.Abs(ballVy);
                newballvelocity = new Vector2(ballVx, ballVy);
                newballlocation = new Vector2(ball.Position.X + ballVx, ball.Position.Y + ballVy);
            }
            
            // fix robot-robot collisions
            for (int i = 0; i < allRobots.Count; i++)
            {
                for (int j = 0; j < allRobots.Count; j++)
                {
                    if (i == j)
                        continue;
                    Vector2 p1 = allRobots[i].Position;
                    Vector2 p2 = allRobots[j].Position;
                    if (p1.distanceSq(p2) <= .2 * .2)
                    {
                        Vector2 t1 = p1 + .01 * (p1 - p2).normalize();
                        Vector2 t2 = p2 + .01 * (p2 - p1).normalize();
                        UpdateRobot(allRobots[i], new RobotInfo(t1, allRobots[i].Orientation,
                            allRobots[i].Team, allRobots[i].ID));
                        UpdateRobot(allRobots[j], new RobotInfo(t2, allRobots[j].Orientation,
                            allRobots[j].Team, allRobots[j].ID));
                    }
                }
            }

            // friction
            UpdateBall(new BallInfo(newballlocation, newballvelocity));
            
            // Synchronously (at least for now) send out a vision message
            SSLVision.SSL_DetectionFrameManaged frame = constructSSLVisionFrame();
            sslVisionServer.send(frame);

            referee.RunRef(this);
            //TODO: A dirty hack to allow us to run without a real refbox. Implement based on referee.GetCurrentPlayType()!
            if (refBoxStarted)
                referee.SendCommand(RefBoxHandler.START);
        }        

        public void UpdateRobot(RobotInfo old_info, RobotInfo new_info)
        {
			if (old_info.Team != new_info.Team)
				throw new ApplicationException("old robot team and new robot team dont match!");
			if (old_info.ID != new_info.ID)
				throw new ApplicationException("old robot id and new robot ids dont match!");

            int index = robots[old_info.Team].IndexOf(old_info);
			if (index >= 0)
			{
				robots[old_info.Team][index].Position = new_info.Position;
				robots[old_info.Team][index].Velocity = new_info.Velocity;
				robots[old_info.Team][index].Orientation = new_info.Orientation;
			}
			else
			{
				throw new ApplicationException("no robot found with id " + old_info.ID + " on team " + old_info.Team);
			}
        }

        public void UpdateBall(BallInfo new_info)
        {
        	ball.Position = new_info.Position;
        	ball.Velocity = new_info.Velocity;
        	ball.LastSeen = new_info.LastSeen;
        }

        private SSLVision.SSL_DetectionFrameManaged constructSSLVisionFrame()
        {
                SSLVision.SSL_DetectionFrameManaged frame = new SSLVision.SSL_DetectionFrameManaged();

                Vector2 ballPos = convertToSSLCoords(ball.Position);
                frame.add_ball(1, (float)ballPos.X, (float)ballPos.Y);

                foreach (RobotInfo robot in robots[Team.Yellow])
                {
                    Vector2 pos = convertToSSLCoords(robot.Position);
                    frame.add_robot_yellow(1, robot.ID, (float)pos.X, (float)pos.Y,
                                         (float)robot.Orientation);
                }
                foreach (RobotInfo robot in robots[Team.Blue])
                {
                    Vector2 pos = convertToSSLCoords(robot.Position);
                    frame.add_robot_blue(1, robot.ID, (float)pos.X, (float)pos.Y,
                                         (float)robot.Orientation);
                }

                // dummy-fill required fields
                frame.set_frame_number(0);
                frame.set_camera_id(0);
                frame.set_t_capture(0);
                frame.set_t_sent(0);

                return frame;
        }

        private Vector2 convertToSSLCoords(Vector2 pt)
        {
            return new Vector2(pt.X * 1000, pt.Y * 1000); // m to mm
        }

        private void mainTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Skip event if the previous one is still being handled
            if (Interlocked.CompareExchange(ref mainTimerSync, 1, 0) == 0)
            {
                Step(mainTimer.Interval / 1000.0); // convert to sec
                counter++;
                mainTimerSync = 0;
            }
        }

        private void cmdReceiver_MessageReceived(RobotCommand command)
        {
            lock (updateScenarioLock)
            {
                // This is not the cleanest, but it's ok, because these IDs are also issued by this
                // engine -- so, the convention is contained. Adding a team into RobotCommand does 
                // *not* make much sense.
                Team team = command.ID < 5 ? Team.Yellow : Team.Blue;

                switch (command.command)
                {
                    case RobotCommand.Command.MOVE:
                        this.speeds[team][command.ID] = command.Speeds;
                        break;
                    case RobotCommand.Command.KICK:
                        {
                            // NOTE: In current setup, all robots are created, so this robot is guaranteed to exist                                                
                            RobotInfo robot = robots[team].Find(new Predicate<RobotInfo>(delegate(RobotInfo bot)
                                   {
                                       return bot.ID == command.ID;
                                   }));
                            // add randomness to actual robot location / direction
                            //const double randomComponent = initial_ball_speed / 3;            

                            double ballVx = (double)(INITIAL_BALL_SPEED) * Math.Cos(robot.Orientation);
                            double ballVy = (double)(INITIAL_BALL_SPEED) * Math.Sin(robot.Orientation);

                            Console.WriteLine("ORIENTATION: " + robot.Orientation + " X: " + ballVx + " Y: " + ballVy);
                            //ballVx += (double)(r.NextDouble() * 2 - 1) * randomComponent;
                            //ballVy += (double)(r.NextDouble() * 2 - 1) * randomComponent;
                            //RobotInfo prev = robot;
                            //const double recoil = .02 / initial_ball_speed; ;
                            UpdateBall(new BallInfo(ball.Position, new Vector2(ballVx, ballVy)));
                            //UpdateRobot(robot, new RobotInfo(prev.Position + (new Vector2(-ballVx * recoil, -ballVy * recoil)), prev.Orientation, prev.ID));
                            break;
                        }
                    case RobotCommand.Command.BREAKBEAM_KICK:
                        {
                            const double CENTER_TO_KICKER_DIST = 0.070; // m
                            const double KICKER_ACTIVITY_RADIUS_SQ = 0.04 * 0.04; // m

                            const int BREAKBEAM_CHECK_PERIOD = 100; // ms
                            const double BREAKBEAM_TIMEOUT = 10; // s

                            if (!break_beams[team].ContainsKey(command.ID))
                            {
                                Console.WriteLine("Could not find robot " + command.ID + " on team " + team.ToString());
                                return;
                            }

                            if (break_beams[team][command.ID]) break;

                            break_beams[team][command.ID] = true;

                            Thread breakBeamThread = new Thread(delegate(object state)
                            {
                                double elapsed;
                                HighResTimer timeoutTimer = new HighResTimer();
                                timeoutTimer.Start();
                                do
                                {
                                    // NOTE: In current setup, all robots are created, so this robot is guaranteed to exist                        
                                    RobotInfo robot = robots[team].Find(new Predicate<RobotInfo>(delegate(RobotInfo bot)
                                    {
                                        return bot.ID == command.ID;
                                    }));

                                    Vector2 orientation = new Vector2(Math.Cos(robot.Orientation), Math.Sin(robot.Orientation));
                                    Vector2 kickerPosition = robot.Position + CENTER_TO_KICKER_DIST * orientation;

                                    //Console.WriteLine("Robot pos: " + robot.Position +
                                    //                  " Kicker pos: " + kickerPosition +
                                    //                  " Ball pos: " + ballInfo.Position +
                                    //                  " Distsq: " + kickerPosition.distanceSq(ballInfo.Position));
                                    if (kickerPosition.distanceSq(ball.Position) < KICKER_ACTIVITY_RADIUS_SQ)
                                    {
                                        double ballVx = (double)(INITIAL_BALL_SPEED) * Math.Cos(robot.Orientation);
                                        double ballVy = (double)(INITIAL_BALL_SPEED) * Math.Sin(robot.Orientation);

                                        Console.WriteLine("ORIENTATION: " + robot.Orientation + " X: " + ballVx + " Y: " + ballVy);
                                        //ballVx += (double)(r.NextDouble() * 2 - 1) * randomComponent;
                                        //ballVy += (double)(r.NextDouble() * 2 - 1) * randomComponent;
                                        //RobotInfo prev = robot;
                                        //const double recoil = .02 / initial_ball_speed; ;
                                        UpdateBall(new BallInfo(ball.Position, new Vector2(ballVx, ballVy)));
                                        //UpdateRobot(robot, new RobotInfo(prev.Position + (new Vector2(-ballVx * recoil, -ballVy * recoil)), prev.Orientation, prev.ID));

                                        // We kicked get out of the loop and thus kill this thread.
                                        break;
                                    }
                                    Thread.Sleep(BREAKBEAM_CHECK_PERIOD);
                                    timeoutTimer.Stop();
                                    elapsed = timeoutTimer.Duration;
                                    timeoutTimer.Start();
                                } while (elapsed < BREAKBEAM_TIMEOUT);
                                break_beams[team][command.ID] = false;
                            });

                            breakBeamThread.Start();

                            break;
                        }
                }
            }
        }
       
        #region IPredictor
        public List<RobotInfo> GetRobots()
        {
            List<RobotInfo> result = new List<RobotInfo>();
            foreach (Team team in Enum.GetValues(typeof(Team)))
            {
                result.AddRange(robots[team]);
            }
            return result;
        }

        public List<RobotInfo> GetRobots(Team team)
        {
            return robots[team];
        }

        public RobotInfo GetRobot(Team team, int id)
        {
            RobotInfo result = robots[team].Find(robot => robot.ID == id);

            if (result == null)
                throw new ApplicationException(String.Format("Simulator can't find robot with id: {0} on team {1}", id, team));

            return result;
        }

        public BallInfo GetBall()
        {
            return ball;
        }

        public void SetBallMark()
        {
            if(ball != null)
                _markedPosition = ball.Position;

            _marking = true;
        }

        public void ClearBallMark()
        {
            _markedPosition = null;
            _marking = false;
        }

        public bool HasBallMoved()
        {
            if (!_marking)
                return false;

            if (_markedPosition == null)
                return false;

            if (ball == null)
                return false;

            return _markedPosition.distanceSq(ball.Position) > BALL_MOVED_DIST * BALL_MOVED_DIST;
        }

        public void SetPlayType(PlayType newPlayType)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
