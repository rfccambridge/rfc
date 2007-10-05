using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Robocup.Core;

namespace Robocup.RRT
{
    public class RRTPlanner : IMotionPlanner
    {
        private enum ExtendResultType
        {
            Success, Blocked, Destination
        }
        private class ExtendResults<T>
        {
            public T extension;
            public ExtendResultType resultType;
            public ExtendResults(T extension, ExtendResultType type)
            {
                this.extension = extension;
                this.resultType = type;
            }
        }

        private Dictionary<int, RRTIndvPlanner> planners = new Dictionary<int, RRTIndvPlanner>();
        public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius)
        {
            if (!planners.ContainsKey(id))
                planners.Add(id, new RRTIndvPlanner());
            return planners[id].PlanMotion(id, desiredState, predictor, avoidBallRadius);
        }
        public void DrawLast(Graphics g, ICoordinateConverter c)
        {
            foreach (RRTIndvPlanner planner in planners.Values)
                planner.DrawLast(g, c);
        }

        const int MAX_ITERATIONS = 200;

        private class StartTree
        {
            private List<RobotInfo> states = new List<RobotInfo>();
            public List<RobotInfo> States
            {
                get { return states; }
            }

        }
        private class GoalTree
        {
            private List<Vector2> states = new List<Vector2>();
            public List<Vector2> States
            {
                get { return states; }
            }
        }

        private class RRTIndvPlanner : IMotionPlanner
        {
            static Random r = new Random();

            bool blocked(Vector2 position)
            {
                foreach (RobotInfo info in allinfos)
                {
                    if (info.Position.distanceSq(position) < .2 * .2)
                    {
                        return true;
                    }
                }
                return false;
            }
            ExtendResults<Vector2> Extend(Vector2 original, Vector2 destination)
            {
                ExtendResultType result = ExtendResultType.Success;
                Vector2 next = original + (destination - original).normalizeToLength(.05);
                if (destination.distanceSq(original) < .05 * .05)
                {
                    next = destination;
                    result = ExtendResultType.Destination;
                }
                if (blocked(next))
                    result = ExtendResultType.Blocked;
                return new ExtendResults<Vector2>(next, result);
            }
            ExtendResults<RobotInfo> Extend(RobotInfo original, RobotInfo destination)
            {
                ExtendResultType result = ExtendResultType.Success;
                Vector2 next = original.Position + (destination.Position - original.Position).normalizeToLength(.05);
                if (destination.Position.distanceSq(original.Position) < .05 * .05)
                {
                    next = destination.Position;
                    result = ExtendResultType.Destination;
                }
                if (blocked(next))
                    result = ExtendResultType.Blocked;
                return new ExtendResults<RobotInfo>(new RobotInfo(next, 0, original.ID), result);
            }
            ExtendResults<RobotInfo> Extend(RobotInfo original, Vector2 destination)
            {
                ExtendResultType result = ExtendResultType.Success;
                Vector2 next = original.Position + (destination - original.Position).normalizeToLength(.05);
                if (destination.distanceSq(original.Position) < .05 * .05)
                {
                    next = destination;
                    result = ExtendResultType.Destination;
                }
                if (blocked(next))
                    result = ExtendResultType.Blocked;
                return new ExtendResults<RobotInfo>(new RobotInfo(next, 0, original.ID), result);
            }
            RobotInfo GenRandInfo(int id)
            {
                return new RobotInfo(new Vector2(r.NextDouble() * 5.5 - 2.75, r.NextDouble() * 4 - 2), 0, id);
            }
            Vector2 GenRandPoint()
            {
                return new Vector2(r.NextDouble() * 5.5 - 2.75, r.NextDouble() * 4 - 2);
            }
            ExtendResults<Vector2> ExtendGoalTreeTo(Vector2 point)
            {
                Vector2 v = v2Finder.NearestNeighbor(point);
                return Extend(v, point);
            }
            ExtendResults<RobotInfo> ExtendStartTreeTo(RobotInfo point)
            {
                RobotInfo info = riFinder.ClosestGoingTo(point);
                return Extend(info, point);
            }
            ExtendResults<RobotInfo> ExtendStartTreeTo(Vector2 point)
            {
                RobotInfo info = riFinder.ClosestGoingTo(point);
                return Extend(info, point);
            }

            StartTree starttree;
            GoalTree goaltree;
            RobotInfoNNFinder riFinder;
            Vector2NNFinder v2Finder;
            List<RobotInfo> ourinfos, theirinfos, allinfos;

            bool addStartTreeNode(ExtendResults<RobotInfo> extend)
            {
                if (extend.resultType != ExtendResultType.Blocked)
                {
                    starttree.States.Add(extend.extension);
                    riFinder.AddInfo(extend.extension);
                }
                return extend.resultType == ExtendResultType.Destination;
            }
            bool addGoalTreeNode(ExtendResults<Vector2> extend)
            {
                if (extend.resultType != ExtendResultType.Blocked)
                {
                    goaltree.States.Add(extend.extension);
                    v2Finder.AddPoint(extend.extension);
                }
                return extend.resultType == ExtendResultType.Destination;
            }

            public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius)
            {
                RobotInfo thisinfo = predictor.getCurrentInformation(id);
                ourinfos = predictor.getOurTeamInfo();
                ourinfos.Remove(thisinfo);
                theirinfos = predictor.getTheirTeamInfo();
                allinfos = new List<RobotInfo>(ourinfos);
                allinfos.AddRange(theirinfos);
                riFinder = new RobotInfoNNFinder();
                starttree = new StartTree();
                goaltree = new GoalTree();
                v2Finder = new Vector2NNFinder();
                riFinder.AddInfo(thisinfo);
                starttree.States.Add(thisinfo);
                v2Finder.AddPoint(desiredState.Position);
                goaltree.States.Add(desiredState.Position);
                for (int i = 0; i < MAX_ITERATIONS; i++)
                {
                    RobotInfo desired_info = GenRandInfo(id);
                    ExtendResults<RobotInfo> extended = ExtendStartTreeTo(desired_info);
                    addStartTreeNode(extended);
                    if (extended.resultType != ExtendResultType.Blocked)
                    {
                        bool done = addGoalTreeNode(ExtendGoalTreeTo(extended.extension.Position));
                        if (done)
                            break;
                    }

                    Vector2 desired_point = GenRandPoint();
                    ExtendResults<Vector2> extended_goal = ExtendGoalTreeTo(desired_point);
                    addGoalTreeNode(extended_goal);
                    if (extended_goal.resultType != ExtendResultType.Blocked)
                    {
                        bool done = addStartTreeNode(ExtendStartTreeTo(extended_goal.extension));
                        if (done)
                            break;
                    }
                }
                return new MotionPlanningResults(new WheelSpeeds(127, 0, 40, 127));
            }
            public void DrawLast(Graphics g, ICoordinateConverter c)
            {
                Brush b = new SolidBrush(Color.Black);
                foreach (RobotInfo info in starttree.States)
                {
                    g.FillRectangle(b, c.fieldtopixelX(info.Position.X) - 1, c.fieldtopixelY(info.Position.Y) - 1, 2, 2);
                }
                foreach (Vector2 v in goaltree.States)
                {
                    g.FillRectangle(b, c.fieldtopixelX(v.X) - 1, c.fieldtopixelY(v.Y) - 1, 2, 2);
                }
                b.Dispose();
            }
        }
    }
}
