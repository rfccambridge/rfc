using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Collections;

namespace RobocupPlays
{
    public class InterpreterPlay : Play, InterpreterObject
    {
        /*private RobotOrderForcer orderforcer=null;
        internal void setOrderForcer(RobotOrderForcer orderforcer){
            this.orderforcer=orderforcer;
        }*/
        /// <summary>
        /// forces the robot definition order, in case the strictness is anything but Strict
        /// </summary>
        /// <returns>Returns true if all robots were successfully defined, otherwise false</returns>
        internal bool forceRobotDefinitionOrder()
        {
            foreach (Expression exp in Robots)
            {
                InterpreterRobot robot = (InterpreterRobot)exp.getValue(evaluatorstate.Tick);
                robot.setEvaluatorState(evaluatorstate);
                bool worked=robot.define(Strictness);
                if (!worked)
                    return false;
                robot.getID();
                //robot.getID();
            }
            return true;
        }
        internal void setEvaluatorState(EvaluatorState state)
        {
            this.evaluatorstate = state;
            if (Ball != null)
                Ball.setEvaluatorState(state);
            /*foreach (Expression s in Robots)
            {
            }*/
            //forceRobotDefinitionOrder();
        }


        private Dictionary<string, Expression> definedObjects = new Dictionary<string, Expression>();
        internal Dictionary<string, Expression> definitionDictionary
        {
            get { return definedObjects; }
        }
        private ArrayList robots=new ArrayList();
        public ArrayList Robots
        {
            get { return robots; }
        }

        private ArrayList conditions = new ArrayList();
        internal ArrayList Conditions
        {
            get { return conditions; }
        }
        private ArrayList actions = new ArrayList();
        internal ArrayList Actions
        {
            get { return actions; }
        }
        private InterpreterBall ball;
        internal InterpreterBall Ball
        {
            get { return ball; }
            set { ball = value; }
        }




        private EvaluatorState evaluatorstate = null;
        internal object getObject(string name)
        {
            Expression rtn;
            if (!definitionDictionary.TryGetValue(name, out rtn))
                throw new ApplicationException("Could not find the name " + name + " in this play");
            return rtn;
        }

        private int numourrobots=0;
        /// <summary>
        /// The number of our robots that are involved in this play.
        /// </summary>
        public int NumOurRobots
        {
            get { return numourrobots; }
        }
        private int numtheirrobots=0;
        /// <summary>
        /// The number of their robots that are involved in this play.
        /// </summary>
        public int NumTheirRobots
        {
            get { return numtheirrobots; }
        }
	
        internal void addRobot(InterpreterExpression obj)
        {
            Robots.Add(obj);
            //if (InterpreterFunctions.IsRobotOnOurTeam(obj))
            if (obj.IsRobotOnOurTeam())
                numourrobots++;
            else
                numtheirrobots++;
        }
    }
    public class BallInfo
    {
        private PlayPoint position;
        public PlayPoint Position
        {
            get { return position; }
        }
        private float dx;
        public float dX
        {
            get { return dx; }
        }
        private float dy;
        public float dY
        {
            get { return dy; }
        }
	
        public BallInfo(PlayPoint position,float dx, float dy)
        {
            this.position = position;
            this.dx = dx;
            this.dy = dy;
        }
    }
    public class RobotInfo
    {
        private PlayPoint position;
        public PlayPoint Position
        {
            get { return position; }
        }
        private float orientation;
        public float Orientation
        {
            get { return orientation; }
        }
        private RobotStates state;
        internal RobotStates State
        {
            get { return state; }
            set { state = value; }
        }
        private bool assigned = false;
        internal bool Assigned
        {
            get { return assigned; }
            set { assigned = value; }
        }
        private int idnum;

        public int ID
        {
            get { return idnum; }
            set { idnum = value; }
        }

        public RobotInfo(PlayPoint position, float orientation, int id) : this(position, orientation, RobotStates.Free, id) { }
        internal RobotInfo(PlayPoint position, float orientation, RobotStates state, int id)
        {
            this.position = position;
            this.orientation = orientation;
            this.state = state;
            this.assigned = false;
            this.idnum = id;
        }

        public void setFree()
        {
            this.State = RobotStates.Free;
            this.Assigned = false;
        }
    }
    enum RobotStates
    {
        Free = 0,
        Busy = 10
    }
    class ActionInfo
    {
        //private int[] robots;
        public int[] RobotsInvolved
        {
            //get { return robots; }
            get { return Definition.RobotsInvolved; }
        }
        private ActionDefinition definition;
        public ActionDefinition Definition
        {
            get { return definition; }
        }
        private InterpreterPlay play;
        public InterpreterPlay Play
        {
            get { return play; }
        }
	
        public ActionInfo(ActionDefinition definition, InterpreterPlay play)//,params int[] robotsinvolved)
        {
            this.definition = definition;
            //this.robots = robotsinvolved;
            this.play = play;
        }
    }
    /*struct ActionCandidate
    {
        public InterpreterExpression[] actions;
        public InterpreterPlay play;
        public ActionCandidate(InterpreterExpression[] actions, InterpreterPlay play)
        {
            this.actions = actions;
            this.play = play;
        }
        public string Name{
            get { return play.Name; }
        }
    }*/
    class EvaluatorState
    {
        private int tick;
        public int Tick
        {
            get { return tick; }
        }
        private RobotInfo[] ourteaminfo;
        public RobotInfo[] OurTeamInfo
        {
            get { return ourteaminfo; }
        }
        private RobotInfo[] theirteaminfo;
        public RobotInfo[] TheirTeamInfo
        {
            get { return theirteaminfo; }
        }
        private BallInfo ball;
        public BallInfo ballInfo
        {
            get { return ball; }
        }
        public EvaluatorState(RobotInfo[] ourteaminfo, RobotInfo[] theirteaminfo, BallInfo ballinfo, int tickNum)
        {
            this.ourteaminfo = ourteaminfo;
            this.theirteaminfo = theirteaminfo;
            this.ball = ballinfo;
            this.tick = tickNum;
        }
    }
}
