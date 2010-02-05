using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using Robocup.CoreRobotics;
using Robocup.MotionControl;
using Robocup.Plays;
using Robocup.ControlForm;

namespace SimplePathFollower
{
	public class PathFollower
	{
        private Team team;
		private int robotID;
		private List<Vector2> waypoints;
		private int waypointIndex;
		private bool running;
        private bool lapping;

		private int _sleepTime;

        const double MIN_GOAL_DIST = .06;
        const double MIN_GOAL_DIFF_ORIENTATION = .3;

        // represents whether the robot has yet reached the goal
        public bool reachedPoint = false;

		public int RobotID { get { return robotID; } set { robotID = value; } }
		public List<Vector2> Waypoints { get { return waypoints; } set { waypoints = value; } }

		private IPredictor predictor;
		private IMotionPlanner planner;
		private Robocup.MessageSystem.IMessageSender<RobotCommand> commander;
		private Controller controller;
        private Interpreter interpreter;

		public IPredictor Predictor { get { return predictor; } }
        public Robocup.MessageSystem.IMessageSender<RobotCommand> Commander { get { return commander; } set { commander = value; } }
        public IMotionPlanner Planner { get { return planner; } }
        public Controller Controller { get { return controller; } }
        public Interpreter Interpreter { get { return interpreter; } set { interpreter = value; } }

        public delegate void EndLapDelegate(bool success, bool invokeStop);
        public EndLapDelegate OnEndLap = null;
        public delegate void StartLapDelegate();
        public StartLapDelegate OnStartLap = null;

        private const double MIN_SQ_DIST_TO_WP = 0.0001;// within 1 cm

		public PathFollower()
		{
			robotID = 0;
			waypoints = new List<Vector2>();
			waypointIndex = 0;
			running = false;
            lapping = false;

            team = (Team)Enum.Parse(typeof(Team), Constants.get<string>("configuration", "OUR_TEAM"), true);
		}

		public PathFollower(int _robotID, List<Vector2> waypointList)
		{
			robotID = _robotID;
			waypoints = waypointList;
			waypointIndex = 0;
			running = false;
            lapping = false;
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
			
			predictor = new AveragingPredictor();
            planner = newPlanner;
             
			controller = new Controller(team, planner, predictor, null);

			_sleepTime = Constants.get<int>("default", "UPDATE_SLEEP_TIME");
		}

        public bool setPlanner(IMotionPlanner newPlanner) {
            if (!running) {
                planner = newPlanner;
                controller = new Controller(team, planner, predictor, null);
                return true;
            }

            return false;
        }

        // returns whether an error has occured or not
		public bool Follow()
		{
			running = true;
            lapping = false;
			waypointIndex = 0;

            //because this class just gets one point from the gui,
            //generating a static path is taken care of in feedbackbackMotionPlanner

            controller.move(robotID, false, waypoints[waypointIndex], 0.0);

            do 
            {
                RobotInfo curinfo;
                // if lapping stops midway...
                //if (waypointIndex > waypoints.Count - 1) {
                //    waypointIndex = 0;
                //}

                try {
                     curinfo = predictor.GetRobot(team, robotID);
                } catch (ApplicationException e) {
                    Console.WriteLine("Failed Predictor.GetRobot(). Dumping exception\n" + e.ToString());                    
                    return true;
                }
				/*double wpDistanceSq = curinfo.Position.distanceSq(waypoints[waypointIndex]);
                if (wpDistanceSq > MIN_SQ_DIST_TO_WP) {
                    waypointIndex = (waypointIndex + 1);//Stop at end for now % waypoints.Count;
                    controller.move(robotID, false, waypoints[waypointIndex]);
                }*/
                
                
                // Lap around

                double sqDistToGoal = curinfo.Position.distanceSq(waypoints[waypointIndex]);
                double diffOrientation = Math.Abs(angleDifference(curinfo.Orientation, 0));

                if (sqDistToGoal < MIN_GOAL_DIST * MIN_GOAL_DIST && diffOrientation < MIN_GOAL_DIFF_ORIENTATION) {
                    if (waypointIndex == 0) {
                        if (!lapping) {
                            Console.WriteLine("Starting lap...");
                            lapping = true;
                            if (OnStartLap != null)
                                OnStartLap();
                        }
                        else {
                            Console.WriteLine("Ending lap...");
                            if (OnEndLap != null) {
                                OnEndLap(true, true);
                                lapping = false;
                            }

                        }
                    }
                    waypointIndex = (waypointIndex + 1) % waypoints.Count;
                }
			
				System.Threading.Thread.Sleep(_sleepTime);
            } while (running);

            return false;
        }

        public void Kick() 
        {
            running = true;

            ActionInterpreter actionInterpreter = new ActionInterpreter(team, controller, predictor);
			actionInterpreter.Kick(robotID, new Vector2(0, 0));
            do {

				actionInterpreter.Kick(robotID, new Vector2(0, 0));
                System.Threading.Thread.Sleep(_sleepTime);
            } while (running);
        }
        public void BeamKick() {
            running = true;
			ActionInterpreter actionInterpreter = new ActionInterpreter(team, controller, predictor);
			actionInterpreter.BeamKick(robotID, new Vector2(0, 0));
            do {

				actionInterpreter.BeamKick(robotID, new Vector2(0, 0));
                System.Threading.Thread.Sleep(_sleepTime);
            } while (running);
        }

		public void Stop()
		{
            if (OnEndLap != null)
                OnEndLap(false, false);

            controller.stop(robotID);
            running = false;
            lapping = false;
		}

        //seeks to reload any constants that this class and its own objects use from files
        //in particular, reloads PID and other constants for the planner
        public void reloadConstants() {
            // calls reloadConstants within planner
            planner.LoadConstants();
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
