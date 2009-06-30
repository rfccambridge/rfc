using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Core;
using Robocup.Geometry;

namespace Robocup.MotionControl
{
    //This is used as a base of different robot models to be used when computing wheelspeed commands.
    //For now, called only from Feedback.ComputeWheelSpeeds
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
