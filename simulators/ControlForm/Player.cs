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

        //The loop for actual playing
        private FunctionLoop _interpretLoop;

        //Lock for starting and stopping in a synchronized way
        private Object _startStopLock = new Object();

        public Team Team
        {
            get { return _team; }
        }

        public FieldHalf FieldHalf
        {
            get { return _fieldHalf; }
            set
            {
                if (Running)
                    throw new ApplicationException("Cannot change field half while running.");
                _fieldHalf = value;
                _interpreter.FieldHalf = _fieldHalf;
            }
        }

        public bool Running
        {
            get { 
                bool b;
                lock(_startStopLock) {b = _interpretLoop.IsRunning();} 
                return b;
            }
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

           // _motionPlanner = new Robocup.MotionControl.TangentBugModelFeedbackMotionPlanner();
			//_motionPlanner = new Robocup.MotionControl.BasicRRTModelFeedbackMotionPlanner();
            //_motionPlanner = new Robocup.MotionControl.SmoothedRRTModelFeedbackMotionPlanner();
            _motionPlanner = new Robocup.MotionControl.SmoothedRRTVelocityPlanner();
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

            _controller = new Controller(_team, _motionPlanner, _predictor, _refbox, _fieldDrawer);            

            _interpreter = new Interpreter(_team, _fieldHalf, _predictor, _controller, _fieldDrawer);            

            LoadConstants();

            _interpretLoop = new FunctionLoop(InterpretLoop);
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
            lock (_startStopLock)
            {
                if (Running)
                    throw new ApplicationException("Already running.");

                _fieldDrawer.UpdateTeam(_team);
                _controller.StartControlling();
                _interpretLoop.SetPeriod(1.0 / Constants.Time.STRATEGY_FREQUENCY);
                _interpretLoop.Start();
            }
        }

        public virtual void Stop()
        {
            lock (_startStopLock)
            {
                if (!Running)
                    throw new ApplicationException("Not running.");

                _interpretLoop.Stop();

                foreach (RobotInfo info in _predictor.GetRobots(_team))
                    _controller.Stop(info.ID);
                _controller.StopControlling();
            }
        }

        private void InterpretLoop()
        {
            _fieldDrawer.UpdateInterpretFreq(1.0 / _interpretLoop.GetObservedPeriod());

            List<RobotInfo> robots = _predictor.GetRobots();
            BallInfo ball = _predictor.GetBall();

            _fieldDrawer.BeginCollectState();
            try
            {
                _fieldDrawer.UpdateRobotsAndBall(robots, ball);
                //RobotPath[] paths = ((Controller)_controller).GetLastPaths(); //TODO(davidwu): This cast is not ideal...
                //for (int i = 0; i < paths.Length; i++)
                //    if (paths[i] != null)
                //        _fieldDrawer.DrawPath(paths[i]);

                PlayType playType = _refbox.GetCurrentPlayType();
                _fieldDrawer.UpdatePlayType(playType);

                doAction();
            }
            finally
            {
                _fieldDrawer.EndCollectState();
            }
            _fieldDrawer.UpdateInterpretDuration(_interpretLoop.GetLoopDuration() * 1000);
        }

        protected virtual void doAction()
        {
            PlayType playType = _refbox.GetCurrentPlayType();
            Score score = _refbox.GetScore();
            _interpreter.interpret(playType, score);
        }

    }
}
