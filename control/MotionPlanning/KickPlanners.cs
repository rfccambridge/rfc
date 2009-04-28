using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Core;
using Robocup.CoreRobotics;
using Robocup.Geometry;

using System.Drawing;

using Navigation.Examples;
using System.IO;
using Robocup.Utilities;

namespace Robocup.MotionControl
{
    /// <summary>
    /// Head towards a point behind the ball (relative to the target), once you are there,
    /// use a very slow version of FeedbackVeerDriver to move forward to hit the ball
    /// </summary>
    public class FeedbackVeerKickPlanner : IKickPlanner
    {
        // Class variables
        bool goingToPoint1 = true;

        IMotionPlanner regularPlanner;
        DefaultMotionPlanner slowPlanner = new DefaultMotionPlanner();

        // Constants
        double DIST_BEHIND_BALL;
        double DIST_THROUGH_BALL;

        double TIME_WAIT_POINT_1;

        double MAX_LATERAL_GO_THROUGH;
        double MAX_PARALLEL_GO_THROUGH;

        double MAX_DIST_BREAK_BEAM;

        double DIST_ACTIVATE_KICK_PLANNER;

        double MAX_DIST_POINT_1;
        double MAX_DIFF_ORIENTATION_POINT_1;

        int SPIN_SPEED_CAP;

        WheelSpeeds CWSpeeds;
        WheelSpeeds CCWSpeeds;

        int WHEEL_SPEED_TURN;

        int NUM_ROBOTS = 5; // NOT SURE WHAT CONSTANTS FILE...

        DateTime arbitraryPastTime = new DateTime(2000, 1, 1);

        private DateTime[] _timesStartedCharging = new DateTime[5]; //number of robots

        PIDLoop loop;

        public FeedbackVeerKickPlanner(IMotionPlanner regularPlanner)
        {
            this.regularPlanner = regularPlanner;
            loop = new PIDLoop("kickplanning", "point1orientation");

            for (int robotID = 0; robotID < NUM_ROBOTS; robotID++)
            {
                //Set to arbitrary time in past- January 1, 2000
                _timesStartedCharging[robotID] = arbitraryPastTime;
            }

            ReloadConstants();
        }

        public void LoadConstants() {
            ReloadConstants();
        }

        public KickPlanningResults kick(int id, Vector2 target, IPredictor predictor) {
            // default break beam not on
            bool breakBeamOn = false;

            // find characteristics of field
            RobotInfo thisrobot = predictor.getCurrentInformation(id);
            BallInfo ballinfo = predictor.getBallInfo();
            Vector2 ball = ballinfo.Position;
            double distRobotToBall = Math.Sqrt(ball.distanceSq(thisrobot.Position));

            Vector2 ballToTarget = target - ball;
            Vector2 targetToBall = ball - target;
            Vector2 ballToRobot = thisrobot.Position - ball;

            // Define points to go to at different stages
            Vector2 p1 = extend(target, ball, DIST_BEHIND_BALL);
            Vector2 p2 = extend(target, ball, -DIST_THROUGH_BALL);
            double desiredOrientation = ballToTarget.cartesianAngle();

            double distToPoint1 = Math.Sqrt(thisrobot.Position.distanceSq(p1));

            // Find lateral and parallel distances
            // First find angle from ball to robot, if the vector from the target through the
            // ball has angle 0
            double ballToRobotAngle = UsefulFunctions.angleDifference(targetToBall.cartesianAngle(),
                                      ballToRobot.cartesianAngle());

            double lateralDistance = distRobotToBall * Math.Sin(ballToRobotAngle);
            double parallelDistance = distRobotToBall * Math.Cos(ballToRobotAngle);

            Console.WriteLine("Parallel distance " + parallelDistance);
            Console.WriteLine("Lateral distance " + lateralDistance);

            // check whether robot is in necessary range to go to point 2
            bool inRangeContinue = (lateralDistance < MAX_LATERAL_GO_THROUGH &&
                                    parallelDistance < MAX_PARALLEL_GO_THROUGH &&
                                    parallelDistance > 0);

            // check whether robot is in necessary distance and orientation range in point 1 to
            // switch to go to point 2
            double diffOrientation = UsefulFunctions.angleDifference(desiredOrientation, thisrobot.Orientation);

            Console.WriteLine("diffOrientation: " + diffOrientation);

            bool pointClose = (distToPoint1 <= MAX_DIST_POINT_1);

            bool pointSwitch = (pointClose && 
                                Math.Abs(diffOrientation) < MAX_DIFF_ORIENTATION_POINT_1);

            // if close to point but orientation is off, use PID loop to correct orientation
            if ((pointClose && !pointSwitch))
            {
                WheelSpeeds tempSpeeds = new WheelSpeeds();
                if (diffOrientation < 0) { tempSpeeds = CCWSpeeds; }
                else { tempSpeeds = CWSpeeds; };

                /*int spinSpeed = (int)loop.compute(diffOrientation, 0);

                if (spinSpeed > SPIN_SPEED_CAP){ spinSpeed = SPIN_SPEED_CAP; }
                if (spinSpeed < -SPIN_SPEED_CAP) { spinSpeed = -SPIN_SPEED_CAP; }

                //Console.WriteLine("spinSpeed: " + spinSpeed);

                WheelSpeeds tempSpeeds = new WheelSpeeds(spinSpeed, -spinSpeed, spinSpeed, -spinSpeed);*/

                breakBeamOn = true;

                return new KickPlanningResults(tempSpeeds);
            }

            // if close to point and orientation is on but haven't started charging yet, wait
            DateTime nowCached = DateTime.Now;
            if ((pointSwitch && (nowCached - _timesStartedCharging[id]).TotalMilliseconds < TIME_WAIT_POINT_1 ||
                _timesStartedCharging[id] == arbitraryPastTime))
            {
                if (_timesStartedCharging[id] == arbitraryPastTime)
                {
                    _timesStartedCharging[id] = nowCached;
                }

                // return empty speeds, telling it to charge the kicker
                WheelSpeeds tempSpeeds = new WheelSpeeds();
                return new KickPlanningResults(tempSpeeds, true);
            }
            
            
            if (pointSwitch && (nowCached - _timesStartedCharging[id]).TotalMilliseconds > 5000)
            {
                _timesStartedCharging[id] = arbitraryPastTime;
            }

            // if the robot is close to point 1, or if it is in continuation range and already going
            // to point two, go to point two
            if (pointSwitch || (inRangeContinue && !goingToPoint1))
            {
                goingToPoint1 = false;
                Console.WriteLine("IN RANGE!!!");
            }
            else
            {
                goingToPoint1 = true;
            }

            // go to the appropriate point
            WheelSpeeds speeds = new WheelSpeeds();
            if (goingToPoint1)
            {
                RobotInfo desiredState = new RobotInfo(p1, desiredOrientation, id);
                speeds = regularPlanner.PlanMotion(id, desiredState, predictor, .3).wheel_speeds;
            }
            else
            {
                RobotInfo desiredState = new RobotInfo(p2, desiredOrientation, id);
                speeds = slowPlanner.PlanMotion(id, desiredState, predictor, .3).wheel_speeds;

                breakBeamOn = true;
            }

            

            // return a KickPlanningResults object with the appropriate information on
            // wheel speeds and break beam

            return new KickPlanningResults(speeds, breakBeamOn);

            /*
            //position the robot close enough to the ball so that it can kick soon,
            //but far enough so that the break-beam doesn't trigger

            Vector2 farDestination = extend(target, ball, 2 * kickDistance);

            Vector2 ballToTarget = (target - ball).normalize();
            Vector2 robotToBall = (ball - thisrobot.Position).normalize();
            Vector2 robotToTarget = (target - thisrobot.Position).normalize();

            bool behindBall = (robotToTarget.magnitudeSq() > ballToTarget.magnitudeSq());

            //Vector2 actualDestination = extend(target, ball, 0.75*kickDistance);
            Vector2 actualDestination = ball + (target - ball).normalizeToLength(kickDistance * 2);
            double destinationAngle = targetAngle(ball, target);
            double theta = Math.Abs(UsefulFunctions.angleDifference(robotToBall.cartesianAngle(), ballToTarget.cartesianAngle()));
            double distRobotToBall = Math.Sqrt(thisrobot.Position.distanceSq(ball));
            double lateralDistance = distRobotToBall * Math.Sin(theta);*/
        }

