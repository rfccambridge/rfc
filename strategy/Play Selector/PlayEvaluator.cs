using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Drawing;
using Robocup.Infrastructure;


namespace RobocupPlays
{
    class EvaluatorResults
    {
        public EvaluatorResults(InterpreterExpression[] actions, int[] robotids)
        {
            this.actions = actions;
            this.robotids = robotids;
        }
        private InterpreterExpression[] actions;
        public InterpreterExpression[] Actions
        {
            get { return actions; }
        }
        private int[] robotids;
        public int[] RobotIDs
        {
            get { return robotids; }
        }

    }

    class PlayEvaluator
    {


        EvaluatorState state;
        public EvaluatorState State
        {
            get { return state; }
        }
        /// <summary>
        /// This is SO NECESSARY if you have more than one interpreter running at once --
        /// if you do not increment both ticks at once, then you get bad caching errors
        /// </summary>
        static int synchronizedTick = 0;
        public int Tick
        {
            get { return state.Tick; }
        }
        InterpreterPlay curplay;
        /*public void updateConditions(RobotInfo[] ourteaminfo, RobotInfo[] theirteaminfo, BallInfo ballinfo, int tickNum)
        {
            state = new EvaluatorState(ourteaminfo, theirteaminfo, ballinfo, tickNum);
        }*/
        /// <summary>
        /// Updates the conditions, and the tick will be increased by one.
        /// </summary>
        public void updateConditions(RobotInfo[] ourteaminfo, RobotInfo[] theirteaminfo, BallInfo ballinfo)
        {
            /*int newtime = 0;
            if (state != null)
                newtime = Tick + 1;*/
            synchronizedTick++;
            state = new EvaluatorState(ourteaminfo, theirteaminfo, ballinfo, synchronizedTick);
        }
        private void clearAssignments()
        {
            foreach (RobotInfo rinf in state.OurTeamInfo)
            {
                rinf.Assigned = false;
            }
            foreach (RobotInfo rinf in state.TheirTeamInfo)
            {
                rinf.Assigned = false;
            }
        }
        Random r = new Random();
        //public ActionInfo[] evaluatePlay(InterpreterPlay play)
        public EvaluatorResults evaluatePlay(InterpreterPlay play, int[] lastAssignedIDs)
        {
            curplay = play;
            if (state == null)
                throw new ApplicationException("You can't evaluate plays without first setting up the evaluator");
            clearAssignments();

            play.setEvaluatorState(state);

            bool failed=!play.forceRobotDefinitionOrder(lastAssignedIDs);

            if (!failed)
            {
                foreach (InterpreterExpression e in play.Conditions)
                {
                    //bool b = evaluateCondition(cond,state.Tick);
                    bool b = (bool)e.getValue(Tick,state);
                    if (!b)
                    {
                        failed = true;
                        break;
                    }
                }
            }
            if (failed)
            {
                //curplay = null;
                return null;//failed
            }
            //else

            /*ArrayList actions = new ArrayList();
            foreach (Expression action in curplay.Actions)
            {
                actions.Add(new ActionInfo((ActionDefinition)action.getValue(state.Tick),curplay));
            }
            curplay = null;
            return (ActionInfo[])actions.ToArray(typeof(ActionInfo));*/

            int[] robotIDs = new int[curplay.Robots.Count];
            int i = 0;
            foreach (InterpreterExpression e in curplay.Robots)
            {
                robotIDs[i] = ((PlayRobotDefinition)e.getValue(Tick, state)).getID();
                i++;
            }
            return new EvaluatorResults(curplay.Actions.ToArray(), robotIDs);
        }

