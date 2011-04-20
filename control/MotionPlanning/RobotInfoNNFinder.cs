using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using Robocup.Geometry;

namespace Robocup.MotionControl
{
    /// <summary>
    /// Contains a collection of RobotInfo objects, and can respond to queries about which state is "closest" to another
    /// desired state.  The main functions are: AddInfo (adds a new RobotInfo to the collection), and Closest* methods.
    /// </summary>
    class RobotInfoNNFinder
    {
        List<RobotInfo> infos = new List<RobotInfo>();

        //Distance metric:
        //
        //distance = Euclidean distance between start and end (squared), 
        //           plus magnitudeSq of start velocity - ideal velocity vector (scaled to start speed), 
        //           plus magnitudeSq of end velocity - ideal velocity vector (scaled to end speed).
        private double distance(RobotInfo start, RobotInfo end)
        {
            double startScale = (Math.Sqrt(start.Velocity.magnitudeSq() / start.Position.distanceSq(end.Position))+1)*1;
            double endScale = (Math.Sqrt(end.Velocity.magnitudeSq() / start.Position.distanceSq(end.Position))+1)*1;
            return start.Position.distanceSq(end.Position) /*+ (startScale * (end.Position.X - start.Position.X) - start.Velocity.X) * (startScale * (end.Position.X - start.Position.X) - start.Velocity.X)
                                                           + (startScale * (end.Position.Y - start.Position.Y) - start.Velocity.Y) * (startScale * (end.Position.Y - start.Position.Y) - start.Velocity.Y)
                                                           + (endScale * (end.Position.X - start.Position.X) - start.Velocity.X) * (endScale * (end.Position.X - start.Position.X) - start.Velocity.X)
                                                           + (endScale * (end.Position.Y - start.Position.Y) - start.Velocity.Y) * (endScale * (end.Position.Y - start.Position.Y) - start.Velocity.Y)
                                                           */;
        }

        private double distance(RobotInfo start, Vector2 end)
        {
            double startScale = (Math.Sqrt(start.Velocity.magnitudeSq() / start.Position.distanceSq(end))+1)*1;
            return start.Position.distanceSq(end) /*+ (startScale * (end.X - start.Position.X) - start.Velocity.X) * (startScale * (end.X - start.Position.X) - start.Velocity.X)
                                                  + (startScale * (end.Y - start.Position.Y) - start.Velocity.Y) * (startScale * (end.Y - start.Position.Y) - start.Velocity.Y)
                                                  */;
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
