using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;

namespace Robocup.RRT
{
    class Vector2NNFinder
    {
        private List<Vector2> points = new List<Vector2>();

        public void AddPoint(Vector2 point)
        {
            points.Add(point);
        }

        public Vector2 NearestNeighbor(Vector2 point)
        {
            //This is a naive brute-force search.
            double mindist = double.MaxValue;
            Vector2 best = null;
            foreach (Vector2 v in points)
            {
                double d = point.distanceSq(v);
                if (d < mindist)
                {
                    mindist = d;
                    best = v;
                }
            }
            return best;
        }
    }
}
