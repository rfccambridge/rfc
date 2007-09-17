using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;

namespace Robocup.Geometry
{
    public class Circle
    {
        private float radius;
        private Vector2 center;

        public Vector2 getCenter()
        {
            return center;
        }

        public float Radius
        {
            get { return radius; }
        }
        public float distanceFromCenter(Vector2 p)
        {
            return UsefulFunctions.distance(p, getCenter());
        }
        public Circle(Vector2 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }
    }
}
