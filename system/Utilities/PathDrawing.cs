using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using System.Drawing;

namespace Robocup.Utilities
{
    public static class PathDrawing
    {
        static public void DrawPath(RobotPath path, Color color1, Color color2, Graphics g, ICoordinateConverter c)
        {
            Pair<List<RobotInfo>, List<Vector2>> pairPath = new Pair<List<RobotInfo>, List<Vector2>>(path.Waypoints, new List<Vector2>());
            DrawPath(pairPath, color1, color2, g, c);
        }
        static public void DrawPath(Pair<List<RobotInfo>, List<Vector2>> path, Color color1, Color color2, Graphics g, ICoordinateConverter c)
        {
            if (path == null)
                return;
            Brush b = new SolidBrush(color1);
            Pen p = new Pen(Color.Black);

            RobotInfo prev = null;
            foreach (RobotInfo info in path.First)
            {
                Vector2 v = info.Position;
                if (!(double.IsNaN(v.X) || double.IsNaN(v.Y)))
                {
                    g.FillRectangle(b, c.fieldtopixelX(v.X) - 1, c.fieldtopixelY(v.Y) - 1, 3, 3);

                    if (prev != null)
                    {
                        Vector2 v2 = prev.Position;
                        g.DrawLine(p, (float)c.fieldtopixelX(v.X), (float)c.fieldtopixelY(v.Y),
                            (float)c.fieldtopixelX(v2.X), (float)c.fieldtopixelY(v2.Y));
                    }

                    prev = info;
                }
            }

            Brush b2 = new SolidBrush(color2);
            Vector2 prevVector = null;
            foreach (Vector2 v in path.Second)
            {
                if (!(double.IsNaN(v.X) || double.IsNaN(v.Y)))
                {
                    g.FillRectangle(b2, c.fieldtopixelX(v.X) - 1, c.fieldtopixelY(v.Y) - 1, 3, 3);

                    if (prevVector != null)
                    {
                        g.DrawLine(p, (float)c.fieldtopixelX(v.X), (float)c.fieldtopixelY(v.Y),
                            (float)c.fieldtopixelX(prevVector.X), (float)c.fieldtopixelY(prevVector.Y));
                    }

                    prevVector = v;
                }
            }

            p.Dispose();
            b.Dispose();
        }
    }
}
