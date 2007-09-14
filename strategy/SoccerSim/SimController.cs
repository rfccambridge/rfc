using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

using Robocup.Infrastructure;
using Navigator = Navigation.Examples.DumbNavigator;

namespace SoccerSim
{
    class SimController : IController
    {
        const int TEAMSIZE = 5;
        const float speed = 0.02f;
        const float ballspeed = .06f;
        Random r = new Random();

        float ballVx;
        float ballVy;

        IPredictor _state;
        IInfoAcceptor _acceptor;
        FieldView _view;
        public SimController(IPredictor state, IInfoAcceptor acceptor, FieldView view)
        {
            _state = state;
            _acceptor = acceptor;
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
            move(robotID, avoidBall, dest, (float)Math.Atan2(ball.Position.Y - r.Position.Y, change * (ball.Position.X - r.Position.X)));
        }

        const float distThreshold = .005f;
        private const float chop = .001f;

        Navigator _navigator = new Navigator(),
            _otherNavigator = new Navigator();
        public void move(int robotID, bool avoidBall, Vector2 destination, float orientation)
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
                ballAvoidance = (float)Math.Max(1, Math.Min(1.7, (1 + 1 * Math.Sqrt(ball.Velocity.magnitudeSq())) * (2.40 - 1.5 * ((destination - ballPosition).normalize() * (position - ballPosition).normalize()))));

            Vector2 result = n.navigate(navigatorId, position, destination, infos, otherinfo, ball, .12f);

            RobotInfo prev = _state.getCurrentInformation(robotID);

            if (position.distanceSq(destination) > chop * chop)
            {
                _view.addArrow(new Arrow(_view.fieldtopixelPoint(position), _view.fieldtopixelPoint(result), Color.Green, 3.0f));
                _view.addArrow(new Arrow(_view.fieldtopixelPoint(position), _view.fieldtopixelPoint(destination), Color.Red, 3.0f));
                if (prev.Position != result)
                {
                    _acceptor.updateRobot(robotID, new RobotInfo(prev.Position + speed * (result - prev.Position).normalize(), (prev.Orientation * .85f + orientation * .15f), prev.ID));
                }
            }
        }

        public void kick(int robotID)
        {
            RobotInfo robot = _state.getCurrentInformation(robotID);
            // add randomness to actual robot location / direction
            const float randomComponent = ballspeed / 3;
            ballVx = (float)(ballspeed * Math.Cos(robot.Orientation));
            ballVy = (float)(ballspeed * Math.Sin(robot.Orientation));
            ballVx += (float)(r.NextDouble() * 2 - 1) * randomComponent;
            ballVy += (float)(r.NextDouble() * 2 - 1) * randomComponent;
            RobotInfo prev = robot;
            float recoil = 1.5f;
            _acceptor.updateRobot(robotID, new RobotInfo(prev.Position + (new Vector2(-ballVx * recoil, -ballVy * recoil)), prev.Orientation, prev.ID));
        }

        public void stop(int robotID)
        {
        }

        #endregion

        
    }
}
