using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Robocup.Utilities
{
    static public class Constants
    {
        static private Dictionary<string, object> dictionary = new Dictionary<string, object>();
        //static private List<string> used = new List<string>();
        static volatile private int numloading = 0;
        /// <summary>
        /// Reloads the constants from file.  This happens once automatically at the beginning of the program.
        /// </summary>
        static public void Load()
        {
            Load(defaultFileName);
        }
        static private object convert(string type, string s)
        {
            switch (type)
            {
                case "int":
                    return int.Parse(s);
                case "string":
                    return s;
                case "float":
                    return double.Parse(s);
                case "bool":
                    return bool.Parse(s);
                default:
                    throw new ApplicationException("Unhandled type: \"" + type + "\"");
            }
        }
        static private void Load(string fname)
        {
            //StringReader reader = new System.IO.StringReader(Properties.Resources.constants);
            dictionary.Clear();
            numloading++;
            if (numloading > 1)
            {
                numloading--;
                return;
            }
            //FileStream file = new FileStream(fname, FileMode.Open);
            //StreamReader reader = new StreamReader(file);
            StreamReader reader = new StreamReader(fname);
            while (!reader.EndOfStream)
            {
                //while(true){
                string s = reader.ReadLine();
                if (s == null)
                    break;
                if (s.Length == 0)
                    continue;
                if (s[0] == '#')
                    continue;
                string[] strings = s.Split(new char[] { ' ' }, StringSplitOptions.None);
                //formate is:
                //type name value
                dictionary.Add(strings[1], convert(strings[0], string.Join(" ", strings, 2, strings.Length - 2)));
            }
            reader.Close();
            //file.Close();
            numloading--;
        }
        /// <summary>
        /// Tries to get the value of a constant.  If the constant exists, returns the value,
        /// otherwise throws an exception.
        /// (If you don't want it to throw an exception, check nondestructiveGet)
        /// </summary>
        /// <typeparam name="T">The type of object to get</typeparam>
        /// <param name="name">The name of the constant</param>
        /// <returns>The value of the constant</returns>
        static public T get<T>(string name)
        {
            object val;
            bool worked = dictionary.TryGetValue(name, out val);
            if (!worked)
            {
                throw new ApplicationException("tried to get an unknown variable called \"" + name + "\"");
            }
#if DEBUG
            if (typeof(T) != val.GetType())
                throw new ApplicationException("you asked for a different type of variable than is stored\nGiven: " + typeof(T).Name + ", Stored: " + val.GetType().Name);
#endif
            /*if (!used.Contains(name))
                used.Add(name);*/
            return (T)val;
        }
        /// <summary>
        /// Tries to get the value of a constant; if it exists, returns true and puts the value in val, otherwise returns false.
        /// </summary>
        /// <typeparam name="T">The type of object to get</typeparam>
        /// <param name="name">The name of the constant</param>
        /// <param name="val">The variable that will have the value loaded into</param>
        /// <returns>Whether or not the constant exists</returns>
        static public bool nondestructiveGet<T>(string name, out T val)
        {
            /*if (!used.Contains(name))
                used.Add(name);*/
            object getter;
            if (dictionary.TryGetValue(name, out getter))
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
            return dictionary.ContainsKey(name);
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

        static readonly private string defaultFileName = "../../resources/constants.txt";
        static Constants()
        {
            Load(defaultFileName);
        }
    }

}
