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
        
        IRefBoxListener _refboxListener;
        IPredictor _predictor;

        private int lastCmdCounter;

        private bool predictor_marking = false;

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

        private void predictorSetBallMark()
        {
            _predictor.SetBallMark();
            predictor_marking = true;
        }

        private void predictorClearBallMark()
        {
            _predictor.ClearBallMark();
            predictor_marking = false;
        }

        public Score GetScore()
        {
            return _refboxListener.GetScore();
        }

        public PlayType GetCurrentPlayType()
        {
            // Default game state is stopped if there is no refbox connected
            if (_refboxListener == null)
                return PlayType.Stopped;

            if (predictor_marking && _predictor.HasBallMoved()) {
                    playsToRun = PlayType.NormalPlay;                    
                    predictorClearBallMark();
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
                        predictorClearBallMark();
                        break;
                    case MulticastRefBoxListener.START:
                        playsToRun = PlayType.NormalPlay;
                        predictorClearBallMark();
                        break;
                    case MulticastRefBoxListener.CANCEL:
                    case MulticastRefBoxListener.STOP:
                    case MulticastRefBoxListener.TIMEOUT_BLUE:
                    case MulticastRefBoxListener.TIMEOUT_YELLOW:
                        //go to stopped/waiting state
                        playsToRun = PlayType.Stopped;
                        predictorClearBallMark();
                        break;
                    case MulticastRefBoxListener.TIMEOUT_END_BLUE:
                    case MulticastRefBoxListener.TIMEOUT_END_YELLOW:
                    case MulticastRefBoxListener.READY:
                        if (playsToRun == PlayType.PenaltyKick_Ours_Setup)
                            playsToRun = PlayType.PenaltyKick_Ours;
                        if (playsToRun == PlayType.KickOff_Ours_Setup)
                            playsToRun = PlayType.KickOff_Ours;
                        predictorSetBallMark();
                        break;
                    case MulticastRefBoxListener.KICKOFF_BLUE:
                        if (_ourTeam == Team.Yellow)
                            playsToRun = PlayType.KickOff_Theirs;
                        else
                            playsToRun = PlayType.KickOff_Ours_Setup;
                        predictorSetBallMark();
                        break;
                    case MulticastRefBoxListener.INDIRECT_BLUE:
                    case MulticastRefBoxListener.DIRECT_BLUE:
                        if (_ourTeam == Team.Yellow)
                            playsToRun = PlayType.SetPlay_Theirs;
                        else
                            playsToRun = PlayType.SetPlay_Ours;
                        predictorSetBallMark();
                        break;
                    case MulticastRefBoxListener.KICKOFF_YELLOW:
                        if (_ourTeam == Team.Blue)
                            playsToRun = PlayType.KickOff_Theirs;
                        else
                            playsToRun = PlayType.KickOff_Ours_Setup;
                        predictorSetBallMark();
                        break;
                    case MulticastRefBoxListener.INDIRECT_YELLOW:
                    case MulticastRefBoxListener.DIRECT_YELLOW:
                        if (_ourTeam == Team.Blue)
                            playsToRun = PlayType.SetPlay_Theirs;
                        else
                            playsToRun = PlayType.SetPlay_Ours;
                        predictorSetBallMark();
                        break;
                    case MulticastRefBoxListener.PENALTY_BLUE:
                        // handle penalty
                        if (_ourTeam == Team.Yellow)
                            playsToRun = PlayType.PenaltyKick_Theirs;
                        else
                            playsToRun = PlayType.PenaltyKick_Ours_Setup;
                        predictorClearBallMark();
                        break;
                    case MulticastRefBoxListener.PENALTY_YELLOW:
                        // penalty kick
                        // handle penalty
                        if (_ourTeam == Team.Blue)
                            playsToRun = PlayType.PenaltyKick_Theirs;
                        else
                            playsToRun = PlayType.PenaltyKick_Ours_Setup;
                        predictorClearBallMark();
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
