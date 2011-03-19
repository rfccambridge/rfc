using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Core;

namespace Robocup.MotionControl
{
    public interface SearchTree<T> where T : class
    {
        /// <summary>
        /// Adds a node to the graph, with a given parent node
        /// </summary>
        /// <param name="node">The node to add to the graph</param>
        /// <param name="parent">The parent of the node; if there is no parent let it be null</param>
        void AddNode(T node, T parent);
        /// <summary>
        /// Finds the parent node of a given node.  Returns null if there is no parent
        /// </summary>
        /// <param name="node">The node whose parent you wish to find</param>
        T ParentNode(T node);
        /// <summary>
        /// Finds the node in the graph closest to the requested point, starting at the given point
        /// and going to the nodes in the graph
        /// </summary>
        /// <param name="point">The point to find the closest node to</param>
        T ClosestStartingAt(T point);
        /// <summary>
        /// Finds the node in the graph closest to the requested point, starting at any point
        /// in the graph and going to the given node
        /// </summary>
        /// <param name="point">The point to find the closest node to</param>
        T ClosestGoingTo(T point);
        /// <summary>
        /// The number of nodes in the tree
        /// </summary>
        int Size();
        /// <summary>
        /// Gets a list of all the nodes in the tree.
        /// </summary>
        List<T> AllNodes();
        /// <summary>
        /// Find the distance between two nodes
        /// </summary>
        double DistanceBetween(T ptA, T ptB);
    }
    public interface BiSearchTree<T, O> : SearchTree<T> where T : class
    {
        /// <summary>
        /// Finds the node in the graph closest to the requested point, starting at the given point
        /// and going to the nodes in the graph
        /// </summary>
        /// <param name="point">The point to find the closest node to</param>
        T ClosestStartingAt(O point);
        /// <summary>
        /// Finds the node in the graph closest to the requested point, starting at any point
        /// in the graph and going to the given node
        /// </summary>
        /// <param name="point">The point to find the closest node to</param>
        T ClosestGoingTo(O point);
    }

    public class Vector2Tree : BiSearchTree<Vector2, RobotInfo>, BiSearchTree<Vector2, Vector2>
    {
        Vector2NNFinder nnfinder = new Vector2NNFinder();
        //maps nodes->parents
        Dictionary<Vector2, Vector2> parents = new Dictionary<Vector2, Vector2>();

        int numelements = 0;

        public Vector2 ClosestStartingAt(RobotInfo point)
        {
            return nnfinder.NearestNeighbor(point.Position);
        }

        public Vector2 ClosestGoingTo(RobotInfo point)
        {
            return nnfinder.NearestNeighbor(point.Position);
        }

        public void AddNode(Vector2 node, Vector2 parent)
        {
            if (parents.ContainsKey(node))
                return;
            numelements++;
            nnfinder.AddPoint(node);
            parents.Add(node, parent);
        }

        public Vector2 ParentNode(Vector2 node)
        {
            return parents[node];
        }

        public Vector2 ClosestStartingAt(Vector2 point)
        {
            return nnfinder.NearestNeighbor(point);
        }

        public Vector2 ClosestGoingTo(Vector2 point)
        {
            return nnfinder.NearestNeighbor(point);
        }
        public int Size()
        {
            return numelements;
        }
        public List<Vector2> AllNodes()
        {
            return new List<Vector2>(parents.Keys);
        }
        public double DistanceBetween(Vector2 ptA, Vector2 ptB)
        {
            return ptA.distanceSq(ptB);
        }
    }
}
