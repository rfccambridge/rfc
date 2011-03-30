using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Collections;
using Robocup.Core;

namespace Robocup.Plays
{
    public class EvaluatorState
    {
        private int tick;
        public int Tick
        {
            get { return tick; }
        }
        private InterpreterRobotInfo[] ourteaminfo;
        public InterpreterRobotInfo[] OurTeamInfo
        {
            get { return ourteaminfo; }
        }
        private InterpreterRobotInfo[] theirteaminfo;
        public InterpreterRobotInfo[] TheirTeamInfo
        {
            get { return theirteaminfo; }
        }
        private BallInfo ball;
        public BallInfo ballInfo
        {
            get { return ball; }
        }
        private int ourgoals;
        public int OurGoals
        {
            get { return ourgoals; }
        }
        private int theirgoals;
        public int TheirGoals
        {
            get { return theirgoals; }
        }
        public EvaluatorState(InterpreterRobotInfo[] ourteaminfo, InterpreterRobotInfo[] theirteaminfo, BallInfo ballinfo, int ourgoals, int theirgoals, int tickNum)
        {
            this.ourteaminfo = ourteaminfo;
            this.theirteaminfo = theirteaminfo;
            this.ball = ballinfo;
            this.tick = tickNum;
            this.ourgoals = ourgoals;
            this.theirgoals = theirgoals;
        }
    }
}
