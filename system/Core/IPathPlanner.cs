using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using System.Drawing;

namespace Robocup.CoreRobotics
{
    
    // This interface is still under construction- please do not use.
    /// <summary>
    /// The parent class for motion planners that are separated into planner and driver classes
    /// planner and driver should be initialized in constructor
    /// </summary>
    public class PlannerDriver : IMotionPlanner
    {
        IPathPlanner _planner;
        IPathDriver _driver;

        public PlannerDriver(IPathPlanner planner, IPathDriver driver)
        {
            _planner = planner;
            _driver = driver;
        }

        /// <summary>
        /// Reloads constants for planner and driver
        /// </summary>
        public void ReloadConstants()
        {
            _planner.ReloadConstants();
            _driver.ReloadConstants();
        }

        public void DrawLast(System.Drawing.Graphics g, ICoordinateConverter c)
        {
            _planner.DrawLast(g, c);
        }

        public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius)
        {
            RobotPath path = _planner.GetPath(id, desiredState, predictor, avoidBallRadius);
            WheelSpeeds speeds = _driver.followPath(path, predictor);
            Console.WriteLine("PlanMotion returning speeds " + speeds);
            return new MotionPlanningResults(speeds);
        }
    }

    /// <summary>
    /// Interface for algorithms to plan paths
    /// </summary>
    public interface IPathPlanner
    {
        RobotPath GetPath(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius);
        void DrawLast(Graphics g, ICoordinateConverter c);
        void ReloadConstants();
    }

    /// <summary>
    /// Interface for algorithms that, given a path, return wheel speeds to
    /// drive around that path
    /// </summary>
    public interface IPathDriver
    {
        WheelSpeeds followPath(RobotPath path, IPredictor predictor);
        void ReloadConstants();
    }
}
