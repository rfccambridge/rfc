using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Robocup.Infrastructure;

namespace RobocupPlays
{
    internal class DesignerPlay : Play<DesignerExpression>
    {
        #region Lists of Expressions
        /*List<DesignerExpression> points = new List<DesignerExpression>();
        public IList<DesignerExpression> Points
        {
            get { return points.AsReadOnly(); }
        }
        List<DesignerExpression> lines = new List<DesignerExpression>();
        public IList<DesignerExpression> Lines
        {
            get { return lines.AsReadOnly(); }
        }
        List<DesignerRobotDefinition> definitions = new List<DesignerRobotDefinition>();*/
        public IList<DesignerRobotDefinition> Definitions
        {
            get
            {
                List<DesignerRobotDefinition> rtn = new List<DesignerRobotDefinition>();
                foreach (DesignerExpression exp in Robots)
                {
                    DesignerRobot robot = (DesignerRobot)exp.StoredValue;
                    if (robot.getDefinition() != null)
                        rtn.Add(robot.getDefinition());
                }
                return rtn.AsReadOnly();
            }
        }
        /*List<DesignerExpression> circles = new List<DesignerExpression>();
        public IList<DesignerExpression> Circles
        {
            get { return circles.AsReadOnly(); }
        }
        List<DesignerExpression> intermediates = new List<DesignerExpression>();*/
        /// <summary>
        /// Represents the other random objects that might be defined
        /// </summary>
        public IList<DesignerExpression> NonDisplayables
        {
            get { return null; }
        }
        /*/// <summary>
        /// Adds this expression to the intermediates that need to be saved;
        /// does nothing if this expression is already there.
        /// </summary>
        /// <param name="exp"></param>
        public void addIntermediate(DesignerExpression exp)
        {
            if (!getAllObjects().Contains(exp))
                intermediates.Add(exp);
        }*/
        private List<DesignerExpression> robots = new List<DesignerExpression>();
        /// <summary>
        /// DO NOT ADD ROBOTS TO THIS LIST.  Call addRobot() instead
        /// </summary>
        public override IList<DesignerExpression> Robots
        {
            get { return robots.AsReadOnly(); }
        }
        public override void addRobot(DesignerExpression obj)
        {
            if (obj.ReturnType != typeof(DesignerRobot))
                return;
            robots.Add(obj);
        }

        private DesignerBall b;
        public DesignerBall Ball
        {
            get { return b; }
            set { b = value; }
        }
        private DesignerExpression fake_ball = new DesignerExpression(new DesignerBall(new Vector2(0, 0)));
        public override DesignerExpression TheBall { get { return fake_ball; } }
        #endregion


