using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Infrastructure;
using Robocup.Geometry;

namespace Navigation
{
    class RelaxingNavigator : INavigator
    {
        #region INavigator Members

        //these are hard limits -- it will try to avoid by more
        const float avoidRobotDist = .22f;
        const float extraAvoidBallDist = .1f;
        const float maxExtraAvoid = .2f;

        const int numWaypoints = 8;
        Dictionary<int, Vector2[]> old_waypoints = new Dictionary<int, Vector2[]>();
        Vector2[] waypoints;

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

        public void drawLast(System.Drawing.Graphics g, Robocup.Infrastructure.ICoordinateConverter c)
        {
            if (waypoints != null)
            {
                System.Drawing.Brush b = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                foreach (Vector2 p in waypoints)
                {
                    Vector2 pixelPoint = c.fieldtopixelPoint(p);
                    g.FillRectangle(b, pixelPoint.X - 1, pixelPoint.Y - 1, 2, 2);
                }
                b.Dispose();
            }
        }

        private Vector2 repulsion(Obstacle o, Line l)
        {
            float d = (float)l.distFromSegment(o.position);
            if (d > o.size + maxExtraAvoid)
                return new Vector2(0, 0);
            Vector2 projection = l.projectionOntoLine(o.position);
            if ((projection - o.position).magnitudeSq() < 1E-9)
                return new Vector2(0, 0);

            float scale = .5f;
            float leeway = Math.Max(.001f, d - o.size);
            float magnitude = scale / (leeway * leeway);
            return magnitude * (projection - o.position).normalize();
        }

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
                waypoints = null;
                old_waypoints[id] = null;
                return new NavigationResults(destination);
            }
            if (!old_waypoints.ContainsKey(id) || old_waypoints[id] == null)
            {
                waypoints = new Vector2[numWaypoints + 2];
                for (int i = 0; i < numWaypoints + 2; i++)
                {
                    float percent = (float)i / (numWaypoints + 1);
                    waypoints[i] = position + percent * (destination - position);
                }
            }
            else
            {
                waypoints = old_waypoints[id];
                waypoints[0] = position;
                for (int i = 0; i < numWaypoints + 2; i++)
                {
                    float percent = (float)i / (numWaypoints + 1);
                    const float keepold = .8f;
                    waypoints[i] = keepold * waypoints[i] + (1 - keepold) * (position + percent * (destination - position));
                }
            }
            Vector2[] forces = new Vector2[numWaypoints + 2];
            for (int step = 0; step < 25; step++)
            {
                forces[0] = forces[numWaypoints + 1] = new Vector2(0, 0);
                for (int i = 1; i < numWaypoints + 1; i++)
                {
                    //initial attraction to the midpoint
                    forces[i] = .5f * (waypoints[i + 1] + waypoints[i - 1]) - waypoints[i];
                }
                for (int line = 0; line < numWaypoints + 1; line++)
                {
                    Line l = new Line(waypoints[line], waypoints[line + 1]);
                    foreach (Obstacle o in obstacles)
                    {
                        Vector2 newforce = repulsion(o, l);
                        forces[line] += newforce;
                        forces[line + 1] += newforce;
                    }
                }
                for (int i = 1; i < numWaypoints + 1; i++)
                {
                    //initial attraction to the midpoint
                    if (forces[i].magnitudeSq() > 1)
                        forces[i] = forces[i].normalize();
                    waypoints[i] += .1f / (.1f*step+1) * forces[i];
                }
            }
            old_waypoints[id] = waypoints;
            return new NavigationResults(waypoints[1]);
        }

        #endregion
    }
}
