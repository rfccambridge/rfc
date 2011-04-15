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
using System.IO;

namespace Robocup.ControlForm
{
	/** RFCController class implements IController (move, kick) */
	public class Controller : IController
	{
        //Constants
        private const int CONTROL_TIMEOUT = 10;
        static int NUM_ROBOTS = Constants.get<int>("default", "NUM_ROBOTS");
        private const double DRIBBLER_TIMER_PERIOD = 0.5; //seconds
        private const double DRIBBLER_TIMEOUT = 6.0; // seconds
        private double CONTROL_LOOP_FREQUENCY;
        private bool DRAW_PATH;
        private double BALL_AVOID_DIST;

        //Team for this controller
        private Team _team;
        
        //Interface for IO with the outside world
        private IMessageSender<RobotCommand> _cmdSender;
        private IPredictor _predictor;
        private FieldDrawer _fieldDrawer;

        //Planners for motion and kicing
        private IMotionPlanner _planner;
		private IKickPlanner _kickPlanner;

        //Move commands
		private RobotInfo[] _move_infos;
        private bool[] _move_avoid_balls;
        private RobotPath[] _last_successful_path;
        private Object[] _move_locks;

        //Loop for running path driver
        private FunctionLoop _controlLoop;

        //Loop for running for figuring out when dribblers should stop
        private FunctionLoop _dribbleLoop;

        //Lock-protected dictionary indicating when dribbling should stop
        private object _dribblingLock = new Object();
        private Dictionary<int, double> _stopDribbleAtTime = new Dictionary<int, double>();

	
        private List<int> _charging = new List<int>();
        private Dictionary<int, double> _lastCharge = new Dictionary<int, double>();


        //Hack to output position and wheel speed data!
        private TextWriter outStream;
        private static int outCount = 0;


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

            _move_infos = new RobotInfo[NUM_ROBOTS];
            _move_avoid_balls = new bool[NUM_ROBOTS];
            _last_successful_path = new RobotPath[NUM_ROBOTS];
            _move_locks = new Object[NUM_ROBOTS];
            for (int i = 0; i < NUM_ROBOTS; i++)
                _move_locks[i] = new Object();

            //Initialize loops with the functions that they should call
            _controlLoop = new FunctionLoop(ControlLoop);
            _dribbleLoop = new FunctionLoop(DribbleLoop);

            //Hack to output position and wheel speed data!
            if(team == Team.Blue)
                outStream = new StreamWriter("poswheeldata" + (outCount++) + ".txt");

            LoadConstants();
		}

        public void LoadConstants()
        {
            CONTROL_LOOP_FREQUENCY = Constants.get<double>("default", "CONTROL_LOOP_FREQUENCY");
            DRAW_PATH = Constants.get<bool>("drawing", "DRAW_PATH");
            BALL_AVOID_DIST = Constants.get<double>("motionplanning", "BALL_AVOID_DIST");

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
            _controlLoop.SetPeriod(1.0 / CONTROL_LOOP_FREQUENCY);
            _controlLoop.Start();
            _dribbleLoop.SetPeriod(DRIBBLER_TIMER_PERIOD);
            _dribbleLoop.Start();
        }

        public void StopControlling()
        {
            Console.WriteLine("Stopped");
            _controlLoop.Stop();
            _dribbleLoop.Stop();

            for (int i = 0; i < NUM_ROBOTS; i++)
            {
                lock (_move_locks[i])
                {
                    _move_infos[i] = null;
                    _move_avoid_balls[i] = false;
                    _last_successful_path[i] = null;
                }

                //Stop all robots from moving and dribbling
                RobotCommand command = new RobotCommand(i, new WheelSpeeds());
                _cmdSender.Post(command);
                RobotCommand command2 = new RobotCommand(i, RobotCommand.Command.STOP_DRIBBLER);
                _cmdSender.Post(command2);

                //TODO (davidwu) also stop charging and discharge?
            }
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
            lock (_move_locks[id])
            {
                _move_infos[id] = new RobotInfo(destination);
                _move_avoid_balls[id] = avoidBall;
            }
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
            RobotCommand command = new RobotCommand(robotID, RobotCommand.Command.FULL_BREAKBEAM_KICK);
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
            lock (_move_locks[robotID])
            {
                _move_infos[robotID] = null;
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
            PerformMotion();
            _fieldDrawer.UpdateControllerDuration(_controlLoop.GetLoopDuration());
        }


        private void PerformMotion()
        {
            for (int i = 0; i < NUM_ROBOTS; i++)
            {


                //Retrieve commands given to the controller for the robot
                RobotInfo desired;
                double avoidBallDist;
                RobotPath oldPath;
                lock (_move_locks[i])
                {
                    desired = _move_infos[i];
                    avoidBallDist = (_move_avoid_balls[i] ? BALL_AVOID_DIST : 0f);
                    oldPath = _last_successful_path[i];
                }

                RobotCommand command;

                //No RobotInfo - do nothing
                if (desired == null)
                    continue;

                //Plan a path
                RobotPath newPath;
                try
                {
                    newPath = _planner.PlanMotion(_team, i, desired, _predictor, avoidBallDist, oldPath);
                }
                catch (Exception e)
                {
                    Console.WriteLine("PlanMotion failed. Dumping exception:\n" + e.ToString());
                    continue;
                }

                if (newPath == null)
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
                command = new RobotCommand(i, mpResults.wheel_speeds);
                _cmdSender.Post(command);
                
                /*
                if (_team == Team.Blue)
                {
                    RobotInfo info = _predictor.GetRobot(_team, i);
                    if (info != null)
                    {
                        Vector2 pos = info.Position;
                        outStream.Write(command.ID + " "
                            + pos.X + " "
                            + pos.Y + " "
                            + info.Orientation + " "
                            + info.Velocity.X + " "
                            + info.Velocity.Y + " "
                            + command.Speeds.rf + " "
                            + command.Speeds.lf + " "
                            + command.Speeds.lb + " "
                            + command.Speeds.rb + " ");
                    }
                }*/

                #region Drawing
                //Arrow showing final destination
                if (_fieldDrawer != null)
                {
                    //Path commited for following
                    if (DRAW_PATH)
                        _fieldDrawer.DrawPath(newPath);
                    //Arrow showing final destination
                    _fieldDrawer.DrawArrow(_team, i, ArrowType.Destination, desired.Position);
                }

                #endregion
            }

            /*
            if (_team == Team.Blue)
            {
                outStream.WriteLine();
                outStream.Flush();
            }*/

        }
	}
}