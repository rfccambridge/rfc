using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Core;

namespace Robocup.MotionControl
{

    /// <summary>
    /// A basic RRT planner (no bidirectional)
    /// Assumes that it's only planning one robot
    /// </summary>
    public class BasicRRTPlanner<T, G>
        where T : class
        where G : class, SearchTree<T>, new()
    {
        #region Algorithm parameters
        private int maxtreesize = 500;
        public int MaxTreeSize
        {
            get { return maxtreesize; }
            set { maxtreesize = value; }
        }

        private int numwaypoints = 200;
        public int NumWaypoints
        {
            get { return numwaypoints; }
            set
            {
                numwaypoints = value;
                waypoints = new T[numwaypoints];
                numSavedWaypoints = 0;
            }
        }

        private double goalprob = .1;
        public double GoalProbability
        {
            get { return goalprob; }
            set { goalprob = value; }
        }

        private double waypointprob = .6;
        public double WaypointProbability
        {
            get { return waypointprob; }
            set { waypointprob = value; }
        }

        private int maxextends = 4;
        public int MaxExtends
        {
            get { return maxextends; }
            set { maxextends = value; }
        }
        #endregion
        Extender<T, T> extender;
        //Predicate<T> blocked;
        ValueFunction<T> randomstate;
        int numSavedWaypoints;
        T[] waypoints;
        Random r = new Random();
        public BasicRRTPlanner(Extender<T, T> extender, /*Predicate<T> blocked, */ValueFunction<T> randomstate)
        {
            this.extender = extender;
            //this.blocked = blocked;
            this.randomstate = randomstate;
            waypoints = new T[numwaypoints];
            numSavedWaypoints = 0;
        }

        public List<T> Plan(T current, T goal)
        {
            List<T> rtn = FindPath(current, goal);
            UpdateWaypoints(rtn);
            return rtn;
        }
        private void ClearWaypoints()
        {
            numSavedWaypoints = 0;
        }
        private void UpdateWaypoints(List<T> rtn)
        {
            foreach (T node in rtn)
            {
                if (numSavedWaypoints < numwaypoints)
                {
                    waypoints[numSavedWaypoints++] = node;
                }
                else
                {
                    waypoints[r.Next(numwaypoints)] = node;
                }
            }
        }

        G lastTree = null;
        private List<T> FindPath(T current, T goal)
        {
            G tree = new G();
            tree.AddNode(current, null);

            while (tree.Size() < MaxTreeSize)
            {
                T extendTo;
                double p = r.NextDouble();
                if (p < goalprob)
                    extendTo = goal;
                else if (numSavedWaypoints > 0 && p < goalprob + waypointprob)
                    extendTo = waypoints[r.Next(numSavedWaypoints)];
                else
                    extendTo = randomstate();

                T extendFrom = tree.ClosestGoingTo(extendTo);
                //if (extendFrom.Equals(extendTo))
                //    continue;

                for (int i = 0; i < maxextends; i++)
                {
                    ExtendResults<T> extendresults = extender(extendFrom, extendTo);
                    if (extendresults.resultType == ExtendResultType.Blocked)
                        break;
                    else if (extendresults.resultType == ExtendResultType.Destination)
                    {
                        tree.AddNode(extendresults.extension, extendFrom);
                        if (extendTo == goal)
                        {
                            lastTree = tree;
                            return GetPath(extendresults.extension, tree);
                        }
                        else
                            break;
                    }
                    else if (extendresults.resultType == ExtendResultType.Success)
                    {
                        tree.AddNode(extendresults.extension, extendFrom);
                        extendFrom = extendresults.extension;
                    }
                }
            }
            //didn't find a path,
            lastTree = tree;
            return GetPath(tree.ClosestGoingTo(goal), tree);
        }
        private List<T> GetPath(T goal, G tree)
        {
            List<T> rtn = new List<T>();
            T cur = goal;
            while (cur != null)
            {
                rtn.Add(cur);
                cur = tree.ParentNode(cur);
            }
            rtn.Reverse();
            return rtn;
        }
        public G LastTree()
        {
            return lastTree;
        }
    }
}
