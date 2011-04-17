using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Robocup.Geometry;

namespace Robocup.Core
{
    static public class Constants
    {
        //NOTE: Values are stored as float because doubles can't be volatile!

        //CONSTANTS------------------------------------------------------------------------------------
        static public class Basic
        {
            /// <summary> Number of robots and/or robot ids possible </summary>
            static public int NUM_ROBOTS { get { InitializeIfNeeded(); return _NUM_ROBOTS; } } static volatile int _NUM_ROBOTS;

            /// <summary> Radius of a robot </summary>
            static public double ROBOT_RADIUS { get { InitializeIfNeeded(); return _ROBOT_RADIUS; } } static volatile float _ROBOT_RADIUS;

            /// <summary> Distance from the center to the front of a robot </summary>
            static public double ROBOT_FRONT_RADIUS { get { InitializeIfNeeded(); return _ROBOT_FRONT_RADIUS; } } static volatile float _ROBOT_FRONT_RADIUS;

            /// <summary> Radius of the ball </summary>
            static public double BALL_RADIUS { get { InitializeIfNeeded(); return _BALL_RADIUS; } } static volatile float _BALL_RADIUS;

            static public void Reload()
            {
                _NUM_ROBOTS = ConstantsRaw.get<int>("default", "NUM_ROBOTS");
                _ROBOT_RADIUS = (float)ConstantsRaw.get<double>("plays", "ROBOT_RADIUS");
                _ROBOT_FRONT_RADIUS = (float)ConstantsRaw.get<double>("plays", "ROBOT_FRONT_RADIUS");
                _BALL_RADIUS = (float)ConstantsRaw.get<double>("plays", "BALL_RADIUS");
            }
        }

        static public class Field
        {
            //SIZES-----------------------------------------------------------

            /// <summary> Width of the in-bounds part of the field </summary>
            static public double WIDTH { get { InitializeIfNeeded(); return _WIDTH; } } static volatile float _WIDTH;

            /// <summary> Height of the in-bounds part of the field </summary>
            static public double HEIGHT { get { InitializeIfNeeded(); return _HEIGHT; } } static volatile float _HEIGHT;

            /// <summary> Width of field including ref zone </summary>
            static public double FULL_WIDTH { get { InitializeIfNeeded(); return _FULL_WIDTH; } } static volatile float _FULL_WIDTH;

            /// <summary> Height of field including ref zone </summary>
            static public double FULL_HEIGHT { get { InitializeIfNeeded(); return _FULL_HEIGHT; } } static volatile float _FULL_HEIGHT;

            //BOUNDS----------------------------------------------------------

            /// <summary> X coord of the left boundary line </summary>
            static public double XMIN { get { InitializeIfNeeded(); return _XMIN; } } static volatile float _XMIN;

            /// <summary> X coord of the right boundary line </summary>
            static public double XMAX { get { InitializeIfNeeded(); return _XMAX; } } static volatile float _XMAX;

            /// <summary> Y coord of the bottom boundary line </summary>
            static public double YMIN { get { InitializeIfNeeded(); return _YMIN; } } static volatile float _YMIN;

            /// <summary> Y coord of the top boundary line </summary>
            static public double YMAX { get { InitializeIfNeeded(); return _YMAX; } } static volatile float _YMAX;


            /// <summary> X coord of the left boundary including the ref zone </summary>
            static public double FULL_XMIN { get { InitializeIfNeeded(); return _FULL_XMIN; } } static volatile float _FULL_XMIN;

            /// <summary> X coord of the right boundary including the ref zone </summary>
            static public double FULL_XMAX { get { InitializeIfNeeded(); return _FULL_XMAX; } } static volatile float _FULL_XMAX;

            /// <summary> Y coord of the bottom boundary including the ref zone </summary>
            static public double FULL_YMIN { get { InitializeIfNeeded(); return _FULL_YMIN; } } static volatile float _FULL_YMIN;

            /// <summary> Y coord of the top boundary including the ref zone </summary>
            static public double FULL_YMAX { get { InitializeIfNeeded(); return _FULL_YMAX; } } static volatile float _FULL_YMAX;

