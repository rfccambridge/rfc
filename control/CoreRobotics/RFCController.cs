using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Robocup.Core;
using Robocup.Utilities;
using Robocup.MotionControl;

namespace Robocup.CoreRobotics
{
	/** RFCController class implements IController (move, kick)
	 *  
	 *  Move method: calls INavigator, uses result for IMovement, and passes commands to IRobots
	 *  kick method: calls IRobots directly
	 * 
	 */
	public class RFCController : IController
	{
		// Kick planner
		IMotionPlanner regularPlanner;
		IKickPlanner _kickPlanner;
		FieldDrawer _fieldDrawer;
		private Team _team;

		private RobotPath[] paths;
		private const int NUM_ROBOTS = 10; //For simulation
		private Object pathsLock = new object();

		private const int control_timeout = 10;
		private double CONTROL_LOOP_FREQUENCY;
		private double control_period;
		private bool control_running;
		private int[] follows_since_plan;
		private System.Timers.Timer t;

		public RFCController(
			Team team,
			IRobots commander,
			IMotionPlanner planner,
			IPredictor predictor,
			FieldDrawer fieldDrawer)
		{
			_team = team;
			_commander = commander;
			_planner = planner;
			_predictor = predictor;
			_fieldDrawer = fieldDrawer;

			regularPlanner = new TangentBugFeedbackMotionPlanner();
			_kickPlanner = new FeedbackVeerKickPlanner(regularPlanner);

			paths = new RobotPath[NUM_ROBOTS];
			follows_since_plan = new int[NUM_ROBOTS];
			control_running = false;

			LoadConstants();
		}

		# region private IRobots,IMovement,INavigator and accessors

		private IRobots _commander;
		public IRobots Commander
		{
			get { return _commander; }
			set { _commander = value; }
		}

		private IMotionPlanner _planner;
		public IMotionPlanner Planner
		{
			get { return _planner; }
			set { _planner = value; }
		}

		private IPredictor _predictor;
		public IPredictor Predictor
		{
			get { return _predictor; }
		}

		private Vector2 getPosition(RobotInfo info)
		{
			return info.Position;
		}

		#endregion

		double ballAvoidDist = .12;

		#region IController Members

		public void charge(int robotID)
		{
			Commander.charge(robotID);
		}
		public void kick(int robotID)
		{
			Commander.kick(robotID);
		}
		public void beamKick(int robotID, bool goForward)
		{
			Commander.beamKick(robotID);

			if (goForward)
			{
				WheelSpeeds speeds = new WheelSpeeds(10, 17, 17, 30);
				Commander.setMotorSpeeds(robotID, speeds);
				System.Threading.Thread.Sleep(1500);
			}
		}
		public void move(int robotID, bool avoidBall, Vector2 destination, double orientation)
		{
			//Console.WriteLine("Destination in RFCController immediately: " + destination.ToString());
			if (double.IsNaN(destination.X) || double.IsNaN(destination.Y))
			{
				Console.WriteLine("invalid destination");
				return;
			}

			double avoidBallDist = (avoidBall ? ballAvoidDist : 0f);
			/*NavigationResults results =
				Navigator.navigate(robotID,
					thisRobot.Position,
					destination,
					Predictor.getOurTeamInfo().ToArray(),
					Predictor.getTheirTeamInfo().ToArray(),
					Predictor.getBallInfo(),
					avoidBallDist);
             
			WheelSpeeds motorSpeeds = GetPlanner(robotID).calculateWheelSpeeds(Predictor, robotID, thisRobot, results);
			 */

			RobotPath currPath;

			//Console.WriteLine("Destination in RFCController later: " + destination.ToString());

			try
			{
				currPath = Planner.PlanMotion(_team, robotID, new RobotInfo(destination, orientation, robotID),
											  Predictor, avoidBallDist);
			}
			catch (ApplicationException e)
			{
				Console.WriteLine("PlanMotion failed. Dumping exception:\n" + e.ToString());
				return;
			}

			lock (pathsLock)
			{
				// Commit path for following
				paths[currPath.ID] = currPath;
			}

			// Clear timeout counter
			follows_since_plan[robotID] = 0;

			// We've already committed a path for following, now start the 
			// control loop if it's not already running
			if (!control_running)
			{
				t = new System.Timers.Timer(control_period);
				t.AutoReset = true;
				t.Elapsed += delegate
								{
									Thread.CurrentThread.Name = "RFCController timer thread";
									followPaths();
								};
				t.Start();
				control_running = true;
			}

			#region Drawing
			//Arrow showing final destination
			if (_fieldDrawer != null)
			{
				_fieldDrawer.DrawArrow(_team, currPath.ID, ArrowType.Destination, currPath.getFinalState().Position);
			}

			#endregion
		}

