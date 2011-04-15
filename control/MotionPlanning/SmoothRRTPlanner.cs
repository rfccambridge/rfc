using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Core;
using Robocup.CoreRobotics;
using Robocup.Geometry;


namespace Robocup.MotionControl
{
    public class MovingShape
    {
        public const double RRT_ROBOT_RADIUS = 0.10;
        public const double RRT_FRONT_PLATE_RADIUS = 0.7;

        public Geom geom;

        public Vector2 center;
        public double orientation;
        public Vector2 velocity;
        public double boundingRadius;
        public double angularVelocity;

        public MovingShape(MovingShape other)
        {
            this.geom = other.geom;
            this.center = other.center;
            this.orientation = other.orientation;
            this.velocity = other.velocity;
            this.boundingRadius = other.boundingRadius;
            this.angularVelocity = other.angularVelocity;
        }

        public MovingShape(Geom geom, Vector2 center, double orientation, Vector2 velocity, double boundingRadius, double angularVelocity)
        {
            this.geom = geom;
            this.center = center;
            this.orientation = orientation;
            this.velocity = velocity;
            this.boundingRadius = boundingRadius;
            this.angularVelocity = angularVelocity;
        }

        public Geom getMovedShape(double dt)
        {
            return geom.rotateAroundPoint(center, dt * angularVelocity).translate(dt * velocity);
        }


        public MovingShape getMoved(double dt)
        {
            return new MovingShape(getMovedShape(dt), center + dt * velocity,
                orientation + dt * angularVelocity, velocity, boundingRadius, angularVelocity);
        }

        public MovingShape(RobotInfo info)
        {
            this.geom = new Circle(info.Position, RRT_ROBOT_RADIUS);
            //this.geom = new RobotShape(info.Position, RRT_ROBOT_RADIUS, info.Orientation, RRT_FRONT_PLATE_RADIUS);
            this.center = info.Position;
            this.orientation = info.Orientation;
            this.velocity = info.Velocity;
            this.boundingRadius = RRT_ROBOT_RADIUS;
            this.angularVelocity = 0;
        }
    }

    public class SmoothRRTPlanner : IPathPlanner
    {
        const double TIME_STEP = 0.12;           //Time step in secs, with velocity determines RRT extension length
        const double ROBOT_VELOCITY = 1.0;       //Assume our robot can move this fast
        const double MAX_ACCEL_PER_STEP = 0.42;  //And that it can accelerate this fast.

        const double MAX_TREE_SIZE = 600;       //Max nodes in tree before we give up
        const double MAX_PATH_TRIES = 200;       //Max number of attempts to extend paths before we give up
        const double CLOSE_ENOUGH_TO_GOAL = 0.001; //We're done when we get this close to the goal.

        const double ROBOT_MAX_TIME_EXTRAPOLATED = 0.7; //Extrapolate other robots' movements up to this amount of seconds.

        //Field boundaries
        static double FIELD_XMIN;
        static double FIELD_XMAX;
        static double FIELD_YMIN;
        static double FIELD_YMAX;

        //Steady state speed for path
        static double STEADY_STATE_SPEED;

        public void ReloadConstants()
        {
            LoadConstants();
        }

        public static void LoadConstants()
        {
            double FIELD_WIDTH = Constants.get<double>("plays", "FIELD_WIDTH");
            double FIELD_HEIGHT = Constants.get<double>("plays", "FIELD_HEIGHT");
            double GOAL_WIDTH = Constants.get<double>("plays", "GOAL_WIDTH");
            double REFEREE_ZONE_WIDTH = Constants.get<double>("plays", "REFEREE_ZONE_WIDTH");

            STEADY_STATE_SPEED = Constants.get<double>("motionplanning", "STEADY_STATE_SPEED");

            // Calculate physics based on physical boundaries slightly larger than field
            FIELD_XMIN = -FIELD_WIDTH / 2 - REFEREE_ZONE_WIDTH;
            FIELD_XMAX = FIELD_WIDTH / 2 + REFEREE_ZONE_WIDTH;
            FIELD_YMIN = -FIELD_HEIGHT / 2 - REFEREE_ZONE_WIDTH;
            FIELD_YMAX = FIELD_HEIGHT / 2 + REFEREE_ZONE_WIDTH;

        }

