using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Robocup.Geometry;
using Robocup.Infrastructure;

namespace RobocupPlays
{
    partial class Function
    {
        private static readonly Random r = new Random();
        private static readonly List<Function> Functions = new List<Function>();
        static public Function getFunction(string name)
        {
            foreach (Function f in Functions)
            {
                if (f.Name == name)
                    return f;
            }
            throw new ApplicationException("Unable to find a function of the name \"" + name + "\"");
            //return null;
        }
        static public Function[] getFunctions(Type returnType)
        {
            List<Function> rtn = new List<Function>();
            foreach (Function f in Functions)
            {
                if (returnType.IsAssignableFrom(f.returnType))
                    rtn.Add(f);
            }
            return rtn.ToArray();
        }
        //two functions are .Equal() if they have the same name
        private static void addFunction(Function f)
        {
            if (!Functions.Contains(f))
                Functions.Add(f);
            else
                throw new ApplicationException("You have tried to add the function " + f.Name + ", but it already exists!");
        }
        /*private static void replaceFunction(Function f)
        {
            if (!Functions.Contains(f))
                throw new ApplicationException("You are attempting to replace the function " + f.Name + ", but it hasn't been added yet!");
            Functions.Remove(f);
            Functions.Add(f);
        }*/
        internal static void AddFunctions(List<Function> functions)
        {
            foreach (Function f in functions)
            {
                addFunction(f);
            }
        }
        internal static void RemoveFunctions(List<Function> functions)
        {
            foreach (Function f in functions)
            {
                Functions.Remove(f);
            }
        }

        #region Generic Functions
        private static string cleanUpName(string s)
        {
            return s.Trim('<', '>');
        }
        public static string getStringFromType(Type t)
        {
            if (t == typeof(float))
                return "<float>";
            else if (t.IsAssignableFrom(typeof(Vector2)))
                return "<point>";
            else if (t == typeof(GreaterLessThan))
                return "<";
            else if (t.IsAssignableFrom(typeof(Line)))
                return "<line>";
            else if (t == typeof(TeamCondition))
                return "our_team";
            else if (t.IsAssignableFrom(typeof(Robot)))
                return "<robot>";
            else if (t.IsAssignableFrom(typeof(Circle)))
                return "<circle>";
            else if (t == typeof(int))
                return "<int>";
            else if (t == typeof(ActionDefinition))
                return "<Action>";
            return "<" + t.Name + ">";
        }

        private delegate void addGenericFunction(Type t);
        private static void addIf(Type t)
        {
            string name = cleanUpName(getStringFromType(t));
            addFunction("if_" + name, "If , Then - " + name, t, new Type[] { typeof(bool), t, t }, "If ~, then ~, else ~", delegate(EvaluatorState state, object[] objects)
            {
                if ((bool)objects[0])
                    return objects[1];
                else
                    return objects[2];
            });
        }
        private static void addAll(addGenericFunction function, params Type[] types)
        {
            foreach (Type t in types)
            {
                function(t);
            }
        }
        static private void addGenerics()
        {
            addAll(addIf, typeof(int), typeof(float), typeof(Vector2), typeof(Line), typeof(Circle), typeof(ActionDefinition));
            /*Type[] typelist = new Type[] { typeof(int), typeof(float), typeof(Vector2), typeof(Line), typeof(PlayCircle) };
            addGenericFunction[] functionlist = new addGenericFunction[] { addIf };
            foreach (addGenericFunction function in functionlist)
            {
                foreach (Type t in typelist)
                {
                    function(t);
                }
            }*/
        }
        #endregion


