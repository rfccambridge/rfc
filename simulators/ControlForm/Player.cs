using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using Robocup.Utilities;
using Robocup.Plays;
using System.Threading;
using Robocup.ControlForm;
using Robocup.CoreRobotics;

namespace Robocup.ControlForm
{
    public class Player
    {
        protected string _name;
        protected Team _team;
        protected FieldHalf _fieldHalf;

        // These are created outside and handed to a Player (refbox should be in this list after
        // the refbox refactoring project)
        protected IPredictor _predictor;
        protected FieldDrawer _fieldDrawer;

        // These are created inside the player
        protected Interpreter _interpreter;
        protected IController _controller;
        protected IMotionPlanner _motionPlanner;
        protected RefBoxState _refbox;
        
        protected bool _running = false;

        System.Timers.Timer _interpretLoopTimer = new System.Timers.Timer();
        int _interpretLoopTimerSync = 0;
        HighResTimer _timerFreq = new HighResTimer();
        HighResTimer _timerDuration = new HighResTimer();

        public Team Team
        {
            get { return _team; }
        }

        public FieldHalf FieldHalf
        {
            get { return _fieldHalf; }
            set
            {
                if (_running)
                    throw new ApplicationException("Cannot change field half while running.");
                _fieldHalf = value;
                _interpreter.FieldHalf = _fieldHalf;
            }
        }

        public bool Running
        {
            get { return _running; }
        }

        public Player(string name, Team team, FieldHalf fieldHalf, FieldDrawer fieldDrawer, IPredictor predictor) {
            _name = name;
            _team = team;
            _fieldHalf = fieldHalf;
            _fieldDrawer = fieldDrawer;
            _predictor = predictor;
            
            //TODO: Change the PlayTypes to Yellow/Blue instead of Ours/Theirs. Then it would
            //      be possible to (1) to hide usage of MulticastRefBoxListener inside RefBoxState
            //      or just encapsulate it completely, and (2) create one refbox outside of player
            //      and share it among all players who want a refbox.
            _refbox = new RefBoxState(_team, _predictor);

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
            //_motionPlanner = new Robocup.MotionControl.TangentBugFeedbackMotionPlanner();

            _motionPlanner = new Robocup.MotionControl.TangentBugModelFeedbackMotionPlanner();
			//_motionPlanner = new Robocup.MotionControl.BasicRRTModelFeedbackMotionPlanner();
			//IMotionPlanner planner = new Robocup.MotionControl.TangentBugVeerMotionPlanner();

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

            _controller = new Controller(_team, _motionPlanner, _predictor, _fieldDrawer);            

            _interpreter = new Interpreter(_team, _fieldHalf, _predictor, _controller, _fieldDrawer);            

            LoadConstants();

            for (int i = 0; i < 10; i++)
            {
                bool value;
                if (ConstantsRaw.nondestructiveGet("default", "ROBOT_HAS_KICKER_" + i, out value))
                    TagSystem.AddTag(i, "kicker");
                if (ConstantsRaw.nondestructiveGet("default", "ROBOT_IS_GOALIE_" + i, out value))
                    TagSystem.AddTag(i, "goalie");
            }

            _interpretLoopTimer.AutoReset = true;
            _interpretLoopTimer.Elapsed += _interpretLoopTimer_Elapsed;
        }

        public override string ToString()
        {
            string fullClass = base.ToString();
            string[] tokens = fullClass.Split(new char[] { '.' });
            string name = _name.Length > 0 ? " (" + _name + ")" : "";
            return tokens[tokens.Length - 1] + name + ": " + _team.ToString() + ", " + _fieldHalf.ToString();
        }

        public virtual void LoadConstants()
        {
            // TODO: remove redundant ifs
            if (_controller != null)
                _controller.LoadConstants();
            if (_motionPlanner != null)
                _motionPlanner.LoadConstants();
            if (_interpreter != null)
                _interpreter.LoadConstants();
        }        

        public void ConnectToController(string host, int port)
        {
            _controller.Connect(host, port);
        }

        public void DisconnectFromController()
        {
            _controller.Disconnect();
        }

        public void RegisterPredictor(IPredictor predictor)
        {
            _predictor = predictor;
        }

        // Unfortunately, RefBox needs to be created inside the player because it depends on team.
        // This will be changed in the refbox refactoring project.
        public void ConnectToRefbox(IRefBoxListener refboxListener)
        {
            _refbox.Connect(refboxListener);
        }

        public void DisconnectFromRefbox()
        {
            _refbox.Disconnect();
        }

        public virtual void LoadPlays(List<InterpreterPlay> plays)
        {            
            _interpreter.LoadPlays(plays);
        }

        public virtual void Start()
        {
            if (_running)
                throw new ApplicationException("Already running.");

            double freq = ConstantsRaw.get<double>("default", "STRATEGY_FREQUENCY");
            double period = 1.0 / freq * 1000; // in ms

            _running = true;

            _fieldDrawer.UpdateTeam(_team);            

            _controller.StartControlling();

            _interpretLoopTimer.Interval = period;                        
            _interpretLoopTimer.Start();
        }

        public virtual void Stop()
        {
            if (!_running)
                throw new ApplicationException("Not running.");

            _interpretLoopTimer.Stop();
            
            foreach (RobotInfo info in _predictor.GetRobots(_team))
                _controller.Stop(info.ID);
			_controller.StopControlling();
            _running = false;
        }

        private void runRound()
        {
            _timerFreq.Stop();
            _fieldDrawer.UpdateInterpretFreq(1.0 / _timerFreq.Duration);
            _timerFreq.Start();

            List<RobotInfo> robots = _predictor.GetRobots();
            BallInfo ball = _predictor.GetBall();

            _fieldDrawer.BeginCollectState();
            _fieldDrawer.UpdateRobotsAndBall(robots, ball);

            PlayType playType = _refbox.GetCurrentPlayType();
            _fieldDrawer.UpdatePlayType(playType);            

            _timerDuration.Start();
            doAction();
            _timerDuration.Stop();

            _fieldDrawer.EndCollectState();

            _fieldDrawer.UpdateInterpretDuration(_timerDuration.Duration * 1000);
        }

        protected virtual void doAction()
        {
            PlayType playType = _refbox.GetCurrentPlayType();
            Score score = _refbox.GetScore();
            _interpreter.interpret(playType, score);
        }
        
        void _interpretLoopTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {            
            // Skip the event if a previous one is still being handled
            if (Interlocked.CompareExchange(ref _interpretLoopTimerSync, 1, 0) == 0)
            {
                runRound();
                _interpretLoopTimerSync = 0;
            }
        }
    }
}