        //One node in the tree
        class RRTNode
        {
            public RobotInfo info;
            public RRTNode parent;
            public double time;

            public RRTNode(RobotInfo info, RRTNode parent, double dt)
            {
                this.info = info;
                this.parent = parent;
                this.time = (parent == null ? 0 : parent.time) + dt;
            }
        }


        public SmoothRRTPlanner()
        {

        }

        Vector2 GetRandomPoint(Vector2 desiredLoc, Vector2 currentLoc, double closestSoFar)
        {
            double factor = closestSoFar/4.0 + 0.18;
            double rand1 = RandGen.NextGaussian();
            double rand2 = RandGen.NextGaussian();

            double fullDist = desiredLoc.distance(currentLoc);
            if (fullDist <= 1e-6)
            {
                return new Vector2(desiredLoc.X + rand1 * factor, desiredLoc.Y + rand2 * factor);
            }

            double prop = closestSoFar/fullDist;
            prop *= 0.95;
            double centerX = desiredLoc.X * (1 - prop) + currentLoc.X * prop;
            double centerY = desiredLoc.Y * (1 - prop) + currentLoc.Y * prop;
            return new Vector2(centerX + rand1 * factor, centerY + rand2 * factor);
        }

        bool IsAllowedByAcceleration(RRTNode node, Vector2 nextVel)
        {
            return (nextVel - node.info.Velocity).magnitudeSq() <= MAX_ACCEL_PER_STEP * MAX_ACCEL_PER_STEP;
        }

        bool IntersectsObstacle(MovingShape robot, MovingShape obstacle)
        {
            return robot.velocity * (obstacle.center - robot.center) > 0 && GeomFuncs.intersects(robot.geom, obstacle.geom);
        }

        bool IntersectsObstacle(MovingShape robot, MovingShape obstacle, double time, double dt)
        {
            //We consider a step okay if it moves away from the obstacle, despite intersecting right now.
            if (robot.velocity * (obstacle.center - robot.center) <= 0)
                return false;

            //A simple bounding test to see if we could possibly ever intersect
            if (robot.boundingRadius + obstacle.boundingRadius + dt * (robot.velocity - obstacle.velocity).magnitude()
                < (robot.center - obstacle.center).magnitude())
                return false;

            //Test points along the step
            int NUM_TO_TEST = 8;
            for (int i = 0; i < NUM_TO_TEST; i++)
            {
                Geom r = robot.getMovedShape(dt * (double)i / NUM_TO_TEST);
                double obsTime = Math.Min(time + dt * (double)i / NUM_TO_TEST, ROBOT_MAX_TIME_EXTRAPOLATED);
                Geom o = obstacle.getMovedShape(obsTime);
                //Geom o = obstacle.geom;
                if (GeomFuncs.intersects(r, o))
                    return true;
                if (r is RobotShape && ((RobotShape)r).contains(obstacle.center + obsTime * obstacle.velocity))
                    return true;
            }
            return false;
        }

        bool IsAllowedByObstacles(MovingShape robot, List<MovingShape> obstacles, double time, double dt)
        {
            //Avoid all robots, except myself
            Console.WriteLine("Robot " + robot.center + " " + (robot.center + robot.velocity * dt));
            foreach (MovingShape obstacle in obstacles)
            {
                if (obstacle.geom is Circle) Console.WriteLine("Obstacle " + obstacle.center + " " + obstacle.boundingRadius);
                if (IntersectsObstacle(robot, obstacle, time, dt))
                    return false;
            }
            Console.WriteLine("True!");
            return true;
        }

