using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Robocup.Geometry
{
    /// <summary>
    /// A geometry object that is simply a composite of several others.
    /// </summary>
    public class MultiGeom : Geom
    {
        protected Geom[] geoms;

        public Geom this[int i]
        { get { return geoms[i]; } }

        public int Qty
        { get { return geoms.Length; } }

        protected MultiGeom()
        {
            this.geoms = null;
        }

        public MultiGeom(Geom[] geoms)
        {
            this.geoms = geoms;
        }

        /// <summary>
        /// Returns a MultiGeom translated by the added vector
        /// </summary>
        public static MultiGeom operator +(MultiGeom g, Vector2 v)
        {
            Geom[] newGeoms = new Geom[g.geoms.Length];
            for (int i = 0; i < g.geoms.Length; i++)
                newGeoms[i] = g.geoms[i].translate(v);
            return new MultiGeom(newGeoms);
        }

        /// <summary>
        /// Returns a MultiGeom translated by the added vector
        /// </summary>
        public static MultiGeom operator +(Vector2 v, MultiGeom g)
        {
            Geom[] newGeoms = new Geom[g.geoms.Length];
            for (int i = 0; i < g.geoms.Length; i++)
                newGeoms[i] = g.geoms[i].translate(v);
            return new MultiGeom(newGeoms);
        }

        /// <summary>
        /// Returns a MultiGeom translated by the negative of the vector
        /// </summary>
        public static MultiGeom operator -(MultiGeom g, Vector2 v)
        {
            Geom[] newGeoms = new Geom[g.geoms.Length];
            for (int i = 0; i < g.geoms.Length; i++)
                newGeoms[i] = g.geoms[i].translate(-v);
            return new MultiGeom(newGeoms);
        }

        /// <summary>
        /// Returns the translation of this MultiGeom by the given vector.
        /// </summary>
        public MultiGeom translate(Vector2 v)
        {
            return this + v;
        }
        Geom Geom.translate(Vector2 v)
        { return translate(v); }

        /// <summary>
        /// Returns a MultiGeom that is this line rotated a given number of radians in the
        /// counterclockwise direction around p.
        /// </summary>
        public MultiGeom rotateAroundPoint(Vector2 p, double angle)
        {
            Geom[] newGeoms = new Geom[geoms.Length];
            for (int i = 0; i < geoms.Length; i++)
                newGeoms[i] = geoms[i].rotateAroundPoint(p,angle);
            return new MultiGeom(newGeoms);
        }
        Geom Geom.rotateAroundPoint(Vector2 p, double angle)
        { return rotateAroundPoint(p, angle); }


        public override string ToString()
        {
            string s = "MultiGeom[";
            for (int i = 0; i < geoms.Length; i++)
                s += (i < geoms.Length - 1 ? geoms[i].ToString() + "," : geoms[i].ToString());
            s += "]";
            return s;
        }
    }
}
