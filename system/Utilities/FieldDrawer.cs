using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Robocup.Core;

namespace Robocup.Utilities
{
    public class FieldDrawer
    {
        // For internal use only
        private enum Team { BLUE, YELLOW };

        private Team OUR_TEAM, THEIR_TEAM;
        

        // drawing constants
        int ROBOT_SIZE;
        int BALL_SIZE;
        int GOAL_DOT_SIZE;
        double ORIENT_ARROW_LEN;
        Color VELOCITY_ARROW_COLOR;
        double VELOCITY_ARROW_SIZE;
        double VELOCITY_ARROW_MIN_MAG_SQ;
        double VELOCITY_ARROW_LEN_SCALE; // length of arrow = magnitude * this scaling factor
        double BALL_VELOCITY_ARROW_LEN_SCALE;
        
        // kicker drawing
        double OUTER_ANGLE = .6;
        double INNER_ANGLE = 1.0;
        double INNER_RADIUS = 7;
        double OUTER_RADIUS = 11;

        // field drawing
        static double FIELD_WIDTH;
        static double FIELD_HEIGHT;
        static double FIELD_XMIN;
        static double FIELD_XMAX;
        static double FIELD_YMIN;
        static double FIELD_YMAX;
        static double GOAL_WIDTH;
        static double GOAL_HEIGHT;

        public Dictionary<int, string> ourPlayNames;
        public Dictionary<int, string> theirPlayNames;


        IPredictor predictor;
        ICoordinateConverter converter;
        PlayTypes playType;

        Dictionary<int, Arrow> _arrows;
        Object _arrowsLock = new Object();
        int _nextArrowID;

        Dictionary<int, RobotPath> _paths;
        Object _pathsLock = new object();
        int _nextPathID;
        Font _font = new Font("Tahoma", 11);
      
        public FieldDrawer(IPredictor predictor, ICoordinateConverter c)
        {
            this.predictor = predictor;
            this.converter = c;
            this.ourPlayNames = new Dictionary<int, string>();//ourPlayNames;
            this.theirPlayNames = new Dictionary<int, string>();//theirPlayNames;


            _arrows = new Dictionary<int, Arrow>();
            _nextArrowID = 0;

            _paths = new Dictionary<int, RobotPath>();
            _nextPathID = 0;

            playType = PlayTypes.Stopped;

            LoadParameters();
        }

        public void LoadParameters()
        {
            OUR_TEAM = Constants.get<string>("configuration", "OUR_TEAM") == "YELLOW" ? Team.YELLOW : Team.BLUE;
            THEIR_TEAM = Constants.get<string>("configuration", "OUR_TEAM") == "YELLOW" ? Team.BLUE : Team.YELLOW;

            // drawing constants
            ROBOT_SIZE =                Constants.get<int>("drawing", "ROBOT_SIZE");
            BALL_SIZE =                 Constants.get<int>("drawing", "BALL_SIZE");
            GOAL_DOT_SIZE =             Constants.get<int>("drawing", "GOAL_DOT_SIZE");
            ORIENT_ARROW_LEN =          Constants.get<double>("drawing", "ORIENT_ARROW_LEN");
            VELOCITY_ARROW_COLOR =      Color.FromName(Constants.get<string>("drawing", "VELOCITY_ARROW_COLOR"));
            VELOCITY_ARROW_SIZE =       Constants.get<double>("drawing", "VELOCITY_ARROW_SIZE");
            VELOCITY_ARROW_MIN_MAG_SQ = Constants.get<double>("drawing", "VELOCITY_ARROW_MIN_MAG_SQ");
            VELOCITY_ARROW_LEN_SCALE =  Constants.get<double>("drawing", "VELOCITY_ARROW_LEN_SCALE");
            BALL_VELOCITY_ARROW_LEN_SCALE = Constants.get<double>("drawing", "BALL_VELOCITY_ARROW_LEN_SCALE");

            // kicker drawing
            OUTER_ANGLE =  Constants.get<double>("drawing", "OUTER_ANGLE");
            INNER_ANGLE =  Constants.get<double>("drawing", "INNER_ANGLE");
            INNER_RADIUS = Constants.get<double>("drawing", "INNER_RADIUS");
            OUTER_RADIUS = Constants.get<double>("drawing", "OUTER_RADIUS");
            
            // field drawing
            FIELD_WIDTH = Constants.get<double>("plays", "FIELD_WIDTH");
            FIELD_HEIGHT = Constants.get<double>("plays", "FIELD_HEIGHT");
            
            FIELD_XMIN = -FIELD_WIDTH / 2;
            FIELD_XMAX = FIELD_WIDTH / 2;
            FIELD_YMIN = -FIELD_HEIGHT / 2;
            FIELD_YMAX = FIELD_HEIGHT/2;

            GOAL_WIDTH = Constants.get<double>("plays","GOAL_WIDTH");
            GOAL_HEIGHT = Constants.get<double>("plays", "GOAL_HEIGHT");
        }

