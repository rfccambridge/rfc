using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Robocup.Infrastructure;

namespace RobocupPlays
{
    class DesignerExpression : Expression
    {
        public bool argDefined(int numArg)
        {
            return Arguments[numArg] != null;
        }
        public bool fullyDefined()
        {
            for (int i = 0; i < Arguments.Length; i++)
            {
                if (!argDefined(i))
                    return false;
            }
            return true;
        }
        public Label[] getLabels()
        {
            if (!IsFunction)
                throw new ApplicationException("uhh...what to do here, in getLabels()?");
            string[] Description=theFunction.Description;
            Type[] ArgTypes = theFunction.ArgTypes;

            Label[] rtn = new Label[ArgTypes.Length + Description.Length];
            for (int i = 0; i < Description.Length; i++)
            {
                rtn[i * 2] = new Label();
                rtn[i * 2].Text = Description[i];
                rtn[i * 2].Size = rtn[i * 2].GetPreferredSize(new Size());
            }
            for (int i = 0; i < ArgTypes.Length; i++)
            {
                rtn[i * 2 + 1] = new Link(i, ArgTypes[i]);
                if (Arguments[i] == null)
                {
                    rtn[i * 2 + 1].Text = Function.getStringFromType(ArgTypes[i]);
                }
                else
                {
                    rtn[i * 2 + 1].Text = Arguments[i].ToString();
                }
                rtn[i * 2 + 1].Size = rtn[i * 2 + 1].GetPreferredSize(new Size());

            }
            return rtn;
        }

        public DesignerExpression(Function f, int numArgs) : base(f,new object[numArgs])
        {
            /*this.function = f;
            this.arguments = new object[numArgs];*/
        }
        public DesignerExpression(Function f, params object[] args) : base(f, args) { }
        public DesignerExpression(object o) : base(o) { }
        public void setArgument(int argNumber, object newArgument)
        {
            Arguments[argNumber] = newArgument;
        }
        public DesignerExpression getArgument(int argNumber)
        {
            object o = Arguments[argNumber];
            if (o == null)
                return null;
            if (!(o is DesignerExpression))
                o = new DesignerExpression(o);
            return (DesignerExpression)o;
            //return (DesignerExpression)Arguments[argNumber];
        }


        private bool highlighted;

        public bool Highlighted
        {
            get { return highlighted; }
            set { highlighted = value; }
        }

        private bool deleted;
        public bool Deleted
        {
            get { return deleted; }
        }
        public void Delete()
        {
            this.deleted = true;
        }
        public bool shouldDelete()
        {
            if (!IsFunction)
                return Deleted;
            foreach (object exp in Arguments)
            {
                if (exp is DesignerExpression && ((DesignerExpression)exp).Deleted)
                    return true;
            }
            return false;
        }
        public override string getDefinition()
        {
            if (ReturnType == typeof(DesignerRobot))
            {
                DesignerRobot r = (DesignerRobot)StoredValue;
                return r.getRobotDefinition();
            }
            return base.getDefinition();
        }

        internal class Factory : Expression.Factory<DesignerExpression>
        {
            public DesignerExpression Create(object value)
            {
                return new DesignerExpression(value);
            }

            public DesignerExpression Create(Function f, object[] args)
            {
                return new DesignerExpression(f, args);
            }

            public List<Function> Functions()
            {
                List<Function> rtn = new List<Function>();
                rtn.Add(new Function("closest", "closest", typeof(DesignerRobot), new Type[] { typeof(TeamCondition), typeof(Vector2), typeof(RobotAssignmentType) }, "the robot on ~ closest to ~", delegate(EvaluatorState state, object[] objects)
                {
                    return 0;
                }));
                rtn.Add(new Function("closest-with-tags", "closest-with-tags", typeof(DesignerRobot), new Type[] { typeof(TeamCondition), typeof(Vector2), typeof(RobotAssignmentType), typeof(string) }, "the robot on ~ closest to ~, with comma-separated tags ~", delegate(EvaluatorState state, object[] objects)
                {
                    return 0;
                }));
                return rtn;
            }
        }
    }
}
