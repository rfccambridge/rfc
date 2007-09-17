using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Robocup.Infrastructure;
using Robocup.Geometry;

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
            get {
                List<DesignerExpression> rtn = new List<DesignerExpression>();
                foreach (DesignerExpression exp in getAllObjects())
                {
                    Type t = exp.ReturnType;
                    if (typeof(Line).IsAssignableFrom(t) ||
                        typeof(Circle).IsAssignableFrom(t) ||
                        typeof(Vector2).IsAssignableFrom(t) ||
                        typeof(Robot).IsAssignableFrom(t))
                    {
                        continue;
                    }
                    rtn.Add(exp);
                }
                return rtn;
            }
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
            PlayObjects.Add(name, exp);
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
            if (PlayObjects.ContainsKey(name))
                //this means someone has added an object, not through this function,
                //and there's not much we can do but try to find another name
                AddPlayObject(exp);
            else
                AddPlayObject(exp, name);
        }
        private bool shouldDelete(DesignerExpression exp, Dictionary<DesignerExpression, bool> todelete)
        {
            if (todelete.ContainsKey(exp))
                return todelete[exp];
            if (!exp.IsFunction)
                return false;
            bool shouldDeleteThis = false;
            for (int i = 0; i < exp.theFunction.NumArguments; i++)
            {
                shouldDeleteThis |= shouldDelete(exp.getArgument(i), todelete);
            }
            todelete.Add(exp, shouldDeleteThis);
            return shouldDeleteThis;
        }
        public void delete(DesignerExpression exp)
        {
            Dictionary<DesignerExpression, bool> todelete = new Dictionary<DesignerExpression, bool>();
            todelete.Add(exp, true);
            foreach (DesignerExpression expr in getAllObjects())
            {
                if (shouldDelete(expr, todelete))
                {
                    PlayObjects.Remove(expr.Name);
                    robots.Remove(expr);
                }
            }
            List<DesignerExpression> conditionsToRemove = new List<DesignerExpression>();
            foreach (DesignerExpression expr in Conditions)
            {
                if (shouldDelete(expr, todelete))
                    conditionsToRemove.Add(expr);
            }
            foreach (DesignerExpression expr in conditionsToRemove)
            {
                Conditions.Remove(expr);
            }
            List<DesignerExpression> actionsToRemove = new List<DesignerExpression>();
            foreach (DesignerExpression expr in Actions)
            {
                if (shouldDelete(expr, todelete))
                    actionsToRemove.Add(expr);
            }
            foreach (DesignerExpression expr in actionsToRemove)
            {
                Actions.Remove(expr);
            }
            /*exp.Delete();
            PlayObjects.Remove(exp.Name);
            robots.Remove(exp);
            Conditions.Remove(exp);
            Actions.Remove(exp);
            List<DesignerExpression> allExpressions = getAllObjects();
            allExpressions.AddRange(Conditions);
            allExpressions.AddRange(Actions);
            foreach (DesignerExpression expr in allExpressions)
            {
                if (expr.shouldDelete())
                {
                    PlayObjects.Remove(exp.Name);
                    robots.Remove(exp);
                    Conditions.Remove(exp);
                    Actions.Remove(exp);
                }
            }*/
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
            PlayObjects.Remove(name);
            // The other loaded items will have been built with the fake value;
            // go back and replace them now.
            foreach (DesignerExpression exp in PlayObjects.Values)
            {
                replaceArg(exp, oldRobot, new_exp);
            }
            PlayObjects.Add(name, new_exp);
        }

        // The loader loads some rather bogus functions into the robots and balls, as placeholders.
        // At the end, it calls this function with the set of designer data, and it's this function's job
        // to go back and replace the placeholders with real data.
        public override void SetDesignerData(List<string> data)
        {
            // We need to keep track of which robots don't have actual data for them,
            // so we can add some (random) data for them later.
            List<DesignerExpression> oldRobots = new List<DesignerExpression>(robots);
            // All the robots in here are placeholders
            robots.Clear();
            // Whether or not there's a position for the ball
            bool foundball = false;
            foreach (string s in data)
            {
                Vector2 position = Vector2.Parse(s.Substring(s.IndexOf(' ')));
                string name = s.Substring(0, s.IndexOf(' '));
                if (name == "ball")
                {
                    //If there is info, we can assume that there is also a point for the ball
                    foundball = true;
                    b = new DesignerBall(position);
                    DesignerExpression new_ball = new DesignerExpression(b);
                    foreach (DesignerExpression exp in PlayObjects.Values)
                    {
                        replaceArg(exp, fake_ball, new_ball);
                    }
                    foreach (DesignerExpression exp in Conditions)
                    {
                        replaceArg(exp, fake_ball, new_ball);
                    }
                    foreach (DesignerExpression exp in Actions)
                    {
                        replaceArg(exp, fake_ball, new_ball);
                    }
                }
                else
                {
                    DesignerExpression loaded = PlayObjects[name];
                    oldRobots.Remove(loaded);
                    CreateNewRobot(name, position, loaded);
                }
            }
            // For each of the robots we didn't find, give it a random position:
            Random r = new Random();
            foreach (DesignerExpression exp in oldRobots)
            {
                CreateNewRobot(exp.Name, new Vector2((float)r.NextDouble() * 5f - 2.5f,
                    (float)r.NextDouble() * 4f - 2f), exp);
            }
            // If we didnt find the ball, let's temporarily create one (in case we need it)
            if (!foundball)
            {
                b = new DesignerBall(new Vector2((float)r.NextDouble() * 2 - 1, (float)r.NextDouble() * 2 - 1));
                // Lets see if anything needs it:
                bool needball = false;
                foreach (DesignerExpression exp in PlayObjects.Values)
                {
                    needball |= replaceArg(exp, fake_ball, new DesignerExpression(b));
                }
                // If nothing needed it, go ahead and remove it
                if (!needball)
                {
                    b = null;
                }
                // else, let's see if we need to add the (pointof ball) point:
                else
                {
                    bool needballpoint = true;
                    foreach (DesignerExpression exp in PlayObjects.Values)
                    {
                        if (exp.UsesFunction("pointof") && !exp.getArgument(0).IsFunction && exp.getArgument(0).StoredValue == b)
                        {
                            needballpoint = false;
                            break;
                        }
                    }
                    if (needballpoint)
                        AddPlayObject(new DesignerExpression(Function.getFunction("pointof"), b));
                }
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