        public static void addFunction(string name, string longname, Type returnType, Type[] argTypes, string description, FunctionRunDelegate run)
        {
            addFunction(new Function(name, longname, returnType, argTypes, description, run));
        }
        public static void addFunction<T>(string name, string longname, Type returnType, Type[] argTypes, string description, FunctionRunDelegate run)
        {
            addFunction(new Function(name, longname, returnType, argTypes, description, run));
        }
        /// <summary>
        /// Clears the stored functions, freeing up the unused ones to be garbage collected
        /// </summary>
        /*public static void clearFunctions()
        {
            Functions.Clear();
            Functions.TrimExcess();
        }*/
        public static void loadFunctions()
        {
            addGenerics();
            #region geometric functions
            addFunction("point", "X, Y - Point", typeof(Vector2), new Type[] { typeof(float), typeof(float) }, "The point ~, ~", delegate(EvaluatorState state, object[] objects)
            {
                return new Vector2((float)objects[0], (float)objects[1]);
            });
            addFunction("getXcoord", "Point - X coordinate", typeof(float), new Type[] { typeof(Vector2) }, "The x coordinate of point ~", delegate(EvaluatorState state, object[] objects)
            {
                return ((Vector2)objects[0]).X;
            });
            addFunction("getYcoord", "Point - Y coordinate", typeof(float), new Type[] { typeof(Vector2) }, "The y coordinate of point ~", delegate(EvaluatorState state, object[] objects)
            {
                return ((Vector2)objects[0]).Y;
            });
            addFunction("line", "Point, Point - Line", typeof(Line), new Type[] { typeof(Vector2), typeof(Vector2) }, "The line connecting ~ and ~", delegate(EvaluatorState state, object[] objects)
            {
                return new Line((Vector2)objects[0], (Vector2)objects[1]);
            });
            addFunction("circle", "Point, Radius - Circle", typeof(Circle), new Type[] { typeof(Vector2), typeof(float) }, "The circle with ~ as its center, and radius ~", delegate(EvaluatorState state, object[] objects)
            {
                return new Circle((Vector2)objects[0], (float)objects[1]);
            });
            addFunction("pointof", "Point-object - Point", typeof(Vector2), new Type[] { typeof(GetPointable) }, "The point of ~", delegate(EvaluatorState state, object[] objects)
            {
                return ((GetPointable)objects[0]).getPoint();
                //return new Vector2((GetPointable)objects[0]);
            });
            addFunction("linelineintersection", "Line, Line - Intersection", typeof(Vector2), new Type[] { typeof(Line), typeof(Line) }, "The intersection of lines ~ and ~", delegate(EvaluatorState state, object[] objects)
            {
                Line l1 = (Line)objects[0];
                Line l2 = (Line)objects[1];
                return Intersections.intersect(l1, l2);
                //return new Vector2(new LineLineIntersection(l1, l2));
            });
            addFunction("linecircleintersection", "Line, Circle - Intersection", typeof(Vector2), new Type[] { typeof(Line), typeof(Circle), typeof(int) }, "The intersection of line ~ and circle ~, number ~", delegate(EvaluatorState state, object[] objects)
            {
                Line line = (Line)objects[0];
                Circle circle = (Circle)objects[1];
                return Intersections.intersect(line, circle, (int)objects[2]);
                //return new Vector2(new LineCircleIntersection(line, circle, (int)objects[2]));
            });
            addFunction("circlecircleintersection", "Circle, Circle - Intersection", typeof(Vector2), new Type[] { typeof(Circle), typeof(Circle), typeof(int) }, "The intersection of circles ~ and ~, number ~", delegate(EvaluatorState state, object[] objects)
            {
                Circle c1 = (Circle)objects[0];
                Circle c2 = (Circle)objects[1];
                return Intersections.intersect(c1, c2, (int)objects[2]);
                //return new Vector2(new PlayCircleCircleIntersection(c1, c2, (int)objects[2]));
            });
            addFunction("pointpointdistance", "Point, Point - Distance", typeof(float), new Type[] { typeof(Vector2), typeof(Vector2) }, "The distance between ~ and ~ (in meters)", delegate(EvaluatorState state, object[] objects)
            {
                /*Vector2 p1 = (Vector2)objects[0];
                Vector2 p2 = (Vector2)objects[1];
                return CommonFunctions.distance(p1.getPoint(), p2.getPoint());*/
                return UsefulFunctions.distance((Vector2)objects[0], (Vector2)objects[1]);
            });
            addFunction("linelength", "Line - Length", typeof(float), new Type[] { typeof(Line) }, "The length of line ~", delegate(EvaluatorState state, object[] objects)
            {
                Vector2[] points = ((Line)objects[0]).getPoints();
                return UsefulFunctions.distance(points[0], points[1]);
            });

            addFunction("scalepoint", "Point, Float - Scale", typeof(Vector2), new Type[] { typeof(Vector2), typeof(float) }, "The point ~ scaled by ~", delegate(EvaluatorState state, object[] objects)
            {
                return ((float)objects[1]) * ((Vector2)objects[0]);
            });
            addFunction("pointadd", "Point, Point - Add", typeof(Vector2), new Type[] { typeof(Vector2), typeof(Vector2) }, "Point ~ + Point ~", delegate(EvaluatorState state, object[] objects)
            {
                return ((Vector2)objects[0]) + ((Vector2)objects[1]);
            });
            addFunction("pointsubtract", "Point, Point - Subtract", typeof(Vector2), new Type[] { typeof(Vector2), typeof(Vector2) }, "Point ~ - Point ~", delegate(EvaluatorState state, object[] objects)
            {
                return ((Vector2)objects[0]) - ((Vector2)objects[1]);
            });
            addFunction("midpoint", "Point, Point - Midpoint", typeof(Vector2), new Type[] { typeof(Vector2), typeof(Vector2) }, "The midpoint of points ~ and ~", delegate(EvaluatorState state, object[] objects)
            {
                return .5f * (((Vector2)objects[0]) + ((Vector2)objects[1]));
            });
            addFunction("closestRobotToLine", "Line, Team - closest robot", typeof(float), new Type[] { typeof(Line), typeof(TeamCondition) }, "The distance from the line ~ to the closest robot on team ~ (excluding the robots that are the endpoints of the line)", delegate(EvaluatorState state, object[] objects)
            {
                float rtn = 1000000;
#if DEBUG
                if (state == null) throw new ApplicationException("tried to call a function that needed an evaluatorState without one!");
#endif
                Line line = (Line)objects[0];
                TeamCondition condition = (TeamCondition)objects[1];
                List<RobotInfo> allinfos = new List<RobotInfo>();
                if (condition.maybeOurs())
                    allinfos.AddRange(state.OurTeamInfo);
                if (condition.maybeTheirs())
                    allinfos.AddRange(state.TheirTeamInfo);
                Vector2[] endpoints = line.getPoints();
                foreach (RobotInfo r in allinfos)
                {
                    Vector2 position = r.Position;
                    if (position == endpoints[0] || position == endpoints[1])
                        continue;
                    rtn = (float)Math.Min(rtn, line.distFromSegment(position));
                }
                return rtn;
            });

            #endregion

            #region arithmetic functions
            addFunction("/", "Float, Float - Divide", typeof(float), new Type[] { typeof(float), typeof(float) }, "~ / ~", delegate(EvaluatorState state, object[] objects)
            {
                return ((float)objects[0]) / ((float)objects[1]);
            });
            addFunction("floatround", "Float - Round", typeof(int), new Type[] { typeof(float) }, "~ rounded to the nearest integer", delegate(EvaluatorState state, object[] objects)
            {
                float f = (float)objects[0];
                return (int)(f + .5);
            });
            addFunction("floatcomparison", "Float, Float - Compare", typeof(bool), new Type[] { typeof(float), typeof(GreaterLessThan), typeof(float) }, "~ ~ ~", delegate(EvaluatorState state, object[] objects)
            {
                float f1 = (float)(objects[0]);
                float f2 = (float)(objects[2]);
                GreaterLessThan glt = (GreaterLessThan)(objects[1]);
                return FloatComparer.compare(f1, f2, glt);
            });
            addFunction("or", "Bool, Bool - Or", typeof(bool), new Type[] { typeof(bool), typeof(bool) }, "~ or ~", delegate(EvaluatorState state, object[] objects)
            {
                return ((bool)objects[0]) || ((bool)objects[1]);
            });
            addFunction("and", "Bool, Bool - And", typeof(bool), new Type[] { typeof(bool), typeof(bool) }, "~ and ~", delegate(EvaluatorState state, object[] objects)
            {
                return ((bool)objects[0]) && ((bool)objects[1]);
            });
            addFunction("+", "Float, Float - Add", typeof(float), new Type[] { typeof(float), typeof(float) }, "~ + ~", delegate(EvaluatorState state, object[] objects)
            {
                return ((float)objects[0]) + ((float)objects[1]);
            });
            addFunction("*", "Float, Float - Multiply", typeof(float), new Type[] { typeof(float), typeof(float) }, "~ * ~", delegate(EvaluatorState state, object[] objects)
            {
                return ((float)objects[0]) * ((float)objects[1]);
            });
            addFunction("-", "Float, Float - Subtract", typeof(float), new Type[] { typeof(float), typeof(float) }, "~ - ~", delegate(EvaluatorState state, object[] objects)
            {
                return ((float)objects[0]) - ((float)objects[1]);
            });
            addFunction("min", "Float, Float - Minimum", typeof(float), new Type[] { typeof(float), typeof(float) }, "min( ~ , ~ )", delegate(EvaluatorState state, object[] objects)
            {
                return Math.Min((float)objects[0], (float)objects[1]);
            });
            addFunction("max", "Float, Float - Maximum", typeof(float), new Type[] { typeof(float), typeof(float) }, "max( ~ , ~ )", delegate(EvaluatorState state, object[] objects)
            {
                return Math.Max((float)objects[0], (float)objects[1]);
            });
            addFunction("intmin", "int, int - Minimum", typeof(float), new Type[] { typeof(int), typeof(int) }, "min( ~ , ~ )", delegate(EvaluatorState state, object[] objects)
            {
                return Math.Min((int)objects[0], (int)objects[1]);
            });
            addFunction("intmax", "int, int - Maximum", typeof(float), new Type[] { typeof(int), typeof(int) }, "max( ~ , ~ )", delegate(EvaluatorState state, object[] objects)
            {
                return Math.Max((int)objects[0], (int)objects[1]);
            });
            addFunction("<", "Float, Float - Less than", typeof(bool), new Type[] { typeof(float), typeof(float) }, "~ < ~", delegate(EvaluatorState state, object[] objects)
            {
                return ((float)objects[0]) < ((float)objects[1]);
            });
            addFunction("intadd", "int, int - Add", typeof(int), new Type[] { typeof(int), typeof(int) }, "~ + ~", delegate(EvaluatorState state, object[] objects)
            {
                return ((int)objects[0]) + ((int)objects[1]);
            });
            addFunction("rand", " - Rand", typeof(float), new Type[] { }, "A random number between 0 and 1", delegate(EvaluatorState state, object[] objects)
            {
                return (float)r.NextDouble();
            });
            addFunction("floatAbs", "Float - Absolute Value", typeof(float), new Type[] { typeof(float) }, "The absolute value of ~", delegate(EvaluatorState state, object[] objects)
            {
                return Math.Abs((float)objects[0]);
            });
            addFunction("intAbs", "Int - Absolute Value", typeof(int), new Type[] { typeof(int) }, "The absolute value of ~", delegate(EvaluatorState state, object[] objects)
            {
                return Math.Abs((int)objects[0]);
            });
            #endregion

            #region misc
            addFunction("numourbots", " - # our robots", typeof(int), new Type[] { }, "The number of robots currently on our team", delegate(EvaluatorState state, object[] objects)
            {
                return state.OurTeamInfo.Length;
            });
            addFunction("numtheirbots", " - # their robots", typeof(int), new Type[] { }, "The number of robots currently on their team", delegate(EvaluatorState state, object[] objects)
            {
                return state.TheirTeamInfo.Length;
            });
            #endregion

            #region actions
            addFunction("robotpointmove", "Robot, Point - Move", typeof(ActionDefinition), new Type[] { typeof(Robot), typeof(Vector2) }, "Have robot ~ move to ~", delegate(EvaluatorState state, object[] objects)
            {
                /*return delegate(Commander c){
                    Vector2 p=((Vector2)objects[1]).getPoint();
                    c.move(((PlayRobot)objects[0]).getID(), p.X, p.Y);
                };*/
                Robot robot = (Robot)objects[0];
                return new ActionDefinition(delegate(IActionInterpreter a)
                {
                    Vector2 p = (Vector2)objects[1];
                    a.Move(robot.getID(), p);
                    //c.move(robot.getID(), p.X, p.Y);
                }, robot.getID());
                //return a;
            });
            addFunction("robotpointkick", "Robot, Point - Kick", typeof(ActionDefinition), new Type[] { typeof(Robot), typeof(Vector2) }, "Have robot ~ kick the ball to ~", delegate(EvaluatorState state, object[] objects)
            {
                Robot robot = (Robot)objects[0];
                Vector2 p = (Vector2)objects[1];
                return new ActionDefinition(delegate(IActionInterpreter a)
                {
                    a.Kick(robot.getID(), p);
                }, robot.getID());
            });
            addFunction("robotpointdribble", "Robot, Point - Dribble", typeof(ActionDefinition), new Type[] { typeof(Robot), typeof(Vector2) }, "Have robot ~ dribble the ball to ~", delegate(EvaluatorState state, object[] objects)
            {
                Robot robot = (Robot)objects[0];
                Vector2 p = (Vector2)objects[1];
                return new ActionDefinition(delegate(IActionInterpreter a)
                {
                    a.Dribble(robot.getID(), p);
                }, robot.getID());
            });
            addFunction("robotpointpointmove", "Robot, Point, Point - Move", typeof(ActionDefinition), new Type[] { typeof(Robot), typeof(Vector2), typeof(Vector2) }, "Have robot ~ move to ~, and face ~", delegate(EvaluatorState state, object[] objects)
            {
                Robot robot = (Robot)objects[0];
                return new ActionDefinition(delegate(IActionInterpreter a)
                {
                    Vector2 target = (Vector2)objects[1];
                    Vector2 facing = (Vector2)objects[2];
                    a.Move(robot.getID(), target, facing);
                }, robot.getID());
            });
            addFunction("donothing", "Robot - Do Nothing", typeof(ActionDefinition), new Type[] { typeof(Robot) }, "Have robot ~ stop and do nothing", delegate(EvaluatorState state, object[] objects)
            {
                Robot robot = (Robot)objects[0];
                return new ActionDefinition(delegate(IActionInterpreter a)
                {
                    a.Stop(robot.getID());
                }/*, robot.getID()*/);
            });
            #endregion
        }
        static Function()
        {
            loadFunctions();
        }
    }
}
