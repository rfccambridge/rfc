using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Robocup.Constants;
using Robocup.Infrastructure;
using RobocupPlays;
using Robocup.CoreRobotics;

namespace SoccerSim
{
    class SimSystem
    {
        IPredictor _predictor;
        IInfoAcceptor _acceptor;
        FieldView _view;
        FieldState _state;

        IController _controller;
        Interpreter _interpreter;
        
        RefBoxListener reflistener;
        Vector2 markedPosition;
        bool marking = false;
        PlayTypes playsToRun;

      
        Thread worker;
        private volatile bool running;
        private bool initialized;
        private int counter;

        private int _sleepTime;
        private bool isYellow;

        public SimSystem(FieldView view, FieldState state, RefBoxListener refbox, bool isYell)
        {
            _view = view;
            initialized = false;
            running = false;
            _sleepTime = Constants.get<int>("UPDATE_SLEEP_TIME");
            isYellow = isYell;
            playsToRun = PlayTypes.NormalPlay;
            reflistener = refbox;

            _state = state;
            initialize();
        }

        # region Play Resource Management
        Dictionary<InterpreterPlay, string> playFiles;
        public InterpreterPlay[] dictionaryToArray(Dictionary<InterpreterPlay, string> plays)
        {
            InterpreterPlay[] toRet = new InterpreterPlay[plays.Keys.Count];
            plays.Keys.CopyTo(toRet, 0);
            return toRet;
        }
        public void loadPlays(string path)
        {
            playFiles = PlayUtils.loadPlays(path);
            if(isYellow)
                _interpreter = new Interpreter(false, dictionaryToArray(playFiles), _predictor, _controller);
            else
                _interpreter = new Interpreter(true, dictionaryToArray(playFiles), new TeamFlipperPredictor(_predictor), _controller);
        }
        public void savePlays()
        {
            PlayUtils.savePlays(playFiles);
        }
        # endregion

        public void initialize()
        {
            bool wasRunning = false;
            if (running)
            {
                Console.WriteLine("rfcsystem is currently running...stopping");
                running = false;
                wasRunning = true;
                System.Threading.Thread.Sleep(1000);
            }
            // create predictor
            if (_predictor == null || _acceptor == null)
            {
                _predictor = _state;
                _acceptor = _state;
            }

            // create controller
            _controller = new SimController(_predictor, _acceptor, _view);

            // create interpreter from file
            loadPlays("../../plays");

            running = false;
            if (wasRunning)
            {
                start();
            }
            
            initialized = true;

        }

        public void setSleepTime(int millis)
        {
            _sleepTime = millis;
        }

        # region Start/Stop
        public void start()
        {
            if (!running)
            {
                if (!initialized)
                    initialize();

                _sleepTime = Constants.get<int>("UPDATE_SLEEP_TIME");
                isYellow = Constants.get<string>("OUR_TEAM_COLOR") == "YELLOW";

                reflistener.start();
                worker = new Thread(run);
                worker.Start();
                counter = 0;
                running = true;
            }
        }

        public void stop()
        {
            if (running)
            {
                running = false;
                reflistener.stop();
                foreach (RobotInfo info in _predictor.getOurTeamInfo())
                {
                    _controller.stop(info.ID);
                }
            }

        }

        # endregion

        public void run()
        {
            //TimeSpan sleepDuration = new TimeSpan(0, 0, 0, 0, _sleepTime);
            while (running)
            {

                //int curTime = DateTime.Now.Millisecond;
                if (counter % 100 == 0)
                    Console.WriteLine("--------------RUNNING ROUND: " + counter + "-----------------");

                runRound();

                counter++;
                Thread.Sleep(_sleepTime);

            }
            Console.WriteLine("--------------DONE RUNNING: -----------------");
        }

