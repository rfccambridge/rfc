using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using Robocup.Utilities;

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
        public RFCController(
            IRobots commander,
            IMotionPlanner planner,
            IPredictor predictor)
        {
            _commander = commander;
            _planner = planner;
            _predictor = predictor;
        }

        private Dictionary<int, Arrow[]> arrows = new Dictionary<int, Arrow[]>();

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

        public void clearArrows()
        {
            lock (arrows)
            {
                arrows.Clear();
            }
        }
        #region IController Members

        public void kick(int robotID)
        {
            Commander.kick(robotID);
        }
        public void move(int robotID, bool avoidBall, Vector2 destination, double orientation)
        {
            if(double.IsNaN(destination.X) || double.IsNaN(destination.Y))
            {
                Console.WriteLine("invalid destination");
                return;
            }

            RobotInfo thisRobot = Predictor.getCurrentInformation(robotID);

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
            MotionPlanningResults mpResults;
            try {
                 mpResults = Planner.PlanMotion(robotID, new RobotInfo(destination, orientation, robotID),
                    Predictor, avoidBallDist);
            } catch (ApplicationException e) {
                Console.WriteLine("PlanMotion failed. Dumping exception:\n" + e.ToString());
                return;
            }

            WheelSpeeds wheelSpeeds = mpResults.wheel_speeds;

            lock (arrows)
            {
                arrows[robotID] = new Arrow[] {
                    new Arrow(thisRobot.Position, destination, Color.Red, .04),
                    new Arrow(thisRobot.Position, mpResults.NearestWaypoint.Position, Color.Green,.04)
                };                
            }


            Commander.setMotorSpeeds(robotID, wheelSpeeds);
        }

        public void move(int robotID, bool avoidBall, Vector2 destination)

        {
            double orientation;
            try {
                orientation = Predictor.getCurrentInformation(robotID).Orientation;
            } catch (ApplicationException e) {
                Console.WriteLine("inside move: Predictor.getCurrentInformation() failed. Dumping exception:\n" + e.ToString());
                return;
            }
            move(robotID, avoidBall, destination, orientation); //TODO make it the current robot position
        }

        public void stop(int robotID)
        {
            Commander.setMotorSpeeds(robotID, new WheelSpeeds());
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
        public void drawCurrent(System.Drawing.Graphics g, ICoordinateConverter converter)
        {
            lock (arrows)
            {
                foreach (KeyValuePair<int, Arrow[]> pair in arrows)
                {
                    foreach (Arrow arrow in pair.Value)
                    {
                        arrow.drawConvertToPixels(g, converter);
                    }
                }
            }            
            Planner.DrawLast(g, converter);
        }

    }
}