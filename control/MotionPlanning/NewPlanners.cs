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

using Navigator = Navigation.Examples.LookAheadPotential;

namespace Robocup.MotionControl
{
    //contains planners written, both for testing and practical purposes, for
    //the spring 2009 CS 199r class

    //To be distinguished from RRTPlanners.cs

#if false
    /// <summary>
    /// Give velocities towards a point with no consideration of obstacles and
    /// no use of PID feedback, RRT planning, or other intelligent paths- meant
    /// for testing WheelSpeedsExtender
    /// </summary>
    public class DumbPlanner : IMotionPlanner
    {
        //Distance to stop
        double STOP_DISTANCE = 0.1;

        // Dummy methods- required for interface
        public void ReloadConstants() { }

        public void DrawLast(System.Drawing.Graphics g, ICoordinateConverter c) { }

        /// <summary>
        /// Plan motion using straightforward WheelSpeedsExtender
        /// </summary>
        public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState,
            IPredictor predictor, double avoidBallRadius)
        {
            RobotInfo currentState = predictor.getCurrentInformation(id);

            // speeds default to zero
            WheelSpeeds speeds = new WheelSpeeds();

            // if farther away than stopping distance, get speesd to point
            if (currentState.Position.distanceSq(desiredState.Position) >
                STOP_DISTANCE * STOP_DISTANCE)
            {
                speeds = WheelSpeedsExtender.GetWheelSpeedsThrough(currentState, desiredState);
            }

            Console.WriteLine(" LF: " + speeds.lf.ToString() + " RF: " + speeds.rf.ToString() +
                " LB: " + speeds.lb.ToString() + " RB: " + speeds.rb.ToString());

            return new MotionPlanningResults(speeds);
        }
    }
    /// <summary>
    /// Head towards goal, veering based on orientation towards goal
    /// IN PROGRESS- do not use
    /// </summary>
    public class DumbVeerPlanner : IMotionPlanner
    {
        //Constants
        static int WHEEL_SPEED_STRAIGHT = Constants.get<int>("motionplanning", "WHEEL_SPEED_STRAIGHT");
        static int WHEEL_SPEED_TURN = Constants.get<int>("motionplanning", "WHEEL_SPEED_TURN");
        double MIN_ANGLE_VEER_DIFFERENCE = Constants.get<double>("motionplanning", "MIN_ANGLE_VEER_DIFFERENCE");
        double STOP_DISTANCE = Constants.get<double>("motionplanning", "STOP_DISTANCE");

        // for every radian off, how much must the wheels veer
        double VEER_RATE = 10;

        WheelSpeeds forwardSpeeds = new WheelSpeeds(WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT);
        WheelSpeeds CWSpeeds = new WheelSpeeds(WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN, WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN);
        WheelSpeeds CCWSpeeds = new WheelSpeeds(-WHEEL_SPEED_TURN, WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN, WHEEL_SPEED_TURN);

        // Dummy methods- required for interface
        // TODO: add 
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

        public void DrawLast(System.Drawing.Graphics g, ICoordinateConverter c) { }

        /// <summary>
        /// Plan motion using straightforward WheelSpeedsExtender
        /// </summary>
        public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState,
            IPredictor predictor, double avoidBallRadius)
        {
            RobotInfo currentState = predictor.getCurrentInformation(id);

            // speeds default to zero
            WheelSpeeds speeds = new WheelSpeeds();

            // find direction to point
            Vector2 dir_to_goal = (desiredState.Position - currentState.Position).normalize();
            double angle_to_goal = dir_to_goal.cartesianAngle();
            double angle_diff = currentState.Orientation - angle_to_goal;

            double sqDistToGoal = currentState.Position.distanceSq(desiredState.Position);

            //If close enough, return speeds of zero
            if (sqDistToGoal < STOP_DISTANCE * STOP_DISTANCE)
            {
                return new MotionPlanningResults(speeds);
            }

            // If pointing in right direction, go forward
            if (Math.Abs(angle_diff) < MIN_ANGLE_VEER_DIFFERENCE)
            {
                speeds = getVeer(angle_diff, sqDistToGoal);
                Console.WriteLine("Going forward: " + speeds.toString());
            }

            // if angle is too far off, go CW or CCW
            else if (angle_diff >= MIN_ANGLE_VEER_DIFFERENCE)
            {
                Console.WriteLine("Going CW: " + CWSpeeds.toString());
                speeds = CWSpeeds;
            }

            else if (angle_diff <= -MIN_ANGLE_VEER_DIFFERENCE)
            {
                Console.WriteLine("Going CCW: " + CCWSpeeds.toString());
                speeds = CCWSpeeds;
            }

            // otherwise, return speeds as set by veering
            speeds = getVeer(angle_diff, sqDistToGoal);

            //print speeds that are set
            Console.WriteLine(speeds.toString());
            Console.WriteLine(angle_diff.ToString());
            Console.WriteLine("Distance to goal squared: " +
                currentState.Position.distanceSq(desiredState.Position).ToString());

            // return speeds as set
            return new MotionPlanningResults(speeds);
        }

        /// <summary>
        /// Given the difference between the current angle and the goal, as well
        /// as the distance to the goal, return wheel speeds
        /// </summary>
        /// <param name="angle_difference"></param>
        /// <returns></returns>
        private WheelSpeeds getVeer(double angle_difference, double sqDistToGoal) {
            // for now, increase veer linearly with difference from goal orientation
            veerAmount = Math.Floor(angle_difference * VEER_RATE);
            return addVeer(veerAmount);
        }

        /// <summary>
        /// Return wheel speeds based on default forward speeds with veer added
        /// </summary>
        /// <param name="veer"></param>
        /// <returns></returns>
        private WheelSpeeds addVeer(double veer)
        {
            // veer represents how much higher left speeds are than right
            return new WheelSpeeds(WHEEL_SPEED_STRAIGHT + veer, WHEEL_SPEED_STRAIGHT - veer,
                WHEEL_SPEED_STRAIGHT + veer, WHEEL_SPEED_STRAIGHT - veer);
        }
    }

    /// <summary>
    /// Goes straight towards goal, uses a PID loop to correct for orientation. Can either go
    /// foward towards goal or at any angle, in which case uses velocity rather than orientation
    /// to correct. Is decided by constant ANGULAR_VEER in motionplanning constants.
    /// </summary>
    public class DumbFeedbackVeerPlanner : IMotionPlanner
    {
        //Constants
        /*
         This should now be obsolete...
        static int NUM_ROBOTS = Constants.get<int>("motionplanning", "NUM_ROBOTS");

        static int WHEEL_SPEED_STRAIGHT = Constants.get<int>("motionplanning", "WHEEL_SPEED_STRAIGHT");
        static int WHEEL_SPEED_TURN = Constants.get<int>("motionplanning", "WHEEL_SPEED_TURN");
        double MIN_ANGLE_SPIN = Constants.get<double>("motionplanning", "MIN_ANGLE_SPIN");
        double STOP_DISTANCE = Constants.get<double>("motionplanning", "STOP_DISTANCE");

        WheelSpeeds forwardSpeeds = new WheelSpeeds(WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT, WHEEL_SPEED_STRAIGHT);
        WheelSpeeds CWSpeeds = new WheelSpeeds(WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN, WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN);
        WheelSpeeds CCWSpeeds = new WheelSpeeds(-WHEEL_SPEED_TURN, WHEEL_SPEED_TURN, -WHEEL_SPEED_TURN, WHEEL_SPEED_TURN);
        */
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

        double MPS_STRAIGHT_SPEED;

        /*Wheel orientations
        double RF_orientation;
        double LF_orientation;
        double LB_orientation;
        double RB_orientation;*/

        double LOOK_AHEAD_DISTANCE;

        double ANGLE_AXIS_TO_WHEEL;

        // Navigation constants
        Navigator navigator;

        Vector2 lastWayPoint;
        bool called;
        int USE_NAVIGATOR;
        double AVOID_ROBOT_DISTANCE;

        WheelSpeeds forwardSpeeds;
        WheelSpeeds CWSpeeds;
        WheelSpeeds CCWSpeeds;
        
        // PID loops
        MultiRobotPIDLoop loops = new MultiRobotPIDLoop("motionplanning", "VEER", NUM_ROBOTS);
        MultiRobotPIDLoop angular_loops = new MultiRobotPIDLoop("motionplanning", "ANGULAR_VEER", NUM_ROBOTS);

        bool alreadySpinning = false;

        public DumbFeedbackVeerPlanner() {
            // load initial values of constants
            navigator = new Navigator();

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

            USE_NAVIGATOR = Constants.get<int>("motionplanning", "USE_NAVIGATOR");

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

            MPS_STRAIGHT_SPEED = Constants.get<double>("motionplanning", "MPS_STRAIGHT_SPEED");

            LOOK_AHEAD_DISTANCE = Constants.get<double>("motionplanning", "LOOK_AHEAD_DISTANCE");
            AVOID_ROBOT_DISTANCE = Constants.get<double>("motionplanning", "AVOID_ROBOT_DISTANCE");

            // Set navigator constants
            //navigator.setLookAheadDist(LOOK_AHEAD_DISTANCE);
            //navigator.setAvoidRobotDist(AVOID_ROBOT_DISTANCE);

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
        public void DrawLast(System.Drawing.Graphics g, ICoordinateConverter c) {
            // if PlanMotion has been called, draw point
            if (called && USE_NAVIGATOR == 1)
            {
                Brush b = new SolidBrush(Color.Blue);
                Rectangle r = new Rectangle(new Point(c.fieldtopixelX(lastWayPoint.X), c.fieldtopixelY(lastWayPoint.Y)),
                                        new Size(10, 10));
                g.FillRectangle(b, r);
                b.Dispose();
            }
        }

        /// <summary>
        /// Plan motion using PID loop to control veer
        /// </summary>
        public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState,
            IPredictor predictor, double avoidBallRadius)
        {
            RobotInfo currentState = predictor.getCurrentInformation(id);

            NavigationResults results = navigator.navigate(currentState.ID, currentState.Position,
                desiredState.Position, predictor.getOurTeamInfo().ToArray(), predictor.getTheirTeamInfo().ToArray(), predictor.getBallInfo(),
                LOOK_AHEAD_DISTANCE);

            lastWayPoint = results.waypoint;
            RobotInfo waypointState = new RobotInfo(lastWayPoint, 0, id);

            if (USE_NAVIGATOR == 0)
            {
                waypointState = desiredState;
            }

            // Remember that this has been called
            called = true;

            Console.WriteLine("WAYPOINT: " + lastWayPoint.ToString());

            WheelSpeeds speeds;
            //get wheel speeds using appropriate method
            if (ANGULAR_VEER == 1) {
                speeds = angularMethod(currentState, waypointState);
            }
            else {
                speeds = orientationMethod(currentState, waypointState);
            }

            //print speeds that are set
            //Console.WriteLine(speeds.toString());
            //Console.WriteLine(angle_diff.ToString());
            //Console.WriteLine("Distance to goal squared: " + sqDistToGoal);

            // return speeds as set
            return new MotionPlanningResults(speeds);
        }

        /// <summary>
        /// get wheel speeds using orientation method
        /// </summary>
        /// <param name="?"></param>
        /// <param name="?"></param>
        /// <returns></returns>
        private WheelSpeeds orientationMethod(RobotInfo currentState, RobotInfo desiredState) {
            // speeds default to zero
            WheelSpeeds speeds = new WheelSpeeds();

            // find direction to point
            //Vector2 dir_to_goal = (goal_vector - currentState.Position).normalize();
            int id = currentState.ID;
            Vector2 dir_to_goal = (desiredState.Position - currentState.Position).normalize();
            double angle_to_goal = dir_to_goal.cartesianAngle();
            double angle_diff = angle_difference(currentState.Orientation, angle_to_goal);
            double sqDistToGoal = currentState.Position.distanceSq(desiredState.Position);

            Console.WriteLine("ORIENTATION " + currentState.Orientation);
            Console.WriteLine("ANGLE DIFFERENCE " + angle_diff);

            //If close enough, spin to face correct orientation and stop
            if (sqDistToGoal < STOP_DISTANCE * STOP_DISTANCE)
            {
                // get angle difference between current and desired orientation
                double diff = angle_difference(currentState.Orientation, desiredState.Orientation);
                // spin to face desired direction
                if (diff > MAX_FINAL_ANGLE_DIFFERENCE)
                {
                    // Go CW
                    return CWSpeeds;
                }
                if (diff < -MAX_FINAL_ANGLE_DIFFERENCE)
                {
                    // Go CCW
                    return CCWSpeeds;
                }
                // If it's close enough, just stop there
                return speeds;
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

        private WheelSpeeds angularMethod(RobotInfo currentState, RobotInfo desiredState)
        {
            // speeds default to zero
            WheelSpeeds speeds = new WheelSpeeds();

            // find direction to point
            //Vector2 dir_to_goal = (goal_vector - currentState.Position).normalize();
            Vector2 dir_to_goal = (desiredState.Position - currentState.Position).normalize();
            double angle_to_goal = dir_to_goal.cartesianAngle();
            double angle_diff = angle_difference(currentState.Orientation, angle_to_goal);
            double sqDistToGoal = currentState.Position.distanceSq(desiredState.Position);

            if (sqDistToGoal < STOP_DISTANCE * STOP_DISTANCE)
            {
                // get angle difference between current and desired orientation
                double diff = angle_difference(currentState.Orientation, desiredState.Orientation);
                // spin to face desired direction
                if (diff > MAX_FINAL_ANGLE_DIFFERENCE)
                {
                    // Go CW
                    return CWSpeeds;
                }
                if (diff < -MAX_FINAL_ANGLE_DIFFERENCE)
                {
                    // Go CCW
                    return CCWSpeeds;
                }
                // If it's close enough, just stop there
                return speeds;
            }

            // otherwise, go in direction with appropriate veer
            speeds = getAngularVeer(currentState, dir_to_goal, sqDistToGoal, currentState.ID);
        
            Console.WriteLine("Final speeds " + speeds.toString());

            return speeds;
        }

        /// <summary>
        /// Given the difference between the current angle and the goal, as well
        /// as the distance to the goal, return wheel speeds
        /// </summary>
        /// <param name="angle_difference"></param>
        /// <returns></returns>
        private WheelSpeeds getVeer(double angle_difference, double sqDistToGoal, int id) {
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
        private WheelSpeeds getAngularVeer(RobotInfo currentState, Vector2 dirGoal, double sqDistToGoal, int id)
        {
            // find velocity angle difference
            double orientation = currentState.Orientation;
            double angle_difference = UsefulFunctions.angleDifference(currentState.Velocity.cartesianAngle(),
                dirGoal.cartesianAngle());

            // use PID loop to get veer based on angle difference
            double veerAmount = angular_loops.compute(angle_difference, 0, id);

            double straightSpeed = MPS_STRAIGHT_SPEED;

            // If too close, slow down
            if (sqDistToGoal < DIST_SLOW_DOWN * DIST_SLOW_DOWN)
            {
                // Make speed proportionate to distance to goal
                straightSpeed = transformToWheelSpeed(straightSpeed * (Math.Sqrt(sqDistToGoal) / DIST_SLOW_DOWN));
                //Console.WriteLine("Slowing down from " + WHEEL_SPEED_STRAIGHT.ToString() + " to " + straightSpeed.ToString());
            }

            return addAngularVeer(veerAmount, straightSpeed, dirGoal, orientation);
        }

        /// <summary>
        /// Return wheel speeds based on default forward speeds with veer added
        /// </summary>
        /// <param name="veer"></param>
        /// <returns></returns>
        private WheelSpeeds addVeer(double veer, int straightSpeed)
        {
            // veer represents how much higher right speeds are than left
            int leftspeed = transformToWheelSpeed(straightSpeed + veer);
            int rightspeed = transformToWheelSpeed(straightSpeed - veer);
            return new WheelSpeeds(leftspeed, rightspeed, leftspeed, rightspeed);
        }

        /// <summary>
        /// Given a number for the veer, a default speed for a single wheel, the direction to
        /// the goal, and the orientation of the robot, get wheel speeds with veer added
        /// </summary>
        /// <param name="veer"></param>
        /// <param name="straightSpeed"></param>
        /// <param name="dirGoal"></param>
        /// <param name="orientation"></param>
        /// <returns></returns>
        private WheelSpeeds addAngularVeer(double veer, double straightSpeed, Vector2 dirGoal,
            double orientation)
        {
            // compute speeds with veer changing direction of goal
            Console.WriteLine("Veer: " + veer.ToString() + " dirgoal: " + dirGoal.ToString());
            return convert(straightSpeed, dirGoal.cartesianAngle()+veer, 0, orientation);
        }

        /// <summary>
        /// Convert any given double into an int that is a valid wheel speed
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        private int transformToWheelSpeed(double s) {
            // ensure number is an int and is between -127 and 127
            return (int) Math.Max(Math.Min(s, 127), -127);
        }

        /// <summary>
        /// Given two angles, return the difference (angle1-angle2), adjusting
        /// for correct orientation
        /// </summary>
        /// <returns></returns>
        private double angle_difference(double angle1, double angle2)
        {
            return UsefulFunctions.angleDifference(angle2, angle1);
        }

        /// <summary>
        /// Convert x and y commands, plus angular velocity, into wheel speeds. Adapted from Feedback class.
        /// </summary>
        /// <returns></returns>
        private WheelSpeeds convert(double velocity, double angle_goal, double angularV, double theta)
        {
            // Find x and y commands
            double xCommand = velocity * Math.Cos(angle_goal);
            double yCommand = velocity * Math.Sin(angle_goal);

            //I assume the x command is effectively in m/s, so r the radius of the wheels from the center of
            //the robot is in meters

            //change from the x and y of the field to forward and lateral(right is positive) used below
            double forward = Math.Cos(theta) * xCommand + Math.Sin(theta) * yCommand;
            double lateral = Math.Sin(theta) * xCommand - Math.Cos(theta) * yCommand;

            //Console.WriteLine(lateral.ToString() + " lateral|Forward: " + forward.ToString());

            //computed here to save typing, since used 4 times
            double sing = Math.Sin(ANGLE_AXIS_TO_WHEEL);
            double cosg = Math.Cos(ANGLE_AXIS_TO_WHEEL);

            //wheel one is the front right wheel  wheel 2 is the back right wheel, and so on around the the robot clockwise
            //make sure each is a safe wheel speed

            int lf = transformToWheelSpeed(sing * lateral + cosg * forward - WHEEL_RADIUS * angularV);
            int rf = transformToWheelSpeed(-(sing * lateral - cosg * forward - WHEEL_RADIUS * angularV));
            int lb = transformToWheelSpeed(-sing * lateral + cosg * forward - WHEEL_RADIUS * angularV);
            int rb = transformToWheelSpeed(-(-sing * lateral - cosg * forward - WHEEL_RADIUS * angularV));

            return new WheelSpeeds(lf, rf, lb, rb);
            //Note somewhere we need to check and ensure that wheel speeds being 
            //sent do not exceed maximum values allowed by the protocol.
        }

    }

    public class DumbVelocityPlanner : IMotionPlanner
    {

        double velocity;
        double VEER_CAP;

        MultiRobotPIDLoop loops = new MultiRobotPIDLoop("motionplanning", "VELOCITY_VEER", 5);

        public DumbVelocityPlanner()
        {
            ReloadConstants();
        }

        public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius)
        {
            RobotInfo currentState = predictor.getCurrentInformation(id);

            // Find waypoint desired state


            Vector2 dir_to_goal = (desiredState.Position - currentState.Position);

            double angle_to_goal = UsefulFunctions.angleDifference(currentState.Orientation, dir_to_goal.cartesianAngle());

            double velocity_angle_to_goal = UsefulFunctions.angleDifference(currentState.Velocity.cartesianAngle(),
                                                       dir_to_goal.cartesianAngle());

            // use PID to find veer
            double veer = loops.compute(velocity_angle_to_goal, 0, id);

            WheelSpeeds speeds = computeSpeedsAtAngle(velocity, angle_to_goal);

            // Cap veer
            if (veer > 0)
            {
                veer = Math.Min(veer, VEER_CAP);
            }
            else
            {
                veer = Math.Max(veer, -VEER_CAP);
            }
            // add veer based on feedback
            speeds = addVeer(speeds, veer);

            return new MotionPlanningResults(speeds);
        
        }
        public void DrawLast(Graphics g, ICoordinateConverter c){}
        public void ReloadConstants(){
            Constants.Load();
            velocity = Constants.get<double>("motionplanning", "DumbVelocity");
            VEER_CAP = Constants.get<double>("motionplanning", "VEER_CAP");
        }

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

        private static int transformToWheelSpeed(double s)
        {
            // ensure number is an int and is between -127 and 127
            return (int)Math.Max(Math.Min(s, 127), -127);
        }

        /// <summary>
        /// Given a set of wheel speeds and an amount of veer, return new wheel speeds,
        /// where positive veer causes robot to swerve to the left
        /// </summary>
        /// <param name="s"></param>
        /// <param name="veer"></param>
        /// <returns></returns>
        private static WheelSpeeds addVeer(WheelSpeeds speeds, double veer)
        {
            // Change every speed by veer amount
            int intVeer = (int) veer;
            Console.WriteLine("Veer = " + veer);
            return new WheelSpeeds(speeds.lf - intVeer, speeds.rf + intVeer, speeds.lb - intVeer, speeds.rb + intVeer);
        }
    }

    public class FeedbackVeerMotionPlanner : IMotionPlanner
    {
        DumbPathPlanner pathplanner = new DumbPathPlanner();
        FeedbackVeerDriver pathdriver = new FeedbackVeerDriver();

        /// <summary>
        /// Reloads constants for planner and driver
        /// </summary>
        public void ReloadConstants()
        {
            pathplanner.ReloadConstants();
            pathdriver.ReloadConstants();
        }

        public void DrawLast(System.Drawing.Graphics g, ICoordinateConverter c)
        {
            pathplanner.DrawLast(g, c);
        }

        public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius)
        {
            RobotPath path = pathplanner.GetPath(id, desiredState, predictor, avoidBallRadius);
            WheelSpeeds speeds = pathdriver.followPath(path, predictor);
            return new MotionPlanningResults(speeds);
        }
    }
    // These are old versions of PlannerDriver implementations. Ignore unless there is a problem with
    class inheiritance for PlannerDrivers. 
    /// <summary>
    /// Uses PointChargePlanner and ExtenderDriver
    /// </summary>
    public class PointChargeExtendMotionPlanner : IMotionPlanner
    {
        PointChargePlanner pathplanner = new PointChargePlanner();
        ExtenderDriver pathdriver = new ExtenderDriver();

        /// <summary>
        /// Reloads constants for planner and driver
        /// </summary>
        public void ReloadConstants()
        {
            pathplanner.ReloadConstants();
            pathdriver.ReloadConstants();
        }

        public void DrawLast(System.Drawing.Graphics g, ICoordinateConverter c)
        {
            Console.WriteLine("DRAWING");
            pathplanner.DrawLast(g, c);
        }

        public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius)
        {
            RobotPath path = pathplanner.GetPath(id, desiredState, predictor, avoidBallRadius);
            WheelSpeeds speeds = pathdriver.followPath(path, predictor);
            return new MotionPlanningResults(speeds);
        }
    }

    public class PointChargeFeedbackVeerMotionPlanner : IMotionPlanner
    {
        PointChargePlanner pathplanner = new PointChargePlanner();
        FeedbackVeerDriver pathdriver = new FeedbackVeerDriver();

        /// <summary>
        /// Reloads constants for planner and driver
        /// </summary>
        public void ReloadConstants()
        {
            pathplanner.ReloadConstants();
            pathdriver.ReloadConstants();
        }

        public void DrawLast(System.Drawing.Graphics g, ICoordinateConverter c)
        {
            pathplanner.DrawLast(g, c);
        }

        public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius)
        {
            RobotPath path = pathplanner.GetPath(id, desiredState, predictor, avoidBallRadius);
            WheelSpeeds speeds = pathdriver.followPath(path, predictor);
            return new MotionPlanningResults(speeds);
        }
    }

    public class BugExtendMotionPlanner : IMotionPlanner
    {
        BugPlanner pathplanner = new BugPlanner();
        ExtenderDriver pathdriver = new ExtenderDriver();

        /// <summary>
        /// Reloads constants for planner and driver
        /// </summary>
        public void ReloadConstants()
        {
            pathplanner.ReloadConstants();
            pathdriver.ReloadConstants();
        }

        public void DrawLast(System.Drawing.Graphics g, ICoordinateConverter c)
        {
            pathplanner.DrawLast(g, c);
        }

        public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius)
        {
            RobotPath path = pathplanner.GetPath(id, desiredState, predictor, avoidBallRadius);
            WheelSpeeds speeds = pathdriver.followPath(path, predictor);
            return new MotionPlanningResults(speeds);
        }
    }

    public class BugFeedbackVeerMotionPlanner : IMotionPlanner
    {
        BugPlanner pathplanner = new BugPlanner();
        FeedbackVeerDriver pathdriver = new FeedbackVeerDriver();

        /// <summary>
        /// Reloads constants for planner and driver
        /// </summary>
        public void ReloadConstants()
        {
            pathplanner.ReloadConstants();
            pathdriver.ReloadConstants();
        }

        public void DrawLast(System.Drawing.Graphics g, ICoordinateConverter c)
        {
            pathplanner.DrawLast(g, c);
        }

        public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius)
        {
            RobotPath path = pathplanner.GetPath(id, desiredState, predictor, avoidBallRadius);
            WheelSpeeds speeds = pathdriver.followPath(path, predictor);
            return new MotionPlanningResults(speeds);
        }
    }

    public class DumbTranslatePlanner : IMotionPlanner
    {
        DumbPathPlanner pathplanner = new DumbPathPlanner();
        TranslateDriver pathdriver = new TranslateDriver();

        /// <summary>
        /// Reloads constants for planner and driver
        /// </summary>
        public void ReloadConstants()
        {
            pathplanner.ReloadConstants();
            pathdriver.ReloadConstants();
        }

        public void DrawLast(System.Drawing.Graphics g, ICoordinateConverter c)
        {
            pathplanner.DrawLast(g, c);
        }

        public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius)
        {
            RobotPath path = pathplanner.GetPath(id, desiredState, predictor, avoidBallRadius);
            WheelSpeeds speeds = pathdriver.followPath(path, predictor);
            return new MotionPlanningResults(speeds);
        }
    }

    /// <summary>
    /// Default planner for testing other components of the robot
    /// </summary>
    public class OldDefaultMotionPlanner : IMotionPlanner
    {
        DumbPathPlanner pathplanner = new DumbPathPlanner();
        DefaultDriver pathdriver = new DefaultDriver();

        /// <summary>
        /// Reloads constants for planner and driver
        /// </summary>
        public void ReloadConstants()
        {
            pathplanner.ReloadConstants();
            pathdriver.ReloadConstants();
        }

        public void DrawLast(System.Drawing.Graphics g, ICoordinateConverter c)
        {
            pathplanner.DrawLast(g, c);
        }

        public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius)
        {
            RobotPath path = pathplanner.GetPath(id, desiredState, predictor, avoidBallRadius);
            WheelSpeeds speeds = pathdriver.followPath(path, predictor);
            return new MotionPlanningResults(speeds);
        }
    }

    /// <summary>
    /// Default motion planner for the CS199r class. Meant to test plays or tactics, so
    /// very slow but accurate and reliable
    /// </summary>
    public class DefaultMotionPlanner : PlannerDriver
    {
        static DumbPathPlanner pathplanner = new DumbPathPlanner();
        static DefaultDriver pathdriver = new DefaultDriver();

        public DefaultMotionPlanner() : base(pathplanner, pathdriver) { }
    }
