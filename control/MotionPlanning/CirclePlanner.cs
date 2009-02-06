using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Core;
using Robocup.CoreRobotics;


namespace Robocup.MotionControl {
    public class CirclePlanner {

        private const double RADIUS = 0.5; // meters?
        private const int NUM_WAYPOINTS = 25; // designed with an even number in mind
        private const double SPEED = .4; // meters/sec
        private const double ANGULAR_V = 0; // rad/sec, 0 for now
                
        private const double ANGLE_STEP = 2 * Math.PI / NUM_WAYPOINTS;     
   
        private Pair<List<RobotInfo>, List<Vector2>> _lastPath;
        private Object _lastPathLock = new Object();

        public Pair<List<RobotInfo>, List<Vector2>> LastPath {
            get {
                Pair<List<RobotInfo>, List<Vector2>> lastPath;
                lock (_lastPathLock) {
                    lastPath = _lastPath;
                }
                return lastPath;
            }
        }

        public Pair<List<RobotInfo>, List<Vector2>> Plan(RobotInfo currInfo, RobotInfo desiredState, List<Obstacle> obstacles) {

            double x, y, orientation;
            double x_prev, y_prev;

            Vector2 center = desiredState.Position;
            List<RobotInfo> waypoints = new List<RobotInfo>();

            x_prev = center.X + RADIUS * Math.Cos(0);
            y_prev = center.Y + RADIUS * Math.Sin(0);
            orientation = 0;

            for (double t = ANGLE_STEP; t - 2.0 * Math.PI < 0.0001; t += ANGLE_STEP) {
                x = center.X + RADIUS * Math.Cos(t);
                y = center.Y + RADIUS * Math.Sin(t);

                Vector2 position = new Vector2(x, y);
                Vector2 velocity = new Vector2(x - x_prev, y - y_prev);
                velocity = velocity.normalize();
                velocity = SPEED * velocity;

                // strage that robotID is required => set it to 0                 
                waypoints.Add(new RobotInfo(position, velocity, ANGULAR_V, orientation, 0));

                x_prev = x;
                y_prev = y;
                orientation += 3*ANGLE_STEP;//for now lets not have the robot turn to make it easier to tune/test
            }          

            Pair<List<RobotInfo>, List<Vector2>> path = new Pair<List<RobotInfo>, List<Vector2>>(waypoints, new List<Vector2>());

            lock (_lastPathLock) {
                _lastPath = path;
            }

            return path;
            
            
        }
    }
}
