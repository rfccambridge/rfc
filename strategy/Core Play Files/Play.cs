using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;

namespace Robocup.Plays
{
    abstract public class Playable<E> where E : Expression
    {
        List<E> actions = new List<E>();
        public List<E> Actions
        {
            get { return actions; }
        }

        protected string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        /// <summary>
        /// This is for loading the play
        /// </summary>
        public abstract E TheBall { get; }
        public abstract IList<E> Robots { get; }
        /// <summary>
        /// Tries to add a robot.  Note to subclasses: you are guaranteed that all robots that are added
        /// are added through here, but not that all things put through here are robots (others should
        /// be rejected).
        /// </summary>
        /// <param name="exp"></param>
        public abstract void addRobot(E exp);
        public abstract void SetDesignerData(List<string> data);

        protected Dictionary<string, E> playObjects = new Dictionary<string, E>();
        /// <summary>
        /// This holds all of the "objects" -- the things that actually determine the geometry of the play.
        /// (operational definition: everything but actions and conditions)
        /// Includes both geometry, like lines and segments, but also values, like bools and ints.
        /// The keys are the names of the items
        /// </summary>
        public Dictionary<string, E> PlayObjects
        {
            get { return playObjects; }
        }
        /*private Dictionary<string, Vector2> designerPositions = new Dictionary<string,Vector2>();
        public Dictionary<string, Vector2> DesignerPositions
        {
            get { return designerPositions; }
        }*/
        public List<E> getAllObjects()
        {
            return new List<E>(playObjects.Values);
        }
    }

    public interface IPlay<E>
        where E : Expression
    {
        List<E> Conditions { get; }
        PlayType PlayType { get; set; }
        double Score { get; set; }
        int ID { get; set; }
        IList<Playable<E>> Tactics { get; }
    }

    public interface ITactic<E>
        where E : Expression
    {
        /// <summary>
        /// Contains placeholders for tactic parameters.
        /// When the tactic is loaded into a play, the play will get the expressions passed instead of these.
        /// </summary>
        List<E> Parameters { get; }
    }
}
