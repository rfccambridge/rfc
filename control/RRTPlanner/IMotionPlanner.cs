using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using System.Drawing;

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
    public interface IMotionPlanner
    {
        MotionPlanningResults PlanMotion(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius);
        void DrawLast(Graphics g, ICoordinateConverter c);
    }
}
