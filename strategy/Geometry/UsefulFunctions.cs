using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;

namespace Robocup.Geometry
{
    public static class UsefulFunctions
    {

        static public double distance(Vector2 p1, Vector2 p2)
        {
            return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
        }
        static public double distancesq(Vector2 p1, Vector2 p2)
        {
            return (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y);
        }
        /// <summary>
        /// Returns the z-component of the crossproduct P2P1 x P2P3
        /// </summary>
        static public double crossproduct(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.X - p2.X) * (p3.Y - p2.Y) - (p3.X - p2.X) * (p1.Y - p2.Y);
        }
        /// <summary>
        /// Returns the dotproduct of P2P1 and P2P3
        /// </summary>
        static public double dotproduct(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.X - p2.X) * (p3.X - p2.X) + (p1.Y - p2.Y) * (p3.Y - p2.Y);
        }

        /// <summary>
        /// Returns how many radians counter-clockwise the ray defined by angle1
        /// needs to be rotated to point in the direction angle2.
        /// Uses
        /// Returns a value in the range [-Pi,Pi)
        /// </summary>
        static public double angleDifference(double angle1, double angle2)
        {
            //first get the inputs in the range [0, 2Pi):
            while (angle1 < 0)
                angle1 += Math.PI * 2;
            while (angle2 < 0)
                angle2 += Math.PI * 2;
            angle1 %= Math.PI * 2;
            angle2 %= Math.PI * 2;

            double anglediff = angle2 - angle1;
            anglediff = (anglediff + Math.PI * 2) % (Math.PI * 2);
            //anglediff is now in the range [0,Pi*2)

            //now we need to get the range to [-Pi, Pi):
            if (anglediff < Math.PI)
                return anglediff;
            else
                return anglediff - Math.PI * 2;
        }

        public static Vector2 extend(Vector2 p1, Vector2 p2, double distance)
        {
            return p2 + (p2 - p1).normalizeToLength(distance);
        }
    }


}
