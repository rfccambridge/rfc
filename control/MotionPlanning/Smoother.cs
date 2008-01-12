using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Core;
using Robocup.CoreRobotics;

namespace Robocup.MotionControl
{
    static class Smoother
    {
        const double valueTol = .01;

        static private RobotInfo Extend(RobotInfo start, Vector2 end)
        {
            return Common.ExtendRV(start, end, new List<Obstacle>()).extension;
        }

        static private bool Blocked(RobotInfo info, List<Obstacle> obstacles)
        {
            foreach (Obstacle o in obstacles)
            {
                if (o.position.distanceSq(info.Position) < o.size * o.size)
                    return true;
            }
            return false;
        }
        static private MotionPlanningResults SmoothWithValue(RobotInfo startState, List<Vector2> waypoints,
            List<Obstacle> obstacles, double maxDistance)
        {
            MotionPlanningResults rtn = null;
            RobotInfo cur = startState;
            for (int i = 0; i < waypoints.Count; i++)
            {
                Vector2 v = waypoints[i];
                while (v.distanceSq(cur.Position) > maxDistance * maxDistance || (i == waypoints.Count - 1 && v.distanceSq(cur.Position) > .01*.01))
                {
                    cur = Extend(cur, v);
                    if (Blocked(cur, obstacles))
                        return null;
                    if (rtn == null)
                        rtn = new MotionPlanningResults(WheelSpeedsExtender.GetWheelSpeeds(startState, cur));
                }
            }
            return rtn;
        }

        static public MotionPlanningResults Smooth(RobotInfo startState, List<Vector2> waypoints,
            List<Obstacle> obstacles)
        {
            double lowerBound = .05;
            double test = .1;
            while (SmoothWithValue(startState, waypoints, obstacles, test) != null)
            {
                lowerBound = test;
                test *= 2;
            }
            double upperBound = test;
            while (upperBound - lowerBound > valueTol)
            {
                double guess = (upperBound + lowerBound) / 2;
                if (SmoothWithValue(startState, waypoints, obstacles, guess) == null)
                    upperBound = guess;
                else
                    lowerBound = guess;
            }
            return SmoothWithValue(startState, waypoints, obstacles, lowerBound);
        }
    }
}
