

using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Core;
using Robocup.CoreRobotics;
using Robocup.Geometry;

using System.Drawing;

using Navigation.Examples;
using System.IO;
using Robocup.Utilities;

namespace Robocup.MotionControl
{
#if false
    public class BasicRRTMotionPlanner : IMotionPlanner
    {
        BasicRRTPlanner<Vector2, Vector2Tree> planner;

        public BasicRRTMotionPlanner()
        {
            planner = new BasicRRTPlanner<Vector2, Vector2Tree>(Common.ExtendVV, Common.RandomStateV);
        }


        List<Vector2> lastpath;
        public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius)
        {
            List<Obstacle> obstacles = new List<Obstacle>();
            foreach (RobotInfo info in predictor.getAllInfos())
            {
                if (info.ID != id)
                    //TODO magic number (robot radius)
                    obstacles.Add(new Obstacle(info.Position, .2));
            }
            if (avoidBallRadius > 0 && predictor.getBallInfo().Position != null)
                obstacles.Add(new Obstacle(predictor.getBallInfo().Position, avoidBallRadius));

            RobotInfo curinfo = predictor.getCurrentInformation(id);
            List<Vector2> path = planner.Plan(curinfo.Position, desiredState.Position, obstacles);
            lastpath = path;

            return new MotionPlanningResults(WheelSpeedsExtender.GetWheelSpeedsThrough(curinfo, path[Math.Min(path.Count - 1, 5)]));
        }

        public void DrawLast(System.Drawing.Graphics g, ICoordinateConverter c)
        {
            Common.DrawVector2Tree(planner.LastTree(), Color.Black, g, c);
        }
    }
    public class KinodynamicRRTMotionPlanner : IMotionPlanner
    {
        BasicRRTPlanner<RobotInfo, RobotInfoTree> planner;

        public KinodynamicRRTMotionPlanner()
        {
            planner = new BasicRRTPlanner<RobotInfo, RobotInfoTree>(Common.ExtendRR, Common.RandomStateR);
        }

        List<RobotInfo> lastpath;
        public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius)
        {
            List<Obstacle> obstacles = new List<Obstacle>();
            foreach (RobotInfo info in predictor.getAllInfos())
            {
                if (info.ID != id)
                    //TODO magic number (robot radius)
                    obstacles.Add(new Obstacle(info.Position, .2));
            }
            if (avoidBallRadius > 0 && predictor.getBallInfo().Position != null)
                obstacles.Add(new Obstacle(predictor.getBallInfo().Position, avoidBallRadius));

            RobotInfo curinfo = predictor.getCurrentInformation(id);
            List<RobotInfo> path = planner.Plan(curinfo, desiredState, obstacles);
            lastpath = path;

            return new MotionPlanningResults(WheelSpeedsExtender.GetWheelSpeedsThrough(curinfo, path[Math.Min(path.Count - 1, 5)]));
        }

        public void DrawLast(System.Drawing.Graphics g, ICoordinateConverter c)
        {
            Common.DrawRobotInfoTree(planner.LastTree(), Color.Black, g, c);
        }
    }
    public class Vector2BiRRTMotionPlanner : IMotionPlanner
    {
        public int MaxExtends
        {
            get { return planner.MaxExtends; }
            set { planner.MaxExtends = value; }
        }


        BidirectionalRRTPlanner<Vector2, Vector2, Vector2Tree, Vector2Tree> planner;

        public Vector2BiRRTMotionPlanner()
        {
            planner = new BidirectionalRRTPlanner<Vector2, Vector2, Vector2Tree, Vector2Tree>(
                Common.ExtendVV, Common.ExtendVV, Common.ExtendVV, Common.ExtendVV, Common.RandomStateV, Common.RandomStateV);
        }

        public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius)
        {
            List<Obstacle> obstacles = new List<Obstacle>();
            foreach (RobotInfo info in predictor.getAllInfos())
            {
                if (info.ID != id)
                    //TODO magic number (robot radius)
                    obstacles.Add(new Obstacle(info.Position, .2));
            }
            if (avoidBallRadius > 0 && predictor.getBallInfo().Position != null)
                obstacles.Add(new Obstacle(predictor.getBallInfo().Position, avoidBallRadius));

            RobotInfo curinfo = predictor.getCurrentInformation(id);
            Pair<List<Vector2>, List<Vector2>> path = planner.Plan(curinfo.Position, desiredState.Position, obstacles);

            //return new MotionPlanningResults(new WheelSpeeds());
            //return new MotionPlanningResults(WheelSpeedsExtender.GetWheelSpeeds(curinfo, path.First[Math.Min(path.First.Count - 1, 5)]));
            

            WheelSpeeds rtn;
            if (path.First.Count > 5)
            {
                rtn = WheelSpeedsExtender.GetWheelSpeedsThrough(curinfo, path.First[5]);
                important = path.First[5];
            }
            else if (path.First.Count + path.Second.Count > 5)
            {
                rtn = WheelSpeedsExtender.GetWheelSpeedsThrough(curinfo, path.Second[5 - path.First.Count]);
                important = path.Second[5 - path.First.Count];
            }
            else
            {
                rtn = WheelSpeedsExtender.GetWheelSpeedsTo(curinfo, desiredState);
                important = desiredState.Position;
            }
            return new MotionPlanningResults(rtn);
        }

        Vector2 important = null;

        public void DrawLast(System.Drawing.Graphics g, ICoordinateConverter c)
        {
            Common.DrawVector2Tree(planner.LastTree1(), Color.Blue, g, c);
            Common.DrawVector2Tree(planner.LastTree2(), Color.Green, g, c);

            if (important != null)
            {
                Brush red = new SolidBrush(Color.Red);
                g.FillRectangle(red, c.fieldtopixelX(important.X) - 2, c.fieldtopixelY(important.Y) - 2, 5, 5);
                red.Dispose();
            }
        }
    }
