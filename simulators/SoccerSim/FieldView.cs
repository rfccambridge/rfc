using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using Robocup.Core;
using Robocup.Utilities;

namespace SoccerSim
{
    class FieldView : ICoordinateConverter
    {
        // drawing constants
        const int ROBOT_SIZE = 20;
        const int BALL_SIZE = 6;
        const int GOAL_DOT_SIZE = 10;
        // kicker drawing
        const double outerangle = .6;
        const double innerangle = 1.0;
        const double innerradius = 7;
        const double outerradius = 11;
        // field drawing
        const double FIELD_XMIN = -2.45;
        const double FIELD_XMAX = 2.45;
        const double FIELD_YMIN = -1.7;
        const double FIELD_YMAX = 1.7;
        const double GOAL_WIDTH = 0.18;
        const double GOAL_HEIGHT = 0.7;


        IPredictor predictor;
        public FieldView(IPredictor predictor)
        {
            this.predictor = predictor;
        }

        #region Drawing Commands
        private void drawRobot(RobotInfo r, Graphics g, Color c)
        {
            // draw robot
            Brush b = new SolidBrush(c);
            Vector2 center = fieldtopixelPoint(r.Position);
            g.FillEllipse(b, (float)(center.X - ROBOT_SIZE / 2), (float)(center.Y - ROBOT_SIZE / 2), (float)(ROBOT_SIZE), (float)(ROBOT_SIZE));

            // draw kicker
            PointF[] corners = new PointF[4];
            double angle = -r.Orientation;
            corners[0] = (center + (new Vector2((double)(innerradius * Math.Cos(angle + innerangle)), (double)(innerradius * Math.Sin(angle + innerangle))))).ToPointF();
            corners[1] = (center + (new Vector2((double)(innerradius * Math.Cos(angle - innerangle)), (double)(innerradius * Math.Sin(angle - innerangle))))).ToPointF();
            corners[2] = (center + (new Vector2((double)(outerradius * Math.Cos(angle - outerangle)), (double)(outerradius * Math.Sin(angle - outerangle))))).ToPointF();
            corners[3] = (center + (new Vector2((double)(outerradius * Math.Cos(angle + outerangle)), (double)(outerradius * Math.Sin(angle + outerangle))))).ToPointF();
            Brush b2 = new SolidBrush(Color.Gray);
            g.FillPolygon(b2, corners);
            b2.Dispose();
            b.Dispose();
        }

        public void paintField(Graphics g)
        {
            // goal dots
            Brush b0 = new SolidBrush(Color.YellowGreen);
            g.FillEllipse(b0, fieldtopixelX(-2.53) - GOAL_DOT_SIZE / 2, fieldtopixelY(0) - GOAL_DOT_SIZE / 2, GOAL_DOT_SIZE, GOAL_DOT_SIZE);
            g.FillEllipse(b0, fieldtopixelX(+2.53) - GOAL_DOT_SIZE / 2, fieldtopixelY(0) - GOAL_DOT_SIZE / 2, GOAL_DOT_SIZE, GOAL_DOT_SIZE);
            b0.Dispose();

            Pen p = new Pen(Color.Black, 3);
            // right goal box 
            g.DrawRectangle(
                p,
                fieldtopixelX(FIELD_XMAX),
                fieldtopixelY(GOAL_HEIGHT / 2),
                fieldtopixelX(FIELD_XMAX + GOAL_WIDTH) - fieldtopixelX(FIELD_XMAX),
                fieldtopixelY(-GOAL_HEIGHT / 2) - fieldtopixelY(GOAL_HEIGHT / 2)
            );
            // left goal box
            g.DrawRectangle(
                p,
                fieldtopixelX(FIELD_XMIN - GOAL_WIDTH),
                fieldtopixelY(GOAL_HEIGHT / 2),
                fieldtopixelX(FIELD_XMAX + GOAL_WIDTH) - fieldtopixelX(FIELD_XMAX),
                fieldtopixelY(-GOAL_HEIGHT / 2) - fieldtopixelY(GOAL_HEIGHT / 2)
            );
            // field rectangle
            g.DrawRectangle(
                p,
                fieldtopixelX(FIELD_XMIN),
                fieldtopixelY(FIELD_YMAX),
                fieldtopixelX(FIELD_XMAX) - fieldtopixelX(FIELD_XMIN),
                fieldtopixelY(FIELD_YMIN) - fieldtopixelY(FIELD_YMAX)
            );
            p.Dispose();
            Brush b = new SolidBrush(Color.Black);
            foreach (RobotInfo r in predictor.getOurTeamInfo())
            {
                drawRobot(r, g, Color.Black);
            }
            b.Dispose();
            b = new SolidBrush(Color.Red);
            foreach (RobotInfo r in predictor.getTheirTeamInfo())
            {
                drawRobot(r, g, Color.Red);
            }


            // draw ball
            b.Dispose();
            b = new SolidBrush(Color.Orange);
            g.FillEllipse(
                b,
                fieldtopixelX(predictor.getBallInfo().Position.X) - BALL_SIZE / 2,
                fieldtopixelY(predictor.getBallInfo().Position.Y) - BALL_SIZE / 2,
                BALL_SIZE,
                BALL_SIZE
            );

        }

        /*List<Arrow> arrows = new List<Arrow>();
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
        }*/

        #endregion
        #region Coordinate Conversions

        const double PIXELSPERMETER = 120.0;
        const double CENTER_X = 350.0;
        const double CENTER_Y = 250.0;
        public int fieldtopixelX(double x)
        {
            return (int)(CENTER_X + PIXELSPERMETER * x);
        }
        public int fieldtopixelY(double y)
        {
            return (int)(CENTER_Y - PIXELSPERMETER * y);
        }
        public Vector2 fieldtopixelPoint(Vector2 p)
        {
            return new Vector2(fieldtopixelX(p.X), fieldtopixelY(p.Y));
        }
        public double pixeltofieldX(double x)
        {
            return (double)((x - CENTER_X) / PIXELSPERMETER);
        }
        public double pixeltofieldY(double y)
        {
            return (double)((y - CENTER_Y) / -PIXELSPERMETER);
        }
        public Vector2 pixeltofieldPoint(Vector2 p)
        {
            return new Vector2(pixeltofieldX(p.X), pixeltofieldY(p.Y));
        }
        public double fieldtopixelDistance(double f)
        {
            return f * PIXELSPERMETER;
        }
        public double pixeltofieldDistance(double f)
        {
            return f / PIXELSPERMETER;
        }
        #endregion
    }
}
