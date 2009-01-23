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
        private Vector2 velocity;
        private double omega;

        public PointVelocity(Vector2 velocity, double omega)
        {
            this.velocity = velocity;
            this.omega = omega;
        }

        public Vector2 Velocity {
            get { return velocity; }
            set { velocity = value; }
        }
        public double Omega {
            get { return omega; }
            set { omega = value; }
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

        private List<RobotInfo> stickypath = null;
        
        private RobotInfo destination = null;

        int id;
        private BidirectionalRRTPlanner<Vector2, Vector2, Vector2Tree, Vector2Tree> planner;

        /// <summary>
        /// Construct a StickyPath with a planner
        /// </summary>
        /// <param name="Planner"></param>
        public StickyPath(int id, BidirectionalRRTPlanner<Vector2, Vector2, Vector2Tree, Vector2Tree> planner)
        {
            // keep RRT planner that is given
            this.id = id;
            this.planner = planner;

            // initialize its own feedback PID loop
            feedback_loop = new Feedback(id);
        }

        /// <summary>
        /// Takes the current position and destination of a robot and creates a path for it, returns speeds
        /// of wheels
        /// </summary>
        /// <param name="currentPosition">The current position of the robot</param>
        /// <param name="currentDestination">The destination of the robot</param>
        /// <param name="obstacles">A current list of obstacles on the board</param>
        public WheelSpeeds getPath(RobotInfo currentPosition, RobotInfo currentDestination, List<Obstacle> obstacles) {
            // if the destination has changed or path doesn't exist, create a completely new path
            if (!(currentDestination.Equals(destination)) || stickypath == null) {
                resetPath(currentPosition, currentDestination, obstacles);
            } else {
                // If there are any obstacles in the way, recalculate path from nearest waypoint
                // check obstacles
                bool recalculatepath = false;

                for (int i = 0; i < obstacles.Count; i++) {
                    for (int j = 0; j < stickypath.Count; j++) {
                        //For this obstacle and waypoint, check if there is a conflict
                        Obstacle current_obstacle = obstacles[i];
                        Vector2 current_waypoint_vector = stickypath[j].Position;
                        double mindist = current_obstacle.size;
                        if (current_obstacle.position.distanceSq(current_waypoint_vector) <= mindist) {
                            recalculatepath = true;
                            break;
                        }
                    }
                    if (recalculatepath) {
                        break;
                    }
                }

                if (recalculatepath) {
                    recalculatePath(currentPosition, currentDestination, obstacles);
                }
            }
            return getWheelSpeeds(currentPosition);
        }

        /// <summary>
        /// Return speeds of wheels based on the current position of the robot and the
        /// path
        /// </summary>
        /// <returns></returns>
        private WheelSpeeds getWheelSpeeds(RobotInfo currentPosition) {
            //create a RobotInfo object with the desired position, orientation and velocity
            RobotInfo desired_state = getNearestWaypoint(currentPosition);
            return feedback_loop.computeWheelSpeeds(currentPosition, desired_state);
        }

        /// <summary>
        /// Given a set of vectors representing waypoints, return a list of RobotInfo states
        /// representing those waypoints and their desired velocities
        /// </summary>
        /// <param name="waypoints"></param>
        private List<RobotInfo> createPath(List<Vector2> waypoints, RobotInfo startState, RobotInfo desiredState) {
            // iterate over waypoints, for each, create a RobotInfo object with that position
            List<RobotInfo> ret_list = new List<RobotInfo>();
            double start_orientation = startState.Orientation;
            double final_orientation = desiredState.Orientation;
            double incremental_turn = (final_orientation - start_orientation) / (waypoints.Count + 1);
            
            //Find velocities at each point
            List<PointVelocity> velocities = getVelocities(waypoints);

            for (int i = 0; i < waypoints.Count; i++) {
                // Construct an appropriate RobotInfo object
                // Find orientation of robot at this point based on final orientation
                // TODO: Adjust orientations to maximize speed- be oriented in right direction
                double this_orientation = start_orientation + incremental_turn * i;

                //Find velocity from list of velocities
                PointVelocity this_velocity = velocities[i];
                Vector2 this_v = this_velocity.Velocity;
                double this_omega = this_velocity.Omega;

                RobotInfo this_state = new RobotInfo(waypoints[i], this_v, this_omega, this_orientation, id);
                ret_list.Add(this_state);
            }

            return ret_list;
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

            stickypath = createPath(waypoints, currentPosition, currentDestination);
        }

        /// <summary>
        /// Recalculates path from nearest waypoint
        /// </summary>
        /// <param name="currentPosition"></param>
        /// <param name="currentDestination"></param>
        /// <param name="obstacles"></param>
        private void recalculatePath(RobotInfo currentPosition, RobotInfo currentDestination, 
            List<Obstacle> obstacles) {
            RobotInfo closest_point = getNearestWaypoint(currentPosition);

            Pair<List<Vector2>, List<Vector2>> path = planner.Plan(closest_point.Position, 
                currentDestination.Position, obstacles);
            List<Vector2> waypoints = path.First;
            waypoints.AddRange(path.Second);

            stickypath = createPath(waypoints, currentPosition, currentDestination);
        }

        /// <summary>
        /// Return the nearest waypoint in the current path to a RobotInfo position
        /// </summary>
        /// <param name="currentPosition"></param>
        /// <returns></returns>
        private RobotInfo getNearestWaypoint(RobotInfo currentPosition) {
            // Find nearest waypoint
            // Keep track of shortest distance squared
            double shortest_dist = -1;
            RobotInfo closest_point = null;
            for (int i = 0; i < stickypath.Count; i++) {
                double dist = stickypath[i].Position.distanceSq(currentPosition.Position);
                if (dist < shortest_dist || shortest_dist == -1) {
                    shortest_dist = dist;
                    closest_point = stickypath[i];
                }
            }
            return closest_point;
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
                ret_list.Add(new PointVelocity(new Vector2(maxX, maxY), maxOmega));
            }
            return ret_list;
        }

        /// <summary>
        /// Find velocities based on maximum velocities and max acceleration/deceleration
        /// Currently just returns half of max velocity for every point
        /// </summary>
        /// <returns></returns>
        private List<PointVelocity> getVelocities(List<Vector2> waypoints) {
            // get velocities based on current maximum velocities
            List<PointVelocity> ret_list = new List<PointVelocity>();
            for (int i = 0; i < waypoints.Count; i++) {
                // TODO: find actual velocities based on maxima
                ret_list.Add(new PointVelocity(new Vector2(maxX / 2, maxY / 2), maxOmega / 2));
            }
            return ret_list;
        }
    }
}
