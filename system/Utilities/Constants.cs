using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Robocup.Utilities
{
    static public class Constants
    {
        /// <summary>
        /// The directory in which we'll look for the constants files.
        /// </summary>
        const string directory = "../../resources/constants/";
        /// <summary>
        /// Each entry in this dictionary is a map from a name (such as "default" or "vision")
        /// to the set of constants in that category ("BLOBSIZE = 1000")
        /// </summary>
        static private Dictionary<string, Dictionary<string, object>> dictionaries = new Dictionary<string, Dictionary<string, object>>();
        //static private List<string> used = new List<string>();
        static volatile private int numloading = 0;
        /// <summary>
        /// Reloads all constants files that have been loaded so far.  (constants files get loaded the first time they are used)
        /// </summary>
        static public void Load()
        {
            List<string> categories = new List<string>(dictionaries.Keys);
            foreach (string category in categories)
                LoadFromFile(category);
        }
        /// <summary>
        /// Loads/Reloads the given constants files
        /// </summary>
        /// <param name="categories">A list of categories to load/reload</param>
        static public void Load(params string[] categories)
        {
            foreach (string category in categories)
                LoadFromFile(category);
        }
        /// <summary>
        /// A helper function that will convert things like "int","5" to the integer 5.
        /// </summary>
        static private object convert(string type, string s)
        {
            switch (type)
            {
                case "int":
                    return int.Parse(s);
                case "string":
                    return s;
                case "float":
                    return float.Parse(s);
                case "double":
                    return double.Parse(s);
                case "bool":
                    return bool.Parse(s);
                default:
                    throw new ApplicationException("Unhandled type: \"" + type + "\"");
            }
        }
        /// <summary>
        /// Loads the given constants file into memory
        /// </summary>
        /// <param name="category">The category to load.  This is not the pathname, but the name of the constants category
        /// (ie "default" versus "C:\default.txt")</param>
        static private void LoadFromFile(string category)
        {
            string fname = directory + category + ".txt";
            dictionaries.Remove(category);
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dictionaries.Add(category, dict);
            numloading++;
            if (numloading > 1)
            {
                numloading--;
                return;
            }
            if (!File.Exists(fname))
                throw new ApplicationException("sorry, could not find the constants file \""+category+"\", looked in "+fname);
            StreamReader reader = new StreamReader(fname);
            while (!reader.EndOfStream)
            {
                string s = reader.ReadLine();
                if (s == null)
                    break;
                if (s.Length == 0)
                    continue;
                //comment line:
                if (s[0] == '#')
                    continue;
                string[] strings = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                //format is:
                //type name value
                dict.Add(strings[1], convert(strings[0], string.Join(" ", strings, 2, strings.Length - 2)));
            }
            reader.Close();
            numloading--;
        }
        /// <summary>
        /// Tries to get the value of a constant.  If the constant exists, returns the value,
        /// otherwise throws an exception.
        /// (If you don't want it to throw an exception, check nondestructiveGet)
        /// </summary>
        /// <typeparam name="T">The type of object to get</typeparam>
        /// <param name="category">The category in which to look for the constant</param>
        /// <param name="name">The name of the constant</param>
        /// <returns>The value of the constant</returns>
        static public T get<T>(string category, string name)
        {
            //load the file if needed
            if (!dictionaries.ContainsKey(category))
                LoadFromFile(category);

            /*if (!used.Contains(name))
                used.Add(name);*/
            object val;
            bool worked = dictionaries[category].TryGetValue(name, out val);
            if (!worked)
                throw new ApplicationException("tried to get an unknown variable called \"" + name + "\" in category \""+category+"\"");
#if DEBUG
            if (typeof(T) != val.GetType())
                throw new ApplicationException("you asked for a different type of variable than is stored\nGiven: " + typeof(T).Name + ", Stored: " + val.GetType().Name);
#endif
            return (T)val;
        }
        /// <summary>
        /// Tries to get the value of a constant; if it exists, returns true and puts the value in val, otherwise returns false.
        /// </summary>
        /// <typeparam name="T">The type of object to get</typeparam>
        /// <param name="category">The category in which to look for the constant</param>
        /// <param name="name">The name of the constant</param>
        /// <param name="val">The variable that will have the value loaded into</param>
        /// <returns>Whether or not the constant exists</returns>
        static public bool nondestructiveGet<T>(string category, string name, out T val)
        {
            //load the file if needed
            if (!dictionaries.ContainsKey(category))
                LoadFromFile(category);

            /*if (!used.Contains(name))
                used.Add(name);*/
            object getter;
            if (dictionaries[category].TryGetValue(name, out getter))
            {
#if DEBUG
                if (typeof(T) != getter.GetType())
                    throw new ApplicationException("you asked for a different type of variable than is stored\nGiven: " + typeof(T).Name + ", Stored: " + getter.GetType().Name);
#endif
                val = (T)getter;
                return true;
            }
            else
            {
                val = default(T);
                return false;
            }
        }
        /// <summary>
        /// Gets whether or not the specified constant has been defined
        /// </summary>
        /// <param name="name">The name of the constant</param>
        /// <returns>Whether or not the constant exists</returns>
        static public bool isDefined(string name)
        {
            return dictionaries.ContainsKey(name);
        }
        /*static public List<string> getUnused()
        {
            List<string> rtn = new List<string>();
            foreach (KeyValuePair<string, object> pair in dictionary)
            {
                if (!used.Contains(pair.Key))
                    rtn.Add(pair.Key);
            }
            return rtn;
        }*/
    }

}
