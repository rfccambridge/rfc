using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

using Robocup.Core;
using Robocup.Utilities;

using Navigator = Navigation.Examples.DumbNavigator;

namespace SoccerSim
{
    /*class SimController : IController
    {
        const int TEAMSIZE = 5;
        const double speed = 0.02;
        Random r = new Random();

        IPredictor _state;
        PhysicsEngine physics_engine;
        FieldView _view;
        public SimController(PhysicsEngine physics_engine, FieldView view)
        {
            _state = physics_engine;
            this.physics_engine = physics_engine;
            _view = view;
        }

        #region IController Members

        // move to dest while facing ball
        public void move(int robotID, bool avoidBall, Vector2 dest)
        {
            BallInfo ball = _state.getBallInfo();

            int newid = robotID;

            // how is this working?
            int change = 1;
            if (robotID >= TEAMSIZE)
            {
                change = -1;
            }
            RobotInfo r = _state.getCurrentInformation(robotID);
            move(robotID, avoidBall, dest, (double)Math.Atan2(ball.Position.Y - r.Position.Y, change * (ball.Position.X - r.Position.X)));
        }

        const double distThreshold = .005;
        private const double chop = .001;

        private Dictionary<int, IMovement> _planners;
        public IMovement GetPlanner(int robotId)
        {
            if (!_planners.ContainsKey(robotId))
                throw new ApplicationException("trying to move with a robot that doesn't have an IMovement defined");
            return _planners[robotId];
        }

        Navigator _navigator = new Navigator(),
            _otherNavigator = new Navigator();
        public void move(int robotID, bool avoidBall, Vector2 destination, double orientation)
        {
            RobotInfo[] infos = _state.getOurTeamInfo().ToArray();
            RobotInfo[] otherinfo = _state.getTheirTeamInfo().ToArray();
            Navigator n = _navigator;
            BallInfo ball = _state.getBallInfo();

            int navigatorId = robotID;
            if (robotID >= TEAMSIZE)
            {
                //infos = _state.getTheirTeamInfo().ToArray();
                //otherinfo = _state.getOurTeamInfo().ToArray();
                n = _otherNavigator;
            }
            Vector2 ballPosition = ball.Position;
            Vector2 position = _state.getCurrentInformation(robotID).Position;
            if (position.distanceSq(destination) <= distThreshold * distThreshold)
                return;
            double ballAvoidance = 0;
            if (avoidBall)
                ballAvoidance = (double)Math.Max(1, Math.Min(1.7, (1 + 1 * Math.Sqrt(ball.Velocity.magnitudeSq())) * (2.40 - 1.5 * ((destination - ballPosition).normalize() * (position - ballPosition).normalize()))));

            NavigationResults results = n.navigate(navigatorId, position, destination, infos, otherinfo, ball, .12);

            WheelSpeeds motorSpeeds = GetPlanner(robotID).calculateWheelSpeeds(robotID, thisRobot, results);

            physics_engine.SetWheelSpeeds(robotID, motorSpeeds);

            /*RobotInfo prev = _state.getCurrentInformation(robotID);

            if (position.distanceSq(destination) > chop * chop)
            {
                _view.addArrow(new Arrow(_view.fieldtopixelPoint(position), _view.fieldtopixelPoint(waypoint), Color.Green, 3.0));
                _view.addArrow(new Arrow(_view.fieldtopixelPoint(position), _view.fieldtopixelPoint(destination), Color.Red, 3.0));
                if (prev.Position != waypoint)
                {
                    _acceptor.updateRobot(robotID, new RobotInfo(prev.Position + speed * (waypoint - prev.Position).normalize(), (prev.Orientation * .85 + orientation * .15), prev.ID));
                }
            }*
        }

        public void kick(int robotID)
        {
            physics_engine.Kick(robotID);
        }

        public void stop(int robotID)
        {
            physics_engine.Stop(robotID);
        }

        #endregion

        
    }*/
}
