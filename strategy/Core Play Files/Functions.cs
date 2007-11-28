using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Robocup.Geometry;
using Robocup.Core;

namespace Robocup.Plays
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
            if (t == typeof(double))
                return "<double>";
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

        private static void addIf(Type t)
        {
            string name = cleanUpName(getStringFromType(t));
            addFunction("if_" + name, "If , Then - " + name, "If ~, then ~, else ~", t, new Type[] { typeof(bool), t, t }, delegate(EvaluatorState state, object[] objects)
            {
                if ((bool)objects[0])
                    return objects[1];
                else
                    return objects[2];
            });
        }
        private static void addAll(Action<Type> function, params Type[] types)
        {
            foreach (Type t in types)
            {
                function(t);
            }
        }
        #endregion


        /// <summary>
        /// Adds a function to the list of all functions
        /// </summary>
        /// <param name="name">The name of the function, that you use when calling it (ex: "linelength")</param>
        /// <param name="longname">A slightly longer, more descriptive name (ex: "Line -> Length")</param>
        /// <param name="description">A description of the function, with tildes (~) where you want the values of the arguments to go
        /// (ex: "The length of line ~"</param>
        /// <param name="returnType">The type of object that gets returned (ex: typeof(double))</param>
        /// <param name="argTypes">An array listing the argument types that it takes (ex: new Type[]{typeof(Line)})</param>
        /// <param name="run">The actual function logic.  It gets passed an EvaluatorState (which can usually be ignored)
        /// and an array of objects of the evaluated arguments.</param>
        private static void addFunction(string name, string longname, string description, Type returnType, Type[] argTypes, FunctionRunDelegate run)
        {
            addFunction(new Function(name, longname, description, returnType, argTypes, run));
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
            addAll(addIf, typeof(int), typeof(double), typeof(Vector2), typeof(Line), typeof(Circle), typeof(ActionDefinition));

            #region geometric functions
            addFunction("point", "X, Y - Point", "The point ~, ~", typeof(Vector2), new Type[] { typeof(double), typeof(double) }, delegate(EvaluatorState state, object[] objects)
            {
                return new Vector2((double)objects[0], (double)objects[1]);
            });
            addFunction("getXcoord", "Point - X coordinate", "The x coordinate of point ~", typeof(double), new Type[] { typeof(Vector2) }, delegate(EvaluatorState state, object[] objects)
            {
                return ((Vector2)objects[0]).X;
            });
            addFunction("getYcoord", "Point - Y coordinate", "The y coordinate of point ~", typeof(double), new Type[] { typeof(Vector2) }, delegate(EvaluatorState state, object[] objects)
            {
                return ((Vector2)objects[0]).Y;
            });
            addFunction("line", "Point, Point - Line", "The line connecting ~ and ~", typeof(Line), new Type[] { typeof(Vector2), typeof(Vector2) }, delegate(EvaluatorState state, object[] objects)
            {
                return new Line((Vector2)objects[0], (Vector2)objects[1]);
            });
            addFunction("circle", "Point, Radius - Circle", "The circle with ~ as its center, and radius ~", typeof(Circle), new Type[] { typeof(Vector2), typeof(double) }, delegate(EvaluatorState state, object[] objects)
            {
                return new Circle((Vector2)objects[0], (double)objects[1]);
            });
            addFunction("pointof", "Point-object - Point", "The point of ~", typeof(Vector2), new Type[] { typeof(GetPointable) }, delegate(EvaluatorState state, object[] objects)
            {
                return ((GetPointable)objects[0]).getPoint();
                //return new Vector2((GetPointable)objects[0]);
            });
            addFunction("linelineintersection", "Line, Line - Intersection", "The intersection of lines ~ and ~", typeof(Vector2), new Type[] { typeof(Line), typeof(Line) }, delegate(EvaluatorState state, object[] objects)
            {
                Line l1 = (Line)objects[0];
                Line l2 = (Line)objects[1];
                return Intersections.intersect(l1, l2);
                //return new Vector2(new LineLineIntersection(l1, l2));
            });
            addFunction("linecircleintersection", "Line, Circle - Intersection", "The intersection of line ~ and circle ~, number ~", typeof(Vector2), new Type[] { typeof(Line), typeof(Circle), typeof(int) }, delegate(EvaluatorState state, object[] objects)
            {
                Line line = (Line)objects[0];
                Circle circle = (Circle)objects[1];
                return Intersections.intersect(line, circle, (int)objects[2]);
                //return new Vector2(new LineCircleIntersection(line, circle, (int)objects[2]));
            });
            addFunction("circlecircleintersection", "Circle, Circle - Intersection", "The intersection of circles ~ and ~, number ~", typeof(Vector2), new Type[] { typeof(Circle), typeof(Circle), typeof(int) }, delegate(EvaluatorState state, object[] objects)
            {
                Circle c1 = (Circle)objects[0];
                Circle c2 = (Circle)objects[1];
                return Intersections.intersect(c1, c2, (int)objects[2]);
                //return new Vector2(new PlayCircleCircleIntersection(c1, c2, (int)objects[2]));
            });
            addFunction("pointpointdistance", "Point, Point - Distance", "The distance between ~ and ~ (in meters)", typeof(double), new Type[] { typeof(Vector2), typeof(Vector2) }, delegate(EvaluatorState state, object[] objects)
            {
                /*Vector2 p1 = (Vector2)objects[0];
                Vector2 p2 = (Vector2)objects[1];
                return CommonFunctions.distance(p1.getPoint(), p2.getPoint());*/
                return UsefulFunctions.distance((Vector2)objects[0], (Vector2)objects[1]);
            });
            addFunction("linelength", "Line - Length", "The length of line ~", typeof(double), new Type[] { typeof(Line) }, delegate(EvaluatorState state, object[] objects)
            {
                Vector2[] points = ((Line)objects[0]).getPoints();
                return UsefulFunctions.distance(points[0], points[1]);
            });

            addFunction("scalepoint", "Point, double - Scale", "The point ~ scaled by ~", typeof(Vector2), new Type[] { typeof(Vector2), typeof(double) }, delegate(EvaluatorState state, object[] objects)
            {
                return ((double)objects[1]) * ((Vector2)objects[0]);
            });
            addFunction("pointadd", "Point, Point - Add", "Point ~ + Point ~", typeof(Vector2), new Type[] { typeof(Vector2), typeof(Vector2) }, delegate(EvaluatorState state, object[] objects)
            {
                return ((Vector2)objects[0]) + ((Vector2)objects[1]);
            });
            addFunction("pointsubtract", "Point, Point - Subtract", "Point ~ - Point ~", typeof(Vector2), new Type[] { typeof(Vector2), typeof(Vector2) }, delegate(EvaluatorState state, object[] objects)
            {
                return ((Vector2)objects[0]) - ((Vector2)objects[1]);
            });
            addFunction("midpoint", "Point, Point - Midpoint", "The midpoint of points ~ and ~", typeof(Vector2), new Type[] { typeof(Vector2), typeof(Vector2) }, delegate(EvaluatorState state, object[] objects)
            {
                return .5 * (((Vector2)objects[0]) + ((Vector2)objects[1]));
            });
            addFunction("closestRobotToLine", "Line, Team - closest robot", "The distance from the line ~ to the closest robot on team ~ (excluding the robots that are the endpoints of the line)", typeof(double), new Type[] { typeof(Line), typeof(TeamCondition) }, delegate(EvaluatorState state, object[] objects)
            {
                double rtn = 1000000;
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
                    rtn = Math.Min(rtn, line.distFromSegment(position));
                }
                return rtn;
            });
            addFunction("linearInterpolation", "Point, Point, Fraction - Point on the line of that fraction", "The point that is between Point ~ and Point ~ for some Fraction ~", typeof(Vector2), new Type[] { typeof(Vector2), typeof(Vector2), typeof(double) }, delegate(EvaluatorState state, object[] objects)
            {
                Vector2 p1 = (Vector2)objects[0];
                Vector2 p2 = (Vector2)objects[1];
                double fraction = (double)objects[2];
                Vector2 v = new Vector2(fraction * (p2.X - p1.X), fraction * (p2.Y - p1.Y));
                return new Vector2(p1.X + v.X, p1.Y + v.Y);
            });
            addFunction("numberOfRobotsInACircle", "Circle, Team - Number of Robots", "In a circle ~, the number of Robots of the Team ~ in it", typeof(int), new Type[] { typeof(Circle), typeof(TeamCondition) }, delegate(EvaluatorState state, object[] objects)
            {
                int count = 0;
                TeamCondition condition = (TeamCondition)objects[1];
                List<RobotInfo> allinfos = new List<RobotInfo>();
                if (condition.maybeOurs())
                    allinfos.AddRange(state.OurTeamInfo);
                if (condition.maybeTheirs())
                    allinfos.AddRange(state.TheirTeamInfo);
                foreach (RobotInfo r in allinfos)
                {
                    if (((Circle)objects[0]).distanceFromCenter(r.Position) <= ((Circle)objects[0]).Radius)
                        count++;
                }
                return count;
            });
            addFunction("angleBetweenTwoLines", "Line,Line - Angle", "The angle between line ~ and line ~", typeof(double), new Type[] { typeof(Line), typeof(Line) }, delegate(EvaluatorState state, object[] objects)
            {
                Line L1 = (Line)objects[0];
                Line L2 = (Line)objects[1];
                Vector2 p1 = Intersections.intersect(L1, L2);
                if (p1 == null)
                    throw new NoIntersectionException("Lines do not intersect, No angle");
                Vector2[] points1 = L1.getPoints();
                Vector2[] points2 = L2.getPoints();
                Vector2 p2 = points1[0];
                Vector2 p3 = points2[0];
                Vector2 v1, v2;
                v1 = new Vector2(p1.X - p2.X, p1.Y - p2.Y);
                v2 = new Vector2(p1.X - p3.X, p1.Y - p3.Y);
                //p1 is the vertex of the angle
                double dotproduct = v1.X * v2.X + v1.Y * v2.Y;
                double angle = Math.Acos(dotproduct / Math.Sqrt((v1.X * v1.X + v1.Y * v1.Y) * (v2.X * v2.X + v2.Y * v2.Y)));
                return angle;
            });
            addFunction("angleBetweenThreePoints", "Point,Point,Point - Angle", "The angle between the rays connecting Point ~ with Point ~ and Point ~", typeof(double), new Type[] { typeof(Vector2), typeof(Vector2), typeof(Vector2) }, delegate(EvaluatorState state, object[] objects)
            {
                Vector2 p1, p2, p3;
                p1 = (Vector2)objects[0];
                p2 = (Vector2)objects[1];
                p3 = (Vector2)objects[2];
                Vector2 v1, v2;
                v1 = new Vector2(p1.X - p2.X, p1.Y - p2.Y);
                v2 = new Vector2(p1.X - p3.X, p1.Y - p3.Y);
                //p1 is the vertex of the angle
                double dotproduct = v1.X * v2.X + v1.Y * v2.Y;
                double angle = Math.Acos(dotproduct / Math.Sqrt((v1.X * v1.X + v1.Y * v1.Y) * (v2.X * v2.X + v2.Y * v2.Y)));
                return angle;
            });
            addFunction("rotatePointAroundAnotherPoint", "Point, Point, Angle - Point", "Around a center point ~, Rotate a Point ~ for an angle ~", typeof(Vector2), new Type[] { typeof(Vector2), typeof(Vector2), typeof(double) }, delegate(EvaluatorState state, object[] objects)
            {
                Vector2 p1, p2;
                p1 = (Vector2)objects[0];
                p2 = (Vector2)objects[1];
                double angle = (double)objects[2];
                Vector2 v = new Vector2(p2.X - p1.X, p2.Y - p1.Y);
                double originalAngle = v.cartesianAngle();
                double length = Math.Sqrt(v.magnitudeSq());
                return new Vector2(p1.X + Math.Cos(originalAngle + angle) * length, p1.Y + Math.Sin(originalAngle + angle) * length);
            });

            #endregion

            #region arithmetic functions
            addFunction("/", "double, double - Divide", "~ / ~", typeof(double), new Type[] { typeof(double), typeof(double) }, delegate(EvaluatorState state, object[] objects)
            {
                return ((double)objects[0]) / ((double)objects[1]);
            });
            addFunction("doubleround", "double - Round", "~ rounded to the nearest integer", typeof(int), new Type[] { typeof(double) }, delegate(EvaluatorState state, object[] objects)
            {
                double f = (double)objects[0];
                return (int)(f + .5);
            });
            addFunction("doublecomparison", "double, double - Compare", "~ ~ ~", typeof(bool), new Type[] { typeof(double), typeof(GreaterLessThan), typeof(double) }, delegate(EvaluatorState state, object[] objects)
            {
                double f1 = (double)(objects[0]);
                double f2 = (double)(objects[2]);
                GreaterLessThan glt = (GreaterLessThan)(objects[1]);
                return doubleComparer.compare(f1, f2, glt);
            });
            addFunction("or", "Bool, Bool - Or", "~ or ~", typeof(bool), new Type[] { typeof(bool), typeof(bool) }, delegate(EvaluatorState state, object[] objects)
            {
                return ((bool)objects[0]) || ((bool)objects[1]);
            });
            addFunction("and", "Bool, Bool - And", "~ and ~", typeof(bool), new Type[] { typeof(bool), typeof(bool) }, delegate(EvaluatorState state, object[] objects)
            {
                return ((bool)objects[0]) && ((bool)objects[1]);
            });
            addFunction("+", "double, double - Add", "~ + ~", typeof(double), new Type[] { typeof(double), typeof(double) }, delegate(EvaluatorState state, object[] objects)
            {
                return ((double)objects[0]) + ((double)objects[1]);
            });
            addFunction("*", "double, double - Multiply", "~ * ~", typeof(double), new Type[] { typeof(double), typeof(double) }, delegate(EvaluatorState state, object[] objects)
            {
                return ((double)objects[0]) * ((double)objects[1]);
            });
            addFunction("-", "double, double - Subtract", "~ - ~", typeof(double), new Type[] { typeof(double), typeof(double) }, delegate(EvaluatorState state, object[] objects)
            {
                return ((double)objects[0]) - ((double)objects[1]);
            });
            addFunction("min", "double, double - Minimum", "min( ~ , ~ )", typeof(double), new Type[] { typeof(double), typeof(double) }, delegate(EvaluatorState state, object[] objects)
            {
                return Math.Min((double)objects[0], (double)objects[1]);
            });
            addFunction("max", "double, double - Maximum", "max( ~ , ~ )", typeof(double), new Type[] { typeof(double), typeof(double) }, delegate(EvaluatorState state, object[] objects)
            {
                return Math.Max((double)objects[0], (double)objects[1]);
            });
            addFunction("intmin", "int, int - Minimum", "min( ~ , ~ )", typeof(double), new Type[] { typeof(int), typeof(int) }, delegate(EvaluatorState state, object[] objects)
            {
                return Math.Min((int)objects[0], (int)objects[1]);
            });
            addFunction("intmax", "int, int - Maximum", "max( ~ , ~ )", typeof(double), new Type[] { typeof(int), typeof(int) }, delegate(EvaluatorState state, object[] objects)
            {
                return Math.Max((int)objects[0], (int)objects[1]);
            });
            addFunction("<", "double, double - Less than", "~ < ~", typeof(bool), new Type[] { typeof(double), typeof(double) }, delegate(EvaluatorState state, object[] objects)
            {
                return ((double)objects[0]) < ((double)objects[1]);
            });
            addFunction("intadd", "int, int - Add", "~ + ~", typeof(int), new Type[] { typeof(int), typeof(int) }, delegate(EvaluatorState state, object[] objects)
            {
                return ((int)objects[0]) + ((int)objects[1]);
            });
            addFunction("rand", " - Rand", "A random number between 0 and 1", typeof(double), new Type[] { }, delegate(EvaluatorState state, object[] objects)
            {
                return (double)r.NextDouble();
            });
            addFunction("doubleAbs", "double - Absolute Value", "The absolute value of ~", typeof(double), new Type[] { typeof(double) }, delegate(EvaluatorState state, object[] objects)
            {
                return Math.Abs((double)objects[0]);
            });
            addFunction("intAbs", "Int - Absolute Value", "The absolute value of ~", typeof(int), new Type[] { typeof(int) }, delegate(EvaluatorState state, object[] objects)
            {
                return Math.Abs((int)objects[0]);
            });
            #endregion

            #region misc
            addFunction("numourbots", " - # our robots", "The number of robots currently on our team", typeof(int), new Type[] { }, delegate(EvaluatorState state, object[] objects)
            {
                return state.OurTeamInfo.Length;
            });
            addFunction("numtheirbots", " - # their robots", "The number of robots currently on their team", typeof(int), new Type[] { }, delegate(EvaluatorState state, object[] objects)
            {
                return state.TheirTeamInfo.Length;
            });
            #endregion

            #region actions
            addFunction("robotpointmove", "Robot, Point - Move", "Have robot ~ move to ~", typeof(ActionDefinition), new Type[] { typeof(Robot), typeof(Vector2) }, delegate(EvaluatorState state, object[] objects)
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
            addFunction("robotpointkick", "Robot, Point - Kick", "Have robot ~ kick the ball to ~", typeof(ActionDefinition), new Type[] { typeof(Robot), typeof(Vector2) }, delegate(EvaluatorState state, object[] objects)
            {
                Robot robot = (Robot)objects[0];
                Vector2 p = (Vector2)objects[1];
                return new ActionDefinition(delegate(IActionInterpreter a)
                {
                    a.Kick(robot.getID(), p);
                }, robot.getID());
            });
            addFunction("robotpointdribble", "Robot, Point - Dribble", "Have robot ~ dribble the ball to ~", typeof(ActionDefinition), new Type[] { typeof(Robot), typeof(Vector2) }, delegate(EvaluatorState state, object[] objects)
            {
                Robot robot = (Robot)objects[0];
                Vector2 p = (Vector2)objects[1];
                return new ActionDefinition(delegate(IActionInterpreter a)
                {
                    a.Dribble(robot.getID(), p);
                }, robot.getID());
            });
            addFunction("robotpointpointmove", "Robot, Point, Point - Move", "Have robot ~ move to ~, and face ~", typeof(ActionDefinition), new Type[] { typeof(Robot), typeof(Vector2), typeof(Vector2) }, delegate(EvaluatorState state, object[] objects)
            {
                Robot robot = (Robot)objects[0];
                return new ActionDefinition(delegate(IActionInterpreter a)
                {
                    Vector2 target = (Vector2)objects[1];
                    Vector2 facing = (Vector2)objects[2];
                    a.Move(robot.getID(), target, facing);
                }, robot.getID());
            });
            addFunction("donothing", "Robot - Do Nothing", "Have robot ~ stop and do nothing", typeof(ActionDefinition), new Type[] { typeof(Robot) }, delegate(EvaluatorState state, object[] objects)
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
