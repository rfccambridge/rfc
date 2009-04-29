using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Drawing;
using Robocup.Geometry;
using Robocup.Core;

namespace Robocup.Plays
{
    class SelectorResults
    {
        public class RobotAssignments
        {
            public RobotAssignments(InterpreterPlay play, int[] assignments){
                this.play=play;
                this.assignments=assignments;
            }
            private InterpreterPlay play;
            public InterpreterPlay Play
            {
                get { return play; }
            }
            private int[] assignments;
            public int[] Assignments
            {
                get { return assignments; }
            }
        }
        public SelectorResults(List<ActionInfo> actions, List<RobotAssignments> assignments)
        {
            this.actions = actions;
            this.assignments = assignments;
        }
        private List<RobotAssignments> assignments;
        public List<RobotAssignments> Assignments
        {
            get { return assignments; }
        }
        private List<ActionInfo> actions;
        public List<ActionInfo> Actions
        {
            get { return actions; }
        }
    }
	
    class PlaySelector
    {
        //int tick = 0;
        PlayEvaluator evaluator;
        public PlaySelector()
        {
            evaluator = new PlayEvaluator();
        }
        Random r = new Random();
        public SelectorResults selectPlays(List<InterpreterPlay> plays, InterpreterRobotInfo[] ourteaminfo, 
            InterpreterRobotInfo[] theirteaminfo, BallInfo ballinfo,
            List<InterpreterPlay> preferedPlays, List<SelectorResults.RobotAssignments> lastAssignments, 
            Dictionary<int, string> ourPlayNames)
        {
            evaluator.updateConditions(ourteaminfo, theirteaminfo, ballinfo);

            //Array.Sort(plays, new PlayComparer(preferedPlays));
            plays.Sort(new RandomizedPlayComparer(preferedPlays, 20, .3));

            List<ActionInfo> rtnActions = new List<ActionInfo>();
            List<SelectorResults.RobotAssignments> assignments=new List<SelectorResults.RobotAssignments>();

#if False
            // NEW CODE!!!!!

            // Free all robots
            foreach (SelectorResults.RobotAssignments ra in lastAssignments) {
                foreach (int id in ra.Assignments) {
                    for (int i = 0; i < ourteaminfo.Length; i++) {
                        if (id == ourteaminfo[i].ID) {
                            ourteaminfo[i].State = RobotStates.Free;
                            ourPlayNames[id] = "N/A";
                            break;
                        }
                    }
                }
            }

            List<SelectorResults.RobotAssignments> allAssignments = new List<SelectorResults.RobotAssignments>();

            // Get play1 and play2
            List<InterpreterPlay> selectedPlays = new List<InterpreterPlay>();
            selectedPlays.Add(getPlay(plays, "AttackOneGuy"));
            selectedPlays.Add(getPlay(plays, "Defense_with_goalie"));

            // Assign all selected plays
            List<int> alreadyAssigned = new List<int>();
            foreach (InterpreterPlay play in selectedPlays) {
                EvaluatorResults results = evaluator.evaluatePlay(play, alreadyAssigned.ToArray());
                if (results == null)
                    //this play was rejected
                    throw new ApplicationException("Play " + play.Name + " was rejected- all plays in these " +
                        "decision tree should have no conditions");

                SelectorResults.RobotAssignments assignedIDs = new SelectorResults.RobotAssignments(play, results.RobotIDs);

                Expression[] actions = results.Actions;
                ActionDefinition[] actiondefinitions = new ActionDefinition[actions.Length];
                for (int i = 0; i < actions.Length; i++) {
                    actiondefinitions[i] = (ActionDefinition)actions[i].getValue(evaluator.Tick, evaluator.State);
                }

                // Iterate over action definitions, set up play names, make robots busy
                for (int j = 0; j < actiondefinitions.Length; j++) {
                    ActionDefinition actiondef = actiondefinitions[j];
                    foreach (int id in actiondef.RobotsInvolved) {
                        for (int i = 0; i < ourteaminfo.Length; i++) {
                            if (id == ourteaminfo[i].ID) {
                                ourteaminfo[i].State = RobotStates.Busy;
                                ourPlayNames[id] = play.ToString() + ":" + actions[j].Name + ":" + id;
                                alreadyAssigned.Add(id);
                                break;
                            }
                        }
                        rtnActions.Add(new ActionInfo(actiondef, play));
                    }
                }
                allAssignments.Add(new SelectorResults.RobotAssignments(play,results.RobotIDs));
            }

            return new SelectorResults(rtnActions, allAssignments);

            // END NEW CODE!!!!!!
#endif

            // Free all robots
            foreach (SelectorResults.RobotAssignments ra in lastAssignments) {
                foreach (int id in ra.Assignments) {
                    for (int i = 0; i < ourteaminfo.Length; i++) {
                        if (id == ourteaminfo[i].ID) {
                            ourteaminfo[i].State = RobotStates.Free;
                            ourPlayNames[id] = "N/A";
                            break;
                        }
                    }
                }
            }

            // get main play
            InterpreterPlay mainplay = getPlay(plays, "main");

            // get actions from main play

            int[] dummyAssignedIDs = new int[0]; // No plays assigned
            EvaluatorResults mainresults = evaluator.evaluatePlay(mainplay, dummyAssignedIDs);

            if (mainresults == null)
                //this play was rejected
                throw new ApplicationException("Main play was rejected!");

            assignments.Add(new SelectorResults.RobotAssignments(mainplay, mainresults.RobotIDs));
            Expression[] mainactions = mainresults.Actions;
            ActionDefinition[] mainactiondefinitions = new ActionDefinition[mainactions.Length];
            List<InterpreterPlay> assignedPlays = new List<InterpreterPlay>();
            for (int i = 0; i < mainactions.Length; i++) {
                mainactiondefinitions[i] = (ActionDefinition)mainactions[i].getValue(evaluator.Tick, evaluator.State);

                if (mainactiondefinitions[i].isEmpty) {
                    // this is a null action, skip it
                    continue;
                }

                if (mainactiondefinitions[i].AssignPlay == null) {
                    throw new ApplicationException("There is an action in the main play that is not an AssignPlay action");
                }

                assignedPlays.Add(getPlay(plays, mainactiondefinitions[i].AssignPlay));
            }

            int numOurRobotsAvailable = ourteaminfo.Length;
            //foreach (InterpreterPlay play in plays)
            for (int playnum = 0; playnum < assignedPlays.Count; playnum++) {
                InterpreterPlay play = assignedPlays[playnum];
                //Console.Write("Evaluating play " + play.Name + "......     ");
                try {
                    if (play.NumOurRobots <= ourteaminfo.Length && play.Actions.Count <= numOurRobotsAvailable &&
                        play.NumTheirRobots <= theirteaminfo.Length) {
                        int[] assignedIDs = null;
                        // Evaluator returns (if play accepted): the actions [not sure about them] and 
                        // the robot assignments, which are a map virtual_robot_id -> real_robot_id. Remember
                        // that the evaluator knows about the world because it keeps state which we update
                        // above in this method.
                        EvaluatorResults results = evaluator.evaluatePlay(play, assignedIDs);
                        if (results == null)
                            //this play was rejected
                            continue;
                        assignments.Add(new SelectorResults.RobotAssignments(play, results.RobotIDs));
                        Expression[] actions = results.Actions;
                        ActionDefinition[] actiondefinitions = new ActionDefinition[actions.Length];
                        for (int i = 0; i < actions.Length; i++) {
                            actiondefinitions[i] = (ActionDefinition)actions[i].getValue(evaluator.Tick, evaluator.State);
                        }

                        //#if DEBUG <--- taken out to stop crashes
#if false
                        //is this check weaker than the one below?
                        bool b = canUseActions(actiondefinitions, ourteaminfo);
                        if (!b)
                            throw new ApplicationException("this should never happen");
#endif
                        for (int j = 0; j < actions.Length; j++) {
                            ActionDefinition action = actiondefinitions[j];
                            {
                                foreach (int id in action.RobotsInvolved) {
                                    for (int i = 0; i < ourteaminfo.Length; i++) {
                                        if (id == ourteaminfo[i].ID) {
                                            //#if DEBUG
                                            //                                            if (ourteaminfo[i].State != RobotStates.Free)
                                            //                                                throw new ApplicationException("this should never happen -- an action was assigned to a robot that wasn't free");
                                            //#endif
                                            numOurRobotsAvailable--;
                                            ourteaminfo[i].State = RobotStates.Busy;
                                            ourPlayNames[id] = play.ToString() + ":" + actions[j].Name + ":" + id;
                                            break;
                                        }
                                    }
                                }
                                rtnActions.Add(new ActionInfo(action, play));
                            }
                        }
                    }
                }
                catch (ImplicitAssumptionFailedException) { }
                //catch (ImplicitAssumptionFailedException e) { Console.WriteLine("but gave the error: " + e.Message); }
            }

            return new SelectorResults(rtnActions, assignments);

            /*

            int numOurRobotsAvailable = ourteaminfo.Length;
            //foreach (InterpreterPlay play in plays)
            for (int playnum = 0; playnum < plays.Count; playnum++)
            {
                InterpreterPlay play = plays[playnum];
                //the score difference between this play, and the one that comes after it
                double scoreDiff = play.Score;
                if (playnum < plays.Count - 1)
                    scoreDiff -= plays[playnum + 1].Score;
                //Console.Write("Evaluating play " + play.Name + "......     ");
                try
                {
                    if (play.NumOurRobots <= ourteaminfo.Length && play.Actions.Count <= numOurRobotsAvailable && 
                        play.NumTheirRobots <= theirteaminfo.Length)
                    {
                        int[] assignedIDs=null;
                        //the larger the difference in score between this play and the next play,
                        //the higher the chances that this play will break free and take the better
                        //robots for itself
                        if (r.NextDouble() > scoreDiff * plays.Count / 10 - .2)
                        {
                            foreach (SelectorResults.RobotAssignments ra in lastAssignments)
                            {
                                if (ra.Play == play)
                                    assignedIDs = ra.Assignments;
                            }
                        }
                        // Evaluator returns (if play accepted): the actions [not sure about them] and 
                        // the robot assignments, which are a map virtual_robot_id -> real_robot_id. Remember
                        // that the evaluator knows about the world because it keeps state which we update
                        // above in this method.
                        EvaluatorResults results = evaluator.evaluatePlay(play,assignedIDs);
                        if (results == null)
                            //this play was rejected
                            continue;
                        assignments.Add(new SelectorResults.RobotAssignments(play,results.RobotIDs));
                        Expression[] actions = results.Actions;
                        ActionDefinition[] actiondefinitions = new ActionDefinition[actions.Length];
                        for (int i = 0; i < actions.Length; i++)
                        {
                            actiondefinitions[i] = (ActionDefinition)actions[i].getValue(evaluator.Tick, evaluator.State);
                        }

//#if DEBUG <--- taken out to stop crashes
#if false
                        //is this check weaker than the one below?
                        bool b = canUseActions(actiondefinitions, ourteaminfo);
                        if (!b)
                            throw new ApplicationException("this should never happen");
#endif
                        for (int j = 0; j < actions.Length; j++)
                        {
                            ActionDefinition action = actiondefinitions[j];
                            {
                                foreach (int id in action.RobotsInvolved)
                                {
                                    for (int i = 0; i < ourteaminfo.Length; i++)
                                    {
                                        if (id == ourteaminfo[i].ID)
                                        {
//#if DEBUG
//                                            if (ourteaminfo[i].State != RobotStates.Free)
//                                                throw new ApplicationException("this should never happen -- an action was assigned to a robot that wasn't free");
//#endif
                                            numOurRobotsAvailable--;
                                            ourteaminfo[i].State = RobotStates.Busy;
                                            ourPlayNames[id] = play.ToString() + ":" + actions[j].Name + ":" + id;
                                            break;
                                        }
                                    }
                                }
                                rtnActions.Add(new ActionInfo(action, play));
                            }
                        }
                    }
                }
                catch (ImplicitAssumptionFailedException) { }
                //catch (ImplicitAssumptionFailedException e) { Console.WriteLine("but gave the error: " + e.Message); }
            }
            return new SelectorResults(rtnActions, assignments);
             */
            
        }
        private bool canUseActions(ActionDefinition[] actions, InterpreterRobotInfo[] ourteam)
        {
            int numrobots = ourteam.Length;
            RobotStates[] states = new RobotStates[numrobots];
            int[] idcodes = new int[numrobots];
            for (int i = 0; i < numrobots; i++)
            {
                states[i] = ourteam[i].State;
                idcodes[i] = ourteam[i].ID;
            }
            foreach (ActionDefinition a in actions)
            {
                foreach (int id in a.RobotsInvolved)
                {
                    bool found = false;
                    for (int i = 0; i < numrobots; i++)
                    {
                        if (id == idcodes[i])
                        {
                            if (states[i] == RobotStates.Busy)
                                return false;
                            //states[i] = RobotStates.Free;
                            found = true;
                            break;
                        }
                    }
                    if (found == false)
                    {
                        //throw new ApplicationException("Error in canUseActions(): could not find robot with ID " + id);
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Get a named play from a list of plays
        /// </summary>
        /// <param name="plays"></param>
        /// <param name="playname"></param>
        /// <returns></returns>
        private static InterpreterPlay getPlay(List<InterpreterPlay> plays, String playname) {
            foreach (InterpreterPlay thisplay in plays) {
                if (String.Compare(thisplay.Name.Split('-')[1].Substring(1), playname) == 0) {
                    return thisplay;
                }
            }
            
            // if no play is found in list, throw exception
            throw new ApplicationException("Error: Play " + playname + " not found in list of plays");
        }
    }
}
