using System;
using System.Collections.Generic;
using System.Text;

namespace Robocup.Utilities
{
    public class GeneralPID
    {
        readonly double Ki, Kd, Kp;
        readonly double maxPower;
        double[] prevMoveErrors = new double[2];
        double lastoutput = 0;
        readonly double errorThreshold;
        public GeneralPID(double Kp, double Kd, double Ki, double maxPower, double errorThreshold)
        {
            this.Kp = Kp;
            this.Ki = Kd;
            this.Kd = Ki;
            this.maxPower = maxPower;
            this.errorThreshold = errorThreshold;
        }
        public double getNext(double error)
        {
            double en = error, en1 = prevMoveErrors[0], en2 = prevMoveErrors[1];
            prevMoveErrors[1] = prevMoveErrors[0];
            prevMoveErrors[0] = error;
            if (Math.Abs(en - en1) > errorThreshold)
            {
                prevMoveErrors[1] = 0;
                prevMoveErrors[0] = error;
                lastoutput = Kp * error;
                lastoutput = Math.Min(maxPower, Math.Abs(lastoutput)) * Math.Sign(lastoutput);
                return lastoutput;
            }
            lastoutput += (Kp + Ki + Kd) * error - (Kp + 2 * Kd) * en1 + (Kd) * en2;
            lastoutput = Math.Min(maxPower, Math.Abs(lastoutput)) * Math.Sign(lastoutput);
            return lastoutput;
        }
    }
}
