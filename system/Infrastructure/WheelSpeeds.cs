using System;
using System.Collections.Generic;
using System.Text;

namespace Robocup.Infrastructure
{
    /// <summary>
    /// A simple storage class for holding the four wheel speeds.
    /// </summary>
    public class WheelSpeeds
    {
        public int lf, rf, lb, rb;
        public WheelSpeeds(int lf, int rf, int lb, int rb)
        {
            this.lf = lf;
            this.lb = lb;
            this.rf = rf;
            this.rb = rb;
        }

        public WheelSpeeds()
            : this(0, 0, 0, 0)
        {
        }

        static public WheelSpeeds operator +(WheelSpeeds lhs, WheelSpeeds rhs)
        {
            return new WheelSpeeds(rhs.lf + lhs.lf, rhs.rf + lhs.rf, rhs.lb + lhs.lb, rhs.rb + lhs.rb);
        }
    }
}
