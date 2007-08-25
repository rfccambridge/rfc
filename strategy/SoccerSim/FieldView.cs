using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using Robocup.Infrastructure;

namespace SoccerSim
{
    public class FieldView
    {
        FieldState _state;
        public FieldView(FieldState state)
        {
            _state = state;
        }

        #region Drawing Commands
        private void drawRobot(RobotInfo r, Graphics g, Color c)
        {
            Brush b = new SolidBrush(c);
            Vector2 center = fieldtopixelPoint(r.Position);
            g.FillEllipse(b, center.X - 10, center.Y - 10, 20, 20);
            PointF[] corners = new PointF[4];
            float angle = -r.Orientation;
            float outerangle = .6f;
            float innerangle = 1.0f;
            float innerradius = 7f;
            float outerradius = 11f;
            corners[0] = (PointF)(center + (new Vector2((float)(innerradius * Math.Cos(angle + innerangle)), (float)(innerradius * Math.Sin(angle + innerangle)))));
            corners[1] = (PointF)(center + (new Vector2((float)(innerradius * Math.Cos(angle - innerangle)), (float)(innerradius * Math.Sin(angle - innerangle)))));
            corners[2] = (PointF)(center + (new Vector2((float)(outerradius * Math.Cos(angle - outerangle)), (float)(outerradius * Math.Sin(angle - outerangle)))));
            corners[3] = (PointF)(center + (new Vector2((float)(outerradius * Math.Cos(angle + outerangle)), (float)(outerradius * Math.Sin(angle + outerangle)))));
            Brush b2 = new SolidBrush(Color.Gray);
            g.FillPolygon(b2, corners);
            b2.Dispose();
            b.Dispose();
        }
        
        public void paintField(Graphics g)
        {
            Brush b0 = new SolidBrush(Color.YellowGreen);
            g.FillEllipse(b0, fieldtopixelX(-2.53) - 5, fieldtopixelY(0) - 5, 10, 10);
            g.FillEllipse(b0, fieldtopixelX(+2.53) - 5, fieldtopixelY(0) - 5, 10, 10);
            b0.Dispose();
            Pen p = new Pen(Color.Black, 3);
            g.DrawRectangle(p, fieldtopixelX(2.45), fieldtopixelY(.35), fieldtopixelX(2.63) - fieldtopixelX(2.45), fieldtopixelY(-.35) - fieldtopixelY(.35));
            g.DrawRectangle(p, fieldtopixelX(-2.63), fieldtopixelY(.35), fieldtopixelX(-2.45) - fieldtopixelX(-2.63), fieldtopixelY(-.35) - fieldtopixelY(.35));
            g.DrawRectangle(p, fieldtopixelX(-2.45), fieldtopixelY(1.7), fieldtopixelX(2.45) - fieldtopixelX(-2.45), fieldtopixelY(-1.7) - fieldtopixelY(1.7));
            p.Dispose();
            Brush b = new SolidBrush(Color.Black);
            foreach (RobotInfo r in _state.getOurTeamInfo())
            {
                drawRobot(r, g, Color.Black);
                //g.FillEllipse(b, fieldtopixelX(r.Position.X) - 10, fieldtopixelY(r.Position.Y) - 10, 20, 20);
            }
            b.Dispose();
            b = new SolidBrush(Color.Red);
            foreach (RobotInfo r in _state.getTheirTeamInfo())
            {
                drawRobot(r, g, Color.Red);
                //g.FillEllipse(b, fieldtopixelX(r.Position.X) - 10, fieldtopixelY(r.Position.Y) - 10, 20, 20);
            }
            

            // draw ball
            b.Dispose();
            b = new SolidBrush(Color.Orange);
            g.FillEllipse(
                b, 
                fieldtopixelX(_state.getBallInfo().Position.X) - 3, 
                fieldtopixelY(_state.getBallInfo().Position.Y) - 3, 
                6, 
                6
            );

        }

        List<Arrow> arrows = new List<Arrow>();
        public void addArrow(Arrow a)
        {
            lock (arrows)
            {
                arrows.Add(a);
            }
        }

        public void paintArrows(Graphics g)
        {
            lock (arrows)
            {
                foreach (Arrow a in arrows)
                {
                    a.draw(g);
                }
            }
        }

        public void clearArrows()
        {
            lock (arrows)
            {
                arrows.Clear();
            }
        }

        #endregion
        #region Coordinate Conversions
        private int fieldtopixelX(double x)
        {
            return (int)(300 + 100 * x);
        }
        private int fieldtopixelY(double y)
        {
            return (int)(200 - 100 * y);
        }
        public Vector2 fieldtopixelPoint(Vector2 p)
        {
            return new Vector2(fieldtopixelX(p.X), fieldtopixelY(p.Y));
        }
        private float pixeltofieldX(float x)
        {
            return (float)((x - 300f) / 100f);
        }
        private float pixeltofieldY(float y)
        {
            return (float)((y - 200f) / -100f);
        }
        private Vector2 pixeltofieldPoint(Vector2 p)
        {
            return new Vector2(pixeltofieldX(p.X), pixeltofieldY(p.Y));
        }
        #endregion
    }
}
