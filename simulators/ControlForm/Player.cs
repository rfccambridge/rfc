using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using Robocup.Utilities;
using Robocup.Plays;
using System.Threading;

namespace Robocup.CoreRobotics
{
    public class Player
    {
        protected Team _team;
        protected FieldHalf _fieldHalf;        

        protected IPredictor _predictor;
        protected Interpreter _interpreter;
        protected IController _controller;
        protected IMotionPlanner _motionPlanner;
        protected RefBoxState _refbox;
        protected FieldDrawer _fieldDrawer;

        protected bool _running = false;

        System.Timers.Timer _interpretLoopTimer;
        HighResTimer _timerFreq = new HighResTimer();
        HighResTimer _timerDuration = new HighResTimer();

        public bool Running
        {
            get { return _running; }
        }

        public Player(Team team, FieldHalf fieldHalf,
                      IPredictor predictor, IRobots commander, FieldDrawer fieldDrawer) {
            _team = team;
            _fieldHalf = fieldHalf;            
            _fieldDrawer = fieldDrawer;
            _predictor = predictor;            

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
            _motionPlanner = new Robocup.MotionControl.TangentBugFeedbackMotionPlanner();

            //_motionPlanner = new Robocup.MotionControl.TangentBugModelFeedbackMotionPlanner();
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

            _controller = new RFCController(_team, commander, _motionPlanner, _predictor, _fieldDrawer);            

            _interpreter = new Interpreter(_team, _fieldHalf, _predictor, _controller, _fieldDrawer);            

            LoadConstants();

            for (int i = 0; i < 10; i++)
            {
                bool value;
                if (Constants.nondestructiveGet("default", "ROBOT_HAS_KICKER_" + i, out value))
                    TagSystem.AddTag(i, "kicker");
                if (Constants.nondestructiveGet("default", "ROBOT_IS_GOALIE_" + i, out value))
                    TagSystem.AddTag(i, "goalie");
            }
        }

        public virtual void LoadConstants()
        {
            if (_controller != null)
                _controller.LoadConstants();
            if (_motionPlanner != null)
                _motionPlanner.LoadConstants();
        }

        public void SetRefBoxListener(IRefBoxListener refboxListener)
        {
            _refbox.setReferee(refboxListener);
        }

        public virtual void LoadPlays(List<InterpreterPlay> plays)
        {            
            _interpreter.LoadPlays(plays);
        }

        public virtual void Start()
        {
            double freq = Constants.get<double>("default", "STRATEGY_FREQUENCY");
            double period = 1.0 / freq * 1000; // in ms

            _running = true;

            _fieldDrawer.UpdateTeam(_team);

            _refbox.start();
            _interpretLoopTimer = new System.Timers.Timer(period);
            _interpretLoopTimer.AutoReset = true;
            _interpretLoopTimer.Elapsed += delegate(object sender, System.Timers.ElapsedEventArgs e)
            {
                _interpretLoopTimer.Enabled = false;
                Thread.CurrentThread.Name = "InterpretLoopTimer thread [" + _team.ToString() + "]";
                runRound();
                _interpretLoopTimer.Enabled = true;
            };
            _interpretLoopTimer.Start();
        }

        public virtual void Stop()
        {
            _interpretLoopTimer.Stop();
            _refbox.stop();
            foreach (RobotInfo info in _predictor.GetRobots(_team))
                _controller.stop(info.ID);
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
            _interpreter.interpret(playType);
        }
    }
}
