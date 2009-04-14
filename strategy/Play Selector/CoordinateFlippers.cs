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
        public FlipPredictor(IPredictor predictor)
        {
            this.predictor = predictor;
        }
        /// <summary>
        /// Creates a copy of the given RobotInfo, and flips the position, velocity, and orientation
        /// </summary>
        private RobotInfo flipRobotInfo(RobotInfo info)
        {
            return new RobotInfo(-info.Position, -info.Velocity, -info.AngularVelocity, info.Orientation+Math.PI, info.ID);
        }
        #region IPredictor Members

        public RobotInfo getCurrentInformation(int robotID)
        {
            return flipRobotInfo(predictor.getCurrentInformation(robotID));
        }

        public List<RobotInfo> getOurTeamInfo()
        {
            return predictor.getOurTeamInfo().ConvertAll<RobotInfo>(flipRobotInfo);
        }

        public List<RobotInfo> getTheirTeamInfo()
        {
            return predictor.getTheirTeamInfo().ConvertAll<RobotInfo>(flipRobotInfo);
        }

        public List<RobotInfo> getAllInfos()
        {
            return predictor.getAllInfos().ConvertAll<RobotInfo>(flipRobotInfo);
        }

        public BallInfo getBallInfo()
        {
            BallInfo info = predictor.getBallInfo();
            return new BallInfo(-info.Position, -info.Velocity);
        }

        public void setBallMark() {
            predictor.setBallMark();
        }

        public void clearBallMark() {
            predictor.clearBallMark();
        }

        public bool hasBallMoved() {
            return predictor.hasBallMoved();
        }

        public void setPlayType(PlayTypes newPlayType) {
            predictor.setPlayType(newPlayType);
        }

        #endregion
    }
    /// <summary>
    /// Wraps an IActionInterpreter, with the effect that all calls have their positions negated.
    /// </summary>
    internal class FlipActionInterpreter : IActionInterpreter
    {
        private IActionInterpreter actionInterpreter;
        public FlipActionInterpreter(IActionInterpreter actionInterpreter)
        {
            this.actionInterpreter = actionInterpreter;
        }

        #region IActionInterpreter Members

        public void Kick(int robotID, Vector2 target)
        {
            actionInterpreter.Kick(robotID, -target);
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

        #endregion
    }

    /// <summary>
    /// This class wraps an IPredictor, and switches the meaning of "our team" and "their team"
    /// </summary>
    public class TeamFlipperPredictor : IPredictor
    {
        private IPredictor predictor;
        public TeamFlipperPredictor(IPredictor predictor)
        {
            this.predictor = predictor;
        }
        #region IPredictor Members

        public RobotInfo getCurrentInformation(int robotID)
        {
            return predictor.getCurrentInformation(robotID);
        }

        public List<RobotInfo> getOurTeamInfo()
        {
            return predictor.getTheirTeamInfo();
        }

        public List<RobotInfo> getTheirTeamInfo()
        {
            return predictor.getOurTeamInfo();
        }

        public List<RobotInfo> getAllInfos()
        {
            return predictor.getAllInfos();
        }

        public BallInfo getBallInfo()
        {
            return predictor.getBallInfo();
        }

        public void setBallMark() {
            predictor.setBallMark();
        }

        public void clearBallMark() {
            predictor.clearBallMark();
        }

        public bool hasBallMoved() {
            return predictor.hasBallMoved();
        }

        public void setPlayType(PlayTypes newPlayType) {
            predictor.setPlayType(newPlayType);
        }

        #endregion
    }


}