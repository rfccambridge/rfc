using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;

namespace Robocup.Plays
{
    public delegate void Action(IActionInterpreter a);
    public class ActionDefinition
    {
        private Action action;
        public Action runAction
        {
            get { return action; }
        }
        private int[] robotsinvolved;
        public int[] RobotsInvolved
        {
            get { return robotsinvolved; }
        }
        public ActionDefinition(Action action, params int[] robotsInvolved)
        {
            this.action = action;
            this.robotsinvolved = robotsInvolved;
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
