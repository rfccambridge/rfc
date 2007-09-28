using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Geometry;
using Robocup.Core;
using Robocup.Utilities;

namespace Navigation
{
    namespace Examples
    {
        public class PotentialBasedNavigator : INavigator
        {
            //for labeling purposes only
            const double avoidRobotDist = .25;
            const double extraAvoidBallDist = .1;
            Vector2 lastDestination;
            List<Obstacle> lastObstacles = null;

            public void drawLast(System.Drawing.Graphics g, ICoordinateConverter c)
            {
                if (lastObstacles != null)
                {
                    for (double x = -2.5; x <= 2.5; x += .2)
                    {
                        for (double y = -2; y <= 2; y += .2)
                        {
                            Vector2 position = new Vector2(x, y);
                            Vector2 force = calcForce(position, lastDestination, lastObstacles);
                            if (force.magnitudeSq() >= 2 * 2)
                            {
                                force = 2 * force.normalize();
                            }
                            Arrow r = new Arrow(c.fieldtopixelPoint(position), c.fieldtopixelPoint(position + .1 * force), System.Drawing.Color.Blue, 1);
                            r.draw(g);
                        }
                    }
                }
            }

            private Vector2 repulsion(Obstacle o, Vector2 position)
            {
                double d = Math.Sqrt(o.position.distanceSq(position));

                double scale = 1.2;
                double magnitude = o.size / (d);
                magnitude = scale * magnitude * magnitude;
                return magnitude * (position - o.position).normalize();
            }
            private Vector2 attraction(Vector2 destination, Vector2 position)
            {
                double scale = 1;
                if (destination.distanceSq(position) < .5 * .5)
                    scale = 2;
                return scale * (destination - position).normalize();
            }
            private Vector2 calcForce(Vector2 position, Vector2 destination, List<Obstacle> obstacles)
            {
                Vector2 total = attraction(destination, position);
                foreach (Obstacle o in obstacles)
                {
                    total += repulsion(o, position);
                }
                Vector2 add = .01 * (new Vector2(destination.Y - position.Y, position.X - destination.X)).normalize();
                while (total.magnitudeSq() < .5 * .5)
                {
                    total += add;
                }
                return total;
            }

            //List<Vector2> sums = new List<Vector2>();

            public NavigationResults navigate(int id, Vector2 position, Vector2 destination, RobotInfo[] teamPositions, RobotInfo[] enemyPositions, BallInfo ballPosition, double avoidBallDist)
            {
                this.lastDestination = destination;
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
                lastObstacles = obstacles;

                Vector2 total = calcForce(position, destination, obstacles);
                return new NavigationResults(position + .1 * (total.normalize()));
            }
        }
    }

}
