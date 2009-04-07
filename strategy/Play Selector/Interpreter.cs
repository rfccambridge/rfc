using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Collections;
using Robocup.Core;

namespace Robocup.Plays
{
    public class Interpreter
    {

        private List<int> active = new List<int>();
        private readonly PlaySelector selector;
        private readonly IActionInterpreter actioninterpreter;
        private readonly IPredictor predictor;
        private List<InterpreterPlay> plays = new List<InterpreterPlay>();
       
        // Needs to be made private
        public Dictionary<int, string> ourPlayNames = new Dictionary<int, string>();
        public Dictionary<int, string> theirPlayNames = new Dictionary<int, string>();

        public Interpreter(bool flipCoordinates, InterpreterPlay[] plays,
                           IPredictor predictor, IActionInterpreter actioninterpreter)
        {
            selector = new PlaySelector();
            this.plays.AddRange(plays);
            if (flipCoordinates)
            {
                //if we have to flip the coordinates, wrap the predictor/actioninterpreter
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
            : this(flipCoordinates, plays, predictor, new ActionInterpreter(commander, predictor)) { }
        volatile bool running = false;
        object run_lock = new object();
        private List<InterpreterPlay> lastRunPlays = new List<InterpreterPlay>();

        public bool canInterpret()
        {
            return !running;
        }

        public void AddPlay(InterpreterPlay play)
        {
            plays.Add(play);
        }
        public void RemovePlay(InterpreterPlay play)
        {
            plays.Remove(play);
        }
        public void ReplacePlay(InterpreterPlay old_play, InterpreterPlay new_play)
        {
            RemovePlay(old_play);
            AddPlay(new_play);
        }

        List<SelectorResults.RobotAssignments> lastAssignments = new List<SelectorResults.RobotAssignments>();

        public List<InterpreterPlay> getPlays()
        {
            return plays;
        }

        /// <returns>Returns true if it actually interpreted,
        /// false if it quit because it was already interpreting.</returns>
        public bool interpret(PlayTypes type)
        {
            List<RobotInfo> ourteaminfo_base = predictor.getOurTeamInfo();
            List<RobotInfo> theirteaminfo_base = predictor.getTheirTeamInfo();
            InterpreterRobotInfo[] ourteaminfo = ourteaminfo_base.ConvertAll<InterpreterRobotInfo>(delegate(RobotInfo info)
            {
                return new InterpreterRobotInfo(info);
            }).ToArray();
            InterpreterRobotInfo[] theirteaminfo = theirteaminfo_base.ConvertAll<InterpreterRobotInfo>(delegate(RobotInfo info)
            {
                return new InterpreterRobotInfo(info);
            }).ToArray();


            BallInfo ballinfo = predictor.getBallInfo();
            
            lock (run_lock)
            {
                if (running)
                    return false;
                running = true;
            }

            //make sure all of the robots are not busy or assigned:
            foreach (InterpreterRobotInfo robot in ourteaminfo)
            {
                robot.setFree();
                ourPlayNames[robot.ID] = "N/A";
            }
            foreach (InterpreterRobotInfo robot in theirteaminfo)
            {
                robot.setFree();
            }
                

            List<InterpreterPlay> plays_to_run = plays.FindAll(
                delegate(InterpreterPlay play) { return play.PlayType == type && play.isEnabled; });
            //find all the actions we want to do
            SelectorResults results = selector.selectPlays(plays_to_run, ourteaminfo, theirteaminfo, ballinfo, lastRunPlays, lastAssignments, ourPlayNames);

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
                try
                {
                    action.Definition.runAction(actioninterpreter);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception in action.runAction:");
                    Console.WriteLine(e.ToString());
                }
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

            //commander.finishRound();
            lock (run_lock)
            {
                running = false;
            }
            return true;
        }
    }
}
