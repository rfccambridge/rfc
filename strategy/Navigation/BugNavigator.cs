using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
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
            Vector2 lastPoint;
            
            double angleSweep = .01; //.1
            double avoidRobotDist = .28;
            double extraAvoidBallDist = .1;
            double lookAheadDist = .4;//.18;//.18; //.15
            //double getCloserAmount = .1;    

            public BugNavigator()
            {
                for (int i = 0; i < TEAMSIZE; i++)
                {
                    traceDirection[i] = 1;
                }
            }

            public void setLookAheadDist(double newDistance)
            {
                lookAheadDist = newDistance;
            }

            public void setAvoidRobotDist(double newDistance)
            {
                avoidRobotDist = newDistance;
            }


            Obstacle blockingObstacle(Vector2 position, List<Obstacle> obstacles, double gettingTiredFactor, Vector2 destination)
            {
                foreach (Obstacle o in obstacles)
                {
                    if ((position - o.position).magnitudeSq() > 0.00001) {//make sure you're not yourself
                        Vector2 botObst = o.position - position;
                        Vector2 botDest = destination - position;
                        double angle_DestObst = UsefulFunctions.angleDifference(botObst.cartesianAngle(), botDest.cartesianAngle());
                        double obstDistsq = position.distanceSq(o.position);
                        double destDistsq = position.distanceSq(destination);
                        //if the you're close enough to the obstacle it's within ninety degrees of planned direction and it's closer than the destination
                        if (obstDistsq < o.size * o.size && Math.Abs(angle_DestObst) < Math.PI / 2 && obstDistsq<destDistsq)
                            return o;
                    }
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
                if (avoidBallDist > 0 && ballPosition != null)
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

                //checks if really close to an obstacle
                foreach (Obstacle o in obstacles)
                {
                    Vector2 botObst = o.position - position;
                    Vector2 botDest = destination - position;
                    double angle_DestObst = UsefulFunctions.angleDifference(botObst.cartesianAngle(), botDest.cartesianAngle());
                    if (position.distanceSq(o.position) < o.size * o.size/5 &&
                        Math.Abs(angle_DestObst) < Math.PI / 2)

                    {
                        // unclear what this does
                        //return new NavigationResults(o.position + (1d + o.size) * (position - o.position).normalize());

                        // this makes the robot go to a waypoint located along the vector that is 90 degrees cc from 
                        // the vector from the robot to the obstacle

                        //Sometimes it thinks itself is an obsticle, or vision sees it as another robot and then the system remembers it,
                        //and if the obsticle and the robot are in the same position it causes problems, because of division by 0.
                        if ((position - o.position).magnitudeSq() > 0.00001) {
                            Vector2 towardsObst = (position - o.position).normalize();
                            towardsObst = towardsObst.normalizeToLength(lookAheadDist);
                            Vector2 awayFromObst = new Vector2(towardsObst.X, -1 * towardsObst.Y);
                            return new NavigationResults(position + awayFromObst);
                        }
                    }
                }

                if (true)//if (!avoidingObstacle[id])
                {
                    //if you're really close to the destination have that be the next point
                    if (position.distanceSq(destination) <= lookAheadDist * lookAheadDist)
                        return new NavigationResults(destination);

                    roundsSinceTrace[id]++;
                    Vector2 wanted = position + lookAheadDist * (destination - position).normalize();
                    Obstacle o = blockingObstacle(wanted, obstacles, 1, destination);
                    if (o == null)//this is redundant with the other chunk marded xkcd I think
                        return new NavigationResults(wanted);
                    
                    avoidingObstacle[id] = true;
                    continueDistanceSq[id] = position.distanceSq(destination);
                    lastDirection[id] = (destination - position).cartesianAngle();
                    //Robocup.Plays.CommonFunctions.crossproduct
                    /*if (roundsSinceTrace[id] > 20)
                    {
                        traceDirection[id] = Math.Sign(UsefulFunctions.crossproduct(destination, position, o.position));
                        if (traceDirection[id] == 0)
                            traceDirection[id] = 1;
                    }*/
                }
                roundsSinceTrace[id] = 0;
                //so now avoidingObstacle[id] should be true
                /*if (position.distanceSq(destination) < continueDistanceSq[id] - getCloserAmount)
                {
                    avoidingObstacle[id] = false;
                    return navigate(id, position, destination, teamPositions, enemyPositions, ballPosition, avoidBallDist);
                }*/
                double traceDir = traceDirection[id];
                double direction = lastDirection[id];
                int count = 0;
               
                //this is the main tangent bug section
                if (blockingObstacle(extend(position, direction), obstacles, 1, destination) == null) {
                    //no obstacle so onward! I think this is redundat with a chunk above marked xkcd
                    Vector2 wanted = position + lookAheadDist * (destination - position).normalize();
                    return new NavigationResults(wanted);
                }
                else {
                    //sweep one direction to try and avoid obstacles
                    while (blockingObstacle(extend(position, direction), obstacles, 1, destination) != null) {
                        direction += traceDir * angleSweep;
                        count++;
                        if (count*angleSweep > Math.PI+.1) {
                            //this way didn't work so reset the direction and try again
                            direction = (destination - position).cartesianAngle();
                            break;
                        }
                    }
                    if (blockingObstacle(extend(position, direction), obstacles, 1, destination) == null) {
                        lastDirection[id] = direction;
                        return new NavigationResults(extend(position, direction));
                    }
                    
                    while (blockingObstacle(extend(position, direction), obstacles, 1, destination) != null) {
                        direction -= traceDir * angleSweep;
                        count++;
                        if (count*angleSweep > Math.PI+.1) {
                            //this way didn't work so give up and go in backwards
                            break;
                        }
                    }
                    lastDirection[id] = direction;

                    // save last point
                    lastPoint = extend(position, direction);

                    return new NavigationResults(lastPoint);
                }
            }


            public void drawLast(System.Drawing.Graphics g, ICoordinateConverter c)
            {
                
            }
        }
    }

}