        public void runRound()
        {
            if (marking)
            {
                if (hasBallMoved(.02f))
                {
                    playsToRun = PlayTypes.NormalPlay;
                    clearBallMark();
                }
            }
            if (reflistener.hasNewCommand())
            {
                //Console.WriteLine("new command: [" + reflistener.getLastCommand() + "]");
                switch (reflistener.getLastCommand())
                {
                    case RefBoxListener.HALT:
                        // stop bots completely
                        Console.WriteLine("halting");
                        playsToRun = PlayTypes.Halt;
                        break;
                    case RefBoxListener.START:
                        Console.WriteLine("force start");
                        playsToRun = PlayTypes.NormalPlay;
                        break;
                    case RefBoxListener.CANCEL:
                    case RefBoxListener.STOP:
                    case RefBoxListener.TIMEOUT_BLUE:
                    case RefBoxListener.TIMEOUT_YELLOW:
                        //go to stopped/waiting state
                        Console.WriteLine("Stopped/waiting state");
                        playsToRun = PlayTypes.Stopped;
                        break;
                    case RefBoxListener.TIMEOUT_END_BLUE:
                    case RefBoxListener.TIMEOUT_END_YELLOW:
                    case RefBoxListener.READY:
                        Console.WriteLine("awaiting play");
                        if (playsToRun == PlayTypes.PenaltyKick_Ours_Setup)
                            playsToRun = PlayTypes.PenaltyKick_Ours;
                        if (playsToRun == PlayTypes.KickOff_Ours_Setup)
                            playsToRun = PlayTypes.KickOff_Ours;
                        setBallMark();

                        break;
                    case RefBoxListener.KICKOFF_BLUE:
                        if (isYellow)
                        {
                            Console.WriteLine("kickoff from enemy");
                            playsToRun = PlayTypes.KickOff_Theirs;
                        }
                        else
                        {
                            Console.WriteLine("kickoff for us");
                            playsToRun = PlayTypes.KickOff_Ours_Setup;
                        }
                        break;
                    case RefBoxListener.INDIRECT_BLUE:
                    case RefBoxListener.DIRECT_BLUE:
                        if (isYellow)
                        {
                            Console.WriteLine("kick from enemy");
                            playsToRun = PlayTypes.SetPlay_Theirs;
                        }
                        else
                        {
                            Console.WriteLine("kick for us");
                            playsToRun = PlayTypes.SetPlay_Ours;
                        }
                        setBallMark();
                        break;
                    case RefBoxListener.KICKOFF_YELLOW:
                        if (!isYellow)
                        {
                            Console.WriteLine("kickoff from enemy");
                            playsToRun = PlayTypes.KickOff_Theirs;
                        }
                        else
                        {
                            Console.WriteLine("kickoff for us");
                            playsToRun = PlayTypes.KickOff_Ours_Setup;
                        }
                        break;
                    case RefBoxListener.INDIRECT_YELLOW:
                    case RefBoxListener.DIRECT_YELLOW:
                        if (!isYellow)
                        {
                            Console.WriteLine("kick from enemy");
                            playsToRun = PlayTypes.SetPlay_Theirs;
                        }
                        else
                        {
                            Console.WriteLine("kick for us");
                            playsToRun = PlayTypes.SetPlay_Ours;
                        }
                        setBallMark();
                        break;
                    case RefBoxListener.PENALTY_BLUE:
                        // handle penalty
                        if (isYellow)
                        {
                            Console.WriteLine("defending penalty");
                            playsToRun = PlayTypes.PenaltyKick_Theirs;
                        }
                        else
                        {
                            Console.WriteLine("shooting penalty");
                            playsToRun = PlayTypes.PenaltyKick_Ours_Setup;
                        }
                        break;
                    case RefBoxListener.PENALTY_YELLOW:
                        // penalty kick
                        // handle penalty
                        if (!isYellow)
                        {
                            Console.WriteLine("defending penalty");
                            playsToRun = PlayTypes.PenaltyKick_Theirs;
                        }
                        else
                        {
                            Console.WriteLine("shooting penalty");
                            playsToRun = PlayTypes.PenaltyKick_Ours_Setup;
                        }
                        break;

                }
            }

            Console.WriteLine("Play type: " + playsToRun);

            interpret(playsToRun);
        }

        # region Ball Mark
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
        # endregion

        private void interpret(PlayTypes toRun)
        {
            _view.clearArrows();
            // TODO: do goalie better
            foreach (RobotInfo r in _predictor.getOurTeamInfo())
            {
                r.Tags.Clear();
                if (isYellow)
                {
                    if (r.ID == 0)
                        r.Tags.Add("goalie");
                }
                else
                {
                    if (r.ID == 5)
                        r.Tags.Add("goalie");
                }
                r.setFree();
            }
            foreach (RobotInfo r in _predictor.getTheirTeamInfo())
                r.setFree();
            _interpreter.interpret(toRun);
        }

    }
}