            /// <summary> Width of the referree zone </summary>
            static public double REFEREE_WIDTH { get { InitializeIfNeeded(); return _REFEREE_WIDTH; } } static volatile float _REFEREE_WIDTH;

            //GOAL--------------------------------------------------------------

            /// <summary> Width of the goal zone </summary>
            static public double GOAL_WIDTH { get { InitializeIfNeeded(); return _GOAL_WIDTH; } } static volatile float _GOAL_WIDTH;

            /// <summary> Height of the goal zone </summary>
            static public double GOAL_HEIGHT { get { InitializeIfNeeded(); return _GOAL_HEIGHT; } } static volatile float _GOAL_HEIGHT;

            /// <summary> Y coord of the bottom of goal zone </summary>
            static public double GOAL_YMIN { get { InitializeIfNeeded(); return _GOAL_YMIN; } } static volatile float _GOAL_YMIN;

            /// <summary> Y coord of the top of goal zone </summary>
            static public double GOAL_YMAX { get { InitializeIfNeeded(); return _GOAL_YMAX; } } static volatile float _GOAL_YMAX;

            /// <summary> X coord of the left of the LEFT goal zone </summary>
            static public double GOAL_XMIN { get { InitializeIfNeeded(); return _GOAL_XMIN; } } static volatile float _GOAL_XMIN;

            /// <summary> X coord of the right of the RIGHT goal zone </summary>
            static public double GOAL_XMAX { get { InitializeIfNeeded(); return _GOAL_XMAX; } } static volatile float _GOAL_XMAX;

            //OTHER----------------------------------------------------------

            static public double CENTER_CIRCLE_RADIUS { get { InitializeIfNeeded(); return _CENTER_CIRCLE_RADIUS; } } static volatile float _CENTER_CIRCLE_RADIUS;

            //--------------------------------------------------------------

            static public void Reload()
            {
                _WIDTH = (float)ConstantsRaw.get<double>("plays", "FIELD_WIDTH");
                _HEIGHT = (float)ConstantsRaw.get<double>("plays", "FIELD_HEIGHT");
                _REFEREE_WIDTH = (float)ConstantsRaw.get<double>("plays", "REFEREE_ZONE_WIDTH");
                _GOAL_WIDTH = (float)ConstantsRaw.get<double>("plays", "GOAL_WIDTH");
                _GOAL_HEIGHT = (float)ConstantsRaw.get<double>("plays", "GOAL_HEIGHT");
                _CENTER_CIRCLE_RADIUS = (float)ConstantsRaw.get<double>("plays", "CENTER_CIRCLE_RADIUS");

                _FULL_WIDTH = _WIDTH + 2.0f * ((float)Math.Max(_REFEREE_WIDTH, _GOAL_WIDTH));
                _FULL_HEIGHT = (float)Math.Max(_HEIGHT + 2.0f * _REFEREE_WIDTH, _GOAL_HEIGHT);

                _XMIN = -_WIDTH / 2.0f;
                _XMAX = _WIDTH / 2.0f;
                _YMIN = -_HEIGHT / 2.0f;
                _YMAX = _HEIGHT / 2.0f;

                _FULL_XMIN = -_FULL_WIDTH / 2.0f;
                _FULL_XMAX = _FULL_WIDTH / 2.0f;
                _FULL_YMIN = -_FULL_HEIGHT / 2.0f;
                _FULL_YMAX = _FULL_HEIGHT / 2.0f;

                _GOAL_YMIN = -_GOAL_HEIGHT / 2.0f;
                _GOAL_YMAX = _GOAL_HEIGHT / 2.0f;
                _GOAL_XMIN = _XMIN - _GOAL_WIDTH;
                _GOAL_XMAX = _XMAX + _GOAL_WIDTH;
            }
        }

        static public class FieldPts
        {
            //BASIC LOCATIONS----------------------------------------------------------------------

            /// <summary> Top left corner of field </summary>
            static public Vector2 TOP_LEFT { get { InitializeIfNeeded(); return _TOP_LEFT; } } static volatile Vector2 _TOP_LEFT;

            /// <summary> Bottom left corner of field </summary>
            static public Vector2 BOTTOM_LEFT { get { InitializeIfNeeded(); return _BOTTOM_LEFT; } } static volatile Vector2 _BOTTOM_LEFT;

