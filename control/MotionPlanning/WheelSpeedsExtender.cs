using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Core;

namespace Robocup.MotionControl
{
    static public class WheelSpeedsExtender
    {
        static public WheelSpeeds GetWheelSpeeds(RobotInfo start, RobotInfo goal)
        {
            return GetWheelSpeeds(start, goal.Position);
        }
        static public WheelSpeeds GetWheelSpeeds(RobotInfo start, Vector2 goal)
        {
            Vector2 desiredDirection = goal - start.Position;
            Vector2 lf = new Vector2(1, -1).rotate(start.Orientation);
            Vector2 rf = new Vector2(1, 1).rotate(start.Orientation);
            Vector2 lb = new Vector2(1, 1).rotate(start.Orientation);
            Vector2 rb = new Vector2(1, -1).rotate(start.Orientation);
            double plf = lf * desiredDirection;
            double prf = rf * desiredDirection;
            double plb = lb * desiredDirection;
            double prb = rb * desiredDirection;
            double max = Math.Max(Math.Max(Math.Abs(plf), Math.Abs(prf)), Math.Max(Math.Abs(plb), Math.Abs(prb)));
            return new WheelSpeeds((int)(127 * plf / max), (int)(127 * prf / max), (int)(127 * plb / max), (int)(127 * prb / max));
        }
    }
}
