using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Core;

namespace Robocup.CoreRobotics
{
    public class RefBoxState : IReferee
    {
        Team _ourTeam;                
        PlayType playsToRun;
        
        IRefBoxListener _referee;
        IPredictor _predictor;

        int lastCmdCounter;

        public RefBoxState(Team team, IPredictor predictor)
        {          
            playsToRun = PlayType.Halt;

            _ourTeam = team;

            _predictor = predictor;

            lastCmdCounter = 0;
        }

        public IRefBoxListener getReferee()
        {
            return _referee;
        }
        public void setReferee(IRefBoxListener listener) {
            _referee = listener;
            lastCmdCounter = -1;
        }

        public void start()
        {
            if (_referee == null)
                return;

            _referee.start();
        }

        public void stop()
        {
            if (_referee == null)
                return;

            _referee.stop();
        }

        public PlayType GetCurrentPlayType()
        {
            // Default game state is stopped if there is no refbox connected
            if (_referee == null)
                return PlayType.Stopped;

            if (_predictor.HasBallMoved()) {
                    playsToRun = PlayType.NormalPlay;                    
                    _predictor.ClearBallMark();
            }
            if (lastCmdCounter < _referee.getCmdCounter())
            {
                lastCmdCounter = _referee.getCmdCounter();
                char lastCommand = _referee.getLastCommand();
                switch (lastCommand)
                {
                    case MulticastRefBoxListener.HALT:
                        // stop bots completely
                        playsToRun = PlayType.Halt;
                        _predictor.ClearBallMark();
                        break;
                    case MulticastRefBoxListener.START:
                        playsToRun = PlayType.NormalPlay;
                        _predictor.ClearBallMark();
                        break;
                    case MulticastRefBoxListener.CANCEL:
                    case MulticastRefBoxListener.STOP:
                    case MulticastRefBoxListener.TIMEOUT_BLUE:
                    case MulticastRefBoxListener.TIMEOUT_YELLOW:
                        //go to stopped/waiting state
                        playsToRun = PlayType.Stopped;
                        _predictor.ClearBallMark();
                        break;
                    case MulticastRefBoxListener.TIMEOUT_END_BLUE:
                    case MulticastRefBoxListener.TIMEOUT_END_YELLOW:
                    case MulticastRefBoxListener.READY:
                        if (playsToRun == PlayType.PenaltyKick_Ours_Setup)
                            playsToRun = PlayType.PenaltyKick_Ours;
                        if (playsToRun == PlayType.KickOff_Ours_Setup)
                            playsToRun = PlayType.KickOff_Ours;
                        _predictor.SetBallMark();
                        break;
                    case MulticastRefBoxListener.KICKOFF_BLUE:
                        if (_ourTeam == Team.Yellow)
                            playsToRun = PlayType.KickOff_Theirs;
                        else
                            playsToRun = PlayType.KickOff_Ours_Setup;
                        _predictor.SetBallMark();
                        break;
                    case MulticastRefBoxListener.INDIRECT_BLUE:
                    case MulticastRefBoxListener.DIRECT_BLUE:
                        if (_ourTeam == Team.Yellow)
                            playsToRun = PlayType.SetPlay_Theirs;
                        else
                            playsToRun = PlayType.SetPlay_Ours;
                        _predictor.SetBallMark();
                        break;
                    case MulticastRefBoxListener.KICKOFF_YELLOW:
                        if (_ourTeam == Team.Blue)
                            playsToRun = PlayType.KickOff_Theirs;
                        else
                            playsToRun = PlayType.KickOff_Ours_Setup;
                        _predictor.SetBallMark();
                        break;
                    case MulticastRefBoxListener.INDIRECT_YELLOW:
                    case MulticastRefBoxListener.DIRECT_YELLOW:
                        if (_ourTeam == Team.Blue)
                            playsToRun = PlayType.SetPlay_Theirs;
                        else
                            playsToRun = PlayType.SetPlay_Ours;
                        _predictor.SetBallMark();
                        break;
                    case MulticastRefBoxListener.PENALTY_BLUE:
                        // handle penalty
                        if (_ourTeam == Team.Yellow)
                            playsToRun = PlayType.PenaltyKick_Theirs;
                        else
                            playsToRun = PlayType.PenaltyKick_Ours_Setup;
                        _predictor.ClearBallMark();
                        break;
                    case MulticastRefBoxListener.PENALTY_YELLOW:
                        // penalty kick
                        // handle penalty
                        if (_ourTeam == Team.Blue)
                            playsToRun = PlayType.PenaltyKick_Theirs;
                        else
                            playsToRun = PlayType.PenaltyKick_Ours_Setup;
                        _predictor.ClearBallMark();
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
