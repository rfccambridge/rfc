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

using Navigator = Navigation.Examples.BugNavigator;


namespace Robocup.MotionControl
{
    // Implementations of IPathDrivers- given a path, follow it

    /// <summary>
    /// Drive straight towards nearest path point using WheelSpeedsExtender
    /// Future generations should know that this does not work well- it uses ancient
    /// code
    /// </summary>
    public class ExtenderDriver : IPathDriver
    {
        double STOP_DISTANCE;

        public WheelSpeeds followPath(RobotPath path, IPredictor predictor)
        {
            // get parameters from input
            Team team = path.Team;
            int id = path.ID;
            RobotInfo currentState;
            try
            {
                currentState = predictor.GetRobot(team, id);
            }
            catch (ApplicationException e)
            {
                return new WheelSpeeds();
            }

            RobotInfo desiredState = path.findNearestWaypoint(currentState);

            // speeds default to zero
            WheelSpeeds speeds = new WheelSpeeds();

            // if farther away than stopping distance, get speesd to point
            if (currentState.Position.distanceSq(desiredState.Position) >
                STOP_DISTANCE * STOP_DISTANCE)
            {
                speeds = WheelSpeedsExtender.GetWheelSpeedsThrough(currentState, desiredState);
            }

            //Console.WriteLine(" LF: " + speeds.lf.ToString() + " RF: " + speeds.rf.ToString() +
            //    " LB: " + speeds.lb.ToString() + " RB: " + speeds.rb.ToString());

            return speeds;
        }

        public void ReloadConstants() {
            ConstantsRaw.Load();
            STOP_DISTANCE = ConstantsRaw.get<double>("motionplanning", "STOP_DISTANCE");
        }
    }

    /// <summary>
    /// Planner that goes to a point by first turning to face it and then
    /// moving forward
    /// </summary>
    public class TurnDriver : IPathDriver
    {
        //Constants
        static int WHEEL_SPEED_STRAIGHT = ConstantsRaw.get<int>("motionplanning", "WHEEL_SPEED_STRAIGHT");
        static int WHEEL_SPEED_TURN = ConstantsRaw.get<int>("motionplanning", "WHEEL_SPEED_TURN");
        double MIN_ANGLE_DIFFERENCE = ConstantsRaw.get<double>("motionplanning", "MIN_ANGLE_DIFFERENCE");
        double STOP_DISTANCE = ConstantsRaw.get<double>("motionplanning", "STOP_DISTANCE");

        WheelSpeeds forwardSpeeds = new WheelSpeeds(WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT);
        WheelSpeeds CWSpeeds = new WheelSpeeds(WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN, WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN);
        WheelSpeeds CCWSpeeds = new WheelSpeeds(-WHEEL_SPEED_TURN, WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN, WHEEL_SPEED_TURN);

        // size of circle in radians
        const double CIRCLE_SIZE = 2 * Math.PI;

        /// <summary>
        /// Send 
        /// </summary>
        /// <param name="angle_difference"></param>
        /// <returns></returns>
        private double getSmallestAngle(double angle_difference, double circle_size)
        {

            if (Math.Abs(angle_difference) > circle_size / 2)
            {
                // Turn in opposite direction than expected
                if (angle_difference > 0)
                {
                    angle_difference = circle_size / 2 - angle_difference;
                }
                else
                {
                    angle_difference = circle_size + angle_difference;
                }
            }

            return angle_difference;
        }

        // Dummy methods- required for interface
        public void ReloadConstants()
        {
            // Speed of a single wheel

            WHEEL_SPEED_STRAIGHT = ConstantsRaw.get<int>("motionplanning", "WHEEL_SPEED_STRAIGHT");
            WHEEL_SPEED_TURN = ConstantsRaw.get<int>("motionplanning", "WHEEL_SPEED_TURN");

            forwardSpeeds = new WheelSpeeds(WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT);
            CWSpeeds = new WheelSpeeds(WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN, WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN);
            CCWSpeeds = new WheelSpeeds(-WHEEL_SPEED_TURN, WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN, WHEEL_SPEED_TURN);

            // Distance from goal at which the robot will stop
            STOP_DISTANCE = ConstantsRaw.get<double>("motionplanning", "STOP_DISTANCE");

            //Distance above which will turn
            MIN_ANGLE_DIFFERENCE = ConstantsRaw.get<double>("motionplanning", "MIN_ANGLE_DIFFERENCE");
            Console.WriteLine("RELOADING CONSTANTS!!! " + STOP_DISTANCE + " " + MIN_ANGLE_DIFFERENCE);
        }

        /// <summary>
        /// Plan motion using straightforward WheelSpeedsExtender
        /// </summary>
        public WheelSpeeds followPath(RobotPath path, IPredictor predictor)
        {
            Team team = path.Team;
            int id = path.ID;
            RobotInfo currentState;

            try
            {
                currentState = predictor.GetRobot(team, id);
            }
            catch (ApplicationException e)
            {
                return new WheelSpeeds();
            }

            RobotInfo desiredState = path.findNearestWaypoint(currentState);

            // speeds default to zero
            WheelSpeeds speeds = new WheelSpeeds();

            // find direction to point
            Vector2 dir_to_goal = (desiredState.Position - currentState.Position).normalize();
            double angle_to_goal = dir_to_goal.cartesianAngle();
            double curAngle = currentState.Orientation;

            double angle_diff = curAngle - angle_to_goal;



            //If close enough, return speeds of zero
            if (currentState.Position.distanceSq(desiredState.Position) <
                STOP_DISTANCE * STOP_DISTANCE)
            {
                return speeds;
            }

            // If pointing in right direction, go forward
            if (Math.Abs(angle_diff) < MIN_ANGLE_DIFFERENCE)
            {
                Console.WriteLine("Going forward: " + forwardSpeeds.toString());
                speeds = forwardSpeeds;
            }

            // if angle is too far off, go CW or CCW
            else if (angle_diff >= MIN_ANGLE_DIFFERENCE)
            {
                Console.WriteLine("Going CW: " + CWSpeeds.toString());
                speeds = CWSpeeds;
            }

            else if (angle_diff <= -MIN_ANGLE_DIFFERENCE)
            {
                Console.WriteLine("Going CCW: " + CCWSpeeds.toString());
                speeds = CCWSpeeds;
            }

            //print speeds that are set
            Console.WriteLine(speeds.toString());
            Console.WriteLine(angle_diff.ToString());
            Console.WriteLine("Distance to goal squared: " +
                currentState.Position.distanceSq(desiredState.Position).ToString());

            // return speeds as set
            return speeds;
        }
    }

    public class FeedbackVeerDriver : IPathDriver
    {
        // CONSTANTS
        int ANGULAR_VEER;

        static int NUM_ROBOTS = ConstantsRaw.get<int>("motionplanning", "NUM_ROBOTS");

        static int WHEEL_SPEED_STRAIGHT;
        static int WHEEL_SPEED_TURN;
        double MIN_ANGLE_SPIN;
        double STOP_DISTANCE;
        double MAX_FINAL_ANGLE_DIFFERENCE;
        double DIST_SLOW_DOWN;
        double WHEEL_RADIUS;
        double MIN_ANGLE_KEEP_SPINNING;

        double MIN_ANGLE_SWITCH;

        bool USE_INDIVIDUAL_WHEEL_SPEEDS;

        // Keep track of goal orientation for angular velocity veer
        // double currentGoalOrientation = 0;
        double currentForwardAngle = 0;

        /*Wheel orientations
        double RF_orientation;
        double LF_orientation;
        double LB_orientation;
        double RB_orientation;*/

