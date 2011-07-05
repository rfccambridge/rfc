using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using System.Drawing;

namespace Robocup.CoreRobotics
{
    /// <summary>
    /// Used to specify whether or how the motion planner should stay out of the defense region around a goal.
    /// </summary>
    public enum DefenseAreaAvoid { NONE, NORMAL, FULL };

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
        RobotPath PlanMotion(RobotInfo desiredState, IPredictor predictor, 
            double avoidBallRadius, RobotPath oldPath, 
            DefenseAreaAvoid leftAvoid, DefenseAreaAvoid rightAvoid);
    	MotionPlanningResults FollowPath(RobotPath path, IPredictor predictor);
        void LoadConstants();
        
    }
}
