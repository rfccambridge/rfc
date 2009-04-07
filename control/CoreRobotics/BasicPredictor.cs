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

            private double DELTA_DIST_SQ_MERGE;
            private double VELOCITY_DT;
            private double VELOCITY_WEIGHT_OLD;
            private double VELOCITY_WEIGHT_NEW;
            private double POSITION_WEIGHT_OLD;
            private double POSITION_WEIGHT_NEW;
            private double MAX_SECONDS_TO_KEEP_INFO;

            public BasicPredictorHelper(bool matchIDs)
            {
                this.matchIDs = matchIDs;
                this.reassignIDs = !matchIDs;

                LoadConstants();
            }
            public void LoadConstants()
            {
                DELTA_DIST_SQ_MERGE = Constants.get<double>("default", "DELTA_DIST_SQ_MERGE");
                VELOCITY_DT = Constants.get<double>("default", "VELOCITY_DT");
                VELOCITY_WEIGHT_OLD = Constants.get<double>("default", "VELOCITY_WEIGHT_OLD");
                VELOCITY_WEIGHT_NEW = Constants.get<double>("default", "VELOCITY_WEIGHT_NEW");
                POSITION_WEIGHT_OLD = Constants.get<double>("default", "POSITION_WEIGHT_OLD");
                POSITION_WEIGHT_NEW = Constants.get<double>("default", "POSITION_WEIGHT_NEW");
                MAX_SECONDS_TO_KEEP_INFO = Constants.get<double>("default", "MAX_SECONDS_TO_KEEP_INFO");
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

            private Dictionary<int, double> lastSeen = new Dictionary<int, double>();

            //private Dictionary<RobotInfo, int> roundsSinceSeen = new Dictionary<RobotInfo, int>();
            private void removeRobot(int id)
            {
                foreach (RobotInfo info in lambdaInfo)
                {
                    if (info.ID == id)
                    {
                        //lastSeen.Remove(info.ID);
                        lambdaInfo.Remove(info);
                        break;
                    }
                }
                foreach (RobotInfo info in omegaInfo)
                {
                    if (info.ID == id)
                    {
                        //lastSeen.Remove(info.ID);
                        omegaInfo.Remove(info);
                        break;
                    }
                }
            }
            public void updateHalfInfos(List<RobotInfo> newInfos, string computerName)
            {
                lock (object_lock)
                {
                    List<RobotInfo> origNewInfos = new List<RobotInfo>();
                    foreach (RobotInfo r in newInfos) {
                        origNewInfos.Add(r);
                    }
                    double time = HighResTimer.SecondsSinceStart();
                    bool isOmega = (computerName == "OMEGA");
                    List<RobotInfo> otherCameraInfos = isOmega ? lambdaInfo : omegaInfo;
                    //here we match all new robots with old robots that are close.
                    //we give them the same id as the old one, then remove the old one
                    //and set the id of the new one to be the old one
                    List<RobotInfo> newInfoList = new List<RobotInfo>();
                    RobotInfo matched;
                    foreach (RobotInfo oldInfo in getMergedInfos())
                    {
                        matched = null;
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
                                if (oldInfo.Position.distanceSq(newInfo.Position) < DELTA_DIST_SQ_MERGE)
                                {
                                    matched = newInfo;
                                    break;
                                }
                            }
                        }
                        if (matched != null)
                        {
                            newInfos.Remove(matched);
                            
                            Vector2 velocity = oldInfo.Velocity;
                            if (matchIDs)
                            {
                                if (lastSeen.ContainsKey(matched.ID))
                                {
                                    double dt = time - lastSeen[matched.ID];
                                    
                                    // Wait until a meaningfully long interval has passed before taking a velocity
                                    // measurement
                                    if (dt > VELOCITY_DT)  // in seconds
                                    {
                                        velocity = (1 / dt) * (matched.Position - oldInfo.Position);    
                                        
                                        velocity = VELOCITY_WEIGHT_NEW * velocity + VELOCITY_WEIGHT_OLD * oldInfo.Velocity;

                                        lastSeen[matched.ID] = time;
                                    }
                                }
                                else
                                {
                                    lastSeen[matched.ID] = time;
                                }
                            }

                            Vector2 position;
                            if (otherCameraInfos.Contains(oldInfo))
                            {
                                position = POSITION_WEIGHT_OLD * oldInfo.Position + POSITION_WEIGHT_NEW * matched.Position;
                            }
                            else
                                position = matched.Position;
                            newInfoList.Add(new RobotInfo(position, velocity, matched.AngularVelocity, matched.Orientation, oldInfo.ID));
                        }
                    }
                    foreach (RobotInfo info in newInfoList)
                    {
                        removeRobot(info.ID);
                    }
                    if (reassignIDs) // for enemies
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
                        if (lastSeen.ContainsKey(oldinfo.ID) && time - lastSeen[oldinfo.ID] >= MAX_SECONDS_TO_KEEP_INFO)
                        {
                            lastSeen.Remove(oldinfo.ID);
                            //don't re-add this
                            continue;
                        }
                        newInfoList.Add(oldinfo);
                        if (!lastSeen.ContainsKey(oldinfo.ID))
                            lastSeen.Add(oldinfo.ID, time);
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

        private double MAX_SECONDS_TO_KEEP_INFO;
        private double VELOCITY_DT;
        private double BALL_POSITION_WEIGHT_OLD;
        private double BALL_POSITION_WEIGHT_NEW;

        public BasicPredictor()
        {
            LoadConstants();
        }
        
        public void LoadConstants()
        {
            MAX_SECONDS_TO_KEEP_INFO = Constants.get<double>("default", "MAX_SECONDS_TO_KEEP_INFO");
            VELOCITY_DT = Constants.get<double>("default", "VELOCITY_DT");
            BALL_POSITION_WEIGHT_OLD = Constants.get<double>("default", "BALL_POSITION_WEIGHT_OLD");
            BALL_POSITION_WEIGHT_NEW = Constants.get<double>("default", "BALL_POSITION_WEIGHT_NEW");
            
            our_helper.LoadConstants();
            their_helper.LoadConstants();
        }


        #region IPredictor Members

        public BallInfo getBallInfo()
        {            
            if (ballInfo == null)
                return new BallInfo(new Vector2(0, 0), new Vector2(0, 0));
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

        public List<RobotInfo> getAllInfos()
        {
            List<RobotInfo> rtn = new List<RobotInfo>();
            rtn.AddRange(our_helper.getMergedInfos());
            rtn.AddRange(their_helper.getMergedInfos());
            return rtn;
        }

        #endregion

        #region IInfoAcceptor Members

        double ballupdate = HighResTimer.SecondsSinceStart();
        public void updateBallInfo(BallInfo ballInfo)
        {
            double now = HighResTimer.SecondsSinceStart();
            double dt = now - ballupdate;

            // Apply the same stickiness rule as for the robots
            if (ballInfo == null) {
                if (dt > MAX_SECONDS_TO_KEEP_INFO) {
                    this.ballInfo = null;                    
                }
                return;
            }
            
            // Wait for a meaningfully long interval to pass before taking a velocity measurement
            if (dt > VELOCITY_DT) {
                if (this.ballInfo != null) {
                    double weightOld = BALL_POSITION_WEIGHT_OLD;
                    double weightNew = BALL_POSITION_WEIGHT_NEW;

                    Vector2 position = weightOld * this.ballInfo.Position + weightNew * ballInfo.Position;
                    Vector2 velocity = (1 / dt) * (ballInfo.Position - this.ballInfo.Position);

                    this.ballInfo = new BallInfo(position, velocity);
                }
                else {
                    this.ballInfo = ballInfo;
                }

                ballupdate = now;
            }
        }

        public void updatePartOurRobotInfo(List<RobotInfo> newInfos, string splitName)
        {
            our_helper.updateHalfInfos(newInfos, splitName);
        }
        public void updatePartTheirRobotInfo(List<RobotInfo> newInfos, string splitName)
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
