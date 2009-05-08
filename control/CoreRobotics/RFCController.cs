using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
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
        TangentBugFeedbackMotionPlanner regularPlanner;
        FeedbackVeerKickPlanner _kickPlanner;

        public RFCController(
            IRobots commander,
            IMotionPlanner planner,
            IPredictor predictor)
        {
            _commander = commander;
            _planner = planner;
            _predictor = predictor;

            regularPlanner = new TangentBugFeedbackMotionPlanner();
            _kickPlanner = new FeedbackVeerKickPlanner(regularPlanner);
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

        public void charge(int robotID) {
            Commander.charge(robotID);
        }
        public void kick(int robotID)
        {
            Commander.kick(robotID);
        }
        public void beamKick(int robotID, bool goForward) 
        {
            Commander.beamKick(robotID);

            if(goForward){
                WheelSpeeds speeds = new WheelSpeeds(10,17,17,30);
                Commander.setMotorSpeeds(robotID, speeds);
                System.Threading.Thread.Sleep(1500);
            }
        }
        public void move(int robotID, bool avoidBall, Vector2 destination, double orientation)
        {
            //Console.WriteLine("Destination in RFCController immediately: " + destination.ToString());
            if(double.IsNaN(destination.X) || double.IsNaN(destination.Y))
            {
                Console.WriteLine("invalid destination");
                return;
            }

            RobotInfo thisRobot;

            try {
                thisRobot = Predictor.getCurrentInformation(robotID);
            }
            catch (ApplicationException) {
                Console.WriteLine("Predictor did not find Robot " + robotID.ToString());
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
            MotionPlanningResults mpResults;

            //Console.WriteLine("Destination in RFCController later: " + destination.ToString());

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
                // TODO: Need to make NearestWaypoint populated inside each implementation of IMotionPlanner
                if (mpResults.NearestWaypoint != null) {
                    arrows[robotID] = new Arrow[] {
                        new Arrow(thisRobot.Position, destination, Color.Red, .04),                    
                        new Arrow(thisRobot.Position, mpResults.NearestWaypoint.Position, Color.Yellow, .04) };
                } else {
                    arrows[robotID] = new Arrow[] {
                        new Arrow(thisRobot.Position, destination, Color.Red, .04) };
                }
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

        /// <summary>
        /// Move to the ball and then kick it. Uses IKickPlanner- see David Robinson or Josh Montana
        /// with any questions
        /// </summary>
        /// <param name="robotID"></param>
        /// <param name="target"></param>
        public void moveKick(int robotID, Vector2 target)
        {
            // Plan kick
            KickPlanningResults kpResults = _kickPlanner.kick(robotID, target, Predictor);

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

        public void LoadConstants()
        {
            _planner.LoadConstants();
            _kickPlanner.LoadConstants();
            //_predictor
        }
    }
}