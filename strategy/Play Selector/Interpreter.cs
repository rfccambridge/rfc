using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Collections;
using Robocup.Core;
using System.IO;
using Robocup.Utilities;
using Robocup.PlaySystem;

namespace Robocup.Plays
{
    public class Interpreter
    {
        private List<int> active = new List<int>();
        private readonly PlaySelector selector;
        private IActionInterpreter actioninterpreter;
        private IPredictor predictor;
        private List<InterpreterPlay> plays = new List<InterpreterPlay>();
        private Team team;
        private FieldHalf fieldHalf;
        private FieldDrawer fieldDrawer;

        // new play system- retain a game state and a play assigner
        private GameState state;
        private PlayAssigner playAssigner;

        public FieldHalf FieldHalf
        {
            get { return fieldHalf; }
            set
            {
                if (running)
                    throw new ApplicationException("Cannot change field half while running.");                

                if (fieldHalf != value) 
                {
                    if (value == FieldHalf.Right)
                    {
                        //if we have to flip the coordinates, wrap the predictor/actioninterpreter
                        actioninterpreter = new FlipActionInterpreter(actioninterpreter);
                        predictor = new FlipPredictor(predictor);
                    }
                    else
                    {
                        FlipActionInterpreter flipAI = actioninterpreter as FlipActionInterpreter;
                        FlipPredictor flipPredictor = predictor as FlipPredictor;
                        actioninterpreter = flipAI.ActionInterpreter;
                        predictor = flipPredictor.Predictor;
                    }

                    // New PlaySystem code
                    // switch the actioninterpreter and the predictor in the game state
                    state.setActionInterpreter(actioninterpreter);
                    state.setPredictor(predictor);
                }
                fieldHalf = value;
            }
        }

        public Interpreter(Team ourTeam, FieldHalf fieldHalf, IPredictor predictor, IActionInterpreter actioninterpreter, FieldDrawer drawer)
        {
            team = ourTeam;            
            fieldDrawer = drawer;            
            this.fieldHalf = fieldHalf;

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

            // new play system: set up the game state
            state = new GameState(this.predictor, this.actioninterpreter, ourTeam);
            playAssigner = new PlayAssigner();

            selector = new PlaySelector();
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

            // load the new play system's constants
            playAssigner.ReloadConstants();
        }
 
        List<SelectorResults.RobotAssignments> lastAssignments = new List<SelectorResults.RobotAssignments>();

        /// <summary>
        /// An alternative to the old play interpreter. Plays are written in C# and found in the PlaySystem project
        /// </summary>
        /// <param name="type">The current play type to be interpreted</param>
        /// <returns>true if play was interpreted, false if is already interpreting</returns>
        public bool interpret_csharp(PlayType type)
        {
            // the old interpret method had a check to make sure the interpreter was not already running
            // I do not believe that was necessary, as I set a breakpoint and it was never broken.
            // Furthermore, one player has only one thread that calls interpret. I removed it 
            // to save space and confusion

            // pass this play type on to the play assigner
            state.Playtype = type;
            playAssigner.assignPlays(state);

            return true;
        }

        /// <returns>Returns true if it actually interpreted,
        /// false if it quit because it was already interpreting.</returns>
        public bool interpret(PlayType type, Score score)
        {
            // if the appropriate constant is set, use the new play system
            if (Constants.PlayFiles.USE_C_SHARP_PLAY_SYSTEM) {
                return interpret_csharp(type);
            }
            
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
                
            // If the ball is not in the field, just return true after setting
            // running to false.
            if (ballinfo == null || !TacticsEval.InField(ballinfo.Position))
            {
                foreach (InterpreterRobotInfo robot in ourteaminfo)
                {
                    actioninterpreter.Stop(robot.ID);
                }
                lock (run_lock)
                {
                    running = false;
                }
                return true;
            }

            int ourgoals = (team == Team.Blue) ? score.GoalsBlue : score.GoalsYellow;
            int theirgoals = (team == Team.Blue) ? score.GoalsYellow : score.GoalsBlue;

            List<InterpreterPlay> plays_to_run = plays.FindAll(
                delegate(InterpreterPlay play) { return play.PlayType == type && play.isEnabled; });
            //find all the actions we want to do
            SelectorResults results = selector.selectPlays(plays_to_run, ourteaminfo, theirteaminfo, ballinfo, ourgoals, theirgoals,
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
