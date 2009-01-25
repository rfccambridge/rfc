using System;
using System.Collections.Generic;
using System.Text;
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
			planner = new FeedbackMotionPlanner();
			controller = new RFCController(commander, planner, predictor);
		}


		public void Follow()
		{
			running = true;
            const double MIN_SQ_DIST_TO_WP = 0;// because distances are very small .01;
			waypointIndex = 0;
			
			controller.move(robotID, false, waypoints[waypointIndex]);

			do
			{
                RobotInfo curinfo;
                try {
                     curinfo = predictor.getCurrentInformation(robotID);
                } catch (ApplicationException e) {
                    Console.WriteLine("Failed Predictor.getCurrentInformation(). Dumping exception\n" + e.ToString());
                    return;
                }
				double wpDistanceSq = curinfo.Position.distanceSq
					(waypoints[waypointIndex]);
				if (wpDistanceSq > MIN_SQ_DIST_TO_WP)
				{
					waypointIndex = (waypointIndex + 1) % waypoints.Count;
					controller.move(robotID, false, waypoints[waypointIndex]);
				}
			
				System.Threading.Thread.Sleep(10);
			} while (running);
		}

		public void Stop()
		{
			running = false;
		}

        public void drawCurrent(System.Drawing.Graphics g, ICoordinateConverter converter) {            
            controller.drawCurrent(g, converter);
        }
        public void clearArrows() {
            controller.clearArrows();
        }

        //seeks to reload any constants that this class and its own objects use from files
        //currently just PID constants, so having it call reload on the planner. Not sure if it should call controller as well or
        //if there should be a controll.reloadConstants(); planne.reloadConstants() chain
        public void reloadConstants() {
            FeedbackMotionPlanner myTempPlanner = (FeedbackMotionPlanner)planner;
            myTempPlanner.reloadConstants();

        }
	}
}