        //double LOOK_AHEAD_DISTANCE;        

        double ANGLE_AXIS_TO_WHEEL;
        WheelSpeeds forwardSpeeds;
        WheelSpeeds CWSpeeds;
        WheelSpeeds CCWSpeeds;

        // PID loops
        MultiRobotPIDLoop loops = new MultiRobotPIDLoop("motionplanning", "VEER", NUM_ROBOTS);
        MultiRobotPIDLoop angular_loops = new MultiRobotPIDLoop("motionplanning", "ANGULAR_VEER", NUM_ROBOTS);

        bool alreadySpinning = false;        

        Vector2 lastWayPoint;

        public FeedbackVeerDriver()
        {
            // load initial values of constants

            ReloadConstants();
        }

        /// <summary>
        /// Reloads both class constants (wheel speeds, distance to stop, etc) and
        /// PID constants for loop
        /// </summary>
        public void ReloadConstants()
        {
            // Reload constants file
            ConstantsRaw.Load();

            // Either 0 or 1, represents whether to use the angular veer planner
            ANGULAR_VEER = ConstantsRaw.get<int>("motionplanning", "ANGULAR_VEER");

            // Speed of a single wheel

            WHEEL_SPEED_STRAIGHT = ConstantsRaw.get<int>("motionplanning", "WHEEL_SPEED_STRAIGHT");
            WHEEL_SPEED_TURN = ConstantsRaw.get<int>("motionplanning", "WHEEL_SPEED_TURN");

            forwardSpeeds = new WheelSpeeds(WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT);
            CWSpeeds = new WheelSpeeds(WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN, WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN);
            CCWSpeeds = new WheelSpeeds(-WHEEL_SPEED_TURN, WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN, WHEEL_SPEED_TURN);

            // Distance from goal at which the robot will stop
            STOP_DISTANCE = ConstantsRaw.get<double>("motionplanning", "STOP_DISTANCE");

            //Distance above which will turn
            MIN_ANGLE_SPIN = ConstantsRaw.get<double>("motionplanning", "MIN_ANGLE_SPIN");

            // Distance above which, if already spinning, will keep turning
            MIN_ANGLE_KEEP_SPINNING = ConstantsRaw.get<double>("motionplanning", "MIN_ANGLE_KEEP_SPINNING");

            //Maximum final angle difference tolerated
            MAX_FINAL_ANGLE_DIFFERENCE = ConstantsRaw.get<double>("motionplanning", "MAX_FINAL_ANGLE_DIFFERENCE");

            DIST_SLOW_DOWN = ConstantsRaw.get<double>("motionplanning", "DIST_SLOW_DOWN");

            USE_INDIVIDUAL_WHEEL_SPEEDS = ConstantsRaw.get<bool>("motionplanning", "USE_INDIVIDUAL_WHEEL_SPEEDS");

            /* Wheel orientations
            double RF_ORIENTATION = ConstantsRaw.get<double>("motionplanning", "RF_ORIENTATION");
            double LF_ORIENTATION = ConstantsRaw.get<double>("motionplanning", "LF_ORIENTATION");
            double LB_ORIENTATION = ConstantsRaw.get<double>("motionplanning", "LB_ORIENTATION");
            double RB_ORIENTATION = ConstantsRaw.get<double>("motionplanning", "RB_ORIENTATION");*/

            // robot composition
            ANGLE_AXIS_TO_WHEEL = ConstantsRaw.get<double>("motionplanning", "ANGLE_AXIS_TO_WHEEL");
            WHEEL_RADIUS = ConstantsRaw.get<double>("motionplanning", "WHEEL_RADIUS");

            MIN_ANGLE_SWITCH = ConstantsRaw.get<double>("motionplanning", "MIN_ANGLE_SWITCH");

            // Reload PID constants
            loops.ReloadConstants();
            angular_loops.ReloadConstants();
        }

        /// <summary>
        /// Plan motion using PID loop to control veer
        /// </summary>
        public WheelSpeeds followPath(RobotPath path, IPredictor predictor)
        {
            RobotInfo currentState;
            try
            {
                currentState = predictor.GetRobot(path.Team, path.ID);
            }
            catch (ApplicationException e)
            {
                return new WheelSpeeds();
            }

            lastWayPoint = path.findNearestWaypoint(currentState).Position;
            RobotInfo waypointState = new RobotInfo(lastWayPoint, 0, path.ID);

            Console.WriteLine("WAYPOINT: " + lastWayPoint.ToString());

            WheelSpeeds speeds;
            //get wheel speeds using appropriate method
            if (ANGULAR_VEER == 1)
            {
                speeds = angularMethod(currentState, waypointState);
            }
            else
            {
                speeds = orientationMethod(currentState, waypointState);
            }

            //print speeds that are set
            //Console.WriteLine(speeds.toString());
            //Console.WriteLine(angle_diff.ToString());
            //Console.WriteLine("Distance to goal squared: " + sqDistToGoal);

            // return speeds as set
            return speeds;
        }

        /// <summary>
        /// get wheel speeds using orientation method
        /// </summary>
        /// <param name="?"></param>
        /// <param name="?"></param>
        /// <returns></returns>
        private WheelSpeeds orientationMethod(RobotInfo currentState, RobotInfo desiredState)
        {
            // speeds default to zero
            WheelSpeeds speeds = new WheelSpeeds();

            // find direction to point
            //Vector2 dir_to_goal = (goal_vector - currentState.Position).normalize();
            int id = currentState.ID;
            Vector2 dir_to_goal = (desiredState.Position - currentState.Position).normalize();
            
            // Angle difference is the amount the goal is to the right of the current orientation
            double angle_to_goal = dir_to_goal.cartesianAngle();
            double angle_diff = UsefulFunctions.angleDifference(angle_to_goal, currentState.Orientation);
            double sqDistToGoal = currentState.Position.distanceSq(desiredState.Position);

            Console.WriteLine("ORIENTATION " + currentState.Orientation);
            Console.WriteLine("ANGLE DIFFERENCE " + angle_diff);

            //If close enough, spin to face correct orientation and stop
            if (sqDistToGoal < STOP_DISTANCE * STOP_DISTANCE)
            {
                return stopSpeeds(currentState, desiredState);
            }

            // If pointing in right direction, go forward with veer
            if (Math.Abs(angle_diff) < MIN_ANGLE_SPIN && (Math.Abs(angle_diff) < MIN_ANGLE_KEEP_SPINNING ||
                                                            !alreadySpinning))
            {
                WheelSpeeds veerSpeeds = getVeer(angle_diff, sqDistToGoal, id);
                //Console.WriteLine("Going forward: " + veerSpeeds.toString());
                speeds = veerSpeeds;
                alreadySpinning = false;
            }

            // if angle is too far off, go CW or CCW
            else if (angle_diff >= 0)
            {
                //Console.WriteLine("Going CW: " + CWSpeeds.toString());
                speeds = CWSpeeds;
                alreadySpinning = true;
            }

            else if (angle_diff < 0)
            {
                //Console.WriteLine("Going CCW: " + CCWSpeeds.toString());
                speeds = CCWSpeeds;
                alreadySpinning = true;
            }

            return speeds;
        }

