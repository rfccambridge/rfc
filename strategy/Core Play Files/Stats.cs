using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robocup.Plays;
using Robocup.Core;
using System.Diagnostics;
using Robocup.Geometry;
using System.Threading;

namespace Robocup.CorePlayFiles
{
    public class Stats
    {
        static Queue<EvaluatorState> statsQueue = new Queue<EvaluatorState>();

        static private Dictionary<string, double> metricMap = new Dictionary<string, double>();

        /* Which was the last team that had possession? Never null. */
        static private Nullable<Team> lastValidPossession = Team.Blue;
        
        /* What was the last state of the ball? Could be null. */
        static private Nullable<Team> lastPossession;

        static public void RunStatsComputer() {
            int backOff = 0;
            while (true)
            {
                if (statsQueue.Count == 0)
                {
                    Thread.Sleep(backOff);
                    backOff = backOff + 1;
                }
                else
                {
                    backOff = backOff / 2;
                    EvaluatorState state = statsQueue.Dequeue();
                    computePossessionStats(state);
                }
            }
        }

        static int counter = 0;
        /**
         * Main publicly available API call.
         * Takes an EvaluatorState and computes all relevant stats.
         */
        static public void ComputeStats(EvaluatorState state)
        {
            statsQueue.Enqueue(state);
            counter = counter + 1;
            if (counter % 100 == 0)
                PrintStats();
        }

        static private void computePossessionStats(EvaluatorState state)
        {
            if ((state.OurTeamInfo.Length == 0) || (state.TheirTeamInfo.Length == 0))
            {
                Stats.Incr("insufficient-data-to-calculate-stats");
                return;
            }

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
            else if (possession == null)
            {
                Stats.Incr("limbo");
            }

            if (possession != null)
                lastValidPossession = possession;

            lastPossession = possession;
        }

        static private RobotInfo getClosestRobot(InterpreterRobotInfo[] teamInfo, Vector2 ballPosition)
        {
            double minDistance = Double.PositiveInfinity;
            RobotInfo closestRobot = teamInfo[0];

            for (int i = 0; i < teamInfo.Length; i++)
            {
                InterpreterRobotInfo robot = teamInfo[i];
                double distance = robot.Position.distance(ballPosition);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestRobot = robot;
                }
            }

            return closestRobot;
        }

        static private Nullable<Team> computeCurrentPossession(InterpreterRobotInfo[] ourTeamInfo, InterpreterRobotInfo[] theirTeamInfo, BallInfo ballinfo)
        {
            Vector2 ballPosition = ballinfo.Position;
            RobotInfo closestRobot;
            RobotInfo ourClosestRobot = getClosestRobot(ourTeamInfo, ballPosition);
            RobotInfo theirClosestRobot = getClosestRobot(theirTeamInfo, ballPosition);
            if (ourClosestRobot.Position.distance(ballPosition) > theirClosestRobot.Position.distance(ballPosition))
                closestRobot = ourClosestRobot;
            else
                closestRobot = theirClosestRobot;
            if (closestRobot.Position.distance(ballPosition) > (Constants.Basic.ROBOT_RADIUS + 0.10))
                return null;
            return closestRobot.Team;
        }
        
        /**
         * Metrics are just key value pairs. This implementation is thread-safe.
         * The api exposed is just Incr(metricName, value) or Incr(metricName) for incrementing
         * metricName by 1.0. Similarly with Decr.
         */
        static public void Incr(string metricName, double value)
        {
            lock (metricMap)
            {
                if (metricMap.ContainsKey(metricName))
                    metricMap[metricName] += value;
                else
                    metricMap.Add(metricName, 1.0);

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

        static public void PrintStats()
        {
            Console.WriteLine("====== STATS ======");
            foreach(KeyValuePair<String,Double> entry in metricMap) 
            {
                Console.WriteLine(entry.Key + ": " + entry.Value);
            }
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