#endif

    public class DumbTurnPlanner : PlannerDriver
    {
        static DumbPathPlanner pathplanner = new DumbPathPlanner();
        static TurnDriver pathdriver = new TurnDriver();

        public DumbTurnPlanner() : base(pathplanner, pathdriver) { }
    }

    public class FeedbackVeerMotionPlanner : PlannerDriver
    {
        static DumbPathPlanner pathplanner = new DumbPathPlanner();
        static FeedbackVeerDriver pathdriver = new FeedbackVeerDriver();

        public FeedbackVeerMotionPlanner() : base(pathplanner, pathdriver) { }
    }

    public class PointChargeExtendMotionPlanner : PlannerDriver
    {
        static PointChargePlanner pathplanner = new PointChargePlanner();
        static ExtenderDriver pathdriver = new ExtenderDriver();

        public PointChargeExtendMotionPlanner() : base(pathplanner, pathdriver) { }
    }

    public class PointChargeFeedbackVeerMotionPlanner : PlannerDriver
    {
        static PointChargePlanner pathplanner = new PointChargePlanner();
        static FeedbackVeerDriver pathdriver = new FeedbackVeerDriver();

        public PointChargeFeedbackVeerMotionPlanner() : base(pathplanner, pathdriver) { }
    }

    public class BugExtendMotionPlanner : PlannerDriver
    {
        static TangentBugPlanner pathplanner = new TangentBugPlanner();
        static ExtenderDriver pathdriver = new ExtenderDriver();

        public BugExtendMotionPlanner() : base(pathplanner, pathdriver) { }
    }

    public class BugNavigatorExtendMotionPlanner : PlannerDriver {
        static BugNavigatorPlanner pathplanner = new BugNavigatorPlanner();
        static ExtenderDriver pathdriver = new ExtenderDriver();

        public BugNavigatorExtendMotionPlanner() : base(pathplanner, pathdriver) { }
    }

    public class BugFeedbackVeerMotionPlanner : PlannerDriver
    {
        static TangentBugPlanner pathplanner = new TangentBugPlanner();
        static FeedbackVeerDriver pathdriver = new FeedbackVeerDriver();

        public BugFeedbackVeerMotionPlanner() : base(pathplanner, pathdriver) { }
    }

    public class DumbTranslatePlanner : PlannerDriver
    {
        static DumbPathPlanner pathplanner = new DumbPathPlanner();
        static TranslateDriver pathdriver = new TranslateDriver();

        public DumbTranslatePlanner() : base(pathplanner, pathdriver) { }
    }

    /// <summary>
    /// Default motion planner for the CS199r class. Meant to test plays or tactics, so
    /// very slow but accurate and reliable
    /// </summary>
    public class DefaultMotionPlanner : PlannerDriver
    {
        static DumbPathPlanner pathplanner = new DumbPathPlanner();
        static DefaultDriver pathdriver = new DefaultDriver();

        public DefaultMotionPlanner() : base(pathplanner, pathdriver) { }
    }

    /// <summary>
    /// New implementation of BugFeedbackMotionPlanner- split into two parts
    /// </summary>
    public class BugFeedbackMotionPlanner : PlannerDriver 
    {
        static BugNavigatorPlanner pathplanner = new BugNavigatorPlanner();
        static PositionFeedbackDriver pathdriver = new PositionFeedbackDriver();

        public BugFeedbackMotionPlanner() : base(pathplanner, pathdriver) { }

        public Feedback GetFeedbackObj(int robotID) { return pathdriver.GetFeedbackObj(robotID); }
    }

    public class TangentBugFeedbackMotionPlanner : PlannerDriver {
        static TangentBugPlanner pathplanner = new TangentBugPlanner();
        static PositionFeedbackDriver pathdriver = new PositionFeedbackDriver();

        public TangentBugFeedbackMotionPlanner() : base(pathplanner, pathdriver) { }

        public Feedback GetFeedbackObj(int robotID) { return pathdriver.GetFeedbackObj(robotID); }
    }

}
