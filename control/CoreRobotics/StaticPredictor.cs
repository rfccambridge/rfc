using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Geometry;
using Robocup.Core;
using Robocup.Utilities;

namespace Robocup.CoreRobotics
{
    /// <summary>
    /// A basic implementation of IPredictor that always returns the information from its previous update 
    /// no matter how long ago the update happened.
    /// </summary>
    public class StaticPredictor : IPredictor, IInfoAcceptor
    {          
        BallInfo _ballInfo = new BallInfo(new Vector2(0, 0));
        Dictionary<Team, Dictionary<int, RobotInfo>> _robots = new Dictionary<Team, Dictionary<int, RobotInfo>>();

        public StaticPredictor()
        {
            foreach (Team team in Enum.GetValues(typeof(Team)))
                _robots.Add(team, new Dictionary<int,RobotInfo>());
        }

        #region IPredictor Members

        public List<RobotInfo> GetRobots(Team team)
        {
            List<RobotInfo> robots = new List<RobotInfo>();
            robots.AddRange(_robots[team].Values);
            return robots;
        }
        public List<RobotInfo> GetRobots()
        {
            List<RobotInfo> combined = new List<RobotInfo>();
            foreach (Team team in Enum.GetValues(typeof(Team)))
                combined.AddRange(_robots[team].Values);
            return combined;
        }
        public RobotInfo GetRobot(Team team, int id)
        {
            if (_robots[team].ContainsKey(id))
                return _robots[team][id];
            return null;
        }
        public BallInfo GetBall()
        {
            return _ballInfo;
        }

        public void SetBallMark() {
            throw new NotImplementedException();
        }

        public void ClearBallMark() {
            throw new NotImplementedException();
        }

        public bool HasBallMoved() {
            throw new NotImplementedException();
        }

        public void SetPlayType(PlayType newPlayType) {
            throw new NotImplementedException();
        }

        public void LoadConstants()
        {
        }

        #endregion

        #region IInfoAcceptor Members

        public void UpdateBallInfo(BallInfo ballInfo)
        {
            this._ballInfo = ballInfo;            
        }

        public void UpdateRobot(RobotInfo newInfo)
        {
            if (_robots[newInfo.Team].ContainsKey(newInfo.ID))
                _robots[newInfo.Team][newInfo.ID] = newInfo;
            else
                _robots[newInfo.Team].Add(newInfo.ID, newInfo);
        }
        #endregion
    }
}
