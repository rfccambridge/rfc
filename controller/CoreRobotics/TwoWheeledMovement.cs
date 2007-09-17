using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Infrastructure;
using Robocup.Core;

namespace Robocup.CoreRobotics
{
    public class TwoWheeledMovement : IMovement
    {
        public enum WhichTwoWheels
        {
            FrontLeftBackRight,
            FrontRightBackLeft
        }
        private IPredictor predictor;
        private WhichTwoWheels whichtwowheels;
        public TwoWheeledMovement(IPredictor predictor, WhichTwoWheels whichtwowheels)
        {
            this.predictor = predictor;
            this.whichtwowheels = whichtwowheels;
        }

        #region IMovement Members

        public WheelSpeeds calculateWheelSpeeds(int robotID, RobotInfo currentInfo, NavigationResults results)
        {
            return calculateWheelSpeeds(robotID, currentInfo, results, currentInfo.Orientation);
        }

        const float MoveTol = .01f;
        GeneralPID anglePID = new GeneralPID(
            Constants.Constants.get<float>("ANGLE_PID_KP"),
            Constants.Constants.get<float>("ANGLE_PID_KI"),
            Constants.Constants.get<float>("ANGLE_PID_KD"),
            Constants.Constants.get<float>("ANGLE_PID_MAX"),
            Constants.Constants.get<float>("ANGLE_PID_RESET")
            );
        GeneralPID movePID = new GeneralPID(
            Constants.Constants.get<float>("MOVE_PID_KP"),
            Constants.Constants.get<float>("MOVE_PID_KI"),
            Constants.Constants.get<float>("MOVE_PID_KD"),
            Constants.Constants.get<float>("MOVE_PID_MAX"),
            Constants.Constants.get<float>("MOVE_PID_RESET")
            );

        public WheelSpeeds calculateWheelSpeeds(int robotID, RobotInfo currentInfo, NavigationResults results, float desiredOrientation)
        {
            Vector2 destination = results.waypoint;
            Vector2 position = currentInfo.Position;
            double distanceToMove = Math.Sqrt(position.distanceSq(destination));
            double angleToTarget = Math.Atan2(destination.Y - position.Y, destination.X - position.X);
            //if we're far enough away, then we want to angle ourselves 45º away
            if (distanceToMove > MoveTol)
            {
                if (whichtwowheels == WhichTwoWheels.FrontLeftBackRight)
                    desiredOrientation = (float)(angleToTarget + Math.PI / 4);
                else
                    desiredOrientation = (float)(angleToTarget - Math.PI / 4);
            }

            double angleChange = desiredOrientation - currentInfo.Orientation;
            while (angleChange > Math.PI)
                angleChange -= 2 * Math.PI;
            while (angleChange < -Math.PI)
                angleChange += 2 * Math.PI;
            bool isbackwards = false;
            if (Math.Abs(angleChange) > Math.PI / 2)
            {
                isbackwards = true;
                if (angleChange > 0)
                    angleChange -= Math.PI;
                else if (angleChange < 0)
                    angleChange += Math.PI;
            }

            int anglepower = (int)(anglePID.getNext(angleChange));
            Console.WriteLine("angle diff: " + angleChange);
            Console.WriteLine("anglepower: " + anglepower);
            WheelSpeeds angleSpeeds = new WheelSpeeds(-anglepower, anglepower, -anglepower, anglepower);
            WheelSpeeds rtn = angleSpeeds;
            Console.WriteLine("move diff: " + distanceToMove);

            if (angleChange < Math.PI / 2 && distanceToMove > MoveTol)
            {
                if (isbackwards)
                    distanceToMove *= -1;
                int forwardpower = (int)(movePID.getNext(distanceToMove));
                Console.WriteLine("forwardpower: " + forwardpower);
                WheelSpeeds forwardSpeeds = new WheelSpeeds(forwardpower, forwardpower, forwardpower, forwardpower);
                rtn += forwardSpeeds;
            }

            if (whichtwowheels == WhichTwoWheels.FrontLeftBackRight)
            {
                rtn.lb = 0;
                rtn.rf = 0;
            }
            else
            {
                rtn.lf = 0;
                rtn.rb = 0;
            }

            return rtn;
        }

        public void reloadPID()
        {
            anglePID = new GeneralPID(
                Constants.Constants.get<float>("ANGLE_PID_KP"),
                Constants.Constants.get<float>("ANGLE_PID_KD"),
                Constants.Constants.get<float>("ANGLE_PID_KI"),
                Constants.Constants.get<float>("ANGLE_PID_MAX"),
                Constants.Constants.get<float>("ANGLE_PID_RESET")
            );
            movePID = new GeneralPID(
                Constants.Constants.get<float>("MOVE_PID_KP"),
                Constants.Constants.get<float>("MOVE_PID_KD"),
                Constants.Constants.get<float>("MOVE_PID_KI"),
                Constants.Constants.get<float>("MOVE_PID_MAX"),
                Constants.Constants.get<float>("MOVE_PID_RESET")
            );
        }

        #endregion
    }
}
