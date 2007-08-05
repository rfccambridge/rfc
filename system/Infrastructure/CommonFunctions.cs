using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Collections;

namespace Robocup.Infrastructure
{
    /// <summary>
    /// This class houses some functions that I found that I reused many times.
    /// </summary>
    public static class CommonFunctions
    {
        //TODO condense this
        static public Vector2 intersect(Line l1, Line l2)
        {
            return new LineLineIntersection(l1, l2).getPoint();
            //throw new ApplicationException("this method is not yet implemented");
        }
        static public Vector2 intersect(Line l, Circle c,int whichintersection)
        {
            return new LineCircleIntersection(l, c, whichintersection).getPoint();
            //throw new ApplicationException("this method is not yet implemented");
        }
        static public Vector2 intersect(Circle c1, Circle c2,int whichintersection)
        {
            return new PlayCircleCircleIntersection(c1, c2, whichintersection).getPoint();
            //throw new ApplicationException("this method is not yet implemented");
        }


    }
}
