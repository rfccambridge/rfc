using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;

namespace Robocup.Plays
{
    public delegate void Action(IActionInterpreter a);
    public class ActionDefinition
    {
        private string Name;
        private Action action;

        // NEW: Allow actions to include a play assignment
        public String AssignPlay;
        public bool isEmpty = false;
        
        public Action runAction
        {
            get { return action; }
        }
        private int[] robotsinvolved;
        public int[] RobotsInvolved
        {
            get { return robotsinvolved; }
        }

        /// <summary>
        /// Initialized with an Action and with the ids of the robots that are involved
        /// </summary>
        /// <param name="action"></param>
        /// <param name="robotsInvolved"></param>
        public ActionDefinition(Action action, params int[] robotsInvolved)
        {
            this.action = action;
            this.robotsinvolved = robotsInvolved;
        }

        /// <summary>
        /// An ActionDefinition with just a play to assign
        /// </summary>
        /// <param name="assignPlay"></param>
        public ActionDefinition(String assignPlay) {
            AssignPlay = assignPlay;
        }

        /// <summary>
        /// A null action
        /// </summary>
        /// <returns></returns>
        public ActionDefinition() {
            isEmpty = true;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("robots: ");
            foreach (int i in robotsinvolved)
            {
                sb.Append(i);
                sb.Append(' ');
            }
            return sb.ToString();
        }
    }
	
}
