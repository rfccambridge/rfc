using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Robocup.Core;

namespace Robocup.Plays
{
    /// <summary>
    /// This class wraps an IPredictor and negates all of the coordinates
    /// (ie if you ask for robot info, its position will be the negative of what
    /// the base predictor would say).
    /// </summary>
    internal class FlipPredictor : IPredictor
    {
        private IPredictor predictor;

        public IPredictor Predictor
        {
            get { return predictor; }
        }

        public FlipPredictor(IPredictor predictor)
        {
            this.predictor = predictor;
        }
        /// <summary>
        /// Creates a copy of the given RobotInfo, and flips the position, velocity, and orientation
        /// </summary>
        private RobotInfo flipRobotInfo(RobotInfo info)
        {
            return new RobotInfo(-info.Position, -info.Velocity, -info.AngularVelocity,
                    Robocup.Geometry.UsefulFunctions.angleDifference(info.Orientation, -Math.PI / 2), info.Team, info.ID);
        }
        #region IPredictor Members

        public List<RobotInfo> GetRobots(Team team)
        {
            return predictor.GetRobots(team).ConvertAll<RobotInfo>(flipRobotInfo);
        }
        public List<RobotInfo> GetRobots() {
            return predictor.GetRobots().ConvertAll<RobotInfo>(flipRobotInfo);
        }
        public RobotInfo GetRobot(Team team, int id)
        {
            RobotInfo robot = predictor.GetRobot(team, id);
            if (robot != null)
                return flipRobotInfo(robot);
            return null;
        }
        public BallInfo GetBall()
        {
            BallInfo info = predictor.GetBall();
            if (info == null)
                return null;

            return new BallInfo(-info.Position, -info.Velocity);
        }

        public void SetBallMark() {
            predictor.SetBallMark();
        }

        public void ClearBallMark() {
            predictor.ClearBallMark();
        }

        public bool HasBallMoved() {
            return predictor.HasBallMoved();
        }

        public void SetPlayType(PlayType newPlayType) {
            predictor.SetPlayType(newPlayType);
        }

        public void LoadConstants()
        {
        }

        #endregion
    }
    /// <summary>
    /// Wraps an IActionInterpreter, with the effect that all calls have their positions negated.
    /// </summary>
    internal class FlipActionInterpreter : IActionInterpreter
    {
        private IActionInterpreter actionInterpreter;

        public IActionInterpreter ActionInterpreter
        {
            get { return actionInterpreter; }
        }

        public FlipActionInterpreter(IActionInterpreter actionInterpreter)
        {
            this.actionInterpreter = actionInterpreter;
        }

        #region IActionInterpreter Members

        public void Charge(int robotID) {
            actionInterpreter.Charge(robotID);
        }
        public void Kick(int robotID, Vector2 target)
        {
            actionInterpreter.Kick(robotID, -target);
        }
        public void Bump(int robotID, Vector2 target) 
        {
            actionInterpreter.Bump(robotID, -target);
        }
        public void Move(int robotID, Vector2 target)
        {
            actionInterpreter.Move(robotID, -target);
        }

        public void Move(int robotID, Vector2 target, Vector2 facing)
        {
            actionInterpreter.Move(robotID, -target, -facing);
        }

        public void Stop(int robotID)
        {
            actionInterpreter.Stop(robotID);
        }

        public void Dribble(int robotID, Vector2 target)
        {
            actionInterpreter.Dribble(robotID, -target);
        }

        public void LoadConstants()
        {
            actionInterpreter.LoadConstants();
        }

        #endregion
    }
}