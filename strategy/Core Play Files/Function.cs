using System;
using System.Collections.Generic;
using System.Text;

namespace RobocupPlays
{
    public class TeamCondition
    {
        string def = "friendly";
        public override string ToString()
        {
            return def;
        }
        public void setString(string s)
        {
            def = s;
        }
        public TeamCondition(string s){
            this.def=s;
        }
        public TeamCondition() : this(strings[0]) { }
        static public TeamCondition Parse(string s)
        {
#if DEBUG
            bool ok = false;
            foreach (string name in strings)
            {
                if (name == s)
                    ok = true;
            }
            if (!ok)
                throw new FormatException("Unable to cast " + s + " into a TeamCondition");
#endif
            return new TeamCondition(s);
        }
        public bool maybeOurs()
        {
            return def != "enemy";
        }
        public bool maybeTheirs()
        {
            return def != "friendly";
        }
        static public bool operator== (TeamCondition t1, TeamCondition t2){
            return t1.def==t2.def;
        }
        static public bool operator!= (TeamCondition t1, TeamCondition t2){
            return t1.def!=t2.def;
        }
        public override bool Equals(object obj)
        {
            TeamCondition t = obj as TeamCondition;
            if (obj == null)
                return false;
            else
                return this == t;
        }
        public override int GetHashCode()
        {
            return def.GetHashCode();
        }
        static public readonly TeamCondition FRIENDLY=new TeamCondition("friendly"),ENEMY=new TeamCondition("enemy"),EITHER=new TeamCondition("either");
        static public readonly string[] strings = new string[] { "friendly", "enemy", "either" };
    }
    public static class FloatComparer
    {
        public static bool compare(float f1, float f2, string comparison)
        {
            switch (comparison)
            {
                case "<":
                    return f1 < f2;
                case "<=":
                    return f1 <= f2;
                case "=":
                    return f1 == f2;
                case ">=":
                    return f1 >= f2;
                case ">":
                    return f1 > f2;
                default:
                    throw new ApplicationException("Unable to compare two floats with the comparison " + comparison);
            }
        }
        public static bool compare(float f1, float f2, GreaterLessThan glt)
        {
            return compare(f1, f2, glt.ToString());
        }
    }
    public class GreaterLessThan
    {
        string def = "<=";
        public override string ToString()
        {
            return def;
        }
        public void setString(string s)
        {
            def = s;
        }
        static public GreaterLessThan Parse(string s)
        {
            bool good = false;
            for (int i = 0; i < strings.Length; i++)
            {
                if (s == strings[i])
                    good = true;
            }
            if (good == false)
                throw new ApplicationException("Invalid GreaterLessThan string to parse");
            GreaterLessThan rtn = new GreaterLessThan();
            rtn.setString(s);
            return rtn;
        }
        static public readonly string[] strings = new string[] { "<", "<=", "=", ">=", ">" };
    }

    public delegate object FunctionRunDelegate(EvaluatorState state, params object[] objects);

    public partial class Function
    {
        /*static public explicit operator ValueObject(Function f){
            return new ValueObject(f);
        }*/
        private Type[] argTypes;
        public Type[] ArgTypes
        {
            get { return argTypes; }
        }
        public int NumArguments
        {
            get { return ArgTypes.Length; }
        }
        private string[] description;
        public string[] Description
        {
            get { return description; }
        }
        private string name;
        public string Name
        {
            get { return name; }
        }
        private string longName;
        public string LongName
        {
            get { return longName; }
        }



        protected const char splitChar = '~';
        protected Function(Function f)
        {
            this.name = f.name;
            this.longName = f.longName;
            this.returnType = f.returnType;
            this.run = f.run;
            this.argTypes = f.argTypes;
            this.description = f.description;
        }
        public Function(string name, string longname, Type returnType, Type[] argTypes, string description, FunctionRunDelegate run)
        {
            this.name = name;
            this.longName = longname;
            this.returnType = returnType;
            this.argTypes = argTypes;
            this.run = run;
            this.description = description.Split(new char[] { splitChar });
        }
        private Type returnType;
        public Type ReturnType
        {
            get { return returnType; }
        }
        private FunctionRunDelegate run;
        public FunctionRunDelegate Run
        {
            get { return run; }
        }
        public override bool Equals(object obj)
        {
            Function f = obj as Function;
            if (f == null)
                return false;
            return f.Name == this.name;
            //return (f.ReturnType == this.ReturnType && f.Name == this.Name);
        }
        public override int GetHashCode()
        {
            return ((string)(Name + LongName + Description + ReturnType.ToString())).GetHashCode();
        }
    }
}
