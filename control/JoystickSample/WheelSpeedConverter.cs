using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Core;

namespace JoystickSample
{
    /// <summary>
    /// A class meant to contain methods for converting wheel speeds for the joystick
    /// </summary>
    static class WheelSpeedConverter
    {
        // Constants
        const double wheelR = 0.0782828; //distance from the center of the robot to the wheels in meters

        const double SCALING_FACTOR = -120;
        const double EXTRA_ANGULAR_SCALING_FACTOR = 10;

        /// <summary>
        /// Convert a desired x velocity, y velocity, angular velocity, and theta arguments to WheelSpeeds object
        /// </summary>
        /// <param name="xCommand"></param>
        /// <param name="yCommand"></param>
        /// <param name="angularV"></param>
        /// <param name="theta"></param>
        /// <returns></returns>
        public static WheelSpeeds convert(double xCommand, double yCommand, double angularV, double theta)
        {
            const double ANGLE_AXIS_TO_WHEEL = 41 * Math.PI / 180;

            //I assume the x command is effectively in m/s, so r the radius of the wheels from the center of
            //the robot is in meters

            //change from the x and y of the field to forward and lateral(right is positive) used below
            double forward = -(Math.Cos(theta) * xCommand + Math.Sin(theta) * yCommand);
            double lateral = -(-Math.Sin(theta) * xCommand + Math.Cos(theta) * yCommand);

            //Console.WriteLine(lateral.ToString() + " lateral|Forward: " + forward.ToString());

            //computed here to save typing, since used 4 times
            double sing = Math.Sin(ANGLE_AXIS_TO_WHEEL);
            double cosg = Math.Cos(ANGLE_AXIS_TO_WHEEL);

            //wheel one is the front right wheel  wheel 2 is the back right wheel, and so on around the the robot clockwise


            double _rf = (sing * lateral + cosg * forward - wheelR * angularV * EXTRA_ANGULAR_SCALING_FACTOR);
            double _lf = (sing * lateral - cosg * forward - wheelR * angularV * EXTRA_ANGULAR_SCALING_FACTOR);            
            double _lb = (-sing * lateral - cosg * forward - wheelR * angularV * EXTRA_ANGULAR_SCALING_FACTOR);
            double _rb = (-sing * lateral + cosg * forward - wheelR * angularV * EXTRA_ANGULAR_SCALING_FACTOR);


            /*int scaleUpFactor = 2;
            if (Math.Abs(_lf) < 10 && Math.Abs(_rf) < 10 && Math.Abs(_lb) < 10 && Math.Abs(_rb) < 10) {
                _lf *= scaleUpFactor;
                _rf *= scaleUpFactor;
                _lb *= scaleUpFactor;
                _rb *= scaleUpFactor;
            }*/

            int lf, rf, lb, rb;
            lf = (int) (_lf * SCALING_FACTOR);
            rf = (int) (_rf * SCALING_FACTOR);
            lb = (int) (_lb * SCALING_FACTOR);
            rb = (int) (_rb * SCALING_FACTOR);


            /*if (lf > 57) 
                lf = 57;
            if (lf < -57)
                lf = -57;
                        
            if (rf > 57) 
                rf = 57;
            if (rf < -57)
                rf = -57;
                        
            if (lb > 57) 
                lb = 57;
            if (lb < -57)
                lb = -57;
                        
            if (rb > 57) 
                rb = 57;
            if (rb < -57)
                rb = -57;
            */

            return new WheelSpeeds(rf, lf, lb, rb);
            //Note somewhere we need to check and ensure that wheel speeds being 
            //sent do not exceed maximum values allowed by the protocol.
        }
    }
}
