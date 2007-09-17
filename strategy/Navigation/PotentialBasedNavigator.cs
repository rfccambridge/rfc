using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Infrastructure;
using Robocup.Geometry;
using Robocup.Core;

namespace Navigation
{
    namespace Examples
    {
        public class PotentialBasedNavigator : INavigator
        {
            //for labeling purposes only
            const float avoidRobotDist = .25f;
            const float extraAvoidBallDist = .1f;
            Vector2 lastDestination;
            List<Obstacle> lastObstacles = null;

            public void drawLast(System.Drawing.Graphics g, ICoordinateConverter c)
            {
                if (lastObstacles != null)
                {
                    for (float x = -2.5f; x <= 2.5f; x += .2f)
                    {
                        for (float y = -2f; y <= 2f; y += .2f)
                        {
                            Vector2 position = new Vector2(x, y);
                            Vector2 force = calcForce(position, lastDestination, lastObstacles);
                            if (force.magnitudeSq() >= 2 * 2)
                            {
                                force = 2f * force.normalize();
                            }
                            Arrow r = new Arrow(c.fieldtopixelPoint(position), c.fieldtopixelPoint(position + .1f * force), System.Drawing.Color.Blue, 1);
                            r.draw(g);
                        }
                    }
                }
            }

            private Vector2 repulsion(Obstacle o, Vector2 position)
            {
                float d = (float)Math.Sqrt(o.position.distanceSq(position));

                float scale = 1.2f;
                float magnitude = o.size / (d);
                magnitude = scale * magnitude * magnitude;
                return magnitude * (position - o.position).normalize();
            }
            private Vector2 attraction(Vector2 destination, Vector2 position)
            {
                float scale = 1f;
                if (destination.distanceSq(position) < .5 * .5)
                    scale = 2f;
                return scale * (destination - position).normalize();
            }
            private Vector2 calcForce(Vector2 position, Vector2 destination, List<Obstacle> obstacles)
            {
                Vector2 total = attraction(destination, position);
                foreach (Obstacle o in obstacles)
                {
                    total += repulsion(o, position);
                }
                Vector2 add = .01f * (new Vector2(destination.Y - position.Y, position.X - destination.X)).normalize();
                while (total.magnitudeSq() < .5 * .5)
                {
                    total += add;
                }
                return total;
            }

            //List<Vector2> sums = new List<Vector2>();

            public NavigationResults navigate(int id, Vector2 position, Vector2 destination, RobotInfo[] teamPositions, RobotInfo[] enemyPositions, BallInfo ballPosition, float avoidBallDist)
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
                return new NavigationResults(position + .1f * (total.normalize()));
            }
        }
    }

}
