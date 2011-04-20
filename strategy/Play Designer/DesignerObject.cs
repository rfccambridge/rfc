using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Geometry;

using System.Drawing;

namespace Robocup.Plays
{
    interface Clickable
    {
        bool willClick(Vector2 p);
        void highlight();
        void unhighlight();
    }
}
