using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Timers;
using Robocup.Core;
using Robocup.Utilities;
using Robocup.MotionControl;
using Robocup.CoreRobotics;
using Robocup.MessageSystem;
using Robocup.Geometry;

namespace Robocup.ControlForm
{
	/** RFCController class implements IController (move, kick) */
	public class Controller : IController
	{
        //Constants
        private const int CONTROL_TIMEOUT = 10;

        //Grabbing the constant now - won't change if constants are reloaded
        private static int NUM_ROBOTS = Constants.Basic.NUM_ROBOTS;

        private const double DRIBBLER_TIMER_PERIOD = 0.5; //seconds
        private const double DRIBBLER_TIMEOUT = 6.0; // seconds
        private bool DRAW_PATH;
        private double CHARGE_DIST;
        private double CHARGE_LOOP_PERIOD = 0.5; //seconds

        //Team for this controller
        private Team _team;
        
        //Interface for IO with the outside world
        private IMessageSender<RobotCommand> _cmdSender;
        private IPredictor _predictor;
        private FieldDrawer _fieldDrawer;

        //Planners for motion and kicing
        private IMotionPlanner _planner;
		private IKickPlanner _kickPlanner;

        private RobotPath[] _paths_to_follow;
        private RobotPath[] _last_successful_path;
        private Object[] _path_locks;

        //Move commands
        //private RobotInfo[] _move_infos;
        //private bool[] _move_avoid_balls;
        //private Object[] _move_locks;

        //Loop for running path driver
        private FunctionLoop _controlLoop;

        //Loop for running for figuring out when dribblers should stop
        private FunctionLoop _dribbleLoop;

        // Loop to precharge robots that are close enough to the ball
        private FunctionLoop _chargeLoop;

        //Lock-protected dictionary indicating when dribbling should stop
        private object _dribblingLock = new Object();
        private Dictionary<int, double> _stopDribbleAtTime = new Dictionary<int, double>();

	
        private List<int> _charging = new List<int>();
        private Dictionary<int, double> _lastCharge = new Dictionary<int, double>();


		public Controller(
			Team team,
			IMotionPlanner planner,
			IPredictor predictor,
			FieldDrawer fieldDrawer)
		{
			_team = team;			
			_predictor = predictor;
			_fieldDrawer = fieldDrawer;

            _planner = planner;
			_kickPlanner = new FeedbackVeerKickPlanner(new TangentBugFeedbackMotionPlanner());

            _last_successful_path = new RobotPath[NUM_ROBOTS];

            _paths_to_follow = new RobotPath[NUM_ROBOTS];
            _path_locks = new Object[NUM_ROBOTS];

            for (int i = 0; i < NUM_ROBOTS; i++)
                _path_locks[i] = new Object();

            //Initialize loops with the functions that they should call
            _controlLoop = new FunctionLoop(ControlLoop);
            _dribbleLoop = new FunctionLoop(DribbleLoop);
            _chargeLoop = new FunctionLoop(ChargeLoop);

            LoadConstants();
		}

        public void LoadConstants()
        {
            DRAW_PATH = ConstantsRaw.get<bool>("drawing", "DRAW_PATH");
            CHARGE_DIST = ConstantsRaw.get<double>("kickplanning", "CHARGE_DIST");

            _planner.LoadConstants();
            _kickPlanner.LoadConstants();
        }

        //CONNECTION TO SEND ROBOT COMMANDS-----------------------------------------------------------

        public void Connect(string host, int port)
        {
            if (_cmdSender != null)
                throw new ApplicationException("Already connected.");
            _cmdSender = Messages.CreateClientSender<RobotCommand>(host, port);            
            if (_cmdSender == null)
                throw new ApplicationException("Could not connect to " + host + ":" + port);
        }

        public void Disconnect()
        {
            if (_cmdSender == null)
                throw new ApplicationException("Not connected");
            _cmdSender.Close();
            _cmdSender = null;
        }

        //CONTROL STARTING / STOPPING ----------------------------------------------------

        public void StartControlling()
        {
            Console.WriteLine("Started");
            _controlLoop.SetPeriod(1.0 / Constants.Time.CONTROL_LOOP_FREQUENCY);
            _controlLoop.Start();
            _dribbleLoop.SetPeriod(DRIBBLER_TIMER_PERIOD);
            _dribbleLoop.Start();
            _chargeLoop.SetPeriod(CHARGE_LOOP_PERIOD);
            _chargeLoop.Start();
        }

