using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Robocup.Utilities;
using Robocup.Plays;
using Robocup.CoreRobotics;
using Robocup.Core;
using Robocup.Simulation;

namespace SoccerSim
{
    class SimSystem
    {
        IPredictor _predictor;
        FieldDrawer _view;
        PhysicsEngine physics_engine;

        RFCController _controller;
        public RFCController Controller
        {
            get { return _controller; }
        }
        Interpreter _interpreter;

        IReferee referee;
        //RefBoxListener _listener;

        System.Timers.Timer t;
        private volatile bool running;
        private bool initialized;
        private int counter;

        private int _sleepTime;
        private bool isYellow;

        public SimSystem(FieldDrawer view, PhysicsEngine physics_engine, IReferee referee, bool isYell)
        {
            _view = view;
            initialized = false;
            running = false;
            _sleepTime = Constants.get<int>("default", "UPDATE_SLEEP_TIME");
            isYellow = isYell;

            this.referee = referee;

            this.physics_engine = physics_engine;
            //_listener = refbox;
            initialize();
        }

        # region Play Resource Management
        public InterpreterPlay[] dictionaryToArray(Dictionary<InterpreterPlay, string> plays)
        {
            InterpreterPlay[] toRet = new InterpreterPlay[plays.Keys.Count];
            plays.Keys.CopyTo(toRet, 0);
            return toRet;
        }
        public void loadPlays(string path)
        {
            Dictionary<InterpreterPlay, string> playFiles = PlayUtils.loadPlays(isYellow ? path : path + "/opponent");
            if (isYellow)
                _interpreter = new Interpreter(false, dictionaryToArray(playFiles), _predictor, _controller);
            else
                _interpreter = new Interpreter(true, dictionaryToArray(playFiles), new TeamFlipperPredictor(_predictor), _controller);
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
            if (_predictor == null)
            {
                _predictor = physics_engine;
            }

            /*Dictionary<int, IMovement> planners = new Dictionary<int, IMovement>();
            foreach (RobotInfo info in physics_engine.getOurTeamInfo())
                //planners.Add(info.ID, new FourWheeledMovement());
            planners.Add(info.ID, new TwoWheeledMovement( TwoWheeledMovement.WhichTwoWheels.FrontLeftBackRight));
            foreach (RobotInfo info in physics_engine.getTheirTeamInfo())
                planners.Add(info.ID, new FourWheeledMovement());*/
                //planners.Add(info.ID, new TwoWheeledMovement(physics_engine, TwoWheeledMovement.WhichTwoWheels.FrontLeftBackRight));
            // create controller
            //_controller = new RFCController(physics_engine, planners, new Navigation.Current.CurrentNavigator(), physics_engine);
            _controller = new RFCController(physics_engine, new Robocup.MotionControl.SmoothVector2BiRRTMotionPlanner(), physics_engine);

            // refboxlistener
            //referee = new RefBoxState(_listener, _predictor, isYellow);

            // create interpreter from file
            loadPlays("../../plays");

            running = false;
            if (wasRunning)
            {
                start();
            }

            initialized = true;

        }

        # region Start/Stop
        public void start()
        {
            // TODO: do goalie better
            /*foreach (RobotInfo r in _predictor.getOurTeamInfo())
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
            }*/
            if (!running)
            {
                if (!initialized)
                    initialize();

                _sleepTime = Constants.get<int>("default", "UPDATE_SLEEP_TIME");
                isYellow = Constants.get<string>("configuration", "OUR_TEAM_COLOR") == "YELLOW";

                //referee.start();
                t = new System.Timers.Timer(_sleepTime);
                t.AutoReset = true;
                t.Elapsed += delegate(object sender, System.Timers.ElapsedEventArgs e)
                {
                    runRound();
                };
                t.Start();
                counter = 0;
                running = true;
            }
        }

        public void stop()
        {
            if (running)
            {
                running = false;
                //referee.stop();
                t.Stop();
                foreach (RobotInfo info in _predictor.getOurTeamInfo())
                {
                    _controller.stop(info.ID);
                }
                Console.WriteLine("--------------DONE RUNNING: -----------------");
            }

        }

        # endregion

        public void runRound()
        {
            if (counter % 100 == 0)
                Console.WriteLine("--------------RUNNING ROUND: " + counter + "-----------------");
            counter++;
            _controller.clearArrows();
            interpret(referee.GetCurrentPlayType());
        }


        private void interpret(PlayTypes toRun)
        {
            //_view.clearArrows();
            _interpreter.interpret(toRun);
        }

    }
}
