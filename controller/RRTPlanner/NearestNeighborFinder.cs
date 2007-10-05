using System;
using System.Collections.Generic;
using System.Text;

namespace Robocup.RRT
{
    /// <summary>
    /// A type that can (efficiently) respond to nearest-neighbor queries
    /// </summary>
    interface NearestNeighborFinder<T>
    {
        void AddPoint(T point);
        /// <summary>
        /// Gets the "nearest" point to the given point
        /// </summary>
        /// <returns>Returns the "nearest" point.  If there are no points, returns null (or default(T) for value types).
        /// If T is a reference type, returns the same reference as was initially added.</returns>
        T NearestNeighbor(T point);
    }
}
