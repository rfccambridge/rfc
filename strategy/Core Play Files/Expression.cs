using System;
using System.Collections.Generic;
using System.Text;

namespace RobocupPlays
{
    public class Expression
    {
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
	
        Function function=null;
        public Function theFunction
        {
            get { return function; }
        }
        object[] arguments=null;
        protected object[] Arguments
        {
            get { return arguments; }
        }

        object value=null;
        public object StoredValue
        {
            get
            {
#if DEBUG
                if (IsFunction)
                    throw new ApplicationException("You tried to get the stored value of an expression that isn't a stored value!");
#endif
                return value;
            }
        }

        public bool IsFunction
        {
            get { return function != null; }
        }

        public Type ReturnType
        {
            get
            {
                if (IsFunction)
                    return function.ReturnType;
                else
                    return value.GetType();
            }
        }

        private object calcValue(EvaluatorState state,int tick)
        {
            if (IsFunction)
            {
                object[] evaluatedArguments = new object[arguments.Length];
                for (int i = 0; i < arguments.Length; i++)
                {
                    Expression e = arguments[i] as Expression;
                    if (e != null)
                        evaluatedArguments[i] = e.getValue(tick,state);
                    else
                        evaluatedArguments[i] = arguments[i];
                    //evaluatedArguments[i] = e.getValue(tick);
                }
                return function.Run(state, evaluatedArguments);
            }
            else
                return StoredValue;
        }
        int lasttick = int.MinValue;
        int lasthash = -1;
        object savedVal;
        public object getValue(int tick, EvaluatorState state)
        {
            if (!IsFunction)
                return StoredValue;
            if (tick > lasttick || lasthash!=savedVal.GetHashCode())
            {
                savedVal = calcValue(state,tick);
                lasttick = tick;
                lasthash = savedVal.GetHashCode();
            }
            return savedVal;
        }

        public Expression(Function f, params object[] args)
        {
            this.function = f;
            this.arguments = args;
            /*for (int i = 0; i < this.arguments.Length; i++)
            {
                if (! (this.arguments[i] is Expression))
                    this.arguments[i] = new Expression(this.arguments[i]);
            }*/
        }
        /*public Expression(Function f, int numArgs)
        {
            this.function = f;
            this.arguments = new object[numArgs];
        }*/
        /*public void setArgument(int argNumber, object newArgument)
        {
            this.arguments[argNumber] = newArgument;
        }*/
        public Expression(object value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            if (Name != null)
                return Name;
            else
                return getDefinition();
        }
        public virtual string getDefinition()
        {
            if (IsFunction)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append('(');
                sb.Append(function.Name);
                for (int i = 0; i < function.NumArguments; i++)
                {
                    sb.Append(' ');
                    sb.Append(arguments[i].ToString());
                }
                sb.Append(')');
                return sb.ToString();
            }
            else
            {
                if (typeof(PlayBall).IsAssignableFrom(this.ReturnType))
                    return "ball";
                return value.ToString();
            }
        }
    }
}
