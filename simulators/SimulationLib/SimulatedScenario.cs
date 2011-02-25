﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robocup.Core;
using Robocup.CoreRobotics;

namespace Robocup.Simulation
{
    public struct SimulatorScene
    {
        public Dictionary<Team, List<RobotInfo>> Robots;
        public BallInfo Ball;
    }

    /// <summary>
    /// This class provides the opportunity to simulate different scenarios (f.e. penalty shootout)
    /// with PhysicsEngine. A scenario is roughly defined by the robot positions and the way the simulated 
    /// world reacts to things like goals, or the ball moving.
    /// </summary>
    abstract public class SimulatedScenario
    {
        protected string _name;
        protected PhysicsEngine _engine;
        protected double FIELD_WIDTH;
        protected double FIELD_HEIGHT;
        protected double FREEKICK_DISTANCE;

        public SimulatedScenario(string name, PhysicsEngine engine)
        {
            _name = name;
            _engine = engine;

            LoadConstants();
        }

        public abstract SimulatorScene GetScene();
        public abstract void GoalScored();
        public abstract void BallOut(Vector2 lastPosition);
        public abstract bool SupportsNumbers { get; }

        public virtual void LoadConstants()
        {
            FIELD_WIDTH = Constants.get<double>("plays", "FIELD_WIDTH");
            FIELD_HEIGHT = Constants.get<double>("plays", "FIELD_HEIGHT");
            FREEKICK_DISTANCE = Constants.get<double>("plays", "FREEKICK_DISTANCE");
        }

        public override string ToString()
        {
            return _name;
        }
    }

    /// <summary>
    /// A regular game scenario. On scored, ball is reset to center. When ball out, placed 
    /// appropriately for a freekick.
    /// </summary>
    public class NormalGameScenario : SimulatedScenario
    {
        public NormalGameScenario(string name, PhysicsEngine engine) :
            base(name, engine)
        {
            _engine.Referee.SetCurrentCommand(MulticastRefBoxListener.KICKOFF_BLUE);
            _engine.Referee.EnqueueCommand(MulticastRefBoxListener.READY, 2000);
        }

        public override bool SupportsNumbers { get { return true; } }

        public override SimulatorScene GetScene()
        {
            SimulatorScene result = new SimulatorScene();

            int numYellow = _engine.NumYellow;
            int numBlue = _engine.NumBlue;

            Dictionary<Team, List<RobotInfo>> robots = new Dictionary<Team, List<RobotInfo>>();

            List<RobotInfo> yellowRobots = new List<RobotInfo>();
            List<RobotInfo> blueRobots = new List<RobotInfo>();

            yellowRobots.Add(new RobotInfo(new Vector2(-1.0, -1), 0, Team.Yellow, 0));
            if (numYellow > 1)
                yellowRobots.Add(new RobotInfo(new Vector2(-1.0, 0), 0, Team.Yellow, 1));
            if (numYellow > 2)
                yellowRobots.Add(new RobotInfo(new Vector2(-1.0, 1), 0, Team.Yellow, 2));
            if (numYellow > 3)
                yellowRobots.Add(new RobotInfo(new Vector2(-2f, -1), 0, Team.Yellow, 3));
            if (numYellow > 4)
                yellowRobots.Add(new RobotInfo(new Vector2(-2f, 1), 0, Team.Yellow, 4));

            blueRobots.Add(new RobotInfo(new Vector2(1.0, -1), Math.PI, Team.Blue, 5));
            if (numBlue > 1)
                blueRobots.Add(new RobotInfo(new Vector2(1.0, 0), Math.PI, Team.Blue, 6));
            if (numBlue > 2)
                blueRobots.Add(new RobotInfo(new Vector2(1.0, 1), Math.PI, Team.Blue, 7));
            if (numBlue > 3)
                blueRobots.Add(new RobotInfo(new Vector2(2f, -1), Math.PI, Team.Blue, 8));
            if (numBlue > 4)
                blueRobots.Add(new RobotInfo(new Vector2(2f, 1), Math.PI, Team.Blue, 9));

            robots[Team.Yellow] = yellowRobots;
            robots[Team.Blue] = blueRobots;

            result.Robots = robots;
            result.Ball = new BallInfo(Vector2.ZERO);

            return result;
        }

        public override void GoalScored()
        {
            _engine.UpdateBall(new BallInfo(Vector2.ZERO));

            // Update referee state
            // XXX: Assume Blue always gets everything for now, implement _engine.LastTouched
            _engine.Referee.SetCurrentCommand(MulticastRefBoxListener.STOP);
            _engine.Referee.EnqueueCommand(MulticastRefBoxListener.KICKOFF_BLUE, 5000);
            _engine.Referee.EnqueueCommand(MulticastRefBoxListener.READY, 2000);
        }

