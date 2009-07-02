// THIS FILE WILL BE USED BUT SHOULD BE MOVED
// currently there is no use for it, but the place to put it doesn't exist yet (some sort of "production" project)
// eventually, move it in there.
// (the things that use this should through compile errors, so we wont forget it)
//#if false
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

using Robocup.Plays;
using Robocup.Utilities;
using Robocup.Core;
using Robocup.CoreRobotics;
using Robocup.MotionControl;


namespace Robocup.ControlForm
{
    // make this load/instantiate from a text file
    public class RFCSystem
    {        
        IPredictor _predictor;
        IVisionInfoAcceptor _acceptor;
        RFCController _controller;
        Interpreter _interpreter;
        IRobots _commander;        

        RefBoxState _refbox;

        public IPredictor Predictor
        {
            get { return _predictor; }
        }
        public IVisionInfoAcceptor Acceptor
        {
            get { return _acceptor; }
        }
        public IController Controller {
            get { return _controller; }
        }
        public RefBoxState Refbox
        {
            get { return _refbox; }
        }

        System.Timers.Timer t;

        private volatile bool running;
        private bool initialized;
        private int counter;

        private int _sleepTime;
        private bool isYellow;
        private int team;
        bool IS_OUR_GOAL_LEFT;

        int REFBOX_PORT;
        string REFBOX_ADDR;
        string PLAY_DIR;
        

        public RFCSystem()
        {
            initialized = false;
            running = false;                    }

        public void LoadConstants() {            
            REFBOX_PORT = Constants.get<int>("default","REFBOX_PORT");
            REFBOX_ADDR = Constants.get<string>("default", "REFBOX_ADDR");
            PLAY_DIR = Constants.get<string>("default", "PLAY_DIR");
            _sleepTime = Constants.get<int>("default", "UPDATE_SLEEP_TIME");
            isYellow = Constants.get<string>("configuration", "OUR_TEAM") == "YELLOW";
            team = Constants.get<int>("configuration", "OUR_TEAM_INT");
            IS_OUR_GOAL_LEFT = Constants.get<bool>("plays", "IS_OUR_GOAL_LEFT");

            if (_controller != null)
            {
                _controller.LoadConstants();
            }
        }

        public void setRefBoxListener(IRefBoxListener refboxListener) {
            if (running)
                throw new ApplicationException("Cannot change refbox listener while running!");

            _refbox.setReferee(refboxListener);
        }

        public void stopRefBoxListener()
        {            
            IRefBoxListener refboxListener = _refbox.getReferee();
            refboxListener.stop();
        }

        public void closeRefBoxListener()
        {            
            IRefBoxListener refboxListener = _refbox.getReferee();            
            refboxListener.close();
            _refbox.setReferee(null);
        }

