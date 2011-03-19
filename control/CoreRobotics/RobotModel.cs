using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Core;
using Robocup.Geometry;

namespace Robocup.CoreRobotics
{
    //This is used as a base of different robot models to be used when computing wheelspeed commands.
    //For now, called only from Feedback.ComputeWheelSpeeds and the joystick
    public abstract class RobotModel
    {
        //a fine-grained may model may be calibrated on a per-robot basis
        private int robotID;

        public RobotModel(int _robotID)
        {
            robotID = _robotID;
        }

        public virtual void LoadConstants()
        {
        }

        public abstract void ComputeCommand(RobotInfo currentState, RobotInfo desiredState, out double xCommand, out double yCommand, out double thetaCommand);

        /// <summary>
        /// Convert a desired velocity (in robot coordinates!!!) to WheelSpeeds object
        /// </summary>
        /// <param name="forward"></param>
        /// <param name="lateral"></param>
        /// <param name="angularV"></param>
        /// <returns></returns>
        public WheelSpeeds Convert(double forward, double lateral, double angularV)
        {
            const double WHEEL_R = 0.0782828; //distance from the center of the robot to the wheels in meters
            const double ANGLE_AXIS_TO_WHEEL = 45 * Math.PI / 180;

            //I assume the x command is effectively in m/s, so r the radius of the wheels from the center of
            //the robot is in meters

            //Console.WriteLine(lateral.ToString() + " lateral|Forward: " + forward.ToString());

            //computed here to save typing, since used 4 times
            double sing = Math.Sin(ANGLE_AXIS_TO_WHEEL);
            double cosg = Math.Cos(ANGLE_AXIS_TO_WHEEL);

            //wheel one is the front right wheel  wheel 2 is the front left wheel, 
            //and so on around the the robot counterclockwise

            double _rf = -sing * lateral - cosg * forward + WHEEL_R * angularV;
            double _lf = -sing * lateral + cosg * forward + WHEEL_R * angularV;
            double _lb =  sing * lateral + cosg * forward + WHEEL_R * angularV;
            double _rb =  sing * lateral - cosg * forward + WHEEL_R * angularV;

            int lf, rf, lb, rb;
            lf = (int)_lf;
            rf = (int)_rf;
            lb = (int)_lb;
            rb = (int)_rb;

            //Note somewhere we need to check and ensure that wheel speeds being 
            //sent do not exceed maximum values allowed by the protocol (done in SerialRobots somewhere).
            return new WheelSpeeds(rf, lf, lb, rb);
        }

        public WheelSpeeds DriveInDirection(double speed, double dx, double dy)
        {

            // rf, lf, lb, rb  forward = all positive forward = + - - +
            double[] wheel_dx = new double[] { 0.71f, -0.71f, -0.74f, 0.74f };
            double[] wheel_dy = new double[] { 0.71f, 0.71f, -0.68f, -0.68f };
            double[] wheel_baseline = new double[] { 3.23f, 3.23f, 3.23f, 3.23f };
            double[] wheel_radius = new double[] { 1.0f, 1.0f, 1.0f, 1.0f };

            // takes in a (unit) vector to translate the robot by
            double[] wheel_speeds = new double[4];

            for (int i = 0; i < 4; i++)
                wheel_speeds[i] = speed * (dx * wheel_dx[i] + dy * wheel_dy[i]) / wheel_radius[i];

            return new WheelSpeeds((int)wheel_speeds[0], (int)wheel_speeds[1],
                                   (int)wheel_speeds[2], (int)wheel_speeds[3]);
        }
    }

    /// <summary>
    /// This is the pre- 23.06.2009 behaviour of Feedback.cs
    /// It used a feed-forward term that was exactly the desired command.
    /// Not sure how it worked at all.
    /// </summary>
    public class FailSafeModel : RobotModel
    {
        public FailSafeModel(int _robotID)
            : base(_robotID)
        { }

        public override void ComputeCommand(RobotInfo currentState, RobotInfo desiredState, out double xCommand, out double yCommand, out double thetaCommand)
        {
            double xOut = 0.0, yOut = 0.0, thetaOut = 0.0;

            //xOut = desiredState.Position.X;
            //yOut = desiredState.Position.Y;
            thetaCommand = UsefulFunctions.angleCheck(desiredState.Orientation);

            xCommand = xOut;
            yCommand = yOut;
            thetaCommand = thetaOut;
        }

    }



    public class TestModel : RobotModel
    {
        private double DEFAULT_VELOCITY;

        public TestModel(int _robotID)
            : base(_robotID)
        {
            DEFAULT_VELOCITY = 0.0;

            LoadConstants();
        }

        public override void LoadConstants()
        {
            DEFAULT_VELOCITY = Constants.get<double>("control", "DEFAULT_VELOCITY");
        }

        public override void ComputeCommand(RobotInfo currentState, RobotInfo desiredState, out double xCommand, out double yCommand, out double thetaCommand)
        {
            double thetaOut = desiredState.Orientation;

            Vector2 positionOffset = desiredState.Position - currentState.Position;
            Vector2 velocity = positionOffset.normalizeToLength(DEFAULT_VELOCITY);

            xCommand = velocity.X;
            yCommand = velocity.Y;
            thetaCommand = thetaOut;
        }

    }
}
