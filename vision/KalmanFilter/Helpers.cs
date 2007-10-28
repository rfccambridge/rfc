using System;
using System.Collections.Generic;
using System.Text;

namespace KalmanFilter {
    static class Helpers {
        public static double erfi(double z) {
            return (Math.Sqrt(Math.PI) / 2.0) * (z + Math.PI * Math.Pow(z, 3.0) / 12.0 +
                7.0 * Math.Pow(Math.PI, 2.0) * Math.Pow(z, 5.0) / 480.0 +
                127.0 * Math.Pow(Math.PI, 3.0) * Math.Pow(z, 7.0) / 40320.0 +
                4369.0 * Math.Pow(Math.PI, 4.0) * Math.Pow(z, 9.0) / 5806080.0);
        }
    }
}
