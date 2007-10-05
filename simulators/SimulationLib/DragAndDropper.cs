using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;

namespace Robocup.Simulation
{
    public class DragAndDropper
    {
        private class DragAndDroppable
        {
            public double radius;
            public Action<Vector2> moveIt;
            public ValueFunction<Vector2> value;
        }
        private List<DragAndDroppable> sets = new List<DragAndDroppable>();
        /// <summary>
        /// Things are checked in the order that you add them; ie if two things are clicked at once, the one that gets chosen
        /// is the one that was added first.
        /// </summary>
        public void AddDragandDrop(ValueFunction<Vector2> fuction, double radius, Action<Vector2> moveIt)
        {
            DragAndDroppable d = new DragAndDroppable();
            d.radius = radius;
            d.moveIt = moveIt;
            d.value = fuction;
            sets.Add(d);
        }

        private DragAndDroppable current = null;
        private Vector2 diff = null;
        public void MouseDown(Vector2 point)
        {
            foreach (DragAndDroppable d in sets)
            {
                if (d.value().distanceSq(point) < d.radius * d.radius)
                {
                    current = d;
                    diff = d.value() - point;
                    return;
                }
            }
        }
        /// <summary>
        /// Returns true if something was moved.
        /// </summary>
        public bool MouseMove(Vector2 point)
        {
            if (current != null)
            {
                current.moveIt(point + diff);
                return true;
            }
            return false;
        }
        public void MouseUp()
        {
            current = null;
        }
    }
}