        public void SetPlayType(PlayTypes newPlayType) {
            playType = newPlayType;
        }

        private void drawRobot(RobotInfo r, Color color, Graphics g)
        {
            // draw robot
            Brush b = new SolidBrush(color);
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
            corners[0] = (center + (new Vector2((double)(INNER_RADIUS * Math.Cos(angle + INNER_ANGLE)), (double)(INNER_RADIUS * Math.Sin(angle + INNER_ANGLE))))).ToPointF();
            corners[1] = (center + (new Vector2((double)(INNER_RADIUS * Math.Cos(angle - INNER_ANGLE)), (double)(INNER_RADIUS * Math.Sin(angle - INNER_ANGLE))))).ToPointF();
            corners[2] = (center + (new Vector2((double)(OUTER_RADIUS * Math.Cos(angle - OUTER_ANGLE)), (double)(OUTER_RADIUS * Math.Sin(angle - OUTER_ANGLE))))).ToPointF();
            corners[3] = (center + (new Vector2((double)(OUTER_RADIUS * Math.Cos(angle + OUTER_ANGLE)), (double)(OUTER_RADIUS * Math.Sin(angle + OUTER_ANGLE))))).ToPointF();
            b = new SolidBrush(Color.Gray);
            g.FillPolygon(b, corners);
            b.Dispose();

            // draw an arrow showing the robot orientation
            Vector2 orientVect = new Vector2(Math.Cos(r.Orientation), Math.Sin(r.Orientation));
            orientVect = orientVect.normalizeToLength(ORIENT_ARROW_LEN);
            new Arrow(r.Position, r.Position + orientVect,
                Color.Cyan, .04).drawConvertToPixels(g, converter);

            b = new SolidBrush(Color.GreenYellow);
            string playName;
            Dictionary<int, string> playNames;
            if ((OUR_TEAM == Team.YELLOW && r.Team == 0) || (OUR_TEAM == Team.BLUE && r.Team == 1))
            {
                playNames = ourPlayNames;
            }
            else
            {
                playNames = theirPlayNames;
            }
            if (ourPlayNames.TryGetValue(r.ID, out playName)) {
                g.DrawString(r.ID.ToString() + playName, _font, b, new PointF((float)(center.X - ROBOT_SIZE / 2), (float)(center.Y - ROBOT_SIZE / 2)));
            }
            b.Dispose();
        }

        public void paintField(Graphics g)
        {
            Color ourColor = (OUR_TEAM == Team.YELLOW) ? Color.Yellow : Color.Blue;
            Color theirColor = (OUR_TEAM == Team.YELLOW) ? Color.Blue : Color.Yellow;            

            // Team color information
            g.DrawString(OUR_TEAM.ToString() + " PLAYER", _font, new SolidBrush(ourColor),
                         converter.fieldtopixelX(FIELD_XMIN) - 5, converter.fieldtopixelY(FIELD_YMIN) + 40);

                // Game state
            g.DrawString("GAME STATE: " + playType.ToString(), _font, new SolidBrush(ourColor),
                            converter.fieldtopixelX(FIELD_XMIN) + 120, converter.fieldtopixelY(FIELD_YMIN) + 40);


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

            List<RobotInfo> ourInfos = predictor.getOurTeamInfo();
            foreach (RobotInfo r in ourInfos)
            {
                //drawRobotOurs(r, g, Color.Black);
                drawRobot(r, ourColor, g);
            }
            
            List<RobotInfo> theirInfos = predictor.getTheirTeamInfo();
            foreach (RobotInfo r in theirInfos)
            {
                //drawRobotTheirs(r, g, Color.Red);
                drawRobot(r, theirColor, g);
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

            if (ballInfo != null) {

                Brush b = new SolidBrush(Color.Orange);
                g.FillEllipse(
                    b,
                    converter.fieldtopixelX(ballInfo.Position.X) - BALL_SIZE / 2,
                    converter.fieldtopixelY(ballInfo.Position.Y) - BALL_SIZE / 2,
                    BALL_SIZE,
                    BALL_SIZE
                );
                b.Dispose();

                // draw ball velocity arrow
                if (ballInfo.Velocity.magnitudeSq() >= VELOCITY_ARROW_MIN_MAG_SQ) {
                    Vector2 velVector = BALL_VELOCITY_ARROW_LEN_SCALE * ballInfo.Velocity.magnitudeSq() * ballInfo.Velocity.normalize();
                    Arrow velArrow = new Arrow(ballInfo.Position, ballInfo.Position + velVector,
                                               VELOCITY_ARROW_COLOR, VELOCITY_ARROW_SIZE);
                    velArrow.drawConvertToPixels(g, converter);
                }
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
