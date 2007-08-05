using System;
using System.Collections.Generic;
using System.Text;

namespace MachineLearning
{
    /// <summary>
    /// An interface for things that try to optimize functions.
    /// </summary>
    abstract public class Optimizer<T>
    {
        /// <summary>
        /// Spawns a new thread to start optimizing.
        /// </summary>
        //abstract public void start();
        /// <summary>
        /// Starts minimizing, on this thread.
        /// </summary>
        abstract public void minimize();
        //abstract public void run();
        abstract public void stop();
        abstract public bool isReady();
        abstract public bool isRunning();
        //abstract public void setMinimizing(bool minimizing);
        /// <summary>
        /// Gets called when an optimizer finishes an iteration.
        /// </summary>
        /// <param name="done">Whether or not this optimizer has finished and is done after this.</param>
        /// <param name="best">The best candidate found so far</param>
        /// <param name="current">All of the candidates that are still active (not eliminated)</param>
        /// <param name="newRejected">All of the canditations that were considered this round but eliminated</param>
        public delegate void FinishedIterationDel(bool done, Candidate<T> best, List<Candidate<T>> current, List<Candidate<T>> newRejected);
        /// <summary>
        /// Gets called when it finishes an iteration.
        /// </summary>
        public event FinishedIterationDel IterationFinished;
        protected void iterationFinished(bool done, Candidate<T> best, List<Candidate<T>> current, List<Candidate<T>> newRejected)
        {
            if (IterationFinished != null)
                IterationFinished(done, best, current, newRejected);
        }
    }
    abstract public class DeterministicOptimizer<T> : Optimizer<T>
    {
        private ScoreFunction<T> scoringFunction;
        /// <param name="scoringFunction">the function to be minimized</param>
        public DeterministicOptimizer(ScoreFunction<T> scoringFunction)
        {
            this.scoringFunction = scoringFunction;
        }
        /// <summary>
        /// scores the arguments
        /// </summary>
        protected double score(T args)
        {
            return scoringFunction(args);
        }
    }
    public class Candidate<T>
    {
        public T args;
        public double score;
        public Candidate(T args, double score)
        {
            this.args = args;
            this.score = score;
        }
        public override string ToString()
        {
            return score + " - " + args.ToString();
        }
    }
    /// <summary>
    /// Something that scores the argument.
    /// </summary>
    public delegate double ScoreFunction<T>(T args);

    /// <summary>
    /// A simple struct for storing both the estimated mean
    /// and the estimated standard deviation.
    /// </summary>
    public struct StochasticAnswer
    {
        public double score;
        public double stdDev;

        public StochasticAnswer(double score, double stdDev)
        {
            this.score = score;
            this.stdDev = stdDev;
        }
    }
    /// <summary>
    /// Something that scores, but with randomness.
    /// </summary>
    public delegate StochasticAnswer StochasticScoringFunction<T>(T args);

    public class StochasticCandidate<T> : Candidate<T>
    {
        public double stdDev;
        public StochasticCandidate(T args, double score, double stdDev)
            : base(args, score)
        {
            this.stdDev = stdDev;
        }
        public override string ToString()
        {
            return score + ", " + stdDev + " - " + args.ToString();
        }
    }
}