        /// <summary>
        /// Given the difference between the current angle and the goal, as well
        /// as the distance to the goal, return wheel speeds
        /// </summary>
        /// <param name="angle_difference"></param>
        /// <returns></returns>
        private WheelSpeeds getVeer(double angle_difference, double sqDistToGoal, int id)
        {
            // use PID loop to get veer based on angle difference
            // is amount to veer to the left
            double veerAmount = loops.compute(angle_difference, 0, id);

            int straightSpeed = WHEEL_SPEED_STRAIGHT;

            // If too close, slow down
            if (sqDistToGoal < DIST_SLOW_DOWN * DIST_SLOW_DOWN)
            {
                // Make speed proportionate to distance to goal
                straightSpeed = transformToWheelSpeed(straightSpeed * (Math.Sqrt(sqDistToGoal) / DIST_SLOW_DOWN));
                //Console.WriteLine("Slowing down from " + WHEEL_SPEED_STRAIGHT.ToString() + " to " + straightSpeed.ToString());

            }

            Console.WriteLine("Speed: " + straightSpeed);

            return addVeer(veerAmount, straightSpeed);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private WheelSpeeds angularMethod(RobotInfo currentState, RobotInfo desiredState)
        {
            // find relevant parameters
            int id = currentState.ID;
            double orientation = currentState.Orientation;
            Vector2 dir_to_goal = desiredState.Position - currentState.Position;
            double sqDistToGoal = dir_to_goal.magnitudeSq();

            // if close enough to goal, just stop (by correcting orientation)
            if (sqDistToGoal < STOP_DISTANCE * STOP_DISTANCE)
            {
                return stopSpeeds(currentState, desiredState);
            }

            // Otherwise, find velocity based on angle difference between current orientation and
            // current goal orientation

            // Get scaled speed
            int straightSpeed;
            // if it is individual, get it for the robot
            if (USE_INDIVIDUAL_WHEEL_SPEEDS)
            {
                straightSpeed = ConstantsRaw.get<int>("motionplanning", "WHEEL_SPEED_STRAIGHT_" + id);
            }
            else
            {
                straightSpeed = WHEEL_SPEED_STRAIGHT;
            }

            // If too close, slow down
            if (sqDistToGoal < DIST_SLOW_DOWN * DIST_SLOW_DOWN)
            {
                // Make speed proportionate to distance to goal
                 straightSpeed = transformToWheelSpeed(straightSpeed * (Math.Sqrt(sqDistToGoal) / DIST_SLOW_DOWN));
                //Console.WriteLine("Slowing down from " + WHEEL_SPEED_STRAIGHT.ToString() + " to " + straightSpeed.ToString());
            }

            // realign the "front" of the robot
            setForwardAngle(currentState, desiredState);

            double absoluteGoalAngle = dir_to_goal.cartesianAngle();

            double goal_angle_difference = UsefulFunctions.angleDifference(absoluteGoalAngle, currentForwardAngle+orientation);

            /*Console.WriteLine("ORIENTATION: " + orientation);
            Console.WriteLine("CURRENT FORWARD ANGLE: " + currentForwardAngle);
            Console.WriteLine("ABSOLUTE GOAL ANGLE: " + absoluteGoalAngle);
            Console.WriteLine("GOAL ANGLE DIFFERENCE: " + goal_angle_difference);
             */

            WheelSpeeds regularSpeed = computeSpeedsAtAngle(straightSpeed, currentForwardAngle);

            // use feedback to find veer
            // use PID loop to get veer based on angle difference between current state and goal orientation

            double veerAmount = angular_loops.compute(goal_angle_difference, 0, id);

            Console.WriteLine("VEER AMOUNT: " + veerAmount);

            return addAngularVeer(regularSpeed, veerAmount);
        }

        /// <summary>
        /// Return wheel speeds based on default forward speeds with veer added
        /// </summary>
        /// <param name="veer"></param>
        /// <returns></returns>
        private WheelSpeeds addVeer(double veer, int straightSpeed)
        {
            // veer represents how much higher right speeds are
            int leftspeed = transformToWheelSpeed(straightSpeed - veer);
            int rightspeed = transformToWheelSpeed(straightSpeed + veer);
            //Console.WriteLine("Left speed: " + leftspeed);
            //Console.WriteLine("Right speed: " + rightspeed);
            return new WheelSpeeds(leftspeed, rightspeed, leftspeed, rightspeed);
        }

        /// <summary>
        /// Given a number for the veer, get wheel speeds with veer added (veer represents
        /// how much robot is turning towards left)
        /// </summary>
        private WheelSpeeds addAngularVeer(WheelSpeeds speeds, double veer)
        {
            // Change every speed by veer amount
            int intVeer = (int) veer;
            WheelSpeeds newspeeds = new WheelSpeeds(speeds.lf - intVeer, speeds.rf + intVeer,
                                        speeds.lb - intVeer, speeds.rb + intVeer);
            return newspeeds;

        }

        /// <summary>
        /// Convert any given double into an int that is a valid wheel speed
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        private static int transformToWheelSpeed(double s)
        {
            // ensure number is an int and is between -127 and 127
            return (int)Math.Max(Math.Min(s, 127), -127);
        }

        /// <summary>
        /// Compute speeds to go at a particular angle, given the angle clockwise from to the robot's
        /// 0 angle
        /// </summary>
        /// <param name="velocity"></param>
        /// <param name="angle_goal"></param>
        /// <returns></returns>
        private static WheelSpeeds computeSpeedsAtAngle(double velocity, double angle_goal)
        {
            const double wheel_angle = Math.PI / 6;
            double sin_wheel_angle = Math.Sin(wheel_angle);
            double cos_wheel_angle = Math.Cos(wheel_angle);

            double sin_goal_angle = Math.Sin(angle_goal);
            double cos_goal_angle = Math.Cos(angle_goal);

            double magnitude_x_component = -velocity * sin_goal_angle / sin_wheel_angle;
            double magnitude_y_component = velocity * cos_goal_angle / cos_wheel_angle;

            int left_front, right_front, right_back, left_back;

            left_front = transformToWheelSpeed(magnitude_y_component + magnitude_x_component);
            right_front = transformToWheelSpeed(magnitude_y_component - magnitude_x_component);
            right_back = transformToWheelSpeed(magnitude_y_component + magnitude_x_component);
            left_back = transformToWheelSpeed(magnitude_y_component - magnitude_x_component);

            WheelSpeeds w = new WheelSpeeds(left_front, right_front, left_back, right_back);
            //Console.WriteLine("velocity: " + Convert.ToString(velocity) + " angle: " + Convert.ToString(angle_goal) + " wheel speeds: " + w.toString());

            return w;
        }

        /// <summary>
        /// Sets currentGoalOrientation based on orientation relative to the goal orientation- if too far,
        /// reset
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="desiredState"></param>
        private void setForwardAngle(RobotInfo currentState, RobotInfo desiredState)
        {
            Vector2 dir_to_goal = desiredState.Position - currentState.Position;
            // if goal is too far, reset
            double orientation = currentState.Orientation;
            double absoluteGoalAngle = dir_to_goal.cartesianAngle();
            if (Math.Abs(UsefulFunctions.angleDifference(absoluteGoalAngle, currentForwardAngle + orientation)) > MIN_ANGLE_SWITCH)
            {
                currentForwardAngle = UsefulFunctions.angleDifference(orientation, absoluteGoalAngle);
            }
        }

        /// <summary>
        /// Call if close enough to goal to correct for orientation and stop when appropriate
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="desiredState"></param>
        /// <returns></returns>
        private WheelSpeeds stopSpeeds(RobotInfo currentState, RobotInfo desiredState)
        {
            // get angle difference between current and desired orientation
            double diff = UsefulFunctions.angleDifference(currentState.Orientation, desiredState.Orientation);
            // spin to face desired direction
            if (diff < -MAX_FINAL_ANGLE_DIFFERENCE)
            {
                // Go CW
                return CWSpeeds;
            }
            if (diff > MAX_FINAL_ANGLE_DIFFERENCE)
            {
                // Go CCW
                return CCWSpeeds;
            }
            // If it's close enough, just stop there
            return new WheelSpeeds();
        }
    }

