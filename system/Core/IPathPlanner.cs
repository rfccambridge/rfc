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

            // if path is empty, don't move
            if (path.isEmpty()) {
                return new MotionPlanningResults(new WheelSpeeds());
            }

            // Make sure path contains desired state
            path.setFinalState(desiredState);

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
    /// An interface for turning an INavigator into a PathPlanner
    /// </summary>
    public class NavigatorPlanner : IPathPlanner
    {
        INavigator _navigator;
        Vector2 lastWaypoint;

        public NavigatorPlanner(INavigator navigator) {
            _navigator = navigator;
        }

        public RobotPath GetPath(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius) {
            
            //Use navigator to get information
            RobotInfo currentState = predictor.getCurrentInformation(id);

            NavigationResults results = _navigator.navigate(currentState.ID, currentState.Position,
                desiredState.Position, predictor.getOurTeamInfo().ToArray(), predictor.getTheirTeamInfo().ToArray(), predictor.getBallInfo(),
                0.15);

            List<Vector2> waypoints = new List<Vector2>();
            lastWaypoint = results.waypoint;
            waypoints.Add(lastWaypoint);
            return new RobotPath(id, waypoints);
        }

        public void DrawLast(Graphics g, ICoordinateConverter c) {
            if (lastWaypoint != null) {
                Brush b = new SolidBrush(Color.Blue);
                g.FillRectangle(b, c.fieldtopixelX(lastWaypoint.X) - 1, c.fieldtopixelY(lastWaypoint.Y) - 1, 4, 4);
            }
        }

        public void ReloadConstants() {

        }
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
