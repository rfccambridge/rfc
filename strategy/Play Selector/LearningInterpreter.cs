using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Infrastructure;


namespace RobocupPlays
{
    /*using Dict = Dictionary<InterpreterPlay, int>;
    using DictPair = KeyValuePair<InterpreterPlay, int>;

    public class LearningInterpreter : Interpreter
    {

        public LearningInterpreter(InterpreterPlay[] plays, IController commander, ActionInterpreter actionInterpreter)
            : base(plays, commander, actionInterpreter) { }
        public LearningInterpreter(InterpreterPlay[] plays, IController commander)
            : base(plays, commander, new ActionInterpreter(commander)) { }

        int tick = 0;
        Dict lastRunTime = new Dict();
        Dict totalRuns = new Dict();
        Dict goalsFor = new Dict();
        Dict goalsAgainst = new Dict();
        int totalgoalsfor = 0, totalgoalsagainst = 0;

        protected override void finishedInterpreting()
        {
            tick++;
            List<InterpreterPlay> ranPlays = this.LastRunPlays;
            foreach (InterpreterPlay play in ranPlays)
            {
                lastRunTime[play] = tick;
                if (!totalRuns.ContainsKey(play))
                    totalRuns.Add(play, 0);
            }
            foreach (DictPair pair in lastRunTime)
            {
                InterpreterPlay play = pair.Key;
                int lastRun = pair.Value;
                if (tick-lastRun < minRoundsAgo)
                    totalRuns[play]++;
            }
        }

        private const int minRoundsAgo = 50;

        //TODO should this be just a function in LearningInterpreter, or also a stub in Interpreter?
        /// <summary>
        /// Tells the interpreter that a goal was scored.
        /// </summary>
        /// <param name="goodGoal">This should be true if the goal was scored by the team being controlled
        /// by this interpreter, and false otherwise.</param>
        public void goalScored(bool goodGoal)
        {
            if (goodGoal)
            {
                totalgoalsfor++;
            }
            else
            {
                totalgoalsagainst++;
            }
            foreach (DictPair pair in lastRunTime)
            {
                int timeSinceRun = tick - pair.Value;
                if (timeSinceRun <= minRoundsAgo)
                {
                    InterpreterPlay play = pair.Key;
                    Dict dict;
                    if (goodGoal)
                    {
                        dict = goalsFor;
                    }
                    else
                    {
                        dict = goalsAgainst;
                    }
                    if (!dict.ContainsKey(play))
                        dict.Add(play, 0);
                    dict[play]++;
                }
            }
            //outputResults();
        }
        public void outputResults()
        {
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~");
            Console.WriteLine(getResults());
        }

        public String getResults()
        {
            StringBuilder sb = new StringBuilder();

            double avgfor = (double)totalgoalsfor / tick;
            double avgagainst = (double)tick / totalgoalsagainst;

            sb.AppendLine(tick + " ticks elapsed");
            sb.AppendLine(totalgoalsfor + " goals scored");
            sb.AppendLine(totalgoalsagainst + " goals allowed");
            sb.AppendLine("an average of " + avgfor + " scores per tick");
            sb.AppendLine("and " + avgagainst + " ticks per goal");
            sb.AppendLine("for both, higher is better");

            foreach (DictPair pair in totalRuns)
            {
                InterpreterPlay play = pair.Key;
                int ticks = pair.Value;
                sb.AppendLine();
                sb.Append("Play: " + play + "\t");
                sb.Append("Total ticks: " + ticks + "\t");
                int goodgoals = 0;
                goalsFor.TryGetValue(play, out goodgoals);
                sb.Append("Goals for: " + goodgoals + "\t");
                int badgoals = 0;
                goalsAgainst.TryGetValue(play, out badgoals);
                sb.AppendLine("Goals against: " + badgoals);

                sb.AppendLine("an average of " + (double)goodgoals / ticks + " scores per tick (" + (double)goodgoals / ticks / avgfor + " times better)");
                if (badgoals > 0)
                    sb.AppendLine("and " + (double)ticks / badgoals + " ticks per goal (" + (double)ticks / badgoals / avgagainst + " times better)");
            }

            return sb.ToString();
        }
    }*/
}
