using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

using Robocup.Constants;
using Robocup.Infrastructure;
using RobocupPlays;


namespace Robocup.CoreRobotics
{
    // make this load/instantiate from a text file
    public class RFCSystem
    {
        IPredictor _predictor;
        RFCController _controller;
        Interpreter _interpreter;
        IRobots _commander;
        ISplitInfoAcceptor _acceptor;

        RefBoxListener reflistener;
        Vector2 markedPosition;
        bool marking = false;
        PlayTypes playsToRun;

        public IPredictor Predictor
        {
            get { return _predictor; }
        }
        public ISplitInfoAcceptor Acceptor
        {
            get { return _acceptor; }
        }


        Thread worker;
        private volatile bool running;
        private bool initialized;
        private int counter;

        private int _sleepTime;
        private bool isYellow;

        public RFCSystem()
        {
            initialized = false;
            running = false;
            _sleepTime = Constants.Constants.get<int>("UPDATE_SLEEP_TIME");
            isYellow = Constants.Constants.get<string>("OUR_TEAM_COLOR") == "YELLOW";
            playsToRun = PlayTypes.NormalPlay;
            reflistener = new RefBoxListener();
            
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


            // create helper interfaces
            if(_commander==null)
                _commander = new StubRobots();

            INavigator navigator = new Navigation.Examples.LookAheadBug();

            Dictionary<int, IMovement> planners = new Dictionary<int, IMovement>();
            for (int i = 0; i < 10; i++)
            {
                string move_model;
                bool exists = Constants.Constants.nondestructiveGet<string>("ROBOT_" + i, out move_model);
                if (!exists)
                    continue;
                if (move_model.Equals("TWO-FLBR"))
                {
                    planners[i] = new TwoWheeledMovement(_predictor, TwoWheeledMovement.WhichTwoWheels.FrontLeftBackRight);
                }
                else if (move_model.Equals("TWO-FRBL"))
                {
                    planners[i] = new TwoWheeledMovement(_predictor, TwoWheeledMovement.WhichTwoWheels.FrontRightBackLeft);
                }
                /*else if (move_model.Equals("FOUR"))
                {
                    //planners[i] = new TwoWheeledMovement(_predictor, TwoWheeledMovement.WhichTwoWheels.FrontLeftBackRight);
                    throw new ApplicationException("Unsupported Movement Model");
                }*/
                else
                {
throw new ApplicationException("invalid movement model: " + move_model);
                }

                //planners[6] = new TwoWheeledMovement(_predictor, TwoWheeledMovement.WhichTwoWheels.FrontLeftBackRight);
            }

            // create controller
            _controller = new RFCController(
                _commander, planners, navigator, _predictor
            );

            // create interpreter from file
            PlayLoader loader = new PlayLoader();
            string[] files = System.IO.Directory.GetFiles("C:/Microsoft Robotics Studio (1.0)/simulator/Simulator/Plays/temp");
            List<InterpreterPlay> plays = new List<InterpreterPlay>();
            Dictionary<InterpreterPlay, string> filenames = new Dictionary<InterpreterPlay, string>();
            foreach (string fname in files)
            {
                string extension = fname.Substring(1 + fname.LastIndexOf('.'));
                if (extension != "txt")
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
                catch (Exception)
                {
                    Console.WriteLine("error loading play \"" + fname + "\"");
                }
            }

            _interpreter = new Interpreter(false, plays.ToArray(), _predictor, _controller);

            running = false;
            if (wasRunning)
            {
                start();
            }
            initialized = true;

        }

        public void registerCommander(IRobots commander)
        {
            if ( ! initialized )
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

                _sleepTime = Constants.Constants.get<int>("UPDATE_SLEEP_TIME");
                isYellow = Constants.Constants.get<string>("OUR_TEAM_COLOR") == "YELLOW";

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
                foreach (RobotInfo info in Predictor.getOurTeamInfo())
                {
                    _controller.stop(info.ID);
                }
            }
            
        }

        public void run()
        {
            //TimeSpan sleepDuration = new TimeSpan(0, 0, 0, 0, _sleepTime);
            while (running)
            {

                //int curTime = DateTime.Now.Millisecond;
                if( counter % 100 == 0)
                    Console.WriteLine("--------------RUNNING ROUND: " + counter + "-----------------");

                _runRound();
                
                /*int timeToSleep = DateTime.Now.Millisecond - curTime;
                if(timeToSleep < 0)
                    timeToSleep = timeToSleep + 1000;*/



                //Thread.Sleep(Math.Max(1,_sleepTime - timeToSleep));
                counter++;
                Thread.Sleep(_sleepTime);
                
            }
            Console.WriteLine("--------------DONE RUNNING: -----------------");
        }

        private void _runRound()
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
                        if (! isYellow)
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

            _controller.clearArrows();
            _interpreter.interpret( 
                playsToRun 
            );
        }

        public void drawCurrent(System.Drawing.Graphics g, ICoordinateConverter converter)
        {
            _controller.drawCurrent(g, converter);
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
