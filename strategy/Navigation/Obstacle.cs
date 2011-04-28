using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using Robocup.Geometry;

namespace Navigation
{
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
}
