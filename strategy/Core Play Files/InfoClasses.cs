using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Collections;
using Robocup.Infrastructure;
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
        public EvaluatorState(InterpreterRobotInfo[] ourteaminfo, InterpreterRobotInfo[] theirteaminfo, BallInfo ballinfo, int tickNum)
        {
            this.ourteaminfo = ourteaminfo;
            this.theirteaminfo = theirteaminfo;
            this.ball = ballinfo;
            this.tick = tickNum;
        }
    }
}
