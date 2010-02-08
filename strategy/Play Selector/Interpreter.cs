using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Collections;
using Robocup.Core;
using System.IO;
using Robocup.Utilities;

namespace Robocup.Plays
{
    public class Interpreter
    {

        private List<int> active = new List<int>();
        private readonly PlaySelector selector;
        private readonly IActionInterpreter actioninterpreter;
        private readonly IPredictor predictor;
        private List<InterpreterPlay> plays = new List<InterpreterPlay>();
        private Team team;
        private FieldDrawer fieldDrawer;

        public Interpreter(Team ourTeam, FieldHalf fieldHalf, IPredictor predictor, IActionInterpreter actioninterpreter, FieldDrawer drawer)
        {
            team = ourTeam;
            fieldDrawer = drawer;

            selector = new PlaySelector();            

            if (fieldHalf == FieldHalf.Right)
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
        public Interpreter(Team team, FieldHalf fieldHalf, IPredictor predictor, IController commander, FieldDrawer fieldDrawer)
            : this(team, fieldHalf, predictor, new ActionInterpreter(team, commander, predictor), fieldDrawer) { }

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
        public void LoadPlays(List<InterpreterPlay> newPlays)
        {
            plays.Clear();
            plays.AddRange(newPlays);
        }
        public void LoadConstants()
        {
            actioninterpreter.LoadConstants();
        }
 
        List<SelectorResults.RobotAssignments> lastAssignments = new List<SelectorResults.RobotAssignments>();

        /// <returns>Returns true if it actually interpreted,
        /// false if it quit because it was already interpreting.</returns>
        public bool interpret(PlayType type)
        {
            List<RobotInfo> ourteaminfo_base = predictor.GetRobots(team);
            List<RobotInfo> theirteaminfo_base = predictor.GetRobots((team == Team.Yellow) ? Team.Blue : Team.Yellow);
            
            InterpreterRobotInfo[] ourteaminfo = ourteaminfo_base.ConvertAll<InterpreterRobotInfo>(delegate(RobotInfo info)
            {
                return new InterpreterRobotInfo(info);
            }).ToArray();
            InterpreterRobotInfo[] theirteaminfo = theirteaminfo_base.ConvertAll<InterpreterRobotInfo>(delegate(RobotInfo info)
            {
                return new InterpreterRobotInfo(info);
            }).ToArray();

            BallInfo ballinfo = predictor.GetBall();
            
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
                if (fieldDrawer != null)
                    fieldDrawer.UpdatePlayName(team, robot.ID, "N/A");
                
            }
            foreach (InterpreterRobotInfo robot in theirteaminfo)
            {
                robot.setFree();
                if (fieldDrawer != null)
                    fieldDrawer.UpdatePlayName(team, robot.ID, "N/A");
            }
                

            List<InterpreterPlay> plays_to_run = plays.FindAll(
                delegate(InterpreterPlay play) { return play.PlayType == type && play.isEnabled; });
            //find all the actions we want to do
            SelectorResults results = selector.selectPlays(plays_to_run, ourteaminfo, theirteaminfo, ballinfo, 
                                                           lastRunPlays, lastAssignments);

            lastAssignments = results.Assignments;

            lastRunPlays.Clear();
            foreach (ActionInfo action in results.Actions)
            {
                if (!lastRunPlays.Contains(action.Play))
                    lastRunPlays.Add(action.Play);
            }

            // Draw the labels with play assignments
            foreach (ActionInfo action in results.Actions)
                foreach (int id in action.RobotsInvolved)
                    for (int i = 0; i < ourteaminfo.Length; i++)
                        if (id == ourteaminfo[i].ID)
                            fieldDrawer.UpdatePlayName(team, id, action.Play.Name);

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
