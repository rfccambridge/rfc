using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Core;
using Robocup.CoreRobotics;
using Robocup.Geometry;


namespace Robocup.MotionControl
{
    public class SmoothRRTPlanner : IPathPlanner
    {
        const double TIME_STEP = 0.12;           //Time step in secs, with velocity determines RRT extension length
        const double ROBOT_VELOCITY = 1.0;       //Assume our robot can move this fast
        const double MAX_ACCEL_PER_STEP = 0.42;  //And that it can accelerate this fast.

        const double MAX_TREE_SIZE = 250;       //Max nodes in tree before we give up
        const double MAX_PATH_TRIES = 80;       //Max number of attempts to extend paths before we give up
        const double CLOSE_ENOUGH_TO_GOAL = 0.001; //We're completely done when we get this close to the goal.
        const double DIST_FOR_SUCCESS = 1.3; //We're done for now when we get this much closer to the goal than we are

        const double ROBOT_MAX_TIME_EXTRAPOLATED = 0.7; //Extrapolate other robots' movements up to this amount of seconds.

        const double RRT_ROBOT_AVOID_DIST = 0.185;  //Avoid robot distance

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

        bool IntersectsObstacle(Vector2 p, Vector2 obsPos, double obsRadius, Vector2 rayUnit)
        {
            return rayUnit * (obsPos - p) >= 0 && obsPos.distanceSq(p) < obsRadius * obsRadius;
        }

        bool IntersectsObstacle(Vector2 src, Vector2 dest, Vector2 rayUnit, double rayLen, Vector2 obsPos, double obsRadius)
        {
            //We consider a step okay if it moves away from the obstacle, despite intersecting right now.
            if (rayUnit * (obsPos - src) <= 0)
                return false;

            if (obsPos.distanceSq(src) < obsRadius * obsRadius)
                return true;
            if (obsPos.distanceSq(dest) < obsRadius * obsRadius)
                return true;

            //See if it intersects in the middle...
            Vector2 srcToObs = obsPos - src;
            double parallelDist = srcToObs * rayUnit;
            if (parallelDist <= 0 || parallelDist >= rayLen)
                return false;

            double perpDist = Math.Abs(Vector2.cross(srcToObs, rayUnit));
            if (perpDist < obsRadius)
                return true;

            return false;
        }

