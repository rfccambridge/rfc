using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Robocup.Core;
using Robocup.Geometry;

namespace Robocup.CoreRobotics
{
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
        public virtual void LoadConstants()
        {
            _planner.ReloadConstants();
            _driver.ReloadConstants();
        }

        public RobotPath PlanMotion(RobotInfo desiredState, IPredictor predictor,
            double avoidBallRadius, RobotPath oldPath, DefenseAreaAvoid leftAvoid, DefenseAreaAvoid rightAvoid)
        {
            RobotPath path = _planner.GetPath(desiredState, predictor, avoidBallRadius, oldPath, leftAvoid, rightAvoid);

            // if path is empty, don't move
            if (path.isEmpty()) {
                return path;
            }

            // Make sure path contains desired state
            path.setFinalState(desiredState);
            return path;
        }

		public MotionPlanningResults FollowPath(RobotPath path, IPredictor predictor)
        {
            try
            {
                WheelSpeeds speeds = _driver.followPath(path, predictor);
                return new MotionPlanningResults(speeds);
            }
            catch (ApplicationException e)
            {
                Console.WriteLine(e.Message + "\r\n" + e.StackTrace);
                return new MotionPlanningResults(new WheelSpeeds());
            }
		}
    }

    /// <summary>
    /// Interface for algorithms to plan paths
    /// </summary>
    public interface IPathPlanner
    {
        RobotPath GetPath(RobotInfo desiredState, IPredictor predictor, double avoidBallRadius,
            RobotPath oldPath, DefenseAreaAvoid leftAvoid, DefenseAreaAvoid rightAvoid);
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

        public RobotPath GetPath(RobotInfo desiredState, IPredictor predictor,
            double avoidBallRadius, RobotPath oldPath, DefenseAreaAvoid leftAvoid, DefenseAreaAvoid rightAvoid)
        {
            Team team = desiredState.Team;
            int id = desiredState.ID;
            Team theirTeam = team == Team.Yellow ? Team.Blue : Team.Yellow;

            //Use navigator to get information
            RobotInfo currentState;
            try
            {
                currentState = predictor.GetRobot(team, id);
            }
            catch (ApplicationException)
            {
                return new RobotPath(team, id);
            }
            
            NavigationResults results = _navigator.navigate(currentState.ID, currentState.Position,
                desiredState.Position, predictor.GetRobots(team).ToArray(), 
                predictor.GetRobots(theirTeam).ToArray(), 
                predictor.GetBall(),
                0.15);

            List<Vector2> waypoints = new List<Vector2>();
            lastWaypoint = results.waypoint;
            waypoints.Add(lastWaypoint);
            return new RobotPath(team, id, waypoints);
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