            /// <summary> Top right corner of field </summary>
            static public Vector2 TOP_RIGHT { get { InitializeIfNeeded(); return _TOP_RIGHT; } } static volatile Vector2 _TOP_RIGHT;

            /// <summary> Bottom right corner of field </summary>
            static public Vector2 BOTTOM_RIGHT { get { InitializeIfNeeded(); return _BOTTOM_RIGHT; } } static volatile Vector2 _BOTTOM_RIGHT;

            /// <summary> Top center edge of field </summary>
            static public Vector2 TOP { get { InitializeIfNeeded(); return _TOP; } } static volatile Vector2 _TOP;

            /// <summary> Bottom center edge of field </summary>
            static public Vector2 BOTTOM { get { InitializeIfNeeded(); return _BOTTOM; } } static volatile Vector2 _BOTTOM;

            /// <summary> Left center edge of field </summary>
            static public Vector2 LEFT { get { InitializeIfNeeded(); return _LEFT; } } static volatile Vector2 _LEFT;

            /// <summary> Right center edge of field </summary>
            static public Vector2 RIGHT { get { InitializeIfNeeded(); return _RIGHT; } } static volatile Vector2 _RIGHT;

            /// <summary> Center of field </summary>
            static public Vector2 CENTER { get { InitializeIfNeeded(); return _CENTER; } } static volatile Vector2 _CENTER;


            //QUADRANTS (octants, really)-----------------------------------------------------------------

            /// <summary> Halfway in between center and top left corner </summary>
            static public Vector2 TOP_LEFT_QUAD { get { InitializeIfNeeded(); return _TOP_LEFT_QUAD; } } static volatile Vector2 _TOP_LEFT_QUAD;

            /// <summary> Halfway in between center and top left corner </summary>
            static public Vector2 BOTTOM_LEFT_QUAD { get { InitializeIfNeeded(); return _BOTTOM_LEFT_QUAD; } } static volatile Vector2 _BOTTOM_LEFT_QUAD;

            /// <summary> Halfway in between center and bottom right corner  </summary>
            static public Vector2 TOP_RIGHT_QUAD { get { InitializeIfNeeded(); return _TOP_RIGHT_QUAD; } } static volatile Vector2 _TOP_RIGHT_QUAD;

            /// <summary> Halfway in between center and bottom right corner </summary>
            static public Vector2 BOTTOM_RIGHT_QUAD { get { InitializeIfNeeded(); return _BOTTOM_RIGHT_QUAD; } } static volatile Vector2 _BOTTOM_RIGHT_QUAD;

            /// <summary> Halfway in between center and top edge </summary>
            static public Vector2 TOP_QUAD { get { InitializeIfNeeded(); return _TOP_QUAD; } } static volatile Vector2 _TOP_QUAD;

            /// <summary> Halfway in between center and bottom edge </summary>
            static public Vector2 BOTTOM_QUAD { get { InitializeIfNeeded(); return _BOTTOM_QUAD; } } static volatile Vector2 _BOTTOM_QUAD;

            /// <summary> Halfway in between center and left edge </summary>
            static public Vector2 LEFT_QUAD { get { InitializeIfNeeded(); return _LEFT_QUAD; } } static volatile Vector2 _LEFT_QUAD;

            /// <summary> Halfway in between center and right edge </summary>
            static public Vector2 RIGHT_QUAD { get { InitializeIfNeeded(); return _RIGHT_QUAD; } } static volatile Vector2 _RIGHT_QUAD;


            //GOAL---------------------------------------------------------

            /// <summary> Center location of the opening of our goal, same as LEFT </summary>
            static public Vector2 OUR_GOAL { get { InitializeIfNeeded(); return _OUR_GOAL; } } static volatile Vector2 _OUR_GOAL;

            /// <summary> Bottom point of our goal </summary>
            static public Vector2 OUR_GOAL_BOTTOM { get { InitializeIfNeeded(); return _OUR_GOAL_BOTTOM; } } static volatile Vector2 _OUR_GOAL_BOTTOM;

            /// <summary> Top point of our goal </summary>
            static public Vector2 OUR_GOAL_TOP { get { InitializeIfNeeded(); return _OUR_GOAL_TOP; } } static volatile Vector2 _OUR_GOAL_TOP;

