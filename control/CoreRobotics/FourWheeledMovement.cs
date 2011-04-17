using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using Robocup.Utilities;

#if false
namespace Robocup.CoreRobotics
{
    public class FourWheeledMovement : IMovement
    {
        public FourWheeledMovement()
        {
        }

        #region IMovement Members

        public WheelSpeeds calculateWheelSpeeds(IPredictor predictor, int robotID, RobotInfo currentInfo, NavigationResults results)
        {
            return calculateWheelSpeeds(predictor, robotID, currentInfo, results, currentInfo.Orientation);
        }

        const double MoveTol = .01;
        GeneralPID anglePID = new GeneralPID(
            ConstantsRaw.get<double>("control", "ANGLE_PID_KP"),
            ConstantsRaw.get<double>("control", "ANGLE_PID_KI"),
            ConstantsRaw.get<double>("control", "ANGLE_PID_KD"),
            ConstantsRaw.get<double>("control", "ANGLE_PID_MAX"),
            ConstantsRaw.get<double>("control", "ANGLE_PID_RESET")
            );
        GeneralPID movePID = new GeneralPID(
            ConstantsRaw.get<double>("control", "MOVE_PID_KP"),
            ConstantsRaw.get<double>("control", "MOVE_PID_KI"),
            ConstantsRaw.get<double>("control", "MOVE_PID_KD"),
            ConstantsRaw.get<double>("control", "MOVE_PID_MAX"),
            ConstantsRaw.get<double>("control", "MOVE_PID_RESET")
            );

        public WheelSpeeds calculateWheelSpeeds(IPredictor predictor, int robotID, RobotInfo currentInfo, NavigationResults results, double desiredOrientation)
        {
            return WheelSpeedsExtender.GetWheelSpeeds(currentInfo, results.waypoint);
            /*Vector2 destination = results.waypoint;
            Vector2 position = currentInfo.Position;
            double distanceToMove = Math.Sqrt(position.distanceSq(destination));
            double angleToTarget = Math.Atan2(destination.Y - position.Y, destination.X - position.X);

            //if we're far enough away, then we want to angle ourselves 45º away
            if (distanceToMove > MoveTol)
            {
                desiredOrientation = angleToTarget;
            }

            double angleChange = desiredOrientation - currentInfo.Orientation;
            while (angleChange > Math.PI)
                angleChange -= 2 * Math.PI;
            while (angleChange < -Math.PI)
                angleChange += 2 * Math.PI;
            bool isbackwards = false;
            /*if (Math.Abs(angleChange) > Math.PI / 2)
            {
                isbackwards = true;
                if (angleChange > 0)
                    angleChange -= Math.PI;
                else if (angleChange < 0)
                    angleChange += Math.PI;
            }*

            int anglepower = (int)(anglePID.getNext(angleChange));
            //Console.WriteLine("angle diff: " + angleChange);
            //Console.WriteLine("anglepower: " + anglepower);
            WheelSpeeds angleSpeeds = new WheelSpeeds(-anglepower, anglepower, -anglepower, anglepower);
            WheelSpeeds rtn = angleSpeeds;
            //Console.WriteLine("move diff: " + distanceToMove);

            if (angleChange < Math.PI / 2 && distanceToMove > MoveTol)
            {
                if (isbackwards)
                    distanceToMove *= -1;
                int forwardpower = (int)(movePID.getNext(distanceToMove));
                forwardpower = Math.Sign(forwardpower) * Math.Max(0, Math.Abs(forwardpower) - Math.Abs(anglepower));
                //Console.WriteLine("forwardpower: " + forwardpower);
                WheelSpeeds forwardSpeeds = new WheelSpeeds(forwardpower, forwardpower, forwardpower, forwardpower);
                rtn += forwardSpeeds;
            }

            return rtn;*/
        }

        public void reloadPID()
        {
            anglePID = new GeneralPID(
                ConstantsRaw.get<double>("default", "ANGLE_PID_KP"),
                ConstantsRaw.get<double>("default", "ANGLE_PID_KD"),
                ConstantsRaw.get<double>("default", "ANGLE_PID_KI"),
                ConstantsRaw.get<double>("default", "ANGLE_PID_MAX"),
                ConstantsRaw.get<double>("default", "ANGLE_PID_RESET")
            );
            movePID = new GeneralPID(
                ConstantsRaw.get<double>("default", "MOVE_PID_KP"),
                ConstantsRaw.get<double>("default", "MOVE_PID_KD"),
                ConstantsRaw.get<double>("default", "MOVE_PID_KI"),
                ConstantsRaw.get<double>("default", "MOVE_PID_MAX"),
                ConstantsRaw.get<double>("default", "MOVE_PID_RESET")
            );
        }

        #endregion
    }
}
#endif