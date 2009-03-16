using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Core;
using Robocup.CoreRobotics;
using Robocup.Geometry;

using System.Drawing;

using Navigation.Examples;
using System.IO;
using Robocup.Utilities;

namespace Robocup.MotionControl
{
    public class DumbPathPlanner : IPathPlanner
    {
        public DumbPathPlanner() {

        }
        
        public RobotPath GetPath(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius) {
            List<RobotInfo> waypoints = new List<RobotInfo>();
            waypoints.Add(desiredState);
            return new RobotPath(waypoints);
        }

        public void DrawLast(Graphics g, ICoordinateConverter c)
        {

        }
        
        public void ReloadConstants() { 
        
        }
    }

    public class PointChargePlanner : IPathPlanner
    {
        double REPULSION_FACTOR;
        double PERPINDICULAR_LENGTH;

        Vector2 lastWaypoint;

        public PointChargePlanner()
        {
            ReloadConstants();
        }

        public RobotPath GetPath(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius)
        {
            RobotInfo currentState = predictor.getCurrentInformation(id);

            Vector2 position = currentState.Position;

            // get list of obstacle positions
            List<Vector2> obstaclePositions = new List<Vector2>();
            foreach (RobotInfo info in predictor.getAllInfos())
            {
                if (info.ID != id)
                    //TODO magic number (robot radius)
                    obstaclePositions.Add(info.Position);
            }

            // keep track of final direction
            Vector2 finalDirection = new Vector2();

            // add direction to goal
            Vector2 directionToGoal = (desiredState.Position - position);
            finalDirection = finalDirection + directionToGoal.normalize();

            // Add perpindicular component to the RIGHT
            Vector2 rightComponent = new Vector2(directionToGoal.Y, -directionToGoal.X);
            rightComponent = rightComponent.normalizeToLength(PERPINDICULAR_LENGTH);
            finalDirection = finalDirection + rightComponent;

            //normalize again
            finalDirection = finalDirection.normalize();

            // add vectors repelling from obstacles
            Vector2 thisObstacleVector;
            foreach (Vector2 o in obstaclePositions)
            {
                thisObstacleVector = (position - o);
                // add opposite vector, normalized to distance use inverse square
                if (thisObstacleVector.magnitudeSq() != 0)
                {
                    Console.WriteLine("OBSTACLE: " + thisObstacleVector.ToString());
                    thisObstacleVector = thisObstacleVector.normalizeToLength(REPULSION_FACTOR / thisObstacleVector.magnitudeSq());
                    finalDirection = finalDirection + thisObstacleVector;
                }
            }

            // create path
            Console.WriteLine("Final direction before normalizing: " + finalDirection);

            finalDirection = finalDirection.normalize();

            Console.WriteLine("FINAL " + finalDirection);

            lastWaypoint = position + finalDirection;
            List<Vector2> waypoints = new List<Vector2>();
            waypoints.Add(lastWaypoint);

            Console.WriteLine("LAST WAYPOINT: " + lastWaypoint.ToString());

            RobotPath path = new RobotPath(id, waypoints);

            return path;
        }

        public void DrawLast(Graphics g, ICoordinateConverter c)
        {
            if (lastWaypoint != null)
            {
                Brush b = new SolidBrush(Color.Blue);
                g.FillRectangle(b, c.fieldtopixelX(lastWaypoint.X) - 1, c.fieldtopixelY(lastWaypoint.Y) - 1, 4, 4);
            }
        }

        public void ReloadConstants()
        {
            Constants.Load("motionplanning");

            REPULSION_FACTOR = Constants.get<double>("motionplanning", "REPULSION_FACTOR");
            PERPINDICULAR_LENGTH = Constants.get<double>("motionplanning", "PERPINDICULAR_LENGTH");
        }
    }

    public class BugPlanner : IPathPlanner
    {
        double LOOK_AHEAD_DIST;
        double AVOID_DIST;
        double ROTATE_ANGLE;
        BugNavigator _navigator;

        Vector2 lastWaypoint;

        public BugPlanner()
        {
            _navigator = new BugNavigator();
            ReloadConstants();
        }