    /// <summary>
    /// Use computeSpeedsAtAngle to find wheel speeds
    /// </summary>
    public class TranslateDriver : IPathDriver
    {
        // CONSTANTS
        int ANGULAR_VEER;

        static int NUM_ROBOTS = ConstantsRaw.get<int>("motionplanning", "NUM_ROBOTS");

        static int WHEEL_SPEED_STRAIGHT;
        static int WHEEL_SPEED_TURN;
        double MIN_ANGLE_SPIN;
        double STOP_DISTANCE;
        double MAX_FINAL_ANGLE_DIFFERENCE;
        double DIST_SLOW_DOWN;
        double WHEEL_RADIUS;
        double MIN_ANGLE_KEEP_SPINNING;

        double MIN_ANGLE_SWITCH;

        // Keep track of goal orientation for angular velocity veer
        // double currentGoalOrientation = 0;
        double currentForwardAngle = 0;

        /*Wheel orientations
        double RF_orientation;
        double LF_orientation;
        double LB_orientation;
        double RB_orientation;*/

        //double LOOK_AHEAD_DISTANCE;        

        double ANGLE_AXIS_TO_WHEEL;
        WheelSpeeds forwardSpeeds;
        WheelSpeeds CWSpeeds;
        WheelSpeeds CCWSpeeds;

        // PID loops
        MultiRobotPIDLoop loops = new MultiRobotPIDLoop("motionplanning", "VEER", NUM_ROBOTS);
        MultiRobotPIDLoop angular_loops = new MultiRobotPIDLoop("motionplanning", "ANGULAR_VEER", NUM_ROBOTS);

        Vector2 lastWayPoint;

        public TranslateDriver()
        {
            // load initial values of constants

            ReloadConstants();
        }

        /// <summary>
        /// Reloads both class constants (wheel speeds, distance to stop, etc) and
        /// PID constants for loop
        /// </summary>
        public void ReloadConstants()
        {
            // Reload constants file
            ConstantsRaw.Load();

            // Either 0 or 1, represents whether to use the angular veer planner
            ANGULAR_VEER = ConstantsRaw.get<int>("motionplanning", "ANGULAR_VEER");

            // Speed of a single wheel

            WHEEL_SPEED_STRAIGHT = ConstantsRaw.get<int>("motionplanning", "WHEEL_SPEED_STRAIGHT");
            WHEEL_SPEED_TURN = ConstantsRaw.get<int>("motionplanning", "WHEEL_SPEED_TURN");

            forwardSpeeds = new WheelSpeeds(WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT);
            CWSpeeds = new WheelSpeeds(WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN, WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN);
            CCWSpeeds = new WheelSpeeds(-WHEEL_SPEED_TURN, WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN, WHEEL_SPEED_TURN);

            // Distance from goal at which the robot will stop
            STOP_DISTANCE = ConstantsRaw.get<double>("motionplanning", "STOP_DISTANCE");

            //Distance above which will turn
            MIN_ANGLE_SPIN = ConstantsRaw.get<double>("motionplanning", "MIN_ANGLE_SPIN");

            // Distance above which, if already spinning, will keep turning
            MIN_ANGLE_KEEP_SPINNING = ConstantsRaw.get<double>("motionplanning", "MIN_ANGLE_KEEP_SPINNING");

            //Maximum final angle difference tolerated
            MAX_FINAL_ANGLE_DIFFERENCE = ConstantsRaw.get<double>("motionplanning", "MAX_FINAL_ANGLE_DIFFERENCE");

            DIST_SLOW_DOWN = ConstantsRaw.get<double>("motionplanning", "DIST_SLOW_DOWN");

            /* Wheel orientations
            double RF_ORIENTATION = ConstantsRaw.get<double>("motionplanning", "RF_ORIENTATION");
            double LF_ORIENTATION = ConstantsRaw.get<double>("motionplanning", "LF_ORIENTATION");
            double LB_ORIENTATION = ConstantsRaw.get<double>("motionplanning", "LB_ORIENTATION");
            double RB_ORIENTATION = ConstantsRaw.get<double>("motionplanning", "RB_ORIENTATION");*/

            // robot composition
            ANGLE_AXIS_TO_WHEEL = ConstantsRaw.get<double>("motionplanning", "ANGLE_AXIS_TO_WHEEL");
            WHEEL_RADIUS = ConstantsRaw.get<double>("motionplanning", "WHEEL_RADIUS");

            MIN_ANGLE_SWITCH = ConstantsRaw.get<double>("motionplanning", "MIN_ANGLE_SWITCH");

            // Reload PID constants
            loops.ReloadConstants();
            angular_loops.ReloadConstants();
        }

        /// <summary>
        /// Plan motion using PID loop to control veer
        /// </summary>
        public WheelSpeeds followPath(RobotPath path, IPredictor predictor)
        {
            RobotInfo currentState;
            try
            {
                currentState = predictor.GetRobot(path.Team, path.ID);
            }
            catch (ApplicationException e)
            {
                return new WheelSpeeds();
            }

            lastWayPoint = path.findNearestWaypoint(currentState).Position;
            RobotInfo waypointState = new RobotInfo(lastWayPoint, 0, path.ID);

            //Console.WriteLine("WAYPOINT: " + lastWayPoint.ToString());

            WheelSpeeds speeds;
            //get wheel speeds using appropriate method
            speeds = angularMethod(currentState, waypointState);

            //print speeds that are set
            //Console.WriteLine(speeds.toString());
            //Console.WriteLine(angle_diff.ToString());
            //Console.WriteLine("Distance to goal squared: " + sqDistToGoal);

            // return speeds as set
            return speeds;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private WheelSpeeds angularMethod(RobotInfo currentState, RobotInfo desiredState)
        {
            // find relevant parameters
            int id = currentState.ID;
            double orientation = currentState.Orientation;
            Vector2 dir_to_goal = desiredState.Position - currentState.Position;
            double sqDistToGoal = dir_to_goal.magnitudeSq();

            // if close enough to goal, just stop (by correcting orientation)
            if (sqDistToGoal < STOP_DISTANCE * STOP_DISTANCE)
            {
                return stopSpeeds(currentState, desiredState);
            }

            // Otherwise, find velocity based on angle difference between current orientation and
            // current goal orientation

            // Get scaled speed
            int straightSpeed = WHEEL_SPEED_STRAIGHT;

            // If too close, slow down
            if (sqDistToGoal < DIST_SLOW_DOWN * DIST_SLOW_DOWN)
            {
                // Make speed proportionate to distance to goal
                straightSpeed = transformToWheelSpeed(straightSpeed * (Math.Sqrt(sqDistToGoal) / DIST_SLOW_DOWN));
                //Console.WriteLine("Slowing down from " + WHEEL_SPEED_STRAIGHT.ToString() + " to " + straightSpeed.ToString());
            }

            // realign the "front" of the robot
            double absoluteGoalAngle = dir_to_goal.cartesianAngle();

            currentForwardAngle = UsefulFunctions.angleDifference(orientation, absoluteGoalAngle);

            Console.WriteLine("ORIENTATION: " + orientation);
            Console.WriteLine("CURRENT FORWARD ANGLE: " + currentForwardAngle);
            Console.WriteLine("ABSOLUTE GOAL ANGLE: " + absoluteGoalAngle);

            WheelSpeeds regularSpeed = computeSpeedsAtAngle(WHEEL_SPEED_STRAIGHT, currentForwardAngle);

            return regularSpeed;
        }

        /// <summary>
        /// Convert any given double into an int that is a valid wheel speed
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        private static int transformToWheelSpeed(double s)
        {
            // ensure number is an int and is between -127 and 127
            return (int)Math.Max(Math.Min(s, 127), -127);
        }

        /// <summary>
        /// Compute speeds to go at a particular angle, given the angle clockwise from to the robot's
        /// 0 angle
        /// </summary>
        /// <param name="velocity"></param>
        /// <param name="angle_goal"></param>
        /// <returns></returns>
        private static WheelSpeeds computeSpeedsAtAngle(double velocity, double angle_goal)
        {
            const double wheel_angle = Math.PI / 6;
            double sin_wheel_angle = Math.Sin(wheel_angle);
            double cos_wheel_angle = Math.Cos(wheel_angle);

            double sin_goal_angle = Math.Sin(angle_goal);
            double cos_goal_angle = Math.Cos(angle_goal);

            double magnitude_x_component = -velocity * sin_goal_angle / sin_wheel_angle;
            double magnitude_y_component = velocity * cos_goal_angle / cos_wheel_angle;

            int left_front, right_front, right_back, left_back;

            left_front = transformToWheelSpeed(magnitude_y_component + magnitude_x_component);
            right_front = transformToWheelSpeed(magnitude_y_component - magnitude_x_component);
            right_back = transformToWheelSpeed(magnitude_y_component + magnitude_x_component);
            left_back = transformToWheelSpeed(magnitude_y_component - magnitude_x_component);

            WheelSpeeds w = new WheelSpeeds(left_front, right_front, left_back, right_back);
            Console.WriteLine("velocity: " + Convert.ToString(velocity) + " angle: " + Convert.ToString(angle_goal) + " wheel speeds: " + w.toString());

            return w;
        }

        /// <summary>
        /// Call if close enough to goal to correct for orientation and stop when appropriate
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="desiredState"></param>
        /// <returns></returns>
        private WheelSpeeds stopSpeeds(RobotInfo currentState, RobotInfo desiredState)
        {
            // get angle difference between current and desired orientation
            double diff = UsefulFunctions.angleDifference(currentState.Orientation, desiredState.Orientation);
            // spin to face desired direction
            if (diff < -MAX_FINAL_ANGLE_DIFFERENCE)
            {
                // Go CW
                return CWSpeeds;
            }
            if (diff > MAX_FINAL_ANGLE_DIFFERENCE)
            {
                // Go CCW
                return CCWSpeeds;
            }
            // If it's close enough, just stop there
            return new WheelSpeeds();
        }
    }

