using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Diagnostics;
using Robocup.Core;

namespace Robocup.Plays
{
    abstract public class InterpreterPlayable : Playable<InterpreterExpression>
    {
        private bool forceRest(int start)
        {
            for (int i=start;i<Robots.Count;i++){
                InterpreterRobotDefinition robot = (InterpreterRobotDefinition)Robots[i].getValue(evaluatorstate.Tick, evaluatorstate);
                robot.setEvaluatorState(evaluatorstate);
                bool worked = robot.define();
                if (!worked)
                    return false;
                //robot.getID();
            }
            return true;
        }
        /// <summary>
        /// forces the robot definition order, in case the strictness is anything but Strict
        /// </summary>
        /// <param name="lastAssignments">The last set of assignments that were used.  Tries to duplicate them, if possible</param>
        /// <returns>Returns true if all robots were successfully defined, otherwise false</returns>
        internal bool forceRobotDefinitionOrder(int[] lastAssignments)
        {
            //reminder:
            //the robots in the Robots list
            //are guaranteed to be in an order such that if i<j, Robots[i] does NOT depend on Robots[j]
            if (lastAssignments != null)
            {
                InterpreterRobotDefinition r;
                bool failed = false;
                int startat = -1;
                for (int i = 0; i < lastAssignments.Length; i++)
                {
                    r = (InterpreterRobotDefinition)Robots[i].getValue(evaluatorstate.Tick, evaluatorstate);
                    r.setEvaluatorState(evaluatorstate);
                    if (!r.canForceDefine(lastAssignments[i]))
                    {
                        failed = true;
                        startat = i;
                        break;
                    }
                    //else
                    try
                    {
                        r.forceDefine(lastAssignments[i]);
                    }
                    catch (ApplicationException)
                    {
                        failed = true;
                        startat = i;
                        break;
                    }
                }
                if (failed)
                {
                    return forceRest(startat);
                }
                /*for (int i = 0; i < lastAssignments.Length; i++)
                {
                    ((InterpreterRobot)Robots[i].getValue(evaluatorstate.Tick, evaluatorstate)).forceDefine(lastAssignments[i]);
                }*/
                return true;
            }
            else
            {
                return forceRest(0);
            }
        }
        internal void setEvaluatorState(EvaluatorState state)
        {
            this.evaluatorstate = state;
            if (Ball != null)
                ((InterpreterBall)Ball).setEvaluatorState(state);
            /*foreach (Expression s in Robots)
            {
            }*/
            //forceRobotDefinitionOrder();
        }

        public override string ToString()
        {
            return this.Name != null && this.Name.Length > 0 ? this.Name : "UNNAMED";
        }

        public override void SetDesignerData(List<string> data)
        {
            
        }
	

        #region objects

        private InterpreterBall ball = new InterpreterBall();
        internal InterpreterBall Ball
        {
            get { return ball; }
        }
        public override InterpreterExpression TheBall { get { return new InterpreterExpression(ball); } }
        private EvaluatorState evaluatorstate = null;
        internal object getObject(string name) {
            InterpreterExpression rtn;
            if (!PlayObjects.TryGetValue(name, out rtn))
                throw new ApplicationException("Could not find the name " + name + " in this play");
            return rtn;
        }
        #endregion

        #region robots
        private List<InterpreterExpression> robots = new List<InterpreterExpression>();
        /// <summary>
        /// DO NOT ADD ROBOTS TO THIS LIST.  Call addRobot() instead
        /// </summary>
        public override IList<InterpreterExpression> Robots
        {
            get { return robots; }
        }
        private int numourrobots = 0;
        /// <summary>
        /// The number of our robots that are involved in this play.
        /// </summary>
        internal int NumOurRobots
        {
            get { return numourrobots; }
        }
        private int numtheirrobots = 0;
        /// <summary>
        /// The number of their robots that are involved in this play.
        /// </summary>
        internal int NumTheirRobots
        {
            get { return numtheirrobots; }
        }

        public override void addRobot(InterpreterExpression obj)
        {
            if (obj.ReturnType != typeof(InterpreterRobotDefinition))
                return;
            Robots.Add(obj);
            //if (InterpreterFunctions.IsRobotOnOurTeam(obj))
            if (obj.IsRobotOnOurTeam())
                numourrobots++;
            else
                numtheirrobots++;
        }
        #endregion
        /*#region learning information
        public class LearningData
        { 
            Dictionary<string, int> learningData = new Dictionary<string, int>();
            public int this[string name]
            {
                get { return learningData[name]; }
                set { learningData[name] = value; }
            }
            /// <summary>
            /// If the variable with name [name] exists, then it does nothing.
            /// Otherwise it sets that to be 0.
            /// </summary>
            /// <param name="name"></param>
            public void initialize(string name)
            {
                if (!learningData.ContainsKey(name))
                    learningData.Add(name, 0);
            }
        }
        private LearningData learningdata = new LearningData();
        public LearningData Learning
        {
            get { return learningdata; }
        }
        #endregion*/
    }

    public class InterpreterPlay : InterpreterPlayable, IPlay<InterpreterExpression>
    {
        #region IPlay members
        List<InterpreterExpression> conditions = new List<InterpreterExpression>();
        public List<InterpreterExpression> Conditions
        {
            get { return conditions; }
        }

        private PlayType type = PlayType.NormalPlay;
        public PlayType PlayType
        {
            get { return type; }
            set { type = value; }
        }

        private double score = 1.0 + new Random().NextDouble() / 100;
        public double Score
        {
            get { return score; }
            set { score = value; }
        }

        private int id;
        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        private IList<Playable<InterpreterExpression>> tactics = new List<Playable<InterpreterExpression>>();
        public IList<Playable<InterpreterExpression>> Tactics
        {
            get { return tactics; }
        }
        #endregion

        public bool isEnabled = true;
    }

    public class InterpreterTactic : InterpreterPlayable, ITactic<InterpreterExpression>
    {
        private List<InterpreterExpression> parameters = new List<InterpreterExpression>();
        public List<InterpreterExpression> Parameters
        {
            get { return parameters; }
        }
    }
}
