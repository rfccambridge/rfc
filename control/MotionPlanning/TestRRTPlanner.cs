using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Robocup.Core;
using Robocup.CoreRobotics;

namespace Robocup.MotionControl
{
    public class TestRRTPlanner : IMotionPlanner
    {

        private Dictionary<int, RRTIndvPlanner> planners = new Dictionary<int, RRTIndvPlanner>();
        public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius)
        {
            if (!planners.ContainsKey(id))
                planners.Add(id, new RRTIndvPlanner());
            RRTIndvPlanner planner = planners[id];
            lock (planner)
            {
                return planner.PlanMotion(id, desiredState, predictor, avoidBallRadius);
            }
        }
        public void DrawLast(Graphics g, ICoordinateConverter c)
        {
            foreach (RRTIndvPlanner planner in planners.Values)
                planner.DrawLast(g, c);
        }

        const int MAX_ITERATIONS = 500;

        public class RRTState
        {
            public RobotInfo current;
            public RobotInfo previous;
            public WheelSpeeds command;
            public RRTState(RobotInfo current, RobotInfo previous, WheelSpeeds command)
            {
                this.current = current;
                this.previous = previous;
                this.command = command;
            }
            public override string ToString()
            {
                return current + " // " + previous + " // " + command;
            }
        }
        private class StartTree
        {
            private List<RRTState> states = new List<RRTState>();
            public List<RRTState> States
            {
                get { return states; }
            }

            private Dictionary<RobotInfo, RRTState> previous = new Dictionary<RobotInfo, RRTState>();
            public Dictionary<RobotInfo, RRTState> Previous
            {
                get { return previous; }
            }