        public void initialize()
        {
            LoadConstants();

            bool wasRunning = false;
            if (running)
            {
                Console.WriteLine("rfcsystem is currently running...stopping");
                running = false;
                wasRunning = true;
                System.Threading.Thread.Sleep(1000);
            }
            // create predictor
            if (_predictor == null && _acceptor == null)
            {
                BasicPredictor basicPredictor = new BasicPredictor();
                _predictor = basicPredictor as IPredictor;
                _acceptor = basicPredictor as IVisionInfoAcceptor;
            }
            else if (_predictor == null)
            {
                _predictor = new BasicPredictor() as IPredictor;
            }
            else if (_acceptor == null)
            {
                _acceptor = new BasicPredictor() as IVisionInfoAcceptor;
            }

            _refbox = new RefBoxState(null, _predictor, isYellow);


            // create helper interfaces
            if (_commander == null)
                _commander = new StubRobots();

            // Set default MotionPlanner (many past options are included...)

            // Note- DefaultMotionPlanner is designed for the CS199r class. It is rather slow and does not
            // avoid obstacles. It is designed to be extremely accurate and reliable, for the purpose of 
            // testing tactics and plays in which speed or obstacles are not a concern.

            //INavigator navigator = new Navigation.Examples.LookAheadBug();
            //IMotionPlanner planner = new Robocup.MotionControl.SmoothVector2BiRRTMotionPlanner();
            //IMotionPlanner planner = new Robocup.MotionControl.MixedBiRRTMotionPlanner();
            //IMotionPlanner planner = new Robocup.MotionControl.CircleFeedbackMotionPlanner();
            //IMotionPlanner planner = new Robocup.MotionControl.BugFeedbackMotionPlanner();
            //IMotionPlanner planner = new Robocup.MotionControl.FeedbackVeerMotionPlanner();
            //IMotionPlanner planner = new Robocup.MotionControl.DefaultMotionPlanner();
            IMotionPlanner planner = new Robocup.MotionControl.TangentBugFeedbackMotionPlanner();
            //IMotionPlanner planner = new Robocup.MotionControl.TangentBugVeerMotionPlanner();

            for (int i = 0; i < 10; i++)
            {
                bool has_kicker;
                if (Constants.nondestructiveGet("default", "ROBOT_HAS_KICKER_" + i, out has_kicker))
                {
                    TagSystem.AddTag(i, "kicker");
                }
                if (Constants.nondestructiveGet("default", "ROBOT_IS_GOALIE_" + i, out has_kicker))
                {
                    TagSystem.AddTag(i, "goalie");
                }
            }

            /*Dictionary<int, IMovement> planners = new Dictionary<int, IMovement>();
            for (int i = 0; i < 10; i++)
            {
                string move_model;
                bool exists = Constants.nondestructiveGet<string>("default", "ROBOT_" + i, out move_model);
                if (!exists)
                    continue;
                if (move_model.Equals("TWO-FLBR"))
                {
                    planners[i] = new TwoWheeledMovement(TwoWheeledMovement.WhichTwoWheels.FrontLeftBackRight);
                }
                else if (move_model.Equals("TWO-FRBL"))
                {
                    planners[i] = new TwoWheeledMovement(TwoWheeledMovement.WhichTwoWheels.FrontRightBackLeft);
                }
                /*else if (move_model.Equals("FOUR"))
                {
                    //planners[i] = new TwoWheeledMovement(_predictor, TwoWheeledMovement.WhichTwoWheels.FrontLeftBackRight);
                    throw new ApplicationException("Unsupported Movement Model");
                }*
                else
                {
                    throw new ApplicationException("invalid movement model: " + move_model);
                }

                //planners[6] = new TwoWheeledMovement(_predictor, TwoWheeledMovement.WhichTwoWheels.FrontLeftBackRight);
            }*/

            // create controller
            _controller = new RFCController(
                _commander, planner, _predictor
            );
           
            _interpreter = new Interpreter(IS_OUR_GOAL_LEFT, _predictor, _controller);

            running = false;
            if (wasRunning)
            {
                start();
            }
            initialized = true;

        }

        public void reloadPlays()
        {
            Dictionary<InterpreterPlay, string> plays = PlayUtils.loadPlays(PLAY_DIR);
             _interpreter.LoadPlays(new List<InterpreterPlay>(plays.Keys));
        }
       
        public Interpreter getInterpreter() {
            return _interpreter;
        }

        public void registerCommander(IRobots commander)
        {
            if (!initialized)
                _commander = commander;
        }

        public void registerPredictor(IPredictor predictor)
        {
            if (!initialized)
                _predictor = predictor;
        }

        public void registerAcceptor(IVisionInfoAcceptor acceptor)
        {
            if (!initialized)
                _acceptor = acceptor;
        }

        public void setSleepTime(int millis)
        {
            _sleepTime = millis;
        }

        public void start()
        {
            if (!running)
            {
                if (!initialized)
                    initialize();

                _sleepTime = Constants.get<int>("default", "UPDATE_SLEEP_TIME");
                isYellow = Constants.get<string>("configuration", "OUR_TEAM") == "YELLOW";

                _refbox.start();
                t = new System.Timers.Timer(_sleepTime);
                t.AutoReset = true;
                t.Elapsed += delegate(object sender, System.Timers.ElapsedEventArgs e)
                {
                    Thread.CurrentThread.Name = "RFCSystem timer thread";
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
                //stop the timer:
                t.Enabled = false;
                _refbox.stop();
                foreach (RobotInfo info in Predictor.GetRobots(team))
                {
                    _controller.stop(info.ID);
                }
                Console.WriteLine("--------------DONE RUNNING: -----------------");
            }

        }

        public event EventHandler RoundRan;

        private void runRound()
        {
            if (counter % 100 == 0)
                Console.WriteLine("--------------RUNNING ROUND: " + counter + "-----------------");
           // _controller.clearArrows();            
            _interpreter.interpret(
                _refbox.GetCurrentPlayType()
            );
            if (RoundRan != null)
                RoundRan(this, new EventArgs());            
            counter++;
        }

        public PlayTypes getCurrentPlayType() {
            return _refbox.GetCurrentPlayType();
        }

        public void drawCurrent(System.Drawing.Graphics g, ICoordinateConverter converter)
        {            
            _controller.drawCurrent(g, converter);

        }
    }
}
//#endif