    public class DefaultDriver: IPathDriver
    {
        // CONSTANTS
        int ANGULAR_VEER;

        static int NUM_ROBOTS = ConstantsRaw.get<int>("motionplanning", "NUM_ROBOTS");

        static int WHEEL_SPEED_STRAIGHT;
        static int WHEEL_SPEED_TURN;
        double MIN_ANGLE_SPIN;
        double STOP_DISTANCE;
        double MAX_FINAL_ANGLE_DIFFERENCE;
        double DIST_SLOW_DOWN;
        double WHEEL_RADIUS;
        double MIN_ANGLE_KEEP_SPINNING;

        double MIN_ANGLE_SWITCH;

        // Keep track of goal orientation for angular velocity veer
        // double currentGoalOrientation = 0;
        double currentForwardAngle = 0;

        /*Wheel orientations
        double RF_orientation;
        double LF_orientation;
        double LB_orientation;
        double RB_orientation;*/
        
        double ANGLE_AXIS_TO_WHEEL;
        WheelSpeeds forwardSpeeds;
        WheelSpeeds CWSpeeds;
        WheelSpeeds CCWSpeeds;

        // PID loops
        MultiRobotPIDLoop loops = new MultiRobotPIDLoop("motionplanning", "DEFAULT", NUM_ROBOTS);
        MultiRobotPIDLoop angular_loops = new MultiRobotPIDLoop("motionplanning", "DEFAULT", NUM_ROBOTS);

        Vector2 lastWayPoint;        

        public DefaultDriver()
        {
            // load initial values of constants

            ReloadConstants();
        }

        /// <summary>
        /// Reloads both class constants (wheel speeds, distance to stop, etc) and
        /// PID constants for loop
        /// </summary>
        public void ReloadConstants()
        {
            // Reload constants file
            ConstantsRaw.Load();

            // Either 0 or 1, represents whether to use the angular veer planner
            //ANGULAR_VEER = ConstantsRaw.get<int>("motionplanning", "ANGULAR_VEER");
            ANGULAR_VEER = 0;

            // Speed of a single wheel

            WHEEL_SPEED_STRAIGHT = ConstantsRaw.get<int>("motionplanning", "DEFAULT_WHEEL_SPEED_STRAIGHT");
            WHEEL_SPEED_TURN = ConstantsRaw.get<int>("motionplanning", "DEFAULT_WHEEL_SPEED_TURN");

            forwardSpeeds = new WheelSpeeds(WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT);
            CWSpeeds = new WheelSpeeds(WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN, WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN);
            CCWSpeeds = new WheelSpeeds(-WHEEL_SPEED_TURN, WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN, WHEEL_SPEED_TURN);

            // Distance from goal at which the robot will stop
            STOP_DISTANCE = ConstantsRaw.get<double>("motionplanning", "DEFAULT_STOP_DISTANCE");

            //Distance above which will turn
            MIN_ANGLE_SPIN = ConstantsRaw.get<double>("motionplanning", "MIN_ANGLE_SPIN");

            // Distance above which, if already spinning, will keep turning
            MIN_ANGLE_KEEP_SPINNING = ConstantsRaw.get<double>("motionplanning", "MIN_ANGLE_KEEP_SPINNING");

            //Maximum final angle difference tolerated
            MAX_FINAL_ANGLE_DIFFERENCE = ConstantsRaw.get<double>("motionplanning", "MAX_FINAL_ANGLE_DIFFERENCE");

            DIST_SLOW_DOWN = ConstantsRaw.get<double>("motionplanning", "DEFAULT_DIST_SLOW_DOWN");

            /* Wheel orientations
            double RF_ORIENTATION = ConstantsRaw.get<double>("motionplanning", "RF_ORIENTATION");
            double LF_ORIENTATION = ConstantsRaw.get<double>("motionplanning", "LF_ORIENTATION");
            double LB_ORIENTATION = ConstantsRaw.get<double>("motionplanning", "LB_ORIENTATION");
            double RB_ORIENTATION = ConstantsRaw.get<double>("motionplanning", "RB_ORIENTATION");*/

            // robot composition
            ANGLE_AXIS_TO_WHEEL = ConstantsRaw.get<double>("motionplanning", "ANGLE_AXIS_TO_WHEEL");
            WHEEL_RADIUS = ConstantsRaw.get<double>("motionplanning", "WHEEL_RADIUS");

            MIN_ANGLE_SWITCH = ConstantsRaw.get<double>("motionplanning", "MIN_ANGLE_SWITCH");

            // Reload PID constants
            loops.ReloadConstants();
            angular_loops.ReloadConstants();
        }

