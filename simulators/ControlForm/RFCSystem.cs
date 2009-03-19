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
        RFCController _controller;
        public Interpreter _interpreter;
        IRobots _commander;
        ISplitInfoAcceptor _acceptor;

        RefBoxState _refbox;

        public IPredictor Predictor
        {
            get { return _predictor; }
        }
        public ISplitInfoAcceptor Acceptor
        {
            get { return _acceptor; }
        }

        System.Timers.Timer t;

        private volatile bool running;
        private bool initialized;
        private int counter;

        private int _sleepTime;
        private bool isYellow;

        int REFBOX_PORT;
        string REFBOX_ADDR;
        string PLAY_DIR;

        public RFCSystem()
        {
            initialized = false;
            running = false;

            reloadConstants();
        }

        public void reloadConstants() {
            REFBOX_PORT = Constants.get<int>("default","REFBOX_PORT");
            REFBOX_ADDR = Constants.get<string>("default", "REFBOX_ADDR");
            PLAY_DIR = Constants.get<string>("default", "PLAY_DIR");
            _sleepTime = Constants.get<int>("default", "UPDATE_SLEEP_TIME");
            isYellow = Constants.get<string>("configuration", "OUR_TEAM_COLOR") == "YELLOW";
        }

        public void setRefBoxListener() {
            if (running) return;

            _refbox.setReferee(new MulticastRefBoxListener(REFBOX_ADDR, REFBOX_PORT));
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
            // create predictor
            if (_predictor == null && _acceptor == null)
            {
                BasicPredictor basicPredictor = new BasicPredictor();
                _predictor = basicPredictor;
                _acceptor = basicPredictor;
            }
            else if (_predictor == null)
            {
                _predictor = new BasicPredictor();
            }
            else if (_acceptor == null)
            {
                _acceptor = new BasicPredictor();
            }

            _refbox = new RefBoxState(new MulticastRefBoxListener(REFBOX_ADDR, REFBOX_PORT), _predictor, isYellow);


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
            IMotionPlanner planner = new Robocup.MotionControl.DefaultMotionPlanner();

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

            // create interpreter from file
            PlayLoader<InterpreterPlay, InterpreterExpression> loader =
                new PlayLoader<InterpreterPlay, InterpreterExpression>(new InterpreterExpression.Factory());

            string[] files = System.IO.Directory.GetFiles(PLAY_DIR);
            List<InterpreterPlay> plays = new List<InterpreterPlay>();
            Dictionary<InterpreterPlay, string> filenames = new Dictionary<InterpreterPlay, string>();
            foreach (string fname in files)
            {
                string extension = Path.GetExtension(fname);
                if (extension != ".txt")
                    continue;
                StreamReader reader = new StreamReader(fname);
                string filecontents = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();

                try
                {
                    InterpreterPlay p = loader.load(filecontents);
                    plays.Add(p);
                    filenames.Add(p, fname);
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.StackTrace);
                    Console.WriteLine("error loading play \"" + fname + "\": " + ex.ToString());
                }
            }

            _interpreter = new Interpreter((Constants.get<int>("plays","IS_OUR_GOAL_LEFT")!=1), plays.ToArray(), _predictor, _controller);

            running = false;
            if (wasRunning)
            {
                start();
            }
            initialized = true;

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

        public void registerAcceptor(ISplitInfoAcceptor acceptor)
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
                isYellow = Constants.get<string>("configuration", "OUR_TEAM_COLOR") == "YELLOW";

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
                foreach (RobotInfo info in Predictor.getOurTeamInfo())
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
            _controller.clearArrows();
            _interpreter.interpret(
                _refbox.GetCurrentPlayType()
            );
            if (RoundRan != null)
                RoundRan(this, new EventArgs());
            Console.WriteLine("play type is: " + _refbox.GetCurrentPlayType());
            counter++;
        }

        public void drawCurrent(System.Drawing.Graphics g, ICoordinateConverter converter)
        {
            _controller.drawCurrent(g, converter);
        }
    }
}
//#endif