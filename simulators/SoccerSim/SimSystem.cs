using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Robocup.Utilities;
using Robocup.Plays;
using Robocup.CoreRobotics;
using Robocup.Core;
using Robocup.Simulation;
using System.Drawing;

namespace SoccerSim
{
    class SimSystem
    {
        private const int YELLOW = 0;
        private const int BLUE = 1;

        IPredictor _predictor;
        public FieldDrawer _view;
        PhysicsEngine physics_engine;

        RFCController _controller;
        public RFCController Controller
        {
            get { return _controller; }
        }
        public Interpreter _interpreter;

    	private MulticastRefBoxListener _refboxListener;
    	private RefBoxState _refbox;

        IReferee referee;
        //RefBoxListener _listener;


        string PLAY_DIR;

        System.Timers.Timer t;
        private volatile bool running;
        private bool initialized;
        private int counter;

        private int _sleepTime;
        private bool isYellow;

    	private int team;

        public SimSystem(FieldDrawer view, PhysicsEngine physics_engine, IReferee referee, MulticastRefBoxListener refboxListener, bool isYell)
        {
            
            _view = view;
            
            initialized = false;
            running = false;
            _sleepTime = Constants.get<int>("default", "UPDATE_SLEEP_TIME");
            isYellow = isYell;

            this.referee = referee;
        	this._refboxListener = refboxListener;

            this.physics_engine = physics_engine;
            //_listener = refbox;
            initialize();
        }

        public void ReloadConstants()
        {
            // reload plays and constants

            // allow only if system is not running
            if (running)
            {
                Console.WriteLine("CANNOT RELOAD PLAYS WHEN SYSTEM IS RUNNING!");
                return;
            }

            // first own constants
            LoadConstants();

            // then plays
            Dictionary<InterpreterPlay, string> plays = PlayUtils.loadPlays(PLAY_DIR);
            _interpreter.LoadPlays(new List<InterpreterPlay>(plays.Keys));

            // constants for plays
            Constants.Load();

            // motion control
            _controller.LoadConstants();
        }

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

            LoadConstants();

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
            //_controller = new RFCController(physics_engine, new Robocup.MotionControl.SmoothVector2BiRRTMotionPlanner(), physics_engine);
            _controller = new RFCController(physics_engine, new Robocup.MotionControl.BugExtendMotionPlanner(), physics_engine);
            //_controller = new RFCController(physics_engine, new Robocup.MotionControl.DumbExtenderPlanner(), physics_engine);


            // refboxlistener
            //referee = new RefBoxState(_listener, _predictor, isYellow);

            // create interpreter from file
            _interpreter = new Interpreter(false, _predictor, _controller);
            Dictionary<InterpreterPlay, string> plays = PlayUtils.loadPlays(PLAY_DIR);
            _interpreter.LoadPlays(new List<InterpreterPlay>(plays.Keys));

            running = false;
            if (wasRunning)
            {
                start();
            }

			_refbox = new RefBoxState(_refboxListener, _predictor, isYellow);

            Color ourColor = (team == YELLOW ? Color.Yellow : Color.Blue);
            string teamString = (team == YELLOW ? "YELLOW" : "BLUE");
            _view.AddString("Team", new FieldDrawer.StringDisplayInfo("Team: " + teamString, new Point(20, 520), ourColor));
            _view.AddString("PlayType", new FieldDrawer.StringDisplayInfo("Play type: ", new Point(20, 540), Color.White));
            _view.AddString("RefboxCommand", new FieldDrawer.StringDisplayInfo("Refbox command: ", new Point(300, 540), Color.White));

            initialized = true;
        }

        public void LoadConstants()
        {
            PLAY_DIR = Constants.get<string>("default", "PLAY_DIR");
            // In my understanding, this only works with Yellow!
        	team = Constants.get<int>("configuration", "OUR_TEAM_INT");
            //team = 0;
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
                foreach (RobotInfo info in _predictor.GetRobots(team))
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
            //interpret(referee.GetCurrentPlayType());
			// now with real refboxstate!

			// add playtype to drawer
            // If this instance of SimSystem is for "our" team (vs. static enemies)
            if (isYellow && team == YELLOW)  // This stuff was confusing...
            {
                interpret(_refbox.GetCurrentPlayType());
                PlayTypes playType = _refbox.GetCurrentPlayType();
                _view.UpdateString("PlayType", "Play type: " + playType.ToString());                       
            }
        }


        private void interpret(PlayTypes toRun)
        {
            //_view.clearArrows();
            _interpreter.interpret(toRun);
        }

    }
}