        /// <summary>
        /// Plan motion using PID loop to control veer
        /// </summary>
        public WheelSpeeds followPath(RobotPath path, IPredictor predictor)
        {
            RobotInfo currentState;
            try
            {
                currentState = predictor.GetRobot(path.Team, path.ID);
            }
            catch (ApplicationException e)
            {
                return new WheelSpeeds();
            }
            RobotInfo desiredState = path.findNearestWaypoint(currentState);

            lastWayPoint = desiredState.Position;

            Console.WriteLine("WAYPOINT: " + lastWayPoint.ToString());          

            WheelSpeeds speeds;
            //get wheel speeds using appropriate method
            if (ANGULAR_VEER == 1)
            {
                speeds = angularMethod(currentState, desiredState);
            }
            else
            {
                speeds = orientationMethod(currentState, desiredState);
            }

            //print speeds that are set
            //Console.WriteLine(speeds.toString());
            //Console.WriteLine(angle_diff.ToString());
            //Console.WriteLine("Distance to goal squared: " + sqDistToGoal);

            // return speeds as set
            return speeds;
        }

        /// <summary>
        /// get wheel speeds using orientation method
        /// </summary>
        /// <param name="?"></param>
        /// <param name="?"></param>
        /// <returns></returns>
        private WheelSpeeds orientationMethod(RobotInfo currentState, RobotInfo desiredState)
        {
            // speeds default to zero
            WheelSpeeds speeds = new WheelSpeeds();

            // find direction to point
            //Vector2 dir_to_goal = (goal_vector - currentState.Position).normalize();
            int id = currentState.ID;
            Vector2 dir_to_goal = (desiredState.Position - currentState.Position).normalize();
            double angle_to_goal = dir_to_goal.cartesianAngle();
            double angle_diff = UsefulFunctions.angleDifference(angle_to_goal, currentState.Orientation);
            double sqDistToGoal = currentState.Position.distanceSq(desiredState.Position);

            Console.WriteLine("ORIENTATION " + currentState.Orientation);
            Console.WriteLine("ANGLE DIFFERENCE " + angle_diff);

            //If close enough, spin to face correct orientation and stop
            if (sqDistToGoal < STOP_DISTANCE * STOP_DISTANCE)
            {
                Console.WriteLine("STOPPING- CLOSE ENOUGH TO GOAL AT " + Math.Sqrt(sqDistToGoal).ToString());
                return stopSpeeds(currentState, desiredState);
            }

            // If pointing in right direction, go forward with veer
            if (Math.Abs(angle_diff) < MIN_ANGLE_SPIN)
            {
                WheelSpeeds veerSpeeds = getVeer(angle_diff, sqDistToGoal, id);
                //Console.WriteLine("Going forward: " + veerSpeeds.toString());
                speeds = veerSpeeds;
            }

            // if angle is too far off, go CW or CCW
            else if (angle_diff >= 0)
            {
                //Console.WriteLine("Going CW: " + CWSpeeds.toString());
                speeds = CWSpeeds;                
            }

            else if (angle_diff < 0)
            {
                //Console.WriteLine("Going CCW: " + CCWSpeeds.toString());
                speeds = CCWSpeeds;                
            }

            return speeds;
        }

        /// <summary>
        /// Given the difference between the current angle and the goal, as well
        /// as the distance to the goal, return wheel speeds
        /// </summary>
        /// <param name="angle_difference"></param>
        /// <returns></returns>
        private WheelSpeeds getVeer(double angle_difference, double sqDistToGoal, int id)
        {
            // use PID loop to get veer based on angle difference
            double veerAmount = loops.compute(angle_difference, 0, id);

            int straightSpeed = WHEEL_SPEED_STRAIGHT;

            // If too close, slow down
            if (sqDistToGoal < DIST_SLOW_DOWN * DIST_SLOW_DOWN)
            {
                // Make speed proportionate to distance to goal
                straightSpeed = transformToWheelSpeed(straightSpeed * (Math.Sqrt(sqDistToGoal) / DIST_SLOW_DOWN));
                //Console.WriteLine("Slowing down from " + WHEEL_SPEED_STRAIGHT.ToString() + " to " + straightSpeed.ToString());

            }

            return addVeer(veerAmount, straightSpeed);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private WheelSpeeds angularMethod(RobotInfo currentState, RobotInfo desiredState)
        {
            // find relevant parameters
            int id = currentState.ID;
            double orientation = currentState.Orientation;
            Vector2 dir_to_goal = desiredState.Position - currentState.Position;
            double sqDistToGoal = dir_to_goal.magnitudeSq();

            // if close enough to goal, just stop (by correcting orientation)
            if (sqDistToGoal < STOP_DISTANCE * STOP_DISTANCE)
            {
                return stopSpeeds(currentState, desiredState);
            }

            // Otherwise, find velocity based on angle difference between current orientation and
            // current goal orientation

            // Get scaled speed
            int straightSpeed = WHEEL_SPEED_STRAIGHT;

            // If too close, slow down
            if (sqDistToGoal < DIST_SLOW_DOWN * DIST_SLOW_DOWN)
            {
                // Make speed proportionate to distance to goal
                straightSpeed = transformToWheelSpeed(straightSpeed * (Math.Sqrt(sqDistToGoal) / DIST_SLOW_DOWN));
                //Console.WriteLine("Slowing down from " + WHEEL_SPEED_STRAIGHT.ToString() + " to " + straightSpeed.ToString());
            }

            // realign the "front" of the robot
            setForwardAngle(currentState, desiredState);

            double absoluteGoalAngle = dir_to_goal.cartesianAngle();

            double goal_angle_difference = UsefulFunctions.angleDifference(absoluteGoalAngle, currentForwardAngle + orientation);

            Console.WriteLine("ORIENTATION: " + orientation);
            Console.WriteLine("CURRENT FORWARD ANGLE: " + currentForwardAngle);
            Console.WriteLine("ABSOLUTE GOAL ANGLE: " + absoluteGoalAngle);
            Console.WriteLine("GOAL ANGLE DIFFERENCE: " + goal_angle_difference);

            WheelSpeeds regularSpeed = computeSpeedsAtAngle(WHEEL_SPEED_STRAIGHT, currentForwardAngle);

            // use feedback to find veer
            // use PID loop to get veer based on angle difference between current state and goal orientation

            double veerAmount = angular_loops.compute(goal_angle_difference, 0, id);

            Console.WriteLine("VEER AMOUNT: " + veerAmount);

            return addAngularVeer(regularSpeed, veerAmount);
        }

        /// <summary>
        /// Return wheel speeds based on default forward speeds with veer added
        /// </summary>
        /// <param name="veer"></param>
        /// <returns></returns>
        private WheelSpeeds addVeer(double veer, int straightSpeed)
        {
            // veer represents how much robot should veer to the left (by right speeds being faster)
            int leftspeed = transformToWheelSpeed(straightSpeed - veer);
            int rightspeed = transformToWheelSpeed(straightSpeed + veer);
            return new WheelSpeeds(leftspeed, rightspeed, leftspeed, rightspeed);
        }

        /// <summary>
        /// Given a number for the veer, get wheel speeds with veer added (veer represents
        /// how much robot is turning towards left)
        /// </summary>
        private WheelSpeeds addAngularVeer(WheelSpeeds speeds, double veer)
        {
            // Change every speed by veer amount
            int intVeer = (int)veer;
            WheelSpeeds newspeeds = new WheelSpeeds(speeds.lf - intVeer, speeds.rf + intVeer,
                                        speeds.lb - intVeer, speeds.rb + intVeer);
            return newspeeds;

        }

        /// <summary>
        /// Convert any given double into an int that is a valid wheel speed
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        private static int transformToWheelSpeed(double s)
        {
            // ensure number is an int and is between -127 and 127
            return (int)Math.Max(Math.Min(s, 127), -127);
        }

        /// <summary>
        /// Compute speeds to go at a particular angle, given the angle clockwise from to the robot's
        /// 0 angle
        /// </summary>
        /// <param name="velocity"></param>
        /// <param name="angle_goal"></param>
        /// <returns></returns>
        private static WheelSpeeds computeSpeedsAtAngle(double velocity, double angle_goal)
        {
            const double wheel_angle = Math.PI / 6;
            double sin_wheel_angle = Math.Sin(wheel_angle);
            double cos_wheel_angle = Math.Cos(wheel_angle);

            double sin_goal_angle = Math.Sin(angle_goal);
            double cos_goal_angle = Math.Cos(angle_goal);

            double magnitude_x_component = -velocity * sin_goal_angle / sin_wheel_angle;
            double magnitude_y_component = velocity * cos_goal_angle / cos_wheel_angle;

            int left_front, right_front, right_back, left_back;

            left_front = transformToWheelSpeed(magnitude_y_component + magnitude_x_component);
            right_front = transformToWheelSpeed(magnitude_y_component - magnitude_x_component);
            right_back = transformToWheelSpeed(magnitude_y_component + magnitude_x_component);
            left_back = transformToWheelSpeed(magnitude_y_component - magnitude_x_component);

            WheelSpeeds w = new WheelSpeeds(left_front, right_front, left_back, right_back);
            Console.WriteLine("velocity: " + Convert.ToString(velocity) + " angle: " + Convert.ToString(angle_goal) + " wheel speeds: " + w.toString());

            return w;
        }

        /// <summary>
        /// Sets currentGoalOrientation based on orientation relative to the goal orientation- if too far,
        /// reset
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="desiredState"></param>
        private void setForwardAngle(RobotInfo currentState, RobotInfo desiredState)
        {
            Vector2 dir_to_goal = desiredState.Position - currentState.Position;
            // if goal is too far, reset
            double orientation = currentState.Orientation;
            double absoluteGoalAngle = dir_to_goal.cartesianAngle();
            if (Math.Abs(UsefulFunctions.angleDifference(absoluteGoalAngle, currentForwardAngle + orientation)) > MIN_ANGLE_SWITCH)
            {
                currentForwardAngle = UsefulFunctions.angleDifference(orientation, absoluteGoalAngle);
            }
        }

        /// <summary>
        /// Call if close enough to goal to correct for orientation and stop when appropriate
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="desiredState"></param>
        /// <returns></returns>
        private WheelSpeeds stopSpeeds(RobotInfo currentState, RobotInfo desiredState)
        {
            // get angle difference between current and desired orientation
            double diff = UsefulFunctions.angleDifference(currentState.Orientation, desiredState.Orientation);
            // spin to face desired direction
            if (diff < -MAX_FINAL_ANGLE_DIFFERENCE)
            {
                // Go CW
                return CWSpeeds;
            }
            if (diff > MAX_FINAL_ANGLE_DIFFERENCE)
            {
                // Go CCW
                return CCWSpeeds;
            }
            // If it's close enough, just stop there
            return new WheelSpeeds();
        }
    }