        public override void BallOut(Vector2 lastPosition)
        {
            double freeKickX, freeKickY;

            // Make sure we are some distance away from field lines (as by rule)
            if (lastPosition.X > FIELD_WIDTH / 2 - FREEKICK_DISTANCE)
                freeKickX = FIELD_WIDTH / 2 - FREEKICK_DISTANCE;
            else if (lastPosition.X < -FIELD_WIDTH / 2 + FREEKICK_DISTANCE)
                freeKickX = -FIELD_WIDTH / 2 + FREEKICK_DISTANCE;
            else
                freeKickX = lastPosition.X;

            if (lastPosition.Y > FIELD_HEIGHT / 2 - FREEKICK_DISTANCE)
                freeKickY = FIELD_HEIGHT / 2 - FREEKICK_DISTANCE;
            else if (lastPosition.Y < -FIELD_HEIGHT / 2 + FREEKICK_DISTANCE)
                freeKickY = -FIELD_HEIGHT / 2 + FREEKICK_DISTANCE;
            else
                freeKickY = lastPosition.Y;

            BallInfo newBall = new BallInfo(new Vector2(freeKickX, freeKickY));
            _engine.UpdateBall(newBall);

            // Update referee state
            // XXX: Assume Blue always gets everything for now, implement _engine.LastTouched
            _engine.Referee.SetCurrentCommand(MulticastRefBoxListener.STOP);
            _engine.Referee.EnqueueCommand(MulticastRefBoxListener.INDIRECT_BLUE, 5000);
        }
    }

    /// <summary>
    /// A shootout game scenario. A few predefined shoot positions that get changed on score.
    /// On ball out, the same position is repeated.
    /// </summary>
    public class ShootoutGameScenario : SimulatedScenario
    {
        private int _sceneIndex;

        private const int NUM_SCENES = 5;

        public ShootoutGameScenario(string name, PhysicsEngine engine)
            : base(name, engine)
        {
            _sceneIndex = 0;

            _engine.Referee.SetCurrentCommand(MulticastRefBoxListener.START);
        }

        public override bool SupportsNumbers { get { return false; } }

        private SimulatorScene CreateScene(int index)
        {
            SimulatorScene scene = new SimulatorScene();
            Dictionary<Team, List<RobotInfo>> robots = new Dictionary<Team, List<RobotInfo>>();

            List<RobotInfo> yellowRobots = new List<RobotInfo>();
            List<RobotInfo> blueRobots = new List<RobotInfo>();

            //Add goalie
            yellowRobots.Add(new RobotInfo(new Vector2(-FIELD_WIDTH / 2 + 0.2, 0f), 0, Team.Yellow, 0));

            //Shooter and ball based on current scene
            switch (index)
            {
                case 0:
                    blueRobots.Add(new RobotInfo(new Vector2(-2.0f, FIELD_HEIGHT / 2 - 0.5), Math.PI, Team.Blue, 5));
                    scene.Ball = new BallInfo(new Vector2(-2.2f, FIELD_HEIGHT / 2 - 0.7));
                    break;
                case 1:
                    blueRobots.Add(new RobotInfo(new Vector2(-1.5f, FIELD_HEIGHT / 2 - 1.0), Math.PI, Team.Blue, 5));
                    scene.Ball = new BallInfo(new Vector2(-1.7f, FIELD_HEIGHT / 2 - 1.2));
                    break;
                case 2:
                    blueRobots.Add(new RobotInfo(new Vector2(-1.3f, 0), Math.PI, Team.Blue, 5));
                    scene.Ball = new BallInfo(new Vector2(-1.5f, 0));
                    break;
                case 3:
                    blueRobots.Add(new RobotInfo(new Vector2(-1.5f, -FIELD_HEIGHT / 2 + 1.0), Math.PI, Team.Blue, 5));
                    scene.Ball = new BallInfo(new Vector2(-1.7f, -FIELD_HEIGHT / 2 + 1.2));
                    break;
                case 4:
                    blueRobots.Add(new RobotInfo(new Vector2(-2.0f, -FIELD_HEIGHT / 2 + 0.5), Math.PI, Team.Blue, 5));
                    scene.Ball = new BallInfo(new Vector2(-2.2f, -FIELD_HEIGHT / 2 + 0.7));
                    break;
                default:
                    break;
            }

            robots[Team.Yellow] = yellowRobots;
            robots[Team.Blue] = blueRobots;

            scene.Robots = robots;

            return scene;
        }

        public override SimulatorScene GetScene()
        {
            return CreateScene(_sceneIndex);
        }

        public override void GoalScored()
        {
            _sceneIndex = (_sceneIndex + 1) % NUM_SCENES;
            _engine.GetScenarioScene();
        }

        public override void BallOut(Vector2 lastPosition)
        {
            //Restart from the same shootout position
            _engine.GetScenarioScene();
        }
    }
}