        //Get the extended point, using the acceleration model
        Vector2 GetAcceleratedExtension(RRTNode node, Vector2 target)
        {
            Vector2 targetDir = target - node.info.Position;
            double magnitude = targetDir.magnitude();
            if(magnitude < 1e-6)
                return targetDir;

            //Is it maybe possible that we could reach goal?
            if (magnitude <= node.info.Velocity.magnitude() + MAX_ACCEL_PER_STEP * TIME_STEP)
            {
                //Check if we can get there right away anywhere in our acceleration circle or the cone reaching to it.
                Vector2 circleCenter = node.info.Position + node.info.Velocity * TIME_STEP;
                double circleRadius = MAX_ACCEL_PER_STEP * 2 * TIME_STEP;

                Vector2 toCircle = (circleCenter - node.info.Position);
                Vector2 closestOffsetTowardsCenter = toCircle.perpendicularComponent(targetDir);
                double closestDistToCenter = closestOffsetTowardsCenter.magnitude();
                if (closestDistToCenter < circleRadius)
                {
                    Vector2 closestApproachToCenter = circleCenter - closestOffsetTowardsCenter;
                    double halfChordLength = Math.Sqrt(circleRadius * circleRadius - closestDistToCenter * closestDistToCenter);
                    double distToClosestApproach = (toCircle * targetDir >= 0) ?
                        (closestApproachToCenter - node.info.Position).magnitude() :
                        -(closestApproachToCenter - node.info.Position).magnitude();
                    double exitDist = distToClosestApproach + halfChordLength;
                    if (magnitude < exitDist)
                    {
                        return targetDir;
                    }
                }
            }

            //Okay, just extend then
            targetDir /= magnitude;
            Vector2 newVel = node.info.Velocity + targetDir * MAX_ACCEL_PER_STEP;
            if(newVel.magnitude() > ROBOT_VELOCITY)
                newVel = newVel.normalizeToLength(ROBOT_VELOCITY);

            return newVel * TIME_STEP;
        }

        //Check if extending according to nextSegment (the position offset vector) would hit obstacles.
        RRTNode TryVsObstacles(RobotInfo currentState, RRTNode node, TwoDTreeMap<RRTNode> map, 
            Vector2 nextSegment, List<MovingShape> obstacles)
        {
            Vector2 nextVel = nextSegment.normalizeToLength(ROBOT_VELOCITY);

            MovingShape nextShape = new MovingShape(currentState);
            nextShape.velocity = (node.info.Position - currentState.Position) / TIME_STEP;
            if (!IsAllowedByObstacles(nextShape, obstacles, node.time, TIME_STEP))
                return null;

            RobotInfo newInfo = new RobotInfo(
                node.info.Position + nextSegment, 
                nextVel,
                0, currentState.Orientation, currentState.Team, currentState.ID);

            RRTNode newNode = new RRTNode(newInfo, node, nextSegment.magnitude() / ROBOT_VELOCITY);
            map.Add(newInfo.Position,newNode);
            return newNode;
        }

        //Extract the full successful path to node
        List<Vector2> GetPathFrom(RRTNode node)
        {
            List<Vector2> list = new List<Vector2>();
            while (node != null)
            {
                list.Add(node.info.Position);
                node = node.parent;
            }

            list.Reverse();
            return list;
        }