    /// <summary>
    /// Contains position feedback driving system originally constructed for BugFeedbackMotionPlanner
    /// </summary>
    public class PositionFeedbackDriver : IPathDriver, ILogger {

        // Each robot has two feedback objects (for long and short distances). The differences are in the
        // setup for each object
        private Feedback[] feedbackObjs;
        public Feedback GetFeedbackObj(int robotID) { return feedbackObjs[robotID]; }

        private Feedback[] shortFeedbackObjs;
        public Feedback GetShortFeedbackObj(int robotID) { return shortFeedbackObjs[robotID]; }

        static int NUM_ROBOTS = ConstantsRaw.get<int>("default", "NUM_ROBOTS");       

        private double MIN_SQ_DIST_TO_WP;
        private double MIN_ANGLE_DIFF_TO_WP;
        private int LOG_EVERY_MSEC;

        public double PLANNER_WAYPOINT_DISTANCE;

        public PositionFeedbackDriver() {

            feedbackObjs = new Feedback[NUM_ROBOTS];
            shortFeedbackObjs = new Feedback[NUM_ROBOTS];

            for (int robotID = 0; robotID < NUM_ROBOTS; robotID++) {
                //DEFAULT: PID on position -> no feed-forward, uses desired instead (pre 2*.06.2009)
                // TODO: the mod by 5 is horrible, but the quickest to make simulation work
                feedbackObjs[robotID] = new Feedback(robotID % 5, "control", new FailSafeModel(robotID), true);
                //TEST: for long distance PID on velocity (x & y), feed-forward constant velocity
                //feedbackObjs[robotID] = new Feedback(robotID, "control-vel", new TestModel(robotID));
                shortFeedbackObjs[robotID] = new Feedback(robotID % 5, "control-short", new FailSafeModel(robotID), false);
            }

            ReloadConstants();
        }

        public WheelSpeeds followPath(RobotPath path, IPredictor predictor)
        {
            Team team = path.Team;
            int id = path.ID;
            RobotInfo desiredState = path.getFinalState();

            List<Object> itemsToLog = new List<Object>();

            RobotInfo curInfo;
            try 
            {
                curInfo = predictor.GetRobot(team, id);
            }
            catch (ApplicationException e) 
            {
                return new WheelSpeeds();
            }

            Vector2 pathWaypoint = path.findNearestWaypoint(curInfo).Position;
            RobotInfo nextWaypoint = new RobotInfo(pathWaypoint, desiredState.Orientation, curInfo.ID);

            double wpDistanceSq = curInfo.Position.distanceSq(nextWaypoint.Position);
            double angleDiff = Math.Abs(UsefulFunctions.angleDifference(curInfo.Orientation, nextWaypoint.Orientation));

            WheelSpeeds wheelSpeeds;

            if (wpDistanceSq > MIN_SQ_DIST_TO_WP || angleDiff > MIN_ANGLE_DIFF_TO_WP) {
                //If we are far enough from actual destination (carrot on a stick), 
                //calculate speeds using default feedback loop
                if (wpDistanceSq >= PLANNER_WAYPOINT_DISTANCE * PLANNER_WAYPOINT_DISTANCE) {
                    //Console.WriteLine("Planning long distance");
                    
                    // get wheel speeds from long range driver
                    List<RobotInfo> pathlst = new List<RobotInfo>();
                    pathlst.Add(nextWaypoint);

                    // TODO: What is _longRangeDriver? Doesn't compile
                    //wheelSpeeds = _longRangeDriver.followPath(new RobotPath(pathlst), predictor);
                    
                    // TODO: Uncommented this line instead of the above
                    wheelSpeeds = feedbackObjs[id].ComputeWheelSpeeds(curInfo, nextWaypoint);

                    //NOTE: This may look redundant, but the second feedback is also called (disregarding the result)
                    //to ensure that any state in the unused loop is properly updated. Hopefully this can ensure smoother
                    //transitions between the two types of motion. This line may be debatable -> TEST, TEST, TEST!!!
                    //shortFeedbackObjs[id].ComputeWheelSpeeds(curInfo, nextWaypoint);
                }
                //If we are close enough to actual destination, use another (hopefully more precise) feedback loop.
                else {
                    //Console.WriteLine("Planning short distance");
                    wheelSpeeds = shortFeedbackObjs[id].ComputeWheelSpeeds(curInfo, nextWaypoint);

                    //See NOTE above
                    //feedbackObjs[id].ComputeWheelSpeeds(curInfo, nextWaypoint);
                }
            }
            else {
                //Console.WriteLine("Close enough to point, stopping now.");
                wheelSpeeds = new WheelSpeeds();
            }

            #region Looging code
            itemsToLog.Add(DateTime.Now);
            itemsToLog.Add(curInfo);
            itemsToLog.Add(desiredState);          
            itemsToLog.Add(nextWaypoint);
            itemsToLog.Add(wheelSpeeds);
            itemsToLog.Add(path);

            DateTime now = DateTime.Now;
            TimeSpan timeSinceLastLog = now.Subtract(_lastLogEntry);
            if (_logging && timeSinceLastLog.TotalMilliseconds > LOG_EVERY_MSEC && id == _logRobotID) {
                _logWriter.LogItems(itemsToLog);
                _lastLogEntry = now;
            }
            #endregion

            return wheelSpeeds;
        }

