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
        
        public RobotPath GetPath(Team team, int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius) {
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

        public RobotPath GetPath(Team team, int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius)
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
            ConstantsRaw.Load();

            REPULSION_FACTOR = ConstantsRaw.get<double>("motionplanning", "REPULSION_FACTOR");
            PERPINDICULAR_LENGTH = ConstantsRaw.get<double>("motionplanning", "PERPINDICULAR_LENGTH");
        }
    }

    public class TangentBugPlanner : IPathPlanner
    {
        double LOOK_AHEAD_DIST;
        double AVOID_DIST;
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

        public RobotPath GetPath(Team team, int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius)
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
                    obstacleSizes.Add(AVOID_DIST);
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
            ConstantsRaw.Load();

            LOOK_AHEAD_DIST = ConstantsRaw.get<double>("motionplanning", "LOOK_AHEAD_DIST");
            AVOID_DIST = ConstantsRaw.get<double>("motionplanning", "AVOID_DIST");
            WAYPOINT_DIST = ConstantsRaw.get<double>("motionplanning", "WAYPOINT_DIST");
            MIN_ABS_VAL_STICK = ConstantsRaw.get<double>("motionplanning", "MIN_ABS_VAL_STICK");
            EXTRA_GOAL_DIST = ConstantsRaw.get<double>("motionplanning", "EXTRA_GOAL_DIST");
        	STEADY_STATE_SPEED = ConstantsRaw.get<double>("motionplanning", "STEADY_STATE_SPEED");

            
            MIN_X_ROBOT_BOUNDARY = ConstantsRaw.get<double>("motionplanning", "MIN_X_ROBOT_BOUNDARY");
            MAX_X_ROBOT_BOUNDARY = ConstantsRaw.get<double>("motionplanning", "MAX_X_ROBOT_BOUNDARY");
            MIN_Y_ROBOT_BOUNDARY = ConstantsRaw.get<double>("motionplanning", "MIN_Y_ROBOT_BOUNDARY");
            MAX_Y_ROBOT_BOUNDARY = ConstantsRaw.get<double>("motionplanning", "MAX_Y_ROBOT_BOUNDARY");

            BOUNDARY_AVOID = ConstantsRaw.get<double>("motionplanning", "BOUNDARY_AVOID");

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
            
            //ROTATE_ANGLE = ConstantsRaw.get<double>("motionplanning", "ROTATE_ANGLE");
            //ITER_INCREMENT = ConstantsRaw.get<double>("motionplanning", "ITER_INCREMENT");
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

	/// <summary>
	/// A basic unidirectional RRT planner that searches over a Vector2 space. A wrapper around the generic BasicRRTPlanner.
	/// The conversion Vector2->RobotInfo (to return RobotPath) is oversimplified - velocities are assigned with constant
	/// speed in the direction of the next waypoint
	/// </summary>
	public class BasicRRTMotionPlanner : IPathPlanner
	{
		double AVOID_DIST;
		double STEADY_STATE_SPEED;

		private readonly BasicRRTPlanner<Vector2, Vector2Tree> planner;

		public BasicRRTMotionPlanner()
		{
			planner = new BasicRRTPlanner<Vector2, Vector2Tree>(Common.ExtendVV, Common.RandomStateV);
		}

		public RobotPath GetPath(Team team, int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius)
		{
			List<Obstacle> obstacles = new List<Obstacle>();
			//Aviod all robots, but myself
			foreach (RobotInfo info in predictor.GetRobots())
			{
				if (info.Team != team || info.ID != id)
					obstacles.Add(new Obstacle(info.Position, AVOID_DIST));
			}
			//If needed, avoid ball
            if (avoidBallRadius > 0 &&
                predictor.GetBall() != null &&
                predictor.GetBall().Position != null)
                obstacles.Add(new Obstacle(predictor.GetBall().Position, avoidBallRadius));

			RobotInfo curinfo;
            try
            {
                curinfo = predictor.GetRobot(team, id);
            }
            catch (ApplicationException e)
            {
                return new RobotPath(team, id);
            }
			List<Vector2> path = planner.Plan(curinfo.Position, desiredState.Position, obstacles);

			List<RobotInfo> robotPath = new List<RobotInfo>();

			//Overly simplistic conversion from planning on position vectors to RobotInfo that has orientation and velocity information
			//velocity at every waypoint just points to next one with constant speed
			for (int i = 0; i < path.Count; i++)
			{
				//0-th waypoint is current state, driver won't like it
				if (i == 0)
					continue;

				RobotInfo waypoint = new RobotInfo(path[i], desiredState.Orientation, team, id);				
				if (i < path.Count - 1)
					waypoint.Velocity = (path[i + 1] - path[i]).normalizeToLength(STEADY_STATE_SPEED);
				else
					waypoint.Velocity = new Vector2(0, 0); //Stop at destination
				
				robotPath.Add(waypoint);
			}

			return new RobotPath(robotPath);
		}


        /*

        public class BidirectionalRRTMotionPlanner : IPathPlanner
	{
		double AVOID_DIST;
		double STEADY_STATE_SPEED;

		private readonly BidirectionalRRTPlanner<Vector2,Vector2, Vector2Tree, Vector2Tree> planner;

		public BidirectionalRRTMotionPlanner()
		{
			planner = new BidirectionalRRTPlanner<Vector2, Vector2, Vector2Tree, Vector2Tree>(Common.ExtendVV, Common.ExtendVV, Common.ExtendVV, Common.ExtendVV, Common.RandomStateV, Common.RandomStateV);
		}

		public RobotPath GetPath(Team team, int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius)
		{
			List<Obstacle> obstacles = new List<Obstacle>();
			//Aviod all robots, but myself
			foreach (RobotInfo info in predictor.GetRobots())
			{
				if (info.Team != team || info.ID != id)
					obstacles.Add(new Obstacle(info.Position, AVOID_DIST));
			}
			//If needed, avoid ball
			if (avoidBallRadius > 0 && predictor.GetBall().Position != null)
				obstacles.Add(new Obstacle(predictor.GetBall().Position, avoidBallRadius));

			RobotInfo curinfo = predictor.GetRobot(team, id);
			Pair<List<Vector2>, List<Vector2>> path = planner.Plan(curinfo.Position, desiredState.Position, obstacles);

			List<RobotInfo> robotPath = new List<RobotInfo>();

			//Overly simplistic conversion from planning on position vectors to RobotInfo that has orientation and velocity information
			//velocity at every waypoint just points to next one with constant speed
			for (int i = 0; i < path.Count; i++)
			{
				//0-th waypoint is current state, driver won't like it
				if (i == 0)
					continue;

				RobotInfo waypoint = new RobotInfo(path[i], desiredState.Orientation, team, id);				
				if (i < path.Count - 1)
					waypoint.Velocity = (path[i + 1] - path[i]).normalizeToLength(STEADY_STATE_SPEED);
				else
					waypoint.Velocity = new Vector2(0, 0); //Stop at destination
				
				robotPath.Add(waypoint);
			}

			return new RobotPath(robotPath);
		}
         * */

		public void ReloadConstants()
		{
			AVOID_DIST = ConstantsRaw.get<double>("motionplanning", "AVOID_DIST");
			STEADY_STATE_SPEED = ConstantsRaw.get<double>("motionplanning", "STEADY_STATE_SPEED");
		}
	}

}