        //Get a path!
        public List<Vector2> GetPathTo(RobotInfo currentState, Vector2 desiredPosition, List<RobotInfo> robots, BallInfo ball, double avoidBallRadius)
        {
            List<MovingShape> obstacles = new List<MovingShape>();
            if (avoidBallRadius > 0 && avoidBallRadius >= MovingShape.RRT_ROBOT_RADIUS && ball != null)
            {
                double ballRad = avoidBallRadius - MovingShape.RRT_ROBOT_RADIUS;
                obstacles.Add(new MovingShape(new Circle(ball.Position, ballRad), ball.Position, 0, ball.Velocity, ballRad, 0));
            }
            foreach (RobotInfo info in robots)
            {
                if (info == null || (info.ID == currentState.ID && info.Team == currentState.Team))
                    continue;
                obstacles.Add(new MovingShape(info));
            }

            TwoDTreeMap<RRTNode> map = new TwoDTreeMap<RRTNode>(FIELD_XMIN, FIELD_XMAX, FIELD_YMIN, FIELD_YMAX);

            RRTNode startNode = new RRTNode(currentState, null, 0);
            map.Add(currentState.Position, startNode);

            RRTNode successNode = null;

            RRTNode activeNode = startNode;
            Vector2 currentTarget = desiredPosition;
            int stepsLeft = 1000;
            double closestSoFar = 1000;
            //double closeEnoughToGoal = CLOSE_ENOUGH_TO_GOAL; //TODO
            double closeEnoughToGoal = (desiredPosition - currentState.Position).magnitude() - 1.0;
            if (closeEnoughToGoal < CLOSE_ENOUGH_TO_GOAL)
                closeEnoughToGoal = CLOSE_ENOUGH_TO_GOAL;

            bool doRandomAgain = false;
            bool tryAgain = false;

            int tries = 0;
            //Repeat while tree not too large
            while (map.Size() < MAX_TREE_SIZE)
            {
                //Do we stop the current pursuit yet?
                if (tryAgain || (--stepsLeft <= 0))
                {
                    tries++;
                    if (tries >= MAX_PATH_TRIES)
                        break;

                    //Go to the goal directly
                    if(!doRandomAgain && currentTarget != desiredPosition)
                    {
                        doRandomAgain = true;
                        currentTarget = desiredPosition;
                        stepsLeft = 1000;
                        tryAgain = false;
                    }
                    //Go randomly
                    else
                    {
                        doRandomAgain = true;
                        currentTarget = GetRandomPoint(desiredPosition, currentState.Position, closestSoFar);
                        activeNode = map.NearestNeighbor(currentTarget).Second;
                        stepsLeft = 1;
                        tryAgain = false;
                    }

                }

                //If we're close enough to the goal, we're done!
                if (activeNode.info.Position.distanceSq(desiredPosition) < closeEnoughToGoal * closeEnoughToGoal)
                {
                    successNode = activeNode;
                    break;
                }

                //Try to generate an extension to our target
                Vector2 segment = GetAcceleratedExtension(activeNode, currentTarget);
                if (segment == null)
                {
                    tryAgain = true;
                    continue;
                }

                //Make sure the extension doesn't hit obstacles
                RRTNode newNode = TryVsObstacles(currentState, activeNode, map, segment, obstacles);
                if (newNode == null)
                {
                    tryAgain = true;
                }
                else
                {
                    //Make sure that we haven't already crossed past our target
                    if ((newNode.info.Position - activeNode.info.Position) * (currentTarget - newNode.info.Position) <= 0)
                        tryAgain = true;

                    //Update closestSoFar
                    double dist = newNode.info.Position.distance(desiredPosition);
                    if (dist < closestSoFar)
                        closestSoFar = dist;

                    //Continue the RRT! Back up to the top now!
                    activeNode = newNode;
                    doRandomAgain = false;
                }
                
            }

            //If we didn't succeed, take the closest node anyways.
            if (!(map.Size() < MAX_TREE_SIZE && tries < MAX_PATH_TRIES))
            {
                successNode = map.NearestNeighbor(desiredPosition).Second;
            }

            List<Vector2> path = GetPathFrom(successNode);
            return path;

        }

        //Do some path smoothing
        public List<Vector2> GetSmoothedPointPath(RobotInfo currentState, RobotInfo desiredState, List<RobotInfo> robots, 
            BallInfo ball, double avoidBallRadius)
        {
            List<Vector2> path = GetPathTo(currentState, desiredState.Position, robots, ball, avoidBallRadius);
            /*
            smoothPath1(path, currentState, robots, ball, avoidBallRadius);
            smoothPath2(path, currentState, robots, ball, avoidBallRadius);
            smoothPath1(path, currentState, robots, ball, avoidBallRadius);
            smoothPath2(path, currentState, robots, ball, avoidBallRadius);
            smoothPath1(path, currentState, robots, ball, avoidBallRadius);
            smoothPath2(path, currentState, robots, ball, avoidBallRadius);
            smoothPath2(path, currentState, robots, ball, avoidBallRadius);
            smoothPath2(path, currentState, robots, ball, avoidBallRadius);
            smoothPath2(path, currentState, robots, ball, avoidBallRadius);*/
            return path;
            
        }

