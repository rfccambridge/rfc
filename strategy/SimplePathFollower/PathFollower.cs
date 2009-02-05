using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using Robocup.CoreRobotics;
using Robocup.MotionControl;
using Robocup.Plays;

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

        private const double MIN_SQ_DIST_TO_WP = 0.0001;// within 1 cm

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


		public void Init(IMotionPlanner newPlanner)
		{
			if (predictor != null)
			{
				Console.Write("Predictor already running.");
				return;
			}

		/*	if (planner != null)
			{
				Console.Write("Planner already running.");
				return;
			}*/

			if (controller != null)
			{
				Console.Write("Controller already running.");
				return;
			}

			commander = new Robocup.ControlForm.RemoteRobots();
			predictor = new BasicPredictor();
            planner = newPlanner;
             
			controller = new RFCController(commander, planner, predictor);
		}

        public bool setPlanner(IMotionPlanner newPlanner) {
            if (!running) {
                planner = newPlanner;
                controller = new RFCController(commander, planner, predictor);
                return true;
            }

            return false;
        }

		public void Follow()
		{
			running = true;
			waypointIndex = 0;

            //because this class just gets one point from the gui,
            //generatign a static path is taken care of in feedbackbackMotionPlanner

            controller.move(robotID, false, waypoints[0]);
           
            do
			{
                RobotInfo curinfo;
                BallInfo ballInfo;
                try {
                     curinfo = predictor.getCurrentInformation(robotID);
                     ballInfo = predictor.getBallInfo();
                } catch (ApplicationException e) {
                    Console.WriteLine("Failed Predictor.getCurrentInformation(). Dumping exception\n" + e.ToString());
                    return;
                }
				/*double wpDistanceSq = curinfo.Position.distanceSq(waypoints[waypointIndex]);
                if (wpDistanceSq > MIN_SQ_DIST_TO_WP) {
                    waypointIndex = (waypointIndex + 1);//Stop at end for now % waypoints.Count;
                    controller.move(robotID, false, waypoints[waypointIndex]);
                }*/
                controller.move(robotID, false, waypoints[waypointIndex]);
			
				System.Threading.Thread.Sleep(10);
			} while (running);
		}

        public void Kick() 
        {
            running = true;

            ActionInterpreter interpreter = new ActionInterpreter(controller, predictor);
            interpreter.Kick(robotID, new Vector2(0, 0));
            do {

                interpreter.Kick(robotID, new Vector2(0, 0));
                System.Threading.Thread.Sleep(10);
            } while (running);
        }

		public void Stop()
		{
            controller.stop(robotID);
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
            // calls reloadConstants within planner
            planner.ReloadConstants();
            //CircleFeedbackMotionPlanner myTempPlanner = (CircleFeedbackMotionPlanner)planner;
            //myTempPlanner.reloadConstants();

        }
	}
}
