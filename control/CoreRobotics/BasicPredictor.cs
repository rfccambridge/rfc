using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using Robocup.Utilities;
using Robocup.Geometry;

namespace Robocup.CoreRobotics
{


    /// <summary>
    /// A basic implementation of IPredictor that just remembers the last values that it saw.
    /// </summary>
    public class BasicPredictor : IPredictor, ISplitInfoAcceptor, IVisionInfoAcceptor
    {
        private class BasicPredictorHelper
        {
            private readonly bool matchIDs, reassignIDs;
            /// <param name="matchIDs">whether or not to match robots based on IDs (if false, does it by positions, and reassigns IDs)</param>

            private bool BLUE_HAS_PATTERN;
            private bool YELLOW_HAS_PATTERN;

            public BasicPredictorHelper(bool matchIDs)
            {
                this.matchIDs = matchIDs;
                this.reassignIDs = !matchIDs;

                LoadConstants();
            }
            public void LoadConstants()
            {
                BLUE_HAS_PATTERN = ConstantsRaw.get<bool>("configuration", "BLUE_HAS_PATTERN");
                YELLOW_HAS_PATTERN = ConstantsRaw.get<bool>("configuration", "YELLOW_HAS_PATTERN");
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
                double VELOCITY_WEIGHT_OLD = Constants.Predictor.VELOCITY_WEIGHT_OLD;
                double VELOCITY_WEIGHT_NEW = Constants.Predictor.VELOCITY_WEIGHT_NEW;
                double POSITION_WEIGHT_OLD = Constants.Predictor.POSITION_WEIGHT_OLD;
                double POSITION_WEIGHT_NEW = Constants.Predictor.POSITION_WEIGHT_NEW;
                double DELTA_DIST_SQ_MERGE = Constants.Predictor.DELTA_DIST_SQ_MERGE;
                double VELOCITY_DT = Constants.Predictor.VELOCITY_DT;
                double MAX_SECONDS_TO_KEEP_INFO = Constants.Predictor.MAX_SECONDS_TO_KEEP_INFO;

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
                    List<RobotInfo> merged = getMergedInfos();
                    foreach (RobotInfo oldInfo in merged)
                    {
                        matched = null;
                        foreach (RobotInfo newInfo in newInfos)
                        {
                            if (matchIDs)
                            {
                                if (oldInfo.ID == newInfo.ID && 
                                    (((newInfo.Team == Team.Yellow) && YELLOW_HAS_PATTERN) ||
                                     ((newInfo.Team == Team.Blue) && BLUE_HAS_PATTERN)))
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
                            newInfoList.Add(new RobotInfo(position, velocity, matched.AngularVelocity, matched.Orientation, matched.Team, oldInfo.ID));
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
                            newInfoList.Add(new RobotInfo(info.Position, info.Orientation, info.Team, curID));
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

        // Should really be a generalized game state
        private PlayType playType;

        private FieldHalf FIELD_HALF;
        private Team OUR_TEAM;
        
        // Predefined ball positions, when in a refbox gamestate
        private bool ASSUME_BALL;
        private BallInfo BALL_POS_KICKOFF;
        private BallInfo BALL_POS_PENALTY;        
        

        public BasicPredictor()
        {
            LoadConstants();
            playType = PlayType.Stopped;
        }
        
        public void LoadConstants()
        {
            OUR_TEAM = (Team)Enum.Parse(typeof(Team), ConstantsRaw.get<string>("configuration", "OUR_TEAM"), true);
            FIELD_HALF = (FieldHalf)Enum.Parse(typeof(FieldHalf), ConstantsRaw.get<string>("plays", "FIELD_HALF"), true);

            // Predefined locations of the ball  based on the play
            ASSUME_BALL = ConstantsRaw.get<bool>("plays", "ASSUME_BALL");
            BALL_POS_PENALTY = new BallInfo(new Vector2(ConstantsRaw.get<double>("plays", "BALL_POS_PENALTY_X"),
                                                        ConstantsRaw.get<double>("plays", "BALL_POS_PENALTY_Y")));
            BALL_POS_KICKOFF = new BallInfo(new Vector2(ConstantsRaw.get<double>("plays", "BALL_POS_KICKOFF_X"),
                                                        ConstantsRaw.get<double>("plays", "BALL_POS_KICKOFF_Y")));
            
            our_helper.LoadConstants();
            their_helper.LoadConstants();
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

        #region IPredictor Members

        public BallInfo getBallInfo()
        {
            return GetBall();
        }

        public BallInfo GetBall()
        {
            int sign;
            BallInfo ballPos;

            if (ballInfo == null)                
                throw new ApplicationException("BasicPredictor.getBallInfo: internal ball is null!");

            if (ballInfo.Position.X == double.NaN || ballInfo.Position.Y == double.NaN)
                return null;
            
            if (!ASSUME_BALL)
                return ballInfo;

            // Either return a predefined ball position or the one the vision actually
            // sees based on the current game state
            switch (playType) {
                case PlayType.PenaltyKick_Ours:
                case PlayType.PenaltyKick_Ours_Setup:
                    sign = (FIELD_HALF == FieldHalf.Left) ? -1 : 1;
                    ballPos = new BallInfo(new Vector2(sign * BALL_POS_PENALTY.Position.X,
                                                              BALL_POS_PENALTY.Position.Y));
                    return ballPos;                    
                case PlayType.PenaltyKick_Theirs:
                    sign = (FIELD_HALF == FieldHalf.Left) ? 1 : -1;
                    ballPos = new BallInfo(new Vector2(sign * BALL_POS_PENALTY.Position.X,
                                                       BALL_POS_PENALTY.Position.Y));
                    return ballPos;
                case PlayType.KickOff_Ours:
                case PlayType.KickOff_Ours_Setup:
                case PlayType.KickOff_Theirs:
                case PlayType.Kickoff_Theirs_Setup:
                    return BALL_POS_KICKOFF;
                default:
                    return ballInfo;
            }
        }

        public List<RobotInfo> GetRobots(Team team)
        {
            if (team == OUR_TEAM)
            {
                return our_helper.getMergedInfos();
            }
            else
            {
                return their_helper.getMergedInfos();
            }
        }
        public List<RobotInfo> GetRobots() {
            
            List<RobotInfo> ourRobots = our_helper.getMergedInfos();            
            List<RobotInfo> theirRobots =  their_helper.getMergedInfos();

            ourRobots.AddRange(theirRobots);

            return ourRobots;
        }

        public RobotInfo GetRobot(Team team, int id)
        {
            List<RobotInfo> list;
            if (team == OUR_TEAM)
            {
                list = our_helper.getMergedInfos();
            }
            else
            {
                list = their_helper.getMergedInfos();
            }

            RobotInfo robot = list.Find(new Predicate<RobotInfo>(delegate(RobotInfo r)
            {
                return r.ID == id;
            }));

            if (robot == null)
            {
                throw new ApplicationException("BasicPredictor.GetRobot: no robot with id=" + id.ToString() +
                    " found on team " + team.ToString());
            }
            return robot;
        }

        public void SetPlayType(PlayType newPlayType)
        {
            playType = newPlayType;
        }

        #endregion

        #region ISplitInfoAcceptor Members        

        double ballupdate = HighResTimer.SecondsSinceStart();
        public void updateBallInfo(BallInfo ballInfo)
        {
            double MAX_SECONDS_TO_KEEP_INFO = Constants.Predictor.MAX_SECONDS_TO_KEEP_INFO;
            double BALL_POSITION_WEIGHT_OLD = Constants.Predictor.BALL_POSITION_WEIGHT_OLD;
            double BALL_POSITION_WEIGHT_NEW = Constants.Predictor.BALL_POSITION_WEIGHT_NEW;
            double VELOCITY_DT = Constants.Predictor.VELOCITY_DT;

            double now = HighResTimer.SecondsSinceStart();
            double dt = now - ballupdate;

            // Apply the same stickiness rule as for the robots
            if (ballInfo == null) {
                if (dt > MAX_SECONDS_TO_KEEP_INFO) {
                    // Just keep the old location for now.
                    //this.ballInfo = null;                    
                }
                return;
            }
            
            // Wait for a meaningfully long interval to pass before taking a velocity measurement
            if (dt > VELOCITY_DT) {
                if (this.ballInfo != null) {
                    double weightOld = BALL_POSITION_WEIGHT_OLD;
                    double weightNew = BALL_POSITION_WEIGHT_NEW;

                    Vector2 position;
                    if (double.IsNaN(ballInfo.Position.X) || double.IsNaN(ballInfo.Position.Y))
                        position = this.ballInfo.Position;
                    else 
                        position = weightOld * this.ballInfo.Position + weightNew * ballInfo.Position;
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

        #region IVisionInfoAcceptor Members
        public void Update(VisionMessage msg)
        {
            updateBallInfo(msg.Ball);

            List<RobotInfo> ours = new List<RobotInfo>();
            List<RobotInfo> theirs = new List<RobotInfo>();

            foreach (VisionMessage.RobotData robot in msg.Robots)
            {                
                RobotInfo robotInfo = new RobotInfo(robot.Position, robot.Orientation, robot.Team, robot.ID);                
                ((robot.Team == OUR_TEAM) ? ours : theirs).Add(robotInfo);
            }

            string splitName;
            if (msg.CameraID == 0)
            {
                splitName = "omega";
            }
            else
            {
                splitName = "lambda";
            }

            updatePartOurRobotInfo(ours, splitName);
            updatePartTheirRobotInfo(theirs, splitName);
        }
        #endregion
    }
}
