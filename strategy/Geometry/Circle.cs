using System;
using System.Collections.Generic;
using System.Text;

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

        public double distanceFromCenter(Vector2 p)
        {
            return UsefulFunctions.distance(p, Center);
        }
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
    }
}