        public RobotPath GetPath(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius)
        {
            RobotInfo currentState = predictor.getCurrentInformation(id);

            Vector2 start = currentState.Position;
            Vector2 end = desiredState.Position;

            // get desired waypoint
            List<Vector2> obstaclePositions = new List<Vector2>();
            foreach (RobotInfo info in predictor.getAllInfos())
            {
                if (info.ID != id)
                    //TODO magic number (robot radius)
                    obstaclePositions.Add(info.Position);
            }

            // keep track of final direction
            Vector2 finalDirection = new Vector2();

            // add direction to goal
            Vector2 directionToGoal = (end - start);
            finalDirection = finalDirection + directionToGoal;

            //normalize to look ahead distance
            finalDirection = finalDirection.normalizeToLength(LOOK_AHEAD_DIST);

            // add vectors repelling from obstacles
            Vector2 thisObstacleVector;
            foreach (Vector2 o in obstaclePositions)
            {
                double distanceOff = distFrom(o, start, finalDirection);
                //double angleOff = angleFrom(o, start, finalDirection);
                //double distAlongPath = (distanceOff / Math.Tan(angleOff));

                Console.WriteLine("DISTANCE OFF: " + distanceOff);
                //Console.WriteLine("ANGLE OFF: " + angleOff);
                //Console.WriteLine("distAlongPath: " + distAlongPath);

                if (Math.Abs(distanceOff) <= AVOID_DIST)
                {
                    /* tangent bug- in progress
                    // Avoid by changing
                    double newDirection;
                    double angleOff = angleFrom(o, start, finalDirection);

                    // use trig to figure out tangent bug
                    // first, distance along path
                    double distAlongPath = (distanceOff / Math.Tan(angleOff));
                    
                    // now find new angle
                    double newAngle = Math.Atan((AVOID_DIST - distanceOff)/distAlongPath);

                    // adjust by that angle
                    double directionAngle = finalDirection.cartesianAngle();
                    double */

                    //Avoid by turning 90 degrees, in opposite of whichever direction the
                    //obstacle is in
                    //finalDirection = rotate90(finalDirection, (distanceOff < 0));
                    finalDirection = rotateDegree(finalDirection, ROTATE_ANGLE);
                }
            }

            // create path
            Console.WriteLine("FINAL " + finalDirection);

            lastWaypoint = start + finalDirection;

            //if desired, at the last second replace with BugNavigator (to compare, for example)
            //NavigationResults results = _navigator.navigate(currentState.ID, currentState.Position,
            //    desiredState.Position, predictor.getOurTeamInfo().ToArray(), predictor.getTheirTeamInfo().ToArray(), predictor.getBallInfo(),
            //    avoidBallRadius);
            //lastWaypoint = results.waypoint;

            List<Vector2> waypoints = new List<Vector2>();
            waypoints.Add(lastWaypoint);

            Console.WriteLine("LAST WAYPOINT: " + lastWaypoint.ToString());

            RobotPath path = new RobotPath(id, waypoints);

            return path;
        }

        public void DrawLast(Graphics g, ICoordinateConverter c)
        {
            if (lastWaypoint != null)
            {
                Brush b = new SolidBrush(Color.Blue);
                g.FillRectangle(b, c.fieldtopixelX(lastWaypoint.X) - 1, c.fieldtopixelY(lastWaypoint.Y) - 1, 4, 4);
            }
        }

        public void ReloadConstants()
        {
            Constants.Load("motionplanning");

            LOOK_AHEAD_DIST = Constants.get<double>("motionplanning", "LOOK_AHEAD_DIST");
            AVOID_DIST = Constants.get<double>("motionplanning", "AVOID_DIST");
            ROTATE_ANGLE = Constants.get<double>("motionplanning", "ROTATE_ANGLE");
        }

        /// <summary>
        /// Return the distance between the position and a path between the start and end
        /// positions, where positive represents a distance the position is to the left of
        /// the path and negative represents a distance the position is to the right of the path
        /// </summary>
        /// <param name="position"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private double distFrom(Vector2 position, Vector2 start, Vector2 pathVector)
        {
            // Simply return distance from end to position
            return Math.Sqrt((position - (start + pathVector)).magnitudeSq());

            // get position relative to start
            Vector2 relativePosition = position - start;

            // project relativePosition onto pathVector
            double hypotenuse = Math.Sqrt(relativePosition.magnitudeSq());
            double angleBetween = UsefulFunctions.angleDifference(pathVector.cartesianAngle(),
                relativePosition.cartesianAngle());

            double opposite = hypotenuse * Math.Sin(angleBetween);

            return opposite;
        }

        /// <summary>
        /// Return the angle between the position and a path between the start and end
        /// positions, where positive represents a distance the position is to the left of
        /// the path and negative represents a distance the position is to the right of the path
        /// </summary>
        /// <param name="position"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private double angleFrom(Vector2 position, Vector2 start, Vector2 end)
        {
            // get vector from start to end
            Vector2 pathVector = end - start;

            // get position relative to start
            Vector2 relativePosition = position - start;

            // project relativePosition onto pathVector
            double hypotenuse = Math.Sqrt(relativePosition.magnitudeSq());
            double angleBetween = UsefulFunctions.angleDifference(pathVector.cartesianAngle(), relativePosition.cartesianAngle());

            return angleBetween;
        }

        /// <summary>
        /// Given a vector and a bool as to whether clockwise, rotate vector
        /// </summary>
        /// <param name="v"></param>
        /// <param name="CW"></param>
        /// <returns></returns>
        private Vector2 rotate90(Vector2 v, bool CW)
        {
            if (CW)
            {
                return new Vector2(v.Y, -v.X);
            }
            return new Vector2(-v.Y, v.X);
        }
        private Vector2 rotateDegree(Vector2 v, double degree)
        {
            return new Vector2(v.X * Math.Cos(degree) - v.Y * Math.Sin(degree), v.X * Math.Sin(degree) + v.Y * Math.Cos(degree));
        }
    }
}