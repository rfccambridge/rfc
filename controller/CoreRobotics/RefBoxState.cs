using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Plays;
using Robocup.Core;

namespace Robocup.CoreRobotics
{
    public class RefBoxState
    {
        bool isYellow;

        bool marking;
        Vector2 markedPosition;
        PlayTypes playsToRun;

        IReferee _referee;
        IPredictor _predictor;

        int lastCmdCounter;

        public RefBoxState(IReferee referee, IPredictor predictor, bool yellow)
        {
            marking = false;
            markedPosition = new Vector2(0.0f, 0.0f);
            playsToRun = PlayTypes.NormalPlay;
            
            isYellow = yellow;

            _predictor = predictor;
            _referee = referee;

            lastCmdCounter = 0;
        }

        public void start()
        {
            _referee.start();
        }

        public void stop()
        {
            _referee.stop();
        }

        public PlayTypes getCurrentPlayType()
        {
            if (marking)
            {
                if (hasBallMoved(.02f))
                {
                    playsToRun = PlayTypes.NormalPlay;
                    clearBallMark();
                }
            }
            if (lastCmdCounter < _referee.getCmdCounter())
            {
                lastCmdCounter = _referee.getCmdCounter();
                switch (_referee.getLastCommand())
                {
                    case RefBoxListener.HALT:
                        // stop bots completely
                        playsToRun = PlayTypes.Halt;
                        break;
                    case RefBoxListener.START:
                        playsToRun = PlayTypes.NormalPlay;
                        break;
                    case RefBoxListener.CANCEL:
                    case RefBoxListener.STOP:
                    case RefBoxListener.TIMEOUT_BLUE:
                    case RefBoxListener.TIMEOUT_YELLOW:
                        //go to stopped/waiting state
                        playsToRun = PlayTypes.Stopped;
                        break;
                    case RefBoxListener.TIMEOUT_END_BLUE:
                    case RefBoxListener.TIMEOUT_END_YELLOW:
                    case RefBoxListener.READY:
                        if (playsToRun == PlayTypes.PenaltyKick_Ours_Setup)
                            playsToRun = PlayTypes.PenaltyKick_Ours;
                        if (playsToRun == PlayTypes.KickOff_Ours_Setup)
                            playsToRun = PlayTypes.KickOff_Ours;
                        setBallMark();
                        break;
                    case RefBoxListener.KICKOFF_BLUE:
                        if (isYellow)
                        {
                            playsToRun = PlayTypes.KickOff_Theirs;
                        }
                        else
                        {
                            playsToRun = PlayTypes.KickOff_Ours_Setup;
                        }
                        break;
                    case RefBoxListener.INDIRECT_BLUE:
                    case RefBoxListener.DIRECT_BLUE:
                        if (isYellow)
                        {
                            playsToRun = PlayTypes.SetPlay_Theirs;
                        }
                        else
                        {
                            playsToRun = PlayTypes.SetPlay_Ours;
                        }
                        setBallMark();
                        break;
                    case RefBoxListener.KICKOFF_YELLOW:
                        if (!isYellow)
                        {
                            playsToRun = PlayTypes.KickOff_Theirs;
                        }
                        else
                        {
                            playsToRun = PlayTypes.KickOff_Ours_Setup;
                        }
                        break;
                    case RefBoxListener.INDIRECT_YELLOW:
                    case RefBoxListener.DIRECT_YELLOW:
                        if (!isYellow)
                        {
                            playsToRun = PlayTypes.SetPlay_Theirs;
                        }
                        else
                        {
                            playsToRun = PlayTypes.SetPlay_Ours;
                        }
                        setBallMark();
                        break;
                    case RefBoxListener.PENALTY_BLUE:
                        // handle penalty
                        if (isYellow)
                        {
                            playsToRun = PlayTypes.PenaltyKick_Theirs;
                        }
                        else
                        {
                            playsToRun = PlayTypes.PenaltyKick_Ours_Setup;
                        }
                        break;
                    case RefBoxListener.PENALTY_YELLOW:
                        // penalty kick
                        // handle penalty
                        if (! isYellow)
                        {
                            playsToRun = PlayTypes.PenaltyKick_Theirs;
                        }
                        else
                        {
                            playsToRun = PlayTypes.PenaltyKick_Ours_Setup;
                        }
                        break;
                }
            }
            //Console.WriteLine("playtype: " + playsToRun);
            return playsToRun;
        }

        void setBallMark()
        {
            markedPosition = new Vector2(_predictor.getBallInfo().Position.X, _predictor.getBallInfo().Position.Y);
            marking = true;
        }

        bool hasBallMoved(float dist_mm)
        {
            if (!marking) return true;
            bool ret = markedPosition.distanceSq(_predictor.getBallInfo().Position) > dist_mm * dist_mm;
            return ret;
        }

        void clearBallMark()
        {
            marking = false;
        }
    }
}
