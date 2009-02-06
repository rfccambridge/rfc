using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using Robocup.Utilities;

namespace Robocup.CoreRobotics
{
    /// <summary>
    /// A basic implementation of IPredictor that just remembers the last values that it saw.
    /// </summary>
    public class StaticPredictor : IPredictor, IInfoAcceptor
    {  
        protected BallInfo ballInfo = new BallInfo(new Vector2(0, 0));
        protected List<RobotInfo> ourRobotsInfo = new List<RobotInfo>();
        protected List<RobotInfo> theirRobotsInfo = new List<RobotInfo>();       

        public StaticPredictor()
        {
        }



        #region IPredictor Members

        public BallInfo getBallInfo()
        {
            return ballInfo;
        }

        public RobotInfo getCurrentInformation(int robotID)
        {
            foreach (RobotInfo info in ourRobotsInfo)
            {
                if (robotID == info.ID)
                    return info;
            }
            foreach (RobotInfo info in theirRobotsInfo)
            {
                if (robotID == info.ID)
                    return info;
            }
            throw new ApplicationException("no robot seen with id " + robotID);
        }

        public List<RobotInfo> getOurTeamInfo()
        {
            return ourRobotsInfo;
        }

        public List<RobotInfo> getTheirTeamInfo()
        {
            return theirRobotsInfo;
        }

        public List<RobotInfo> getAllInfos()
        {
            List<RobotInfo> rtn = new List<RobotInfo>();
            rtn.AddRange(ourRobotsInfo);
            rtn.AddRange(ourRobotsInfo);
            return rtn;
        }

        #endregion

        #region IInfoAcceptor Members

        double ballupdate = HighResTimer.SecondsSinceStart();
        public void updateBallInfo(BallInfo ballInfo)
        {
            this.ballInfo = ballInfo;            
        }

        public void updateRobot(int id, RobotInfo newInfo)
        {
            foreach (RobotInfo robotInfo in ourRobotsInfo) {
                if (robotInfo.ID == id) {
                    ourRobotsInfo.Remove(robotInfo);
                }
            }
            ourRobotsInfo.Add(newInfo);
        }
        #endregion
    }
}
