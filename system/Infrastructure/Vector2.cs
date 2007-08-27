using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Robocup.Infrastructure {
    /// <summary>
    /// An immutable class that represents a point in 2D space, or a vector in 2D space.
    /// </summary>
    public class Vector2 {
        private readonly float x;

        /// <summary>
        /// The x-coordinate of this vector
        /// </summary>
        public float X {
            get { return x; }
        }
        private readonly float y;

        /// <summary>
        /// The y-coordinate of this vector
        /// </summary>
        public float Y {
            get { return y; }
        }
        /// <summary>
        /// Creates a new Vector2
        /// </summary>
        /// <param name="x">the x-coordinate</param>
        /// <param name="y">the y-coordinate</param>
        public Vector2(float x, float y) {
            this.x = x;
            this.y = y;
        }
        static public implicit operator PointF(Vector2 p) {
            return new PointF(p.X, p.Y);
        }
        static public explicit operator Vector2(Point p)
        {
            return new Vector2(p.X, p.Y);
        }

        /// <summary>
        /// Checks for equality between two Vector2's.  Since the class is
        /// immutable, does value-equality.
        /// </summary>
        public static bool operator ==(Vector2 p1, Vector2 p2) {
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
        public static bool operator !=(Vector2 p1, Vector2 p2) {
            return !(p1 == p2);
        }
        /// <summary>
        /// Checks for value equality between this and another object.  Returns
        /// false if the other object is null or not a Vector2.
        /// </summary>
        public override bool Equals(object obj) {
            Vector2 v = obj as Vector2;
            if (v == null)
                return false;
            return (this == v);
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
        public override int GetHashCode() {
            return 43 * X.GetHashCode() + 37 * Y.GetHashCode();
        }
        /// <summary>
        /// Returns the square of the length of this vector.
        /// </summary>
        public float magnitudeSq() {
            return X * X + Y * Y;
        }
        /// <summary>
        /// Returns the angle, in radians, that you must rotate the
        /// vector (1,0) in the counter-clockwise direction until
        /// it points in the same direction as this Vector2.
        /// </summary>
        public float cartesianAngle() {
            return (float)Math.Atan2(Y, X);
        }
        /// <summary>
        /// Adds two Vector2's and returns the result.  Addition is done
        /// component by component.
        /// </summary>
        static public Vector2 operator +(Vector2 p1, Vector2 p2) {
            return new Vector2(p1.X + p2.X, p1.Y + p2.Y);
        }
        /// <summary>
        /// Subtracts two Vector2's and returns the result.  Subtraction is done
        /// component by component.
        /// </summary>
        static public Vector2 operator -(Vector2 p1, Vector2 p2) {
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
        public float distanceSq(Vector2 p2) {
            if (p2 == null)
                return float.PositiveInfinity;
            //less safe but faster?:
            //return (x - p2.x) * (x - p2.x) + (y - p2.y) * (y - p2.y);
            //more safe but slower?
            return (X - p2.X) * (X - p2.X) + (Y - p2.Y) * (Y - p2.Y);
            //depends on whether or not the compiler is inlining them
        }
        /// <summary>
        /// Returns the dot product of two Vector2's
        /// </summary>
        static public float operator *(Vector2 p1, Vector2 p2) {
            return p1.X * p2.X + p1.Y * p2.Y;
        }
        /// <summary>
        /// Returns this vector scaled by a constant.
        /// </summary>
        static public Vector2 operator *(float f, Vector2 p) {
            return new Vector2(p.X * f, p.Y * f);
        }
        /// <summary>
        /// Returns a vector that is parallel to this vector and has length 1.
        /// Has no meaning for the zero vector.
        /// </summary>
        public Vector2 normalize() {
            return (float)(1 / Math.Sqrt(magnitudeSq())) * this;
        }
        /// <summary>
        /// Provides a string representation of this Vector2.
        /// </summary>
        public override string ToString() {
            return String.Format("<{0:G4},{1:G4}>", x, y);
        }
    }
}
