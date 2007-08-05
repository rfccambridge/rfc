using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Collections;
using Robocup.Infrastructure;

namespace RobocupPlays
{
    public class Interpreter
    {

        private List<int> active = new List<int>();
        private readonly PlaySelector selector;
        private readonly IActionInterpreter actioninterpreter;
        private readonly IPredictor predictor;

        public Interpreter(bool flipCoordinates, InterpreterPlay[] plays,
                           IPredictor predictor, IActionInterpreter actioninterpreter)
        {
            selector = new PlaySelector(plays);
            if (flipCoordinates)
            {
                this.actioninterpreter = new FlipActionInterpreter(actioninterpreter);
                this.predictor = new FlipPredictor(predictor);
            }
            else
            {
                this.actioninterpreter = actioninterpreter;
                this.predictor = predictor;
            }
        }
        public Interpreter(bool flipCoordinates, InterpreterPlay[] plays, IPredictor predictor, IController commander)
            : this(flipCoordinates, plays, predictor, new ActionInterpreter(commander, predictor))
        {
        }
        volatile int numselecting = 0;
        private List<InterpreterPlay> lastRunPlays = new List<InterpreterPlay>();
        /// <summary>
        /// Returns the list of all plays that were run the most recent time.
        /// </summary>
        protected List<InterpreterPlay> LastRunPlays
        {
            get { return lastRunPlays; }
        }
        /// <summary>
        /// This method is intended to be overriden in any subclasses.  It gets called at the end of every
        /// interpreting step.
        /// </summary>
        protected virtual void finishedInterpreting()
        {
        }

        public bool canInterpret()
        {
            return numselecting == 0;
        }

        List<SelectorResults.RobotAssignments> lastAssignments = new List<SelectorResults.RobotAssignments>();

        /// <returns>Returns true if it actually interpreted,
        /// false if it quit because it was already interpreting.</returns>
        public bool interpret(PlayTypes type)
        {
            RobotInfo[] ourteaminfo = predictor.getOurTeamInfo().ToArray();
            RobotInfo[] theirteaminfo = predictor.getTheirTeamInfo().ToArray();
            BallInfo ballinfo = predictor.getBallInfo();

            numselecting++;
            if (numselecting > 1)
            {
                numselecting--;
                return false;
            }
            //DateTime start = System.DateTime.Now;

            //make sure all of the robots are not busy or assigned:
            foreach (RobotInfo robot in ourteaminfo)
            {
                robot.setFree();
            }
            foreach (RobotInfo robot in theirteaminfo)
            {
                robot.setFree();
            }

            //find all the actions we want to do
            SelectorResults results = selector.selectPlays(type, ourteaminfo, theirteaminfo, ballinfo, lastRunPlays, lastAssignments);

            lastAssignments = results.Assignments;

            lastRunPlays.Clear();
            foreach (ActionInfo action in results.Actions)
            {
                if (!lastRunPlays.Contains(action.Play))
                    lastRunPlays.Add(action.Play);
            }

            List<int> nowactive = new List<int>();
            //update the actioninterpreter with the current state
            //actioninterpreter.NewRound(ourteaminfo, theirteaminfo, ballinfo);
            //do each action
            foreach (ActionInfo action in results.Actions)
            {
                action.Definition.runAction(actioninterpreter);
                foreach (int robot in action.RobotsInvolved)
                {
                    if (!nowactive.Contains(robot))
                        nowactive.Add(robot);
                    else
                    {
                        Console.WriteLine("this robot has two actions assigned to it!");
                        //throw new ApplicationException("this robot has two actions assigned to it!");
                    }
                }
            }
            foreach (int robot in active)
            {
                if (!nowactive.Contains(robot))
                {
                    actioninterpreter.Stop(robot);
                }
            }
            active = nowactive;

            finishedInterpreting();
            //commander.finishRound();
            numselecting--;
            return true;
        }
    }
}
