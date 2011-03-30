using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using Robocup.CoreRobotics;

namespace Robocup.PlaySystem
{
    /// <summary>
    /// A wrapper for an actioninterpreter, which, along with performing an action, 
    /// keeps track of the assigned IDs. 
    /// This is, like the FlipActionInterpreter, unfortunate, as it requires adding
    /// a new action to add wrappers to each of them. (Indeed, it find it the worst hack in the entire 
    /// new play system). It is possible such functionality could be added to the action interpreter, 
    /// or some kind of reflection could automatically wrap it (although that leads to many problems
    /// itself).
    /// </summary>
    public class ActionAssigner : IActionInterpreter
    {
        // This action interpreter keeps track of IDs that are assigned
        // (until it is reset)
        List<int> assignedIDs;
        public List<int> AssignedIDs
        {
            get { return assignedIDs; }
        }

        // the IActionInterpreter that is being wrapped
        IActionInterpreter actioninterpreter;

        public ActionAssigner(IActionInterpreter a)
        {
            assignedIDs = new List<int>();
            actioninterpreter = a;
        }

        /// <summary>
        /// Change the currently assigned action interpreter
        /// </summary>
        /// <param name="actioninterpreter"></param>
        public void setActionInterpreter(IActionInterpreter actioninterpreter)
        {
            this.actioninterpreter = actioninterpreter;
        }

        /// <summary>
        /// forget all currently assigned robots
        /// </summary>
        public void reset()
        {
            assignedIDs.Clear();
        }

        public void Charge(int robotID)
        {
            assignedIDs.Add(robotID);
            actioninterpreter.Charge(robotID);
        }

        public void Charge(int robotID, int strength)
        {
            assignedIDs.Add(robotID);
            actioninterpreter.Charge(robotID, strength);
        }

        public void Kick(int robotID, Vector2 target)
        {
            assignedIDs.Add(robotID);
            actioninterpreter.Kick(robotID, target);
        }

        public void Kick(int robotID, Vector2 target, int strength)
        {
            assignedIDs.Add(robotID);
            actioninterpreter.Kick(robotID, target, strength);
        }

        public void Bump(int robotID, Vector2 target)
        {
            assignedIDs.Add(robotID);
            actioninterpreter.Bump(robotID, target);
        }

        public void Move(int robotID, Vector2 target)
        {
            assignedIDs.Add(robotID);
            actioninterpreter.Move(robotID, target);
        }

        public void Move(int robotID, Vector2 target, Vector2 facing)
        {
            assignedIDs.Add(robotID);
            actioninterpreter.Move(robotID, target, facing);
        }

        public void Move(int robotID, bool avoidBall, Vector2 target, Vector2 facing)
        {
            assignedIDs.Add(robotID);
            actioninterpreter.Move(robotID, avoidBall, target, facing);
        }

        public void Stop(int robotID)
        {
            assignedIDs.Add(robotID);
            actioninterpreter.Stop(robotID);
        }

        public void Dribble(int robotID, Vector2 target)
        {
            assignedIDs.Add(robotID);
            actioninterpreter.Dribble(robotID, target);
        }

        public void LoadConstants()
        {
            actioninterpreter.LoadConstants();
        }
    }
}
