using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Collections;
using Robocup.Infrastructure;
using Robocup.Core;

namespace Robocup.Plays
{
    abstract class InterpreterRobotDefinition : PlayRobotDefinition, GetPointable
    {
        //static public void addDefinitionFunctions(){
        //    Function.addFunction("closest","Point - Closest Robot",typeof(InterpreterRobot),new Type[]{typeof(TeamCondition),typeof(InterpreterPoint)

        /// <summary>
        /// Calculates which robot on the field this Robot corresponds to.
        /// </summary>
        /// <param name="strictness">The strictness of the play that is being run</param>
        /// <returns>Returns true if successfully defined this Robot.</returns>
        public abstract bool define();

        /// <summary>
        /// Returns whether or not this robot satisfies this robot type
        /// </summary>
        public bool canForceDefine(int robotID)
        {
            foreach (InterpreterRobotInfo rinf in evaluatorstate.OurTeamInfo)
            {
                if (rinf.ID == robotID)
                {
                    bool failed = (rinf.Assigned && !Assignment.OkIfAssigned) ||
                        (rinf.State == RobotStates.Busy && !Assignment.OkIfBusy);
                    return !failed;
                }
            }
            foreach (InterpreterRobotInfo rinf in evaluatorstate.TheirTeamInfo)
            {
                if (rinf.ID == robotID)
                {
                    bool failed = (rinf.Assigned && !Assignment.OkIfAssigned) ||
                        (rinf.State == RobotStates.Busy && !Assignment.OkIfBusy);
                    return !failed;
                }
            }
            //if we couldn't find it, then fail
            return false;
            //throw new ApplicationException("could not find the robot with ID " + robotID);
        }
        public void forceDefine(int robotID)
        {
            foreach (InterpreterRobotInfo rinf in evaluatorstate.OurTeamInfo)
            {
                if (rinf.ID == robotID)
                {
                    bool failed = (rinf.Assigned && !Assignment.OkIfAssigned) ||
                        (rinf.State == RobotStates.Busy && !Assignment.OkIfBusy);
                    System.Diagnostics.Debug.Assert(!failed, "internal assumption failure", "you are trying to force a definition on a robot that has already been assigned to");
                    thisrobot = rinf;
                    thisrobot.Assigned = true;
                    return;
                }
            }
            foreach (InterpreterRobotInfo rinf in evaluatorstate.TheirTeamInfo)
            {
                if (rinf.ID == robotID)
                {
                    bool failed = (rinf.Assigned && !Assignment.OkIfAssigned) ||
                        (rinf.State == RobotStates.Busy && !Assignment.OkIfBusy);
                    System.Diagnostics.Debug.Assert(!failed, "internal assumption failure", "you are trying to force a definition on a robot that has already been assigned to");
                    thisrobot = rinf;
                    thisrobot.Assigned = true;
                    return;
                }
            }
            throw new ApplicationException("could not find the robot with ID " + robotID);
        }

        protected InterpreterRobotInfo thisrobot = null;
        protected EvaluatorState evaluatorstate = null;
        public void setEvaluatorState(EvaluatorState evaluatorstate)
        {
            this.evaluatorstate = evaluatorstate;
        }

        //protected int lasttick = -1;
        public override int getID()
        {
            //update(evaluatorstate.Tick);
            return thisrobot.ID;
        }
        private bool ours;
        public override bool Ours
        {
            get { return ours; }
        }
        private RobotAssignmentType assignment;
        public RobotAssignmentType Assignment
        {
            get { return assignment; }
        }

        public override Vector2 getPoint()
        {
            return thisrobot.Position;
        }
        public InterpreterRobotDefinition(bool ours, RobotAssignmentType assignment)
        {
            this.ours = ours;
            this.assignment = assignment;
        }
    }
    class InterpreterSavedDefinition : InterpreterRobotDefinition
    {
        private int ID;
        public InterpreterSavedDefinition(bool ours, int ID)
            : base(ours, null)
        {
            this.ID = ID;
        }
        public override bool define()
        {
            InterpreterRobotInfo[] infos;
            if (Ours)
                infos = evaluatorstate.OurTeamInfo;
            else
                infos = evaluatorstate.TheirTeamInfo;
            foreach (InterpreterRobotInfo info in infos)
            {
                if (info.ID == this.ID)
                    thisrobot = info;
            }
            return true;
        }
    }
    class InterpreterClosestDefinition : InterpreterRobotDefinition
    {
        private Vector2 closesttopoint;
        private List<String> tags = null;
        public InterpreterClosestDefinition(bool ours, Vector2 p, RobotAssignmentType assignment)
            : base(ours, assignment)
        {
            this.closesttopoint = p;
            //this.ours = ours;
        }
        public InterpreterClosestDefinition(bool ours, Vector2 p, RobotAssignmentType assignment, List<string> tags)
            : this(ours, p, assignment)
        {
            this.tags = tags;
        }

        public override bool define()
        {
            InterpreterRobotInfo[] infos;
            if (Ours)
                infos = evaluatorstate.OurTeamInfo;
            else
                infos = evaluatorstate.TheirTeamInfo;

            Vector2 closest = closesttopoint;

            InterpreterRobotInfo bestrobot = null;
            float bestdistance = 1000000;
            foreach (InterpreterRobotInfo rinf in infos)
            {
                if (Assignment.SkipAssigned && rinf.Assigned)
                    continue;
                if (Assignment.SkipBusy && rinf.State == RobotStates.Busy)
                    continue;

                if (tags != null)
                {
                    bool matchestags = true;
                    foreach (string tag in tags)
                    {
                        if (!rinf.Tags.Contains(tag))
                        {
                            matchestags = false;
                            break;
                        }
                    }
                    if (!matchestags)
                        continue;
                }

                float dist = closest.distanceSq(rinf.Position);
                if (dist < bestdistance)
                {
                    bestdistance = dist;
                    bestrobot = rinf;
                }
            }
            //if no robot at all is found, then fail
            if (bestrobot == null)
            {
                return false;
            }

            /*ArrayList eligiblerobots = new ArrayList();
            foreach (RobotInfo rinf in infos)
            {
                if (!rinf.Assigned)
                    eligiblerobots.Add(rinf);
            }*/
            //eligiblerobots.Sort(new RobotInfoDistanceComparer(closest));
            //thisrobot = (RobotInfo)eligiblerobots[0];
            thisrobot = bestrobot;
            bool failed = (thisrobot.Assigned && !Assignment.OkIfAssigned) || (thisrobot.State == RobotStates.Busy && !Assignment.OkIfBusy);
            //bool rtn = !(thisrobot.Assigned || (thisrobot.State != RobotStates.Free));
            thisrobot.Assigned = true;
            return !failed;
        }
    }
}