#endif

    /// <summary>
    /// StickyDumbPath follows a straight course to the destination regardless of obstacles
    /// </summary>
    /// 
    public class StickyDumbMotionPlanner : IMotionPlanner {
        const int NUM_ROBOTS = 5;

        // Each robot has a feedback object, a timer, and a path object
        private Feedback[] _feedbackObjs;
        private DateTime[] _timesLastCalled;
        private RobotPath[] _paths;

        private const double MIN_SQ_DIST_TO_WP= 0.0001;// within 1 cm
        private const double MIN_SQ_DIST_TO_DST = 0.0121;// within 11 cm
		private double WAYPOINT_DISTANCE = .05;
        private static int PATH_RECALCULATE_INTERVAL;

        public StickyDumbMotionPlanner() {
            LoadParameters();

            //Store feedback for each robot
            _feedbackObjs = new Feedback[NUM_ROBOTS];
            for (int robotID = 0; robotID < NUM_ROBOTS; robotID++)
                _feedbackObjs[robotID] = new Feedback(robotID);

            //Set empty paths
            _paths = new RobotPath[NUM_ROBOTS];
            for (int robotID = 0; robotID < NUM_ROBOTS; robotID++) {
                _paths[robotID] = new RobotPath();
            }

            //Store time last called
            _timesLastCalled = new DateTime[NUM_ROBOTS];
            for (int robotID = 0; robotID < NUM_ROBOTS; robotID++) {
                //Set to arbitrary time in past- January 1, 2000
                _timesLastCalled[robotID] = new DateTime(2000, 1, 1);
            }
        }

        private void LoadParameters() {
            PATH_RECALCULATE_INTERVAL = Constants.get<int>("default", "PATH_RECALCULATE_INTERVAL");
        }

        /// <summary>
        /// Reloads PID constants from file
        /// </summary>
        public void ReloadConstants() {
            for (int robotID = 0; robotID < NUM_ROBOTS; robotID++)
                _feedbackObjs[robotID].ReloadConstants();
        }

        /// <summary>
        /// Create a linear series of waypoints between origin and destination, given waypoint distance
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        private RobotPath getPath(RobotInfo origin, RobotInfo destination, double extendDistance) {
            List<RobotInfo> retpath = new List<RobotInfo>();

        	int id = origin.ID;
            retpath.Add(origin);

        	Vector2 newposition;
            //double orientation = (destination.Position - origin.Position).cartesianAngle();
            double orientation = destination.Orientation;
            Vector2 singlevector = (destination.Position - origin.Position).normalizeToLength(extendDistance);


            do
            {
                newposition = retpath[retpath.Count - 1].Position + singlevector;

                // Hm... system ran out of memory at this line:    
                retpath.Add(new RobotInfo(newposition, orientation, id));
            } while (newposition.distanceSq(destination.Position) > extendDistance * extendDistance);

        	retpath.Add(destination);

            return new RobotPath(retpath);
        }

        public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius) {
            Console.WriteLine("desired Location: " + desiredState.Position.ToString());
            
            List<Obstacle> obstacles = new List<Obstacle>();
            foreach (RobotInfo info in predictor.getAllInfos()) {
                if (info.ID != id)
                    //TODO magic number (robot radius)
                    obstacles.Add(new Obstacle(info.Position, .2));
            }
            //TODO goal hack
            if (!TagSystem.GetTags(id).Contains("goalie")) {
                obstacles.Add(new Obstacle(new Vector2(Constants.get<double>("plays", "FIELD_WIDTH") / 2, 0), .7 + .1));
                obstacles.Add(new Obstacle(new Vector2(-Constants.get<double>("plays", "FIELD_WIDTH") / 2, 0), .7 + .1));
            }

            RobotInfo curinfo;
            try {
                curinfo = predictor.getCurrentInformation(id);
            } catch (ApplicationException e) {
                throw e;
            }

            foreach (Obstacle o in obstacles) {
                if (curinfo.Position.distanceSq(o.position) < o.size * o.size) {
                    o.size = .9 * Math.Sqrt(curinfo.Position.distanceSq(o.position));
                }
            }
			
            //Check whether there has been enough time since last refresh
            DateTime nowCached = DateTime.Now;
            if ((nowCached - _timesLastCalled[id]).TotalMilliseconds > PATH_RECALCULATE_INTERVAL) {
                _paths[id] = getPath(curinfo, desiredState, WAYPOINT_DISTANCE);
                _timesLastCalled[id] = nowCached;
            }

            if (_paths[id] == null)
                throw new Exception("Something wrong with StickyDumbPath! No path calculated!!!");

        	RobotInfo nextWaypoint;
			int nextWaypointIndex = _paths[id].findNearestWaypointIndex(curinfo);
            if (nextWaypointIndex != _paths[id].Waypoints.Count - 1)
                nextWaypointIndex = nextWaypointIndex + 1;
            nextWaypoint = _paths[id].getWaypoint(nextWaypointIndex);

            Console.WriteLine(_paths[id].Waypoints.Count.ToString());


            double wpDistanceSq = curinfo.Position.distanceSq(nextWaypoint.Position);

            if (nextWaypointIndex >= _paths[id].Waypoints.Count - 1 && wpDistanceSq <= MIN_SQ_DIST_TO_DST) {
                Console.WriteLine("Close enough to point, stopping now.");
                return new MotionPlanningResults(new WheelSpeeds(), nextWaypoint);
            }
            
            if (wpDistanceSq > MIN_SQ_DIST_TO_WP) {
                WheelSpeeds wheelSpeeds = _feedbackObjs[id].ComputeWheelSpeeds(curinfo, nextWaypoint);
                return new MotionPlanningResults(wheelSpeeds, nextWaypoint);
            }
            else {

                Console.WriteLine("Close enough to point, stopping now.");
                return new MotionPlanningResults(new WheelSpeeds(), nextWaypoint);
            }

            //WheelSpeeds wheelSpeeds = _feedbackObjs[id].computeWheelSpeeds(curinfo, nextWayPoint);
            //return new MotionPlanningResults(wheelSpeeds, nextWayPoint);

        }

        public void DrawLast(System.Drawing.Graphics g, ICoordinateConverter c) {
            if (_paths[0] == null)
                return;
            if (_paths[0].Waypoints == null)
                return;

            List<Vector2> emptylist = new List<Vector2>();
            PathDrawing.DrawPath(new Pair<List<RobotInfo>, List<Vector2>>(_paths[0].Waypoints, emptylist), Color.Blue, Color.Green, g, c);
        }

        //reload all necessary constants from files, for now just PID reload
        public void reloadConstants() {
            for (int robotID = 0; robotID < NUM_ROBOTS; robotID++)
                _feedbackObjs[robotID].ReloadConstants();
        }
    }

    public class MixedBiRRTMotionPlanner : IMotionPlanner
    {
        public int MaxExtends
        {
            get { return planner.MaxExtends; }
            set { planner.MaxExtends = value; }
        }

        public void ReloadConstants() {}

        BidirectionalRRTPlanner<RobotInfo, Vector2, RobotInfoTree, Vector2Tree> planner;

        public MixedBiRRTMotionPlanner()
        {           
            planner = new BidirectionalRRTPlanner<RobotInfo, Vector2, RobotInfoTree, Vector2Tree>(
                Common.ExtendRRThrough, Common.ExtendRVThrough, Common.ExtendVR, Common.ExtendVV, Common.RandomStateR, Common.RandomStateV);       
        }

        public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius)
        {
            List<Obstacle> obstacles = new List<Obstacle>();
            foreach (RobotInfo info in predictor.getAllInfos())
            {
                if (info.ID != id)
                    //TODO magic number (robot radius)
                    obstacles.Add(new Obstacle(info.Position, .2));
            }
            //TODO goal hack
            if (!TagSystem.GetTags(id).Contains("goalie"))
            {
                obstacles.Add(new Obstacle(new Vector2(Constants.get<double>("plays", "FIELD_WIDTH") / 2, 0), .7 + .1));
                obstacles.Add(new Obstacle(new Vector2(-Constants.get<double>("plays", "FIELD_WIDTH") / 2, 0), .7 + .1));
            }

            RobotInfo curinfo = predictor.getCurrentInformation(id);
            foreach (Obstacle o in obstacles)
            {
                if (curinfo.Position.distanceSq(o.position) < o.size * o.size)
                {
                    o.size = .9 * Math.Sqrt(curinfo.Position.distanceSq(o.position));
                }
            }

            Pair<List<RobotInfo>, List<Vector2>> path = planner.Plan(curinfo, desiredState.Position, obstacles);

            //return new MotionPlanningResults(new WheelSpeeds());
            WheelSpeeds rtn;
            if (desiredState.Position.distanceSq(curinfo.Position) < .15 * .15)
                rtn = WheelSpeedsExtender.GetWheelSpeedsTo(curinfo, desiredState);
            else if (path.First.Count + path.Second.Count < 5)
                rtn = WheelSpeedsExtender.GetWheelSpeedsTo(curinfo, desiredState);
            else if (path.First.Count > 5)
                rtn = WheelSpeedsExtender.GetWheelSpeedsTo(curinfo, path.First[5]);
            else
                rtn = WheelSpeedsExtender.GetWheelSpeedsTo(curinfo, desiredState);
            
            return new MotionPlanningResults(Common.addOrientation(curinfo.Orientation, desiredState.Orientation,
                rtn));

        }

        public void DrawLast(System.Drawing.Graphics g, ICoordinateConverter c)
        {
            Common.DrawRobotInfoTree(planner.LastTree1(), Color.Blue, g, c);
            Common.DrawVector2Tree(planner.LastTree2(), Color.Green, g, c);
        }
    }
    public class CircleFeedbackMotionPlanner : IMotionPlanner, ILogger {
        //the index of the next waypoint the robots going to try and go to and an associated robotinfo
        int nextWaypointIndex = 0;
        RobotInfo nextWayPoint;


        // Each robot has a feedback object
        private Feedback[] _feedbackObjs;
            
        private CirclePlanner _planner;
        //private BidirectionalRRTPlanner<RobotInfo, Vector2, RobotInfoTree, Vector2Tree> _planner;

        const int NUM_ROBOTS = 5;

        private const double MIN_SQ_DIST_TO_WP = 0.0001;// within 1 cm
        private const double MIN_ANGLE_DIFF_TO_WP = 0.01;
        private int LOG_EVERY_MSEC;

    

        public CircleFeedbackMotionPlanner() {
            

            //replaced with static testing planner
            _planner = new CirclePlanner();
            //_planner = new BidirectionalRRTPlanner<RobotInfo,Vector2, RobotInfoTree,Vector2Tree>(
            //    Common.ExtendRRThrough, Common.ExtendRVThrough, Common.ExtendVR, Common.ExtendVV, Common.RandomStateR, Common.RandomStateV);

            _feedbackObjs = new Feedback[NUM_ROBOTS];
            for (int robotID = 0; robotID < NUM_ROBOTS; robotID++)
                _feedbackObjs[robotID] = new Feedback(robotID);

            ReloadConstants();
        }
        
         
        /// <summary>
        /// !! Implementation only valid for testing purposes because ignores the 
        /// Vector2 part of the path (the one that grows from the destination). Only compatible
        /// with CircleMotionPlanner.
        /// 
        /// </summary>
        /// <param name="currInfo"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public RobotInfo findNearestWaypoint(RobotInfo currInfo, Pair<List<RobotInfo>, List<Vector2>> path) {
            // For now, brute force search.

            RobotInfo closestWaypoint = path.First[0];
            double minDistSq = double.MaxValue;

            for (int i = 0; i < path.First.Count; i++) {
                RobotInfo waypoint = path.First[i];
                double distSq = waypoint.Position.distanceSq(currInfo.Position);
                if (distSq < minDistSq) {
                    closestWaypoint = waypoint;
                    minDistSq = distSq;
                }
            }

            return closestWaypoint;
        }

        public int findNearestWaypointIndex(RobotInfo currInfo, Pair<List<RobotInfo>, List<Vector2>> path) {

            // For now, brute force search.

            int closestWaypointIndex = 0;
            double minDistSq = double.MaxValue;

            for (int i = 0; i < path.First.Count; i++) {
                RobotInfo waypoint = path.First[i];
                double distSq = waypoint.Position.distanceSq(currInfo.Position);
                if (distSq < minDistSq) {
                    closestWaypointIndex = i;
                    minDistSq = distSq;
                }
            }
            return closestWaypointIndex;
        }

        public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius) {
            List<Object> itemsToLog = new List<Object>();
            
            List<Obstacle> obstacles = new List<Obstacle>();
            foreach (RobotInfo info in predictor.getAllInfos()) {
                if (info.ID != id)
                    //TODO magic number (robot radius)
                    obstacles.Add(new Obstacle(info.Position, .2));
            }
            //TODO goal hack
            if (!TagSystem.GetTags(id).Contains("goalie")) {
                obstacles.Add(new Obstacle(new Vector2(Constants.get<double>("plays", "FIELD_WIDTH") / 2, 0), .7 + .1));
                obstacles.Add(new Obstacle(new Vector2(-Constants.get<double>("plays", "FIELD_WIDTH") / 2, 0), .7 + .1));
            }

            RobotInfo curinfo;
            try {
                 curinfo = predictor.getCurrentInformation(id);
            } catch (ApplicationException e) {
                throw e;
            }
            
            foreach (Obstacle o in obstacles) {
                if (curinfo.Position.distanceSq(o.position) < o.size * o.size) {
                    o.size = .9 * Math.Sqrt(curinfo.Position.distanceSq(o.position));
                }
            }

            Pair<List<RobotInfo>, List<Vector2>> path = _planner.Plan(curinfo, desiredState, obstacles);
            //Pair<List<RobotInfo>, List<Vector2>> path = _planner.Plan(curinfo, desiredState.Position, obstacles);
                     
            ///instead of going to nearest going to try more of a carrot on a stick approach and go to the next one.
            ///  RobotInfo nearestWayPoint = findNearestWaypoint(curinfo, path);
            ///WheelSpeeds wheelSpeeds = _feedbackObjs[id].computeWheelSpeeds(curinfo, nearestWayPoint);

            nextWaypointIndex = findNearestWaypointIndex(curinfo, path);
            if (nextWaypointIndex!=path.First.Count-1)
                nextWaypointIndex = nextWaypointIndex+1;
            nextWayPoint = path.First[nextWaypointIndex];

            // Logging
            itemsToLog.Add(DateTime.Now);
            itemsToLog.Add(curinfo);
            itemsToLog.Add(desiredState);
            itemsToLog.Add(nextWayPoint);            
            
            double wpDistanceSq = curinfo.Position.distanceSq(nextWayPoint.Position);
            double angleDiff = Math.Abs(UsefulFunctions.angleDifference(curinfo.Orientation, nextWayPoint.Orientation));

            WheelSpeeds wheelSpeeds;

            if (wpDistanceSq > MIN_SQ_DIST_TO_WP||angleDiff>MIN_ANGLE_DIFF_TO_WP) {
                wheelSpeeds = _feedbackObjs[id].ComputeWheelSpeeds(curinfo, nextWayPoint);                
            } else {                
                Console.WriteLine("Close enough to point, stopping now.");
                wheelSpeeds = new WheelSpeeds();                
            }

            itemsToLog.Add(wheelSpeeds);

            MotionPlanningResults results = new MotionPlanningResults(wheelSpeeds, nextWayPoint);
            
            RobotPath robotPath = new RobotPath(path.First);
            itemsToLog.Add(robotPath);

            DateTime now = DateTime.Now;
            TimeSpan timeSinceLastLog = now.Subtract(_lastLogEntry);
            if (_logging && timeSinceLastLog.TotalMilliseconds > 500)
            {
                _logWriter.LogItems(itemsToLog);
                _lastLogEntry = now;
            }

            //WheelSpeeds wheelSpeeds = _feedbackObjs[id].computeWheelSpeeds(curinfo, nextWayPoint);
            //return new MotionPlanningResults(wheelSpeeds, nextWayPoint);

            return results;
        }

        public void DrawLast(System.Drawing.Graphics g, ICoordinateConverter c) {
            //Common.DrawRobotInfoTree(_planner.LastTree1(), Color.Blue, g, c);
            //Common.DrawVector2Tree(_planner.LastTree2(), Color.Green, g, c);
            PathDrawing.DrawPath(_planner.LastPath, Color.Blue, Color.Green, g, c);
        }

        /// <summary>
        /// Reloads PID constants from file
        /// </summary>
        public void ReloadConstants() {
            for (int robotID = 0; robotID < NUM_ROBOTS; robotID++)
                _feedbackObjs[robotID].ReloadConstants();

            LOG_EVERY_MSEC = Constants.get<int>("control", "LOG_EVERY_MSEC");
        }

        #region ILogger

        private string _logFile = null;
        private bool _logging = false;
        private DateTime _lastLogEntry;
        private LogWriter _logWriter = new LogWriter();

        public string LogFile
        {
            get { return _logFile; }
            set { _logFile = value; }
        }

        public bool Logging
        {
            get
            {
                return _logging;
            }
        }

        public void StartLogging()
        {
            if (_logging)
                return;

            if (_logFile == null)
            {
                throw new ApplicationException("Logger: must set LogFile before calling start");
            }

            _logWriter.OpenLogFile(_logFile);
            _logging = true;
        }

        public void StopLogging()
        {
            if (!_logging)
                return;

            _logWriter.CloseLogFile();
            _logging = false;
        }
        #endregion        
    }

    public class StickyRRTFeedbackMotionPlanner : IMotionPlanner {
        //the index of the next waypoint the robots going to try and go to and an associated robotinfo
        int nextWaypointIndex = 0;
        RobotInfo nextWayPoint;

        List<RobotInfo> fullpath;

        // Each robot has a feedback object
        private Feedback[] _feedbackObjs;
        private DateTime[] _timesLastCalled;

        //private BidirectionalRRTPlanner<RobotInfo, Vector2, RobotInfoTree, Vector2Tree> _planner;
        private BidirectionalRRTPlanner<RobotInfo, RobotInfo, RobotInfoTree, RobotInfoTree> _planner;
        //private Pair<List<RobotInfo>, List<Vector2>> _path;
        private Pair<List<RobotInfo>, List<RobotInfo>> _path;

        const int NUM_ROBOTS = 5;

        private const double MIN_SQ_DIST_TO_WP = 0.0001;// within 1 cm
        private static int PATH_RECALCULATE_INTERVAL;

        public StickyRRTFeedbackMotionPlanner() {
            
            
            LoadParameters();
            
            //replaced with static testing planner
            _planner = new BidirectionalRRTPlanner<RobotInfo,RobotInfo, RobotInfoTree,RobotInfoTree>(
                Common.ExtendRRThrough, Common.ExtendRRThrough, Common.ExtendRRThrough, Common.ExtendRRThrough, Common.RandomStateR, Common.RandomStateR);

            //Store feedback for each robot
            _feedbackObjs = new Feedback[NUM_ROBOTS];
            for (int robotID = 0; robotID < NUM_ROBOTS; robotID++)
                _feedbackObjs[robotID] = new Feedback(robotID);

            //Store time last called
            _timesLastCalled = new DateTime[NUM_ROBOTS];
            for (int robotID = 0; robotID < NUM_ROBOTS; robotID++) {
                //Set to arbitrary time in past- January 1, 2000
                _timesLastCalled[robotID] = new DateTime(2000, 1, 1);
            }
            
        }

        private void LoadParameters() {
            PATH_RECALCULATE_INTERVAL = Constants.get<int>("default", "PATH_RECALCULATE_INTERVAL");
        }
        
        /// <summary>
        /// Reloads PID constants from file
        /// </summary>
        public void ReloadConstants() {
            for (int robotID = 0; robotID < NUM_ROBOTS; robotID++)
                _feedbackObjs[robotID].ReloadConstants();
        }

        /// <summary>
        /// !! Implementation only valid for testing purposes because ignores the 
        /// Vector2 part of the path (the one that grows from the destination). Only compatible
        /// with CircleMotionPlanner.
        /// 
        /// </summary>
        /// <param name="currInfo"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private RobotInfo findNearestWaypoint(RobotInfo currInfo, Pair<List<RobotInfo>, List<Vector2>> path) {
            // For now, brute force search.

            RobotInfo closestWaypoint = path.First[0];
            double minDistSq = double.MaxValue;

            for (int i = 0; i < path.First.Count; i++) {
                RobotInfo waypoint = path.First[i];
                double distSq = waypoint.Position.distanceSq(currInfo.Position);
                if (distSq < minDistSq) {
                    closestWaypoint = waypoint;
                    minDistSq = distSq;
                }
            }

            return closestWaypoint;
        }

        private int findNearestWaypointIndex(RobotInfo currInfo, Pair<List<RobotInfo>, List<Vector2>> path) {

            // For now, brute force search.

            int closestWaypointIndex = 0;
            double minDistSq = double.MaxValue;

            for (int i = 0; i < path.First.Count; i++) {
                RobotInfo waypoint = path.First[i];
                double distSq = waypoint.Position.distanceSq(currInfo.Position);
                if (distSq < minDistSq) {
                    closestWaypointIndex = i;
                    minDistSq = distSq;
                }
            }
            return closestWaypointIndex;
        }

        /// <summary>
        /// Use private variable fullpath to find nearest waypoint in path from start to end
        /// </summary>
        /// <param name="currInfo"></param>
        /// <returns></returns>
        private int findNearestWaypointIndex(RobotInfo currInfo) {

            // For now, brute force search.

            int closestWaypointIndex = 0;
            double minDistSq = double.MaxValue;

            for (int i = 0; i < fullpath.Count; i++) {
                RobotInfo waypoint = fullpath[i];
                double distSq = waypoint.Position.distanceSq(currInfo.Position);
                if (distSq < minDistSq) {
                    closestWaypointIndex = i;
                    minDistSq = distSq;
                }
            }
            return closestWaypointIndex;
        }

        public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius) {
            List<Obstacle> obstacles = new List<Obstacle>();
            foreach (RobotInfo info in predictor.getAllInfos()) {
                if (info.ID != id)
                    //TODO magic number (robot radius)
                    obstacles.Add(new Obstacle(info.Position, .2));
            }
            //TODO goal hack
            if (!TagSystem.GetTags(id).Contains("goalie")) {
                obstacles.Add(new Obstacle(new Vector2(Constants.get<double>("plays", "FIELD_WIDTH") / 2, 0), .7 + .1));
                obstacles.Add(new Obstacle(new Vector2(-Constants.get<double>("plays", "FIELD_WIDTH") / 2, 0), .7 + .1));
            }

            RobotInfo curinfo;
            try {
                curinfo = predictor.getCurrentInformation(id);
            } catch (ApplicationException e) {
                throw e;
            }

            foreach (Obstacle o in obstacles) {
                if (curinfo.Position.distanceSq(o.position) < o.size * o.size) {
                    o.size = .9 * Math.Sqrt(curinfo.Position.distanceSq(o.position));
                }
            }

            //Check whether there has been enough time since last refresh
            DateTime nowCached = DateTime.Now;
            if ((nowCached - _timesLastCalled[id]).TotalMilliseconds > PATH_RECALCULATE_INTERVAL) {
                _path = _planner.Plan(curinfo, desiredState, obstacles);
                //Combine first and second
                fullpath = _path.First;
                fullpath.AddRange(_path.Second);
                _timesLastCalled[id] = nowCached;
            }

            if (_path == null)
                throw new Exception("Something wrong with stickyPlanner! No path calculated!!!");

            ///instead of going to nearest going to try more of a carrot on a stick approach and go to the next one.
            ///  RobotInfo nearestWayPoint = findNearestWaypoint(curinfo, path);
            ///WheelSpeeds wheelSpeeds = _feedbackObjs[id].computeWheelSpeeds(curinfo, nearestWayPoint);

            /*nextWaypointIndex = findNearestWaypointIndex(curinfo, _path);
            if (nextWaypointIndex != _path.First.Count - 1)
               nextWaypointIndex = nextWaypointIndex + 1;
            nextWayPoint = _path.First[nextWaypointIndex];*/

            nextWaypointIndex = findNearestWaypointIndex(curinfo);
            if (nextWaypointIndex != fullpath.Count - 1)
                nextWaypointIndex = nextWaypointIndex + 1;
            nextWayPoint = fullpath[nextWaypointIndex];


            double wpDistanceSq = curinfo.Position.distanceSq(nextWayPoint.Position);

            if (wpDistanceSq > MIN_SQ_DIST_TO_WP) {
                WheelSpeeds wheelSpeeds = _feedbackObjs[id].ComputeWheelSpeeds(curinfo, nextWayPoint);
                return new MotionPlanningResults(wheelSpeeds, nextWayPoint);
            } else {

                Console.WriteLine("Close enough to point, stopping now.");
                return new MotionPlanningResults(new WheelSpeeds(), nextWayPoint);
            }

            //WheelSpeeds wheelSpeeds = _feedbackObjs[id].computeWheelSpeeds(curinfo, nextWayPoint);
            //return new MotionPlanningResults(wheelSpeeds, nextWayPoint);

        }

        public void DrawLast(System.Drawing.Graphics g, ICoordinateConverter c) {
            if (_path == null)
                return;
            List<Vector2> emptylist = new List<Vector2>();
            PathDrawing.DrawPath(new Pair<List<RobotInfo>, List<Vector2>>(fullpath, emptylist), Color.Blue, Color.Green, g, c);
            //Common.DrawRobotInfoTree(_planner.LastTree1(), Color.Blue, g, c);
            //Common.DrawRobotInfoTree(_planner.LastTree2(), Color.Green, g, c);
            //Common.DrawPath(_planner.LastPath, Color.Blue, g, c);
        }

        //reload all necessary constants from files, for now just PID reload
        public void reloadConstants() {
            for (int robotID = 0; robotID < NUM_ROBOTS; robotID++)
                _feedbackObjs[robotID].ReloadConstants();
        }
    }

    public class SmoothVector2BiRRTMotionPlanner : IMotionPlanner
    {

        public void ReloadConstants() { }

        // keep track of waypoints to draw
        private List<Vector2> waypointslist = new List<Vector2>();

        public int MaxExtends
        {
            get { return planner.MaxExtends; }
            set { planner.MaxExtends = value; }
        }

        BidirectionalRRTPlanner<Vector2, Vector2, Vector2Tree, Vector2Tree> planner;

        public SmoothVector2BiRRTMotionPlanner()
        {
            planner = new BidirectionalRRTPlanner<Vector2, Vector2, Vector2Tree, Vector2Tree>(
                Common.ExtendVV, Common.ExtendVV, Common.ExtendVV, Common.ExtendVV, Common.RandomStateV, Common.RandomStateV);
        }

        public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius)
        {
            List<Obstacle> obstacles = new List<Obstacle>();
            foreach (RobotInfo info in predictor.getAllInfos())
            {
                if (info.ID != id)
                    //TODO magic number (robot radius)
                    obstacles.Add(new Obstacle(info.Position, .28));
            }
            if (avoidBallRadius > 0 && predictor.getBallInfo().Position != null)
                obstacles.Add(new Obstacle(predictor.getBallInfo().Position, avoidBallRadius));
            //TODO goal hack
            if (!TagSystem.GetTags(id).Contains("goalie"))
            {
                obstacles.Add(new Obstacle(new Vector2(Constants.get<double>("plays", "FIELD_WIDTH") / 2, 0), .7 + .1));
                obstacles.Add(new Obstacle(new Vector2(-Constants.get<double>("plays", "FIELD_WIDTH") / 2, 0), .7 + .1));
            }

            RobotInfo curinfo = predictor.getCurrentInformation(id);
            foreach (Obstacle o in obstacles)
            {
                if (curinfo.Position.distanceSq(o.position) < o.size * o.size)
                {
                    o.size = .9 * Math.Sqrt(curinfo.Position.distanceSq(o.position));
                }
            }

            Pair<List<Vector2>, List<Vector2>> path = planner.Plan(curinfo.Position, desiredState.Position, obstacles);
            
            List<Vector2> waypoints = path.First;
            waypoints.AddRange(path.Second);

            waypointslist = waypoints;

            return Smoother.Smooth(curinfo, desiredState, waypoints, obstacles);
        }

        Vector2 important = null;

        public void DrawLast(System.Drawing.Graphics g, ICoordinateConverter c)
        {
            Common.DrawVector2Tree(planner.LastTree1(), Color.Blue, g, c);
            Common.DrawVector2Tree(planner.LastTree2(), Color.Green, g, c);

            //Console.WriteLine(waypointslist.Count.ToString());

            Brush blue = new SolidBrush(Color.Blue);

            for (int i = 0; i < waypointslist.Count; i++) {
                //g.FillRectangle(blue, c.fieldtopixelX(waypointslist[i].X) - 2, c.fieldtopixelY(waypointslist[i].Y) - 2, 2, 2);
            }
            blue.Dispose();

            if (important != null)
            {
                Brush red = new SolidBrush(Color.Red);
                g.FillRectangle(red, c.fieldtopixelX(important.X) - 2, c.fieldtopixelY(important.Y) - 2, 5, 5);
                red.Dispose();
            }

            Smoother.DrawLast(g, c);
        }
    }

    //NOTE: BugFeedbackMotionPlanner, below, is now defined in NewPlanners.cs. It is seperated into
    //its components, a planner and a driver, but should act exactly identically. You can
    //edit the components, which are BugNavigatorPlanner to plan the path and PositionFeedbackDriver
    //to follow that waypoint. You can also comment out that planner and uncomment this one if you must.

