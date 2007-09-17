using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Robocup.Constants;
using Robocup.Infrastructure;
using Robocup.Plays;
using Robocup.CoreRobotics;
using Robocup.Core;

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
        
        RefBoxState _refbox;
        RefBoxListener _listener;
      
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

            _state = state;
            _listener = refbox;
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
        //Plays loaded for the interpreter no longer save
        /*public void savePlays()
        {
            PlayUtils.savePlays(playFiles);
        }*/
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

            // refboxlistener
            _refbox = new RefBoxState(_listener, _predictor, isYellow);

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

                _refbox.start();
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
                _refbox.stop();
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
            interpret( _refbox.getCurrentPlayType() );
        }


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
            }
            _interpreter.interpret(toRun);
        }

    }
}
