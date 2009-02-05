using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Core;
using Robocup.CoreRobotics;

namespace Robocup.MotionControl {
    /// <summary>
    /// Describes a path to a destination as a series of RobotInfo waypoints
    /// </summary>
    public class RobotPath {
        // Store id
        int _id;

        // Store path as a list of robot info objects
        List<RobotInfo> _path;

        // Can be constructed multiple ways, depending on how the path is determined

        /// <summary>
        /// Given a list of RobotInfo waypoints
        /// </summary>
        /// <param name="waypoints">RobotInfo waypoints along determined path</param>
        public RobotPath(List<RobotInfo> waypoints) {
            // take id and path from given waypoints
            if (waypoints.Count == 0)
                throw new Exception("Empty path given to path constructor");

            _id = waypoints[0].ID;

            _path = waypoints;
        }

        /// <summary>
        /// Constructed with a starting list of RobotInfo objects and ending list of Vector2 waypoints
        /// </summary>
        /// <param name="waypoints1">RobotInfo starting list of waypoints</param>
        /// <param name="waypoints2">Vector2 ending list of waypoints</param>
        public RobotPath(List<RobotInfo> waypoints1, List<Vector2> waypoints2) {
            _id = waypoints1[0].ID;

            // Combine paths into a single waypoints list
            _path = waypoints1;
            _path.AddRange(makeRobotInfoList(_id, waypoints2));
        }

        /// <summary>
        /// Can be initialized with a single list of vectors and a Robot ID, which are
        /// converted into a list of RobotInfo objects
        /// </summary>
        /// <param name="id">ID of robot</param>
        /// <param name="waypoints">Vector2 list of waypoints along path</param>
        public RobotPath(int id, List<Vector2> waypoints) {
            _id = id;
            _path = makeRobotInfoList(_id, waypoints);
        }

        // Public interface for variables
        public int ID {
            get { return _id; }
        }

        public List<RobotInfo> Waypoints {
            get { return _path; }
        }

        /// <summary>
        /// Turn a list of Vector2 into a list of RobotInfo objects- each is oriented towards next waypoint.
        /// </summary>
        /// <param name="waypoints"></param>
        /// <returns></returns>
        private List<RobotInfo> makeRobotInfoList(int id, List<Vector2> waypoints) {
            List<RobotInfo> retlst = new List<RobotInfo>();
            double orientation = 0;
            for (int i = 0; i < waypoints.Count; i++) {
                // Point orientation towards next waypoint if there is one- otherwise,
                // do not change it
                if (i < (waypoints.Count - 1)) {
                    orientation = (waypoints[i+1] - waypoints[i]).cartesianAngle();
                }
                retlst.Add(new RobotInfo(waypoints[i], orientation, id));
            }

            return retlst;
        }

        /// <summary>
        /// Given a RobotInfo object, return the nearest waypoint to that object
        /// </summary>
        /// <param name="point">A RobotInfo representing the current position</param>
        /// <returns></returns>
        public RobotInfo findNearestWaypoint(RobotInfo point) {
            //Call findNearestWaypointIndex
            return _path[findNearestWaypointIndex(point)];
        }

        /// <summary>
        /// Given a RobotInfo object, return the index of the nearest waypoint to that object
        /// </summary>
        /// <param name="point">A RobotInfo representing the current position</param>
        /// <returns></returns>
        public int findNearestWaypointIndex(RobotInfo point) {
            // for now, brute force search

            int closestWaypointIndex = 0;
            double minDistSq = double.MaxValue;

            for (int i = 0; i < _path.Count; i++) {
                RobotInfo waypoint = _path[i];
                double distSq = waypoint.Position.distanceSq(point.Position);
                if (distSq < minDistSq) {
                    closestWaypointIndex = i;
                    minDistSq = distSq;
                }
            }

            return closestWaypointIndex;
        }

        /// <summary>
        /// Get waypoint by index
        /// </summary>
        /// <param name="index">Index of desired waypoint</param>
        /// <returns></returns>
        public RobotInfo getWaypoint(int index) {
            return _path[index];
        }
    }
}
