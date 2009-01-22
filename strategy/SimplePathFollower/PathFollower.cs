using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Plays;
using Robocup.Core;
using Robocup.CoreRobotics;
using Robocup.MotionControl;

namespace SimplePathFollower
{
	class PathFollower
	{
		private int robotID;
		private List<Vector2> waypoints;
		private int waypointIndex;
		private bool running;

		public int RobotID { get { return robotID; } set { robotID = value; } }
		public List<Vector2> Waypoints { get { return waypoints; } set { waypoints = value; } }

		private IPredictor predictor;
		private IMotionPlanner planner;
		private IRobots commander;
		private RFCController controller;

		public IPredictor Predictor { get { return predictor; } }
		public IRobots Commander { get { return commander; } }

		public PathFollower()
		{
			robotID = 0;
			waypoints = new List<Vector2>();
			waypointIndex = 0;
			running = false;
		}

		public PathFollower(int _robotID, List<Vector2> waypointList)
		{
			robotID = _robotID;
			waypoints = waypointList;
			waypointIndex = 0;
			running = false;
		}


		public void Init()
		{
			if (predictor != null)
			{
				Console.Write("Predictor already running.");
				return;
			}

			if (planner != null)
			{
				Console.Write("Planner already running.");
				return;
			}

			if (controller != null)
			{
				Console.Write("Controller already running.");
				return;
			}

			commander = new Robocup.ControlForm.RemoteRobots();
			predictor = new BasicPredictor();
			planner = new MixedBiRRTMotionPlanner();
			controller = new RFCController(commander, planner, predictor);
		}


		public void Follow()
		{
			running = true;
			const double MIN_SQ_DIST_TO_WP = .01;
			waypointIndex = 0;
			
			controller.move(robotID, false, waypoints[waypointIndex]);

			do
			{
				double wpDistanceSq = predictor.getCurrentInformation(robotID).Position.distanceSq
					(waypoints[waypointIndex]);
				if (wpDistanceSq < MIN_SQ_DIST_TO_WP)
				{
					waypointIndex = (waypointIndex + 1) % waypoints.Count;
					controller.move(robotID, false, waypoints[waypointIndex]);
				}
			
				System.Threading.Thread.Sleep(1000);
			} while (running);
		}

		public void Stop()
		{
			running = false;
		}
	}
}
