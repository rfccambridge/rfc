using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Robocup.Plays
{
    class PlayComparer : Comparer<InterpreterPlay>
    {
        List<InterpreterPlay> preferedPlays;
        public PlayComparer(List<InterpreterPlay> preferedPlays)
        {
            this.preferedPlays = preferedPlays;
        }
        private const float BOOST = .1f;
        public override int Compare(InterpreterPlay x, InterpreterPlay y)
        {
            if (x == y)
                return 0;
            float diff = 0;
            if (preferedPlays.Contains(x))
                diff -= BOOST;
            if (preferedPlays.Contains(y))
                diff += BOOST;
            return Math.Sign(y.Score - x.Score + diff);
        }
    }
    class RandomizedPlayComparer : Comparer<InterpreterPlay>
    {
        Dictionary<InterpreterPlay, double> scores = new Dictionary<InterpreterPlay,double>();
        Random rand = new System.Random();
        private readonly double expBase, maxAdd;
        private double genAdd()
        {
            double r = rand.NextDouble();
            return (Math.Pow(expBase + 1, r) - 1) / expBase * maxAdd;
        }
        private const float BOOST = .3f;
        /// <summary>
        /// Adds some randomness to it.  expBase determines how skewed the distribution is
        /// (as expBase -> 0, it approaches linearity)
        /// maxAdd determines the maximum addition that can come from the randomness.
        /// </summary>
        /// <param name="preferedPlays"></param>
        /// <param name="expBase"></param>
        /// <param name="maxAdd"></param>
        public RandomizedPlayComparer(List<InterpreterPlay> preferedPlays, double expBase, double maxAdd)
        {
            this.expBase = Math.Pow(2, expBase);
            this.maxAdd = maxAdd;
            foreach (InterpreterPlay play in preferedPlays)
            {
                double score = play.Score + BOOST;
                double r = rand.NextDouble();
                score += genAdd();
                scores.Add(play, score);
            }
        }
        public override int Compare(InterpreterPlay x, InterpreterPlay y)
        {
            if (x == y)
                return 0;
            double xscore = 0, yscore = 0;
            if (scores.ContainsKey(x))
                xscore = scores[x];
            else
            {
                xscore = x.Score + genAdd();
                scores.Add(x, xscore);
            }
            if (scores.ContainsKey(y))
                yscore = scores[y];
            else
            {
                yscore = y.Score + genAdd();
                scores.Add(y, yscore);
            }
            return Math.Sign(yscore-xscore);
        }
    }
}