#if false
        public class BugFeedbackMotionPlanner : IMotionPlanner, Robocup.Core.ILogger {       

        // Each robot has a feedback object
        private Feedback[] _feedbackObjs;
        public Feedback GetFeedbackObj(int robotID) { return _feedbackObjs[robotID]; }
        private NavigationResults results;

        private DateTime[] _timesLastCalled = new DateTime[5]; //number of robots

        private Pair<List<RobotInfo>, List<Vector2>>[] paths;        

        //private NavigationPlanner _planner;
        BugNavigator _navigator;

        const int NUM_ROBOTS = 5;
        
        private static int PATH_RECALCULATE_INTERVAL = 1;
        private const double MIN_SQ_DIST_TO_WP = 0.0001;// within 1 cm
        private const double MIN_ANGLE_DIFF_TO_WP = 0.01;
        private int LOG_EVERY_MSEC;
      
        public BugFeedbackMotionPlanner() {


            //replaced with static testing planner
            //_planner = new CirclePlanner();
            //_planner = new BidirectionalRRTPlanner<RobotInfo,Vector2, RobotInfoTree,Vector2Tree>(
            //    Common.ExtendRRThrough, Common.ExtendRVThrough, Common.ExtendVR, Common.ExtendVV, Common.RandomStateR, Common.RandomStateV);

            _navigator = new BugNavigator();
            // _planner = new NavigationPlanner(_navigator);


            _feedbackObjs = new Feedback[NUM_ROBOTS];
            paths = new Pair<List<RobotInfo>, List<Vector2>>[NUM_ROBOTS];
            for (int robotID = 0; robotID < NUM_ROBOTS; robotID++)
                _feedbackObjs[robotID] = new Feedback(robotID);

            ReloadConstants();                     

            for (int robotID = 0; robotID < NUM_ROBOTS; robotID++) {
                //Set to arbitrary time in past- January 1, 2000
                _timesLastCalled[robotID] = new DateTime(2000, 1, 1);
            }

        }

        /// <summary>
        /// !! Implementation only valid for testing purposes because ignores the 
        /// Vector2 part of the path (the one that grows from the destination). Only compatible
        /// with CircleMotionPlanner.
        /// 
        /// </summary>
        /// <param name="currInfo"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public RobotInfo findNearestWaypoint(RobotInfo currInfo, Pair<List<RobotInfo>, List<Vector2>> path) {
            // For now, brute force search.

            RobotInfo closestWaypoint = path.First[0];
            double minDistSq = double.MaxValue;

            for (int i = 0; i < path.First.Count; i++) {
                RobotInfo waypoint = path.First[i];
                double distSq = waypoint.Position.distanceSq(currInfo.Position);
                if (distSq < minDistSq) {
                    closestWaypoint = waypoint;
                    minDistSq = distSq;
                }
            }

            return closestWaypoint;
        }

        public int findNearestWaypointIndex(RobotInfo currInfo, Pair<List<RobotInfo>, List<Vector2>> path) {

            // For now, brute force search.

            int closestWaypointIndex = 0;
            double minDistSq = double.MaxValue;

            for (int i = 0; i < path.First.Count; i++) {
                RobotInfo waypoint = path.First[i];
                double distSq = waypoint.Position.distanceSq(currInfo.Position);
                if (distSq < minDistSq) {
                    closestWaypointIndex = i;
                    minDistSq = distSq;
                }
            }
            return closestWaypointIndex;
        }

        public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius) {
            //Console.WriteLine("desired Location: " + desiredState.Position.ToString());

            List<Object> itemsToLog = new List<Object>();

            List<Obstacle> obstacles = new List<Obstacle>();
            foreach (RobotInfo info in predictor.getAllInfos()) {
                if (info.ID != id)
                    //TODO magic number (robot radius)
                    obstacles.Add(new Obstacle(info.Position, .2));
            }
            //TODO goal hack
            if (!TagSystem.GetTags(id).Contains("goalie")) {
                obstacles.Add(new Obstacle(new Vector2(Constants.get<double>("plays", "FIELD_WIDTH") / 2, 0), .7 + .1));
                obstacles.Add(new Obstacle(new Vector2(-Constants.get<double>("plays", "FIELD_WIDTH") / 2, 0), .7 + .1));
            }

            RobotInfo curinfo;
            try {
                curinfo = predictor.getCurrentInformation(id);
            } catch (ApplicationException e) {
                throw e;
            }

            foreach (Obstacle o in obstacles) {
                if (curinfo.Position.distanceSq(o.position) < o.size * o.size) {
                    o.size = .9 * Math.Sqrt(curinfo.Position.distanceSq(o.position));
                }
            }

            //Pair<List<RobotInfo>, List<Vector2>> path = _planner.Plan(curinfo, desiredState, obstacles);
            //Pair<List<RobotInfo>, List<Vector2>> path = _planner.Plan(curinfo, desiredState.Position, obstacles);
            //path = _planner.Plan(curinfo, desiredState, obstacles, predictor);

            //Check whether there has been enough time since last refresh
            DateTime nowCached = DateTime.Now;
            if ((nowCached - _timesLastCalled[id]).TotalMilliseconds > PATH_RECALCULATE_INTERVAL) {
                results = _navigator.navigate(curinfo.ID, curinfo.Position,
                desiredState.Position, predictor.getOurTeamInfo().ToArray(), predictor.getTheirTeamInfo().ToArray(), predictor.getBallInfo(),
                0.15);
                _timesLastCalled[id] = nowCached;
            }

            if (results == null)
                throw new Exception("Something wrong with BugFeedback! No path calculated!!!");


            //NavigationResults results = _navigator.navigate(curinfo.ID, curinfo.Position,
            //    desiredState.Position, predictor.getOurTeamInfo().ToArray(), predictor.getTheirTeamInfo().ToArray(), predictor.getBallInfo(),
            //    0);
            //results.waypoint

            itemsToLog.Add(DateTime.Now);
            itemsToLog.Add(curinfo);
            itemsToLog.Add(desiredState);
            RobotInfo nextWaypoint = new RobotInfo(results.waypoint, 0, 0);
            itemsToLog.Add(nextWaypoint);                        

            RobotInfo rInfo = new RobotInfo(new Vector2(results.waypoint.X, results.waypoint.Y), desiredState.Orientation, curinfo.ID);
              //  /*results.waypoint.cartesianAngle()*/ (results.waypoint - curinfo.Position).cartesianAngle(), curinfo.ID);
              
            List<RobotInfo> waypoints = new List<RobotInfo>();
            waypoints.Add(rInfo);
            List<Vector2> desState = new List<Vector2>();
            desState.Add(desiredState.Position);
            paths[id] = new Pair<List<RobotInfo>, List<Vector2>>(waypoints, desState);

            double wpDistanceSq = curinfo.Position.distanceSq(waypoints[0].Position);
            double angleDiff = Math.Abs(UsefulFunctions.angleDifference(curinfo.Orientation, waypoints[0].Orientation));
            


            MotionPlanningResults mpResults;
            WheelSpeeds wheelSpeeds;

            if (wpDistanceSq > MIN_SQ_DIST_TO_WP || angleDiff>MIN_ANGLE_DIFF_TO_WP ) {
                wheelSpeeds = _feedbackObjs[id].ComputeWheelSpeeds(curinfo, waypoints[0]);                
                mpResults = new MotionPlanningResults(wheelSpeeds, waypoints[0]);
            } else {

                Console.WriteLine("Close enough to point, stopping now.");
                wheelSpeeds = new WheelSpeeds();
                mpResults =  new MotionPlanningResults(wheelSpeeds, null);
            }

            itemsToLog.Add(wheelSpeeds);

            RobotPath robotPath = new RobotPath(waypoints);
            itemsToLog.Add(robotPath);

            DateTime now = DateTime.Now;
            TimeSpan timeSinceLastLog = now.Subtract(_lastLogEntry);
            if (_logging && timeSinceLastLog.TotalMilliseconds > 500)
            {
                _logWriter.LogItems(itemsToLog);
                _lastLogEntry = now;
            }

            return mpResults;



        }

        public void DrawLast(System.Drawing.Graphics g, ICoordinateConverter c) {
            //Common.DrawRobotInfoTree(_planner.LastTree1(), Color.Blue, g, c);
            //Common.DrawVector2Tree(_planner.LastTree2(), Color.Green, g, c);
            foreach (Pair<List<RobotInfo>, List<Vector2>> path in paths) {
                if (path == null)
                    continue;

                PathDrawing.DrawPath(path, Color.Blue, Color.Green, g, c);                
            }
        }

        //reload all necessary constants from files, for now just PID reload
        public void ReloadConstants() {
            Constants.Load("control");
            for (int robotID = 0; robotID < NUM_ROBOTS; robotID++)
                _feedbackObjs[robotID].ReloadConstants();

            LOG_EVERY_MSEC = Constants.get<int>("control", "LOG_EVERY_MSEC");
        }

        #region ILogger

        private string _logFile = null;
        private bool _logging = false;
        private DateTime _lastLogEntry;
        private LogWriter _logWriter = new LogWriter();

        public string LogFile
        {
            get { return _logFile; }
            set { _logFile = value; }
        }

        public bool Logging
        {
            get
            {
                return _logging;
            }
        }

        public void StartLogging()
        {
            if (_logging)
                return;

            if (_logFile == null)
            {
                throw new ApplicationException("Logger: must set LogFile before calling start");
            }

            _logWriter.OpenLogFile(_logFile);
            _logging = true;
        }

        public void StopLogging()
        {
            if (!_logging)
                return;

            _logWriter.CloseLogFile();
            _logging = false;
        }
        #endregion        

    }
#endif



}
