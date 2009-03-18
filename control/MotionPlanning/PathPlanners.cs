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
        double ITER_INCREMENT;

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
            double rotateAmount = 0;
            Vector2 currentPath;

            while (true)
            {
                // check if rotate amount works either way, otherwise keep rotating
                currentPath = rotateDegree(finalDirection, rotateAmount);

                Console.WriteLine("Path: " + currentPath.ToString());
                Console.WriteLine("Rotate amount: " + rotateAmount);

                if (isGoodPath(obstaclePositions, start, currentPath))
                {
                    break;
                }

                currentPath = rotateDegree(finalDirection, -rotateAmount);

                if (isGoodPath(obstaclePositions, start, currentPath))
                {
                    break;
                }

                rotateAmount = rotateAmount + ITER_INCREMENT;
            }

            Console.WriteLine("IT'S A GOOD PATH AT " + rotateAmount + " DEGREES");

            // create path
            Console.WriteLine("FINAL " + currentPath);

            lastWaypoint = start + currentPath;

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
            ITER_INCREMENT = Constants.get<double>("motionplanning", "ITER_INCREMENT");
        }

        private bool isOutOfWay(Vector2 position, Vector2 start, Vector2 pathVector)
        {

            // get position relative to start
            Vector2 relativePosition = position - start;

            // project relativePosition onto pathVector

            double hypotenuse = Math.Sqrt(relativePosition.magnitudeSq());
            double angleBetween = UsefulFunctions.angleDifference(pathVector.cartesianAngle(),
                relativePosition.cartesianAngle());

            // is it completely out of the way:
            if (Math.Abs(angleBetween) > Math.PI / 2)
                return true;

            Console.WriteLine("hypotenuse = " + hypotenuse + "; angleBetween = " + angleBetween);

            double opposite = hypotenuse * Math.Sin(angleBetween);

            //return whether the obstacle is far enough off path or is completely out of way
            return (opposite > AVOID_DIST);
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

        private bool isGoodPath(List<Vector2> obstacles, Vector2 start, Vector2 pathVector)
        {
            foreach (Vector2 o in obstacles)
            {
                if (!isOutOfWay(o, start, pathVector))
                {
                    return false;
                }
            }

            return true;
        }
    }
}