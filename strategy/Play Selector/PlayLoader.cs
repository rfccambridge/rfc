using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Drawing;
using Robocup.Infrastructure;

namespace RobocupPlays
{
    public class PlayLoader
    {
        InterpreterPlay play;
        public PlayLoader()
        {
            InterpreterFunctions.addFunctions();
        }
        /// <summary>
        /// Returns a new play, loaded from the string that is the play (not the filename).
        /// 
        /// As for the objects, they are guaranteed to be added to the play in a topologically sorted way:
        /// if A depends on B, then B is added first.  (Currently 4/30, this only matters for the robots)
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public InterpreterPlay load(string s)
        {
            play = new InterpreterPlay();

            s = s.Replace("#ml", "");

            int hash = s.Substring(s.IndexOf('\n')).Trim('\n', '\r').GetHashCode();

            string[] lines = s.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            System.Collections.Specialized.ListDictionary definitionLists = new System.Collections.Specialized.ListDictionary();

            definitionLists.Add("objects", new ArrayList());

            definitionLists.Add("metadata", new ArrayList());

            definitionLists.Add("conditions", new ArrayList());
            definitionLists.Add("actions", new ArrayList());

            ArrayList curList = null;
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
                    curList = (ArrayList)definitionLists[label];
                }
                else
                    curList.Add(line);
            }

            remainingDefinitions = new Dictionary<string, string>();
            foreach (string def in (ArrayList)definitionLists["objects"])
            {
#if DEBUG
                if (def == "<undefined>")
                    throw new ApplicationException("Silly you, you left one of the robots undefined");
#endif
                string name = def.Substring(0, def.IndexOf(' '));
                string definition = def.Substring(def.IndexOf(' ')).Trim();
                remainingDefinitions.Add(name, definition);
            }
            InterpreterBall ball = new InterpreterBall();
            play.Ball = ball;
            play.definitionDictionary.Add("ball", new InterpreterExpression(ball));
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
            foreach (string action in (ArrayList)definitionLists["actions"])
            {
                play.Actions.Add(getObject(action, typeof(ActionDefinition)));
            }
            foreach (string condition in (ArrayList)definitionLists["conditions"])
            {
                play.Conditions.Add(getObject(condition, typeof(bool)));
            }
            foreach (string header in (ArrayList)definitionLists["metadata"])
            {
                processMetadata(header);
            }

            return play;
        }
        private void addToPlay(string name, InterpreterExpression obj)
        {
            obj.Name = name;
            play.definitionDictionary.Add(name, obj);
            if (obj.ReturnType == typeof(InterpreterRobot))
                play.addRobot(obj);
        }

        Dictionary<string, string> remainingDefinitions;

        private void processMetadata(string header)
        {
            string[] strings = UsefulFunctions.parse(header);
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
                case "learning":
                    play.Learning[strings[1]] = int.Parse(strings[2]);
                    break;
                default:
                    throw new ApplicationException("Unrecognized type of metadata: \"" + strings[0] + '"');
            }
        }

        private InterpreterExpression getObject(string definition, Type wantedType)
        {
            if (definition[0] == '(')
                return treatAsFunction(definition, wantedType);
            if (wantedType == typeof(string))
                //just create the new expression directly from the input (ie, don't remove quotes)
                return new InterpreterExpression(definition);

            //else, it's a name:
            InterpreterExpression rtn;
            if (play.definitionDictionary.TryGetValue(definition, out rtn))
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
                InterpreterExpression obj = getObject(newdefinition, typeof(object));
                if (!wantedType.IsAssignableFrom(obj.ReturnType))
                    throw new ApplicationException("Received an object of an unexpected type: expected " + wantedType.Name + ", but got " + obj.ReturnType.Name);

                addToPlay(name, obj);
                return obj;
            }

            //if it's not a name, then try to parse it as the type that we're expecting:
            try
            {
                return new InterpreterExpression(wantedType.InvokeMember("Parse", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Static, null, null, new object[] { name }));
            }
            catch (MissingMethodException)
            {
                //it has no parse method
            }

            throw new ApplicationException("there are no remaining definitions for this object.  most likely cause: circular definition");
        }

        private InterpreterExpression treatAsFunction(string definition, Type wantedType)
        {
            string[] strings = UsefulFunctions.parse(definition);
            Function f = Function.getFunction(strings[0]);
            if (f == null)
                throw new ApplicationException("Could not find a function of the name \"" + strings[0] + "\"");
            if (!wantedType.IsAssignableFrom(f.ReturnType))
                throw new ApplicationException("Received an function of an unexpected return type: expected " + wantedType.Name + ", but got " + f.ReturnType.Name);
            if (strings.Length - 1 != f.NumArguments)
                throw new ApplicationException("The function \"" + f.Name + "\" expects " + f.NumArguments + " arguments, but received " + (strings.Length - 1));
            InterpreterExpression[] objects = new InterpreterExpression[f.NumArguments];
            for (int i = 0; i < f.NumArguments; i++)
            {
                objects[i] = getObject(strings[i + 1], f.ArgTypes[i]);
            }
            return new InterpreterExpression(f, objects);
        }
    }
}