        /// <summary>
        /// Processes the actions from an array of strings corresponding to that action (the string output to
        /// a file when saved, split at space characters).  It returns an ActionInfo object, which stores an
        /// ActionDefinition object, which defines that action (which is currently just the action string, with
        /// all robot names replaced with their ID numbers), and an array of ints corresponding to the ID numbers
        /// of the robots that need to be free for that action to take place
        /// </summary>
        /// 
        //Any changes here must also be made in PlayDesigner.Action.actions, and PlaySelector.Interpreter.interpretActions()
        /*private ActionInfo processAction(string[] strings)
        {
            //I wanted to use the switch...case syntax here, but in C# apparently all the case
            //blocks are in the same scope, so you can't variables with the same name in them,
            //so I converted it into if...else if syntax

            string command = strings[0];
            if (command == "robotpointmove")
            {
                //format: robot destination
                InterpreterRobot r = (InterpreterRobot)curplay.getObject(strings[1]);
                Vector2 p = ((InterpreterPoint)curplay.getObject(strings[2])).getPoint();
                return new ActionInfo(new ActionDefinition("(robotpointmove " + r.getID() + " " + p.X + " " + p.Y + ")"), curplay, r.getID());
            }
            else if (command == "robotpointmoveorientation")
            {
                //format: robot point point
                InterpreterRobot r = (InterpreterRobot)curplay.getObject(strings[1]);
                Vector2 p1 = ((InterpreterPoint)curplay.getObject(strings[2])).getPoint();
                Vector2 p2 = ((InterpreterPoint)curplay.getObject(strings[3])).getPoint();
                return new ActionInfo(new ActionDefinition("(robotpointmoveorientation " + r.getID() + " " + p1.X + " " + p1.Y + " " + p2.X + " " + p2.Y + ")"), curplay, r.getID());
            }
            else if (command == "robotrobotpass")
            {
                //format: robot robot
                InterpreterRobot passer = (InterpreterRobot)curplay.getObject(strings[1]);
                InterpreterRobot passee = (InterpreterRobot)curplay.getObject(strings[2]);
                return new ActionInfo(new ActionDefinition("(robotrobotpass " + passer.getID() + " " + passee.getID() + ")"), curplay, passer.getID(), passee.getID());
            }
            else if (command == "robotshoot")
            {
                //format: robot
                InterpreterRobot robot = (InterpreterRobot)curplay.getObject(strings[1]);
                return new ActionInfo(new ActionDefinition("(robotshoot " + robot.getID() + ")"), curplay, robot.getID());
            }
            else if (command == "robotrobotpointmovingpass")
            {
                //format: passer passee passee-destination
                InterpreterRobot passer = (InterpreterRobot)curplay.getObject(strings[1]);
                InterpreterRobot passee = (InterpreterRobot)curplay.getObject(strings[2]);
                InterpreterPoint point = (InterpreterPoint)curplay.getObject(strings[3]);
                Vector2 p = point.getPoint();
                return new ActionInfo(new ActionDefinition("(robotrobotpointmovingpass " + passer.getID() + " " + passer.getID() + " " + p.X + " " + p.Y + ")"), curplay, passer.getID(), passee.getID());
            }
            else
                throw new ApplicationException("Could not process the actiion " + String.Join(" ", strings));

        }*/
        //any changes here must also be made to PlayDesigner.Condition.conditions
        /*private bool evaluateCondition(string[] strings, int tickNum)
        {

            string command = strings[0];
            if (command == "linelength")
            {
                //format: line <> float
                PlayLine l = (InterpreterLine)curplay.getObject(strings[1]);
                Vector2[] points = l.getPoints();
                return FloatComparer.compare(CommonFunctions.distance(points[0], points[1]), float.Parse(strings[3]), strings[2]);
            }
            else if (command == "pointpointdistance")
            {
                //format: point point <> float
                Vector2 p1 = ((InterpreterPoint)curplay.getObject(strings[1])).getPoint();
                Vector2 p2 = ((InterpreterPoint)curplay.getObject(strings[2])).getPoint();
                return FloatComparer.compare(CommonFunctions.distance(p1, p2), float.Parse(strings[4]), strings[3]);
            }
            else if (command == "robotpointclosest")
            {
                //format: robot point
                InterpreterRobot r = (InterpreterRobot)curplay.getObject(strings[1]);
                InterpreterPoint p = (InterpreterPoint)curplay.getObject(strings[2]);
                RobotInfoDistanceComparer comparer = new RobotInfoDistanceComparer(p.getPoint());
                //RobotInfoDistanceComparer comparer = new RobotInfoDistanceComparer(((InterpreterPoint)curplay.getObject(strings[2])).getPoint());
                if (r.Ours)
                {
                    ArrayList info = new ArrayList(state.OurTeamInfo);
                    info.Sort(comparer);
                    return ((RobotInfo)info[0]).ID == r.getID();
                }
                else
                {
                    ArrayList info = new ArrayList(state.TheirTeamInfo);
                    info.Sort(comparer);
                    return ((RobotInfo)info[0]).ID == r.getID();
                }
            }
            else if (command == "pointsegmentdistance")
            {
                //format: point line <> float
                Vector2 p = ((InterpreterPoint)curplay.getObject(strings[1])).getPoint();
                PlayLine l = (InterpreterLine)(curplay.getObject(strings[2]));
                return FloatComparer.compare((float)l.distFromSegment(p), float.Parse(strings[4]), strings[3]);
            }
            else if (command == "circleinside")
            {
                //format: point circle
                InterpreterPoint p = (InterpreterPoint)curplay.getObject(strings[1]);
                InterpreterCircle c = (InterpreterCircle)curplay.getObject(strings[2]);
                return c.distanceFromCenter(p.getPoint()) <= c.Radius;
            }
            else if (command == "circlerobotsinside")
            {
                //format: team circle <> float
                ArrayList distances = new ArrayList();

                InterpreterCircle c = (InterpreterCircle)curplay.getObject(strings[2]);

                //[team] is either going to be "our_team", "their_team", or "either_team".
                //so the only time you don't add our robots is if it's "their_team":
                if (strings[1] != "our_team")
                {
                    //theserobots.AddRange(state.TheirTeamInfo);
                    foreach (RobotInfo info in state.TheirTeamInfo)
                    {
                        distances.Add(c.distanceFromCenter(info.Position));
                    }
                }
                if (strings[1] != "their_team")
                {
                    //theserobots.AddRange(state.OurTeamInfo);
                    foreach (RobotInfo info in state.OurTeamInfo)
                    {
                        distances.Add(c.distanceFromCenter(info.Position));
                    }
                }
                //RobotInfoDistanceComparer comparer = new RobotInfoDistanceComparer(((InterpreterPoint)curplay.getObject(strings[2])).getPoint());
                //distances.Sort();
                int numinside = 0;
                for (int i = 0; i < distances.Count; i++)
                {
                    if ((float)distances[i] <= c.Radius)
                    {
                        numinside++;
                    }
                }
                return FloatComparer.compare(numinside, Int32.Parse(strings[4]), strings[3]);
            }
            else
                throw new ApplicationException("Could not recognize condition " + String.Join(" ", strings));


        }*/
    }
}
