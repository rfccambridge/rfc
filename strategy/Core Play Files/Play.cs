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
        public abstract IList<T> Robots { get;}


        protected abstract List<T> getAllObjects();

        public string Save()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Metadata:");
            sb.AppendLine("type " + PlayType.ToString());
            //sb.AppendLine("ID " + new Random().Next());
            if (Name!=null)
                sb.AppendLine("name "+Name);
            sb.AppendLine("score " + Score);
            sb.AppendLine("Objects:");
            foreach (T exp in getAllObjects())
            {
                if (typeof(PlayBall).IsAssignableFrom(exp.ReturnType))
                    continue;
                sb.AppendLine(exp.Name + ' ' + exp.getDefinition());
            }
            sb.AppendLine("Conditions:");
            foreach (T exp in Conditions)
            {
                sb.AppendLine(exp.ToString());
            }
            sb.AppendLine("Actions:");
            foreach (T exp in Actions)
            {
                sb.AppendLine(exp.ToString());
            }
            //sb.Insert(0, (sb.ToString().GetHashCode()+"\n"));
            return sb.ToString();
        }
    }
}
