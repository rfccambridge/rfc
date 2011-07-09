using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Geometry;
using Robocup.Core;

namespace Robocup.CoreRobotics
{
    public class RefBoxState : IReferee
    {
        Team _ourTeam;                
        PlayType playsToRun;
        
        IRefBoxListener _refboxListener;
        IPredictor _predictor;

        private int lastCmdCounter;

        bool predictor_marking = false;
        private Vector2 markedPosition = null;

        public RefBoxState(Team team, IPredictor predictor)
        {          
            playsToRun = PlayType.Halt;

            _ourTeam = team;

            _predictor = predictor;

            lastCmdCounter = 0;
        }

        // The listener can only be created once, hence can't pass the nicer host, port pair
        public void Connect(IRefBoxListener listener)
        {
            if (_refboxListener != null)
                throw new ApplicationException("Already connected.");

            _refboxListener = listener;
            lastCmdCounter = 0;
        }
       
        public void Disconnect()
        {
            if (_refboxListener == null)
                throw new ApplicationException("Not connected.");

            _refboxListener = null;
        }

        public bool IsReceiving()
        {
            return _refboxListener.IsReceiving();
        }

        public Score GetScore()
        {
            return _refboxListener.GetScore();
        }

        private void setBallMark()
        {
            predictor_marking = true;
            BallInfo ball = _predictor.GetBall();
            if (ball == null)
                return;
            markedPosition = ball.Position;
        }

        private void clearBallMark()
        {
            predictor_marking = false;
            markedPosition = null;
        }

        //A free kick has occured when the ball has moved a sufficient distance (indicating that it was
        //bumped by some robot, and it leaves contact with that robot). Returns true if this is the case,
        //so that we know to resume play.
        private bool shouldResumeNormalPlay()
        {
            BallInfo ball = _predictor.GetBall();

            //Check if ball moved sufficient distance
            double BALL_MOVED_DIST = Constants.Plays.BALL_MOVED_DIST;
            bool hasBallMoved = (ball != null && (markedPosition == null ||
                              markedPosition.distanceSq(ball.Position) > BALL_MOVED_DIST * BALL_MOVED_DIST));
            if (!hasBallMoved)
                return false;

            List<RobotInfo> allrobots = _predictor.GetRobots();
            double mindistsq = double.PositiveInfinity;
            for (int i = 0; i < allrobots.Count; i++)
            {
                if (allrobots[i] == null || ball == null)
                    continue;
                double distsq = allrobots[i].Position.distanceSq(ball.Position);
                if (distsq < mindistsq)
                    mindistsq = distsq;
            }

            //TODO: Hardcoded .108: The approximate distance from robot to ball that suffices to conclude ball has been kicked
            double _BALL_ROBOT_FREEKICK_DIST = Constants.Plays.BALL_ROBOT_FREEKICK_DIST;
            return mindistsq > _BALL_ROBOT_FREEKICK_DIST * _BALL_ROBOT_FREEKICK_DIST;
        }


        public PlayType GetCurrentPlayType()
        {
            // Default game state is stopped if there is no refbox connected
            if (_refboxListener == null)
                return PlayType.Stopped;

            if (predictor_marking && shouldResumeNormalPlay())
            {
                    playsToRun = PlayType.NormalPlay;                    
                    clearBallMark();
            }
            if (lastCmdCounter < _refboxListener.GetCmdCounter())
            {
                lastCmdCounter = _refboxListener.GetCmdCounter();
                char lastCommand = _refboxListener.GetLastCommand();
                switch (lastCommand)
                {
                    case MulticastRefBoxListener.HALT:
                        // stop bots completely
                        playsToRun = PlayType.Halt;
                        clearBallMark();
                        break;
                    case MulticastRefBoxListener.START:
                        playsToRun = PlayType.NormalPlay;
                        clearBallMark();
                        break;
                    case MulticastRefBoxListener.CANCEL:
                    case MulticastRefBoxListener.STOP:
                    case MulticastRefBoxListener.TIMEOUT_BLUE:
                    case MulticastRefBoxListener.TIMEOUT_YELLOW:
                        //go to stopped/waiting state
                        playsToRun = PlayType.Stopped;
                        clearBallMark();
                        break;
                    case MulticastRefBoxListener.TIMEOUT_END_BLUE:
                    case MulticastRefBoxListener.TIMEOUT_END_YELLOW:
                    case MulticastRefBoxListener.READY:
                        if (playsToRun == PlayType.PenaltyKick_Ours_Setup)
                            playsToRun = PlayType.PenaltyKick_Ours;
                        if (playsToRun == PlayType.KickOff_Ours_Setup)
                            playsToRun = PlayType.KickOff_Ours;
                        setBallMark();
                        break;
                    case MulticastRefBoxListener.KICKOFF_BLUE:
                        if (_ourTeam == Team.Yellow)
                            playsToRun = PlayType.KickOff_Theirs;
                        else
                            playsToRun = PlayType.KickOff_Ours_Setup;
                        setBallMark();
                        break;
                    case MulticastRefBoxListener.INDIRECT_BLUE:
                    case MulticastRefBoxListener.DIRECT_BLUE:
                        if (_ourTeam == Team.Yellow)
                            playsToRun = PlayType.SetPlay_Theirs;
                        else
                            playsToRun = PlayType.SetPlay_Ours;
                        setBallMark();
                        break;
                    case MulticastRefBoxListener.KICKOFF_YELLOW:
                        if (_ourTeam == Team.Blue)
                            playsToRun = PlayType.KickOff_Theirs;
                        else
                            playsToRun = PlayType.KickOff_Ours_Setup;
                        setBallMark();
                        break;
                    case MulticastRefBoxListener.INDIRECT_YELLOW:
                    case MulticastRefBoxListener.DIRECT_YELLOW:
                        if (_ourTeam == Team.Blue)
                            playsToRun = PlayType.SetPlay_Theirs;
                        else
                            playsToRun = PlayType.SetPlay_Ours;
                        setBallMark();
                        break;
                    case MulticastRefBoxListener.PENALTY_BLUE:
                        // handle penalty
                        if (_ourTeam == Team.Yellow)
                            playsToRun = PlayType.PenaltyKick_Theirs;
                        else
                            playsToRun = PlayType.PenaltyKick_Ours_Setup;
                        clearBallMark();
                        break;
                    case MulticastRefBoxListener.PENALTY_YELLOW:
                        // penalty kick
                        // handle penalty
                        if (_ourTeam == Team.Blue)
                            playsToRun = PlayType.PenaltyKick_Theirs;
                        else
                            playsToRun = PlayType.PenaltyKick_Ours_Setup;
                        clearBallMark();
                        break;
                }
            }
            //Console.WriteLine("playtype: " + playsToRun);
            _predictor.SetPlayType(playsToRun);
            return playsToRun;
        }

        public void LoadConstants()
        {
            throw new Exception("The method or operation is not implemented.");
        }

    }
}
