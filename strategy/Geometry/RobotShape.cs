using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Robocup.Geometry
{
    /// <summary>
    /// A robot shape. Circular arc, with a flat front part.
    /// </summary>
    public class RobotShape : MultiGeom
    {
        private const int ARC_NUM = 0;
        private const int SEG_NUM = 1;

        public Arc Arc
        { get { return (Arc)geoms[ARC_NUM]; } }

        public LineSegment Segment
        { get { return (LineSegment)geoms[SEG_NUM]; } }

        private RobotShape(Arc arc, LineSegment segment)
        {
            this.geoms = new Geom[2];
            this.geoms[ARC_NUM] = arc;
            this.geoms[SEG_NUM] = segment;
        }

        /// <summary>
        /// Creates a robot shape. 
        /// </summary>
        public RobotShape(Vector2 center, double radius, double orientation, double frontPlateRadius)
        {
            double angle = Math.Acos(frontPlateRadius / radius);

            this.geoms = new Geom[2];
            this.geoms[ARC_NUM] = new Arc(center, radius, orientation - angle, orientation + angle);
            this.geoms[SEG_NUM] = new LineSegment(Arc.StartPt, Arc.StopPt);
        }

        /// <summary>
        /// Returns a robot shape translated by the added vector
        /// </summary>
        public static RobotShape operator +(RobotShape rs, Vector2 v)
        {
            return (RobotShape)((MultiGeom)rs + v);
        }

        /// <summary>
        /// Returns a line segment translated by the added vector
        /// </summary>
        public static RobotShape operator +(Vector2 v, RobotShape rs)
        {
            return (RobotShape)(v + (MultiGeom)rs);
        }

        /// <summary>
        /// Returns a line segment translated by the negative of the vector
        /// </summary>
        public static RobotShape operator -(RobotShape rs, Vector2 v)
        {
            return (RobotShape)((MultiGeom)rs - v);
        }

        /// <summary>
        /// Returns the translation of this line segment by the given vector.
        /// </summary>
        new public RobotShape translate(Vector2 v)
        {
            return this + v;
        }

        /// <summary>
        /// Returns a line segment that is this line rotated a given number of radians in the
        /// counterclockwise direction around p.
        /// </summary>
        new public RobotShape rotateAroundPoint(Vector2 p, double angle)
        {
            return (RobotShape)(((MultiGeom)this).rotateAroundPoint(p, angle));
        }


        public bool contains(Vector2 p)
        {
            return Arc.Center.distanceSq(p) <= Arc.Radius * Arc.Radius && Segment.Line.signedDistance(p) <= 0;
        }

        public override string ToString()
        {
            return "RobotShape[" + geoms[ARC_NUM].ToString() + ", " + geoms[SEG_NUM].ToString() + "]";
        }
    }
}
