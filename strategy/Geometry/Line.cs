using System;
using System.Collections.Generic;
using System.Text;

namespace Robocup.Geometry
{
    /// <summary>
    /// A line. Note that although the line is defined by 2 points, a starting and an ending point, 
    /// for intersections and distances, the line is treated as infinitely extended in both directions.
    /// For a line segment, see LineSegment.
    /// </summary>
    public class Line
    {
        private Vector2 p0;
        private Vector2 p1;

        /// <summary>
        /// Creates a line extending from p0 to p1
        /// </summary>
        public Line(Vector2 p0, Vector2 p1)
        {
            this.p0 = p0;
            this.p1 = p1;
        }

        /// <summary>
        /// Creates a line extending from p0 in the desired direction in radians.
        /// </summary>
        public Line(Vector2 p0, double direction)
        {
            this.p0 = p0;
            this.p1 = p0 + Vector2.GetUnitVector(direction);
        }

        /// <summary>
        /// The starting point defining this line
        /// </summary>
        public Vector2 P0
        { get { return p0; } }

        /// <summary>
        /// The ending point defining this line
        /// </summary>
        public Vector2 P1
        { get { return p1; } }

        /// <summary>
        /// The vector from the starting point to the ending point of this line
        /// </summary>
        public Vector2 Direction
        { get { return p1 - p0; } }

        /// <summary>
        /// The finite line segment defined by the same points as this line
        /// </summary>
        public LineSegment Segment
        { get { return new LineSegment(this); } }

        /// <summary>
        /// Returns a line with the start and end reversed.
        /// </summary>
        public static Line operator -(Line l)
        {
            return new Line(l.p1, l.p0);
        }

        /// <summary>
        /// Returns a line translated by the added vector
        /// </summary>
        public static Line operator +(Line l, Vector2 v)
        {
            return new Line(l.p0 + v, l.p1 + v);
        }

        /// <summary>
        /// Returns a line translated by the added vector
        /// </summary>
        public static Line operator +(Vector2 v, Line l)
        {
            return new Line(l.p0 + v, l.p1 + v);
        }

        /// <summary>
        /// Returns a line translated by the negative of the vector
        /// </summary>
        public static Line operator -(Line l, Vector2 v)
        {
            return new Line(l.p0 - v, l.p1 - v);
        }

        /// <summary>
        /// Computes the distance between a point and this line
        /// </summary>
        public double distance(Vector2 p)
        {
            double dx = p1.X - p0.X;
            double dy = p1.Y - p0.Y;
            double mag = Math.Sqrt(dx * dx + dy * dy);
            double crossp = UsefulFunctions.crossproduct(p0, p1, p);
            double dist = crossp / mag;
            return Math.Abs(dist);
        }

        /// <summary>
        /// Computes the point on the line that would result from projecting
        /// p perpendicularly towards the line.
        /// </summary>
        public Vector2 projectionOntoLine(Vector2 p)
        {
            Vector2 tangent = (p1 - p0).normalize();
            double dotp = tangent * (p - p0);
            return p0 + dotp * tangent;
        }

        public override string ToString()
        {
            return "Line(" + p0 + ", " + p1 + ")";
        }
    }

    /// <summary>
    /// A line segment.
    /// </summary>
    public class LineSegment
    {
        private Line l;

        /// <summary>
        /// Creates a line extending from p0 to p1
        /// </summary>
        public LineSegment(Vector2 p0, Vector2 p1)
        {
            this.l = new Line(p0, p1);
        }

        /// <summary>
        /// Creates a line extending from p0 in the desired direction in radians.
        /// </summary>
        public LineSegment(Vector2 p0, double direction)
        {
            this.l = new Line(p0, direction);
        }

        /// <summary>
        /// Creates a line extending from p0 in the desired direction in radians.
        /// </summary>
        public LineSegment(Line l)
        {
            this.l = l;
        }

        /// <summary>
        /// The starting point defining this line segment
        /// </summary>
        public Vector2 P0
        { get { return l.P0; } }

        /// <summary>
        /// The ending point defining this line segment
        /// </summary>
        public Vector2 P1
        { get { return l.P1; } }

        /// <summary>
        /// The vector from the starting point to the ending point of this line segment
        /// </summary>
        public Vector2 Direction
        { get { return l.Direction; } }

        /// <summary>
        /// The infinitely extended line defined by the same points as this line segment
        /// </summary>
        public Line Line
        { get { return l; } }


        /// <summary>
        /// Returns a line segment with the start and end reversed.
        /// </summary>
        public static LineSegment operator -(LineSegment seg)
        {
            return new LineSegment(-(seg.l));
        }

        /// <summary>
        /// The length of this line segment
        /// </summary>
        public double length()
        { return l.P1.distance(l.P0); }

        /// <summary>
        /// The squared length of this line segment
        /// </summary>
        public double lengthSq()
        { return l.P1.distanceSq(l.P0); }

        /// <summary>
        /// Computes the distance between a point and a line segment
        /// </summary>
        public double distance(Vector2 p)
        {
            if (UsefulFunctions.dotproduct(l.P0, l.P1, p) < 0)
                return l.P1.distance(p);

            else if (UsefulFunctions.dotproduct(l.P1, l.P0, p) < 0)
                return l.P0.distance(p);

            return l.distance(p);
        }

        public override string ToString()
        {
            return "LineSegment(" + l.P0 + ", " + l.P1 + ")";
        }
    }

}


