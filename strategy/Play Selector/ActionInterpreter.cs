using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Geometry;
using Robocup.Core;
using Robocup.CoreRobotics;
using Robocup.MotionControl;

namespace Robocup.Plays
{
    public class ActionInterpreter : IActionInterpreter
    {
        //private const int MAXBOTS=10;
        //private bool[] active=new bool[MAXBOTS];
        //private bool[] shouldbeactive=new bool[MAXBOTS];

        private double KICK_POSITION_DIST; // m
        private double CHARGE_DIST; // m
        private double MAX_LATERAL_DIST; // m        
        private double MAX_DIST_TO_KICK_POSITION; // m 
        private double MAX_ANGLE_TO_KICK_AXIS; // rad
        private double KICK_ORIENTATION_ERROR; // rad        

        private IController commander;
        private IPredictor predictor;
        private Team team;

        public ActionInterpreter(Team team, IController commander, IPredictor predictor)
            : this(team, commander, predictor, 0.0, 0.0, 0.0, 0.0) { }

        /// <param name="kickDistance">Default: .12</param>
        /// <param name="ballLead">Default: 3</param>
        /// <param name="distTolerance">Default: .04</param>
        /// <param name="angleTolerance">Default: Math.PI/60</param>
        public ActionInterpreter(Team team, IController commander, IPredictor predictor, double kickDistance, double ballLead,
                                 double distTolerance, double angleTolerance)
        {
            this.team = team;
            this.commander = commander;
            this.predictor = predictor;
            this.kickDistance = kickDistance;
            this.ballLeading = ballLead;
            this.distanceTolerance = distTolerance;
            this.angleTolerance = angleTolerance;

            LoadConstants();
        }

        public void LoadConstants()
        {
            KICK_POSITION_DIST = Constants.get<double>("kickplanning", "KICK_POSITION_DIST");
            MAX_LATERAL_DIST = Constants.get<double>("kickplanning", "MAX_LATERAL_DIST");
                        
            // must be bigger than distance that defines the kicking position (KICK_POSITION_DIST),
            // to avoid going back to the kicking position after we started to approach the ball for
            // kicking
            MAX_DIST_TO_KICK_POSITION = 2 * KICK_POSITION_DIST;

            CHARGE_DIST = Constants.get<double>("kickplanning", "CHARGE_DIST");

            MAX_ANGLE_TO_KICK_AXIS = Math.PI / 180 * Constants.get<double>("kickplanning", "MAX_ANGLE_TO_KICK_AXIS");
            KICK_ORIENTATION_ERROR = Math.PI / 180 * Constants.get<double>("kickplanning", "KICK_ORIENTATION_ERROR");
        }

        private RobotInfo getOurRobotFromID(int robotID)
        {
            return predictor.GetRobot(team, robotID);
        }