            public void AddState(RRTState state)
            {
                states.Add(state);
                previous.Add(state.current, state);
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
            Random r = new Random();
            MovementModeler mm = new MovementModeler();

            bool blocked(Vector2 position)
            {
                foreach (RobotInfo info in allinfos)
                {
                    if (info.Position.distanceSq(position) < .2 * .2)
                    {
                        return true;
                    }
                }
                if (position.distanceSq(ball.Position) < avoidball * avoidball)
                    return true;
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
            List<Vector2> extended_to = new List<Vector2>();
            List<Vector2> extended_from = new List<Vector2>();
            ExtendResults<RRTState> Extend(RobotInfo original, RobotInfo destination)
            {
                extended_to.Add(destination.Position);
                extended_from.Add(original.Position);
                ExtendResultType result = ExtendResultType.Success;
                WheelSpeeds ws = WheelSpeedsExtender.GetWheelSpeedsThrough(original, destination);
                RobotInfo newInfo = mm.ModelWheelSpeeds(original, ws, .1);
                if (destination.Position.distanceSq(original.Position) < .1 * .1)
                {
                    //newInfo = destination;
                    result = ExtendResultType.Destination;
                }
                if (blocked(newInfo.Position))
                    result = ExtendResultType.Blocked;
                return new ExtendResults<RRTState>(new RRTState(newInfo, original, ws), result);
                /*Vector2 next = original.Position + (destination.Position - original.Position).normalizeToLength(.05);
                RobotInfo newInfo = new RobotInfo(next, 0, original.ID);
                return new ExtendResults<RRTState>(new RRTState(newInfo, original, null), result);*/
            }
            ExtendResults<RRTState> Extend(RobotInfo original, Vector2 destination)
            {
                ExtendResultType result = ExtendResultType.Success;
                WheelSpeeds ws = WheelSpeedsExtender.GetWheelSpeedsThrough(original, destination);
                RobotInfo newInfo = mm.ModelWheelSpeeds(original, ws, .1);
                //Vector2 next = original.Position + (destination - original.Position).normalizeToLength(.05);
                if (destination.distanceSq(original.Position) < .1 * .1)
                {
                    result = ExtendResultType.Destination;
                }
                if (blocked(newInfo.Position))
                    result = ExtendResultType.Blocked;
                //RobotInfo newInfo = new RobotInfo(next, 0, original.ID);
                return new ExtendResults<RRTState>(new RRTState(newInfo, original, ws), result);
            }
            RobotInfo GenRandInfo(int id)
            {
                return new RobotInfo(new Vector2(r.NextDouble() * 5.5 - 2.75, r.NextDouble() * 4 - 2), 0, id);
            }
            Vector2 GenRandPoint()
            {
                return new Vector2(r.NextDouble() * 5.5 - 2.75, r.NextDouble() * 4 - 2);
            }
            const int MAX_EXTENDS = 3;
            ExtendResults<Vector2> ExtendGoalTreeTo(Vector2 point)
            {
                tree_lock.AcquireReaderLock(10000);
                Vector2 v = v2Finder.NearestNeighbor(point);
                tree_lock.ReleaseReaderLock();
                ExtendResults<Vector2> cur = Extend(v, point);
                if (cur.resultType != ExtendResultType.Success)
                    return cur;
                int num = 1;
                while (num < MAX_EXTENDS)
                {
                    ExtendResults<Vector2> next = Extend(cur.extension, point);
                    if (next.resultType == ExtendResultType.Destination)
                        return next;
                    if (next.resultType == ExtendResultType.Blocked)
                        return cur;
                    addGoalTreeNode(cur);
                    cur = next;
                    num++;
                }
                return cur;
            }
            ExtendResults<RRTState> ExtendStartTreeTo(RobotInfo point)
            {
                tree_lock.AcquireReaderLock(10000);
                RobotInfo info = riFinder.ClosestGoingTo(point);
                tree_lock.ReleaseReaderLock();
                return Extend(info, point);
            }
            ExtendResults<RRTState> ExtendStartTreeTo(Vector2 point)
            {
                tree_lock.AcquireReaderLock(10000);
                RobotInfo info = riFinder.ClosestGoingTo(point);
                tree_lock.ReleaseReaderLock();
                return Extend(info, point);
            }

            System.Threading.ReaderWriterLock tree_lock = new System.Threading.ReaderWriterLock();
            StartTree starttree;
            GoalTree goaltree;
            double avoidball;
            RobotInfoNNFinder riFinder;
            Vector2NNFinder v2Finder;
            List<RobotInfo> ourinfos, theirinfos, allinfos;
            BallInfo ball;

            bool addStartTreeNode(ExtendResults<RRTState> extend)
            {
                if (extend.resultType != ExtendResultType.Blocked)
                {
                    tree_lock.AcquireWriterLock(10000);
                    starttree.AddState(extend.extension);
                    riFinder.AddInfo(extend.extension.current);
                    tree_lock.ReleaseWriterLock();
                }
                return extend.resultType == ExtendResultType.Destination;
            }
            bool addGoalTreeNode(ExtendResults<Vector2> extend)
            {
                if (extend.resultType != ExtendResultType.Blocked)
                {
                    tree_lock.AcquireWriterLock(10000);
                    goaltree.States.Add(extend.extension);
                    v2Finder.AddPoint(extend.extension);
                    tree_lock.ReleaseWriterLock();
                }
                return extend.resultType == ExtendResultType.Destination;
            }

            RRTState ExtendStartTree(int id)
            {
                RobotInfo desired_info = GenRandInfo(id);
                ExtendResults<RRTState> extended = ExtendStartTreeTo(desired_info);
                addStartTreeNode(extended);
                if (extended.resultType != ExtendResultType.Blocked)
                {
                    bool done = addGoalTreeNode(ExtendGoalTreeTo(extended.extension.current.Position));
                    if (done)
                    {
                        return extended.extension;
                    }
                }
                return null;
            }
            RRTState ExtendGoalTree()
            {
                Vector2 desired_point = GenRandPoint();
                ExtendResults<Vector2> extended_goal = ExtendGoalTreeTo(desired_point);
                addGoalTreeNode(extended_goal);
                if (extended_goal.resultType != ExtendResultType.Blocked)
                {
                    ExtendResults<RRTState> extended_start = ExtendStartTreeTo(extended_goal.extension);
                    bool done = addStartTreeNode(extended_start);
                    if (done)
                    {
                        return extended_start.extension;
                    }
                }
                return null;
            }

            public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius)
            {
                extended_to = new List<Vector2>();
                extended_from = new List<Vector2>();

                this.avoidball = avoidBallRadius;
                ball = predictor.getBallInfo();
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
                starttree.AddState(new RRTState(thisinfo, null, null));
                v2Finder.AddPoint(desiredState.Position);
                goaltree.States.Add(desiredState.Position);

                for (int i=0;i<100;i++)
                {
                    ExtendResults<RRTState> extend = ExtendStartTreeTo(desiredState);
                    if (extend.resultType == ExtendResultType.Success)
                        addStartTreeNode(extend);
                    else if (extend.resultType == ExtendResultType.Blocked)
                        break;
                    else if (extend.resultType == ExtendResultType.Destination)
                        return new MotionPlanningResults(Smooth(extend.extension));
                }

                //if (thisinfo.Position.distanceSq(desiredState.Position) < .2 * .2)
                //    return new MotionPlanningResults(GetWheelSpeeds(thisinfo, desiredState));

                double prob_start = .4;
                RRTState last = null;
                for (int i = 0; i < MAX_ITERATIONS; i++)
                {
                    double d = r.NextDouble();
                    if (d < prob_start)
                    {
                        last = ExtendStartTree(id);
                        if (last != null)
                            break;
                    }
                    else
                    {
                        last = ExtendGoalTree();
                        if (last != null)
                            break;
                    }
                    //prob_start *= .98;
                }
                if (last == null)
                    return new MotionPlanningResults(new WheelSpeeds());
                return new MotionPlanningResults(Smooth(last));
            }
            private WheelSpeeds Smooth(RRTState last)
            {
                WheelsInfo<double> current = 1.0 * last.command;
                while (last.previous != null)
                {
                    current = WheelsInfo<double>.Add(1.0 * last.command, .5 * (WheelSpeeds)current);
                    last = starttree.Previous[last.previous];
                }

                return (WheelSpeeds)current;
            }

            public void DrawLast(Graphics g, ICoordinateConverter c)
            {
                Brush b = new SolidBrush(Color.Black);
                Pen p = new Pen(b);
                tree_lock.AcquireReaderLock(10000);
                foreach (RRTState info in starttree.States)
                {
                    Vector2 position = info.current.Position;
                    g.FillRectangle(b, c.fieldtopixelX(position.X) - 1, c.fieldtopixelY(position.Y) - 1, 2, 2);
                    if (info.previous != null)
                    {
                        Vector2 prev = info.previous.Position;
                        g.DrawLine(p, c.fieldtopixelX(position.X), c.fieldtopixelY(position.Y),
                            c.fieldtopixelX(prev.X), c.fieldtopixelY(prev.Y));
                    }
                }
                foreach (Vector2 v in goaltree.States)
                {
                    g.FillRectangle(b, c.fieldtopixelX(v.X) - 1, c.fieldtopixelY(v.Y) - 1, 2, 2);
                }
                /*for (int i = 0; i < extended_from.Count; i++)
                {
                    g.DrawLine(p, c.fieldtopixelX(extended_from[i].X), c.fieldtopixelY(extended_from[i].Y),
                        c.fieldtopixelX(extended_to[i].X), c.fieldtopixelY(extended_to[i].Y));
                }*/
                tree_lock.ReleaseReaderLock();
                p.Dispose();
                b.Dispose();
            }
        }
    }
}
