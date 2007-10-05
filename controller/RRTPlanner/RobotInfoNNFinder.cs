using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;

namespace Robocup.RRT
{
    class RobotInfoNNFinder : NearestNeighborFinder<RobotInfo>
    {
        List<RobotInfo> infos = new List<RobotInfo>();

        private double distance(RobotInfo r1, RobotInfo r2)
        {
            return r1.Position.distanceSq(r2.Position);
        }

        public void AddPoint(RobotInfo point)
        {
            infos.Add(point);
        }

        public RobotInfo NearestNeighbor(RobotInfo point)
        {
            //This is a naive brute-force search.
            double mindist = double.MaxValue;
            RobotInfo best = null;
            foreach (RobotInfo info in infos)
            {
                double d = distance(point, info);
                if (d < mindist)
                {
                    mindist = d;
                    best = info;
                }
            }
            return best;
        }
    }
}
