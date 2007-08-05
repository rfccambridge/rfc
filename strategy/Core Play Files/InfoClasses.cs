using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Collections;
using Robocup.Infrastructure;

namespace RobocupPlays
{
    public class EvaluatorState
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
