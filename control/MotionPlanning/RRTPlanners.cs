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
            planner = new BasicRRTPlanner<Vector2, Vector2Tree>(Extend, RandomState);
        }

        const double extendDistance = .03;
        private ExtendResults<Vector2> Extend(Vector2 start, Vector2 end)
        {
            if (start.distanceSq(end) < extendDistance * extendDistance)
                return new ExtendResults<Vector2>(end, ExtendResultType.Destination);
            Vector2 next = (end - start).normalizeToLength(extendDistance) + start;
            if (Blocked(next))
                return new ExtendResults<Vector2>(next, ExtendResultType.Blocked);
            return new ExtendResults<Vector2>(next, ExtendResultType.Success);
        }

        private bool Blocked(Vector2 point)
        {
            foreach (Obstacle o in obstacles)
            {
                if (o.position.distanceSq(point) < o.size * o.size)
                    return true;
            }
            return false;
        }
        Random r = new Random();
        private Vector2 RandomState()
        {
            return new Vector2((r.NextDouble() - .5) * 2 * 2.75, (r.NextDouble() - .5) * 2 * 2);
        }

        List<Obstacle> obstacles;
        List<Vector2> lastpath;
        public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius)
        {
            obstacles = new List<Obstacle>();
            foreach (RobotInfo info in predictor.getAllInfos())
            {
                if (info.ID != id)
                    //TODO magic number (robot radius)
                    obstacles.Add(new Obstacle(info.Position, .2));
            }
            if (avoidBallRadius > 0)
                obstacles.Add(new Obstacle(predictor.getBallInfo().Position, avoidBallRadius));

            RobotInfo curinfo = predictor.getCurrentInformation(id);
            List<Vector2> path = planner.Plan(curinfo.Position, desiredState.Position);
            lastpath = path;

            return new MotionPlanningResults(WheelSpeedsExtender.GetWheelSpeeds(curinfo, path[Math.Min(path.Count - 1, 5)]));
        }

        public void DrawLast(System.Drawing.Graphics g, ICoordinateConverter c)
        {
            if (lastpath != null)
            {
                Brush b = new SolidBrush(Color.Black);

                //foreach (Vector2 v in lastpath)
                foreach (Vector2 v in planner.LastTree().AllNodes())
                {
                    g.FillRectangle(b, c.fieldtopixelX(v.X) - 1, c.fieldtopixelY(v.Y) - 1, 2, 2);
                }

                b.Dispose();
            }
        }
    }
    public class KinodynamicRRTMotionPlanner : IMotionPlanner
    {
        BasicRRTPlanner<RobotInfo, RobotInfoTree> planner;

        public KinodynamicRRTMotionPlanner()
        {
            planner = new BasicRRTPlanner<RobotInfo, RobotInfoTree>(Extend, RandomState);
        }

        MovementModeler mm = new MovementModeler();
        const double extendTime = .05;
        private ExtendResults<RobotInfo> Extend(RobotInfo start, RobotInfo end)
        {
            ExtendResultType result = ExtendResultType.Success;
            WheelSpeeds ws = WheelSpeedsExtender.GetWheelSpeeds(start, end);
            RobotInfo newInfo = mm.ModelWheelSpeeds(start, ws, extendTime);
            if (end.Position.distanceSq(start.Position) < .1 * .1)
            {
                //newInfo = destination;
                result = ExtendResultType.Destination;
            }
            if (Blocked(newInfo.Position))
                result = ExtendResultType.Blocked;
            return new ExtendResults<RobotInfo>(newInfo, result);
        }

        private bool Blocked(Vector2 point)
        {
            foreach (Obstacle o in obstacles)
            {
                if (o.position.distanceSq(point) < o.size * o.size)
                    return true;
            }
            return false;
        }
        Random r = new Random();
        private RobotInfo RandomState()
        {
            Vector2 position = new Vector2((r.NextDouble() - .5) * 2 * 2.75, (r.NextDouble() - .5) * 2 * 2);
            return new RobotInfo(position, 0, -1);
        }

        List<Obstacle> obstacles;
        List<RobotInfo> lastpath;
        public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius)
        {
            obstacles = new List<Obstacle>();
            foreach (RobotInfo info in predictor.getAllInfos())
            {
                if (info.ID != id)
                    //TODO magic number (robot radius)
                    obstacles.Add(new Obstacle(info.Position, .2));
            }
            if (avoidBallRadius > 0)
                obstacles.Add(new Obstacle(predictor.getBallInfo().Position, avoidBallRadius));

            RobotInfo curinfo = predictor.getCurrentInformation(id);
            List<RobotInfo> path = planner.Plan(curinfo, desiredState);
            lastpath = path;

            return new MotionPlanningResults(WheelSpeedsExtender.GetWheelSpeeds(curinfo, path[Math.Min(path.Count - 1, 5)]));
        }

        public void DrawLast(System.Drawing.Graphics g, ICoordinateConverter c)
        {
            if (lastpath != null)
            {
                Brush b = new SolidBrush(Color.Black);
                Pen p = new Pen(b);

                RobotInfoTree tree = planner.LastTree();
                //foreach (RobotInfo info in lastpath)
                foreach (RobotInfo info in tree.AllNodes())
                {
                    Vector2 v = info.Position;
                    g.FillRectangle(b, c.fieldtopixelX(v.X) - 1, c.fieldtopixelY(v.Y) - 1, 3, 3);
                    RobotInfo prev = tree.ParentNode(info);
                    if (prev != null)
                    {
                        Vector2 v2 = prev.Position;
                        g.DrawLine(p, (float)c.fieldtopixelX(v.X), (float)c.fieldtopixelY(v.Y),
                            (float)c.fieldtopixelX(v2.X), (float)c.fieldtopixelY(v2.Y));
                    }
                }

                p.Dispose();
                b.Dispose();
            }
        }
    }
}