        bool IsAllowedByObstacles(RobotInfo currentState, Vector2 src, Vector2 nextSegment, double curTime, BallInfo ball, List<RobotInfo> robots, double avoidBallRadius)
        {
            Vector2 dest = src + nextSegment;
            Vector2 ray = dest - src;
            if (ray.magnitudeSq() < 1e-16)
                return true;

            Vector2 rayUnit = ray.normalizeToLength(1.0);
            double rayLen = ray.magnitude();

            //Avoid all robots, except myself
            foreach (RobotInfo info in robots)
            {
                if (info.Team != currentState.Team || info.ID != currentState.ID)
                {
                    //Extrapolate the robot's position into the future.
                    Vector2 obsPos = info.Position;
                    double time = Math.Max(curTime, ROBOT_MAX_TIME_EXTRAPOLATED);
                    obsPos += info.Velocity * time;

                    if (IntersectsObstacle(src, dest, rayUnit, rayLen, obsPos, RRT_ROBOT_AVOID_DIST))
                    { return false; }

                    //It's also bad if the obstacle would collide with us next turn, by virtue of moving...
                    //But it's still okay if we're moving away from it.
                    Vector2 obsPosNext = obsPos += info.Velocity * TIME_STEP;
                    if (IntersectsObstacle(dest, obsPosNext, RRT_ROBOT_AVOID_DIST, rayUnit))
                    { return false; }
                }
            }

            //If needed, avoid ball
            if (avoidBallRadius > 0 &&
                ball != null &&
                ball.Position != null)
            {
                if (IntersectsObstacle(src, dest, rayUnit, rayLen, ball.Position, avoidBallRadius))
                { return false; }
            }
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
        RRTNode TryVsObstacles(RobotInfo currentState, RRTNode node, TwoDTreeMap<RRTNode> map, Vector2 nextSegment, 
            BallInfo ball, List<RobotInfo> robots, double avoidBallRadius)
        {
            Vector2 nextVel = nextSegment.normalizeToLength(ROBOT_VELOCITY);

            if (!IsAllowedByObstacles(currentState, node.info.Position, nextSegment, node.time, ball, robots, avoidBallRadius))
                return null;

            RobotInfo newInfo = new RobotInfo(
                node.info.Position + nextSegment, 
                nextVel,
                0, 0, currentState.Team, currentState.ID);

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
            TwoDTreeMap<RRTNode> map = new TwoDTreeMap<RRTNode>(FIELD_XMIN, FIELD_XMAX, FIELD_YMIN, FIELD_YMAX);

            RRTNode startNode = new RRTNode(currentState, null, 0);
            map.Add(currentState.Position, startNode);

            RRTNode successNode = null;

            RRTNode activeNode = startNode;
            Vector2 currentTarget = desiredPosition;
            int stepsLeft = 1000;
            double closestSoFar = 1000;
            //double closeEnoughToGoal = CLOSE_ENOUGH_TO_GOAL; //TODO
            double closeEnoughToGoal = (desiredPosition - currentState.Position).magnitude() - DIST_FOR_SUCCESS;
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
                RRTNode newNode = TryVsObstacles(currentState, activeNode, map, segment, ball, robots, avoidBallRadius);
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
        public List<Vector2> GetBestPointPath(Team team, int id, IPredictor predictor, RobotInfo desiredState, 
            double avoidBallRadius, RobotPath oldPath)
        {
            RobotInfo currentState;
            try
            { currentState = predictor.GetRobot(team, id); }
            catch (ApplicationException e)
            { return new List<Vector2>(); }

            BallInfo ball = new BallInfo(predictor.GetBall());
            List<RobotInfo> robots = predictor.GetRobots();

            if (ball != null)
            {
                // Console.WriteLine("Warning: told to move to a point closer to the ball than " + avoidBallRadius +
                //     " at the same time as staying away from the ball!");

                //Recenter the ball avoid position, and lower the radius
                Vector2 ballToDesired = desiredState.Position - ball.Position;
                if(ballToDesired.magnitude() < 1e-6)
                    avoidBallRadius = 0;
                else
                {
                    Vector2 ballToAlmostDesired = ballToDesired * 0.95;
                    Vector2 ballAwayFromDesired = (-ballToDesired).normalizeToLength(avoidBallRadius);

                    Vector2 radPos1 = ballToAlmostDesired + ball.Position;
                    Vector2 radPos2 = ballAwayFromDesired + ball.Position;
                    Vector2 average = (radPos1 + radPos2)/2.0;
                    avoidBallRadius = average.distance(radPos1);
                    ball.Position = average;
                }
            }

            List<Vector2> bestPath = null;
            double bestPathScore = Double.NegativeInfinity;

            for (int i = 0; i < 12; i++)
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
                    double dScore = firstDir * currentState.Velocity * 30;
                    score += dScore;
                }

                //Win/lose up to 20 points times current speed if the path agrees with our old path
                if (oldPath != null && path.Count > 1 && oldPath.Waypoints.Count > 1)
                {
                    const double DISTCAP = 0.5;
                    double distSum = 0;
                    for (int j = 1; j < path.Count; j++)
                    {
                        Vector2 pathLoc = path[j];

                        double closestDist = DISTCAP;
                        for (int k = 0; k < oldPath.Waypoints.Count-1; k++)
                        {
                            Line seg = new Line(oldPath.Waypoints[k + 1].Position, oldPath.Waypoints[k].Position);
                            double dist = seg.distFromSegment(pathLoc);
                            if (dist < closestDist)
                                closestDist = dist;
                        }

                        distSum += closestDist;
                    }

                    double avgDist = (distSum) / (path.Count - 1);
                    double dScore = (DISTCAP - avgDist) * 20 * currentState.Velocity.magnitude();
                    score += dScore;
                }
                
                if (score > bestPathScore)
                {
                    bestPath = path;
                    bestPathScore = score;
                }
            }
            return bestPath;
        }

        private bool isObstacleFree(List<Vector2> path, int startIdx, RobotInfo currentState, List<RobotInfo> robots, BallInfo ball, double avoidBallRadius)
        {
            double distSoFar = 0;
            for (int j = 0; j < startIdx; j++)
            {
                distSoFar += (path[j + 1] - path[j]).magnitude();
            }

            for (int j = startIdx; j < path.Count-1; j++)
            {
                if (!IsAllowedByObstacles(currentState, path[j], path[j + 1] - path[j],
                    distSoFar / ROBOT_VELOCITY, ball, robots, avoidBallRadius))
                { return false; }
                distSoFar += (path[j+1] - path[j]).magnitude();
            }
            return true;
        }


