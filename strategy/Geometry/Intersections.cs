using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Infrastructure;

/*
 * This file stores all the code for determining the intersection of two lines/circles.  Since there's
 * the arbitrary decision of how to label the intersections, it's good that it's handled in one place
 * rather than both in the designer and the interpreter.
 * 
 * There are also these things that I've called "ImplicitAssumptions."  For instance, what do you do if
 * a point defined by the intersection of two circles doesn't exist, because the two circles don't intersect?
 * And what if that point is used in other things...you get the idea.  If that happens, then an implicit
 * assumption has been failed, which counts as a condition failing.  These may also be included in actions;
 * for instance, maybe you have an action which is to shoot the ball.  What if that robot isn't anywhere
 * near the ball?  So implicit assumptions are basically conditions that the designer/interpreter will handle
 * for you.  Eventually, maybe there'll be a list of them so people will know what they do and don't have to
 * worry about in actions (for instance, do people have to worry about having a clear shot before shooting?).
 */
namespace Robocup.Geometry
{

    public static class Intersections
    {
        static public Vector2 intersect(Line l1, Line l2)
        {
            return LineLineIntersection.Intersect(l1, l2);
        }
        static public Vector2 intersect(Line l, Circle c, int whichintersection)
        {
            return LineCircleIntersection.Intersect(l, c, whichintersection);
        }
        static public Vector2 intersect(Circle c1, Circle c2, int whichintersection)
        {
            return PlayCircleCircleIntersection.Intersect(c1, c2, whichintersection);
        }
    }

