using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;

namespace Robocup.RRT
{
    public class MotionPlanningResults
    {
        public MotionPlanningResults(WheelSpeeds wheel_speeds)
        {
            this.wheel_speeds = wheel_speeds;
        }
        public WheelSpeeds wheel_speeds;
    }
    interface IMotionPlanner
    {
        MotionPlanningResults PlanMotion(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius);
    }
}
