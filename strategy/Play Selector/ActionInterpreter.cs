using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Infrastructure;
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
        public ActionInterpreter(IController commander, IPredictor predictor, float kickDistance, float ballLead, float distTolerance, float angleTolerance)
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
            RobotInfo thisrobot = getOurRobotFromID(robotID);
            Vector2 ball = predictor.getBallInfo().Position;
            Vector2 robotposition = thisrobot.Position;
            float dotP = (target - ball).normalize() * (ball - robotposition).normalize();
            Vector2 destination = target;
            bool avoidBall = false;
            if (dotP > .9)  //~8 degrees
            {
                destination = extend(target, ball, -.14f);
            }
            else
            {
                avoidBall = true;
                destination = extend(target, ball, kickDistance);
            }
            //Move(robotID, target);
            commander.move(robotID, avoidBall, destination);
        }

        /// <summary>
        /// Extends the line P1->P2 (from P2) by a distance [distance], and returns that point.
        /// </summary>
        private Vector2 extend(Vector2 p1, Vector2 p2, float distance)
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            float mag = (float)Math.Sqrt(dx * dx + dy * dy);
            dx *= distance / mag;
            dy *= distance / mag;
            return new Vector2(p2.X + dx, p2.Y + dy);
        }
        /// <summary>
        /// Returns the angle that this robot will have to face to face at the target.
        /// </summary>
        private float targetAngle(Vector2 robot, Vector2 target)
        {
            return (float)Math.Atan2(target.Y - robot.Y, target.X - robot.X);
        }
        /// <summary>
        /// This is the distance that the robots should put themselves from the ball,
        /// when they get ready to kick it.
        /// </summary>
        private readonly float kickDistance = .11f;//.095f

        /// <summary>
        /// This is how many ticks of ball motion you should add to the distance to lead the ball appropriately
        /// </summary>
        private readonly float ballLeading = 3.0f;

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
            RobotInfo thisrobot = getOurRobotFromID(robotID);
            BallInfo ballinfo = predictor.getBallInfo();
            Vector2 ball = predictor.getBallInfo().Position;
            //float dx = ballinfo.Position.X - target.X;
            //float dy = ballinfo.Position.Y - target.Y;
            Vector2 destination = extend(target, ball, kickDistance);
            float destinationAngle = targetAngle(ball, target);
            if (closeEnough(thisrobot, destination.X, destination.Y, destinationAngle))
            {
                commander.kick(robotID);
            }
            else
            {
                destination += ballLeading * ballinfo.Velocity;
                commander.move(
                    robotID,
                    true,
                    new Vector2(destination.X, destination.Y),
                    destinationAngle
                );
            }
        }

        /// <summary>
        /// Has the robot move to the point target, avoiding all obstacles (including the ball)
        /// </summary>
        public void Move(int robotID, Vector2 target)
        {
            if (getOurRobotFromID(robotID).Position.distanceSq(target) < .01f * .01f)
                commander.stop(robotID);
            else
                commander.move(robotID, true, target);
        }
        /// <summary>
        /// Has the robot move to the point target, avoiding all obstacles (including the ball)
        /// </summary>
        public void Move(int robotID, Vector2 target, Vector2 facing)
        {
            if (getOurRobotFromID(robotID).Position.distanceSq(target) < .01f * .01f)
                commander.stop(robotID);
            else
                commander.move(robotID, true, target, (float)Math.Atan2(facing.Y - target.Y, facing.X - target.X));
        }
        private readonly float angleTolerance = (float)(Math.PI / 10);  //18º
        //private const float angleTolerance = (float)(Math.PI);  //180º
        private readonly float distanceTolerance = .04f;  //4d cm
        /// <summary>
        /// Returns if this robot is close enough to the desired position and orientation
        /// (such as to decide whether or not the robot is in position to kick the ball)
        /// </summary>
        /// 
        private bool closeEnough(RobotInfo robot, float x, float y, float orientation)
        {
            double anglediff = UsefulFunctions.angleDifference(orientation, robot.Orientation);
            float dist = UsefulFunctions.distance(new Vector2(x, y), robot.Position);
            return (Math.Abs(anglediff) <= angleTolerance) && (dist <= distanceTolerance);
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