    /// <summary>
    /// This signifies that an assumption implicitly built into a play
    /// (such as the fact that a certain intersection exists) has failed.
    /// </summary>
    public class ImplicitAssumptionFailedException : ApplicationException
    {
        public ImplicitAssumptionFailedException(string s) : base(s) { }
    }
    public class NoIntersectionException : ImplicitAssumptionFailedException
    {
        public NoIntersectionException(string s) : base(s) { }
    }
    /* the way that the circle-circle intersection labeling is resolved, is you label one of the circles circle1
     * (which automaticall happens because you have to place one circle first), and then look at the angle O1O2P,
     * where O1 is the center of circle 1, and P is one of the points of intersection.  One of the points will have
     * an angle that is greater than 180º, the other will have one that is less.  You can distinguish between the two
     * by using the cross product, and keep track of which one the user clicked on.
     */
    static public class PlayCircleCircleIntersection
    {
        static public Vector2 Intersect(Circle c0, Circle c1, int whichintersection)
        {
            Vector2[] bothpoints = GetPoints(c0, c1);

            Vector2 p0 = c0.getCenter();
            Vector2 p1 = c1.getCenter();

            if (whichintersection == 0)
                throw new ApplicationException("CircleCircleIntersection.getPoint() called, but the direction to find the point is not defined");

            int angle = anglesign(p1, p0, bothpoints[0]);
            if (angle == whichintersection)
                return bothpoints[0];
            else if (angle == -whichintersection)
                return bothpoints[1];
            throw new ApplicationException("Unhandled case in CircleCircleIntersection.getPoint()");
        }
        /// <summary>
        /// Given two circles and a point of intersection, returns which intersection number it is closer to.
        /// </summary>
        static public int WhichIntersection(Circle c0, Circle c1, Vector2 p)
        {
            return anglesign(c1.getCenter(), c0.getCenter(), p);
        }
        static private Vector2[] GetPoints(Circle c0, Circle c1)
        {
            Vector2 p0 = c0.getCenter();
            Vector2 p1 = c1.getCenter();
            float d = (float)Math.Sqrt(p0.distanceSq(p1));
            float r0 = c0.Radius;
            float r1 = c1.Radius;
            if (d > r0 + r1 || d < Math.Abs(r1 - r0))
            {
                throw new NoIntersectionException("No intersection!");
            }
            float a = (r0 * r0 - r1 * r1 + d * d) / (2 * d);

            Vector2[] bothpoints = new Vector2[2];
            float newx = p0.X + (p1.X - p0.X) * a / d;
            float newy = p0.Y + (p1.Y - p0.Y) * a / d;
            float h = (float)Math.Sqrt(r0 * r0 - a * a);
            float dx = h * (p1.Y - p0.Y) / d;
            float dy = h * (p0.X - p1.X) / d;
            bothpoints[0] = new Vector2(newx + dx, newy + dy);
            bothpoints[1] = new Vector2(newx - dx, newy - dy);
            return bothpoints;
        }
        private static int anglesign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            float crossp = UsefulFunctions.crossproduct(p1, p2, p3);//(p1.X - p2.X) * (p3.Y - p2.Y) - (p3.X - p2.X) * (p1.Y - p2.Y);
            return Math.Sign(crossp);
        }
    }

    /* the way the line-circle intersection labeling is resolved, is again, you have to arbitrarily label the line
     * to have a direction (which automatically happens too, because the two points have to be arranged somehow).
     * You can arbitrarily call that direction positive, and the other negative.  So the two points have "values" on
     * this psuedo number-line, with one bigger than the other.  You keep track of whether the user clicked on the 
     * more positive one or more negative one.
     */
    static public class LineCircleIntersection
    {
        static public Vector2 Intersect(Line line, Circle circle, int whichintersection)
        {
            Vector2[] linepoints = line.getPoints();
            if (whichintersection == 1)
            {
                Vector2 temp = linepoints[0];
                linepoints[0] = linepoints[1];
                linepoints[1] = temp;
            }

            double[] dists = new double[2];
            Vector2[] points = getPoints(line, circle);
            dists[0] = distalongline(points[0], linepoints);
            dists[1] = distalongline(points[1], linepoints);

            if (dists[0] > dists[1])
                return points[0];
            else
                return points[1];
        }
        static public int WhichIntersection(Line line, Circle circle, Vector2 p)
        {
            Vector2[] points = getPoints(line, circle);
            double dist = distalongline(p, line.getPoints());
            float d0 = UsefulFunctions.distancesq(points[0], p);
            float d1 = UsefulFunctions.distancesq(points[1], p);
            Vector2 otherpoint = points[0];
            if (d1 > d0)
                otherpoint = points[1];
            double dist2 = LineCircleIntersection.distalongline(otherpoint, line.getPoints());
            if (dist > dist2)
                return 0;
            else
                return 1;
        }
        private static double distalongline(Vector2 p, Vector2[] linepoints)
        {
            double d0 = Math.Sqrt(UsefulFunctions.distancesq(p, linepoints[0]));
            double d1 = Math.Sqrt(UsefulFunctions.distancesq(p, linepoints[1]));
            double d = Math.Sqrt(UsefulFunctions.distancesq(linepoints[0], linepoints[1]));
            if (d1 - d0 >= d * .99)
                return d0;
            else
                return -d0;
        }
        static private Vector2[] getPoints(Line line, Circle circle)
        {
            Vector2[] points = (Vector2[])line.getPoints().Clone();
            Vector2 center = circle.getCenter();
            points[0] -= center;
            points[1] -= center;

            float dx = points[1].X - points[0].X;
            float dy = points[1].Y - points[0].Y;
            float drs = dx * dx + dy * dy;
            float dr = (float)Math.Sqrt(drs);
            float D = points[0].X * points[1].Y - points[1].X * points[0].Y;
            float r = circle.Radius;
            float det = r * r * dr * dr - D * D;
            if (det < 0)
            {
                throw new NoIntersectionException("no intersection!");
                //throw new Exception("There is no intersection of line " + line.getName() + " and circle " + circle.getName() + ".");
            }

            Vector2[] rtnpoints = new Vector2[2];

            float ddx = (float)(Math.Sqrt(det) * sign(dy) * dx);
            float ddy = (float)(Math.Sqrt(det) * Math.Abs(dy));
            float x0 = dy * D;
            float y0 = -dx * D;

            rtnpoints[0] = new Vector2((x0 - ddx) / drs, (y0 - ddy) / drs);
            rtnpoints[1] = new Vector2((x0 + ddx) / drs, (y0 + ddy) / drs);

            rtnpoints[0] += center;
            rtnpoints[1] += center;

            return rtnpoints;
        }
        static private int sign(float f)
        {
            if (f < 0)
                return -1;
            return 1;
        }
    }
    static public class LineLineIntersection
    {
        static public Vector2 Intersect(Line line0, Line line1)
        {
            Vector2[] l1 = line0.getPoints();
            Vector2[] l2 = line1.getPoints();
            float denom = (l2[1].Y - l2[0].Y) * (l1[1].X - l1[0].X) - (l2[1].X - l2[0].X) * (l1[1].Y - l1[0].Y);
            if (denom == 0) //the lines are parallel
            {
                throw new NoIntersectionException("no intersection!");
            }
            float numerator = (l2[1].X - l2[0].X) * (l1[0].Y - l2[0].Y) - (l2[1].Y - l2[0].Y) * (l1[0].X - l2[0].X);
            float x = l1[0].X + numerator * (l1[1].X - l1[0].X) / denom;
            float y = l1[0].Y + numerator * (l1[1].Y - l1[0].Y) / denom;
            return new Vector2(x, y);
        }
    }
}
