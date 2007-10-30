using System;
using System.Collections.Generic;
using System.Text;

namespace Robocup.Core
{
    [Serializable]
    /// <summary>
    /// A generic class for associating data with wheels of a robot.
    /// </summary>
    public class WheelsInfo<T>
    {
        public T lf, rf, lb, rb;
        public WheelsInfo(T lf, T rf, T lb, T rb)
        {
            this.lf = lf;
            this.lb = lb;
            this.rf = rf;
            this.rb = rb;
        }

        /// <summary>
        /// Creates a WheelInfo object with default values
        /// </summary>
        public WheelsInfo()
            : this(default(T), default(T), default(T), default(T))
        {
        }

        public override string ToString()
        {
            return "{" + lf.ToString() + " " + rf.ToString() + " " + lb.ToString() + " " + rb.ToString() + "}";
        }

        static public WheelsInfo<double> Add(WheelsInfo<double> lhs, WheelsInfo<double> rhs)
        {
            return new WheelsInfo<double>(rhs.lf + lhs.lf, rhs.rf + lhs.rf, rhs.lb + lhs.lb, rhs.rb + lhs.rb);
        }
    }
    [Serializable]
    /// <summary>
    /// A storage class for holding the four wheel speeds.
    /// 
    /// The convention for wheel speeds is that positive values contribute to the robot going forward,
    /// negative values for the robot going backwards.  (As opposed to the EE team, which has positive values going counterclockwise)
    /// </summary>
    public class WheelSpeeds : WheelsInfo<int>
    {
        public WheelSpeeds(int lf, int rf, int lb, int rb)
            : base(lf, rf, lb, rb)
        { }

        /// <summary>
        /// Creates a WheelSpeeds object with all speeds defaulting to 0.
        /// </summary>
        public WheelSpeeds()
            : this(0, 0, 0, 0)
        { }

        static public WheelSpeeds operator +(WheelSpeeds lhs, WheelSpeeds rhs)
        {
            return new WheelSpeeds(rhs.lf + lhs.lf, rhs.rf + lhs.rf, rhs.lb + lhs.lb, rhs.rb + lhs.rb);
        }

        static public WheelsInfo<double> operator *(double d, WheelSpeeds ws)
        {
            return new WheelsInfo<double>(d * ws.lf, d * ws.rf, d * ws.lb, d * ws.rb);
        }
        static public WheelsInfo<double> operator +(WheelsInfo<double> lhs, WheelSpeeds rhs)
        {
            return new WheelsInfo<double>(rhs.lf + lhs.lf, rhs.rf + lhs.rf, rhs.lb + lhs.lb, rhs.rb + lhs.rb);
        }
        static public explicit operator WheelSpeeds(WheelsInfo<double> ws)
        {
            return new WheelSpeeds((int)(ws.lf + .5), (int)(ws.rf + .5), (int)(ws.lb + .5), (int)(ws.rb + .5));
        }
        static public explicit operator WheelsInfo<double>(WheelSpeeds ws)
        {
            return new WheelsInfo<double>(ws.lf, ws.rf, ws.lb, ws.rb);
        }
    }
}
