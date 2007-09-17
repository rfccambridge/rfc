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
    public class BasicPredictor : IPredictor, ISplitInfoAcceptor
    {
        private class BasicPredictorHelper
        {
            private readonly bool matchIDs, reassignIDs;
            /// <param name="matchIDs">whether or not to match robots based on IDs (if false, does it by positions, and reassigns IDs)</param>
            public BasicPredictorHelper(bool matchIDs)
            {
                this.matchIDs = matchIDs;
                this.reassignIDs = !matchIDs;
            }
            private object object_lock = new object();
            private List<RobotInfo> lambdaInfo = new List<RobotInfo>(), omegaInfo = new List<RobotInfo>();
            public List<RobotInfo> getMergedInfos()
            {
                List<RobotInfo> rtn = new List<RobotInfo>();
                lock (object_lock)
                {
                    rtn.AddRange(lambdaInfo);
                    rtn.AddRange(omegaInfo);
                }
                return rtn;
            }


            private Dictionary<RobotInfo, int> roundsSinceSeen = new Dictionary<RobotInfo, int>();
            private void removeRobot(int id)
            {
                foreach (RobotInfo info in lambdaInfo)
                {
                    if (info.ID == id)
                    {
                        roundsSinceSeen.Remove(info);
                        lambdaInfo.Remove(info);
                        break;
                    }
                }
                foreach (RobotInfo info in omegaInfo)
                {
                    if (info.ID == id)
                    {
                        roundsSinceSeen.Remove(info);
                        omegaInfo.Remove(info);
                        break;
                    }
                }
            }
            public void updateHalfInfos(List<RobotInfo> newInfos, string computerName)
            {
                lock (object_lock)
                {
                    bool isOmega = (computerName == "OMEGA");
                    List<RobotInfo> otherCameraInfos;
                    if (isOmega)
                    {
                        otherCameraInfos = lambdaInfo;
                    }
                    else
                    {
                        otherCameraInfos = omegaInfo;
                    }
                    //here we match all new robots with old robots that are close.
                    //we give them the same id as the old one, then remove the old one
                    //and set the id of the new one to be the old one
                    List<RobotInfo> newInfoList = new List<RobotInfo>();
                    foreach (RobotInfo oldInfo in getMergedInfos())
                    {
                        RobotInfo matched = null;
                        foreach (RobotInfo newInfo in newInfos)
                        {
                            if (matchIDs)
                            {
                                if (oldInfo.ID == newInfo.ID)
                                {
                                    matched = newInfo;
                                    break;
                                }
                            }
                            else
                            {
                                if (oldInfo.Position.distanceSq(newInfo.Position) <
                                    Constants.get<float>("DELTA_DIST_SQ_MERGE"))
                                {
                                    matched = newInfo;
                                    break;
                                }
                            }
                        }
                        if (matched != null)
                        {
                            newInfos.Remove(matched);
                            Vector2 position = matched.Position;
                            if (otherCameraInfos.Contains(oldInfo))
                            {
                                position = .5f * (position + 1f * oldInfo.Position);
                            }
                            newInfoList.Add(new RobotInfo(position, matched.Orientation, oldInfo.ID));
                        }
                    }
                    foreach (RobotInfo info in newInfoList)
                    {
                        removeRobot(info.ID);
                    }
                    if (reassignIDs)
                    {
                        //so at this point there should be no matches between new and old robots, in terms of position
                        //but their ids might clash.  so we get rid of the ids on the new robots that were not matched
                        int curID = 0;
                        while (newInfos.Count > 0)
                        {
                            RobotInfo info = newInfos[0];
                            while (!canUseId(curID, newInfoList))
                            {
                                curID++;
                            }
                            newInfos.Remove(info);
                            newInfoList.Add(new RobotInfo(info.Position, info.Orientation, curID));
                        }
                    }
                    else
                    {
                        newInfoList.AddRange(newInfos);
                        newInfos.Clear();
                    }

                    List<RobotInfo> oldInfos;
                    if (isOmega)
                    {
                        oldInfos = omegaInfo;
                        omegaInfo = newInfoList;
                    }
                    else
                    {
                        oldInfos = lambdaInfo;
                        lambdaInfo = newInfoList;
                    }

                    //at this point any robots in "oldInfos" did not match any new ones
                    foreach (RobotInfo oldinfo in oldInfos)
                    {
                        if (roundsSinceSeen.ContainsKey(oldinfo) && roundsSinceSeen[oldinfo] >=
                            Constants.get<int>("MAX_ROUNDS_TO_KEEP_INFO"))
                        {
                            roundsSinceSeen.Remove(oldinfo);
                            //don't re-add this
                            continue;
                        }
                        newInfoList.Add(oldinfo);
                        if (!roundsSinceSeen.ContainsKey(oldinfo))
                            roundsSinceSeen.Add(oldinfo, 0);
                        roundsSinceSeen[oldinfo]++;
                    }
                }
            }
            /// <summary>
            /// MUST BE USED WITH object_lock LOCKED
            /// </summary>
            private bool canUseId(int id, List<RobotInfo> otherInfos)
            {
                foreach (RobotInfo info in lambdaInfo)
                {
                    if (info.ID == id)
                        return false;
                }
                foreach (RobotInfo info in omegaInfo)
                {
                    if (info.ID == id)
                        return false;
                }
                foreach (RobotInfo info in otherInfos)
                {
                    if (info.ID == id)
                        return false;
                }
                return true;
            }
        }

        protected BallInfo ballInfo = new BallInfo(new Vector2(0, 0));
        private readonly BasicPredictorHelper our_helper = new BasicPredictorHelper(true);
        private readonly BasicPredictorHelper their_helper = new BasicPredictorHelper(false);


        public BasicPredictor()
        {
        }



        #region IPredictor Members

        public BallInfo getBallInfo()
        {
            return ballInfo;
        }

        public RobotInfo getCurrentInformation(int robotID)
        {
            foreach (RobotInfo info in our_helper.getMergedInfos())
            {
                if (robotID == info.ID)
                    return info;
            }
            foreach (RobotInfo info in their_helper.getMergedInfos())
            {
                if (robotID == info.ID)
                    return info;
            }
            throw new ApplicationException("no robot seen with id " + robotID);
        }

        public List<RobotInfo> getOurTeamInfo()
        {
            return our_helper.getMergedInfos();
        }

        public List<RobotInfo> getTheirTeamInfo()
        {
            return their_helper.getMergedInfos();
        }

        #endregion

        #region IInfoAcceptor Members

        public void updateBallInfo(BallInfo ballInfo)
        {
            this.ballInfo = ballInfo;
        }

        public void updateHalfOurRobotInfo(List<RobotInfo> newInfos, string splitName)
        {
            our_helper.updateHalfInfos(newInfos, splitName);
        }
        public void updateHalfTheirRobotInfo(List<RobotInfo> newInfos, string splitName)
        {
            their_helper.updateHalfInfos(newInfos, splitName);
        }

        public void updateRobot(int id, RobotInfo newInfo)
        {
            throw new ApplicationException("this is not implemented");
        }
        #endregion
    }
}
