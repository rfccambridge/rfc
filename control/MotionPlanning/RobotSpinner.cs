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
    /// These perform the task of spinning a robot towards a particular orientation
    /// </summary>
    public interface IRobotSpinner
    {
        WheelSpeeds spinTo(int id, double desiredOrientation, IPredictor predictor);
        void ReloadConstants();
    }

    /// <summary>
    /// Spins at a given speed either CW or CCW
    /// </summary>
    public class DirectRobotSpinner
    {
        int DIRECT_SPIN_SPEED;
        //double STOP_WITHIN_ORIENTATION;

        WheelSpeeds CWSpeeds;
        WheelSpeeds CCWSpeeds;

        public WheelSpeeds spinTo(int id, double desiredOrientation, double stopWithin, IPredictor predictor)
        {
            RobotInfo currentState = predictor.getCurrentInformation(id);

            // angleDifference is positive when current robot needs to turn CW, negative when need to go CCW
            double angleDifference = UsefulFunctions.angleDifference(desiredOrientation, currentState.Orientation);

            Console.WriteLine("Angle difference " + angleDifference);

            // If close enough, stop
            if (Math.Abs(angleDifference) < stopWithin)
            {
                return new WheelSpeeds();
            }

            // otherwise, turn CW or CCW
            if (angleDifference > 0)
                return CWSpeeds;
            else
                return CCWSpeeds;
        }
        
        
        public void ReloadConstants()
        {
            DIRECT_SPIN_SPEED = Constants.get<int>("motionplanning", "DIRECT_SPIN_SPEED");

            CWSpeeds = new WheelSpeeds(DIRECT_SPIN_SPEED, -DIRECT_SPIN_SPEED, DIRECT_SPIN_SPEED, -DIRECT_SPIN_SPEED);
            CCWSpeeds = new WheelSpeeds(-DIRECT_SPIN_SPEED, DIRECT_SPIN_SPEED, -DIRECT_SPIN_SPEED, DIRECT_SPIN_SPEED);

            
        }
    }
}