		private void followPaths()
		{
			for (int i = 0; i < NUM_ROBOTS; i++)
			{
				//Keep a local copy of the path in order not to lock the whole procedure
				RobotPath currPath;
				
				lock (pathsLock)
				{
					// Ensures we clear any stale paths if no planning calls have been made
					if (follows_since_plan[i] >= control_timeout)
					{
						paths[i] = null;
						continue;
					}

					follows_since_plan[i]++;

					// Important because we may not be planning for the whole team
					if (paths[i] == null)
						continue;

					currPath = paths[i];
				}

				//If we've been sent an empty path, this is a clear sign to stop
				if (currPath.Waypoints == null)
				{
					Commander.setMotorSpeeds(currPath.ID, new WheelSpeeds());
					continue;
				}

				MotionPlanningResults mpResults = Planner.FollowPath(currPath, Predictor);
				WheelSpeeds wheelSpeeds = mpResults.wheel_speeds;

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

				//Console.WriteLine("Sending speeds: {0}, {1}, {2}, {3}",
				//	wheelSpeeds.rf, wheelSpeeds.lf, wheelSpeeds.lb, wheelSpeeds.rb);
				Commander.setMotorSpeeds(currPath.ID, wheelSpeeds);
			}
		}

		public void move(int robotID, bool avoidBall, Vector2 destination)
		{
			double orientation;
			try
			{
				RobotInfo robot = Predictor.GetRobot(_team, robotID);
				orientation = robot.Orientation;

			}
			catch (ApplicationException e)
			{
				Console.WriteLine("inside move: Predictor.GetRobot() failed. Dumping exception:\n" + e.ToString());
				return;
			}
			move(robotID, avoidBall, destination, orientation); //TODO make it the current robot position
		}

		/// <summary>
		/// Move to the ball and then kick it. Uses IKickPlanner- see David Robinson or Josh Montana
		/// with any questions
		/// </summary>
		/// <param name="robotID"></param>
		/// <param name="target"></param>
		public void moveKick(int robotID, Vector2 target)
		{
			// Plan kick
			KickPlanningResults kpResults = _kickPlanner.kick(_team, robotID, target, Predictor);

			WheelSpeeds wheelSpeeds = kpResults.wheel_speeds;
			bool turnOnBreakBeam = kpResults.turnOnBreakBeam;

			// If instructed, turn on break beam
			if (turnOnBreakBeam)
			{
				beamKick(robotID, false);
			}

			//Console.WriteLine("KickPlanner sent back speeds of " + wheelSpeeds.toString());

			Commander.setMotorSpeeds(robotID, wheelSpeeds);
		}

		public void stop(int robotID)
		{
			paths[robotID] = null;
			Commander.setMotorSpeeds(robotID, new WheelSpeeds());
		}

		public void StopControlling()
		{
			if (control_running)
			{
				lock (pathsLock)
				{
					for (int i = 0; i < NUM_ROBOTS; i++)
						paths[i] = null;
				}

				t.Stop();
				control_running = false;
			}
		}

		#endregion

		private Vector2[] getPositions(RobotInfo[] robotInfo)
		{
			Vector2[] ret = new Vector2[robotInfo.Length];
			for (int i = 0; i < robotInfo.Length; i++)
			{
				ret[i] = robotInfo[i].Position;
			}
			return ret;
		}

		public void LoadConstants()
		{
			CONTROL_LOOP_FREQUENCY = Constants.get<double>("default", "CONTROL_LOOP_FREQUENCY");
			control_period = 1 / CONTROL_LOOP_FREQUENCY * 1000; //in ms

			_planner.LoadConstants();
			_kickPlanner.LoadConstants();
			//_predictor
		}
	}
}