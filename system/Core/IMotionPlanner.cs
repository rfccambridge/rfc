using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using System.Drawing;

namespace Robocup.CoreRobotics
{
    /// <summary>
    /// Contains wheel speeds to send to the robot
    /// </summary>
    public class MotionPlanningResults
    {
        public MotionPlanningResults(WheelSpeeds wheel_speeds)
        {
            this.wheel_speeds = wheel_speeds;
        }
        public MotionPlanningResults(WheelSpeeds wheel_speeds, RobotInfo nearestWaypoint) {
            this.wheel_speeds = wheel_speeds;
            this.NearestWaypoint = nearestWaypoint;
        }
        public WheelSpeeds wheel_speeds;
        public RobotInfo NearestWaypoint;
    }

    public interface IMotionPlanner
    {
        MotionPlanningResults PlanMotion(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius);
        void DrawLast(Graphics g, ICoordinateConverter c);
        void ReloadConstants();
        
    }
}
