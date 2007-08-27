using System;
using System.Collections.Generic;
using System.Text;

namespace RobocupPlays
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


        private Dictionary<string, T> definedObjects = new Dictionary<string, T>();
        public Dictionary<string, T> definitionDictionary
        {
            get { return definedObjects; }
        }
        public List<T> getAllObjects()
        {
            List<T> rtn = new List<T>();
            foreach (KeyValuePair<string, T> pair in definedObjects)
            {
                rtn.Add(pair.Value);
            }
            return rtn;
        }
    }
}
