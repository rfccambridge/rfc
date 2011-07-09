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
#if false
    /// <summary>
    /// Head towards a point behind the ball (relative to the target), once you are there,
    /// use a very slow version of FeedbackVeerDriver to move forward to hit the ball
    /// </summary>
    /// TODO: Gets called at strategy loop frequency! Modify to incorporate separate planning & control loops!
    /// TODO: On a second look, seems to always call the same planner. Why the need for this wrapper then?
    public class FeedbackVeerKickPlanner : IKickPlanner
    {
        // Class variables
        bool goingToPoint1 = true;

        IMotionPlanner regularPlanner; //why is this and dumb planner both used!!!? WTF? they're both tangent bug's ???
        TangentBugFeedbackMotionPlanner dumbPlanner = new TangentBugFeedbackMotionPlanner();

        // Constants
        double DIST_BEHIND_BALL;
        double DIST_THROUGH_BALL;

        double TIME_WAIT_POINT_1;

        double MAX_LATERAL_GO_THROUGH;
        double MAX_PARALLEL_GO_THROUGH;

        double MAX_DIST_BREAK_BEAM;

        double DIST_ACTIVATE_KICK_PLANNER;

        double MAX_DIST_POINT_1;
        double MAX_DIST_MOVE_TRANSLATE_POINT_1;
        double MAX_DIFF_ORIENTATION_POINT_1;
        int SPEED_LATERAL;

        int SPIN_SPEED_CAP;

        double BALL_AVOID_RADIUS;

        double BALL_DISTANCE_CHARGE;

        WheelSpeeds CWSpeeds;
        WheelSpeeds CCWSpeeds;
        WheelSpeeds LeftSpeed;
        WheelSpeeds RightSpeed;

        int WHEEL_SPEED_TURN;

        //Local copy, will not change on reload
        static int NUM_ROBOTS = Constants.Basic.NUM_ROBOTS;

        DateTime arbitraryPastTime = new DateTime(2000, 1, 1);

        private DateTime[] _timesStartedCharging = new DateTime[NUM_ROBOTS]; //number of robots

        PIDLoop loop;        

        int SATURATION_LIMIT;

        int[] saturation_counters;

        // Planner for spinning
        //DirectRobotSpinner spinplanner;
        PIDRobotSpinner spinplanner;
        
        public FeedbackVeerKickPlanner(IMotionPlanner regularPlanner)
        {
            //why is this here?! it's passed a tangent bug planner, just like dumbPlanner (terrible naming conventiosn by the way)
            // BEN: Dumb planner is called dumb planner because it used to switch to an obstacle ignoring planner at short ranges,
            // but no longer does this. It is passed a tangent bug planner because the long range motion planner can be
            // changed in RFCSystem, and this change should propagate down so we don't have to change this file as well.

            this.regularPlanner = regularPlanner;
            loop = new PIDLoop("kickplanning", "point1orientation");
            spinplanner = new PIDRobotSpinner();

            saturation_counters = new int[NUM_ROBOTS];

            for (int robotID = 0; robotID < NUM_ROBOTS; robotID++)
            {
                //Set to arbitrary time in past- January 1, 2000
                _timesStartedCharging[robotID] = arbitraryPastTime;

                saturation_counters[robotID] = 0;
            }

            LoadConstants();
        }

        public KickPlanningResults kick(Team team, int id, Vector2 target, IPredictor predictor, IReferee refbox) {
            WheelSpeeds speeds = new WheelSpeeds();

            // default break beam not on
            bool breakBeamOn = false;

            // find characteristics of field
            RobotInfo thisrobot;
            try
            {
                thisrobot = predictor.GetRobot(team, id);
            }
            catch(ApplicationException)
            {
                return new KickPlanningResults(new WheelSpeeds(), false);
            }

            BallInfo ballinfo = predictor.GetBall();
            Vector2 ball = ballinfo.Position;

            Vector2 robotToBall = ball - thisrobot.Position;

            double distRobotToBall = Math.Sqrt(ball.distanceSq(thisrobot.Position));

            Vector2 ballToTarget = target - ball;
            Vector2 targetToBall = ball - target;
            Vector2 ballToRobot = thisrobot.Position - ball;

            // Define points to go to at different stages
            Vector2 p1 = extend(target, ball, DIST_BEHIND_BALL);
            //Vector2 p2 = extend(thisrobot.Position, ball, DIST_THROUGH_BALL);
            //Vector2 p2 = ball;
            Vector2 p2 = extend(target, ball, -DIST_THROUGH_BALL);
            //double desiredOrientation = ballToTarget.cartesianAngle();

            // CHANGE!
            double desiredOrientation = robotToBall.cartesianAngle();

            double distToPoint1 = Math.Sqrt(thisrobot.Position.distanceSq(p1));

            //Console.WriteLine("dist to point 1 " + distToPoint1);

            // Find lateral and parallel distances
            // First find angle from ball to robot, if the vector from the target through the
            // ball has angle 0
            double ballToRobotAngle = Angle.AngleDifference(targetToBall.cartesianAngle(),
                                      ballToRobot.cartesianAngle());

            double lateralDistance = distRobotToBall * Math.Sin(ballToRobotAngle);
            double parallelDistance = distRobotToBall * Math.Cos(ballToRobotAngle);

            //Console.WriteLine("Parallel distance " + parallelDistance);
            //Console.WriteLine("Lateral distance " + lateralDistance);

            // check whether robot is in necessary range to go to point 2
            bool inRangeContinue = (lateralDistance < MAX_LATERAL_GO_THROUGH &&
                                    parallelDistance < MAX_PARALLEL_GO_THROUGH &&
                                    parallelDistance > 0);

            // check whether robot is in necessary distance and orientation range in point 1 to
            // switch to go to point 2
            double diffOrientation = Angle.AngleDifference(desiredOrientation, thisrobot.Orientation);

            //Console.WriteLine("diffOrientation: " + diffOrientation);

            bool pointCloseTranslate = (distToPoint1 <= MAX_DIST_MOVE_TRANSLATE_POINT_1);
            bool pointClose = (distToPoint1 <= MAX_DIST_POINT_1);
            bool orientationCorrect = (Math.Abs(diffOrientation) < MAX_DIFF_ORIENTATION_POINT_1);

            bool pointSwitch = (pointClose && orientationCorrect);

            /*// if close enough to point for dumb planning but not for final, switch to dumb planner
            if ((pointCloseTranslate && !pointClose && goingToPoint1))
            {
                RobotInfo desiredState = new RobotInfo(p1, desiredOrientation, id);
                speeds = dumbPlanner.PlanMotion(id, desiredState, predictor, 0).wheel_speeds;
                return new KickPlanningResults(speeds, false);
            }*/

            // if close enough to point but orientation is off,
            // first use IRobotSpinner to correct orientation
            //if (pointCloseLateral && !orientationCorrect)

            //Console.WriteLine(" orientationDiff " + diffOrientation + "Robot position: " + thisrobot.Position + " ball position " + ball);
            
            // spin robot to correct orientation
            /*if (pointClose && (!orientationCorrect) && goingToPoint1)
            {
                Console.WriteLine("SPINNING: distance is " + distToPoint1 + "\t Angle difference is " + diffOrientation);
                speeds = spinplanner.spinTo(id, desiredOrientation, MAX_DIFF_ORIENTATION_POINT_1, predictor);
                //RobotInfo desiredState = new RobotInfo(p1, desiredOrientation, id);
                //speeds = dumbPlanner.PlanMotion(id, desiredState, predictor, 0).wheel_speeds;
                // DO NOT CHARGE WHILE SPINNING
                return new KickPlanningResults(speeds, false);
            }*/
            if (pointClose && goingToPoint1) {
                //spinSatisfied = true;
                //return new KickPlanningResults(new WheelSpeeds(), false);
            }
            

            // if orientation is correct, move laterally
            /*if (pointCloseLateral && !pointClose && orientationCorrect)
            {
                // positive means robot moves left, negative menas robot moves right
                if (lateralDistance > 0)
                    speeds = LeftSpeed;
                else
                    speeds = RightSpeed;
                return new KickPlanningResults(speeds, false);
            }*/

            // if close to point and orientation is on but haven't started charging yet, wait
            //DateTime nowCached = DateTime.Now;
            //if ((pointSwitch && (nowCached - _timesStartedCharging[id]).TotalMilliseconds < TIME_WAIT_POINT_1 ||
            //    _timesStartedCharging[id] == arbitraryPastTime)) {
            //    if (_timesStartedCharging[id] == arbitraryPastTime) {
            //        _timesStartedCharging[id] = nowCached;
            //    }

            //    // return empty speeds, telling it to charge the kicker
            //    WheelSpeeds tempSpeeds = new WheelSpeeds();
            //    return new KickPlanningResults(tempSpeeds, true);
            //}


            //if (pointSwitch && (nowCached - _timesStartedCharging[id]).TotalMilliseconds > 5000) {
            //    _timesStartedCharging[id] = arbitraryPastTime;
            //}

            // if the robot is close to point 1, or if it is in continuation range and already going
            // to point two, go to point two
            if (pointSwitch || (inRangeContinue && !goingToPoint1))
            {
                if (saturation_counters[id] >= SATURATION_LIMIT) {
                    goingToPoint1 = false;
                }
                else {
                    goingToPoint1 = true;
                    saturation_counters[id]++;
                }
            }
            else
            {
                goingToPoint1 = true;
                saturation_counters[id] = 0;

               
            }

            // go to the appropriate point
            if (goingToPoint1)
            {
                //TODO(davidwu): Need to use something for the DefenseAreaAvoid?
                RobotInfo desiredState = new RobotInfo(p1, desiredOrientation, team, id);
            	RobotPath path = dumbPlanner.PlanMotion(desiredState, predictor, BALL_AVOID_RADIUS, null, 
                    DefenseAreaAvoid.NONE, DefenseAreaAvoid.NONE); 
            	speeds = dumbPlanner.FollowPath(path, predictor).wheel_speeds;
                //why was regular Planner being used here? it seems inconsistent, sometimes using one instance of the bugNavigator and sometimes another.
                    //regularPlanner.PlanMotion(team, id, desiredState, predictor, BALL_AVOID_RADIUS).wheel_speeds;
            }
            else
            {
                //TODO(davidwu): Need to use something for the DefenseAreaAvoid?
                RobotInfo desiredState = new RobotInfo(p2, desiredOrientation, team, id);
                RobotPath path = dumbPlanner.PlanMotion(desiredState, predictor, 0, null, 
                    DefenseAreaAvoid.NONE, DefenseAreaAvoid.NONE);
				speeds = dumbPlanner.FollowPath(path,predictor).wheel_speeds;// This is the "normal one"
                //speeds = dumbPlanner.PlanMotion(team, id, desiredState, predictor, 0).wheel_speeds;

                // FOR NOW!!! Go forwards rather than use PID feedback. Meant to fix forward curving problem
                //speeds = new WheelSpeeds(10, 10, 10, 10);                
                breakBeamOn = true;
            }

            // ignore above code on break beam (remove later, this is at competition and we are concerned about quick
            // reversibility), instead decide whether to turn on the break beam based on the distance to the ball

            breakBeamOn = (distRobotToBall <= BALL_DISTANCE_CHARGE);

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


        public void LoadConstants() {
            ConstantsRaw.Load();

            DIST_BEHIND_BALL = ConstantsRaw.get<double>("kickplanning", "DIST_BEHIND_BALL");
            DIST_THROUGH_BALL = ConstantsRaw.get<double>("kickplanning", "DIST_THROUGH_BALL");

            // convert waiting time to milliseconds
            TIME_WAIT_POINT_1 = ConstantsRaw.get<double>("kickplanning", "TIME_WAIT_POINT_1") * 1000;

            MAX_LATERAL_GO_THROUGH = ConstantsRaw.get<double>("kickplanning", "MAX_LATERAL_GO_THROUGH");
            MAX_PARALLEL_GO_THROUGH = ConstantsRaw.get<double>("kickplanning", "MAX_PARALLEL_GO_THROUGH");

            MAX_DIST_BREAK_BEAM = ConstantsRaw.get<double>("kickplanning", "MAX_DIST_BREAK_BEAM");

            DIST_ACTIVATE_KICK_PLANNER = ConstantsRaw.get<double>("kickplanning", "DIST_ACTIVATE_KICK_PLANNER");

            MAX_DIST_MOVE_TRANSLATE_POINT_1 = ConstantsRaw.get<double>("kickplanning", "MAX_DIST_MOVE_TRANSLATE_POINT_1");
            MAX_DIST_POINT_1 = ConstantsRaw.get<double>("kickplanning", "MAX_DIST_POINT_1");
            MAX_DIFF_ORIENTATION_POINT_1 = ConstantsRaw.get<double>("kickplanning", "MAX_DIFF_ORIENTATION_POINT_1");
            
            SPEED_LATERAL = ConstantsRaw.get<int>("kickplanning", "SPEED_LATERAL");

            SPIN_SPEED_CAP = ConstantsRaw.get<int>("kickplanning", "SPIN_SPEED_CAP");

            SATURATION_LIMIT = ConstantsRaw.get<int>("kickplanning", "SATURATION_LIMIT");

            BALL_AVOID_RADIUS = ConstantsRaw.get<double>("kickplanning", "BALL_AVOID_RADIUS");

            LeftSpeed = new WheelSpeeds(-SPEED_LATERAL, SPEED_LATERAL, SPEED_LATERAL, -SPEED_LATERAL);
            RightSpeed = new WheelSpeeds(-SPEED_LATERAL, SPEED_LATERAL, SPEED_LATERAL, -SPEED_LATERAL);

            WHEEL_SPEED_TURN = SPIN_SPEED_CAP;

            BALL_DISTANCE_CHARGE = ConstantsRaw.get<double>("kickplanning", "BALL_DISTANCE_CHARGE");

            CWSpeeds = new WheelSpeeds(WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN, WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN);
            CCWSpeeds = new WheelSpeeds(-WHEEL_SPEED_TURN, WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN, WHEEL_SPEED_TURN);

            regularPlanner.LoadConstants();      
            spinplanner.ReloadConstants();
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
        public Vector2 getDestinationPoint(Team team, int id, Vector2 target, IPredictor predictor)
        {
            // find characteristics of field
            RobotInfo thisrobot;
            try
            {
                thisrobot = predictor.GetRobot(team, id);
            }
            catch (ApplicationException)
            {
                return new Vector2(0.0, 0.0); 
            }
            BallInfo ballinfo = predictor.GetBall();
            Vector2 ball = ballinfo.Position;

            // return point to go to
            return extend(target, ball, DIST_BEHIND_BALL);
        }

        public static Vector2 extend(Vector2 p1, Vector2 p2, double distance)
        {
            return p2 + (p2 - p1).normalizeToLength(distance);
        }
    }
#endif
}
