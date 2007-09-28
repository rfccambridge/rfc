using System;
using System.Collections.Generic;
using System.Text;

namespace Robocup.Core
{
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
    }
    /// <summary>
    /// A storage class for holding the four wheel speeds.
    /// 
    /// The convention for wheel speeds is that positive values contribute to the robot going forward,
    /// negative values for the robot going backwards.  (Not clockwise vs counterclockwise)
    /// </summary>
    public class WheelSpeeds : WheelsInfo<int>
    {
        public WheelSpeeds(int lf, int rf, int lb, int rb)
            : base(lf, rf, lb , rb)
        { }

        /// <summary>
        /// Creates a WheelSpeeds object with all speeds defaulting to 0.
        /// </summary>
        public WheelSpeeds()
            : this(0, 0, 0 , 0)
        { }

        static public WheelSpeeds operator +(WheelSpeeds lhs, WheelSpeeds rhs)
        {
            return new WheelSpeeds(rhs.lf + lhs.lf, rhs.rf + lhs.rf, rhs.lb + lhs.lb, rhs.rb + lhs.rb);
        }
    }
}