        //Try a bunch of paths and take the best one
        public List<Vector2> GetBestPointPath(Team team, int id, IPredictor predictor, RobotInfo desiredState, double avoidBallRadius)
        {
            RobotInfo currentState;
            try
            { currentState = predictor.GetRobot(team, id); }
            catch (ApplicationException e)
            { return new List<Vector2>(); }

            BallInfo ball = predictor.GetBall();
            List<RobotInfo> robots = predictor.GetRobots();

            double ballDistanceFromDesired = ball.Position.distance(desiredState.Position);
            if (ballDistanceFromDesired <= avoidBallRadius)
            {
                // Console.WriteLine("Warning: told to move to a point closer to the ball than " + avoidBallRadius +
                //     " at the same time as staying away from the ball!");
                avoidBallRadius = ballDistanceFromDesired - 0.005;
            }

            List<Vector2> bestPath = null;
            double bestPathScore = Double.NegativeInfinity;

            for (int i = 0; i < 7; i++)
            {
                List<Vector2> path = GetSmoothedPointPath(currentState, desiredState, robots, ball, avoidBallRadius);
                double score = 0;

                /*
                //Up to 60 points for geting to the goal
                score += 80.0 / (20*20*desiredState.Position.distanceSq(path[path.Count - 1]) + 1);

                //Lose 60 points per meter of path length greater than the start and end points
                double len = 0;
                for (int j = 0; j < path.Count-1; j++)
                    len += path[j+1].distance(path[j]);
                len -= path[path.Count - 1].distance(path[0]);
                score -= len * 60;
                 */

                //Lose 60 points per meter of path length greater than the start and end points
                double len = 0;
                for (int j = 0; j < path.Count - 1; j++)
                    len += path[j + 1].distance(path[j]);
                len += path[path.Count - 1].distance(desiredState.Position);
                len -= desiredState.Position.distance(path[0]);
                score -= len * 60;

                //Lose up to a point per node in the path if it's too sharp a bend.
                for (int j = 1; j < path.Count-1; j++)
                {
                    Vector2 vec1 = path[j] - path[j - 1];
                    Vector2 vec2 = path[j + 1] - path[j];
                    if (vec1.magnitudeSq() < 1e-6 || vec2.magnitudeSq() < 1e-6)
                    { score += 1; continue; }
                    score += (vec1 * vec2) / vec1.magnitude() / vec2.magnitude();
                }

                //Win/lose up to 30 points times current speed if the first segment of the path agrees 
                //with our current velocity
                int firstStep = 1;
                Vector2 firstVec = null;
                while(firstStep < path.Count && (firstVec = path[firstStep]-path[firstStep-1]).magnitudeSq() < 1e-12)
                {firstStep++;}
                if(firstStep >= path.Count)
                    score += 30;
                else
                {
                    Vector2 firstDir = firstVec.normalize();
                    score += firstDir * currentState.Velocity * 30;
                }
                
                if (score > bestPathScore)
                {
                    bestPath = path;
                    bestPathScore = score;
                }
            }
            return bestPath;
        }


        public RobotPath GetPath(Team team, int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius)
        {
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

            List<Vector2> bestPath = GetBestPointPath(team, id, predictor, new RobotInfo(desiredState), avoidBallRadius);
                        
            //Convert the path
            List<RobotInfo> robotPath = new List<RobotInfo>();

            //Overly simplistic conversion from planning on position vectors to RobotInfo that has orientation and velocity information
            //velocity at every waypoint just points to next one with constant speed
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
    }

}
