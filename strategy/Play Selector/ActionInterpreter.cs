using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Geometry;
using Robocup.Core;

namespace Robocup.Plays
{
    public class ActionInterpreter : IActionInterpreter
    {
        //private const int MAXBOTS=10;
        //private bool[] active=new bool[MAXBOTS];
        //private bool[] shouldbeactive=new bool[MAXBOTS];

        private IController commander;
        private IPredictor predictor;
        public ActionInterpreter(IController commander, IPredictor predictor)
        {
            this.commander = commander;
            this.predictor = predictor;
        }

        /// <param name="kickDistance">Default: .12</param>
        /// <param name="ballLead">Default: 3</param>
        /// <param name="distTolerance">Default: .04</param>
        /// <param name="angleTolerance">Default: Math.PI/60</param>
        public ActionInterpreter(IController commander, IPredictor predictor, double kickDistance, double ballLead, double distTolerance, double angleTolerance)
        {
            this.commander = commander;
            this.predictor = predictor;
            this.kickDistance = kickDistance;
            this.ballLeading = ballLead;
            this.distanceTolerance = distTolerance;
            this.angleTolerance = angleTolerance;
        }
        private RobotInfo getOurRobotFromID(int robotID)
        {            
                return predictor.getCurrentInformation(robotID);
        }

        public void Dribble(int robotID, Vector2 target)
        {
            RobotInfo thisrobot;
            Vector2 ball;
            try {
                thisrobot = getOurRobotFromID(robotID);
                ball = predictor.getBallInfo().Position;
            } catch (ApplicationException e) {
                Console.WriteLine("Predictor failed to find Robot " + robotID.ToString() + " OR the ball.");
                return;
            }
            Vector2 robotposition = thisrobot.Position;
            double dotP = (target - ball).normalize() * (ball - robotposition).normalize();
            Vector2 destination = target;
            bool avoidBall = false;
            if (dotP > .9)  //~8 degrees
            {
                destination = extend(target, ball, -.14);
            }
            else
            {
                avoidBall = true;
                destination = extend(target, ball, kickDistance);
            }
            
            Move(robotID, avoidBall, destination, ball);
            //commander.move(robotID, avoidBall, destination);
        }

        /// <summary>
        /// Extends the line P1->P2 (from P2) by a distance [distance], and returns that point.
        /// </summary>
        private Vector2 extend(Vector2 p1, Vector2 p2, double distance)
        {
            return p2 + (p2 - p1).normalizeToLength(distance);
            /*double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double mag = Math.Sqrt(dx * dx + dy * dy);
            dx *= distance / mag;
            dy *= distance / mag;
            return new Vector2(p2.X + dx, p2.Y + dy);*/
        }
        /// <summary>
        /// Returns the angle that this robot will have to face to face at the target.
        /// </summary>
        private double targetAngle(Vector2 robot, Vector2 target)
        {
            return (target - robot).cartesianAngle();
        }
        /// <summary>
        /// This is the distance that the robots should put themselves from the ball,
        /// when they get ready to kick it.
        /// </summary>
        private readonly double kickDistance = .10;//.095

        /// <summary>
        /// This is how many ticks of ball motion you should add to the distance to lead the ball appropriately
        /// </summary>
        private readonly double ballLeading = 0;//3.0;

        public void Stop(int robotID)
        {
            commander.stop(robotID);
        }

