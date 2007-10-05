using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;

namespace Robocup.RRT
{
    /// <summary>
    /// Contains a collection of RobotInfo objects, and can respond to queries about which state is "closest" to another
    /// desired state.  The main functions are: AddInfo (adds a new RobotInfo to the collection), and Closest* methods.
    /// </summary>
    class RobotInfoNNFinder
    {
        List<RobotInfo> infos = new List<RobotInfo>();

        private double distance(RobotInfo start, RobotInfo end)
        {
            return start.Position.distanceSq(end.Position);
        }

        private double distance(RobotInfo start, Vector2 end)
        {
            return start.Position.distanceSq(end);
        }

        public void AddInfo(RobotInfo info)
        {
            infos.Add(info);
        }

        public RobotInfo ClosestGoingFrom(RobotInfo point)
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

        public RobotInfo ClosestGoingTo(RobotInfo point)
        {
            //This is a naive brute-force search.
            double mindist = double.MaxValue;
            RobotInfo best = null;
            foreach (RobotInfo info in infos)
            {
                double d = distance(info, point);
                if (d < mindist)
                {
                    mindist = d;
                    best = info;
                }
            }
            return best;
        }

        public RobotInfo ClosestGoingTo(Vector2 point)
        {
            //This is a naive brute-force search.
            double mindist = double.MaxValue;
            RobotInfo best = null;
            foreach (RobotInfo info in infos)
            {
                double d = distance(info, point);
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
