using System;
using System.Collections.Generic;
using System.Text;

namespace NavigationRacer
{
    class TestResults
    {
        public TestResults(int numRuns, int totalIterations, float totalMilliseconds,float closestDistance)
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
        private readonly float totalMilliseconds;
        public float TotalMilliseconds
        {
            get { return totalMilliseconds; }
        }
        public float AverageMillisecondsPerRun
        {
            get { return TotalMilliseconds / NumRuns; }
        }
        public float AverageMillisecondsPerIteration
        {
            get { return TotalMilliseconds / TotalIterations; }
        }
        public float AverageIterationsPerRun
        {
            get { return TotalIterations / NumRuns; }
        }
        private readonly float closestDistance;
        public float ClosestDistanceToObstacle
        {
            get { return closestDistance; }
        }
        // for labeling purposes only
        private string navigatorName;
        public string NavigatorName {
            get { return navigatorName; }
            set { navigatorName = value; }
        }
        private string testFileName;
        public string TestFileName {
            get { return testFileName; }
            set { testFileName = value; }
        }
	

        public float IterationsScore(TestResults reference)
        {
            return reference.AverageIterationsPerRun / this.AverageIterationsPerRun;
        }
        public float TotalTimeScore(TestResults reference)
        {
            return reference.AverageMillisecondsPerRun / this.AverageMillisecondsPerRun;
        }
        public float IterationSpeedScore(TestResults reference)
        {
            return reference.AverageMillisecondsPerIteration / this.AverageMillisecondsPerIteration;
        }
        

        public string compileSingleResult(TestResults reference)
        {
            StringBuilder sb=new StringBuilder();
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
