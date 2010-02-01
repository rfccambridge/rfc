using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Core;

namespace Robocup.CoreRobotics
{
    static public class WheelSpeedsExtender
    {

        /// <summary>
        /// Get wheel speeds to go from one RobotInfo point to drive through another 
        /// RobotInfo point
        /// </summary>
        /// <param name="start"></param>
        /// <param name="goal"></param>
        /// <returns></returns>
        static public WheelSpeeds GetWheelSpeedsThrough(RobotInfo start, RobotInfo goal)
        {
            return GetWheelSpeedsThrough(start, goal.Position);
        }

        /// <summary>
        /// Get wheel speeds to go from one RobotInfo point to drive through a Vector2
        /// </summary>
        /// <param name="start"></param>
        /// <param name="goal"></param>
        /// <returns></returns>
        static public WheelSpeeds GetWheelSpeedsThrough(RobotInfo start, Vector2 goal)
        {
            // we have orientation and speed information 
            // TODO orientation updating

            // break down wheelspeeds based on desired vector
            Vector2 desiredDirection = (goal - start.Position);
            if (desiredDirection.magnitudeSq() < .001 * .001)
                return new WheelSpeeds();


            Vector2 lf = new Vector2(0.71, -0.71).rotate(start.Orientation);
            Vector2 rf = new Vector2(0.71, 0.71).rotate(start.Orientation);
            Vector2 lb = new Vector2(0.74, 0.68).rotate(start.Orientation);
            Vector2 rb = new Vector2(0.74, -0.68).rotate(start.Orientation);
            double plf = lf * desiredDirection;
            double prf = rf * desiredDirection;
            double plb = lb * desiredDirection;
            double prb = rb * desiredDirection;
            double max = Math.Max(Math.Max(Math.Abs(plf), Math.Abs(prf)), Math.Max(Math.Abs(plb), Math.Abs(prb)));
            //max = Math.Max(.10, max);

            // compute magnitude of wheelspeeds based on PD
            double speed = Constants.get<double>("control", "MOVE_PID_MAX") ;// Math.Min(30.0, Math.Sqrt(desiredDirection.magnitudeSq()) * 3000.0);
            //speed -= Math.Sqrt((goal.Velocity - start.Velocity).magnitudeSq());

            //Console.WriteLine("doing speed1: " + speed + " lf: " + (speed * plf / max) + " rf: " + (speed * prf / max) + " lb: " + (speed * plb / max) + " rb: " + (speed * prb / max));
            return new WheelSpeeds((int)(speed * prf / max), (int)(speed * plf / max), (int)(speed * plb / max), (int)(speed * prb / max));
            //return GetWheelSpeeds(start, goal.Position);
        }

        static public WheelSpeeds GetWheelSpeedsTo(RobotInfo start, RobotInfo goal)
        {
            double vprop = Math.Sqrt(start.Velocity.distanceSq(goal.Velocity))/start.Position.distanceSq(goal.Position);
            double dprop = Constants.get<double>("control", "MOVE_DTERM_PROP");
            double iprop = goal.Velocity.magnitudeSq() / 10;
                Vector2 lf = new Vector2(0.71, -0.71).rotate(start.Orientation);
                Vector2 rf = new Vector2(0.71, 0.71).rotate(start.Orientation);
                Vector2 lb = new Vector2(0.74, 0.68).rotate(start.Orientation);
                Vector2 rb = new Vector2(0.74, -0.68).rotate(start.Orientation);
            WheelSpeeds dwheels, vwheels;
            {
                Vector2 deltaV = (goal.Velocity - start.Velocity);
                double plf = lf * deltaV;
                double prf = rf * deltaV;
                double plb = lb * deltaV;
                double prb = rb * deltaV;
                double max = Math.Max(Math.Max(Math.Abs(plf), Math.Abs(prf)), Math.Max(Math.Abs(plb), Math.Abs(prb)));

                // compute magnitude of wheelspeeds based on PD
                double speed = Math.Min(Constants.get<double>("control", "MOVE_VTERM_MAX"),
                    Math.Sqrt(deltaV.magnitudeSq()) * Constants.get<double>("control", "MOVE_VTERM_GAIN"));

                vwheels = new WheelSpeeds((int)(speed * prf / max), (int)(speed * plf / max), (int)(speed * plb / max), (int)(speed * prb / max));
            }
            {
                // break down wheelspeeds based on desired vector
                Vector2 desiredDirection = (goal.Position - start.Position);
                if (desiredDirection.magnitudeSq() < .001 * .001)
                    return new WheelSpeeds();
                //Console.WriteLine("going to: " + goal.Position + " " + goal.Orientation + " from: " + start.Position + " " + start.Orientation);

                double plf = lf * desiredDirection;
                double prf = rf * desiredDirection;
                double plb = lb * desiredDirection;
                double prb = rb * desiredDirection;
                double max = Math.Max(Math.Max(Math.Abs(plf), Math.Abs(prf)), Math.Max(Math.Abs(plb), Math.Abs(prb)));

                // compute magnitude of wheelspeeds based on PD
                double speed = Math.Min(Constants.get<double>("control", "MOVE_PTERM_MAX"),
                    Math.Sqrt(desiredDirection.magnitudeSq()) * Constants.get<double>("control", "MOVE_PTERM_GAIN"));

                dwheels = new WheelSpeeds((int)(speed * prf / max), (int)(speed * plf / max), (int)(speed * plb / max), (int)(speed * prb / max));
            }
            WheelSpeeds toRet = (WheelSpeeds)WheelsInfo<double>.Times(1/(dprop + vprop),(WheelsInfo<double>.Add(dprop * dwheels,vprop * vwheels)));
            Console.WriteLine("sending speeds: " + toRet.ToString());
            return toRet;
        }
        //static public WheelSpeeds GetWheelSpeedsTo(RobotInfo start, Vector2 end)
        //{
        //}

        //static public WheelSpeeds GetWheelSpeeds(RobotInfo start, RobotInfo goal)
        //{
        //}
        //static public WheelSpeeds GetWheelSpeeds(RobotInfo start, Vector2 goal)
        //{
        //}
        /*static public WheelSpeeds GetWheelSpeeds(RobotInfo start, Vector2 goal)
        {
            Vector2 desiredDirection = goal - start.Position;

            //try to cancel out any side-to-side velocity, to avoid circling the target
            double dotp = (desiredDirection * start.Velocity) / Math.Sqrt(desiredDirection.magnitudeSq() * start.Velocity.magnitudeSq());
            //theoretically this should always be true, but if the vectors are very small it might not be
            if (Math.Abs(dotp) < 1)
            {
                Vector2 orth = start.Velocity - dotp * desiredDirection;
                desiredDirection -= .05 * orth;
            }

            Vector2 lf = new Vector2(1, -1).rotate(start.Orientation);
            Vector2 rf = new Vector2(1, 1).rotate(start.Orientation);
            Vector2 lb = new Vector2(1, 1).rotate(start.Orientation);
            Vector2 rb = new Vector2(1, -1).rotate(start.Orientation);
            double plf = lf * desiredDirection;
            double prf = rf * desiredDirection;
            double plb = lb * desiredDirection;
            double prb = rb * desiredDirection;
            double max = Math.Max(Math.Max(Math.Abs(plf), Math.Abs(prf)), Math.Max(Math.Abs(plb), Math.Abs(prb)));
            max = Math.Max(.01, max);
            return new WheelSpeeds((int)(127 * plf / max), (int)(127 * prf / max), (int)(127 * plb / max), (int)(127 * prb / max));
        }*/
    }
}