            /// <summary> Center location of the opening of their goal, same as RIGHT </summary>
            static public Vector2 THEIR_GOAL { get { InitializeIfNeeded(); return _THEIR_GOAL; } } static volatile Vector2 _THEIR_GOAL;
            
            /// <summary> Bottom point of their goal </summary>
            static public Vector2 THEIR_GOAL_BOTTOM { get { InitializeIfNeeded(); return _THEIR_GOAL_BOTTOM; } } static volatile Vector2 _THEIR_GOAL_BOTTOM;

            /// <summary> Top point of their goal </summary>
            static public Vector2 THEIR_GOAL_TOP { get { InitializeIfNeeded(); return _THEIR_GOAL_TOP; } } static volatile Vector2 _THEIR_GOAL_TOP;

            static public void Reload()
            {
                double dx = (float)ConstantsRaw.get<double>("plays", "FIELD_WIDTH") / 2;
                double dy = (float)ConstantsRaw.get<double>("plays", "FIELD_HEIGHT") / 2;
                double dyg = (float)ConstantsRaw.get<double>("plays", "GOAL_HEIGHT") / 2;

                _TOP_LEFT = new Vector2(-dx, dy);
                _BOTTOM_LEFT = new Vector2(-dx, -dy);
                _TOP_RIGHT = new Vector2(dx, dy);
                _BOTTOM_RIGHT = new Vector2(dx, -dy);
                _TOP = new Vector2(0, dy);
                _BOTTOM = new Vector2(0, -dy);
                _LEFT = new Vector2(-dx, 0);
                _RIGHT = new Vector2(dx, 0);
                _CENTER = new Vector2(0, 0);

                _TOP_LEFT_QUAD =     (_TOP_LEFT     + _CENTER) / 2.0;
                _BOTTOM_LEFT_QUAD =  (_BOTTOM_LEFT  + _CENTER) / 2.0;
                _TOP_RIGHT_QUAD =    (_TOP_RIGHT    + _CENTER) / 2.0;
                _BOTTOM_RIGHT_QUAD = (_BOTTOM_RIGHT + _CENTER) / 2.0;
                _TOP_QUAD =          (_TOP          + _CENTER) / 2.0;
                _BOTTOM_QUAD =       (_BOTTOM       + _CENTER) / 2.0;
                _LEFT_QUAD =         (_LEFT         + _CENTER) / 2.0;
                _RIGHT_QUAD =        (_RIGHT        + _CENTER) / 2.0;

                _OUR_GOAL = new Vector2(-dx, 0);
                _OUR_GOAL_BOTTOM = new Vector2(-dx, -dyg);
                _OUR_GOAL_TOP = new Vector2(-dx, dyg);
                _THEIR_GOAL = new Vector2(dx, 0);
                _THEIR_GOAL_BOTTOM = new Vector2(dx, -dyg);
                _THEIR_GOAL_TOP = new Vector2(dx, dyg);
            }
        }

        static Object _lock = new Object();
        static volatile bool _is_initialized = false;
        static volatile bool _is_reloading = false;
        static void InitializeIfNeeded()
        {
            if (!_is_initialized)
            {
                lock (_lock)
                {
                    if (!_is_initialized)
                    {
                        Reload();
                        _is_initialized = true;
                    }
                }
            }
        }

        public static void Reload()
        {
            lock (_lock)
            {
                if (_is_reloading)
                {
                    Console.WriteLine("Somehow we made it in to Constants.reload() when we are already reloading!");
                    Console.WriteLine("THIS IS EXTREMELY BAD!");
                    throw new Exception("Somehow we made it in to Constants.reload() when we are already reloading!");
                }
                _is_reloading = true;
                Basic.Reload();
                Field.Reload();
                FieldPts.Reload();
                _is_reloading = false;
            }
        }
    }




    static public class ConstantsRaw
    {
        //INTERFACE-------------------------------------------------------------------------------------

        /// <summary>
        /// The directory in which we'll look for the constants files.
        /// </summary>
        const string directory = "../../resources/constants/";

