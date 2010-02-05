using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Robocup.Core;
using Robocup.Utilities;
using Robocup.MotionControl;
using Robocup.CoreRobotics;
using Robocup.MessageSystem;

namespace Robocup.ControlForm
{
	/** RFCController class implements IController (move, kick)
	 *  
	 *  Move method: calls INavigator, uses result for IMovement, and passes commands to IRobots
	 *  kick method: calls IRobots directly
	 * 
	 */
	public class Controller : IController
	{
        private const int CONTROL_TIMEOUT = 10;
        private const int NUM_ROBOTS = 10; //For simulation
        private const int CHARGE_TIME = 4;  // TODO: madeup value! is this in seconds or milliseconds        

        private Team _team;
        
        private IMessageSender<RobotCommand> _cmdSender;
        private IPredictor _predictor;
        private FieldDrawer _fieldDrawer;

        private IMotionPlanner _planner;
        private IMotionPlanner _regularPlanner;
		private IKickPlanner _kickPlanner;		        

		private RobotPath[] _paths;		
		private Object _pathsLock = new object();
                
		private double CONTROL_LOOP_FREQUENCY;
        private bool DRAW_PATH;

		private double _controlPeriod;
		private bool _controlRunning;
		private int[] _followsSincePlan;
		private System.Timers.Timer _followPathsTimer;
        private int _followPathsTimerSync = 0;
        double _ballAvoidDist = .12;

        private List<int> _charging = new List<int>();
        private Dictionary<int, double> _lastCharge = new Dictionary<int, double>();
        private Dictionary<int, System.Threading.Timer> _timers = new Dictionary<int, System.Threading.Timer>();

		public Controller(
			Team team,
			IMotionPlanner planner,
			IPredictor predictor,
			FieldDrawer fieldDrawer)
		{
			_team = team;			
			_planner = planner;
			_predictor = predictor;
			_fieldDrawer = fieldDrawer;

			_regularPlanner = new TangentBugFeedbackMotionPlanner();
			_kickPlanner = new FeedbackVeerKickPlanner(_regularPlanner);

			_paths = new RobotPath[NUM_ROBOTS];
			_followsSincePlan = new int[NUM_ROBOTS];
			_controlRunning = false;

			LoadConstants();
		}

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

		public void Charge(int robotID)
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

                RobotCommand command = new RobotCommand(robotID, RobotCommand.Command.START_CHARGING);
                _cmdSender.Post(command);
                Console.WriteLine("Controller: robot {0} is charging for a break-beam kick", robotID);

                _charging.Add(robotID);
                _lastCharge[robotID] = currTime;

                //After 5*charge time, stop charging to save battery
                System.Threading.Timer t = new System.Threading.Timer(delegate(object o)
                {
                    _timers.Remove(robotID);

                    command = new RobotCommand(robotID, RobotCommand.Command.STOP_CHARGING);
                    _cmdSender.Post(command);
                    Console.WriteLine("Controller: robot {0} stopped charging", robotID);

                    _charging.Remove(robotID);
                }, null, 5 * CHARGE_TIME, System.Threading.Timeout.Infinite);
                _timers[robotID] = t;
            }
		}		
	
		public void Move(int robotID, bool avoidBall, Vector2 destination, double orientation)
		{
			if (double.IsNaN(destination.X) || double.IsNaN(destination.Y))
			{
				Console.WriteLine("invalid destination");
				return;
			}

			double avoidBallDist = (avoidBall ? _ballAvoidDist : 0f);

			RobotPath currPath;

			try
			{
				currPath = _planner.PlanMotion(_team, robotID, new RobotInfo(destination, orientation, robotID),
											  _predictor, avoidBallDist);
			}
			catch (ApplicationException e)
			{
				Console.WriteLine("PlanMotion failed. Dumping exception:\n" + e.ToString());
				return;
			}

			lock (_pathsLock)
			{
				// Commit path for following
				_paths[currPath.ID] = currPath;
			}

			// Clear timeout counter
			_followsSincePlan[robotID] = 0;

			// We've already committed a path for following, now start the 
			// control loop if it's not already running
			if (!_controlRunning)
			{
				_followPathsTimer = new System.Timers.Timer(_controlPeriod);
				_followPathsTimer.AutoReset = true;
				_followPathsTimer.Elapsed += delegate
								{
                                    // Skip event if still handling a previous event
                                    if (Interlocked.CompareExchange(ref _followPathsTimerSync, 1, 0) == 0)
                                    {
                                        Thread.CurrentThread.Name = "Controller timer thread";
                                        followPaths();
                                        _followPathsTimerSync = 0;
                                    }
								};
				_followPathsTimer.Start();
				_controlRunning = true;
			}

			#region Drawing
			//Arrow showing final destination
			if (_fieldDrawer != null)
			{
				//Path commited for following
                if(DRAW_PATH)
                    _fieldDrawer.DrawPath(currPath);
                //Arrow showing final destination
                _fieldDrawer.DrawArrow(_team, currPath.ID, ArrowType.Destination,
                    destination);
                    //currPath.getFinalState().Position);
			}

			#endregion
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
			_paths[robotID] = null;
            RobotCommand command = new RobotCommand(robotID, new WheelSpeeds());
            _cmdSender.Post(command);
		}

		public void StopControlling()
		{
			if (_controlRunning)
			{
				lock (_pathsLock)
				{
					for (int i = 0; i < NUM_ROBOTS; i++)
						_paths[i] = null;
				}

				_followPathsTimer.Stop();
				_controlRunning = false;
			}
		}        

		public void LoadConstants()
		{
			CONTROL_LOOP_FREQUENCY = Constants.get<double>("default", "CONTROL_LOOP_FREQUENCY");			
            _controlPeriod = 1 / CONTROL_LOOP_FREQUENCY * 1000; //in ms

            DRAW_PATH = Constants.get<bool>("drawing", "DRAW_PATH");

			_planner.LoadConstants();
			_kickPlanner.LoadConstants();
			//_predictor
		}

        private void followPaths()
        {
            RobotCommand command;

            for (int i = 0; i < NUM_ROBOTS; i++)
            {
                //Keep a local copy of the path in order not to lock the whole procedure
                RobotPath currPath;

                lock (_pathsLock)
                {
                    // Ensures we clear any stale paths if no planning calls have been made
                    if (_followsSincePlan[i] >= CONTROL_TIMEOUT)
                    {
                        _paths[i] = null;
                        continue;
                    }

                    _followsSincePlan[i]++;

                    // Important because we may not be planning for the whole team
                    if (_paths[i] == null)
                        continue;

                    currPath = _paths[i];
                }

                //If we've been sent an empty path, this is a clear sign to stop
                if (currPath.Waypoints == null)
                {
                    command = new RobotCommand(currPath.ID, new WheelSpeeds());
                    _cmdSender.Post(command);
                    continue;
                }

                MotionPlanningResults mpResults = _planner.FollowPath(currPath, _predictor);

                #region Drawing code
#if false
				//This doesn't make any sense (we should be thread-safe), but without it, 
				//drawing fails after we stop.
				if (paths[i] == null)
					continue;

				if (_fieldDrawer != null)
				{
					//Arrow showing nearest waypoint in path
					// TODO: Need to make NearestWaypoint populated inside each implementation of IMotionPlanner
					if (mpResults.NearestWaypoint != null)
						_fieldDrawer.DrawArrow(_team, currPath.ID, ArrowType.Waypoint, mpResults.NearestWaypoint.Position);
				}
#endif
                #endregion

                command = new RobotCommand(currPath.ID, mpResults.wheel_speeds);
                _cmdSender.Post(command);
            }
        }      
	}
}