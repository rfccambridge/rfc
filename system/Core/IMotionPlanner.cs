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
        public WheelSpeeds wheel_speeds;
    }

    public interface IMotionPlanner
    {
        RobotPath PlanMotion(Team team, int id, RobotInfo desiredState, IPredictor predictor, 
            double avoidBallRadius, RobotPath oldPath);
    	MotionPlanningResults FollowPath(RobotPath path, IPredictor predictor);
        void LoadConstants();
        
    }
}
