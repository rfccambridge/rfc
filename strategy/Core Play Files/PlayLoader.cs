using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Drawing;
using Robocup.Infrastructure;
using Robocup.Geometry;

namespace RobocupPlays
{
    public class PlayLoader<P, E> where P:Play<E>, new() where E:Expression
    {
        P play;
        Expression.Factory<E> factory;
        public PlayLoader(Expression.Factory<E> factory)
        {
            //InterpreterFunctions.addFunctions();
            this.factory = factory;
        }
        /// <summary>
        /// Returns a new play, loaded from the string that is the play (not the filename).
        /// 
        /// As for the objects, they are guaranteed to be added to the play in a topologically sorted way:
        /// if A depends on B, then B is added first.  (Currently 4/30, this only matters for the robots)
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public P load(string s)
        {
            Function.AddFunctions(factory.Functions());

            play = new P();

            s = s.Replace("#ml", "");

            int hash = s.Substring(s.IndexOf('\n')).Trim('\n', '\r').GetHashCode();

            string[] lines = s.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            System.Collections.Specialized.ListDictionary definitionLists = new System.Collections.Specialized.ListDictionary();

            definitionLists.Add("objects", new List<string>());

            definitionLists.Add("metadata", new List<string>());

            definitionLists.Add("conditions", new List<string>());
            definitionLists.Add("actions", new List<string>());
            definitionLists.Add("designerdata", new List<string>());

            List<string> curList = null;
            for (int stringnum = 0; stringnum < lines.Length; stringnum++)
            {
                string line = lines[stringnum].Trim();
                //if the last character is a colon,
                //then it's a label for something:
                if (line[line.Length - 1] == ':')
                {
                    string label = line.Trim(':').ToLower();
                    //bool found = definitionLists.TryGetValue(line.Trim(':').ToLower(), out curList);
                    if (!definitionLists.Contains(label))
                        throw new ApplicationException("Could not recognize label \"" + line.Trim(':').ToLower() + "\"");
                    curList = (List<string>)definitionLists[label];
                }
                else
                    curList.Add(line);
            }

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
                play.Actions.Add(getObject(action, typeof(ActionDefinition)));
            }
            foreach (string condition in (List<string>)definitionLists["conditions"])
            {
                play.Conditions.Add(getObject(condition, typeof(bool)));
            }
            foreach (string header in (List<string>)definitionLists["metadata"])
            {
                processMetadata(header);
            }
            play.SetDesignerData((List<string>)definitionLists["designerdata"]);

            Function.RemoveFunctions(factory.Functions());

            return play;
        }
        private void addToPlay(string name, E obj)
        {
            obj.Name = name;
            play.PlayObjects.Add(name, obj);
                play.addRobot(obj);
        }

        Dictionary<string, string> remainingDefinitions;

        private void processMetadata(string header)
        {
            string[] strings = parse(header);
            string command = strings[0];

            switch (command)
            {
                case "ID":
                    play.ID = int.Parse(strings[1]);
                    break;
                case "type":
                    play.PlayType = (PlayTypes)Enum.Parse(typeof(PlayTypes), strings[1], true);
                    break;
                /*case "numruns":
                    play.NumRuns = int.Parse(strings[1]);
                    break;*/
                case "name":
                    play.Name = strings[1];
                    break;
                case "score":
                    play.Score = float.Parse(strings[1]);
                    break;
                default:
                    throw new ApplicationException("Unrecognized type of metadata: \"" + strings[0] + '"');
            }
        }

        private E getObject(string definition, Type wantedType)
        {
            if (definition[0] == '(')
                return treatAsFunction(definition, wantedType);
            if (wantedType == typeof(string))
                //just create the new expression directly from the input (ie, don't remove quotes)
                return factory.Create(definition);
            if (definition == "ball")
                return play.TheBall;

            //else, it's a name:
            E rtn;
            if (play.PlayObjects.TryGetValue(definition, out rtn))
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

            throw new ApplicationException("there are no remaining definitions for this object.  most likely cause: circular definition");
        }

        private E treatAsFunction(string definition, Type wantedType)
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


        /// <summary>
        /// Takes a string s, assumed to start and end with parenthesis, and splits it up into subexpressions.
        /// Ex: (line (robot robot1) point2) gives {"line","(robot robot1)","point2"}
        /// </summary>
        static private string[] parse(string s)
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
    }
}
