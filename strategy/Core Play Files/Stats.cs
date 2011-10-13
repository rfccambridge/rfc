using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robocup.Plays;
using Robocup.Core;
using System.Diagnostics;

namespace Robocup.CorePlayFiles
{
    class Stats
    {
        static private Dictionary<string, double> metricMap = new Dictionary<string, double>();

        /* Which was the last team that had possession? Never null. */
        static private Nullable<Team> lastValidPossession;
        
        /* What was the last state of the ball? Could be null. */
        static private Nullable<Team> lastPossession;

        static public void ComputeStats(EvaluatorState state)
        {
            computePossessionStats(state);
        }

        static private void computePossessionStats(EvaluatorState state)
        {
            Nullable<Team> possession = computeCurrentPossession(state.OurTeamInfo, state.TheirTeamInfo, state.ballInfo);
            Debug.Assert(lastValidPossession != null);
 
            if (possession == Team.Blue)
            {
                /* Check to see if Blue intercepted the ball. */
                if (lastValidPossession == Team.Yellow)
                {
                    Incr("blue-interceptions");
                }
                else
                {
                    /* Was there a pass? */
                    if (lastPossession == null)
                        Incr("blue-passes");
                }
                Incr("blue-possession");
            }
            else if (possession == Team.Yellow)
            {
                /* Check to see if Yellow intercepted the ball. */
                if (lastValidPossession == Team.Blue)
                {
                    Incr("yellow-interceptions");
                }
                else
                {
                    /* Was there a pass? */
                    if (lastPossession == null)
                        Incr("yellow-passes");
                }
                Incr("yellow-possession");
            }

            if (possession != null)
                lastValidPossession = possession;

            lastPossession = possession;
        }

        static private Nullable<Team> computeCurrentPossession(InterpreterRobotInfo[] ourteaminfo, InterpreterRobotInfo[] theirteaminfo, BallInfo ballinfo)
        {
            return null;
        }
        
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
