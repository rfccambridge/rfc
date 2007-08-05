using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Robocup.Infrastructure;

namespace RobocupPlays
{
    [Serializable]
    internal class DesignerPlay : Play<DesignerExpression>
    {
        #region Lists of Expressions
        List<DesignerExpression> robots = new List<DesignerExpression>();
        public override IList<DesignerExpression> Robots
        {
            get { return robots; }
        }
        List<DesignerExpression> points = new List<DesignerExpression>();
        public IList<DesignerExpression> Points
        {
            get { return points.AsReadOnly(); }
        }
        List<DesignerExpression> lines = new List<DesignerExpression>();
        public IList<DesignerExpression> Lines
        {
            get { return lines.AsReadOnly(); }
        }
        List<DesignerRobotDefinition> definitions = new List<DesignerRobotDefinition>();
        public IList<DesignerRobotDefinition> Definitions
        {
            get { return definitions; }
        }
        List<DesignerExpression> circles = new List<DesignerExpression>();
        public IList<DesignerExpression> Circles
        {
            get { return circles.AsReadOnly(); }
        }
        List<DesignerExpression> intermediates = new List<DesignerExpression>();
        /// <summary>
        /// Represents the other random objects that might be defined
        /// </summary>
        public IList<DesignerExpression> Intermediates
        {
            get { return intermediates.AsReadOnly(); }
        }
        /// <summary>
        /// Adds this expression to the intermediates that need to be saved;
        /// does nothing if this expression is already there.
        /// </summary>
        /// <param name="exp"></param>
        public void addIntermediate(DesignerExpression exp)
        {
            if (!getAllObjects().Contains(exp))
                intermediates.Add(exp);
        }

        private DesignerBall b;
        public DesignerBall Ball
        {
            get { return b; }
            set { b = value; }
        }
        #endregion


        public void AddPlayObject(DesignerExpression exp, string name)
        {
            //if (!typeof(PlayObject).IsAssignableFrom(exp.ReturnType))
            //    throw new ApplicationException("You can only add PlayObjects to a play!");

            exp.Name = name;
            if (typeof(Circle).IsAssignableFrom(exp.ReturnType))
            {
                circles.Add(exp);
            }
            else if (typeof(Line).IsAssignableFrom(exp.ReturnType))
            {
                lines.Add(exp);
            }
            else if (typeof(Vector2).IsAssignableFrom(exp.ReturnType))
            {
                points.Add(exp);
            }
            else if (typeof(PlayRobot).IsAssignableFrom(exp.ReturnType))
            {
                //keep the robot name for itself and the expression name the same:
                ((DesignerRobot)exp.StoredValue).rename(name);
                robots.Add(exp);
            }
            else
                throw new ApplicationException("Unrecognized type in Play.AddPlayObject(): " + exp.ReturnType.Name);
        }

        int numobjects = 0;
        public void AddPlayObject(DesignerExpression exp)
        {
            string name = null;
            if (typeof(Circle).IsAssignableFrom(exp.ReturnType))
            {
                name = "circle" + numobjects++;
            }
            else if (typeof(Line).IsAssignableFrom(exp.ReturnType))
            {
                name = "line" + numobjects++;
            }
            else if (typeof(Vector2).IsAssignableFrom(exp.ReturnType))
            {
                name = "point" + numobjects++;
            }
            else if (typeof(PlayRobot).IsAssignableFrom(exp.ReturnType))
            {
                name = "robot" + numobjects++;
            }
            else
                throw new ApplicationException("Unrecognized type in Play.AddPlayObject(): " + exp.ReturnType.Name);
            AddPlayObject(exp, name);
        }
        private List<DesignerExpression> getAllExpressions()
        {
            List<DesignerExpression> rtn = new List<DesignerExpression>();
            rtn.AddRange(getAllObjects());
            rtn.AddRange(Conditions);
            rtn.AddRange(Actions);
            //rtn.AddRange(definitions.ToArray());
            return rtn;
        }
        protected override List<DesignerExpression> getAllObjects()
        {
            List<DesignerExpression> rtn = new List<DesignerExpression>();
            rtn.AddRange(Robots);
            rtn.AddRange(Points);
            rtn.AddRange(Lines);
            rtn.AddRange(Circles);
            rtn.AddRange(Intermediates);
            return rtn;
        }
        public void delete(DesignerExpression exp)
        {
            exp.Delete();
            robots.Remove(exp);
            Conditions.Remove(exp);
            Actions.Remove(exp);
            points.Remove(exp);
            lines.Remove(exp);
            //definitions.Remove(exp);
            circles.Remove(exp);
            List<DesignerExpression> allExpressions = getAllExpressions();
            foreach (DesignerExpression expr in allExpressions)
            {
                if (expr.shouldDelete())
                    delete(expr);
            }
        }

        public List<DesignerExpression> getClickables()
        {
            List<DesignerExpression> rtn = new List<DesignerExpression>();
            rtn.AddRange(Points);
            rtn.AddRange(Lines);
            rtn.AddRange(Circles);
            rtn.AddRange(Robots);
            return rtn;
        }
        /*public string Save()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Metadata:");
            sb.AppendLine("type " + PlayType.ToString());
            sb.AppendLine("ID " + new Random().Next());
            sb.AppendLine("score " + Score);
            sb.AppendLine("Objects:");
            foreach (DesignerExpression exp in getAllObjects())
            {
                sb.AppendLine(exp.Name + ' ' + exp.getDefinition());
            }
            sb.AppendLine("Conditions:");
            foreach (DesignerExpression exp in Conditions)
            {
                sb.AppendLine(exp.ToString());
            }
            sb.AppendLine("Actions:");
            foreach (DesignerExpression exp in Actions)
            {
                sb.AppendLine(exp.ToString());
            }
            //sb.Insert(0, (sb.ToString().GetHashCode()+"\n"));
            return sb.ToString();
        }*/
    }
}
