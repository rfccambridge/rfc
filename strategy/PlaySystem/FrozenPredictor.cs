using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using Robocup.CoreRobotics;
using Robocup.Geometry;

namespace Robocup.PlaySystem
{
    /// <summary>
    /// This is a wrapper for a predictor that temporarily freezes it in its state. This 
    /// prevents race conditions when, for example, a predictor is referred to multiple
    /// times while running plays
    /// </summary>
    public class FrozenPredictor : IPredictor
    {
        // keep the predictor privately
        IPredictor _predictor;

        // state freezing variables
        List<RobotInfo> robots;
        BallInfo ballinfo;

        /// <summary>
        /// Given a predictor whose state will be occasionally frozen
        /// </summary>
        public FrozenPredictor(IPredictor predictor)
        {
            _predictor = predictor;
            
            // start out frozen
            freezeState();
        }

        /// <summary>
        /// Freeze the predictor in its current state
        /// </summary>
        public void freezeState() {
            robots = _predictor.GetRobots();
            ballinfo = _predictor.GetBall();
        }

        /// <summary>
        /// return a list of all robots
        /// </summary>
        public List<RobotInfo> GetRobots()
        {
            return robots;
        }

        /// <summary>
        /// return all robots on a team
        /// </summary>
        public List<RobotInfo> GetRobots(Team team)
        {
            List<RobotInfo> ret = new List<RobotInfo>();
            foreach (RobotInfo r in robots)
            {
                if (r.Team == team)
                {
                    ret.Add(r);
                }
            }

            return ret;
        }

        /// <summary>
        /// Return a particular robot on a particular team
        /// </summary>
        public RobotInfo GetRobot(Team team, int id)
        {
            RobotInfo robot = robots.Find(new Predicate<RobotInfo>(delegate(RobotInfo r)
            {
                return (r.ID == id && r.Team == team);
            }));

            return robot;
        }

        /// <summary>
        /// returns the ball info if it is seen, and (0,0) if it is not seen.
        /// </summary>
        /// <returns></returns>
        public BallInfo GetBall()
        {
            // returns (0, 0) if there is no ball seen. To my understanding this
            // copies the behavior of the old play system, but this is a good
            // candidate for things to change
            if (ballinfo == null)
            {
                return new BallInfo(new Vector2(0, 0));
            }
            return ballinfo;
        }

        // wrapper methods

        public void LoadConstants()
        {
            _predictor.LoadConstants();
        }

        public void SetPlayType(PlayType newPlayType)
        {
            _predictor.SetPlayType(newPlayType);
        }
    }
}