        public void DrawLast(Graphics g, ICoordinateConverter c) { }

        public void ReloadConstants() {
            Constants.Load("kickplanning");

            DIST_BEHIND_BALL = Constants.get<double>("kickplanning", "DIST_BEHIND_BALL");
            DIST_THROUGH_BALL = Constants.get<double>("kickplanning", "DIST_THROUGH_BALL");

            // convert waiting time to milliseconds
            TIME_WAIT_POINT_1 = Constants.get<double>("kickplanning", "TIME_WAIT_POINT_1") * 1000;

            MAX_LATERAL_GO_THROUGH = Constants.get<double>("kickplanning", "MAX_LATERAL_GO_THROUGH");
            MAX_PARALLEL_GO_THROUGH = Constants.get<double>("kickplanning", "MAX_PARALLEL_GO_THROUGH");

            MAX_DIST_BREAK_BEAM = Constants.get<double>("kickplanning", "MAX_DIST_BREAK_BEAM");

            DIST_ACTIVATE_KICK_PLANNER = Constants.get<double>("kickplanning", "DIST_ACTIVATE_KICK_PLANNER");

            MAX_DIST_POINT_1 = Constants.get<double>("kickplanning", "MAX_DIST_POINT_1");
            MAX_DIFF_ORIENTATION_POINT_1 = Constants.get<double>("kickplanning", "MAX_DIFF_ORIENTATION_POINT_1");

            SPIN_SPEED_CAP = Constants.get<int>("kickplanning", "SPIN_SPEED_CAP");

            WHEEL_SPEED_TURN = SPIN_SPEED_CAP;

            CWSpeeds = new WheelSpeeds(WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN, WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN);
            CCWSpeeds = new WheelSpeeds(-WHEEL_SPEED_TURN, WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN, WHEEL_SPEED_TURN);

            regularPlanner.LoadConstants();
            slowPlanner.LoadConstants();
            loop.ReloadConstants();
        }

        /// <summary>
        /// Return the distance from the destination point the robot should be at for 
        /// this distance planner to activate
        /// </summary>
        /// <returns></returns>
        public double getDistanceActivate() {
            return DIST_ACTIVATE_KICK_PLANNER;
        }

        /// <summary>
        /// Given the parameters for kicking, if the regular motion planner is still being
        /// used, return the destination point for the robot to set up kicking
        /// </summary>
        /// <param name="id"></param>
        /// <param name="target"></param>
        /// <param name="predictor"></param>
        /// <returns></returns>
        public Vector2 getDestinationPoint(int id, Vector2 target, IPredictor predictor)
        {
            // find characteristics of field
            RobotInfo thisrobot = predictor.getCurrentInformation(id);
            BallInfo ballinfo = predictor.getBallInfo();
            Vector2 ball = ballinfo.Position;

            // return point to go to
            return extend(target, ball, DIST_BEHIND_BALL);
        }

        public static Vector2 extend(Vector2 p1, Vector2 p2, double distance)
        {
            return p2 + (p2 - p1).normalizeToLength(distance);
        }
    }

}
