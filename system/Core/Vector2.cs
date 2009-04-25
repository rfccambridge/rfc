using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Robocup.Core
{
    /// <summary>
    /// An immutable class that represents a point in 2D space, or a vector in 2D space.
    /// </summary>
    [Serializable]
    public class Vector2
    {
        private readonly double x;

        /// <summary>
        /// The x-coordinate of this vector
        /// </summary>
        public double X
        {
            get { return x; }
        }
        private readonly double y;

        /// <summary>
        /// The y-coordinate of this vector
        /// </summary>
        public double Y
        {
            get { return y; }
        }
        /// <summary>
        /// Creates a zero Vector2
        /// </summary>
        public Vector2() : this(0, 0) { }
        /// <summary>
        /// Creates a new Vector2
        /// </summary>
        /// <param name="x">the x-coordinate</param>
        /// <param name="y">the y-coordinate</param>
        public Vector2(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
        /// <summary>
        /// Constructs a vector that is a copy of another one
        /// </summary>
        /// <param name="copy">Original object</param>
        public Vector2(Vector2 orig) : this(orig.X, orig.Y) { }        
        /*static public implicit operator PointF(Vector2 p)
        {
            return new PointF((float)p.X, (float)p.Y);
        }*/
        public PointF ToPointF()
        {
            return new PointF((float)X, (float)Y);
        }
        static public explicit operator Vector2(Point p)
        {
            return new Vector2(p.X, p.Y);
        }

        static private Vector2 zero = new Vector2(0, 0);
        /// <summary>
        /// The Vector (0,0)
        /// </summary>
        static public Vector2 ZERO
        {
            get { return zero; }
        }

        static public Vector2 GetUnitVector(double orientation)
        {
            return new Vector2(Math.Cos(orientation), Math.Sin(orientation));
        }


        /// <summary>
        /// Checks for equality between two Vector2's.  Since the class is
        /// immutable, does value-equality.
        /// </summary>
        public static bool operator ==(Vector2 p1, Vector2 p2)
        {
            if (object.ReferenceEquals(p1, null))
                return (object.ReferenceEquals(p2, null));
            else if (object.ReferenceEquals(p2, null))
                return (object.ReferenceEquals(p1, null));
            return (p1.X == p2.X && p1.Y == p2.Y);
        }
        /// <summary>
        /// Checks for inequality between two Vector2's.  Since the class is
        /// immutable, does value-equality.
        /// </summary>
        public static bool operator !=(Vector2 p1, Vector2 p2)
        {
            return !(p1 == p2);
        }
        /// <summary>
        /// Checks for value equality between this and another object.  Returns
        /// false if the other object is null or not a Vector2.
        /// </summary>
        public override bool Equals(object obj)
        {
            Vector2 v = obj as Vector2;
            if (v == null)
                return false;
            return (X == v.X && Y == v.Y);
        }
        /// <summary>
        /// Checks for value equality between this and another object.  Returns
        /// false if the other object is null.
        /// </summary>
        public bool Equals(Vector2 obj)
        {
            return this == obj;
        }
        /// <summary>
        /// Returns a hash code of this Vector2.
        /// </summary>
        public override int GetHashCode()
        {
            return 43 * X.GetHashCode() + 37 * Y.GetHashCode();
        }
        /// <summary>
        /// Returns the square of the length of this vector.
        /// </summary>
        public double magnitudeSq()
        {
            return X * X + Y * Y;
        }
        /// <summary>
        /// Returns the angle, in radians, that you must rotate the
        /// vector (1,0) in the counter-clockwise direction until
        /// it points in the same direction as this Vector2.
        /// </summary>
        public double cartesianAngle()
        {
            return Math.Atan2(Y, X);
        }
        /// <summary>
        /// Adds two Vector2's and returns the result.  Addition is done
        /// component by component.
        /// </summary>
        static public Vector2 operator +(Vector2 p1, Vector2 p2)
        {
            return new Vector2(p1.X + p2.X, p1.Y + p2.Y);
        }
        /// <summary>
        /// Subtracts two Vector2's and returns the result.  Subtraction is done
        /// component by component.
        /// </summary>
        static public Vector2 operator -(Vector2 p1, Vector2 p2)
        {
            return new Vector2(p1.X - p2.X, p1.Y - p2.Y);
        }
        /// <summary>
        /// Returns the negation of this vector.
        /// </summary>
        static public Vector2 operator -(Vector2 p)
        {
            return new Vector2(-p.X, -p.Y);
        }
        /// <summary>
        /// Returns the distance between this point and another point.
        /// Returns the same value (within tolerance) as (p1-p2).magnitudeSq()
        /// </summary>
        public double distanceSq(Vector2 p2)
        {
            if (p2 == null)
                return double.PositiveInfinity;
            //less safe but faster?:
            //return (x - p2.x) * (x - p2.x) + (y - p2.y) * (y - p2.y);
            //more safe but slower?
            return (X - p2.X) * (X - p2.X) + (Y - p2.Y) * (Y - p2.Y);
            //depends on whether or not the compiler is inlining them
        }
        /// <summary>
        /// Returns the dot product of two Vector2's
        /// </summary>
        static public double operator *(Vector2 p1, Vector2 p2)
        {
            return p1.X * p2.X + p1.Y * p2.Y;
        }
        /// <summary>
        /// Returns this vector scaled by a constant.
        /// </summary>
        static public Vector2 operator *(double f, Vector2 p)
        {
            return new Vector2(p.X * f, p.Y * f);
        }
        /// <summary>
        /// Returns this vector divided by a constant.
        /// </summary>
        static public Vector2 operator /(Vector2 p, double f)
        {
            return new Vector2(p.X / f, p.Y / f);
        }

        /// <summary>
        /// Returns a vector that is parallel to this vector and has length 1.
        /// Has no meaning for the zero vector (will return NaN).
        /// </summary>
        public Vector2 normalize()
        {
            double lengthSq = magnitudeSq();
            if (lengthSq == 0)
                return this;
            
            return (1 / Math.Sqrt(magnitudeSq())) * this;
        }
        /// <summary>
        /// Returns a vector in the same direction as this one, with the desired length.
        /// Returns NaN for the zero vector.
        /// </summary>
        public Vector2 normalizeToLength(double newLength)
        {
            return newLength * (this.normalize());
        }
        /// <summary>
        /// Returns a vector that is this vector rotated a given number of radians in the
        /// counterclockwise direction.
        /// </summary>
        public Vector2 rotate(double angle)
        {
            double c = Math.Cos(angle);
            double s = Math.Sin(angle);
            return new Vector2(c * X - s * y, c * Y + s * x);
        }
        /// <summary>
        /// Provides a string representation of this Vector2.
        /// </summary>
        public override string ToString()
        {
            return String.Format("<{0:G4},{1:G4}>", x, y);
        }

        /// <summary>
        /// Parses a Vector2 from the string format of ToString().  There is not much guarantee about
        /// how constant the string representation will be, however.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        static public Vector2 Parse(string s)
        {
            string[] split = s.Trim('<', '>', ' ').Split(',');
            if (split.Length != 2)
                throw new FormatException("invalid format for Vector2");
            return new Vector2(double.Parse(split[0]), double.Parse(split[1]));
        }
    }
}
