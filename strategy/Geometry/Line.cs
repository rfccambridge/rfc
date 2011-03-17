using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;

namespace Robocup.Geometry {
    public class Line {
        private Vector2[] points;

        public Line(Vector2 p1, Vector2 p2) {
            points = new Vector2[] { p1, p2 };
        }
		public Line(Vector2 p, double direction){
			Vector2 p2 = new Vector2(1.0, 0.0).rotate(direction);
			points = new Vector2[] { p, p2 };
		}
        /// <summary>
        /// Creates a unit strech inthe x-direction
        /// </summary>
        public Line()
        {
            points = new Vector2[] {
                new Vector2(0.0, 0.0),
                new Vector2(1.0, 0.0)
            };
        }
        public Vector2[] getPoints() {
            return points;
            //return new Vector2[] { points[0].getPoint(), points[1].getPoint() };
        }
        /// <summary>
        /// Returns the cross product of P2P1 x P2P3
        /// </summary>
        public double distFromLine(Vector2 p) {
            Vector2[] points = getPoints();
            double dx = points[1].X - points[0].X;
            double dy = points[1].Y - points[0].Y;
            double mag = Math.Sqrt(dx * dx + dy * dy);
            double crossp = UsefulFunctions.crossproduct(points[0], points[1], p);
            //double crossp = dx * (p.Y - points[0].Y) - dy * (p.X - points[0].X);
            double dist = crossp / mag;
            return Math.Abs(dist);
        }

        public double distFromSegment(Vector2 p) {
            Vector2[] points = getPoints();
            if (UsefulFunctions.dotproduct(points[0], points[1], p) < 0) {
                return UsefulFunctions.distance(points[1], p);
            } else if (UsefulFunctions.dotproduct(points[1], points[0], p) < 0) {
                return UsefulFunctions.distance(points[0], p);
            }
            return distFromLine(p);
        }
        public Vector2 projectionOntoLine(Vector2 p)
        {
            Vector2[] points = getPoints();
            Vector2 tangent = (points[1] - points[0]).normalize();
            double dotp = tangent * (p - points[0]);
            return points[0] + dotp * tangent;
        }
        public override string ToString() {
            Vector2[] points = getPoints();
            return "line " + points[0] + " to " + points[1];
        }
    }
}
