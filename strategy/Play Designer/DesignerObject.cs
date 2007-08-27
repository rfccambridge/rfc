using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Infrastructure;

using System.Drawing;

namespace RobocupPlays
{
    interface Clickable
    {
        bool willClick(Vector2 p);
        void highlight();
        void unhighlight();
    }
}
