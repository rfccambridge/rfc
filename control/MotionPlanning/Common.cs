using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Geometry;
using Robocup.Core;
using Robocup.CoreRobotics;

using System.Drawing;

namespace Robocup.MotionControl
{
    public enum ExtendResultType
    {
        Success, Blocked, Destination
    }
    public class ExtendResults<T>
    {
        public T extension;
        public ExtendResultType resultType;
        public ExtendResults(T extension, ExtendResultType type)
        {
            this.extension = extension;
            this.resultType = type;
        }
    }
    //TODO shouldn't take an Object as state
    public delegate ExtendResults<T1> Extender<T1, T2>(T1 start, T2 end, object state);


    /// <summary>
    /// A simple class to represent obstacles, it gives a useful way to
    /// treat robots and the ball as similar things.  Also, setting the
    /// avoid-radius can be done once at the beginning and stored in the
    /// obstacle.
    /// </summary>
    //yes it should probably be a struct, but then you can't return null when you ask for the blocking obstacle
    public class Obstacle
    {
        public Obstacle(Vector2 pos, double size)
        {
            this.position = pos;
            this.size = size;
        }
        public Vector2 position;
        public double size;
    }
    static public class Common
    {
        const double extendDistance = .20;
        static public ExtendResults<Vector2> ExtendVV(Vector2 start, Vector2 end, object state)
        {
            List<Obstacle> obstacles = (List<Obstacle>)state;
            if (start.distanceSq(end) < extendDistance * extendDistance)
                return new ExtendResults<Vector2>(end, ExtendResultType.Destination);
            Vector2 next = (end - start).normalizeToLength(extendDistance) + start;
            if (SegmentBlocked(start, next, obstacles))
                return new ExtendResults<Vector2>(next, ExtendResultType.Blocked);
            return new ExtendResults<Vector2>(next, ExtendResultType.Success);
        }
        static public ExtendResults<Vector2> ExtendVR(Vector2 start, RobotInfo end, object state)
        {
            return ExtendVV(start, end.Position, state);
        }

        static public bool Blocked(Vector2 point, List<Obstacle> obstacles)
        {
            foreach (Obstacle o in obstacles)
            {
                if (o.position.distanceSq(point) < o.size * o.size)
                    return true;
            }
            return false;
        }

        //Checks if any part of the line segment from (point) to (point+ray) intersects an obstacle.
        static public bool SegmentBlocked(Vector2 point, Vector2 dest, List<Obstacle> obstacles)
        {
            Vector2 ray = dest - point;
            if (ray.magnitudeSq() < 1e-16)
                return Blocked(point + ray, obstacles);

            Vector2 rayUnit = ray.normalizeToLength(1.0);
            double rayLen = ray.magnitude();

            foreach (Obstacle o in obstacles)
            {
                if (o.position.distanceSq(point) < o.size * o.size)
                    return true;
                if (o.position.distanceSq(dest) < o.size * o.size)
                    return true;

                //See if it intersects in the middle...
                Vector2 pointToObs = o.position - point;
                double parallelDist = pointToObs * rayUnit;
                if (parallelDist <= 0 || parallelDist >= rayLen)
                    continue;

                double perpDist = Math.Abs(Vector2.cross(pointToObs, rayUnit));
                if (perpDist < o.size)
                    return true;
            }
            return false;
        }

        static readonly Random r = new Random();
        static public Vector2 RandomStateV()
        {
            return new Vector2((r.NextDouble() - .5) * 2 * 2.75, (r.NextDouble() - .5) * 2 * 2);
        }

    }
}
