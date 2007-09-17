using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Infrastructure;
using Robocup.Geometry;

namespace Navigation
{
    namespace Examples
    {

        //I added these so that there are concrete classes to create.
        //When the tester queries for any Navigator classes, it can't create a generic class,
        //because it won't know what generic type parameters to provide.
        public class LookAheadBug : LookAheadNavigator<Navigation.Examples.BugNavigator> { }
        public class LookAheadPotential : LookAheadNavigator<PotentialBasedNavigator> { }


        /// <summary>
        /// A class that takes on type of navigator, and extends it by evaluating it and trying to optimize the path.
        /// </summary>
        /// <typeparam name="T">The type of navigator to extend</typeparam>
        public class LookAheadNavigator<T> : INavigator where T : INavigator, new()
        {
            T navigator = new T();

            public LookAheadNavigator()
            {
                for (int i = 0; i < lastReturn.Length; i++)
                {
                    lastReturn[i] = null;
                }
            }

            const int TEAMSIZE = 50;
            /// <summary>
            /// The amount that this moves the virtual robot each time in creating th path
            /// </summary>
            const float stepSize = .05f;
            /// <summary>
            /// How close the path has to get to the goal before it's considered there.
            /// </summary>
            const float destTolerance = .01f;

            //these are only used to backtrack once we find a path, not in finding the path:
            const float avoidRobotDist = .22f;
            const float extraAvoidBallDist = .1f;

            /// <summary>
            /// Returns if this line is blocked by any of the obstacles.
            /// </summary>
            private bool blocked(Line line, List<Obstacle> obstacles)
            {
                foreach (Obstacle o in obstacles)
                {
                    double d = line.distFromSegment(o.position);
                    if (d < o.size)
                        return true;
                }
                return false;
            }

            //These store some previous information:
            /// <summary>
            /// An array of the last returned value from navigate() for each robot.
            /// We store this so that on the next time, we can just return the previous
            /// destination if it's still reachable.
            /// </summary>
            NavigationResults[] lastReturn = new NavigationResults[TEAMSIZE];
            /// <summary>
            /// An array of the destination that the path was calculated to, for each robot.
            /// If the destination changes too much, then the path should be recalculated.
            /// Also, if the path changes only a little, but many times (so that the sum is large),
            /// it should still recalculate the path.  So this only holds the last destination
            /// that was calculated for, not necessarily the last one asked for.
            /// </summary>
            Vector2[] lastDestination = new Vector2[TEAMSIZE];

            readonly Vector2 goal1 = new Vector2(-2.45f, 0f);
            readonly Vector2 goal2 = new Vector2(2.45f, 0f);
            const float goalieBoxAvoid = .65f;
            public NavigationResults navigate(int id, Vector2 position, Vector2 destination, RobotInfo[] teamPositions, RobotInfo[] enemyPositions, BallInfo ballPosition, float avoidBallDist)
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

                //if we can go straight to the destination, go for it
                if (!blocked(new Line(position, destination), obstacles))
                {
                    lastReturn[id] = new NavigationResults(destination);
                    return lastReturn[id];
                }
                foreach (Obstacle o in obstacles)
                {
                    Line l = new Line(position, destination);
                    if (o.position.distanceSq(destination) < o.size * o.size)
                    {
                        Circle obstacleCircle = new Circle(o.position, o.size);
                        Vector2 newDestination = Intersections.intersect(l, obstacleCircle, 0);
                        newDestination = newDestination + .15f * (newDestination - o.position);
                        return this.navigate(id, position, newDestination, teamPositions, enemyPositions, ballPosition, avoidBallDist);
                        //System.Windows.Forms.MessageBox.Show("moving destination from\n"+destination+"\nto\n"+newDestination);
                        //return recursed;
                        //return this.navigate(
                    }
                }

                //if we returned something last time,
                if (lastReturn[id] != null)
                {
                    //if we're there, then move one
                    if (position.distanceSq(lastReturn[id].waypoint) < .1 * .1)
                        lastReturn[id] = null;
                    //if we can't get there, move on
                    else if (blocked(new Line(position, lastReturn[id].waypoint), obstacles))
                        lastReturn[id] = null;
                    //if the destination has moved, then move on
                    else if (destination.distanceSq(lastDestination[id]) > .2 * .2)
                        //else if (lastDestination[id].distanceSq(destination) > .2 * .2)
                        lastReturn[id] = null;
                    //otherwise, just return what we returned last time
                    else
                        return lastReturn[id];
                }

                //only update this is more computation is done:
                lastDestination[id] = destination;

                //go through and predict a path based on current information:
                steps = new List<Vector2>();
                Vector2 current = position;
                while (current.distanceSq(destination) >= destTolerance * destTolerance)
                {
                    if (steps.Count > 500)
                        return navigator.navigate(id, position, destination, teamPositions, enemyPositions, ballPosition, avoidBallDist);
                    NavigationResults results = navigator.navigate(id, current, destination, teamPositions, enemyPositions, ballPosition, avoidBallDist);
                    Vector2 waypoint = results.waypoint;
                    float step = (float)Math.Min(stepSize, Math.Sqrt((waypoint - current).magnitudeSq()));
                    if (!blocked(new Line(waypoint, position), obstacles))
                        step = (float)Math.Max(stepSize, Math.Sqrt((waypoint - current).magnitudeSq()));
                    current = current + step * (waypoint - current).normalize();
                    steps.Add(current);
                }
                //if the robot is already within destTolerance, then nothing is ever added to steps
                //in that case, then you just want to return the destination anyway
                if (steps.Count == 0)
                {
                    steps.Add(destination);
                    return new NavigationResults(steps[0]);
                }

                //now start at the end of the path, and work your way backwards until you find a point
                //that is reachable from the current position:
                for (int i = steps.Count - 1; i >= 0; i--)
                {
                    Line line = new Line(position, steps[i]);
                    if (!blocked(line, obstacles))
                    {
                        lastReturn[id] = new NavigationResults(steps[i]);
                        break;
                    }
                }
                //if we couldn't find such a point, then just return what the base navigator returned
                if (lastReturn[id] == null)
                {
                    lastReturn[id] = navigator.navigate(id, position, destination, teamPositions, enemyPositions, ballPosition, avoidBallDist);
                }
                return lastReturn[id];
            }

            List<Vector2> steps = new List<Vector2>();
            //for debug drawing, just draw the predicted path.
            public void drawLast(System.Drawing.Graphics g, ICoordinateConverter c)
            {
                System.Drawing.Brush b = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                foreach (Vector2 p in steps)
                {
                    Vector2 pixelPoint = c.fieldtopixelPoint(p);
                    g.FillRectangle(b, pixelPoint.X - 1, pixelPoint.Y - 1, 2, 2);
                }
                b.Dispose();
            }
        }
    }
}