        public void StopControlling()
        {
            Console.WriteLine("Stopped");
            _controlLoop.Stop();
            _dribbleLoop.Stop();
            _chargeLoop.Stop();

            for (int i = 0; i < NUM_ROBOTS; i++)
            {
                lock (_path_locks[i])
                {
                    _last_successful_path[i] = null;
                    _paths_to_follow[i] = null;
                }

                //Stop all robots from moving and dribbling
                RobotCommand command = new RobotCommand(i, new WheelSpeeds());
                _cmdSender.Post(command);
                RobotCommand command2 = new RobotCommand(i, RobotCommand.Command.STOP_DRIBBLER);
                _cmdSender.Post(command2);

                //TODO (davidwu) also stop charging and discharge?
            }
        }

        //DATA RETRIVAL---------------------------------------------------------------------
        public RobotPath[] GetLastPaths()
        {
            RobotPath[] paths = new RobotPath[NUM_ROBOTS];
            for (int i = 0; i < NUM_ROBOTS; i++)
                lock (_path_locks[i])
                {
                    paths[i] = _last_successful_path[i];
                }
            return paths;
        }


        //ROBOT COMMANDS---------------------------------------------------------------------

        public void Charge(int robotID)
        {
            this.Charge(robotID, RobotCommand.MAX_KICKER_STRENGTH);
        }

		public void Charge(int robotID, int strength)
		{
            lock (_charging)
            {
                //If we are already charging this robot do nothing...
                double currTime = HighResTimer.SecondsSinceStart();
                //if (charging.Contains(robotID)) {
                //    if (currTime - lastCharge[robotID] > chargeTime) {
                //        comport.Write(msgStopCharge);
                //        return;
                //    }
                //    return;
                //}

                RobotCommand command = new RobotCommand(robotID, RobotCommand.Command.START_VARIABLE_CHARGING);
                command.KickerStrength = (byte)strength;
                _cmdSender.Post(command);
                //Console.WriteLine("Controller: robot {0} is charging for a break-beam kick", robotID);

                _charging.Add(robotID);
                _lastCharge[robotID] = currTime;

                ////After 5*charge time, stop charging and dribbler to save battery
                //System.Threading.Timer t = new System.Threading.Timer(delegate(object o)
                //{
                //    _timers.Remove(robotID);

                //    command = new RobotCommand(robotID, RobotCommand.Command.STOP_CHARGING);
                //    _cmdSender.Post(command);
                //    //Console.WriteLine("Controller: robot {0} stopped charging", robotID);

                //    _charging.Remove(robotID);
                //}, null, 5 * CHARGE_TIME, System.Threading.Timeout.Infinite);
                //if(_timers.ContainsKey(robotID))
                //    _timers[robotID] = t;
            }
		}

        public void Move(RobotInfo destination, bool avoidBall)
        {
            if (double.IsNaN(destination.Position.X) || double.IsNaN(destination.Position.Y))
            {
                Console.WriteLine("invalid destination");
                return;
            }

            int id = destination.ID;

            double avoidBallDist = (avoidBall ? Constants.Motion.BALL_AVOID_DIST : 0f);
            RobotPath oldPath;
            lock (_path_locks[id])
            {
                oldPath = _last_successful_path[id];
            }

            //No RobotInfo - do nothing
            if (destination == null)
                return;

            //Plan a path
            RobotPath newPath;
            try
            {
                newPath = _planner.PlanMotion(_team, id, destination, _predictor, avoidBallDist, oldPath);
            }
            catch (Exception e)
            {
                Console.WriteLine("PlanMotion failed. Dumping exception:\n" + e.ToString());
                return;
            }

            lock (_path_locks[id])
            {
                if (newPath != null)
                    _last_successful_path[id] = newPath;
                _paths_to_follow[id] = newPath;
            }

            #region Drawing
            if (_fieldDrawer != null)
            {
                //Path commited for following
                if (DRAW_PATH)
                    _fieldDrawer.DrawPath(newPath);
                //Arrow showing final destination
                _fieldDrawer.DrawArrow(_team, id, ArrowType.Destination, destination.Position);
            }
            #endregion

        }
        
        public void Move(int robotID, bool avoidBall, Vector2 destination, double orientation)
		{
            Move(new RobotInfo(destination, orientation, robotID), avoidBall);
		}		

