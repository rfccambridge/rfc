using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Robocup.Constants
{
    static public class Constants
    {
        static private Dictionary<string, object> dictionary = new Dictionary<string, object>();
        static private List<string> used = new List<string>();
        static volatile private int numloading = 0;
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
                    return float.Parse(s);
                case "bool":
                    return bool.Parse(s);
                default:
                    throw new ApplicationException("Unhandled type: \"" + type + "\"");
            }
        }
        static public void Load(string fname)
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
            if (!used.Contains(name))
                used.Add(name);
            return (T)val;
        }
        /*static public float get(string name) {
            /*object val = 0;
            bool worked = dictionary.TryGetValue(name, out val);
            if (!worked)
                throw new ApplicationException("tried to get an unknown variable called \"" + name + "\"");
            return (float)val;*
            return get<float>(name);
        }*/
        static public bool nondestructiveGet<T>(string name, out T val)
        {
            if (!used.Contains(name))
                used.Add(name);
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
        static public bool isDefined(string name)
        {
            return dictionary.ContainsKey(name);
        }
        static public List<string> getUnused()
        {
            List<string> rtn = new List<string>();
            foreach (KeyValuePair<string, object> pair in dictionary)
            {
                if (!used.Contains(pair.Key))
                    rtn.Add(pair.Key);
            }
            return rtn;
        }
        //static readonly private string defaultFileName = "C:/Microsoft Robotics Studio (1.0)/bin/services/constants.txt";
        static readonly private string defaultFileName = "C:/Microsoft Robotics Studio (1.0)/simulator/Constants/constants.txt";
        static Constants()
        {
            Load(defaultFileName);
        }
    }

}
