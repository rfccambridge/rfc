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
            throw new NotImplementedException("not implemented");
        }

        public List<RobotInfo> getTheirTeamInfo()
        {
            throw new NotImplementedException("not implemented");
        }

        public List<RobotInfo> getAllInfos()
        {
            throw new NotImplementedException("not implemented");
        }

        public BallInfo getBallInfo()
        {
            throw new NotImplementedException("not implemented");
        }

        // TO REPLACE THE ABOVE
        public List<RobotInfo> GetRobots(int team)
        {
            return predictor.GetRobots(team).ConvertAll<RobotInfo>(flipRobotInfo);
        }
        public List<RobotInfo> GetRobots() {
            return predictor.GetRobots().ConvertAll<RobotInfo>(flipRobotInfo);
        }
        public RobotInfo GetRobot(int team, int id)
        {
            throw new NotImplementedException("not implemented");
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

        public void SetPlayType(PlayTypes newPlayType) {
            predictor.SetPlayType(newPlayType);
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

        // TO REPLACE THE ABOVE:

        public List<RobotInfo> GetRobots(int team)
        {
            throw new NotImplementedException("not implemented");
        }
        public List<RobotInfo> GetRobots() {
            throw new NotImplementedException("not implemented");
        }
        
        public RobotInfo GetRobot(int team, int id)
        {
            throw new NotImplementedException("not implemented");
        }
        public BallInfo GetBall()
        {
            throw new NotImplementedException("not implemented");
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

        public void SetPlayType(PlayTypes newPlayType) {
            predictor.SetPlayType(newPlayType);
        }

        #endregion
    }


}