        public void Move(int robotID, bool avoidBall, Vector2 destination)
		{
			double orientation;
			try
			{
				RobotInfo robot = _predictor.GetRobot(_team, robotID);
				orientation = robot.Orientation;

			}
			catch (ApplicationException e)
			{
				Console.WriteLine("inside move: Predictor.GetRobot() failed. Dumping exception:\n" + e.ToString());
				return;
			}
			Move(robotID, avoidBall, destination, orientation); //TODO make it the current robot position
		}

        public void BreakBeam(int robotID, int strength)
        {
            RobotCommand command = new RobotCommand(robotID, RobotCommand.Command.MIN_BREAKBEAM_KICK);
            command.KickerStrength = (byte)strength;
            _cmdSender.Post(command);
        }

		/// <summary>
		/// Move to the ball and then kick it. Uses IKickPlanner- see David Robinson or Josh Montana
		/// with any questions
		/// </summary>
		/// <param name="robotID"></param>
		/// <param name="target"></param>
		public void Kick(int robotID, Vector2 target)
		{
            RobotCommand command;

            // Plan kick
			KickPlanningResults kpResults = _kickPlanner.kick(_team, robotID, target, _predictor);

			// If instructed, turn on break beam
			if (kpResults.turnOnBreakBeam)
			{
                command = new RobotCommand(robotID, RobotCommand.Command.BREAKBEAM_KICK);
                _cmdSender.Post(command);
			}

            command = new RobotCommand(robotID, kpResults.wheel_speeds);
            _cmdSender.Post(command);
		}

		public void Stop(int robotID)
		{
            lock (_path_locks[robotID])
            {
                _paths_to_follow[robotID] = null;
            }
            
            RobotCommand command = new RobotCommand(robotID, new WheelSpeeds());
            _cmdSender.Post(command);
		}

        public void StartDribbling(int robotID)
        {
            StartDribbling(robotID, DRIBBLER_TIMEOUT);
        }

        public void StartDribbling(int robotID, double secondsToDribble)
        {
            double curTime = HighResTimer.SecondsSinceStart();
            lock (_dribblingLock)
            {
                _stopDribbleAtTime[robotID] = curTime + secondsToDribble;
            }

            RobotCommand command = new RobotCommand(robotID, RobotCommand.Command.START_DRIBBLER);
            _cmdSender.Post(command);
        }

        public void StopDribbling(int robotID)
        {
            RobotCommand command = new RobotCommand(robotID, RobotCommand.Command.STOP_DRIBBLER);
            _cmdSender.Post(command);
        }

        //CHARGE LOOP-----------------------------------------------------------------------
        private void ChargeLoop()
        {
            List<RobotInfo> robots = _predictor.GetRobots();
            BallInfo ball = _predictor.GetBall();

            if (ball == null)
                return;

            foreach (RobotInfo robot in robots)
            {
                if (robot.Position.distance(ball.Position) <= CHARGE_DIST)
                    Charge(robot.ID);
            }
        }

        //DRIBBLE LOOP-----------------------------------------------------------------------

        private void DribbleLoop()
        {
            lock (_dribblingLock)
            {
                double time = HighResTimer.SecondsSinceStart();
                for (int i = 0; i < NUM_ROBOTS; i++)
                {
                    if (!_stopDribbleAtTime.ContainsKey(i))
                        continue;

                    if(time > _stopDribbleAtTime[i])
                    {
                        RobotCommand command = new RobotCommand(i, RobotCommand.Command.STOP_DRIBBLER);
                        _cmdSender.Post(command);
                    }
                }
            }
        }


        //CONTROL LOOP---------------------------------------------------------------------
        private void ControlLoop()
        {
            FollowPaths();
            _fieldDrawer.UpdateControllerDuration(_controlLoop.GetLoopDuration() * 1000);
        }


        private void FollowPaths()
        {
            for (int i = 0; i < NUM_ROBOTS; i++)
            {
                RobotPath newPath;
                //Retrieve commands given to the controller for the robot
                lock (_path_locks[i])
                {
                    newPath = _paths_to_follow[i];
                }

                if (newPath == null || newPath.isEmpty())
                    continue;

                //Follow the path
                MotionPlanningResults mpResults;
                try
                {
                    mpResults = _planner.FollowPath(newPath, _predictor);
                }
                catch (Exception e)
                {
                    Console.WriteLine("FollowPath failed. Dumping exception:\n" + e.ToString());
                    continue;
                }

                //Send the wheel speeds
                RobotCommand command = new RobotCommand(i, mpResults.wheel_speeds);
                _cmdSender.Post(command);
            }
        }
	}
}