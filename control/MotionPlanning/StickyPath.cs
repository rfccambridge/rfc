using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Core;
using Robocup.CoreRobotics;

using System.Drawing;

namespace Robocup.MotionControl {
    /// <summary>
    /// Represents the velocity at a point on the path
    /// </summary>
    class PointVelocity {
        private double vx;
        private double vy;
        private double dtheta;
        public PointVelocity(double vx, double vy, double dtheta)
        {
            this.vx = vx;
            this.vy = vy;
            this.dtheta = dtheta;
        }

        public double VX {
            get { return vx; }
            set { vx = value; }
        }
        public double VY {
            get { return vy; }
            set { vx = value; }
        }
        public double dTheta {
            get { return dtheta; }
            set { dtheta = value; }
        }
    }

    /// <summary>
    /// Represents a path that sticks between frames for a single robot
    /// </summary>
    class StickyPath {
        // Parameters for the robot's path
        //TODO: Move this constant into a designated constant file
        double maxX = 1;
        double maxY = 1;
        double maxOmega = 1;

        Feedback feedback_loop;

        private List<Vector2> stickypath = null;
        private List<PointVelocity> max_velocities = null;
        
        private RobotInfo destination = null;
        private BidirectionalRRTPlanner<Vector2, Vector2, Vector2Tree, Vector2Tree> planner;

        /// <summary>
        /// Construct a StickyPath with a planner
        /// </summary>
        /// <param name="Planner"></param>
        public StickyPath(BidirectionalRRTPlanner<Vector2, Vector2, Vector2Tree, Vector2Tree> planner)
        {
            // initialize its own feedback PID loop
            this.planner = planner;
            
            feedback_loop = new Feedback();
        }

        /// <summary>
        /// Takes the current position and destination of a robot and creates a path for it
        /// </summary>
        /// <param name="currentPosition">The current position of the robot</param>
        /// <param name="currentDestination">The destination of the robot</param>
        /// <param name="obstacles">A current list of obstacles on the board</param>
        public void setStickyPath(RobotInfo currentPosition, RobotInfo currentDestination, List<Obstacle> obstacles) {
            // if the destination has changed or path doesn't exist, create a completely new path
            if (!(currentDestination.Equals(destination)) || stickypath == null) {
                resetPath(currentPosition, currentDestination, obstacles);
                return;
            }

            // If there are any obstacles in the way, recalculate path from nearest waypoint
            // check obstacles
            bool recalculatepath = false;

            for (int i = 0; i < obstacles.Count; i++) {
                for (int j = 0; j < stickypath.Count; j++) {
                    //For this obstacle and waypoint, check if there is a conflict
                    Obstacle current_obstacle = obstacles[i];
                    Vector2 current_waypoint = stickypath[j];
                    double mindist = current_obstacle.size;
                    if (current_obstacle.position.distanceSq(current_waypoint) <= mindist) {
                        recalculatepath = true;
                        break;
                    }
                }
                if (recalculatepath)
                    break;
            }

            if (recalculatepath) {
                recalculatePath(currentPosition, currentDestination, obstacles);
            }
        }

        /// <summary>
        /// Completely reset path from the current position
        /// </summary>
        /// <param name="currentPosition"></param>
        /// <param name="currentDestination"></param>
        /// <param name="obstacles"></param>
        private void resetPath(RobotInfo currentPosition, RobotInfo currentDestination, List<Obstacle> obstacles) {
            Pair<List<Vector2>, List<Vector2>> path = planner.Plan(currentPosition.Position, 
                currentDestination.Position, obstacles);
            List<Vector2> waypoints = path.First;
            waypoints.AddRange(path.Second);

            stickypath = waypoints;
            max_velocities = getMaxVelocities();
        }
        /// <summary>
        /// Recalculates path from nearest waypoint
        /// </summary>
        /// <param name="currentPosition"></param>
        /// <param name="currentDestination"></param>
        /// <param name="obstacles"></param>
        private void recalculatePath(RobotInfo currentPosition, RobotInfo currentDestination, 
            List<Obstacle> obstacles) {
            // Find nearest waypoint
            // Keep track of shortest distance squared
            double shortest_dist = -1;
            Vector2 closest_point = null;
            for (int i = 0; i < stickypath.Count; i++) {
                double dist = stickypath[i].distanceSq(currentPosition.Position);
                if (dist < shortest_dist || shortest_dist == -1) {
                    shortest_dist = dist;
                    closest_point = stickypath[i];
                }
            }

            Pair<List<Vector2>, List<Vector2>> path = planner.Plan(closest_point, 
                currentDestination.Position, obstacles);
            List<Vector2> waypoints = path.First;
            waypoints.AddRange(path.Second);

            stickypath = waypoints;
            max_velocities = getMaxVelocities();

        }

        /// <summary>
        /// Calculate the maximum velocities of each point in stickypath based on the
        /// curvature- currently just returns max velocities of each
        /// </summary>
        /// <returns></returns>
        private List<PointVelocity> getMaxVelocities() {
            List<PointVelocity> ret_list = new List<PointVelocity>();
            for (int i = 0; i < stickypath.Count; i++) {
                // TODO: find actual max velocities based on curvature of points
                ret_list.Add(new PointVelocity(maxX, maxY, maxOmega));
            }
            return ret_list;
        }
    }
}
