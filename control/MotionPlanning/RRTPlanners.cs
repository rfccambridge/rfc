using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Core;
using Robocup.CoreRobotics;

using System.Drawing;

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
    public class MixedBiRRTMotionPlanner : IMotionPlanner
    {
        public int MaxExtends
        {
            get { return planner.MaxExtends; }
            set { planner.MaxExtends = value; }
        }


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
    public class FeedbackMotionPlanner : IMotionPlanner {
        //the index of the next waypoint the robots going to try and go to and an associated robotinfo
        int nextWaypointIndex = 0;
        RobotInfo nextWayPoint;


        // Each robot has a feedback object
        private Feedback[] _feedbackObjs;

        // For drawing
        private RobotInfo _lastNearestWaypoint;
             
        private CirclePlanner _planner;

        const int NUM_ROBOTS = 5;

        public FeedbackMotionPlanner() {
            

            //replaced with static testing planner
            _planner = new CirclePlanner();

            _feedbackObjs = new Feedback[NUM_ROBOTS];
            for (int robotID = 0; robotID < NUM_ROBOTS; robotID++)
                _feedbackObjs[robotID] = new Feedback(robotID);

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
           
            
            
            ///instead of going to nearest going to try more of a carrot on a stick approach and go to the next one.
            ///  RobotInfo nearestWayPoint = findNearestWaypoint(curinfo, path);
            ///_lastNearestWaypoint = nearestWayPoint;
            ///WheelSpeeds wheelSpeeds = _feedbackObjs[id].computeWheelSpeeds(curinfo, nearestWayPoint);

            nextWaypointIndex = findNearestWaypointIndex(curinfo, path);
            if (nextWaypointIndex!=path.First.Count-1)
                nextWaypointIndex = nextWaypointIndex+1;
            nextWayPoint = path.First[nextWaypointIndex];

            WheelSpeeds wheelSpeeds = _feedbackObjs[id].computeWheelSpeeds(curinfo, nextWayPoint);
            
            return new MotionPlanningResults(wheelSpeeds, nextWayPoint);

            //return new MotionPlanningResults(new WheelSpeeds());
            /*WheelSpeeds rtn;
            if (desiredState.Position.distanceSq(curinfo.Position) < .15 * .15)
                rtn = WheelSpeedsExtender.GetWheelSpeedsTo(curinfo, desiredState);
            else if (path.First.Count + path.Second.Count < 5)
                rtn = WheelSpeedsExtender.GetWheelSpeedsTo(curinfo, desiredState);
            else if (path.First.Count > 5)
                rtn = WheelSpeedsExtender.GetWheelSpeedsTo(curinfo, path.First[5]);
            else
                rtn = WheelSpeedsExtender.GetWheelSpeedsTo(curinfo, desiredState);
            
            return new MotionPlanningResults(Common.addOrientation(curinfo.Orientation, desiredState.Orientation,
                rtn));*/

        }

        public void DrawLast(System.Drawing.Graphics g, ICoordinateConverter c) {
            //Common.DrawRobotInfoTree(_planner.LastTree1(), Color.Blue, g, c);
            //Common.DrawVector2Tree(_planner.LastTree2(), Color.Green, g, c);            
            Common.DrawPath(_planner.LastPath, Color.Blue, g, c);
        }

        //reload all necessary constants from files, for now just PID reload
        public void reloadConstants() {
            for (int robotID = 0; robotID < NUM_ROBOTS; robotID++)
                _feedbackObjs[robotID].reloadConstands();
        }
    }
    public class SmoothVector2BiRRTMotionPlanner : IMotionPlanner
    {
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
}
