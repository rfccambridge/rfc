using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Infrastructure;

namespace Robocup.CoreRobotics
{
    /*class FourWheeledMovement : IMovement
    {
        private IPredictor predictor;
        public FourWheeledMovement(IPredictor predictor)
        {
            this.predictor = predictor;
        }

        #region IMovement Members

        public WheelSpeeds calculateWheelSpeeds(int robotID, RobotInfo currentInfo, Vector2 destination)
        {
            return calculateWheelSpeeds(robotID, currentInfo, destination, currentInfo.Orientation);
        }

        const float MoveTol = .01f;
        GeneralPID anglePID = new GeneralPID(30,10,0,127);
        GeneralPID movePID = new GeneralPID(75,10,0,127);
        public WheelSpeeds calculateWheelSpeeds(int robotID, RobotInfo currentInfo, Vector2 destination, float desiredOrientation)
        {
            Vector2 position = currentInfo.Position;
            float distanceToMove = position.distanceSq(destination);
            double angleToTarget = Math.Atan2(destination.Y-position.Y, destination.X-position.X);

            double angleChange = desiredOrientation - currentInfo.Orientation;
            while (angleChange > Math.PI)
                angleChange -= 2 * Math.PI;
            while (angleChange < -Math.PI)
                angleChange += 2 * Math.PI;

            int anglepower = (int)(anglePID.getNext(angleChange));
            WheelSpeeds angleSpeeds = new WheelSpeeds(-anglepower, anglepower, -anglepower, anglepower);
            WheelSpeeds rtn = angleSpeeds;

            if (angleChange < Math.PI / 2 && distanceToMove > MoveTol)
            {
                int forwardpower = (int)(movePID.getNext(distanceToMove));
                WheelSpeeds forwardSpeeds = new WheelSpeeds(forwardpower, forwardpower, forwardpower, forwardpower);
                rtn += forwardSpeeds;
            }

            return rtn;
        }

        #endregion
    }*/
}