        /// <summary>
        /// Each entry in this dictionary is a map from a name (such as "default" or "vision")
        /// to the set of constants in that category ("BLOBSIZE = 1000")
        /// </summary>
        static private Dictionary<string, Dictionary<string, object>> dictionaries = 
            new Dictionary<string, Dictionary<string, object>>();
        
        /// <summary>
        /// Reloads all constants files that have been loaded so far.  (constants files get loaded the first time they are used)
        /// </summary>
        static public void Load()
        {
            lock (dictionaries)
            {
                List<string> categories = new List<string>(dictionaries.Keys);
                foreach (string category in categories)
                    LoadFromFile(category);
            }
        }

        /// <summary>
        /// A helper function that will convert things like "int","5" to the integer 5.
        /// </summary>
        static private object convert(string type, string s)
        {
            switch (type)
            {
                case "int":    return int.Parse(s);
                case "string": return s;
                case "float":  return float.Parse(s);
                case "double": return double.Parse(s);
                case "bool":   return bool.Parse(s);
                default:
                    throw new ApplicationException("Unhandled type: \"" + type + "\"");
            }
        }

        /// <summary>
        /// Loads the given constants file into memory if they have not yet been
        /// </summary>
        /// <param name="category">The category to load.  This is not the pathname, but the name of the constants category
        /// (ie "default" instead of "C:\default.txt")</param>
        static private void LoadFromFileIfNeeded(string category)
        {
            lock (dictionaries)
            {
                if (!dictionaries.ContainsKey(category) || dictionaries[category].Count == 0)
                    LoadFromFile(category);
            }
        }

        /// <summary>
        /// Loads the given constants file into memory
        /// </summary>
        /// <param name="category">The category to load.  This is not the pathname, but the name of the constants category
        /// (ie "default" instead of "C:\default.txt")</param>
        static private void LoadFromFile(string category)
        {
            lock(dictionaries)
            {
                string fname = directory + category + ".txt";
                dictionaries.Remove(category);
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dictionaries.Add(category, dict);

                if (!File.Exists(fname))
                    throw new ApplicationException("sorry, could not find the constants file \""+category+"\", looked in "+fname);
                StreamReader reader = new StreamReader(fname);
                while (!reader.EndOfStream)
                {
                    string s = reader.ReadLine();
                    if (s == null)
                        break;

                    //Remove comments (if we find a # anywhere on the line, remove all chars after it)
                    int commentIndex = s.IndexOf('#');
                    if (commentIndex >= 0)
                        s = s.Remove(commentIndex);

                    if (s.Length == 0)
                        continue;

                    string[] strings = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    //format is:
                    //type name value   
                    try {
                        dict.Add(strings[1], convert(strings[0], string.Join(" ", strings, 2, strings.Length - 2)));
                    } catch (ArgumentException e) {
                        reader.Close();
                        throw e;
                    }
                }
                reader.Close();
            }
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
            lock (dictionaries)
            {
                LoadFromFileIfNeeded(category);

                object val;
                bool worked = dictionaries[category].TryGetValue(name, out val);
                if (!worked)
                    throw new ApplicationException("tried to get an unknown variable called \"" + name + "\" in category \"" + category + "\"");

                if (typeof(T) != val.GetType())
                    throw new ApplicationException("you asked for a different type of variable than is stored\nGiven: " + typeof(T).Name + ", Stored: " + val.GetType().Name);

                return (T)val;
            }
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
            lock (dictionaries)
            {
                LoadFromFileIfNeeded(category);

                object getter;
                if (dictionaries[category].TryGetValue(name, out getter))
                {
                    if (typeof(T) != getter.GetType())
                        throw new ApplicationException("you asked for a different type of variable than is stored\nGiven: " + typeof(T).Name + ", Stored: " + getter.GetType().Name);

                    val = (T)getter;
                    return true;
                }
                else
                {
                    val = default(T);
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets whether or not the specified constant has been defined
        /// </summary>
        /// <param name="name">The name of the constant</param>
        /// <returns>Whether or not the constant exists</returns>
        //TODO(davidwu): This is wrong. It checks whether a category is defined, not whether a constant is defined!
        static public bool isDefined(string name)
        {
            lock (dictionaries)
            {
                return dictionaries.ContainsKey(name);
            }
        }
    }
}
