using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Geometry;
using Robocup.Core;
using Robocup.Utilities;

namespace Robocup.PlaySystem
{
    /// <summary>
    // Contains functions that are used by the new, C# play system. Unlike Functions.cs, which provides
    // functions in the appropriate format for the PlaySelector system, these are straightforward C#
    // methods, though many are ported from Functions.cs.
    /// </summary>

    //TODO(davidwu): Almost all of these functions do things that are manually duplicated and coded everywhere else, including
    //in places like the controller, which don't have access to a GameState. Almost alls of these function should be generalized
    //to not depend on a GameState, but rather be static utility functions, and take in the necessary info as arguments
    //(such as a list of robots). Then we can get rid of a lot of duplicated code everywhere.    
    public class PlayFunctions
    {
        GameState state;
        /// <summary>
        /// initialized with a game state
        /// </summary>
        public PlayFunctions(GameState state)
        {
            this.state = state;
        }

        /// <summary>
        /// Current closest robot on a given team to a given point. If requested, includes
        /// only those on our team that are unassigned. If there are no robots
        /// on the team, returns null.
        /// </summary>
        /// <param name="point">Point on the field</param>
        /// <param name="team">The team whose closest robot you are looking for</param>
        /// <returns>Information about the closest robot</returns>
        public RobotInfo closestRobot(Vector2 point, Team team, bool unassigned)
        {
            // iterate over the robots on the given team
            double shortestDistanceSq = double.MaxValue;
            RobotInfo closest = null;
            foreach (RobotInfo info in state.Predictor.GetRobots(team))
            {
                // if unassigned is true, and it is our team, skip this one
                if (unassigned && state.AssignedIDs.Contains(info.ID))
                {
                    continue;
                }

                double dist = info.Position.distanceSq(point);
                if (dist < shortestDistanceSq)
                {
                    shortestDistanceSq = dist;
                    closest = info;
                }
            }

            return closest;
        }

        /// <summary>
        /// Current closest of our unassigned robots to a given point
        /// </summary>
        public RobotInfo closestRobot(Vector2 point)
        {
            return closestRobot(point, state.OurTeam, true);
        }

        /// <summary>
        /// Number of robots on a given team
        /// </summary>
        public int NumRobots(Team team)
        {
            return state.Predictor.GetRobots(team).Count;
        }

        /// <summary>
        /// Number of robots on our team
        /// </summary>
        public int NumRobots()
        {
            return NumRobots(state.OurTeam);
        }

        /// <summary>
        /// Returns whether there is a path from p1 to p2 with at least mindist room around it
        /// </summary>
        public bool pathClear(Vector2 p1, Vector2 p2, double mindist)
        {
            // get distance of closest robot to line
            List<RobotInfo> allinfos = state.Predictor.GetRobots();
 
            double rtn = double.MaxValue;

            foreach (RobotInfo r in allinfos)
            {
                Vector2 position = r.Position;
                if (Math.Sqrt(position.distanceSq(p1)) < mindist ||
                    Math.Sqrt(position.distanceSq(p2)) < mindist)
                    continue;
                rtn = Math.Min(rtn, (new Line(p1, p2)).Segment.distance(position));
            }

            return (rtn >= mindist);
        }

        /// <summary>
        /// Returns whether a point on the field is above a given line on the field 
        /// </summary>
        public bool pointAboveLine(Vector2 point, Line line)
        {
            Vector2 projpoint = line.projectionOntoLine(point);
            return (projpoint.Y < point.Y);
        }

        /// <summary>
        /// Is a point in the boundaries of the soccer field
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool inField(Vector2 point)
        {
            return ((point.X <= Constants.Field.XMAX) && (point.X >= Constants.Field.XMIN) &&
                    (point.Y <= Constants.Field.YMAX) && (point.Y >= Constants.Field.YMIN));
        }

        /// <summary>
        /// Rotate a point a certain angle around a given center
        /// </summary>
        public Vector2 rotatePointAroundAnotherPoint(Vector2 center, Vector2 point, double angle)
        {
            Vector2 v = point - center;
            //Vector2 v = new Vector2(point.X - center.X, point.Y - center.Y);
            double originalAngle = v.cartesianAngle();
            double length = Math.Sqrt(v.magnitudeSq());
            return new Vector2(center.X + Math.Cos(originalAngle + angle) * length, center.Y + Math.Sin(originalAngle + angle) * length);
        }
    }
}
