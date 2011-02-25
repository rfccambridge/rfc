using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Robocup.Geometry;
using Robocup.Core;
using Robocup.Utilities;

namespace Robocup.Plays
{
    partial class Function
    {
        private static readonly Random r = new Random();
        private static readonly List<Function> Functions = new List<Function>();

        private static Dictionary<String, Object> session = new Dictionary<string, object>();

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
            else if (t == typeof(bool))
                return "<bool>";
            else if (t == typeof(ActionDefinition))
                return "<Action>";
            else if (t == typeof(Robot))
                return "<robot>";
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
        private static void addConstruct(Type t)
        {
            string name = cleanUpName(getStringFromType(t));
            addFunction(name, name, "Declare type-safe " + name, t, new Type[] { t }, delegate(EvaluatorState state, object[] objects)
            {
                return objects[0];
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
            addAll(addIf, typeof(int), typeof(double), typeof(Vector2), typeof(Line), typeof(Circle),
                   typeof(ActionDefinition), typeof(Robot));

            addAll(addConstruct, typeof(int), typeof(double), typeof(bool));

            #region geometric functions

            addFunction("point", "X, Y - Point", "The point ~, ~", typeof(Vector2),
                        new Type[] { typeof(double), typeof(double) }, delegate(EvaluatorState state, object[] objects)
            {
                return new Vector2((double)objects[0], (double)objects[1]);
            });
            addFunction("getXcoord", "Point - X coordinate", "The x coordinate of point ~", typeof(double),
                        new Type[] { typeof(Vector2) }, delegate(EvaluatorState state, object[] objects)
            {
                return ((Vector2)objects[0]).X;
            });
            addFunction("getYcoord", "Point - Y coordinate", "The y coordinate of point ~", typeof(double),
                        new Type[] { typeof(Vector2) }, delegate(EvaluatorState state, object[] objects)
            {
                return ((Vector2)objects[0]).Y;
            });
            addFunction("getAngle", "Point - cartesian angle", "The angle of point ~, CCW from the x axis", typeof(double),
                        new Type[] { typeof(Vector2) }, delegate(EvaluatorState state, object[] objects)
            {
                return ((Vector2)objects[0]).cartesianAngle();
            });
            addFunction("line", "Point, Point - Line", "The line connecting ~ and ~", typeof(Line),
                        new Type[] { typeof(Vector2), typeof(Vector2) }, delegate(EvaluatorState state, object[] objects)
            {
                return new Line((Vector2)objects[0], (Vector2)objects[1]);
            });
            addFunction("linedirection", "Point, Direction - Line", "The unit length line from point ~ oriented at ~", typeof(Line),
                        new Type[] { typeof(Vector2), typeof(double) }, delegate(EvaluatorState state, object[] objects)
            {
                return new Line((Vector2)objects[0], (double)objects[1]);
            });
            addFunction("circle", "Point, Radius - Circle", "The circle with ~ as its center, and radius ~",
                        typeof(Circle), new Type[] { typeof(Vector2), typeof(double) }, delegate(EvaluatorState state, object[] objects)
            {
                return new Circle((Vector2)objects[0], (double)objects[1]);
            });
            addFunction("pointof", "Point-object - Point", "The point of ~", typeof(Vector2),
                        new Type[] { typeof(GetPointable) }, delegate(EvaluatorState state, object[] objects)
            {
                return ((GetPointable)objects[0]).getPoint();
            });
            addFunction("orientationof", "orientation of a robot", "The orientation of robot ~", typeof(double), new Type[] { typeof(Robot) }, delegate(EvaluatorState state, object[] objects)
            {
                return ((Robot)objects[0]).getOrientation();
            });
            addFunction("velocityof", "Point-object - Velocity", "The velocity of ~", typeof(Vector2),
                        new Type[] { typeof(GetPointable) }, delegate(EvaluatorState state, object[] objects)
            {
                return ((GetPointable)objects[0]).getVelocity();
            });
            addFunction("linelineintersection", "Line, Line - Intersection", "The intersection of lines ~ and ~",
                        typeof(Vector2), new Type[] { typeof(Line), typeof(Line) },
                        delegate(EvaluatorState state, object[] objects)
                        {
                            Line l1 = (Line)objects[0];
                            Line l2 = (Line)objects[1];
                            return Intersections.intersect(l1, l2);
                            //return new Vector2(new LineLineIntersection(l1, l2));
                        });
            addFunction("linecircleintersection", "Line, Circle - Intersection",
                        "The intersection of line ~ and circle ~, number ~", typeof(Vector2),
                        new Type[] { typeof(Line), typeof(Circle), typeof(int) },
                        delegate(EvaluatorState state, object[] objects)
                        {
                            Line line = (Line)objects[0];
                            Circle circle = (Circle)objects[1];
                            return Intersections.intersect(line, circle, (int)objects[2]);
                            //return new Vector2(new LineCircleIntersection(line, circle, (int)objects[2]));
                        });
            addFunction("circlecircleintersection", "Circle, Circle - Intersection",
                        "The intersection of circles ~ and ~, number ~", typeof(Vector2),
                        new Type[] { typeof(Circle), typeof(Circle), typeof(int) },
                        delegate(EvaluatorState state, object[] objects)
                        {
                            Circle c1 = (Circle)objects[0];
                            Circle c2 = (Circle)objects[1];
                            return Intersections.intersect(c1, c2, (int)objects[2]);
                            //return new Vector2(new PlayCircleCircleIntersection(c1, c2, (int)objects[2]));
                        });
            addFunction("pointpointdistance", "Point, Point - Distance", "The distance between ~ and ~ (in meters)",
                        typeof(double), new Type[] { typeof(Vector2), typeof(Vector2) },
                        delegate(EvaluatorState state, object[] objects)
                        {
                            return UsefulFunctions.distance((Vector2)objects[0], (Vector2)objects[1]);
                        });
            addFunction("linelength", "Line - Length", "The length of line ~", typeof(double), new Type[] { typeof(Line) },
                        delegate(EvaluatorState state, object[] objects)
                        {
                            Vector2[] points = ((Line)objects[0]).getPoints();
                            return UsefulFunctions.distance(points[0], points[1]);
                        });
            addFunction("scalepoint", "Point, double - Scale", "The point ~ scaled by ~", typeof(Vector2),
                        new Type[] { typeof(Vector2), typeof(double) }, delegate(EvaluatorState state, object[] objects)
            {
                return ((double)objects[1]) * ((Vector2)objects[0]);
            });
            addFunction("pointadd", "Point, Point - Add", "Point ~ + Point ~", typeof(Vector2),
                        new Type[] { typeof(Vector2), typeof(Vector2) }, delegate(EvaluatorState state, object[] objects)
            {
                return ((Vector2)objects[0]) + ((Vector2)objects[1]);
            });
            addFunction("pointsubtract", "Point, Point - Subtract", "Point ~ - Point ~", typeof(Vector2),
            new Type[] { typeof(Vector2), typeof(Vector2) }, delegate(EvaluatorState state, object[] objects)
            {
                return ((Vector2)objects[0]) - ((Vector2)objects[1]);
            });
            addFunction("midpoint", "Point, Point - Midpoint", "The midpoint of points ~ and ~", typeof(Vector2),
                        new Type[] { typeof(Vector2), typeof(Vector2) }, delegate(EvaluatorState state, object[] objects)
            {
                return .5 * (((Vector2)objects[0]) + ((Vector2)objects[1]));
            });
            addFunction("closestRobotToLine", "Line, Team - closest robot",
                        "The distance from the line ~ to the closest robot on team ~ (excluding the robots that are the endpoints of the line)",
                        typeof(double), new Type[] { typeof(Line), typeof(TeamCondition) },
                        delegate(EvaluatorState state, object[] objects)
                        {
                            double rtn = 1000000;
#if DEBUG
                            if (state == null)
                                throw new ApplicationException(
                                    "tried to call a function that needed an evaluatorState without one!");
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

            addFunction("pointAboveLine", "Point, Line - Above", "Whether point ~ is above line ~", typeof(bool),
                        new Type[] { typeof(Vector2), typeof(Line) }, delegate(EvaluatorState state, object[] objects)
            {
                Vector2 point = (Vector2)objects[0];
                Line line = (Line)objects[1];

                return pointAboveLine(point, line);
            });

            addFunction("pathClear", "Point, Point, double - Path clear",
                        "Whether the path from point ~ to point ~ has no obstacles within distance ~", typeof(bool),
                        new Type[] { typeof(Vector2), typeof(Vector2), typeof(double) },
                        delegate(EvaluatorState state, object[] objects)
                        {
                            Vector2 p1 = (Vector2)objects[0];
                            Vector2 p2 = (Vector2)objects[1];
                            double mindist = (double)objects[2];

                            Line line = new Line(p1, p2);

                            // get distance of closest robot to line
                            List<RobotInfo> allinfos = new List<RobotInfo>();
                            if (state != null)  //designer-friendly
                            {
                                allinfos.AddRange(state.OurTeamInfo);
                                allinfos.AddRange(state.TheirTeamInfo);
                            }
                            Vector2[] endpoints = line.getPoints();

                            double rtn = 10000;

                            foreach (RobotInfo r in allinfos)
                            {
                                Vector2 position = r.Position;
                                if (Math.Sqrt(position.distanceSq(endpoints[0])) < mindist ||
                                    Math.Sqrt(position.distanceSq(endpoints[1])) < mindist)
                                    continue;
                                rtn = Math.Min(rtn, line.distFromSegment(position));
                            }

                            return (rtn >= mindist);

                        });

            addFunction("robotInGoal", "Robot in Goal",
                        "Whether there is a robot (any robot) in a goal", typeof(bool),
                        new Type[] { },
                        delegate(EvaluatorState state, object[] objects)
                        {
                            double GOAL_BOX_X = -2.5;
                            double GOAL_BOX_Y = .175;

                            List<Vector2> GOAL_CORNERS = new List<Vector2>(new Vector2[] { new Vector2(-3, .175), new Vector2(-3, -.175) });
                            double QUARTER_CIRCLE_RADIUS = .5;

                            // get info about each of our robots
                            foreach (RobotInfo robot in state.OurTeamInfo)
                            {
                                // figure out if the robot is in the goal
                                Vector2 position = robot.Position;

                                // first see if it is in the square
                                if (position.X < GOAL_BOX_X && Math.Abs(position.Y) < GOAL_BOX_Y)
                                {
                                    // in that square part of the goal
                                    //Console.WriteLine(position.ToString() + " in goal box", ProjectDomains.Plays);
                                    return true;
                                }

                                // Otherwise, see whether it is within either of those quarter circles
                                foreach (Vector2 corner in GOAL_CORNERS)
                                {
                                    if (corner.distanceSq(position) < QUARTER_CIRCLE_RADIUS * QUARTER_CIRCLE_RADIUS)
                                    {
                                        // is within a quarter circle
                                        //Console.WriteLine(position.ToString() + " in goal circles", ProjectDomains.Plays);
                                        return true;
                                    }
                                }
                            }

                            // no robot is in the goal
                            return false;

                        });

            // SO SAD THAT MUCH OF THIS IS COPIED AND PASTED FROM ABOVE, BUT THERE WAS NO 
            // PARTICULARLY GOOD PLACE TO PUT THE FUNCTION
            addFunction("pointInGoal", "Point in Goal",
                        "Whether point ~ is in our goal", typeof(bool),
                        new Type[] { typeof(Vector2) },
                        delegate(EvaluatorState state, object[] objects)
                        {
                            Vector2 position = (Vector2)objects[0];

                            double GOAL_BOX_X = -2.5;
                            double GOAL_BOX_Y = .175;

                            List<Vector2> GOAL_CORNERS = new List<Vector2>(new Vector2[] { new Vector2(-3, .175), new Vector2(-3, -.175) });
                            double QUARTER_CIRCLE_RADIUS = .5;

                            // get info about each of our robots

                            // first see if it is in the square
                            if (position.X < GOAL_BOX_X && Math.Abs(position.Y) < GOAL_BOX_Y)
                            {
                                // in that square part of the goal
                                //Console.WriteLine(position.ToString() + " in goal box", ProjectDomains.Plays);
                                return true;
                            }

                            // Otherwise, see whether it is within either of those quarter circles
                            foreach (Vector2 corner in GOAL_CORNERS)
                            {
                                if (corner.distanceSq(position) < QUARTER_CIRCLE_RADIUS * QUARTER_CIRCLE_RADIUS)
                                {
                                    // is within a quarter circle
                                    //Console.WriteLine(position.ToString() + " in goal circles", ProjectDomains.Plays);
                                    return true;
                                }
                            }


                            // no robot is in the goal
                            return false;

                        });

            addFunction("inField", "Point inside field boundaries", "Whether the point ~ is inside the field",
                        typeof(bool), new Type[] { typeof(Vector2) }, delegate(EvaluatorState state, object[] objects)
            {
                Vector2 point = (Vector2)objects[0];
                return TacticsEval.InField(point);
            });

            /*addFunction("closest_now", "Team, point - closest robot", "return the nearest robot to the point", typeof(Robot), new Type[] { typeof(TeamCondition), typeof(Vector2) }, delegate(EvaluatorState state, object[] objects)
            {
#if DEBUG
                if (state == null) throw new ApplicationException("tried to call a function that needed an evaluatorState without one!");
#endif
                TeamCondition condition = (TeamCondition)objects[0];
                Vector2 point = (Vector2)objects[1];

                List<RobotInfo> allinfos = new List<RobotInfo>();
                if (condition.maybeOurs())
                    allinfos.AddRange(state.OurTeamInfo);
                if (condition.maybeTheirs())
                    allinfos.AddRange(state.TheirTeamInfo);

                double nearest = 10000;
                Robot closest_robot;

                foreach (RobotInfo r in allinfos)
                {
                    Vector2 position = r.Position;
                    double dist = Math.Sqrt(point.distanceSq(position));
                    if (dist < nearest)
                    {
                        closest_robot = r;
                        nearest = dist;
                    }
                }
                return closest_robot;
            });*/

            addFunction("linearInterpolation", "Point, Point, Fraction - Point on the line of that fraction",
                        "The point that is between Point ~ and Point ~ for some Fraction ~", typeof(Vector2),
                        new Type[] { typeof(Vector2), typeof(Vector2), typeof(double) },
                        delegate(EvaluatorState state, object[] objects)
                        {
                            Vector2 p1 = (Vector2)objects[0];
                            Vector2 p2 = (Vector2)objects[1];
                            double fraction = (double)objects[2];
                            Vector2 v = new Vector2(fraction * (p2.X - p1.X), fraction * (p2.Y - p1.Y));
                            return new Vector2(p1.X + v.X, p1.Y + v.Y);
                        });
            addFunction("numberOfRobotsInACircle", "Circle, Team - Number of Robots",
                        "In a circle ~, the number of Robots of the Team ~ in it", typeof(int),
                        new Type[] { typeof(Circle), typeof(TeamCondition) },
                        delegate(EvaluatorState state, object[] objects)
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
            addFunction("angleBetweenTwoLines", "Line,Line - Angle", "The angle between line ~ and line ~", typeof(double),
                        new Type[] { typeof(Line), typeof(Line) },
                        delegate(EvaluatorState state, object[] objects)
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
            addFunction("angleBetweenThreePoints", "Point,Point,Point - Angle",
                        "The angle between the rays connecting Point ~ with Point ~ and Point ~", typeof(double),
                        new Type[] { typeof(Vector2), typeof(Vector2), typeof(Vector2) },
                        delegate(EvaluatorState state, object[] objects)
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
            addFunction("rotatePointAroundAnotherPoint", "Point, Point, Angle - Point",
                        "Around a center point ~, Rotate a Point ~ for an angle ~", typeof(Vector2),
                        new Type[] { typeof(Vector2), typeof(Vector2), typeof(double) },
                        delegate(EvaluatorState state, object[] objects)
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
            addFunction("closerRobotToPoint", "Robot, Robot, Point - Closest",
                        "Return whichever robot of ~ and ~ is closer to point ~", typeof(Robot),
                        new Type[] { typeof(Robot), typeof(Robot), typeof(Vector2) },
                        delegate(EvaluatorState state, object[] objects)
                        {
                            Robot robot1 = (Robot)objects[0];
                            Robot robot2 = (Robot)objects[1];
                            Vector2 pt = (Vector2)objects[2];

                            double robot1distsq = pt.distanceSq(robot1.getPoint());
                            double robot2distsq = pt.distanceSq(robot2.getPoint());

                            if (robot1distsq > robot2distsq)
                            {
                                return robot2;
                            }
                            else
                            {
                                return robot1;
                            }
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
            addFunction("doublecast", "int - Cast to double", "Cast ~ to a double", typeof(double), new Type[] { typeof(int) }, delegate(EvaluatorState state, object[] objects)
            {
                return ((double)((int)objects[0]));
            });
            addFunction("or", "Bool, Bool - Or", "~ or ~", typeof(bool), new Type[] { typeof(bool), typeof(bool) }, delegate(EvaluatorState state, object[] objects)
            {
                return ((bool)objects[0]) || ((bool)objects[1]);
            });
            addFunction("and", "Bool, Bool - And", "~ and ~", typeof(bool), new Type[] { typeof(bool), typeof(bool) }, delegate(EvaluatorState state, object[] objects)
            {
                return ((bool)objects[0]) && ((bool)objects[1]);
            });
            addFunction("not", "Bool - Not", "not ~", typeof(bool), new Type[] { typeof(bool) }, delegate(EvaluatorState state, object[] objects)
            {
                return (!(bool)objects[0]);
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
            addFunction(">", "double, double - Greater than", "~ > ~", typeof(bool), new Type[] { typeof(double), typeof(double) }, delegate(EvaluatorState state, object[] objects)
            {
                return ((double)objects[0]) > ((double)objects[1]);
            });
            addFunction("=", "int, int - equals", "~ = ~", typeof(bool), new Type[] { typeof(double), typeof(double) }, delegate(EvaluatorState state, object[] objects)
            {
                return ((double)objects[0]) == ((double)objects[1]);
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
            addFunction("doubleSin", "double - Sin", "The sine of ~", typeof(double), new Type[] { typeof(double) }, delegate(EvaluatorState state, object[] objects)
            {
                return Math.Sin((double)objects[0]);
            });
            addFunction("doubleCos", "double - Cos", "The cosine of ~", typeof(double), new Type[] { typeof(double) }, delegate(EvaluatorState state, object[] objects)
            {
                return Math.Cos((double)objects[0]);
            });
            #endregion

            #region misc
            addFunction("numourbots", " - # our robots", "The number of robots currently on our team", typeof(double), new Type[] { }, delegate(EvaluatorState state, object[] objects)
            {
                return (double)state.OurTeamInfo.Length;
            });
            addFunction("numtheirbots", " - # their robots", "The number of robots currently on their team", typeof(double), new Type[] { }, delegate(EvaluatorState state, object[] objects)
            {
                return (double)state.TheirTeamInfo.Length;
            });
            addFunction("const-double", "double constant", "The (double) constant ~", typeof(double), new Type[] { typeof(string) }, delegate(EvaluatorState state, object[] objects)
            {
                return Core.Constants.get<double>("plays", (string)objects[0]);
            });
            addFunction("const-int", "int constant", "The (int) constant ~", typeof(int), new Type[] { typeof(string) }, delegate(EvaluatorState state, object[] objects)
            {
                return Core.Constants.get<int>("plays", (string)objects[0]);
            });
            addFunction("const-bool", "bool constant", "The (bool) constant ~", typeof(bool), new Type[] { typeof(string) }, delegate(EvaluatorState state, object[] objects)
            {
                return Core.Constants.get<bool>("plays", (string)objects[0]);
            });
            addFunction("const-string", "string constant", "The (string) constant ~", typeof(string), new Type[] { typeof(string) }, delegate(EvaluatorState state, object[] objects)
            {
                return Core.Constants.get<string>("plays", (string)objects[0]);
            });


            addFunction("print-if", "bool string print", "print ~", typeof(bool), new Type[] { typeof(bool), typeof(string) }, delegate(EvaluatorState state, object[] objects)
            {
                if ((bool)objects[0])
                {
                    Console.WriteLine((string)objects[1]);
                }
                return true;
            });

            addFunction("print-bool", "bool print", "print ~", typeof(bool), new Type[] { typeof(bool) }, delegate(EvaluatorState state, object[] objects)
            {
                //DebugConsole.Write("print-bool: " + objects[0].ToString(), ProjectDomains.Plays);
                Console.WriteLine("print-bool" + objects[0].ToString());
                return true;
            });

            addFunction("print-double", "double print", "print ~", typeof(bool), new Type[] { typeof(double) }, delegate(EvaluatorState state, object[] objects)
            {
                Console.WriteLine(objects[0].ToString());
                return true;
            });

            addFunction("print-string", "string print", "print ~", typeof(bool), new Type[] { typeof(string) }, delegate(EvaluatorState state, object[] objects)
            {
                Console.WriteLine((string)objects[0]);
                return true;
            });

            addFunction("sameRobot", "Robot, Robot - Same", "~ == ~", typeof(bool), new Type[] { typeof(Robot), typeof(Robot) }, delegate(EvaluatorState state, object[] objects)
            {
                return ((Robot)objects[0]).getID() == ((Robot)objects[1]).getID();
            });

            addFunction("setSession", "String, Object", "Set ~ to ~", typeof(bool), new Type[] { typeof(string), typeof(object) }, delegate(EvaluatorState state, object[] objects)
            {
                session[(string)objects[0]] = objects[1];
                return true;
            });

            // Set a session constant if it is not already set
            addFunction("setSessionInit", "String, Object", "Set ~ to ~ if doesn't yet exist", typeof(bool), new Type[] { typeof(string), typeof(object) }, delegate(EvaluatorState state, object[] objects)
            {
                if (!session.ContainsKey((string)objects[0]))
                {
                    session[(string)objects[0]] = objects[1];
                }
                return true;
            });

            addFunction("getSessionDouble", "String", "Get session double ~", typeof(double), new Type[] { typeof(string) }, delegate(EvaluatorState state, object[] objects)
            {
                return (double)session[(string)objects[0]];
            });

            addFunction("getSessionPoint", "String", "Get session point ~", typeof(Vector2), new Type[] { typeof(string) }, delegate(EvaluatorState state, object[] objects)
            {
                return (Vector2)session[(string)objects[0]];
            });

            addFunction("ourgoal", "OurGoal", "Our goal position", typeof(Vector2), new Type[] { }, delegate(EvaluatorState state, object[] objects)
            {
                return new Vector2(-1 * Constants.get<double>("plays", "FIELD_WIDTH") / 2 - 0.08, 0);
            });

            addFunction("theirgoal", "TheirGoal", "Their goal position", typeof(Vector2), new Type[] { }, delegate(EvaluatorState state, object[] objects)
            {
                return new Vector2(Constants.get<double>("plays", "FIELD_WIDTH") / 2 + 0.08, 0);
            });

            addFunction("theirGoalBestShot", "theirGoalBestShot", "Attempts to find a good location to shoot at", typeof(Vector2), new Type[] { }, delegate(EvaluatorState state, object[] objects)
            {
                double fieldWidth = Constants.get<double>("plays", "FIELD_WIDTH");
                double goalHeight = Constants.get<double>("plays", "GOAL_HEIGHT");

                double buffer = 0.01;
                int numIncrements = 40;

                Vector2 goalEnd0 = new Vector2(fieldWidth / 2, goalHeight / 2 - buffer);
                Vector2 goalEnd1 = new Vector2(fieldWidth / 2, -goalHeight / 2 + buffer);

                BallInfo ball = state.ballInfo;
                if (ball == null)
                    return (goalEnd0 + goalEnd1) / 2.0;

                //Simulate a bell curve in our expectation of where the ball will actually go
                double[] kickUncertaintyFactor = {1,8,28,56,70,56,28,8,1};
                int kickUncertantyMid = 4;
                double kickUncertaintyDiv = 256;
                double[] blockednessArray = new double[numIncrements + 1 + kickUncertantyMid*2];
                for(int i = 0; i<blockednessArray.Length; i++)
                    blockednessArray[i] = 1;

                //Interpolate between the ends of the goal, testing the point every increment
                for (int i = 0; i <= numIncrements; i++)
                {
                    double prop = (double)i / numIncrements;
                    Vector2 target = goalEnd0 + prop * (goalEnd1 - goalEnd0);

                    //Distance from enemy robots
                    double blockedness = TacticsEval.kickBlockednessLinear(state.TheirTeamInfo, state.ballInfo, target);
                    blockednessArray[i+kickUncertantyMid] = blockedness;
                }

                //Find the best shot!
                int besti = 0;
                double bestBlockedness = 10000;
                for (int i = 0; i <= numIncrements; i++)
                {
                    double blockedness = 0;
                    for (int j = 0; j < kickUncertantyMid * 2; j++)
                        blockedness += blockednessArray[i + j] * kickUncertaintyFactor[j];
                    blockedness /= kickUncertaintyDiv;
                    if(blockedness < bestBlockedness)
                    {
                        bestBlockedness = blockedness;
                        besti = i;
                    }
                }
                Console.WriteLine("Best blockedness: " + bestBlockedness + " (" + besti + ")");
                double bestProp = (double)besti / numIncrements;
                return goalEnd0 + bestProp * (goalEnd1 - goalEnd0);
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
            addFunction("robotpointstrengthkick", "Robot, strength - Kick", "Have robot ~ kick the ball towards ~ with strength ~", typeof(ActionDefinition), new Type[] { typeof(Robot),  typeof(Vector2), typeof(int) }, delegate(EvaluatorState state, object[] objects)
            {
                Robot robot = (Robot)objects[0];
                Vector2 p = (Vector2)objects[1];
                int s = (int)objects[2];
                return new ActionDefinition(delegate(IActionInterpreter a)
                {
                    a.Kick(robot.getID(), p, s);
                }, robot.getID());
            });
            addFunction("robotpointbump", "Robot, Point - bump", "Have robot ~ bump the ball to ~", typeof(ActionDefinition), new Type[] { typeof(Robot), typeof(Vector2) }, delegate(EvaluatorState state, object[] objects)
            {
                Robot robot = (Robot)objects[0];
                Vector2 p = (Vector2)objects[1];
                return new ActionDefinition(delegate(IActionInterpreter a)
                {
                    a.Bump(robot.getID(), p);
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
                    int id = robot.getID();
                    a.Move(id, target, facing);
                    //Console.WriteLine("robotpointpointmove: r=" + id.ToString() + "target=" + target.ToString() + " facing=" + facing.ToString());
                }, robot.getID());
            });
            addFunction("robotpointpointmovenoavoid", "Robot, Point, Point - Move", "Have robot ~ move to ~, and face ~ (not avoiding ball)", typeof(ActionDefinition), new Type[] { typeof(Robot), typeof(Vector2), typeof(Vector2) }, delegate(EvaluatorState state, object[] objects)
            {
                Robot robot = (Robot)objects[0];
                return new ActionDefinition(delegate(IActionInterpreter a)
                {
                    Vector2 target = (Vector2)objects[1];
                    Vector2 facing = (Vector2)objects[2];
                    int id = robot.getID();
                    a.Move(id, false, target, facing);
                    //Console.WriteLine("robotpointpointmove: r=" + id.ToString() + "target=" + target.ToString() + " facing=" + facing.ToString());
                }, robot.getID());
            });
            addFunction("robotpointpointmovecharge", "Robot, Point, Point - Move", "Have robot ~ move to ~, and face ~ and charge", typeof(ActionDefinition), new Type[] { typeof(Robot), typeof(Vector2), typeof(Vector2) }, delegate(EvaluatorState state, object[] objects)
            {
                Robot robot = (Robot)objects[0];
                return new ActionDefinition(delegate(IActionInterpreter a)
                {
                    Vector2 target = (Vector2)objects[1];
                    Vector2 facing = (Vector2)objects[2];
                    a.Move(robot.getID(), target, facing);
                    a.Charge(robot.getID());
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
            addFunction("reloadConstants", "", "Reload constants", typeof(ActionDefinition), new Type[] { }, delegate(EvaluatorState state, object[] objects)
            {
                return new ActionDefinition(delegate(IActionInterpreter a)
                {
                    Constants.Load("plays");
                });
            });


            // Special action- tells the Play Selector to assign a play to the field
            // CAN ONLY BE USED IN THE MAIN PLAY
            addFunction("assignPlay", "String - assign play", "Assign play ~ to field", typeof(ActionDefinition), new Type[] { typeof(String) }, delegate(EvaluatorState state, object[] objects)
            {
                return new ActionDefinition((string)objects[0]);
            });

            // Special action- tells the play selector to ignore this action
            addFunction("nullAction", "String - assign play", "Assign play ~ to field", typeof(ActionDefinition), new Type[] { }, delegate(EvaluatorState state, object[] objects)
            {
                return new ActionDefinition();
            });
            #endregion
        }
        static Function()
        {
            loadFunctions();
        }

        // HELPER FUNCTIONS
        public static bool pointAboveLine(Vector2 point, Line line)
        {
            Vector2 projpoint = line.projectionOntoLine(point);

            return (projpoint.Y < point.Y);
        }
    }
}
