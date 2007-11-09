using System;
using System.Collections.Generic;
using System.Text;

using System.Threading;

namespace MachineLearning
{
    /// <summary>
    /// A function to generate the next guess from a previous guess.  Should not
    /// return the same value on multiple calls with the same arguments.
    /// </summary>
    /// <param name="previous">The previous guess</param>
    /// <param name="temperature">The "temperature" of the system</param>
    public delegate T GenerateNextArgs<T>(T previous, double temperature);
    /// <summary>
    /// A function that returns true when the optimization should end.
    /// </summary>
    public delegate bool SingleTerminationFunction<T>(T current, double score);

    public class SimulatedAnnealing<T> : DeterministicOptimizer<T>
    {
        public SimulatedAnnealing(ScoreFunction<T> scoreFunction) : base(scoreFunction) { }

        #region algorithm parameters
        private bool verbose = false;
        private double coolingFactor = .999;
        private GenerateNextArgs<T> generationFunction;
        private SingleTerminationFunction<T> termFunction;
        #endregion

        #region Setting Parameters
        /// <summary>
        /// Sets whether or not this optimizer will output verbose output or not.
        /// (Verbose is multiple lines per iteration, non-verbose is less
        /// than one line per iteration.)
        /// </summary>
        public void setVerbose(bool verbose)
        {
            this.verbose = verbose;
        }
        public void setTemp(double temp)
        {
            this.curTemp = temp;
        }
        public void setCoolingFactor(double coolingFactor)
        {
            this.coolingFactor = coolingFactor;
        }
        public void setCurrent(T current)
        {
            this.current = current;
            needEvaluateCurrent = true;
        }
        public void setGenFunction(GenerateNextArgs<T> genFunction)
        {
            this.generationFunction = genFunction;
        }
        public void setTermFunction(SingleTerminationFunction<T> termFunction)
        {
            this.termFunction = termFunction;
        }
        public override bool isReady()
        {
            return (
                generationFunction != null &&
                termFunction != null
            );
        }
        #endregion

        #region current data
        Random r = new Random();
        T best;
        double bestScore;
        bool noBest = true;
        bool needEvaluateCurrent = false;
        T current;
        double currentScore;
        double curTemp;

        volatile bool running = false;
        volatile bool stillRun = false;
        #endregion

        private int numthreads = 1;

        public int NumThreads
        {
            get { return numthreads; }
            set { numthreads = value; }
        }


        public T getBest()
        {
            return best;
        }
        public void clearBest()
        {
            best = default(T);
            noBest = true;
        }
        /*public void start()
        {
            stillRun = true;
            new System.Threading.Thread(minimize).Start();
        }
        public override void run()
        {
            stillRun = true;
            minimize();
        }*/
        public override void stop()
        {
            stillRun = false;
        }
        public override bool isRunning()
        {
            return running;
        }

        private object best_lock = new object();
        List<Thread> workers = new List<Thread>();
        AutoResetEvent waithandle = new AutoResetEvent(false);

        public override void minimize()
        {
            running = true;
            stillRun = true;

            if (needEvaluateCurrent)
            {
                if (verbose)
                    Console.WriteLine("evaluating the starting candidate");
                currentScore = score(current);
                if (verbose)
                {
                    Console.WriteLine("starting score: " + currentScore);
                    Console.WriteLine();
                }
                if (noBest || currentScore < bestScore)
                {
                    best = current;
                    bestScore = currentScore;
                    noBest = false;
                }
                needEvaluateCurrent = false;
            }

            {
                Candidate<T> currentCand = new Candidate<T>(current, currentScore);
                Candidate<T> bestCand = new Candidate<T>(best, bestScore);
                List<Candidate<T>> currentCandList = new List<Candidate<T>>();
                currentCandList.Add(currentCand);
                List<Candidate<T>> rejectedList = new List<Candidate<T>>();
                iterationFinished(!stillRun, bestCand, currentCandList, rejectedList);
            }

            for (int i = 0; i < numthreads; i++)
            {
                Thread t = new Thread(Run);
                t.Start();
            }
            //Run();
            waithandle.WaitOne();

            running = false;
        }

        private void Run()
        {
            while (stillRun)
            {
                if (verbose)
                {
                    Console.WriteLine("temp = " + curTemp);
                    Console.WriteLine("current score: " + currentScore);
                }
                T next = generationFunction(current, curTemp);
                double nextScore = score(next);
                if (verbose)
                    Console.WriteLine("next score: " + nextScore);

                lock (best_lock)
                {
                    double prob = 1;
                    if (nextScore > currentScore)
                    {
                        prob = Math.Exp((currentScore - nextScore) / (curTemp));
                        if (verbose)
                            Console.WriteLine("probability: " + prob);
                    }

                    if (r.NextDouble() < prob || noBest)
                    {
                        if (nextScore < bestScore || noBest)
                        {
                            best = next;
                            bestScore = nextScore;
                            noBest = false;
                        }
                        current = next;
                        currentScore = nextScore;
                        if (verbose)
                            Console.WriteLine("accepted");
                    }
                    else
                    {
                        if (verbose)
                            Console.WriteLine("rejected");
                    }
                    if (verbose)
                        Console.WriteLine();

                    curTemp *= 1 - coolingFactor;

                    if (termFunction(current, currentScore))
                        stillRun = false;

                    Candidate<T> currentCand = new Candidate<T>(current, currentScore);
                    Candidate<T> bestCand = new Candidate<T>(best, bestScore);
                    List<Candidate<T>> currentCandList = new List<Candidate<T>>();
                    currentCandList.Add(currentCand);
                    List<Candidate<T>> rejectedList = new List<Candidate<T>>();
                    if (!object.ReferenceEquals(next, current))
                        rejectedList.Add(new Candidate<T>(next, nextScore));
                    bool quit = !stillRun;
                    iterationFinished(quit, bestCand, currentCandList, rejectedList);
                    if (quit)
                        break;
                }
            }
            waithandle.Set();
        }


        /// <summary>
        /// Attempts to minimize the given scoring function.
        /// </summary>
        /// <param name="initialGuess">the initial guess for the optimizer</param>
        /// <param name="genNext">the function to produce new guesses</param>
        /// <param name="termFunction">the function to determine when the optimization should stop.
        /// once it returns false, the method stops</param>
        /// <returns>The best value found at the first point that termFunction returns true</returns>
        //abstract public T minimize(T initialGuess, GenerateNextArgs<T> genNext,TerminationFunction<T> termFunction);
    }
}
