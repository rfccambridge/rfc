using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Robocup.Core
{
    class Stats
    {
        static private Dictionary<string, double> metricMap = new Dictionary<string, double>();

        /**
         * Metrics are just key value pairs. This implementation is thread-safe.
         * The api exposed is just Incr(metricName, value) or Incr(metricName) for incrementing
         * metricName by 1.0. Similarly with Decr.
         */
        static public void Incr(string metricName, double value)
        {
            if (metricMap.ContainsKey(metricName))
            {
                lock (metricMap)
                {
                    double currentValue = metricMap[metricName];
                    metricMap.Remove(metricName);
                    double newValue = currentValue + value;
                    metricMap.Add(metricName, newValue);
                }
            }
            else
            {
                lock (metricMap)
                {
                    metricMap.Add(metricName, 1.0);
                }
            }
        }

        static public void Incr(string metricName)
        {
            Incr(metricName, 1.0);
        }

        static public void Decr(string metricName, double value)
        {
            Incr(metricName, -1.0 * value);
        }

        static public void Decr(string metricName)
        {
            Incr(metricName, -1.0);
        }

        /**
         * Gauges.
         * These are one time values. Overwrites the previous value if it exists.
         */
        static private Dictionary<string, double> gaugeMap = new Dictionary<string, double>();

        static public void SetGauge(string metricName, double value)
        {
            lock (gaugeMap)
            {
                if (gaugeMap.ContainsKey(metricName))
                    gaugeMap.Remove(metricName);

                gaugeMap.Add(metricName, value);
            }
        }
    }
}
