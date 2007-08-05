using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using Robocup.Infrastructure;
using RobocupPlays;



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
            Dictionary<int, IMovement> planners,
            INavigator navigator,
            IPredictor predictor)
        {
            _commander = commander;
            _planners = planners;
            _navigator = navigator;
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

        private Dictionary<int, IMovement> _planners;
        public IMovement GetPlanner(int robotId)
        {
            if (!_planners.ContainsKey(robotId))
                throw new ApplicationException("trying to move with a robot that doesn't have an IMovement defined");
            return _planners[robotId];
        }

        private INavigator _navigator;
        public INavigator Navigator
        {
            get { return _navigator; }
            set { _navigator = value; }
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

        float ballAvoidDist = .02f;

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
        public void move(int robotID, bool avoidBall, Vector2 destination, float orientation)
        {
            if(float.IsNaN(destination.X) || float.IsNaN(destination.Y))
            {
                Console.WriteLine("invalid destination");
                return;
            }

            RobotInfo thisRobot = Predictor.getCurrentInformation(robotID);

            float avoidBallDist = (avoidBall ? ballAvoidDist : 0f);
            Vector2 newDest =
                Navigator.navigate(robotID,
                    thisRobot.Position,
                    destination,
                    Predictor.getOurTeamInfo().ToArray(),
                    Predictor.getTheirTeamInfo().ToArray(),
                    Predictor.getBallInfo(),
                    avoidBallDist);

            lock (arrows)
            {
                arrows[robotID] = new Arrow[] {
                    new Arrow(thisRobot.Position, destination, Color.Red, .04f),
                    new Arrow(thisRobot.Position, newDest, Color.Green,.04f)
                };
            }

            WheelSpeeds motorSpeeds = GetPlanner(robotID).calculateWheelSpeeds(robotID, thisRobot, newDest);

            Commander.setMotorSpeeds(robotID, motorSpeeds);
        }

        public void move(int robotID, bool avoidBall, Vector2 destination)
        {
            move(robotID, avoidBall, destination, Predictor.getCurrentInformation(robotID).Orientation); //TODO make it the current robot position
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
        }

    }
}
