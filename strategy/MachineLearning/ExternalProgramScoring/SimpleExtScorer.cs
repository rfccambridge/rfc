using System;
using System.Collections.Generic;
using System.Text;

namespace MachineLearning.ExternalProgramScoring
{
    class SimpleExtScorer : ExtProgScorerBase<double>
    {
        protected override double parseOutputFile(string wholeFile)
        {
            string[] lines = wholeFile.Split('\n', '\r');
            return double.Parse(lines[0]);
        }
    }
    class StochasticExtScorer : ExtProgScorerBase<StochasticAnswer>
    {
        protected override StochasticAnswer parseOutputFile(string wholeFile)
        {
            string[] lines = wholeFile.Split(new char[] { '\n', '\r' },StringSplitOptions.RemoveEmptyEntries);
            return new StochasticAnswer(double.Parse(lines[0]), double.Parse(lines[1]));
        }

        public void setCalcPower(double d)
        {
            this.setArguments(d + "");
        }
    }
}
