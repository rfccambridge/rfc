using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Drawing;
using Robocup.Geometry;
using Robocup.Core;
using System.Reflection;

namespace Robocup.Plays
{
    abstract public class PlayableLoader<P, E>
        where P : Playable<E>, new()
        where E : Expression
    {
        protected P playable;
        protected Expression.Factory<E> factory;
        protected Dictionary<string, string> remainingDefinitions;


        /// <summary>
        /// Returns a new playable entity, loaded from the string that is the play/tactic (not the filename).
        /// 
        /// As for the objects, they are guaranteed to be added in a topologically sorted way:
        /// if A depends on B, then B is added first.  (Currently 4/30, this only matters for the robots)
        /// </summary>s
        /// 
        public P load(string s)
        {
            return load(s, "UNNAMED?");
        }
        protected System.Collections.Specialized.ListDictionary definitionLists;
        protected void parseDefinitions(string[] lines, bool throwOnMiss)
        {
            List<string> curList = null;
            for (int stringnum = 0; stringnum < lines.Length; stringnum++)
            {
                string line = lines[stringnum].Trim();
                if (line[0] == '#')
                    continue;
                //if the last character is a colon,
                //then it's a label for something:
                if (line[line.Length - 1] == ':')
                {
                    string label = line.Trim(':').ToLower();
                    //bool found = definitionLists.TryGetValue(line.Trim(':').ToLower(), out curList);
                    if (!definitionLists.Contains(label)) //label may be processed by descendants
                    {
                        if (throwOnMiss)
                            throw new ApplicationException("Could not recognize label \"" + line.Trim(':').ToLower() + "\"");
                        curList = null;
                        continue;
                    }
                    curList = (List<string>)definitionLists[label];
                }
                else
                    if (curList != null)
                        curList.Add(line);
            }
        }
        public virtual P load(string s, string defaultName)
        {
            Function.AddFunctions(factory.Functions());

            playable.Name = defaultName;

            s = s.Replace("#ml", "");

            string[] lines = s.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            definitionLists.Add("objects", new List<string>());
            definitionLists.Add("metadata", new List<string>());
            definitionLists.Add("actions", new List<string>());
            definitionLists.Add("designerdata", new List<string>());

            parseDefinitions(lines, false);

            remainingDefinitions = new Dictionary<string, string>();
            foreach (string def in (List<string>)definitionLists["objects"])
            {
#if DEBUG
                if (def == "<undefined>")
                    throw new ApplicationException("Silly you, you left one of the robots undefined");
#endif
                string name = def.Substring(0, def.IndexOf(' '));
                string definition = def.Substring(def.IndexOf(' ')).Trim();
                remainingDefinitions.Add(name, definition);
            }

            while (remainingDefinitions.Count != 0)
            {
                //this is so dirty....
                Dictionary<string, string>.KeyCollection.Enumerator enumerator = remainingDefinitions.Keys.GetEnumerator();
                enumerator.MoveNext();
                string nextname = enumerator.Current;
                string definition;
                remainingDefinitions.TryGetValue(nextname, out definition);
                remainingDefinitions.Remove(nextname);

                addToPlay(nextname, getObject(definition, typeof(object)));
            }
            foreach (string action in (List<string>)definitionLists["actions"])
            {
                playable.Actions.Add(getObject(action, typeof(ActionDefinition)));
            }

            foreach (string header in (List<string>)definitionLists["metadata"])
            {
                processMetadata(header);
            }

            playable.SetDesignerData((List<string>)definitionLists["designerdata"]);

            return playable;
        }

        /// <summary>
        /// Takes a string s, assumed to start and end with parenthesis, and splits it up into subexpressions.
        /// Ex: (line (robot robot1) point2) gives {"line","(robot robot1)","point2"}
        /// </summary>
        static protected string[] parse(string s)
        {
            //s = s.Trim('(', ')');
            //TODO is this really what I want?
            //TODO change TODO to NOTE?
            if (s[0] == '(')
                s = s.Substring(1, s.Length - 2);
            StringBuilder token = new StringBuilder();
            List<string> strings = new List<string>();

            int depth = 0;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == ' ' && depth == 0)
                {
                    if (token.Length != 0)
                    {
                        strings.Add(token.ToString());
                        token = new StringBuilder();
                    }
                    continue;
                }

                if (c == '(')
                {
                    depth++;
                }
                else if (c == ')')
                {
                    depth--;
                    if (depth == 0)
                    {
                        token.Append(c);
                        if (token.Length != 0)
                        {
                            strings.Add(token.ToString());
                            token = new StringBuilder();
                        }
                        continue;
                    }
                }
                token.Append(c);
            }
            if (token.Length != 0)
                strings.Add(token.ToString());

