using System;
using System.Collections.Generic;
using System.Text;

namespace Robocup.Plays
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
        public bool IsFunction
        {
            get { return function != null; }
        }
        public bool UsesFunction(string functionName)
        {
            if (!IsFunction)
                return false;
            return theFunction.Name == functionName;
        }
        object[] arguments=null;
        public object[] Arguments
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

        /// <summary>
        /// Gets the value, but does no caching.
        /// </summary>
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
        /// <summary>
        /// Gets the value, using caching.
        /// </summary>
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

        /// <param name="args">Each object in args is interpreted as a raw value if it is not of type Expression,
        /// otherwise it is treated like an Expression</param>
        public Expression(Function f, params object[] args)
        {
            this.function = f;
            this.arguments = args;
        }
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

        public interface Factory<T> where T:Expression {
            T Create(object value);
            T Create(Function f, object[] args);
            List<Function> Functions();
        }
    }
}
