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
        const int BALL_SIZE = 8;
        const int GOAL_DOT_SIZE = 10;
        const double ORIENT_ARROW_LEN = 0.25;
        Color VELOCITY_ARROW_COLOR = Color.Blue;
        const double VELOCITY_ARROW_SIZE = 0.03;
        const double VELOCITY_ARROW_MIN_MAG_SQ = 0.01;
        const double VELOCITY_ARROW_LEN_SCALE = 0.5; // length of arrow = magnitude * this scaling factor
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

        public String[] playNames;


        IPredictor predictor;
        ICoordinateConverter converter;

        Dictionary<int, Arrow> _arrows;
        Object _arrowsLock = new Object();
        int _nextArrowID;

        Dictionary<int, RobotPath> _paths;
        Object _pathsLock = new object();
        int _nextPathID;
        Font _font = new Font("Tahoma", 11);

        public FieldDrawer(IPredictor predictor, ICoordinateConverter c)
            : this(predictor, c, null)
        {
            
        }
       
        public FieldDrawer(IPredictor predictor, ICoordinateConverter c, String [] playNames)
        {
            this.predictor = predictor;
            this.converter = c;
            this.playNames = playNames;

            _arrows = new Dictionary<int, Arrow>();
            _nextArrowID = 0;

            _paths = new Dictionary<int, RobotPath>();
            _nextPathID = 0;
        }
        private void drawRobotOurs(RobotInfo r, Graphics g)
        {
            // draw robot
            Brush b = new SolidBrush(Color.Black);
            Vector2 center = converter.fieldtopixelPoint(r.Position);
            g.FillEllipse(b, (float)(center.X - ROBOT_SIZE / 2), (float)(center.Y - ROBOT_SIZE / 2), (float)(ROBOT_SIZE), (float)(ROBOT_SIZE));
            b.Dispose();


            // draw velocity arrow
            if (r.Velocity.magnitudeSq() >= VELOCITY_ARROW_MIN_MAG_SQ)
            {
                Vector2 velVector = VELOCITY_ARROW_LEN_SCALE * r.Velocity.magnitudeSq() * r.Velocity.normalize();
                Arrow velArrow = new Arrow(r.Position, r.Position + velVector, VELOCITY_ARROW_COLOR, VELOCITY_ARROW_SIZE);
                velArrow.drawConvertToPixels(g, converter);
            }

            // draw kicker
            PointF[] corners = new PointF[4];
            double angle = -r.Orientation;
            corners[0] = (center + (new Vector2((double)(innerradius * Math.Cos(angle + innerangle)), (double)(innerradius * Math.Sin(angle + innerangle))))).ToPointF();
            corners[1] = (center + (new Vector2((double)(innerradius * Math.Cos(angle - innerangle)), (double)(innerradius * Math.Sin(angle - innerangle))))).ToPointF();
            corners[2] = (center + (new Vector2((double)(outerradius * Math.Cos(angle - outerangle)), (double)(outerradius * Math.Sin(angle - outerangle))))).ToPointF();
            corners[3] = (center + (new Vector2((double)(outerradius * Math.Cos(angle + outerangle)), (double)(outerradius * Math.Sin(angle + outerangle))))).ToPointF();
            b = new SolidBrush(Color.Gray);
            g.FillPolygon(b, corners);
            b.Dispose();

            // draw an arrow showing the robot orientation
            Vector2 orientVect = new Vector2(Math.Cos(r.Orientation), Math.Sin(r.Orientation));
            orientVect = orientVect.normalizeToLength(ORIENT_ARROW_LEN);
            new Arrow(r.Position, r.Position + orientVect,
                Color.Cyan, .04).drawConvertToPixels(g, converter);

            b = new SolidBrush(Color.GreenYellow);            
            g.DrawString(r.ID.ToString() + (playNames == null ? "" : ": " + playNames[r.ID]), _font, b, new PointF((float)(center.X - ROBOT_SIZE / 2), (float)(center.Y - ROBOT_SIZE / 2)));
            b.Dispose();
        }

        private void drawRobotTheirs(RobotInfo r, Graphics g)
        {
            // draw robot
            Brush b = new SolidBrush(Color.Red);
            Vector2 center = converter.fieldtopixelPoint(r.Position);
            g.FillEllipse(b, (float)(center.X - ROBOT_SIZE / 2), (float)(center.Y - ROBOT_SIZE / 2), (float)(ROBOT_SIZE), (float)(ROBOT_SIZE));
            b.Dispose();


            // draw velocity arrow
            if (r.Velocity.magnitudeSq() >= VELOCITY_ARROW_MIN_MAG_SQ)
            {
                Vector2 velVector = VELOCITY_ARROW_LEN_SCALE * r.Velocity.magnitudeSq() * r.Velocity.normalize();
                Arrow velArrow = new Arrow(r.Position, r.Position + velVector, VELOCITY_ARROW_COLOR, VELOCITY_ARROW_SIZE);
                velArrow.drawConvertToPixels(g, converter);
            }

            b = new SolidBrush(Color.GreenYellow);
            g.DrawString(r.ID.ToString() + (playNames == null ? "" : ": " + playNames[r.ID]), _font, b, new PointF((float)(center.X - ROBOT_SIZE / 2), (float)(center.Y - ROBOT_SIZE / 2)));
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
                //drawRobotOurs(r, g, Color.Black);
                drawRobotOurs(r, g);
            }
            b.Dispose();
            b = new SolidBrush(Color.Red);
            foreach (RobotInfo r in predictor.getTheirTeamInfo())
            {
                //drawRobotTheirs(r, g, Color.Red);
                drawRobotTheirs(r, g);
            }
            
            // draw arrows

            lock (_arrowsLock)
            {
                foreach (Arrow arrow in _arrows.Values)
                    arrow.drawConvertToPixels(g, converter);
            }

            // draw paths

            lock (_pathsLock)
            {
                foreach (RobotPath path in _paths.Values)
                    PathDrawing.DrawPath(path, Color.Blue, Color.Blue, g, converter);
            }

            // draw ball
            BallInfo ballInfo = predictor.getBallInfo();

            b = new SolidBrush(Color.Orange);
            g.FillEllipse(
                b,
                converter.fieldtopixelX(ballInfo.Position.X) - BALL_SIZE / 2,
                converter.fieldtopixelY(ballInfo.Position.Y) - BALL_SIZE / 2,
                BALL_SIZE,
                BALL_SIZE
            );
            b.Dispose();

            // draw ball velocity arrow
            if (ballInfo.Velocity.magnitudeSq() >= VELOCITY_ARROW_MIN_MAG_SQ)
            {
                Vector2 velVector = VELOCITY_ARROW_LEN_SCALE * ballInfo.Velocity.magnitudeSq() * ballInfo.Velocity.normalize();
                Arrow velArrow = new Arrow(ballInfo.Position, ballInfo.Position + velVector, 
                                           VELOCITY_ARROW_COLOR, VELOCITY_ARROW_SIZE);
                velArrow.drawConvertToPixels(g, converter);
            }
        }

        public int AddArrow(Arrow arrow)
        {
            int arrowID;

            lock (_arrowsLock)
            {
                _arrows.Add(_nextArrowID, arrow);
                arrowID = _nextArrowID;
                _nextArrowID++;
            }

            return arrowID;
        }

        public void ClearArrows()
        {
            lock (_arrowsLock)
            {
                _arrows.Clear();
                _nextArrowID = 0;
            }
        }

        public void RemoveArrow(int arrowID)
        {
            lock (_arrowsLock)
            {
                _arrows.Remove(arrowID);
            }
        }


        public int AddPath(RobotPath path)
        {
            int pathID;

            lock (_pathsLock)
            {
                _paths.Add(_nextPathID, path);
                pathID = _nextPathID;
                _nextPathID++;
            }

            return pathID;
        }

        public void ClearPaths()
        {
            lock (_pathsLock)
            {
                _paths.Clear();
                _nextPathID = 0;
            }
        }

        public void RemovePath(int pathID)
        {
            lock (_pathsLock)
            {
                _paths.Remove(pathID);
            }
        }


    }
}
