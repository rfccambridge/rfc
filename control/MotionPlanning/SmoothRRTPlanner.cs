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
        //Movement model
        const double TIME_STEP = 0.12; //Time step in secs, with velocity determines RRT extension length
        const double ROBOT_VELOCITY = 1.0; //Assume our robot can move this fast
        const double MAX_ACCEL_PER_STEP = 0.48; //And that it can accelerate this fast.
        const double MAX_OBSERVABLE_VELOCITY = 0.4; //Pretend that the current robot is moving at most this fast

        //RRT parameters
        const double MAX_TREE_SIZE = 200; //Max nodes in tree before we give up
        const double MAX_PATH_TRIES = 70; //Max number of attempts to extend paths before we give up
        const double CLOSE_ENOUGH_TO_GOAL = 0.001; //We're completely done when we get this close to the goal.
        const double DIST_FOR_SUCCESS = 1.2; //We're done for now when we get this much closer to the goal than we are

        const double DODGE_OBS_DIST = 1.6;

        //Path extension
        const double EXTRA_EXTENSION_ROTATE_ANGLE = 2.0 * Math.PI / 180.0;

        //Motion extrapolation
        const double ROBOT_MAX_TIME_EXTRAPOLATED = 0.7; //Extrapolate other robots' movements up to this amount of seconds.

        //Collisions
        const double RRT_ROBOT_AVOID_DIST = 0.185;  //Avoid robot distance
        const double RRT_ROBOT_FAR_AVOID_DIST = 0.21; //Avoid robot distance when not close to goal
        const double RRT_ROBOT_FAR_DIST = 0.35; //What is considered "far", for the purposes of robot avoidance? 

        //Scoring
        const double DIST_FROM_GOAL_SCORE = 0; //Penalty per m of distance from goal.
        const double EXCESS_LEN_SCORE = 60; //Penalty for each m of path length >= the straightline distance
        const double PER_BEND_SCORE = 1; //Bonus/Penalty per bend in the path based on bend sharpness
        const double VELOCITY_AGREEMENT_SCORE = 30; //Bonus for agreeing with current velocity, per m/s velocity
        const double OLDPATH_AGREEMENT_SCORE = 200; //Bonus for agreeing with the old path, per m/s velocity
        const double OLDPATH_AGREEMENT_DIST = 0.5; //Score nothing for points that differ by this many meters from the old path

        const double NUM_PATHS_TO_SCORE = 9; //How many paths do we generate and score?

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


        private bool includeCurStateInPath;

        public SmoothRRTPlanner(bool includeCurStateInPath)
        {
            this.includeCurStateInPath = includeCurStateInPath;
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

        double GetEffectiveAvoidDist(double goalDist)
        {
            double avoidProp = (goalDist - RRT_ROBOT_AVOID_DIST) / (RRT_ROBOT_FAR_DIST - RRT_ROBOT_AVOID_DIST);
            if (avoidProp < 0) avoidProp = 0;
            if (avoidProp > 1) avoidProp = 1;
            double avoidDist = RRT_ROBOT_AVOID_DIST + avoidProp * (RRT_ROBOT_FAR_AVOID_DIST - RRT_ROBOT_AVOID_DIST);
            return avoidDist;
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

        Vector2 GetFuturePos(Vector2 obsPos, Vector2 obsVel, double time)
        {
            time = Math.Max(time, ROBOT_MAX_TIME_EXTRAPOLATED);
            return obsPos + obsVel * time;
        }

        bool IsAllowedByObstacles(RobotInfo currentState, Vector2 src, Vector2 nextSegment, double curTime, BallInfo ball, List<RobotInfo> robots, double avoidBallRadius, Vector2 goal)
        {
            Vector2 dest = src + nextSegment;
            Vector2 ray = nextSegment;
            if (ray.magnitudeSq() < 1e-16)
                return true;

            double goalDist = dest.distance(goal);
            double avoidDist = GetEffectiveAvoidDist(goalDist);

            Vector2 rayUnit = ray.normalizeToLength(1.0);
            double rayLen = ray.magnitude();

            //Time to extrapolate forth
            double time = Math.Max(curTime, ROBOT_MAX_TIME_EXTRAPOLATED);

            //Avoid all robots, except myself
            int count = robots.Count;
            for(int i = 0; i<count; i++)
            {
                RobotInfo info = robots[i];
                if (info.Team != currentState.Team || info.ID != currentState.ID)
                {
                    //Extrapolate the robot's position into the future.
                    Vector2 obsPos = GetFuturePos(info.Position,info.Velocity,curTime);

                    if (IntersectsObstacle(src, dest, rayUnit, rayLen, obsPos, avoidDist))
                    { return false; }

                    //It's also bad if the obstacle would collide with us next turn, by virtue of moving...
                    //But it's still okay if we're moving away from it.
                    Vector2 obsPosNext = obsPos + info.Velocity * TIME_STEP;
                    if (IntersectsObstacle(dest, obsPosNext, avoidDist, rayUnit))
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

        Vector2 GetAdjustedTargetDir(Vector2 cur, Vector2 targetDir, Vector2 obsPos, Vector2 obsVel, double time, double avoidDist, double targetDist)
        {
            double obsDist = obsPos.distance(cur);
            time += (obsDist / ROBOT_VELOCITY);
            obsPos = GetFuturePos(obsPos, obsVel, time);
            Vector2 relObsPos = obsPos - cur;

            double parallelDist = targetDir * relObsPos;
            if (parallelDist <= 0 || parallelDist >= targetDist)
                return null;

            double perpDist = Vector2.cross(relObsPos, targetDir);
            if (perpDist >= 0 && perpDist < avoidDist)
            {
                Vector2 newTargetDir = relObsPos.parallelComponent(targetDir) + targetDir.rotatePerpendicular() * (avoidDist - perpDist) * 1.2;
                return newTargetDir.normalize();
            }
            else if (perpDist < 0 && perpDist > -avoidDist)
            {
                Vector2 newTargetDir = relObsPos.parallelComponent(targetDir) - targetDir.rotatePerpendicular() * (avoidDist + perpDist) * 1.2;
                return newTargetDir.normalize();
            }
            return null;
        }

        //Get the extended point, using the acceleration model
        Vector2 GetAcceleratedExtension(RRTNode node, Vector2 target, BallInfo ball, List<RobotInfo> robots, Vector2 goal, double ballAvoidRadius, bool adjust)
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
            //Get the vector from us to the target
            targetDir /= magnitude;

            if (adjust)
            {
                double avoidDist = GetEffectiveAvoidDist(node.info.Position.distance(goal));

                //Extend taking obstacles into account, move the target if there's an obstacle in the way
                double closestObsDistSq = 100000000;
                Vector2 adjustedTargetDir = targetDir;
                for (int i = 0; i < robots.Count; i++)
                {
                    RobotInfo info = robots[i];
                    if (info == null || info.Position == null || info.Velocity == null)
                        continue;
                    if (info.Team == node.info.Team && info.ID == node.info.ID)
                        continue;

                    double distSq = node.info.Position.distanceSq(info.Position);
                    if (DODGE_OBS_DIST <= 0 || distSq > DODGE_OBS_DIST * DODGE_OBS_DIST || distSq > closestObsDistSq)
                        continue;

                    Vector2 adj = GetAdjustedTargetDir(node.info.Position, targetDir, info.Position, info.Velocity, node.time, avoidDist, magnitude);
                    if (adj == null)
                        continue;
                    adjustedTargetDir = adj;
                    closestObsDistSq = distSq;
                }

                while (ball != null && ballAvoidRadius > 0)
                {
                    double distSq = node.info.Position.distanceSq(ball.Position);
                    if (distSq > DIST_FOR_SUCCESS * DIST_FOR_SUCCESS || distSq > closestObsDistSq)
                        break;

                    Vector2 adj = GetAdjustedTargetDir(node.info.Position, targetDir, ball.Position, ball.Velocity, node.time, ballAvoidRadius, magnitude);
                    if (adj == null)
                        break;
                    adjustedTargetDir = adj;
                    closestObsDistSq = distSq;
                    break;
                }

                targetDir = adjustedTargetDir;
            }
            Vector2 newVel = node.info.Velocity + targetDir * MAX_ACCEL_PER_STEP;
            double newVelMag = newVel.magnitude();
            if(newVelMag > ROBOT_VELOCITY)
            {
                newVel = newVel.normalizeToLength(ROBOT_VELOCITY);
                newVelMag = ROBOT_VELOCITY;
            }

            //Small hack - rotate the newVel towards the targetDir a little bit
            if(newVelMag > 1e-6)
            {
                double angleDiff = UsefulFunctions.angleDifference(newVel.cartesianAngle(),targetDir.cartesianAngle());
                if (angleDiff <= EXTRA_EXTENSION_ROTATE_ANGLE && angleDiff >= -EXTRA_EXTENSION_ROTATE_ANGLE)
                    newVel = newVel.rotate(angleDiff);
                else if (angleDiff < -EXTRA_EXTENSION_ROTATE_ANGLE)
                    newVel = newVel.rotate(-EXTRA_EXTENSION_ROTATE_ANGLE);
                else
                    newVel = newVel.rotate(EXTRA_EXTENSION_ROTATE_ANGLE);
            }

            return newVel * TIME_STEP;
        }

        //Check if extending according to nextSegment (the position offset vector) would hit obstacles.
        RRTNode TryVsObstacles(RobotInfo currentState, RRTNode node, TwoDTreeMap<RRTNode> map, Vector2 nextSegment, 
            BallInfo ball, List<RobotInfo> robots, double avoidBallRadius, Vector2 goal)
        {
            Vector2 nextVel = nextSegment.normalizeToLength(ROBOT_VELOCITY);

            if (!IsAllowedByObstacles(currentState, node.info.Position, nextSegment, node.time, ball, robots, avoidBallRadius, goal))
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
            double mapXMin = Math.Min(currentState.Position.X, desiredPosition.X) - 0.3;
            double mapYMin = Math.Min(currentState.Position.Y, desiredPosition.Y) - 0.3;
            double mapXMax = Math.Max(currentState.Position.X, desiredPosition.X) + 0.3;
            double mapYMax = Math.Max(currentState.Position.Y, desiredPosition.Y) + 0.3;

            TwoDTreeMap<RRTNode> map = new TwoDTreeMap<RRTNode>(mapXMin, mapXMax, mapYMin, mapYMax);

            RRTNode startNode = new RRTNode(currentState, null, 0);
            map.Add(currentState.Position, startNode);

            RRTNode successNode = null;

            RRTNode activeNode = startNode;
            Vector2 currentTarget = desiredPosition;
            int stepsLeft = 1000;
            double closestSoFar = 1000;
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
                Vector2 segment = GetAcceleratedExtension(activeNode, currentTarget, ball, robots, desiredPosition, avoidBallRadius, currentTarget == desiredPosition);
                if (segment == null)
                {
                    tryAgain = true;
                    continue;
                }

                //Make sure the extension doesn't hit obstacles
                RRTNode newNode = TryVsObstacles(currentState, activeNode, map, segment, ball, robots, avoidBallRadius, desiredPosition);
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

        //Try a bunch of paths and take the best one
        public List<Vector2> GetBestPointPath(Team team, int id, IPredictor predictor, RobotInfo desiredState, 
            double avoidBallRadius, RobotPath oldPath)
        {
            RobotInfo currentState;
            try
            { currentState = predictor.GetRobot(team, id); }
            catch (ApplicationException e)
            { return new List<Vector2>(); }
            currentState = new RobotInfo(currentState);
            if (currentState.Velocity.magnitude() > MAX_OBSERVABLE_VELOCITY)
                currentState.Velocity = currentState.Velocity.normalizeToLength(MAX_OBSERVABLE_VELOCITY);

            BallInfo ball = predictor.GetBall();
            List<RobotInfo> robots = predictor.GetRobots();

            if (ball != null)
            {
                ball = new BallInfo(ball);

                //Recenter the ball avoid position, and lower the radius
                Vector2 ballToDesired = desiredState.Position - ball.Position;
                if(ballToDesired.magnitude() < 1e-6)
                    avoidBallRadius = 0;
                else if (ballToDesired.magnitude() < avoidBallRadius)
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

            for (int i = 0; i < NUM_PATHS_TO_SCORE; i++)
            {
                List<Vector2> path = GetPathTo(currentState, desiredState.Position, robots, ball, avoidBallRadius);
                double score = 0;

                //Penalty based on distance from the goal, per meter
                if(path.Count >= 1)
                    score -= DIST_FROM_GOAL_SCORE * desiredState.Position.distance(path[path.Count - 1]);

                //Lose points per meter of path length greater than the start and end points
                double len = 0;
                for (int j = 0; j < path.Count - 1; j++)
                    len += path[j + 1].distance(path[j]);
                len += path[path.Count - 1].distance(desiredState.Position);
                len -= desiredState.Position.distance(path[0]);
                score -= len * EXCESS_LEN_SCORE;

                //Lose per node in the path if it's too sharp a bend.
                for (int j = 1; j < path.Count-1; j++)
                {
                    Vector2 vec1 = path[j] - path[j - 1];
                    Vector2 vec2 = path[j + 1] - path[j];
                    if (vec1.magnitudeSq() < 1e-6 || vec2.magnitudeSq() < 1e-6)
                    { score += 1; continue; }
                    score += PER_BEND_SCORE * (vec1 * vec2) / vec1.magnitude() / vec2.magnitude();
                }

                //Win/lose points if the first segment of the path agrees 
                //with our current velocity, multiplied by the current speed
                int firstStep = 1;
                Vector2 firstVec = null;
                while(firstStep < path.Count && (firstVec = path[firstStep]-path[firstStep-1]).magnitudeSq() < 1e-12)
                {firstStep++;}
                if(firstStep >= path.Count)
                    score += VELOCITY_AGREEMENT_SCORE;
                else
                {
                    Vector2 firstDir = firstVec.normalize();
                    double dScore = firstDir * currentState.Velocity * VELOCITY_AGREEMENT_SCORE;
                    score += dScore;
                }

                //Win/lose points if the path agrees with our old path, multiplied by the current speed
                if (oldPath != null && path.Count > 1 && oldPath.Waypoints.Count > 1)
                {
                    double distSum = 0;
                    for (int j = 1; j < path.Count; j++)
                    {
                        Vector2 pathLoc = path[j];

                        double closestDist = OLDPATH_AGREEMENT_DIST;
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
                    double dScore = (OLDPATH_AGREEMENT_DIST - avgDist) * OLDPATH_AGREEMENT_SCORE * 
                        currentState.Velocity.magnitude();
                    score += dScore;
                }
                
                //Is it the best path so far?
                if (score > bestPathScore)
                {
                    bestPath = path;
                    bestPathScore = score;
                }
            }
            return bestPath;
        }


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
            int pathStart = includeCurStateInPath ? 0 : 1;
            for (int i = pathStart; i < bestPath.Count; i++)
            {
                RobotInfo waypoint = new RobotInfo(bestPath[i], desiredState.Orientation, team, id);
                if (i < bestPath.Count - 1)
                    waypoint.Velocity = (bestPath[i + 1] - bestPath[i]).normalizeToLength(STEADY_STATE_SPEED);
                else
                    waypoint.Velocity = new Vector2(0, 0); //Stop at destination

                robotPath.Add(waypoint);
            }

            if (robotPath.Count <= 0)
                return new RobotPath(team, id);

            return new RobotPath(robotPath);
        }
    }

}
