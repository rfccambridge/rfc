using System;
using System.Collections.Generic;
using System.Text;

namespace Robocup.Simulation
{
    public class TestResults
    {
        public TestResults(int numRuns, int totalIterations, double totalMilliseconds, double closestDistance)
        {
            this.numRuns = numRuns;
            this.totalIterations = totalIterations;
            this.totalMilliseconds = totalMilliseconds;
            this.closestDistance = closestDistance;
        }
        private readonly int numRuns;
        public int NumRuns
        {
            get { return numRuns; }
        }
        private readonly int totalIterations;
        public int TotalIterations
        {
            get { return totalIterations; }
        }
        private readonly double totalMilliseconds;
        public double TotalMilliseconds
        {
            get { return totalMilliseconds; }
        }
        public double AverageMillisecondsPerRun
        {
            get { return TotalMilliseconds / NumRuns; }
        }
        public double AverageMillisecondsPerIteration
        {
            get { return TotalMilliseconds / TotalIterations; }
        }
        public double AverageIterationsPerRun
        {
            get { return TotalIterations / NumRuns; }
        }
        private readonly double closestDistance;
        public double ClosestDistanceToObstacle
        {
            get { return closestDistance; }
        }
        // for labeling purposes only
        private string navigatorName;
        public string NavigatorName
        {
            get { return navigatorName; }
            set { navigatorName = value; }
        }
        private string testFileName;
        public string TestFileName
        {
            get { return testFileName; }
            set { testFileName = value; }
        }


        public double IterationsScore(TestResults reference)
        {
            return reference.AverageIterationsPerRun / this.AverageIterationsPerRun;
        }
        public double TotalTimeScore(TestResults reference)
        {
            return reference.AverageMillisecondsPerRun / this.AverageMillisecondsPerRun;
        }
        public double IterationSpeedScore(TestResults reference)
        {
            return reference.AverageMillisecondsPerIteration / this.AverageMillisecondsPerIteration;
        }


        public string compileSingleResult(TestResults reference)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Closest distance: " + closestDistance);
            sb.AppendLine("Average time: " + AverageMillisecondsPerRun + " ms");
            sb.AppendLine("Average iterations: " + AverageIterationsPerRun);
            sb.AppendLine("ms/iteration: " + AverageMillisecondsPerIteration);
            sb.AppendLine("estimated time: " + (AverageIterationsPerRun / 200 + 5 * AverageMillisecondsPerRun / 1000) + " s");
            sb.AppendLine("Total time score: " + TotalTimeScore(reference));
            sb.AppendLine("Average iteration time score: " + IterationSpeedScore(reference));
            sb.AppendLine("Num iterations score: " + IterationsScore(reference));
            return sb.ToString();
        }



    }
}
