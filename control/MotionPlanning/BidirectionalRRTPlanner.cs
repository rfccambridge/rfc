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
    public class BidirectionalRRTPlanner<T1, T2, G1, G2>
        where T1 : class
        where T2 : class
        where G1 : class, BiSearchTree<T1, T2>, new()
        where G2 : class, BiSearchTree<T2, T1>, new()
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
                waypoints1 = new T1[numwaypoints];
                waypoints2 = new T2[numwaypoints];
                numSavedWaypoints1 = 0;
                numSavedWaypoints2 = 0;
            }
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
        Extender<T1, T1> extender11;
        Extender<T1, T2> extender12;
        Extender<T2, T2> extender22;
        Extender<T2, T1> extender21;
        //Predicate<T> blocked;
        ValueFunction<T1> randomstate1;
        ValueFunction<T2> randomstate2;
        int numSavedWaypoints1, numSavedWaypoints2;
        T1[] waypoints1;
        T2[] waypoints2;
        Random r = new Random();
        /// <summary>
        /// Note: both extender2? extenders extend the T2 type -- which is the goal node;
        /// this means that these extenders are extending BACKWARDS
        /// </summary>
        public BidirectionalRRTPlanner(
            Extender<T1, T1> extender11, Extender<T1, T2> extender12,
            Extender<T2, T1> backextender21, Extender<T2, T2> backextender22,
            ValueFunction<T1> randomstate1, ValueFunction<T2> randomstate2)
        {
            this.extender11 = extender11;
            this.extender12 = extender12;
            this.extender21 = backextender21;
            this.extender22 = backextender22;
            this.randomstate1 = randomstate1;
            this.randomstate2 = randomstate2;
            waypoints1 = new T1[numwaypoints];
            waypoints2 = new T2[numwaypoints];
            numSavedWaypoints1 = 0;
            numSavedWaypoints2 = 0;
        }

        public Pair<List<T1>, List<T2>> Plan(T1 current, T2 goal, object state)
        {
            Pair<List<T1>, List<T2>> rtn = FindPath(current, goal, state);
            UpdateWaypoints(rtn);
            return rtn;
        }
        private void ClearWaypoints()
        {
            numSavedWaypoints1 = 0;
            numSavedWaypoints2 = 0;
        }
        private void UpdateWaypoints(Pair<List<T1>, List<T2>> rtn)
        {
            foreach (T1 node in rtn.First)
            {
                if (numSavedWaypoints1 < numwaypoints)
                {
                    waypoints1[numSavedWaypoints1++] = node;
                }
                else
                {
                    waypoints1[r.Next(numwaypoints)] = node;
                }
            }
            foreach (T2 node in rtn.Second)
            {
                if (numSavedWaypoints2 < numwaypoints)
                {
                    waypoints2[numSavedWaypoints2++] = node;
                }
                else
                {
                    waypoints2[r.Next(numwaypoints)] = node;
                }
            }
        }

        Pair<T1, T2> ExtendStartTree(G1 startTree, G2 endTree, object state)
        {
            T1 extendTo;
            double p = r.NextDouble();
            if (numSavedWaypoints1 > 0 && p < waypointprob)
                extendTo = waypoints1[r.Next(numSavedWaypoints1)];
            else
                extendTo = randomstate1();

            T1 extendFrom = startTree.ClosestGoingTo(extendTo);

            for (int i = 0; i < maxextends; i++)
            {
                ExtendResults<T1> extendresults = extender11(extendFrom, extendTo, state);
                if (extendresults.resultType == ExtendResultType.Blocked)
                    break;
                else if (extendresults.resultType == ExtendResultType.Destination)
                {
                    startTree.AddNode(extendresults.extension, extendFrom);
                    break;
                }
                else if (extendresults.resultType == ExtendResultType.Success)
                {
                    startTree.AddNode(extendresults.extension, extendFrom);
                    extendFrom = extendresults.extension;
                }
            }
            T2 otherExtendFrom = endTree.ClosestStartingAt(extendFrom);
            for (int i = 0; i < maxextends; i++)
            {
                ExtendResults<T2> extendresults = extender21(otherExtendFrom, extendFrom, state);
                if (extendresults.resultType == ExtendResultType.Blocked)
                    break;
                else if (extendresults.resultType == ExtendResultType.Destination)
                {
                    endTree.AddNode(extendresults.extension, otherExtendFrom);
                    return new Pair<T1, T2>(extendFrom, extendresults.extension);
                }
                else if (extendresults.resultType == ExtendResultType.Success)
                {
                    endTree.AddNode(extendresults.extension, otherExtendFrom);
                    otherExtendFrom = extendresults.extension;
                }
            }
            return null;
        }
        Pair<T1, T2> ExtendEndTree(G1 startTree, G2 endTree, object state)
        {
            T2 extendTo;
            double p = r.NextDouble();
            if (numSavedWaypoints2 > 0 && p < waypointprob)
                extendTo = waypoints2[r.Next(numSavedWaypoints2)];
            else
                extendTo = randomstate2();

            T2 extendFrom = endTree.ClosestStartingAt(extendTo);

            for (int i = 0; i < maxextends; i++)
            {
                ExtendResults<T2> extendresults = extender22(extendFrom, extendTo, state);
                if (extendresults.resultType == ExtendResultType.Blocked)
                    break;
                else if (extendresults.resultType == ExtendResultType.Destination)
                {
                    endTree.AddNode(extendresults.extension, extendFrom);
                    break;
                }
                else if (extendresults.resultType == ExtendResultType.Success)
                {
                    endTree.AddNode(extendresults.extension, extendFrom);
                    extendFrom = extendresults.extension;
                }
            }
            T1 otherExtendFrom = startTree.ClosestGoingTo(extendFrom);
            for (int i = 0; i < maxextends; i++)
            {
                ExtendResults<T1> extendresults = extender12(otherExtendFrom, extendFrom, state);
                if (extendresults.resultType == ExtendResultType.Blocked)
                    break;
                else if (extendresults.resultType == ExtendResultType.Destination)
                {
                    startTree.AddNode(extendresults.extension, otherExtendFrom);
                    return new Pair<T1, T2>(extendresults.extension, extendFrom);
                }
                else if (extendresults.resultType == ExtendResultType.Success)
                {
                    startTree.AddNode(extendresults.extension, otherExtendFrom);
                    otherExtendFrom = extendresults.extension;
                }
            }
            return null;
        }

        G1 lastTree1 = null;
        G2 lastTree2 = null;
        private Pair<List<T1>, List<T2>> FindPath(T1 current, T2 goal, object state)
        {
            G1 startTree = new G1();
            G2 endTree = new G2();
            startTree.AddNode(current, null);
            endTree.AddNode(goal, null);

            while (startTree.Size() + endTree.Size() < MaxTreeSize)
            {
                Pair<T1, T2> connection = ExtendStartTree(startTree, endTree, state);
                if (connection != null)
                {
                    lastTree1 = startTree;
                    lastTree2 = endTree;
                    return GetPath(startTree, endTree, connection.First, connection.Second);
                }
                connection = ExtendEndTree(startTree, endTree, state);
                if (connection != null)
                {
                    lastTree1 = startTree;
                    lastTree2 = endTree;
                    return GetPath(startTree, endTree, connection.First, connection.Second);
                }
            }
            //didn't find a path,
            lastTree1 = startTree;
            lastTree2 = endTree;
            return GetPath(startTree, endTree, startTree.ClosestGoingTo(goal), endTree.ClosestStartingAt(current));
        }
        private Pair<List<T1>, List<T2>> GetPath(G1 startTree, G2 endTree, T1 node1, T2 node2)
        {
            Pair<List<T1>, List<T2>> rtn = new Pair<List<T1>, List<T2>>(null, null);
            rtn.First = new List<T1>();
            rtn.Second = new List<T2>();
            while (node1 != null)
            {
                rtn.First.Add(node1);
                node1 = startTree.ParentNode(node1);
            }
            rtn.First.Reverse();
            while (node2 != null)
            {
                rtn.Second.Add(node2);
                node2 = endTree.ParentNode(node2);
            }
            return rtn;
        }
        public G1 LastTree1()
        {
            return lastTree1;
        }
        public G2 LastTree2()
        {
            return lastTree2;
        }
    }
}