        /// <summary>
        /// This method does all the necessary work to get a robot to kick the ball to a certain point.
        /// This should make other functions easier, such as a moving pass.
        /// </summary>
        public void Kick(int robotID, Vector2 target)
        {
            RobotInfo thisrobot;
            Vector2 ball;
            BallInfo ballinfo;
            try {
                thisrobot = getOurRobotFromID(robotID);
                ballinfo = predictor.getBallInfo();
                ball = ballinfo.Position;
            }
            catch (ApplicationException e) {
                Console.WriteLine("Predictor failed to find Robot " + robotID.ToString() + " OR the ball.");
                return;
            }

            //JUST FOR TEMPORARY TESTING!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //ball = new Vector2(1.0, 0);



            //Console.WriteLine("Ball's position: "+ball.ToString());
            /*if (ball.X > 2.2 || ball.X < .4 || ball.Y > 1.4 || ball.Y < -1.5)
                Console.WriteLine("Terrible ball location!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");*/
            //double dx = ballinfo.Position.X - target.X;
            //double dy = ballinfo.Position.Y - target.Y;
            Vector2 destination = extend(target, ball, kickDistance);
            /*Console.WriteLine("destination: " + destination.ToString());
            if (destination.X > 2.2 || destination.X < .4 || destination.Y > 1.4 || destination.Y < -1.5) {
                Console.WriteLine("Terrible destination!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            }*/

            double destinationAngle = targetAngle(ball, target);
            //Console.WriteLine("Distance from robot to ball: " + Math.Sqrt(ball.distanceSq(thisrobot.Position)));
            //Console.WriteLine("angle Difference: " + UsefulFunctions.angleDifference(destinationAngle, thisrobot.Orientation));
            if (closeEnough(thisrobot, destination.X, destination.Y, destinationAngle))
            {
                Console.WriteLine("Going to try and Kick!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                commander.kick(robotID);
                /*commander.move(
                    robotID,
                    true,
                    new Vector2(destination.X, destination.Y),
                    destinationAngle);*/
            }
            else if (thisrobot.Position.distanceSq(destination) < 4*kickDistance*kickDistance) {
                //we're close to the ball but not quite there yet
                commander.move(
                    robotID,
                    true,
                    new Vector2(destination.X, destination.Y),
                    destinationAngle);

            }else//we're kindof far away from the ball
                {
                //destination += ballLeading * ballinfo.Velocity;
                commander.move(
                    robotID,
                    true,
                    new Vector2(destination.X, destination.Y),
                    destinationAngle);
            }
        }

        /// <summary>
        /// Has the robot move to the point target, avoiding all obstacles (including the ball)
        /// </summary>
        public void Move(int robotID, Vector2 target)
        {
            try {
                if (getOurRobotFromID(robotID).Position.distanceSq(target) < .01 * .01)
                    commander.stop(robotID);
                else
                    commander.move(robotID, true, target);
            }
            catch (ApplicationException e) {
                Console.WriteLine("Predictor failed to find Robot " + robotID.ToString() + " OR the ball.");
                return;
            }
        }
        /// <summary>
        /// Has the robot move to the point target, avoiding all obstacles (including the ball)
        /// </summary>
        public void Move(int robotID, Vector2 target, Vector2 facing)
        {
            Move(robotID, true, target, facing);
        }

        /// <summary>
        /// Has the robot move to the point target, avoiding all obstacles. takes avoidBall as second argument
        /// </summary>
        private void Move(int robotID, bool avoidBall, Vector2 target, Vector2 facing)
        {
            double orient = Math.Atan2(facing.Y - target.Y, facing.X - target.X); //hack for different coordinates
            try {
                if (getOurRobotFromID(robotID).Position.distanceSq(target) < .01 * .01 &&
                    Math.Abs(getOurRobotFromID(robotID).Orientation - orient) < 0.15)
                    commander.stop(robotID);
                else
                    commander.move(robotID, avoidBall, target, orient);
            }
            catch (ApplicationException e) {
                Console.WriteLine("Predictor failed to find Robot " + robotID.ToString() + " OR the ball.");
                return;
            }
            //HACK: commander.move(robotID, true, target, Math.Atan2(facing.Y - target.Y, facing.X - target.X));
        }

        private readonly double angleTolerance = Math.PI / 20;// 2 degrees | 120;  //1.5º
        //private const double angleTolerance = (Math.PI);  //180º
        private readonly double distanceTolerance = .05;  //4d cm
        
        /// <summary>
        /// Returns if this robot is close enough to the desired position and orientation
        /// (such as to decide whether or not the robot is in position to kick the ball)
        /// </summary>
        /// 
        private bool closeEnough(RobotInfo robot, double x, double y, double orientation)
        {
            double anglediff = UsefulFunctions.angleDifference(orientation, robot.Orientation);
            double dist = UsefulFunctions.distance(new Vector2(x, y), robot.Position);
            return (Math.Abs(anglediff) <= angleTolerance) && ((dist <= distanceTolerance)||dist<=kickDistance);
        }
    }



    public class ActionInfo
    {
        //private int[] robots;
        public int[] RobotsInvolved
        {
            //get { return robots; }
            get { return Definition.RobotsInvolved; }
        }
        private ActionDefinition definition;
        public ActionDefinition Definition
        {
            get { return definition; }
        }
        private InterpreterPlay play;
        public InterpreterPlay Play
        {
            get { return play; }
        }

        public ActionInfo(ActionDefinition definition, InterpreterPlay play)//,params int[] robotsinvolved)
        {
            this.definition = definition;
            //this.robots = robotsinvolved;
            this.play = play;
        }
    }
}
