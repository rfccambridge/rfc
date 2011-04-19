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
        
        public RobotPath GetPath(Team team, int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius,
            RobotPath oldPath) {
            List<RobotInfo> waypoints = new List<RobotInfo>();
            waypoints.Add(desiredState);
            return new RobotPath(waypoints);
        }
      
        public void ReloadConstants() { 
        
        }
    }

    // PathPlanner version of the BugNavigator INavigator
    public class BugNavigatorPlanner : NavigatorPlanner {
        static BugNavigator navigator = new BugNavigator();

        public BugNavigatorPlanner() : base(navigator) { }
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

        public RobotPath GetPath(Team team, int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius,
            RobotPath oldPath)
        {
            RobotInfo currentState;
            try
            {
                currentState = predictor.GetRobot(team, id);
            }
            catch (ApplicationException e)
            {
                return new RobotPath(team, id);
            }

            Vector2 position = currentState.Position;

            // get list of obstacle positions
            List<Vector2> obstaclePositions = new List<Vector2>();
            foreach (RobotInfo info in predictor.GetRobots())
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

            RobotPath path = new RobotPath(team, id, waypoints);

            return path;
        }

        public void ReloadConstants()
        {
            Constants.Load("motionplanning");

            REPULSION_FACTOR = Constants.get<double>("motionplanning", "REPULSION_FACTOR");
            PERPINDICULAR_LENGTH = Constants.get<double>("motionplanning", "PERPINDICULAR_LENGTH");
        }
    }

    public class TangentBugPlanner : IPathPlanner
    {
        double LOOK_AHEAD_DIST;
        double TBUG_AVOID_DIST;
        double MIN_ABS_VAL_STICK;
        public double WAYPOINT_DIST;
        double EXTRA_GOAL_DIST;
        double BOUNDARY_AVOID;

        double MIN_X_ROBOT_BOUNDARY;
        double MAX_X_ROBOT_BOUNDARY;
        double MIN_Y_ROBOT_BOUNDARY;
        double MAX_Y_ROBOT_BOUNDARY;

    	double STEADY_STATE_SPEED;

        // line segments that must be avoided- field edges and goals
        List<Line> boundary_lines;

        double previousAngle;

        Vector2 lastWaypoint;        

        public TangentBugPlanner()
        {            
            ReloadConstants();
            previousAngle = 0;
        }

        public RobotPath GetPath(Team team, int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius,
            RobotPath oldPath)
        {
            RobotInfo currentState;
            try
            {
                currentState = predictor.GetRobot(team, id);
            }
            catch (ApplicationException e)
            {
                return new RobotPath(team, id);
            }

            Vector2 start = currentState.Position;
            Vector2 end = desiredState.Position;

            // keep track of final direction
            Vector2 finalDirection = new Vector2();

            // add direction to goal
            Vector2 directionToGoal = (end - start);

            // if close to the goal, decrease look ahead distance
            if (directionToGoal.magnitudeSq() < WAYPOINT_DIST * WAYPOINT_DIST) {
                //Note: this implies zero velocity!
				return new RobotPath(team, id, desiredState);
            }

            // get desired waypoint
            List<Vector2> obstaclePositions = new List<Vector2>();
            List<double> obstacleSizes = new List<double>();
            foreach (RobotInfo info in predictor.GetRobots())
            {
                if (info.ID != id) {
                    obstaclePositions.Add(info.Position);
                    obstacleSizes.Add(TBUG_AVOID_DIST);
                }
                //Console.WriteLine(info.ToString());
            }

            if (avoidBallRadius > 0){
                BallInfo ballInfo = predictor.GetBall();
                if (ballInfo != null) {
                    obstaclePositions.Add(ballInfo.Position);
                    obstacleSizes.Add(avoidBallRadius);
                }
            }


            finalDirection = finalDirection + directionToGoal;

            //normalize to look ahead distance
            finalDirection = finalDirection.normalizeToLength(WAYPOINT_DIST);
            double finalDirectionAngle = finalDirection.cartesianAngle();

            // create possible number range
            NumberRange angle_range = new NumberRange(-Math.PI, Math.PI, 720);

            double obstacleDist;
            double obstacleAngle;
            double angleDiff;
            double angleAvoid;

            // remove impossible angles
            for (int i=0; i < obstaclePositions.Count; i++) {

                Vector2 obstPos = obstaclePositions[i];
                // get distance to obstacle
                obstacleDist = start.distanceSq(obstPos);

                // if it is too far away, don't worry
                // HACK: can be same obstacle, so avoid changes that are way too low. This is a problem.
                if (obstacleDist > LOOK_AHEAD_DIST || obstacleDist < .001)
                {
                    continue;
                }

                // get angle to obstacle
                obstacleAngle = (obstPos-start).cartesianAngle();
                angleDiff = UsefulFunctions.angleDifference(finalDirectionAngle, obstacleAngle);

                // get amount around angle to avoid
                // note- isn't perfect tangent
                angleAvoid = Math.Atan(obstacleSizes[i] / obstacleDist);
                //Console.WriteLine("FINAL: " + finalDirectionAngle + " OBSTACLE: " + obstacleAngle);
                //Console.WriteLine("Avoiding obstacle at distance " + obstacleDist);
                //Console.WriteLine("Obstacle is at angle " + angleDiff);
                //Console.WriteLine("Avoiding from " + (angleDiff - angleAvoid) + " to " + (angleDiff + angleAvoid));

                // remove this range of the possible angles
                angle_range.remove(angleDiff - angleAvoid, angleDiff + angleAvoid);

            }
            
            // avoid boundary lines, but only if inside boundaries
            double dist_to_line;
            // do not perform this loop if not inside boundaries
            if (start.X > MIN_X_ROBOT_BOUNDARY && start.X < MAX_X_ROBOT_BOUNDARY &&
                start.Y > MIN_Y_ROBOT_BOUNDARY && start.Y < MAX_Y_ROBOT_BOUNDARY) {
                foreach (Line l in boundary_lines) {
                    // get distance from line
                    dist_to_line = l.distFromLine(start);
                    // ignore lines more than a certain distance
                    if (dist_to_line > BOUNDARY_AVOID) {
                        continue;
                    }

                    // avoid line
                    Vector2 proj_point = l.projectionOntoLine(start);
                    Vector2 dir_to_point = proj_point - start;
                    double dist_to_point = Math.Sqrt(dir_to_point.magnitudeSq());

                    angleAvoid = Math.Acos(dist_to_point / BOUNDARY_AVOID);
                    angleDiff = UsefulFunctions.angleDifference(finalDirectionAngle, dir_to_point.cartesianAngle());

                    // remove this range of the possible angles
                    angle_range.remove(angleDiff - angleAvoid, angleDiff + angleAvoid);
                }
            }

            // use closest angle to center
            double bestAngle;

            // Once a side has been picked, stick to it
            // THIS IS A LITTLE BIT OF A HACK!
            if (previousAngle > MIN_ABS_VAL_STICK)
            {
                 bestAngle = angle_range.closestToCenter(-MIN_ABS_VAL_STICK, 100);
            }
            else if (previousAngle < -MIN_ABS_VAL_STICK)
            {
                bestAngle = angle_range.closestToCenter(-100, MIN_ABS_VAL_STICK);
            }
            else
            {
                bestAngle = angle_range.closestToCenter(-100, 100);
            }

            if (bestAngle == -1000) {
                // no good route- return empty path
                Console.WriteLine("NO GOOD ROUTE!");
                return new RobotPath(team, id);

            }

            /*
            if (Math.Abs(bestAngle) > MIN_ABS_VAL_STICK && Math.Abs(previousAngle) > MIN_ABS_VAL_STICK && 
                Math.Abs(bestAngle - previousAngle) > 2 * MIN_ABS_VAL_STICK) {
                double bestAngle = angle_range.closestToCenter();

            }
             */

            finalDirection = rotateDegree(finalDirection, bestAngle);

            previousAngle = bestAngle;

            lastWaypoint = start + finalDirection;

            List<RobotInfo> waypoints = new List<RobotInfo>();

        	Vector2 velocity = finalDirection.normalizeToLength(STEADY_STATE_SPEED);
			//State representing carrot on a stick
            RobotInfo carrotState = new RobotInfo(lastWaypoint, 
				velocity, 0, desiredState.Orientation, team, id);
			waypoints.Add(carrotState);


            //Add more waypoints along the line carrotState -> desiredState
            //That way, more complicated followers can follow the whole path
            int numWaypoints = Convert.ToInt32(Math.Floor(Math.Sqrt(
                desiredState.Position.distanceSq(carrotState.Position)) / WAYPOINT_DIST));

            Vector2 currentPosition = lastWaypoint;
            for (int i = 0; i < numWaypoints; i++)
            {
                currentPosition += finalDirection;
                RobotInfo newWaypoint = new RobotInfo(currentPosition, velocity, 0,
                    desiredState.Orientation, team, id);
                waypoints.Add(newWaypoint);
            }


            //Console.WriteLine("LAST WAYPOINT: " + lastWaypoint.ToString());

            RobotPath path = new RobotPath(waypoints);

            return path;
        }

        public void ReloadConstants()
        {
            Constants.Load("motionplanning");

            LOOK_AHEAD_DIST = Constants.get<double>("motionplanning", "LOOK_AHEAD_DIST");
            TBUG_AVOID_DIST = Constants.get<double>("motionplanning", "TBUG_AVOID_DIST");
            WAYPOINT_DIST = Constants.get<double>("motionplanning", "WAYPOINT_DIST");
            MIN_ABS_VAL_STICK = Constants.get<double>("motionplanning", "MIN_ABS_VAL_STICK");
            EXTRA_GOAL_DIST = Constants.get<double>("motionplanning", "EXTRA_GOAL_DIST");
        	STEADY_STATE_SPEED = Constants.get<double>("motionplanning", "STEADY_STATE_SPEED");

            
            MIN_X_ROBOT_BOUNDARY = Constants.get<double>("motionplanning", "MIN_X_ROBOT_BOUNDARY");
            MAX_X_ROBOT_BOUNDARY = Constants.get<double>("motionplanning", "MAX_X_ROBOT_BOUNDARY");
            MIN_Y_ROBOT_BOUNDARY = Constants.get<double>("motionplanning", "MIN_Y_ROBOT_BOUNDARY");
            MAX_Y_ROBOT_BOUNDARY = Constants.get<double>("motionplanning", "MAX_Y_ROBOT_BOUNDARY");

            BOUNDARY_AVOID = Constants.get<double>("motionplanning", "BOUNDARY_AVOID");

            // field edges
            // corner points
            Vector2 topleft = new Vector2(MIN_X_ROBOT_BOUNDARY, MAX_Y_ROBOT_BOUNDARY);
            Vector2 topright = new Vector2(MAX_X_ROBOT_BOUNDARY, MAX_Y_ROBOT_BOUNDARY);
            Vector2 bottomleft = new Vector2(MIN_X_ROBOT_BOUNDARY, MIN_Y_ROBOT_BOUNDARY);
            Vector2 bottomright = new Vector2(MAX_Y_ROBOT_BOUNDARY, MIN_Y_ROBOT_BOUNDARY);

            // create boundary lines
            boundary_lines = new List<Line>();
            // top
            boundary_lines.Add(new Line(topleft, topright));
            // left
            boundary_lines.Add(new Line(topleft, bottomleft));
            // bottom
            boundary_lines.Add(new Line(bottomleft, bottomright));
            // right
            boundary_lines.Add(new Line(topright, bottomright));
            
            //ROTATE_ANGLE = Constants.get<double>("motionplanning", "ROTATE_ANGLE");
            //ITER_INCREMENT = Constants.get<double>("motionplanning", "ITER_INCREMENT");
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
            return (opposite > TBUG_AVOID_DIST);
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

	/// <summary>
	/// A basic unidirectional RRT planner that searches over a Vector2 space. A wrapper around the generic BasicRRTPlanner.
	/// The conversion Vector2->RobotInfo (to return RobotPath) is oversimplified - velocities are assigned with constant
	/// speed in the direction of the next waypoint
	/// </summary>
	public class BasicRRTMotionPlanner : IPathPlanner
	{
		double RRT_OBSTACLE_AVOID_DIST;
		double STEADY_STATE_SPEED;

        private int numPathIterations = 8;
        private int numSmoothIterations = 5;
        private readonly bool doPathSmoothing;

		private readonly BasicRRTPlanner<Vector2, Vector2Tree> planner;

		public BasicRRTMotionPlanner(bool doPathSmoothing)
		{
            this.doPathSmoothing = doPathSmoothing;
			planner = new BasicRRTPlanner<Vector2, Vector2Tree>(Common.ExtendVV, Common.RandomStateV);
		}

		public RobotPath GetPath(Team team, int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius,
            RobotPath oldPath)
		{
            //Build obstacles!-------------------------------
			List<Obstacle> obstacles = new List<Obstacle>();

            //Avoid all robots, except myself
			foreach (RobotInfo info in predictor.GetRobots())
			{
				if (info.Team != team || info.ID != id)
                    obstacles.Add(new Obstacle(info.Position, RRT_OBSTACLE_AVOID_DIST));
			}

            //If needed, avoid ball
            if (avoidBallRadius > 0 &&
                predictor.GetBall() != null &&
                predictor.GetBall().Position != null)
                obstacles.Add(new Obstacle(predictor.GetBall().Position, avoidBallRadius));

            //Try to find myself
			RobotInfo curinfo;
            try
            {
                curinfo = predictor.GetRobot(team, id);
            }
            catch (ApplicationException e)
            {
                return new RobotPath(team, id);
            }

            //Plan!
            List<Vector2> bestPath = null;
            double bestPathLen = 0;
            for (int i = 0; i < numPathIterations; i++)
            {
                List<Vector2> path = planner.Plan(curinfo.Position, desiredState.Position, obstacles);

                //Smooth the path if desired
                if (doPathSmoothing)
                {
                    for (int j = 0; j < numSmoothIterations; j++)
                        smoothPath(path, obstacles);
                }

                double len = pathLen(path);
                if (len < bestPathLen || bestPath == null)
                {
                    bestPathLen = len;
                    bestPath = path;
                }
            }
            

            //Convert the path
			List<RobotInfo> robotPath = new List<RobotInfo>();

            
			//Overly simplistic conversion from planning on position vectors to RobotInfo that has orientation and velocity information
			//velocity at every waypoint just points to next one with constant speed
            //0th waypoint is current state - skip it, driver won't like it.
            for (int i = 1; i < bestPath.Count; i++)
			{
                RobotInfo waypoint = new RobotInfo(bestPath[i], desiredState.Orientation, team, id);
                if (i < bestPath.Count - 1)
                    waypoint.Velocity = (bestPath[i + 1] - bestPath[i]).normalizeToLength(STEADY_STATE_SPEED);
				else
					waypoint.Velocity = new Vector2(0, 0); //Stop at destination
				
				robotPath.Add(waypoint);
			}

            if (bestPath.Count <= 1)
                return new RobotPath(team, id);
            

			return new RobotPath(robotPath);
		}


        private double pathLen(List<Vector2> path)
        {
            double len = 0;
            for (int i = 0; i < path.Count-1; i++)
                len += (path[i + 1] - path[i]).magnitude();
            return len;
        }

        private void smoothPath(List<Vector2> path, List<Obstacle> obstacles)
        {
            //Compute sharpness of each bend
            int len = path.Count;
            if(len <= 2)
                return;
            double[] sharpnessArr = new double[len-2];
            int[] indexArr = new int[len-2];

            for (int i = 1; i < len-1; i++)
            {
                Vector2 vec1 = path[i] - path[i-1];
                Vector2 vec2 = path[i+1] - path[i];
                double sharpness;
                if (vec1.magnitudeSq() < 1e-16 || vec2.magnitudeSq() < 1e-16)
                    sharpness = 0;
                else
                    sharpness = Math.Abs(UsefulFunctions.angleDifference(vec1.cartesianAngle(), vec2.cartesianAngle()));
                sharpnessArr[i - 1] = sharpness;
                indexArr[i - 1] = i;
            }

            //Sort the most sharp to the front
            //Insertion sort, currently
            for (int i = 1; i < len - 2; i++)
            {
                int j = i;
                while (j > 0 && sharpnessArr[j - 1] < sharpnessArr[j])
                {
                    int temp = indexArr[j - 1];
                    indexArr[j - 1] = indexArr[j];
                    indexArr[j] = temp;
                    double temp2 = sharpnessArr[j - 1];
                    sharpnessArr[j - 1] = sharpnessArr[j];
                    sharpnessArr[j] = temp2;
                }
            }

            //Now, in order of sharpness, attempt to smooth the path
            for (int i = 0; i < len - 2; i++)
            {
                int idx = indexArr[i];

                Vector2 cur = path[idx];
                Vector2 prev = path[idx-1];
                Vector2 next = path[idx+1];
                Vector2 mid = (prev + next) / 2.0;

                //Try just using the mid outright
                if (!Common.SegmentBlocked(prev,next,obstacles))
                {
                    if ((next - prev).magnitudeSq() <= 0.2 * 0.2)
                    {
                        path.RemoveAt(idx); for (int j = i + 1; j < len - 2; j++)
                            if (indexArr[j] > idx) indexArr[j]--; continue;
                    }
                    else
                    {
                        path[idx] = mid;
                    }
                }

                //Try smoothing partway to the mid
                Vector2 curmid2 = (cur + mid) / 2.0;
                if (!Common.SegmentBlocked(prev, curmid2, obstacles) && !Common.SegmentBlocked(curmid2, next, obstacles))
                { path[idx] = curmid2; continue; }

                //Try smoothing partway to the mid
                Vector2 curmid4 = (cur*3 + mid) / 4.0;
                if (!Common.SegmentBlocked(prev, curmid4, obstacles) && !Common.SegmentBlocked(curmid4, next, obstacles))
                { path[idx] = curmid4; continue; }

                //Try smoothing partway to the mid
                Vector2 curmid8 = (cur * 7 + mid) / 8.0;
                if (!Common.SegmentBlocked(prev, curmid8, obstacles) && !Common.SegmentBlocked(curmid8, next, obstacles))
                { path[idx] = curmid8; continue; }

                //No? Okay, we can't smooth this one any more
            }
        }


        public void ReloadConstants()
		{
            RRT_OBSTACLE_AVOID_DIST = Constants.get<double>("motionplanning", "RRT_OBSTACLE_AVOID_DIST");
			STEADY_STATE_SPEED = Constants.get<double>("motionplanning", "STEADY_STATE_SPEED");
		}
	}

}