        private void smoothPath1(List<Vector2> path, RobotInfo currentState, List<RobotInfo> robots, BallInfo ball, double avoidBallRadius)
        {
            if(path.Count <= 2)
                return;

            List<Vector2> smoothedPath = new List<Vector2>(path.Count);
            for (int i = 0; i < 6; i++)
            {
                int x = RandGen.Next(path.Count);
                int y = RandGen.Next(path.Count);
                if (x == y) continue;
                if (x > y)
                { int temp = x; x = y; y = temp; }

                smoothedPath.Clear();
                smoothedPath.Add(path[0]);

                double distSoFar = 0;
                for (int j = 1; j <= x; j++)
                {
                    smoothedPath.Add(path[j]);
                    distSoFar += (path[j]-path[j-1]).magnitude();
                }

                bool stopThisIter = false;
                Vector2 offset = (path[y] - path[x])/(y-x);
                for (int j = x + 1; j <= y; j++)
                {
                    if (!IsAllowedByObstacles(currentState, smoothedPath[j - 1], offset,
                        distSoFar / ROBOT_VELOCITY, ball, robots, avoidBallRadius))
                    { stopThisIter = true; break; }
                    smoothedPath.Add(smoothedPath[j - 1] + offset);
                    distSoFar += offset.magnitude();
                }
                if(stopThisIter)
                    continue;

                for (int j = y + 1; j < path.Count; j++)
                {
                    if (!IsAllowedByObstacles(currentState, smoothedPath[j - 1], path[j] - path[j - 1],
                           distSoFar / ROBOT_VELOCITY, ball, robots, avoidBallRadius))
                    { stopThisIter = true; break; }
                    smoothedPath.Add(path[j]);
                    distSoFar += (path[j] - path[j - 1]).magnitude();
                }
                if (stopThisIter)
                    continue;

                for (int j = 0; j < path.Count; j++)
                    path[j] = smoothedPath[j];
                
            }
        }

        private void smoothPath2(List<Vector2> path, RobotInfo currentState, List<RobotInfo> robots, BallInfo ball, double avoidBallRadius)
        {
            //Compute sharpness of each bend
            int len = path.Count;
            if (len <= 2)
                return;
            double[] sharpnessArr = new double[len - 2];
            int[] indexArr = new int[len - 2];

            for (int i = 1; i < len - 1; i++)
            {
                Vector2 vec1 = path[i] - path[i - 1];
                Vector2 vec2 = path[i + 1] - path[i];
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
                Vector2 prev = path[idx - 1];
                Vector2 next = path[idx + 1];
                Vector2 mid = (prev + next) / 2.0;

                //Try just using the mid outright
                path[idx] = mid;
                if (isObstacleFree(path, idx - 1, currentState, robots, ball, avoidBallRadius))
                {
                    if ((next - prev).magnitudeSq() <= 0.15 * 0.15)
                    {
                        path.RemoveAt(idx);
                        for (int j = i + 1; j < len - 2; j++)
                            if (indexArr[j] > idx) indexArr[j]--;
                    }
                    continue;
                }
                else 
                    path[idx] = cur;

                //Try smoothing partway to the mid
                Vector2 curmid2 = (cur + mid) / 2.0;
                path[idx] = curmid2;
                if (isObstacleFree(path, idx - 1, currentState, robots, ball, avoidBallRadius))
                    continue;
                else
                    path[idx] = cur;

                //Try smoothing partway to the mid
                Vector2 curmid4 = (cur * 3 + mid) / 4.0;
                path[idx] = curmid4;
                if (isObstacleFree(path, idx - 1, currentState, robots, ball, avoidBallRadius))
                    continue;
                else
                    path[idx] = cur;

                //No? Okay, we can't smooth this one any more
            }
        }


        /*
        public List<RobotInfo> GetPathInDirection(Team team, int id, IPredictor predictor, Vector2 direction, double avoidBallRadius)
        {

        }

        public List<RobotInfo> GetPathInterceptBall(Team team, int id, IPredictor predictor)
        {

        }*/


        public RobotPath GetPath(Team team, int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius,
            RobotPath oldPath)
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

            List<Vector2> bestPath = GetBestPointPath(team, id, predictor, new RobotInfo(desiredState), avoidBallRadius, oldPath);
                        
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
