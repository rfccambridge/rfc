using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Geometry;
using Robocup.Core;

namespace Navigation
{
    namespace Examples
    {
        public class BugNavigator : INavigator
        {
            const int TEAMSIZE = 25;
            bool[] avoidingObstacle = new bool[TEAMSIZE];
            double[] lastDirection = new double[TEAMSIZE];
            double[] continueDistanceSq = new double[TEAMSIZE];
            double[] traceDirection = new double[TEAMSIZE];
            int[] roundsSinceTrace = new int[TEAMSIZE];
            Vector2[] lastDestination = new Vector2[TEAMSIZE];
            const double angleSweep = .1;
            const double avoidRobotDist = .22;
            const double extraAvoidBallDist = .1;
            const double lookAheadDist = .1;
            const double getCloserAmount = .1;

            public BugNavigator()
            {
                for (int i = 0; i < TEAMSIZE; i++)
                {
                    traceDirection[i] = 1;
                }
            }


            Obstacle blockingObstacle(Vector2 position, List<Obstacle> obstacles, double gettingTiredFactor)
            {
                foreach (Obstacle o in obstacles)
                {
                    if (position.distanceSq(o.position) <= o.size * o.size / gettingTiredFactor)
                        return o;
                }
                return null;
            }

            Vector2 extend(Vector2 position, double direction)
            {
                return new Vector2(position.X + lookAheadDist * Math.Cos(direction), position.Y + lookAheadDist * Math.Sin(direction));
            }

            readonly Vector2 goal1 = new Vector2(-2.45, 0);
            readonly Vector2 goal2 = new Vector2(2.45, 0);
            const double goalieBoxAvoid = .65;
            public NavigationResults navigate(int id, Vector2 position, Vector2 destination, RobotInfo[] teamPositions, RobotInfo[] enemyPositions, BallInfo ballPosition, double avoidBallDist)
            {
                List<Obstacle> obstacles = new List<Obstacle>();
                for (int i = 0; i < teamPositions.Length; i++)
                {
                    if (id != teamPositions[i].ID)
                        obstacles.Add(new Obstacle(teamPositions[i].Position, avoidRobotDist));
                }
                for (int i = 0; i < enemyPositions.Length; i++)
                {
                    obstacles.Add(new Obstacle(enemyPositions[i].Position, avoidRobotDist));
                }
                if (avoidBallDist > 0)
                    obstacles.Add(new Obstacle(ballPosition.Position, avoidBallDist + extraAvoidBallDist));
                {
                    //goal1:
                    foreach (RobotInfo info in teamPositions)
                    {
                        if (info.Position.distanceSq(goal1) < goalieBoxAvoid * goalieBoxAvoid
                            && info.ID != id)
                        {
                            obstacles.Add(new Obstacle(goal1, goalieBoxAvoid));
                            break;
                        }
                    }
                    //goal2
                    foreach (RobotInfo info in teamPositions)
                    {
                        if (info.Position.distanceSq(goal2) < goalieBoxAvoid * goalieBoxAvoid
                            && info.ID != id)
                        {
                            obstacles.Add(new Obstacle(goal2, goalieBoxAvoid));
                            break;
                        }
                    }
                }

                if (destination.distanceSq(lastDestination[id]) > .5 * .5)
                    //if (lastDestination[id].distanceSq(destination) > .5 * .5)
                    avoidingObstacle[id] = false;
                lastDestination[id] = destination;

                foreach (Obstacle o in obstacles)
                {
                    if (position.distanceSq(o.position) < o.size * o.size)
                    {
                        return new NavigationResults(o.position + (1d + o.size) * (position - o.position).normalize());
                    }
                }

                if (!avoidingObstacle[id])
                {
                    roundsSinceTrace[id]++;
                    Vector2 wanted = position + lookAheadDist * (destination - position).normalize();
                    Obstacle o = blockingObstacle(wanted, obstacles, 1);
                    if (o == null)
                        return new NavigationResults(wanted);
                    avoidingObstacle[id] = true;
                    continueDistanceSq[id] = position.distanceSq(destination);
                    lastDirection[id] = (destination - position).cartesianAngle();
                    //Robocup.Plays.CommonFunctions.crossproduct
                    if (roundsSinceTrace[id] > 20)
                    {
                        traceDirection[id] = Math.Sign(UsefulFunctions.crossproduct(destination, position, o.position));
                        if (traceDirection[id] == 0)
                            traceDirection[id] = 1;
                    }
                }
                roundsSinceTrace[id] = 0;
                //so now avoidingObstacle[id] should be true
                if (position.distanceSq(destination) < continueDistanceSq[id] - getCloserAmount)
                {
                    avoidingObstacle[id] = false;
                    return navigate(id, position, destination, teamPositions, enemyPositions, ballPosition, avoidBallDist);
                }
                double traceDir = traceDirection[id];
                double direction = lastDirection[id];
                int count = 0;
                if (blockingObstacle(extend(position, direction), obstacles, 1) == null)
                {
                    while (blockingObstacle(extend(position, direction + angleSweep), obstacles, 1) == null)
                    {
                        direction += traceDir * angleSweep;
                        count++;
                        if (count > Math.PI * 2 / angleSweep + 10)
                        {
                            avoidingObstacle[id] = false;
                            direction = (destination - position).cartesianAngle();
                            break;
                        }

                    }
                    lastDirection[id] = direction;
                    return new NavigationResults(extend(position, direction));
                }
                while (blockingObstacle(extend(position, direction), obstacles, 1d + count / 1000d) != null)
                {
                    direction -= traceDir * angleSweep;
                    count++;
                }
                lastDirection[id] = direction;
                return new NavigationResults(extend(position, direction));
            }


            public void drawLast(System.Drawing.Graphics g, ICoordinateConverter c)
            {
            }
        }
    }

}