            return strings.ToArray();
        }

        abstract protected void processMetadata(string header);

        protected void addToPlay(string name, E obj)
        {
            obj.Name = name;
            playable.PlayObjects.Add(name, obj);
            playable.addRobot(obj);
        }

        protected E getObject(string definition, Type wantedType)
        {
            if (definition[0] == '(')
                return treatAsFunction(definition, wantedType);
            if (wantedType == typeof(string))
                //just create the new expression directly from the input (ie, don't remove quotes)
                return factory.Create(definition);
            if (definition == "ball")
                return playable.TheBall;

            //else, it's a name:
            E rtn;
            if (playable.PlayObjects.TryGetValue(definition, out rtn))
            {
                if (!wantedType.IsAssignableFrom(rtn.ReturnType))
                    throw new ApplicationException("Received an object of an unexpected type: expected " + wantedType.Name + ", but got " + rtn.ReturnType.Name);
                return rtn;
            }
            //if it hasn't been defined before, search through the remaining definitions for it:
            string name = definition;
            string newdefinition;
            if (remainingDefinitions.TryGetValue(name, out newdefinition))
            {
                remainingDefinitions.Remove(name);
                E obj = getObject(newdefinition, typeof(object));
                if (!wantedType.IsAssignableFrom(obj.ReturnType))
                    throw new ApplicationException("Received an object of an unexpected type: expected " + wantedType.Name + ", but got " + obj.ReturnType.Name);

                addToPlay(name, obj);
                return obj;
            }

            //if it's not a name, then try to parse it as the type that we're expecting:
            try
            {
                return factory.Create(wantedType.InvokeMember("Parse",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Static, null, null, new object[] { name }));
            }
            catch (MissingMethodException)
            {
                //it has no parse method
            }

            throw new ApplicationException("there are no remaining definitions for object " + definition + ".  most likely cause: typo");
        }

        protected E treatAsFunction(string definition, Type wantedType)
        {
            string[] strings = parse(definition);
            Function f = Function.getFunction(strings[0]);
            if (f == null)
                throw new ApplicationException("Could not find a function of the name \"" + strings[0] + "\"");
            if (!wantedType.IsAssignableFrom(f.ReturnType))
                throw new ApplicationException("Received an function of an unexpected return type: expected " + wantedType.Name + ", but got " + f.ReturnType.Name);
            if (strings.Length - 1 != f.NumArguments)
                throw new ApplicationException("The function \"" + f.Name + "\" expects " + f.NumArguments + " arguments, but received " + (strings.Length - 1));
            E[] objects = new E[f.NumArguments];
            for (int i = 0; i < f.NumArguments; i++)
            {
                objects[i] = getObject(strings[i + 1], f.ArgTypes[i]);
            }
            return factory.Create(f, objects);
        }
    }

    public class PlayLoader<P, T, E> : PlayableLoader<P, E>
        where P : Playable<E>, IPlay<E>, new()
        where T : Playable<E>, ITactic<E>, new()
        where E : Expression
    {
        P play
        {
            get { return (P)playable; }
            set { playable = value; }
        }

        Dictionary<string, T> tacticBook;

        public PlayLoader(Expression.Factory<E> factory, Dictionary<string, T> tacticBook = null)
        {
            //InterpreterFunctions.addFunctions();
            this.factory = factory;
            this.tacticBook = tacticBook;
        }


        override public P load(string s, string defaultName)
        {
            play = new P();

            definitionLists = new System.Collections.Specialized.ListDictionary();

            base.load(s, defaultName);

            string[] lines = s.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            definitionLists.Add("conditions", new List<string>());
            definitionLists.Add("tactics", new List<string>());

            parseDefinitions(lines, true);

            foreach (string condition in (List<string>)definitionLists["conditions"])
            {
                play.Conditions.Add(getObject(condition, typeof(bool)));
            }

            //Propagate tactics objects and bind their parameters
            int index = 0;
            foreach (string def in (List<string>)definitionLists["tactics"])
            {
                string[] strings = parse(def);
                string name = strings[0];

                T tactic;
                if (!tacticBook.TryGetValue(name, out tactic))
                    throw new ApplicationException("Play " + play.Name + " tries to use missing tactic " + name + "!");
                if (tactic.Parameters.Count != strings.Length - 1)
                    throw new ApplicationException("Play " + play.Name + " invokes tactic " + tactic.Name + " with the wrong number of arguments!");

                play.Tactics.Add(tactic);
                index++;

                string prefix = String.Format("{0}{1}.", name, index);

                // To do parameter binding, we create objects for the parameter expressions and add them to the play
                // with names prefixed as if they are internal to the tactic. This way dependant expressions won't duplicate
                // fake parameters, but will bind to the real ones.
                for (int i = 0; i < tactic.Parameters.Count; i++)
                {
                    E fake_param = tactic.Parameters[i];
                    E new_param = getObject(strings[i + 1], tactic.Parameters[i].ReturnType);
                    string param_name = prefix + fake_param.Name;
                    addToPlay(param_name, new_param);
                }

                foreach (E currObject in tactic.getAllObjects())
                {
                    duplicateObject(currObject, tactic, prefix, true);
                }

                foreach (E action in tactic.Actions)
                {
                    E newAction = duplicateObject(action, tactic, prefix, true);
                    play.Actions.Add(newAction);
                }
            }


            play.SetDesignerData((List<string>)definitionLists["designerdata"]);

            Function.RemoveFunctions(factory.Functions());

            return play;
        }

        protected E duplicateObject(E currObject, T tactic, string namePrefix, bool rootObj)
        {
            if (currObject == null)
                throw new ApplicationException("Cannot duplicate non-existing object!");

            if (typeof(PlayBall).IsAssignableFrom(currObject.ReturnType))
                return playable.TheBall;

            // If object not defined in tactic, skip it (except if it's a subexpression)
            if (rootObj &&
                ((String.IsNullOrEmpty(currObject.Name) && !tactic.Actions.Contains(currObject)) ||
                (!String.IsNullOrEmpty(currObject.Name) && !tactic.PlayObjects.ContainsKey(currObject.Name))))
                return currObject;

            E newObject;

            string newName = "";
            if (!String.IsNullOrEmpty(currObject.Name))
                newName = namePrefix + currObject.Name;

            E result;
            // If we've already duplicated this particular object
            if (play.PlayObjects.TryGetValue(newName, out result))
                return result;

            if (currObject.IsFunction)
            {
                E[] newArgs = new E[currObject.theFunction.NumArguments];
                for (int i = 0; i < currObject.theFunction.NumArguments; i++)
                    newArgs[i] = duplicateObject((E)currObject.Arguments[i], tactic, namePrefix, false);

                newObject = factory.Create(currObject.theFunction, newArgs);
            }
            else
                newObject = factory.Create(currObject.StoredValue);

            // This should be reached with an empty name only for actions or subexpressions,
            // which are taken care of elsewhere
            if (!String.IsNullOrEmpty(newName))
                addToPlay(newName, newObject);
            return newObject;
        }

        override protected void processMetadata(string header)
        {
            string[] strings = parse(header);
            string command = strings[0];

            switch (command)
            {
                case "ID":
                    play.ID = int.Parse(strings[1]);
                    break;
                case "type":
                    if (String.Compare(strings[1], "play") != 0)
                        throw new ApplicationException("A play must have type=play!");
                    break;
                case "playtype":
                    play.PlayType = (PlayType)Enum.Parse(typeof(PlayType), strings[1], true);
                    break;
                /*case "numruns":
                    play.NumRuns = int.Parse(strings[1]);
                    break;*/
                case "name":
                    play.Name = play.Name + " - " + strings[1];
                    break;
                case "score":
                    play.Score = double.Parse(strings[1]);
                    break;
                default:
                    throw new ApplicationException("Unrecognized type of metadata: \"" + strings[0] + '"');
            }
        }
    }

    public class TacticLoader<T, E> : PlayableLoader<T, E>
        where T : Playable<E>, ITactic<E>, new()
        where E : Expression
    {
        T tactic
        {
            get { return (T)playable; }
            set { playable = value; }
        }
        private static List<string> namespaces;

        public TacticLoader(Expression.Factory<E> factory)
        {
            //InterpreterFunctions.addFunctions();
            this.factory = factory;

            //Get namespaces of this and all referenced assemblies (do once since it may be expensive)
            if (namespaces == null)
            {
                namespaces = new List<string>();
                namespaces.Add("");
                namespaces.Add("System.");

                foreach (AssemblyName assemblyName in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
                {
                    foreach (Type type in Assembly.Load(assemblyName).GetTypes())
                    {
                        string nspace = type.Namespace + ".";
                        if (!String.IsNullOrEmpty(nspace) && !namespaces.Contains(nspace))
                            namespaces.Add(nspace);
                    }
                }
            }
        }

        protected override void processMetadata(string header)
        {
            string[] strings = parse(header);
            string command = strings[0];

            switch (command)
            {
                case "type":
                    if (String.Compare(strings[1], "tactic") != 0)
                        throw new ApplicationException("A tactic must have type=tactic!");
                    break;
                case "name":
                    playable.Name = strings[1];
                    break;
                default:
                    throw new ApplicationException("Unrecognized type of metadata: \"" + strings[0] + '"');
            }
        }

        /// <summary>
        /// If we want to be able to load parametrized tactics without a play (f.e. in the designer),
        /// we need to have information about parameter types. Here we try to parse a type string and
        /// return the underlying CLR type.
        /// </summary>
        private Type parseParameterType(string type)
        {
            string typeName = type.ToLower();
            //A few shorthands for the play language
            switch (typeName)
            {
                case "point": return typeof(Vector2);
                case "robot": return typeof(PlayRobotDefinition);
                case "int": return typeof(System.Int32);
                case "double": return typeof(System.Double);
                case "bool": return typeof(System.Boolean);

                default:
                    // go over namespaces looking for the given type
                    // XXX: this won't be correct for type conflicts, but we don't care
                    foreach (string nspace in namespaces)
                    {
                        string fullType = nspace + type;

                        //Try to parse the type in case it is defined in this assembly (or mscorelib)
                        Type rtn = Type.GetType(fullType, false, true);
                        if (rtn != null)
                            return rtn;

                        //Also try referenced assemblies
                        foreach (AssemblyName assembly in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
                        {
                            //System.Reflection.
                            rtn = Type.GetType(nspace + type + "," + assembly.FullName, false, true);
                            if (rtn != null)
                                return rtn;
                        }
                    }
                    throw new ApplicationException("Failed to parse parameter type: " + type);
            }
        }

        public override T load(string s, string defaultName)
        {
            tactic = new T();

            definitionLists = new System.Collections.Specialized.ListDictionary();

            string[] lines = s.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            definitionLists.Add("parameters", new List<string>());

            parseDefinitions(lines, false);

            foreach (string parameter in (List<string>)definitionLists["parameters"])
            {
                string[] strings = parse(parameter);
                string name = strings[0];

                // Get the parameter type
                Type partype = parseParameterType(strings[1]);
                // And create a dummy instance of it, invoking the parameterless constructor
                Object fake_param = Activator.CreateInstance(partype);

                E fake_expr = factory.Create(fake_param);
                addToPlay(name, fake_expr);
                tactic.Parameters.Add(fake_expr);
            }

            base.load(s, defaultName);

            Function.RemoveFunctions(factory.Functions());

            return tactic;
        }
    }
}
