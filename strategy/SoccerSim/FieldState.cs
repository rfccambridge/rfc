using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Infrastructure;
using RobocupPlays;

namespace SoccerSim
{
    public class FieldState : IPredictor, IInfoAcceptor
    {
        /* this class maintains the state of the simulation; where the robots are, 
         * where the ball is, etc. */

        const int TEAM_SIZE = 5;

        List<RobotInfo> _ourRobots;
        List<RobotInfo> _theirRobots;
        BallInfo _ballInfo;
        
        # region Initialization
        public FieldState()
        {
            RobotInfo[] ourBots = {
                new RobotInfo(new Vector2(-1.0f, -1), 3, 0),
                new RobotInfo(new Vector2(-1.0f, 0), 3, 1),
                new RobotInfo(new Vector2(-1.0f, 1), 3, 2),
                new RobotInfo(new Vector2(-2f, -1), 3, 3),
                new RobotInfo(new Vector2(-2f, 1), 3, 4),
            };

            RobotInfo[] theirBots = {
                new RobotInfo(new Vector2(1.0f, -1), 3, TEAM_SIZE+0),
                new RobotInfo(new Vector2(1.0f, 0), 3, TEAM_SIZE+1),
                new RobotInfo(new Vector2(1.0f, 1), 3, TEAM_SIZE+2),
                new RobotInfo(new Vector2(2f, -1), 3, TEAM_SIZE+3),
                new RobotInfo(new Vector2(2f, 1), 3, TEAM_SIZE+4)
            };
            BallInfo ball = new BallInfo(new Vector2(0, 0), 0, 0);

            init(ourBots, theirBots, ball);
        }

        public FieldState(RobotInfo[] ourBots, RobotInfo[] theirBots, BallInfo ball)
        {
            init(ourBots, theirBots, ball);
        }

        void init(RobotInfo[] ourBots, RobotInfo[] theirBots, BallInfo ball)
        {
            _ourRobots = new List<RobotInfo>();
            foreach (RobotInfo bot in ourBots)
            {
                _ourRobots.Add(bot);
            }
            _theirRobots = new List<RobotInfo>();
            foreach (RobotInfo bot in theirBots)
            {
                _theirRobots.Add(bot);
            }
            _ballInfo = ball;
            
        }

        # endregion

        # region Accessors
        
        private RobotInfo getRobot(int i)
        {
            lock (_ourRobots)
            {
                foreach (RobotInfo robot in _ourRobots)
                {
                    if (robot.ID == i)
                        return robot;
                }
            }
            lock (_theirRobots)
            {
                foreach (RobotInfo robot in _theirRobots)
                {
                    if (robot.ID == i)
                        return robot;
                }
            }
            return null;
        }
       
        # endregion

        

        #region IPredictor Members
        public RobotInfo getCurrentInformation(int i)
        {          
            return getRobot(i);
        }
        public List<RobotInfo> getOurTeamInfo()
        {
            return new List<RobotInfo>(_ourRobots);
        }

        public List<RobotInfo> getTheirTeamInfo()
        {
            return new List<RobotInfo>(_theirRobots);
        }

        public BallInfo getBallInfo()
        {
            return new BallInfo(_ballInfo);
        }

        #endregion

        # region IInfoAcceptor Members
        public void updateRobot(int robotID, RobotInfo newBot)
        {
            int id = robotID;

            for (int i = 0; i < _ourRobots.Count; i++)
            {
                if (_ourRobots[i].ID == id)
                {
                    lock (_ourRobots)
                    {
                        _ourRobots[i] = newBot;
                    }
                    return;
                }
            }

            for (int i = 0; i < _theirRobots.Count; i++)
            {
                if (_theirRobots[i].ID == id)
                {
                    lock (_theirRobots)
                    {
                        _theirRobots[i] = newBot;
                    }
                    return;
                }
            }

        }

        public void updateBallInfo(BallInfo ballInfo)
        {
            lock (_ballInfo)
            {
                _ballInfo = ballInfo;
            }
        }

        # endregion
    }
        

}
