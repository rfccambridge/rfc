using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Core;

namespace Robocup.CoreRobotics
{
    public class RefBoxState : IReferee
    {
        bool isYellow;

        bool marking;
        Vector2 markedPosition;
        PlayTypes playsToRun;

        IRefBoxListener _referee;
        IPredictor _predictor;

        int lastCmdCounter;

        public RefBoxState(IRefBoxListener referee, IPredictor predictor, bool yellow)
        {
            marking = false;
            markedPosition = new Vector2(0.0, 0.0);
            //TODO change this back so that it defaults to Halt?
            playsToRun = PlayTypes.Halt;
            
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

        public PlayTypes GetCurrentPlayType()
        {
            if (marking)
            {
                if (hasBallMoved(.02))
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
                    case MulticastRefBoxListener.HALT:
                        // stop bots completely
                        playsToRun = PlayTypes.Halt;
                        break;
                    case MulticastRefBoxListener.START:
                        playsToRun = PlayTypes.NormalPlay;
                        break;
                    case MulticastRefBoxListener.CANCEL:
                    case MulticastRefBoxListener.STOP:
                    case MulticastRefBoxListener.TIMEOUT_BLUE:
                    case MulticastRefBoxListener.TIMEOUT_YELLOW:
                        //go to stopped/waiting state
                        playsToRun = PlayTypes.Stopped;
                        break;
                    case MulticastRefBoxListener.TIMEOUT_END_BLUE:
                    case MulticastRefBoxListener.TIMEOUT_END_YELLOW:
                    case MulticastRefBoxListener.READY:
                        if (playsToRun == PlayTypes.PenaltyKick_Ours_Setup)
                            playsToRun = PlayTypes.PenaltyKick_Ours;
                        if (playsToRun == PlayTypes.KickOff_Ours_Setup)
                            playsToRun = PlayTypes.KickOff_Ours;
                        setBallMark();
                        break;
                    case MulticastRefBoxListener.KICKOFF_BLUE:
                        if (isYellow)
                        {
                            playsToRun = PlayTypes.KickOff_Theirs;
                        }
                        else
                        {
                            playsToRun = PlayTypes.KickOff_Ours_Setup;
                        }
                        break;
                    case MulticastRefBoxListener.INDIRECT_BLUE:
                    case MulticastRefBoxListener.DIRECT_BLUE:
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
                    case MulticastRefBoxListener.KICKOFF_YELLOW:
                        if (!isYellow)
                        {
                            playsToRun = PlayTypes.KickOff_Theirs;
                        }
                        else
                        {
                            playsToRun = PlayTypes.KickOff_Ours_Setup;
                        }
                        break;
                    case MulticastRefBoxListener.INDIRECT_YELLOW:
                    case MulticastRefBoxListener.DIRECT_YELLOW:
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
                    case MulticastRefBoxListener.PENALTY_BLUE:
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
                    case MulticastRefBoxListener.PENALTY_YELLOW:
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

        bool hasBallMoved(double dist_mm)
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
