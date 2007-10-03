using System;
using System.Collections.Generic;
using System.Text;

namespace Robocup.RRT
{
    interface RRTGraph<T>
    {
        void AddNode(T new_node);
        T GetClosest(T waypoint);
    }
}
