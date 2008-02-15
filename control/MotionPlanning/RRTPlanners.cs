using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Core;
using Robocup.CoreRobotics;

using System.Drawing;

namespace Robocup.MotionControl
{
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

            return new MotionPlanningResults(WheelSpeedsExtender.GetWheelSpeeds(curinfo, path[Math.Min(path.Count - 1, 5)]));
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

            return new MotionPlanningResults(WheelSpeedsExtender.GetWheelSpeeds(curinfo, path[Math.Min(path.Count - 1, 5)]));
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
                rtn = WheelSpeedsExtender.GetWheelSpeeds(curinfo, path.First[5]);
                important = path.First[5];
            }
            else if (path.First.Count + path.Second.Count > 5)
            {
                rtn = WheelSpeedsExtender.GetWheelSpeeds(curinfo, path.Second[5 - path.First.Count]);
                important = path.Second[5 - path.First.Count];
            }
            else
            {
                rtn = WheelSpeedsExtender.GetWheelSpeeds(curinfo, desiredState);
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
                Common.ExtendRR, Common.ExtendRV, Common.ExtendVR, Common.ExtendVV, Common.RandomStateR, Common.RandomStateV);
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
            Pair<List<RobotInfo>, List<Vector2>> path = planner.Plan(curinfo, desiredState.Position, obstacles);

            //return new MotionPlanningResults(new WheelSpeeds());
            WheelSpeeds rtn;
            if (path.First.Count > 5)
                rtn = WheelSpeedsExtender.GetWheelSpeeds(curinfo, path.First[5]);
            else if (path.First.Count + path.Second.Count > 5)
                rtn = WheelSpeedsExtender.GetWheelSpeeds(curinfo, path.Second[5 - path.First.Count]);
            else
                rtn = WheelSpeedsExtender.GetWheelSpeeds(curinfo, desiredState);
            return new MotionPlanningResults(rtn);
        }

        public void DrawLast(System.Drawing.Graphics g, ICoordinateConverter c)
        {
            Common.DrawRobotInfoTree(planner.LastTree1(), Color.Blue, g, c);
            Common.DrawVector2Tree(planner.LastTree2(), Color.Green, g, c);
        }
    }
    public class SmoothVector2BiRRTMotionPlanner : IMotionPlanner
    {
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
                    obstacles.Add(new Obstacle(info.Position, .22));
            }
            if (avoidBallRadius > 0 && predictor.getBallInfo().Position != null)
                obstacles.Add(new Obstacle(predictor.getBallInfo().Position, avoidBallRadius));

            RobotInfo curinfo = predictor.getCurrentInformation(id);
            Pair<List<Vector2>, List<Vector2>> path = planner.Plan(curinfo.Position, desiredState.Position, obstacles);

            List<Vector2> waypoints = path.First;
            waypoints.AddRange(path.Second);

            Console.WriteLine("SMOOTHING: final orientation: " + desiredState.Orientation);
            
            return Smoother.Smooth(curinfo, desiredState, waypoints, obstacles);
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

            Smoother.DrawLast(g, c);
        }
    }
}
