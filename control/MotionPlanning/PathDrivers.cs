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
    /// </summary>
    public class ExtenderDriver : IPathDriver
    {
        double STOP_DISTANCE;

        public WheelSpeeds followPath(RobotPath path, IPredictor predictor)
        {
            // get parameters from input
            int id = path.ID;
            RobotInfo currentState = predictor.getCurrentInformation(id);

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
            Constants.Load("motionplanning");
            STOP_DISTANCE = Constants.get<double>("motionplanning", "STOP_DISTANCE");
        }
    }

    /// <summary>
    /// Planner that goes to a point by first turning to face it and then
    /// moving forward
    /// </summary>
    public class TurnDriver : IPathDriver
    {
        //Constants
        static int WHEEL_SPEED_STRAIGHT = Constants.get<int>("motionplanning", "WHEEL_SPEED_STRAIGHT");
        static int WHEEL_SPEED_TURN = Constants.get<int>("motionplanning", "WHEEL_SPEED_TURN");
        double MIN_ANGLE_DIFFERENCE = Constants.get<double>("motionplanning", "MIN_ANGLE_DIFFERENCE");
        double STOP_DISTANCE = Constants.get<double>("motionplanning", "STOP_DISTANCE");

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

            WHEEL_SPEED_STRAIGHT = Constants.get<int>("motionplanning", "WHEEL_SPEED_STRAIGHT");
            WHEEL_SPEED_TURN = Constants.get<int>("motionplanning", "WHEEL_SPEED_TURN");

            forwardSpeeds = new WheelSpeeds(WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT);
            CWSpeeds = new WheelSpeeds(WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN, WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN);
            CCWSpeeds = new WheelSpeeds(-WHEEL_SPEED_TURN, WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN, WHEEL_SPEED_TURN);

            // Distance from goal at which the robot will stop
            STOP_DISTANCE = Constants.get<double>("motionplanning", "STOP_DISTANCE");

            //Distance above which will turn
            MIN_ANGLE_DIFFERENCE = Constants.get<double>("motionplanning", "MIN_ANGLE_DIFFERENCE");
            Console.WriteLine("RELOADING CONSTANTS!!! " + STOP_DISTANCE + " " + MIN_ANGLE_DIFFERENCE);
        }

        /// <summary>
        /// Plan motion using straightforward WheelSpeedsExtender
        /// </summary>
        public WheelSpeeds followPath(RobotPath path, IPredictor predictor)
        {
            int id = path.ID;
            RobotInfo currentState = predictor.getCurrentInformation(id);
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

        static int NUM_ROBOTS = Constants.get<int>("motionplanning", "NUM_ROBOTS");

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

        double LOOK_AHEAD_DISTANCE;

        bool called = false;

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
            Constants.Load("motionplanning");

            // Either 0 or 1, represents whether to use the angular veer planner
            ANGULAR_VEER = Constants.get<int>("motionplanning", "ANGULAR_VEER");

            // Speed of a single wheel

            WHEEL_SPEED_STRAIGHT = Constants.get<int>("motionplanning", "WHEEL_SPEED_STRAIGHT");
            WHEEL_SPEED_TURN = Constants.get<int>("motionplanning", "WHEEL_SPEED_TURN");

            forwardSpeeds = new WheelSpeeds(WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT);
            CWSpeeds = new WheelSpeeds(WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN, WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN);
            CCWSpeeds = new WheelSpeeds(-WHEEL_SPEED_TURN, WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN, WHEEL_SPEED_TURN);

            // Distance from goal at which the robot will stop
            STOP_DISTANCE = Constants.get<double>("motionplanning", "STOP_DISTANCE");

            //Distance above which will turn
            MIN_ANGLE_SPIN = Constants.get<double>("motionplanning", "MIN_ANGLE_SPIN");

            // Distance above which, if already spinning, will keep turning
            MIN_ANGLE_KEEP_SPINNING = Constants.get<double>("motionplanning", "MIN_ANGLE_KEEP_SPINNING");

            //Maximum final angle difference tolerated
            MAX_FINAL_ANGLE_DIFFERENCE = Constants.get<double>("motionplanning", "MAX_FINAL_ANGLE_DIFFERENCE");

            DIST_SLOW_DOWN = Constants.get<double>("motionplanning", "DIST_SLOW_DOWN");

            /* Wheel orientations
            double RF_ORIENTATION = Constants.get<double>("motionplanning", "RF_ORIENTATION");
            double LF_ORIENTATION = Constants.get<double>("motionplanning", "LF_ORIENTATION");
            double LB_ORIENTATION = Constants.get<double>("motionplanning", "LB_ORIENTATION");
            double RB_ORIENTATION = Constants.get<double>("motionplanning", "RB_ORIENTATION");*/

            // robot composition
            ANGLE_AXIS_TO_WHEEL = Constants.get<double>("motionplanning", "ANGLE_AXIS_TO_WHEEL");
            WHEEL_RADIUS = Constants.get<double>("motionplanning", "WHEEL_RADIUS");

            MIN_ANGLE_SWITCH = Constants.get<double>("motionplanning", "MIN_ANGLE_SWITCH");

            // Reload PID constants
            loops.ReloadConstants();
            angular_loops.ReloadConstants();
        }

        /// <summary>
        /// Draw last path- currently draws nothing (as this is a dumb planner, there
        /// is no path)
        /// </summary>
        /// <param name="g"></param>
        /// <param name="c"></param>
        public void DrawLast(System.Drawing.Graphics g, ICoordinateConverter c)
        {
        }

        /// <summary>
        /// Plan motion using PID loop to control veer
        /// </summary>
        public WheelSpeeds followPath(RobotPath path, IPredictor predictor)
        {
            RobotInfo currentState = predictor.getCurrentInformation(path.ID);

            lastWayPoint = path.findNearestWaypoint(currentState).Position;
            RobotInfo waypointState = new RobotInfo(lastWayPoint, 0, path.ID);

            // Remember that this has been called
            called = true;

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

            double goal_angle_difference = UsefulFunctions.angleDifference(absoluteGoalAngle, currentForwardAngle+orientation);

            /*Console.WriteLine("ORIENTATION: " + orientation);
            Console.WriteLine("CURRENT FORWARD ANGLE: " + currentForwardAngle);
            Console.WriteLine("ABSOLUTE GOAL ANGLE: " + absoluteGoalAngle);
            Console.WriteLine("GOAL ANGLE DIFFERENCE: " + goal_angle_difference);
             */

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

        static int NUM_ROBOTS = Constants.get<int>("motionplanning", "NUM_ROBOTS");

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

        double LOOK_AHEAD_DISTANCE;

        bool called = false;

        double ANGLE_AXIS_TO_WHEEL;
        WheelSpeeds forwardSpeeds;
        WheelSpeeds CWSpeeds;
        WheelSpeeds CCWSpeeds;

        // PID loops
        MultiRobotPIDLoop loops = new MultiRobotPIDLoop("motionplanning", "VEER", NUM_ROBOTS);
        MultiRobotPIDLoop angular_loops = new MultiRobotPIDLoop("motionplanning", "ANGULAR_VEER", NUM_ROBOTS);

        bool alreadySpinning = false;

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
            Constants.Load("motionplanning");

            // Either 0 or 1, represents whether to use the angular veer planner
            ANGULAR_VEER = Constants.get<int>("motionplanning", "ANGULAR_VEER");

            // Speed of a single wheel

            WHEEL_SPEED_STRAIGHT = Constants.get<int>("motionplanning", "WHEEL_SPEED_STRAIGHT");
            WHEEL_SPEED_TURN = Constants.get<int>("motionplanning", "WHEEL_SPEED_TURN");

            forwardSpeeds = new WheelSpeeds(WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT);
            CWSpeeds = new WheelSpeeds(WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN, WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN);
            CCWSpeeds = new WheelSpeeds(-WHEEL_SPEED_TURN, WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN, WHEEL_SPEED_TURN);

            // Distance from goal at which the robot will stop
            STOP_DISTANCE = Constants.get<double>("motionplanning", "STOP_DISTANCE");

            //Distance above which will turn
            MIN_ANGLE_SPIN = Constants.get<double>("motionplanning", "MIN_ANGLE_SPIN");

            // Distance above which, if already spinning, will keep turning
            MIN_ANGLE_KEEP_SPINNING = Constants.get<double>("motionplanning", "MIN_ANGLE_KEEP_SPINNING");

            //Maximum final angle difference tolerated
            MAX_FINAL_ANGLE_DIFFERENCE = Constants.get<double>("motionplanning", "MAX_FINAL_ANGLE_DIFFERENCE");

            DIST_SLOW_DOWN = Constants.get<double>("motionplanning", "DIST_SLOW_DOWN");

            /* Wheel orientations
            double RF_ORIENTATION = Constants.get<double>("motionplanning", "RF_ORIENTATION");
            double LF_ORIENTATION = Constants.get<double>("motionplanning", "LF_ORIENTATION");
            double LB_ORIENTATION = Constants.get<double>("motionplanning", "LB_ORIENTATION");
            double RB_ORIENTATION = Constants.get<double>("motionplanning", "RB_ORIENTATION");*/

            // robot composition
            ANGLE_AXIS_TO_WHEEL = Constants.get<double>("motionplanning", "ANGLE_AXIS_TO_WHEEL");
            WHEEL_RADIUS = Constants.get<double>("motionplanning", "WHEEL_RADIUS");

            MIN_ANGLE_SWITCH = Constants.get<double>("motionplanning", "MIN_ANGLE_SWITCH");

            // Reload PID constants
            loops.ReloadConstants();
            angular_loops.ReloadConstants();
        }

        /// <summary>
        /// Draw last path- currently draws nothing (as this is a dumb planner, there
        /// is no path)
        /// </summary>
        /// <param name="g"></param>
        /// <param name="c"></param>
        public void DrawLast(System.Drawing.Graphics g, ICoordinateConverter c)
        {
        }

        /// <summary>
        /// Plan motion using PID loop to control veer
        /// </summary>
        public WheelSpeeds followPath(RobotPath path, IPredictor predictor)
        {
            RobotInfo currentState = predictor.getCurrentInformation(path.ID);

            lastWayPoint = path.findNearestWaypoint(currentState).Position;
            RobotInfo waypointState = new RobotInfo(lastWayPoint, 0, path.ID);

            // Remember that this has been called
            called = true;

            Console.WriteLine("WAYPOINT: " + lastWayPoint.ToString());

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

        static int NUM_ROBOTS = Constants.get<int>("motionplanning", "NUM_ROBOTS");

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

        double LOOK_AHEAD_DISTANCE;

        bool called = false;

        double ANGLE_AXIS_TO_WHEEL;
        WheelSpeeds forwardSpeeds;
        WheelSpeeds CWSpeeds;
        WheelSpeeds CCWSpeeds;

        // PID loops
        MultiRobotPIDLoop loops = new MultiRobotPIDLoop("motionplanning", "DEFAULT", NUM_ROBOTS);
        MultiRobotPIDLoop angular_loops = new MultiRobotPIDLoop("motionplanning", "DEFAULT", NUM_ROBOTS);

        bool alreadySpinning = false;

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
            Constants.Load("motionplanning");

            // Either 0 or 1, represents whether to use the angular veer planner
            //ANGULAR_VEER = Constants.get<int>("motionplanning", "ANGULAR_VEER");
            ANGULAR_VEER = 0;

            // Speed of a single wheel

            WHEEL_SPEED_STRAIGHT = Constants.get<int>("motionplanning", "DEFAULT_WHEEL_SPEED_STRAIGHT");
            WHEEL_SPEED_TURN = Constants.get<int>("motionplanning", "DEFAULT_WHEEL_SPEED_TURN");

            forwardSpeeds = new WheelSpeeds(WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT);
            CWSpeeds = new WheelSpeeds(WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN, WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN);
            CCWSpeeds = new WheelSpeeds(-WHEEL_SPEED_TURN, WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN, WHEEL_SPEED_TURN);

            // Distance from goal at which the robot will stop
            STOP_DISTANCE = Constants.get<double>("motionplanning", "DEFAULT_STOP_DISTANCE");

            //Distance above which will turn
            MIN_ANGLE_SPIN = Constants.get<double>("motionplanning", "MIN_ANGLE_SPIN");

            // Distance above which, if already spinning, will keep turning
            MIN_ANGLE_KEEP_SPINNING = Constants.get<double>("motionplanning", "MIN_ANGLE_KEEP_SPINNING");

            //Maximum final angle difference tolerated
            MAX_FINAL_ANGLE_DIFFERENCE = Constants.get<double>("motionplanning", "MAX_FINAL_ANGLE_DIFFERENCE");

            DIST_SLOW_DOWN = Constants.get<double>("motionplanning", "DEFAULT_DIST_SLOW_DOWN");

            /* Wheel orientations
            double RF_ORIENTATION = Constants.get<double>("motionplanning", "RF_ORIENTATION");
            double LF_ORIENTATION = Constants.get<double>("motionplanning", "LF_ORIENTATION");
            double LB_ORIENTATION = Constants.get<double>("motionplanning", "LB_ORIENTATION");
            double RB_ORIENTATION = Constants.get<double>("motionplanning", "RB_ORIENTATION");*/

            // robot composition
            ANGLE_AXIS_TO_WHEEL = Constants.get<double>("motionplanning", "ANGLE_AXIS_TO_WHEEL");
            WHEEL_RADIUS = Constants.get<double>("motionplanning", "WHEEL_RADIUS");

            MIN_ANGLE_SWITCH = Constants.get<double>("motionplanning", "MIN_ANGLE_SWITCH");

            // Reload PID constants
            loops.ReloadConstants();
            angular_loops.ReloadConstants();
        }

        /// <summary>
        /// Draw last path- currently draws nothing (as this is a dumb planner, there
        /// is no path)
        /// </summary>
        /// <param name="g"></param>
        /// <param name="c"></param>
        public void DrawLast(System.Drawing.Graphics g, ICoordinateConverter c)
        {
        }

        /// <summary>
        /// Plan motion using PID loop to control veer
        /// </summary>
        public WheelSpeeds followPath(RobotPath path, IPredictor predictor)
        {
            RobotInfo currentState = predictor.getCurrentInformation(path.ID);

            lastWayPoint = path.findNearestWaypoint(currentState).Position;
            RobotInfo waypointState = new RobotInfo(lastWayPoint, 0, path.ID);

            // Remember that this has been called
            called = true;

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
}