        public void Dribble(int robotID, Vector2 target)
        {
            RobotInfo thisrobot;
            Vector2 ball;
            try
            {
                thisrobot = getOurRobotFromID(robotID);
                ball = predictor.GetBall().Position;
            }
            catch (ApplicationException)
            {
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
        private readonly double kickDistance = .085;//.095;

        /// <summary>
        /// This is how many ticks of ball motion you should add to the distance to lead the ball appropriately
        /// </summary>
        private readonly double ballLeading = 0;//3.0;

        public void Stop(int robotID)
        {
            commander.Stop(robotID);
        }

        public void Charge(int robotID)
        {
            commander.Charge(robotID);
        }
        public void Charge(int robotID, int strength)
        {
            commander.Charge(robotID, strength);
        }

        private readonly double bumpDistance = 0.2;
        private readonly double bumpOrientationOffset = Math.PI / 6;
        /// <summary>
        /// This is a fail-safe replacement of kicking - just go to the ball and bump it hard with the side of the robot
        /// </summary>
        public void Bump(int robotID, Vector2 target)
        {

            RobotInfo thisrobot;
            Vector2 ball;
            BallInfo ballinfo;
            try
            {
                thisrobot = getOurRobotFromID(robotID);
                ballinfo = predictor.GetBall();
                ball = ballinfo.Position;
            }
            catch (ApplicationException e)
            {
                Console.WriteLine("Predictor failed to find Robot " + robotID.ToString() + " OR the ball.");
                return;
            }

            Vector2 destination = extend(target, ball, bumpDistance);
            double destinationAngle = targetAngle(ball, target) + bumpOrientationOffset;

            Vector2 destinationBehind = extend(target, ball, -bumpDistance);

            Vector2 robotToBall = ball - thisrobot.Position;
            Vector2 robotToDest = destination - thisrobot.Position;

            double angleDiff = Math.Abs(UsefulFunctions.angleDifference(robotToBall.cartesianAngle(), robotToDest.cartesianAngle()));
            bool nearLine = thisrobot.Position.distanceSq(destination) <= bumpDistance * bumpDistance && angleDiff <= 3 * angleTolerance;
            bool tooClose = thisrobot.Position.distanceSq(destination) <= 0.01 * bumpDistance * bumpDistance && angleDiff <= Math.PI / 2;

            if (nearLine || tooClose)
            {
                commander.Move(robotID,
                        false,
                        destinationBehind,
                        destinationAngle);
            }
            else
            {
                commander.Move(robotID,
                               true,
                               destination,
                               destinationAngle);
            }

            return;
        }


        /// <summary>
        /// This method does all the necessary work to get a robot to kick the ball to a certain point.
        /// This should make other functions easier, such as a moving pass.
        /// </summary>
        public void Kick(int robotID, Vector2 target, int strength)
        {
            //DEBUG!

            BeamKick(robotID, target, strength);
            return;
#if FALSE
            RobotInfo thisrobot;
            Vector2 ball;
            BallInfo ballinfo;
            try
            {
                thisrobot = getOurRobotFromID(robotID);
                ballinfo = predictor.GetBall();
                ball = ballinfo.Position;
            }
            catch (ApplicationException)
            {
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
                Console.WriteLine("Distance from robot to ball:{0}", Math.Sqrt(thisrobot.Position.distanceSq(destination)));
                Console.WriteLine("Going to try and Kick!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                commander.Kick(robotID, target);
                /*commander.move(
                    robotID,
                    true,
                    new Vector2(destination.X, destination.Y),
                    destinationAngle);*/
            }
            else if (thisrobot.Position.distanceSq(destination) < 4 * kickDistance * kickDistance)
            {
                //we're close to the ball but not quite there yet
                commander.Move(
                    robotID,
                    true,
                    new Vector2(destination.X, destination.Y),
                    destinationAngle);

            }
            else//we're kindof far away from the ball
            {
                //destination += ballLeading * ballinfo.Velocity;
                commander.Move(
                    robotID,
                    true,
                    new Vector2(destination.X, destination.Y),
                    destinationAngle);
            }
#endif
        }
        /// <summary>
        /// Overloaded version of the above (with default kick stregth)
        /// </summary>
        public void Kick(int robotID, Vector2 target)
        {
            BeamKick(robotID, target);
        }

        /// <summary>
        /// This pretty much replicates what Kick() does on the higher level. The robot kicks the ball to a certain point.
        /// The main difference is that closeness to the ball is determined by a break-beam sensor on the robot itself and 
        /// that is, hopefully, much more accurate than vision distances.
        /// </summary>
        public void BeamKick(int robotID, Vector2 target, int strength = RobotCommand.MAX_KICKER_STRENGTH)
        {
            RobotInfo thisrobot;
            Vector2 ball;
            BallInfo ballinfo;
            try
            {
                thisrobot = getOurRobotFromID(robotID);
                ballinfo = predictor.GetBall();
                // without this if statement, throws an exception when the ball is not found 
                // (not sure why this isn't caught in the next line)
                if (ballinfo != null)
                {
                    ball = ballinfo.Position;
                }
                else
                {
                    return;
                }
            }
            catch (ApplicationException)
            {
                Console.WriteLine("Predictor failed to find Robot " + robotID.ToString() + " OR the ball.");
                return;
            }            

            Vector2 kickPosition = extend(target, ball, KICK_POSITION_DIST); // kicking position
            double kickOrientation = targetAngle(ball, target); // orientation in kicking position

            // a waypoint between ball and target direction -- you move here once break beam is on 
            // to actually kick the ball
            Vector2 waypoint = ball + (target - ball).normalizeToLength(KICK_POSITION_DIST); // distance here doesn't matter much

            Vector2 ballToTarget = (target - ball);
            Vector2 robotToBall = (ball - thisrobot.Position);
            Vector2 robotToTarget = (target - thisrobot.Position);
            double distRobotToBall = Math.Sqrt(thisrobot.Position.distanceSq(ball));

            double distToKickPosition = Math.Sqrt(thisrobot.Position.distanceSq(kickPosition));            
            
            double theta = Math.Abs(UsefulFunctions.angleDifference(robotToBall.cartesianAngle(), ballToTarget.cartesianAngle()));            
            double lateralDistance = distRobotToBall * Math.Sin(theta);

            double angleToKickAxis = Math.Abs(UsefulFunctions.angleDifference(robotToTarget.cartesianAngle(),
                                                                              ballToTarget.cartesianAngle()));            

            // should we print all these annoying little details about kicking
            bool VERBOSE = false;

            if (thisrobot.Position.distanceSq(kickPosition) < CHARGE_DIST * CHARGE_DIST)
            {
                if (VERBOSE)
                    Console.WriteLine("Close to the ball. CHARGING!");

                commander.Charge(robotID, strength);
            }

            if (VERBOSE)
            {
                Console.WriteLine("Dist to KickPosition: {0:F3}", distToKickPosition);
                Console.WriteLine("Orientation error: {0:F3}",
                        Math.Abs(UsefulFunctions.angleDifference(thisrobot.Orientation,
                                                             kickOrientation)) * 180 / Math.PI);
                Console.WriteLine("AngleToKickAxis: {0:F3}", angleToKickAxis);
                Console.WriteLine("Lateral Distance: {0:F3}", lateralDistance);
                Console.WriteLine("robotToTarget {0:F3} > ballToTarget {1:F3} [{2:G}]",
                        robotToTarget.magnitudeSq(), ballToTarget.magnitudeSq(),
                        robotToTarget.magnitudeSq() > ballToTarget.magnitudeSq());
            }

            if ( // is the robot in the position to kick

                // close enough to kicking position
                distToKickPosition < MAX_DIST_TO_KICK_POSITION && 

                // robot oriented towards the target
                Math.Abs(UsefulFunctions.angleDifference(thisrobot.Orientation, 
                                                         kickOrientation)) < KICK_ORIENTATION_ERROR &&
                
                // robot is on the opposite side of the ball from the target
                angleToKickAxis < MAX_ANGLE_TO_KICK_AXIS &&
                
                // robot is outside of circle defined by target to ball vector                
                robotToTarget.magnitudeSq() > ballToTarget.magnitudeSq() &&

                // robot is collinear with ball and with target
                lateralDistance < MAX_LATERAL_DIST 

                ) 
            {
                if (VERBOSE)
                    Console.WriteLine("Turning on break beam and moving towards the ball to kick.");
                commander.BreakBeam(robotID, strength);
                //commander.StartDribbling(robotID);
                commander.Move(
                        robotID,
                        false, // don't avoid the ball, we are kicking it
                        waypoint,
                        kickOrientation);
            }
            else
            {
                if (VERBOSE)
                    Console.WriteLine("Not in kicking position for one of the above reasons. " +
                                      "Moving to kicking position.");
                //if we are too far

                //position the robot close enough to the ball so that it can kick soon,
                //but far enough so that the break-beam doesn't trigger
                commander.Move(
                    robotID,
                    true,
                    kickPosition,
                    kickOrientation);
            }
        }

        /// <summary>
        /// Has the robot move to the point target, avoiding all obstacles (including the ball)
        /// </summary>
        public void Move(int robotID, Vector2 target)
        {
            try
            {
                if (getOurRobotFromID(robotID).Position.distanceSq(target) < .01 * .01)
                    commander.Stop(robotID);
                else
                    commander.Move(robotID, true, target);
            }
            catch (ApplicationException)
            {
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
        public void Move(int robotID, bool avoidBall, Vector2 target, Vector2 facing)
        {
            double orient = Math.Atan2(facing.Y - target.Y, facing.X - target.X); //hack for different coordinates
            try
            {
                if (getOurRobotFromID(robotID).Position.distanceSq(target) < .01 * .01 &&
                    Math.Abs(getOurRobotFromID(robotID).Orientation - orient) < 0.15)
                    commander.Stop(robotID);
                else
                    commander.Move(robotID, avoidBall, target, orient);
            }
            catch (ApplicationException)
            {
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
