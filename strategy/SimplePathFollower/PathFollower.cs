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

        double MIN_GOAL_DIST = .10;
        double MIN_GOAL_DIFF_ORIENTATION = .3;

        // represents whether the robot has yet reached the goal
        public bool reachedPoint = false;

		public int RobotID { get { return robotID; } set { robotID = value; } }
		public List<Vector2> Waypoints { get { return waypoints; } set { waypoints = value; } }

		private IPredictor predictor;
		private IMotionPlanner planner;
		private IRobots commander;
		private RFCController controller;

		public IPredictor Predictor { get { return predictor; } }
		public IRobots Commander { get { return commander; } }
        public IMotionPlanner Planner { get { return planner; } }

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

        // returns whether an error has occured or not
		public bool Follow()
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

                // if lapping stops midway...
                if (waypointIndex > waypoints.Count - 1) {
                    waypointIndex = 0;
                }

                try {
                     curinfo = predictor.getCurrentInformation(robotID);
                     ballInfo = predictor.getBallInfo();
                } catch (ApplicationException e) {
                    Console.WriteLine("Failed Predictor.getCurrentInformation(). Dumping exception\n" + e.ToString());                    
                    return true;
                }
				/*double wpDistanceSq = curinfo.Position.distanceSq(waypoints[waypointIndex]);
                if (wpDistanceSq > MIN_SQ_DIST_TO_WP) {
                    waypointIndex = (waypointIndex + 1);//Stop at end for now % waypoints.Count;
                    controller.move(robotID, false, waypoints[waypointIndex]);
                }*/
                controller.move(robotID, false, waypoints[waypointIndex],0.0);

                // Lap around

                double sqDistToGoal = curinfo.Position.distanceSq(waypoints[waypointIndex]);
                double diffOrientation = Math.Abs(angleDifference(curinfo.Orientation, 0));

                if (sqDistToGoal < MIN_GOAL_DIST * MIN_GOAL_DIST && diffOrientation < MIN_GOAL_DIFF_ORIENTATION) {
                    waypointIndex = (waypointIndex + 1) % waypoints.Count;
                }
			
				System.Threading.Thread.Sleep(10);
            } while (running);

            return false;
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
        public void BeamKick() {
            running = true;
            ActionInterpreter interpreter = new ActionInterpreter(controller, predictor);
            interpreter.BeamKick(robotID, new Vector2(0, 0));
            do {

                interpreter.BeamKick(robotID, new Vector2(0, 0));
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
        //in particular, reloads PID and other constants for the planner
        public void reloadConstants() {
            // calls reloadConstants within planner
            planner.ReloadConstants();
        }

        /// <summary>
        /// Returns how many radians counter-clockwise the ray defined by angle1
        /// needs to be rotated to point in the direction angle2. Stolen from UsefulFunctions
        /// in Geometry namespace.
        /// Uses
        /// Returns a value in the range [-Pi,Pi)
        /// </summary>
        static private double angleDifference(double angle1, double angle2) {
            //first get the inputs in the range [0, 2Pi):
            while (angle1 < 0)
                angle1 += Math.PI * 2;
            while (angle2 < 0)
                angle2 += Math.PI * 2;
            angle1 %= Math.PI * 2;
            angle2 %= Math.PI * 2;

            double anglediff = angle2 - angle1;
            anglediff = (anglediff + Math.PI * 2) % (Math.PI * 2);
            //anglediff is now in the range [0,Pi*2)

            //now we need to get the range to [-Pi, Pi):
            if (anglediff < Math.PI)
                return anglediff;
            else
                return anglediff - Math.PI * 2;
        }
	}
}
