using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Robocup.Core;

namespace Robocup.Utilities
{
    public class FieldDrawer
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
        static double FIELD_WIDTH = Constants.get<double>("plays", "FIELD_WIDTH");
        static double FIELD_HEIGHT = Constants.get<double>("plays", "FIELD_HEIGHT");
        static double FIELD_XMIN = -FIELD_WIDTH / 2;
        static double FIELD_XMAX = FIELD_WIDTH / 2;
        static double FIELD_YMIN = -FIELD_HEIGHT / 2;
        static double FIELD_YMAX = FIELD_HEIGHT/2;
        static double GOAL_WIDTH = Constants.get<double>("plays","GOAL_WIDTH");
        static double GOAL_HEIGHT = Constants.get<double>("plays", "GOAL_HEIGHT");


        IPredictor predictor;
        ICoordinateConverter converter;
        public FieldDrawer(IPredictor predictor, ICoordinateConverter c)
        {
            this.predictor = predictor;
            this.converter = c;
        }
        private void drawRobot(RobotInfo r, Graphics g, Color c)
        {
            // draw robot
            Brush b = new SolidBrush(c);
            Vector2 center = converter.fieldtopixelPoint(r.Position);
            g.FillEllipse(b, (float)(center.X - ROBOT_SIZE / 2), (float)(center.Y - ROBOT_SIZE / 2), (float)(ROBOT_SIZE), (float)(ROBOT_SIZE));
            new Arrow(r.Position, r.Position + r.Velocity, Color.Blue, .03).drawConvertToPixels(g, converter);

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
            g.FillEllipse(b0, converter.fieldtopixelX(FIELD_XMIN - GOAL_WIDTH) - GOAL_DOT_SIZE / 2, converter.fieldtopixelY(0) - GOAL_DOT_SIZE / 2, GOAL_DOT_SIZE, GOAL_DOT_SIZE);
            g.FillEllipse(b0, converter.fieldtopixelX(FIELD_XMAX + GOAL_WIDTH) - GOAL_DOT_SIZE / 2, converter.fieldtopixelY(0) - GOAL_DOT_SIZE / 2, GOAL_DOT_SIZE, GOAL_DOT_SIZE);
            b0.Dispose();

            Pen p = new Pen(Color.Black, 3);
            // right goal box 
            g.DrawRectangle(
                p,
                converter.fieldtopixelX(FIELD_XMAX),
                converter.fieldtopixelY(GOAL_HEIGHT / 2),
                converter.fieldtopixelX(FIELD_XMAX + GOAL_WIDTH) - converter.fieldtopixelX(FIELD_XMAX),
                converter.fieldtopixelY(-GOAL_HEIGHT / 2) - converter.fieldtopixelY(GOAL_HEIGHT / 2)
            );
            // left goal box
            g.DrawRectangle(
                p,
                converter.fieldtopixelX(FIELD_XMIN - GOAL_WIDTH),
                converter.fieldtopixelY(GOAL_HEIGHT / 2),
                converter.fieldtopixelX(FIELD_XMAX + GOAL_WIDTH) - converter.fieldtopixelX(FIELD_XMAX),
                converter.fieldtopixelY(-GOAL_HEIGHT / 2) - converter.fieldtopixelY(GOAL_HEIGHT / 2)
            );
            // field rectangle
            g.DrawRectangle(
                p,
                converter.fieldtopixelX(FIELD_XMIN),
                converter.fieldtopixelY(FIELD_YMAX),
                converter.fieldtopixelX(FIELD_XMAX) - converter.fieldtopixelX(FIELD_XMIN),
                converter.fieldtopixelY(FIELD_YMIN) - converter.fieldtopixelY(FIELD_YMAX)
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
                converter.fieldtopixelX(predictor.getBallInfo().Position.X) - BALL_SIZE / 2,
                converter.fieldtopixelY(predictor.getBallInfo().Position.Y) - BALL_SIZE / 2,
                BALL_SIZE,
                BALL_SIZE
            );

        }
    }
}