        //reload all necessary constants from files, for now just PID reload
        public void ReloadConstants() {
            ConstantsRaw.Load();

            for (int robotID = 0; robotID < NUM_ROBOTS; robotID++) {
                feedbackObjs[robotID].ReloadConstants();
                shortFeedbackObjs[robotID].ReloadConstants();
            }

            LOG_EVERY_MSEC = ConstantsRaw.get<int>("control", "LOG_EVERY_MSEC");
            //PLANNER_WAYPOINT_DISTANCE = ConstantsRaw.get<double>("motionplanning", "PLANNER_WAYPOINT_DISTANCE");

            MIN_SQ_DIST_TO_WP = ConstantsRaw.get<double>("motionplanning", "MIN_SQ_DIST_TO_WP");
            MIN_ANGLE_DIFF_TO_WP = ConstantsRaw.get<double>("motionplanning", "MIN_ANGLE_DIFF_TO_WP");

            // TODO: What is _longRangeDriver? Doesn't compile!
            //_longRangeDriver.ReloadConstants();
        }

        public void UpdateConstants(int robotID, DOF_Constants newXY, DOF_Constants newTheta, bool isShort, bool save)
        {
            if (!isShort)
                feedbackObjs[robotID].UpdateConstants(newXY, newXY, newTheta, save);
            else
                shortFeedbackObjs[robotID].UpdateConstants(newXY, newXY, newTheta, save);
        }

        public void GetConstants(int robotID, bool isShort, out DOF_Constants newXY, out DOF_Constants newTheta)
        {
            DOF_Constants XPID, YPID, ThPID;
            
            if (!isShort)
                feedbackObjs[robotID].GetConstants(out XPID, out YPID, out ThPID);
            else
                shortFeedbackObjs[robotID].GetConstants(out XPID, out YPID, out ThPID);

            newXY = XPID;
            newTheta = ThPID;
        }
        #region ILogger

        private string _logFile = null;
        private bool _logging = false;
        private DateTime _lastLogEntry;
        private LogWriter _logWriter = new LogWriter();
        private int _logRobotID;

        public string LogFile {
            get { return _logFile; }
            set { _logFile = value; }
        }

        public bool Logging {
            get {
                return _logging;
            }
        }

        public void StartLogging(int robotID) {
            if (_logging)
                return;

            if (_logFile == null) {
                throw new ApplicationException("Logger: must set LogFile before calling start");
            }

            _logWriter.OpenLogFile(_logFile);
            _logging = true;
            _logRobotID = robotID;
        }

        public void StopLogging() {
            if (!_logging)
                return;

            _logWriter.CloseLogFile();
            _logging = false;
        }
        #endregion

    }

	public class ModelFeedbackDriver : IPathDriver, ILogger {
        static int NUM_ROBOTS = ConstantsRaw.get<int>("default", "NUM_ROBOTS");

		private ModelFeedback[] feedbackObjs;
		public ModelFeedback GetFeedbackObj(int robotID) { return feedbackObjs[robotID]; }

		private double MIN_SQ_DIST_TO_WP;
		private double MIN_ANGLE_DIFF_TO_WP;
	
		private int LOG_EVERY_MSEC;

		public ModelFeedbackDriver()
		{
			feedbackObjs = new ModelFeedback[NUM_ROBOTS];
			for (int robotID = 0; robotID < NUM_ROBOTS; robotID++)
                feedbackObjs[robotID] = new ModelFeedback();

			ReloadConstants();
		}

		public void ReloadConstants()
		{   
            for (int robotID = 0; robotID < NUM_ROBOTS; robotID++)
                feedbackObjs[robotID].LoadConstants();

			LOG_EVERY_MSEC = ConstantsRaw.get<int>("control", "LOG_EVERY_MSEC");			
			MIN_SQ_DIST_TO_WP = ConstantsRaw.get<double>("motionplanning", "MIN_SQ_DIST_TO_WP");
			MIN_ANGLE_DIFF_TO_WP = ConstantsRaw.get<double>("motionplanning", "MIN_ANGLE_DIFF_TO_WP");
		}

		public WheelSpeeds followPath(RobotPath path, IPredictor predictor)
		{
            Team team = path.Team;
			int id = path.ID;
			RobotInfo desiredState = path.getFinalState();

			RobotInfo curInfo;
			try
			{
				curInfo = predictor.GetRobot(team, id);
			}
			catch (ApplicationException e)
			{
                return new WheelSpeeds();
			}

			//Vector2 pathWaypoint = path.findNearestWaypoint(curInfo).Position;
			//RobotInfo nextWaypoint = new RobotInfo(pathWaypoint, desiredState.Orientation, curInfo.ID);

			RobotInfo nextWaypoint = path.findNearestWaypoint(curInfo);

			double wpDistanceSq = curInfo.Position.distanceSq(nextWaypoint.Position);
			double angleDiff = Math.Abs(UsefulFunctions.angleDifference(curInfo.Orientation, nextWaypoint.Orientation));

			WheelSpeeds wheelSpeeds;

			//Note: ModelFeeback cares about desired velocities!!!
			//We should make sure the planner returns a plan with velocities set accordingly
			
			if (wpDistanceSq > MIN_SQ_DIST_TO_WP || angleDiff > MIN_ANGLE_DIFF_TO_WP)
			{
				wheelSpeeds = feedbackObjs[id].ComputeWheelSpeeds(curInfo, nextWaypoint);				
			}
			else
			{
				//Console.WriteLine("Close enough to point, stopping now.");
				wheelSpeeds = new WheelSpeeds();
			}



			#region Looging code
			List<Object> itemsToLog = new List<Object>();
			itemsToLog.Add(DateTime.Now);
			itemsToLog.Add(curInfo);
			itemsToLog.Add(desiredState);
			itemsToLog.Add(nextWaypoint);
			itemsToLog.Add(wheelSpeeds);
			itemsToLog.Add(path);

			DateTime now = DateTime.Now;
			TimeSpan timeSinceLastLog = now.Subtract(_lastLogEntry);
			if (_logging && timeSinceLastLog.TotalMilliseconds > LOG_EVERY_MSEC && id == _logRobotID)
			{
				_logWriter.LogItems(itemsToLog);
				_lastLogEntry = now;
			}
			#endregion

			return wheelSpeeds;
		}

		#region ILogger
		private string _logFile = null;
		private bool _logging = false;
		private DateTime _lastLogEntry;
		private LogWriter _logWriter = new LogWriter();
		private int _logRobotID;

		public string LogFile
		{
			get { return _logFile; }
			set { _logFile = value; }
		}

		public bool Logging
		{
			get
			{
				return _logging;
			}
		}

		public void StartLogging(int robotID)
		{
			if (_logging)
				return;

			if (_logFile == null)
			{
				throw new ApplicationException("Logger: must set LogFile before calling start");
			}

			_logWriter.OpenLogFile(_logFile);
			_logging = true;
			_logRobotID = robotID;
		}

		public void StopLogging()
		{
			if (!_logging)
				return;

			_logWriter.CloseLogFile();
			_logging = false;
		}
		#endregion
	}
}
