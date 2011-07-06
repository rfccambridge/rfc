using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Robocup.Core;
using Robocup.CoreRobotics;
using Robocup.Geometry;

namespace Robocup.Core
{
    /// <summary>
    /// This class wraps an IPredictor and optionally negates all of the coordinates
    /// (ie if you ask for robot info, its position will be the negative of what
    /// the base predictor would say).
    /// </summary>
    public class FlippablePredictor : IPredictor
    {
        private IPredictor predictor;
        private bool isFlipped;

        public IPredictor Predictor
        {
            get { return predictor; }
        }

        public FlippablePredictor(IPredictor predictor)
        {
            this.predictor = predictor;
            this.isFlipped = false;
        }

        public void setFlip(bool b)
        {
            isFlipped = b;
        }

        /// <summary>
        /// Creates a copy of the given RobotInfo, and flips the position, velocity, and orientation
        /// </summary>
        private RobotInfo flipRobotInfo(RobotInfo info)
        {
            return new RobotInfo(-info.Position, -info.Velocity, info.AngularVelocity,
                    info.Orientation + Math.PI, info.Team, info.ID);
        }

        public List<RobotInfo> GetRobots(Team team)
        {
            if(isFlipped)
                return predictor.GetRobots(team).ConvertAll<RobotInfo>(flipRobotInfo);
            return predictor.GetRobots(team);
        }

        public List<RobotInfo> GetRobots() 
        {
            if (isFlipped)
                return predictor.GetRobots().ConvertAll<RobotInfo>(flipRobotInfo);
            return predictor.GetRobots();
        }

        public RobotInfo GetRobot(Team team, int id)
        {
            RobotInfo robot = predictor.GetRobot(team, id);
            if (robot != null && isFlipped)
                return flipRobotInfo(robot);
            return robot;
        }

        public BallInfo GetBall()
        {
            BallInfo ball = predictor.GetBall();
            if (ball != null && isFlipped)
                return new BallInfo(-ball.Position, -ball.Velocity);
            return ball;
        }

        public void SetBallMark() 
        {
            predictor.SetBallMark();
        }

        public void ClearBallMark() 
        {
            predictor.ClearBallMark();
        }

        public bool HasBallMoved() 
        {
            return predictor.HasBallMoved();
        }

        public void SetPlayType(PlayType newPlayType) 
        {
            predictor.SetPlayType(newPlayType);
        }

        public void LoadConstants()
        {
            predictor.LoadConstants();
        }

    }
   
}