        public void AddPlayObject(DesignerExpression exp, string name)
        {
            //if (!typeof(PlayObject).IsAssignableFrom(exp.ReturnType))
            //    throw new ApplicationException("You can only add PlayObjects to a play!");

            exp.Name = name;
            definitionDictionary.Add(name, exp);
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
            else if (typeof(DesignerRobot).IsAssignableFrom(exp.ReturnType))
            {
                name = "robot" + numobjects++;
            }
            else
                throw new ApplicationException("Unrecognized type in Play.AddPlayObject(): " + exp.ReturnType.Name);
            if (definitionDictionary.ContainsKey(name))
                //this means someone has added an object, not through this function,
                //and there's not much we can do but try to find another name
                AddPlayObject(exp);
            else
                AddPlayObject(exp, name);
        }
        public void delete(DesignerExpression exp)
        {
            exp.Delete();
            definitionDictionary.Remove(exp.Name);
            robots.Remove(exp);
            Conditions.Remove(exp);
            Actions.Remove(exp);
            List<DesignerExpression> allExpressions = getAllObjects();
            foreach (DesignerExpression expr in allExpressions)
            {
                if (expr.shouldDelete())
                    delete(expr);
            }
        }
        /// <summary>
        /// Replaces all occurrences of toReplace with replaceWith that occur anywhere in the
        /// expression tree of current.
        /// </summary>
        private bool replaceArg(DesignerExpression current, DesignerExpression toReplace,
            DesignerExpression replaceWith)
        {
            if (!current.IsFunction)
                return false; ;
            bool replaced = false;
            for (int i = 0; i < current.theFunction.NumArguments; i++)
            {
                if (current.getArgument(i) == toReplace)
                {
                    current.setArgument(i, replaceWith);
                    replaced = true;
                }
                else
                {
                    replaced |= replaceArg(current.getArgument(i), toReplace, replaceWith);
                }
            }
            return replaced;
        }
        private void CreateNewRobot(string name, Vector2 position, DesignerExpression oldRobot)
        {
            bool ours = ((TeamCondition)oldRobot.getArgument(0).StoredValue).maybeOurs();
            DesignerRobot robot = new DesignerRobot(position, ours, name);
            DesignerExpression new_exp = new DesignerExpression(robot);
            new_exp.Name = name;
            robot.setDefinition(new ClosestDefinition(new_exp, oldRobot.getArgument(1)));
            addRobot(new_exp);
            definitionDictionary.Remove(name);
            // The other loaded items will have been built with the fake value;
            // go back and replace them now.
            foreach (DesignerExpression exp in definitionDictionary.Values)
            {
                replaceArg(exp, oldRobot, new_exp);
            }
            definitionDictionary.Add(name, new_exp);
        }
        public override void SetDesignerData(List<string> data)
        {
            List<DesignerExpression> oldRobots = new List<DesignerExpression>(robots);
            robots.Clear();
            bool foundball = false;
            foreach (string s in data)
            {
                Vector2 position = Vector2.Parse(s.Substring(s.IndexOf(' ')));
                string name = s.Substring(0, s.IndexOf(' '));
                if (name == "ball")
                {
                    foundball = true;
                    b = new DesignerBall(position);
                    //AddPlayObject(new DesignerExpression(Function.getFunction("pointof"), b));
                }
                else
                {
                    DesignerExpression loaded = definitionDictionary[name];
                    oldRobots.Remove(loaded);
                    CreateNewRobot(name, position, loaded);
                }
            }
            Random r = new Random();
            foreach (DesignerExpression exp in oldRobots)
            {
                CreateNewRobot(exp.Name, new Vector2((float)r.NextDouble() * 5f - 2.5f,
                    (float)r.NextDouble() * 4f - 2f), exp);
            }
            if (!foundball)
                b = new DesignerBall(new Vector2((float)r.NextDouble() * 2 - 1, (float)r.NextDouble() * 2 - 1));
            bool needball = false;
            foreach (DesignerExpression exp in definitionDictionary.Values)
            {
                needball |= replaceArg(exp, fake_ball, new DesignerExpression(b));
            }
            if (!foundball && !needball)
                b = null;
            else if (!foundball && needball)
            {
                bool needballpoint = true;
                foreach (DesignerExpression exp in definitionDictionary.Values)
                {
                    if (exp.IsFunction && exp.theFunction.Name == "pointof"
                        && !exp.getArgument(0).IsFunction && exp.getArgument(0).StoredValue==b)
                    {
                        needballpoint = false;
                        break;
                    }
                }
                if (needballpoint)
                    AddPlayObject(new DesignerExpression(Function.getFunction("pointof"), b));
            }
            fake_ball = null;
        }
        public string Save()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Metadata:");
            sb.AppendLine("type " + PlayType.ToString());
            //sb.AppendLine("ID " + new Random().Next());
            if (Name != null)
                sb.AppendLine("name " + Name);
            sb.AppendLine("score " + Score);
            sb.AppendLine("Objects:");
            foreach (DesignerExpression exp in getAllObjects())
            {
                if (typeof(PlayBall).IsAssignableFrom(exp.ReturnType))
                    continue;
                if (exp.Name == "")
                {
                    System.Windows.Forms.MessageBox.Show("unnamed expression");
                    return null;
                }
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
            sb.AppendLine("DesignerData:");
            if (Ball != null)
                sb.AppendLine("ball " + Ball.getPoint());
            foreach (DesignerExpression exp in Robots)
            {
                DesignerRobot robot = (DesignerRobot)exp.StoredValue;
                if (robot.getRobotDefinition() == "<undefined>")
                {
                    System.Windows.Forms.MessageBox.Show("undefined robot");
                    return null;
                }
                sb.AppendLine(exp.Name + ' ' + robot.getPoint());
            }
            //sb.Insert(0, (sb.ToString().GetHashCode()+"\n"));
            return sb.ToString();
        }
    }
}
