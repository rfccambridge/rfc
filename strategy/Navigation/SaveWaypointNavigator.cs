using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Geometry;
using Robocup.Core;

namespace Navigation
{
    namespace Examples
    {
        class CurrentWithSavingNavigator : SaveWaypointNavigator<Current.CurrentNavigator> { }
        class SaveWaypointNavigator<T> : INavigator where T : INavigator, new()
        {
            T navigator = new T();

            public SaveWaypointNavigator()
            {
                for (int i = 0; i < lastReturn.Length; i++)
                {
                    lastReturn[i] = null;
                }
            }


            const int TEAMSIZE = 5;

            //these are only used to backtrack once we find a path, not in finding the path:
            const double avoidRobotDist = .22;
            const double extraAvoidBallDist = .1;

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

                //if we can go straight to the destination, go for it
                if (!blocked(new Line(position, destination), obstacles))
                {
                    lastReturn[id] = new NavigationResults(destination);
                    return lastReturn[id];
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
                    else if (lastDestination[id].distanceSq(destination) > .2 * .2)
                        lastReturn[id] = null;
                    //otherwise, just return what we returned last time
                    else
                        return lastReturn[id];
                }

                //only update this is more computation is done:
                lastDestination[id] = destination;

                lastReturn[id] = navigator.navigate(id, position, destination, teamPositions, enemyPositions, ballPosition, avoidBallDist);

                return lastReturn[id];
            }
            public void drawLast(System.Drawing.Graphics g, ICoordinateConverter c)
            {
                navigator.drawLast(g, c);
            }
        }
    }

}
