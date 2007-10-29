using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Robocup.Core;

namespace Robocup.Plays
{
    static class ExpressionUtils
    {
        static public Label[] getLabels(DesignerExpression exp)
        {
            if (!exp.IsFunction)
                throw new ApplicationException("uhh...what to do here, in getLabels()?");
            string[] Description = exp.theFunction.Description;
            Type[] ArgTypes = exp.theFunction.ArgTypes;

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
                if (exp.getArgument(i) == null)
                {
                    rtn[i * 2 + 1].Text = Function.getStringFromType(ArgTypes[i]);
                }
                else
                {
                    rtn[i * 2 + 1].Text = exp.getArgument(i).ToString();
                }
                rtn[i * 2 + 1].Size = rtn[i * 2 + 1].GetPreferredSize(new Size());

            }
            return rtn;
        }
    }
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

        public DesignerExpression(Function f, params object[] args) : base(f, args) { }
        public DesignerExpression(object o) : base(o) { }
        public void setArgument(int argNumber, object newArgument)
        {
            if (Arguments.GetType() == typeof(DesignerExpression).MakeArrayType() &&
                newArgument.GetType() != typeof(DesignerExpression))
                newArgument = new DesignerExpression(newArgument);
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
        }


        private bool highlighted;

        public bool Highlighted
        {
            get { return highlighted; }
            set { highlighted = value; }
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
                rtn.Add(new Function("closest", "closest", "the robot on ~ closest to ~", typeof(DesignerRobot), new Type[] { typeof(TeamCondition), typeof(Vector2), typeof(RobotAssignmentType) }, delegate(EvaluatorState state, object[] objects)
                {
                    return 0;
                }));
                rtn.Add(new Function("closest-with-tags", "closest-with-tags", "the robot on ~ closest to ~, with comma-separated tags ~", typeof(DesignerRobot), new Type[] { typeof(TeamCondition), typeof(Vector2), typeof(RobotAssignmentType), typeof(string) }, delegate(EvaluatorState state, object[] objects)
                {
                    return 0;
                }));
                return rtn;
            }
        }
    }
}
