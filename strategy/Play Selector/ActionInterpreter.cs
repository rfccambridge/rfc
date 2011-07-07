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
        private double MAX_LATERAL_SPEED; //m/s
        private double MAX_ANGULAR_SPEED; //rad/s

        private double BUMP_ANGLE_TOLERANCE;  //rad
        private double BUMP_DIST_TOLERANCE;   //m

        private double DRIBBLE_DIST; //m

        private IController commander;
        private IPredictor predictor;
        private Team team;

        public ActionInterpreter(Team team, IController commander, IPredictor predictor)
        {
            this.team = team;
            this.commander = commander;
            this.predictor = predictor;

            LoadConstants();
        }

        public void LoadConstants()
        {
            KICK_POSITION_DIST = ConstantsRaw.get<double>("kickplanning", "AIKICK_POSITION_DIST");
            MAX_LATERAL_DIST = ConstantsRaw.get<double>("kickplanning", "AIKICK_MAX_LATERAL_DIST");
                        
            // must be bigger than distance that defines the kicking position (KICK_POSITION_DIST),
            // to avoid going back to the kicking position after we started to approach the ball for
            // kicking
            MAX_DIST_TO_KICK_POSITION = 2 * KICK_POSITION_DIST;

            CHARGE_DIST = ConstantsRaw.get<double>("kickplanning", "CHARGE_DIST");

            MAX_ANGLE_TO_KICK_AXIS = Math.PI / 180 * ConstantsRaw.get<double>("kickplanning", "AIKICK_MAX_ANGLE_TO_KICK_AXIS");
            KICK_ORIENTATION_ERROR = Math.PI / 180 * ConstantsRaw.get<double>("kickplanning", "AIKICK_ORIENTATION_ERROR");
            MAX_LATERAL_SPEED = ConstantsRaw.get<double>("kickplanning", "AIKICK_MAX_LATERAL_SPEED");
            MAX_ANGULAR_SPEED = ConstantsRaw.get<double>("kickplanning", "AIKICK_MAX_ANGULAR_SPEED");

            BUMP_ANGLE_TOLERANCE = Math.PI / 180 * ConstantsRaw.get<double>("kickplanning", "AIBUMP_ANGLE_TOLERANCE");
            BUMP_DIST_TOLERANCE = ConstantsRaw.get<double>("kickplanning", "AIBUMP_DIST_TOLERANCE");

            DRIBBLE_DIST = ConstantsRaw.get<double>("kickplanning", "AIDRIBBLE_DIST");
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
                destination = extend(target, ball, DRIBBLE_DIST);
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
        }
        /// <summary>
        /// Returns the angle that this robot will have to face to face at the target.
        /// </summary>
        private double targetAngle(Vector2 robot, Vector2 target)
        {
            return (target - robot).cartesianAngle();
        }

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
            catch (ApplicationException)
            {
                Console.WriteLine("Predictor failed to find Robot " + robotID.ToString() + " OR the ball.");
                return;
            }

            Vector2 destination = extend(target, ball, BUMP_DIST_TOLERANCE);
            double destinationAngle = targetAngle(ball, target);

            Vector2 destinationBehind = extend(target, ball, -BUMP_DIST_TOLERANCE);

            Vector2 robotToBall = ball - thisrobot.Position;
            Vector2 robotToTarget = target - thisrobot.Position;

            double angleDiff = Math.Abs(Angle.AngleDifference(robotToBall.cartesianAngle(), robotToTarget.cartesianAngle()));
            bool nearLine = thisrobot.Position.distanceSq(destination) <= BUMP_DIST_TOLERANCE * BUMP_DIST_TOLERANCE 
                && angleDiff <= BUMP_ANGLE_TOLERANCE;

            if (nearLine)
                commander.Move(robotID,false,destinationBehind,destinationAngle);
            else
                commander.Move(robotID,true,destination,destinationAngle);

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

            Vector2 destination = extend(target, ball, kickDistance);

            double destinationAngle = targetAngle(ball, target);

            if (closeEnough(thisrobot, destination.X, destination.Y, destinationAngle))
            {
                Console.WriteLine("Distance from robot to ball:{0}", Math.Sqrt(thisrobot.Position.distanceSq(destination)));
                Console.WriteLine("Going to try and Kick!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                commander.Kick(robotID, target);
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
        public void BeamKick(int robotID, Vector2 target, int strength = 10)
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
            double distToKickPosition = thisrobot.Position.distance(kickPosition);            
            double lateralDistance = robotToBall.perpendicularComponent(ballToTarget).magnitude();
            double angleToKickAxis = Math.Abs(Angle.AngleDifference(robotToTarget.cartesianAngle(),
                                                                              ballToTarget.cartesianAngle()));

            double lateralSpeed = thisrobot.Velocity.perpendicularComponent(ballToTarget).magnitude();

            // should we print all these annoying little details about kicking
            bool VERBOSE = false;

            if (distToKickPosition < CHARGE_DIST)
            {
                if (VERBOSE)
                    Console.WriteLine("Close to the ball. CHARGING!");

                commander.Charge(robotID);
            }

            if (VERBOSE)
            {
                Console.WriteLine("Dist to KickPosition: {0:F3}", distToKickPosition);
                Console.WriteLine("Orientation error: {0:F3}",
                        Math.Abs(Angle.AngleDifference(thisrobot.Orientation,
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
                Math.Abs(Angle.AngleDifference(thisrobot.Orientation, 
                                                         kickOrientation)) < KICK_ORIENTATION_ERROR &&
                
                // robot is on the opposite side of the ball from the target
                angleToKickAxis < MAX_ANGLE_TO_KICK_AXIS &&
                
                // robot is outside of circle defined by target to ball vector                
                robotToTarget.magnitudeSq() > ballToTarget.magnitudeSq() &&

                // robot is collinear with ball and with target
                lateralDistance < MAX_LATERAL_DIST &&

                // robot not moving too quickly laterally and not rotating too fast
                lateralSpeed <= MAX_LATERAL_SPEED &&

                thisrobot.AngularVelocity <= MAX_ANGULAR_SPEED

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
                        kickOrientation,
                        0.5);
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
    }



    public class ActionInfo
    {
        public int[] RobotsInvolved
        {
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

        public ActionInfo(ActionDefinition definition, InterpreterPlay play)
        {
            this.definition = definition;
            this.play = play;
        }
    }
}
