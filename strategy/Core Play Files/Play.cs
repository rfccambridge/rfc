using System;
using System.Collections.Generic;
using System.Text;

namespace Robocup.Plays
{
    abstract public class Play<T> where T : Expression
    {
        List<T> conditions = new List<T>();
        public List<T> Conditions
        {
            get { return conditions; }
        }
        List<T> actions = new List<T>();
        public List<T> Actions
        {
            get { return actions; }
        }

        private PlayTypes type = PlayTypes.NormalPlay;
        public PlayTypes PlayType
        {
            get { return type; }
            set { type = value; }
        }
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private float score = (float)(1.0 + new Random().NextDouble() / 100);
        public float Score
        {
            get { return score; }
            set { score = value; }
        }

        private int id;
        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// This is for loading the play
        /// </summary>
        public abstract T TheBall { get;}
        public abstract IList<T> Robots { get;}
        /// <summary>
        /// Tries to add a robot.  Note to subclasses: you are guaranteed that all robots that are added
        /// are added through here, but not that all things put through here are robots (others should
        /// be rejected).
        /// </summary>
        /// <param name="exp"></param>
        public abstract void addRobot(T exp);
        public abstract void SetDesignerData(List<string> data);

        private Dictionary<string, T> playObjects = new Dictionary<string, T>();
        /// <summary>
        /// This holds all of the "objects" -- the things that actually determine the geometry of the play.
        /// (operational definition: everything but actions and conditions)
        /// Includes both geometry, like lines and segments, but also values, like bools and ints.
        /// The keys are the names of the items
        /// </summary>
        public Dictionary<string, T> PlayObjects
        {
            get { return playObjects; }
        }
        /*private Dictionary<string, Vector2> designerPositions = new Dictionary<string,Vector2>();
        public Dictionary<string, Vector2> DesignerPositions
        {
            get { return designerPositions; }
        }*/
        public List<T> getAllObjects()
        {
            return new List<T>(playObjects.Values);
        }
    }
}
