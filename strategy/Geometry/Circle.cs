using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;

namespace Robocup.Geometry
{
    public class Circle
    {
        private double radius;
        private Vector2 center;

        public Vector2 Center
        {get { return center; }}

        public double Radius
        {get { return radius; }}

        /// <summary>
        /// Creates a circle with the given center and radius
        /// </summary>
        public Circle(Vector2 center, double radius)
        {
            this.center = center;
            this.radius = radius;
        }

        /// <summary>
        /// Creates a unit circle at the origin
        /// </summary>
        public Circle()
        {
            this.center = new Vector2();
            this.radius = 1.0;
        }

        /// <summary>
        /// Returns a circle translated by the added vector
        /// </summary>
        public static Circle operator +(Circle c, Vector2 v)
        {
            return new Circle(c.center + v, c.radius);
        }

        /// <summary>
        /// Returns a circle translated by the added vector
        /// </summary>
        public static Circle operator +(Vector2 v, Circle c)
        {
            return new Circle(v + c.center, c.radius);
        }

        /// <summary>
        /// Returns a circle translated by the negative of the vector
        /// </summary>
        public static Circle operator -(Circle c, Vector2 v)
        {
            return new Circle(c.center - v, c.radius);
        }

        /// <summary>
        /// Returns a circle that is this circle rotated a given number of radians in the
        /// counterclockwise direction around p.
        /// </summary>
        public Circle rotateAroundPoint(Vector2 p, double angle)
        {
            return new Circle(center.rotateAroundPoint(p, angle), radius);
        }


        public override string ToString()
        {
            return "Circle(" + center + ", " + radius + ")";
        }

    }
}
