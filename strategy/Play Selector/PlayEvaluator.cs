using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Drawing;
using Robocup.CorePlayFiles;
using Robocup.Core;
using System.Threading;

namespace Robocup.Plays
{
    class EvaluatorResults
    {
        public EvaluatorResults(InterpreterExpression[] actions, int[] robotids)
        {
            this.actions = actions;
            this.robotids = robotids;
        }
        private InterpreterExpression[] actions;
        public InterpreterExpression[] Actions
        {
            get { return actions; }
        }
        private int[] robotids;
        public int[] RobotIDs
        {
            get { return robotids; }
        }

    }

    class PlayEvaluator
    {
        EvaluatorState state;

        public PlayEvaluator() {
            Console.WriteLine("Starting Stats computer thread.");
            new Thread(Stats.RunStatsComputer);
        }

        public EvaluatorState State
        {
            get { return state; }
        }
        /// <summary>
        /// This is SO NECESSARY if you have more than one interpreter running at once --
        /// if you do not increment both ticks at once, then you get bad caching errors
        /// </summary>
        static int synchronizedTick = 0;
        public int Tick
        {
            get { return state.Tick; }
        }
        InterpreterPlay curplay;
        uint counter = 0;
        /// <summary>
        /// Updates the conditions, and the tick will be increased by one.
        /// </summary>
        public void updateConditions(InterpreterRobotInfo[] ourteaminfo, InterpreterRobotInfo[] theirteaminfo, BallInfo ballinfo, int ourgoals, int theirgoals)
        {
            synchronizedTick++;
            state = new EvaluatorState(ourteaminfo, theirteaminfo, ballinfo, ourgoals, theirgoals, synchronizedTick);
            Stats.ComputeStats(state);
        }
        private void clearAssignments()
        {
            foreach (InterpreterRobotInfo rinf in state.OurTeamInfo)
            {
                rinf.Assigned = false;
            }
            foreach (InterpreterRobotInfo rinf in state.TheirTeamInfo)
            {
                rinf.Assigned = false;
            }
        }
        Random r = new Random();
        public EvaluatorResults evaluatePlay(InterpreterPlay play, int[] lastAssignedIDs)
        {
            curplay = play;
            if (state == null)
                throw new ApplicationException("You can't evaluate plays without first setting up the evaluator");
            clearAssignments();

            play.setEvaluatorState(state);

            bool failed=!play.forceRobotDefinitionOrder(lastAssignedIDs);

            if (!failed)
            {
                foreach (InterpreterExpression e in play.Conditions)
                {
                    bool b = (bool)e.getValue(Tick,state);
                    if (!b)
                    {
                        failed = true;
                        break;
                    }
                }
            }
            if (failed)
            {
                return null;//failed
            }

            int[] robotIDs = new int[curplay.Robots.Count];
            int i = 0;
            foreach (InterpreterExpression e in curplay.Robots)
            {
                PlayRobotDefinition definition = e.getValue(Tick, state) as PlayRobotDefinition;
                robotIDs[i] = definition.getID();
                i++;
            }
            return new EvaluatorResults(curplay.Actions.ToArray(), robotIDs);
        }
    }
}
