using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Robocup.Utilities;
using System.Drawing.Text;
using Robocup.Core;

namespace Vision {
    public partial class FieldStateForm : Form {
        
        /* --------------------------------
         * |(3400, 4900)        (0, 4900) |
         * |                              |
         * |           TOP                |
         * |           CAM 1              |
         * |                              |
         * |                              |
         * |------------------------------|
         * |                              |
         * |            BOTTOM            |
         * |            CAM 2             |
         * |                              |
         * |(3400, 0)                (0,0)|
         * --------------------------------
         */


        private Size FIELD_SIZE = new Size(340, 490);
        private SizeF FIELD_SIZE_WORLD = new SizeF(3400, 4900);
        private int OUT_ZONE_WIDTH = 30;
        private Color FIELD_COLOR = Color.DarkGreen;
        private Color OUR_COLOR = VisionStatic.ColorClasses.COLOR_CLASSES[VisionStatic.ColorClasses.OUR_CENTER_DOT];
        private Color THEIR_COLOR = VisionStatic.ColorClasses.COLOR_CLASSES[VisionStatic.ColorClasses.THEIR_CENTER_DOT];
        private Color BALL_COLOR = VisionStatic.ColorClasses.COLOR_CLASSES[VisionStatic.ColorClasses.COLOR_BALL];

        private Bitmap _bmpField;
        private Graphics _gfxField;

        public FieldStateForm() {
            InitializeComponent();

            picField.Width = 2 * OUT_ZONE_WIDTH + FIELD_SIZE.Width;
            picField.Height = 2 * OUT_ZONE_WIDTH + FIELD_SIZE.Height;
            _bmpField = new Bitmap(2 * OUT_ZONE_WIDTH + FIELD_SIZE.Width, 2 * OUT_ZONE_WIDTH + FIELD_SIZE.Height);
            _gfxField = Graphics.FromImage(_bmpField);
            picField.BackgroundImage = _bmpField;
        }

        public void UpdateState(VisionMessage visionMessage) {
            _gfxField.Clear(FIELD_COLOR);
            DrawCoords();
            DrawLines();

            int botsOnField = 0;
            int ballsOnField = 0;
            foreach (VisionMessage.RobotData robot in visionMessage.OurRobots) {
                //if (robot != null && robot.ID >= 0) {
                DrawOurRobot(robot);
                botsOnField++;
                //}
            }
            foreach (VisionMessage.RobotData robot in visionMessage.TheirRobots) {
                //if (robot != null && robot.ID >= 0) {
                    DrawTheirRobot(robot);
                    botsOnField++;
              //  }
            }

            //if (visionMessage.Ball != null && (visionMessage.BallPosition.X != 0 && visionMessage.BallPosition.Y != 0)) {
                DrawBall(visionMessage.BallPosition);
                ballsOnField++;
           // }

           // if (ballsOnField == 0)
           //     Console.WriteLine("Warning: No balls on field!");
            //if (botsOnField == 0)
            //   Console.WriteLine("Warning: No bots on field!");

            picField.Invalidate();
        }
        private void DrawCoords()
        {
            Font font = new Font(FontFamily.GenericSansSerif, 8);
            _gfxField.DrawString("(0, 0)", font, Brushes.Black, 
                                 OUT_ZONE_WIDTH + FIELD_SIZE.Width - 40, OUT_ZONE_WIDTH + FIELD_SIZE.Height - 20);
            _gfxField.DrawString("(" + FIELD_SIZE_WORLD.Width.ToString() + ", " + FIELD_SIZE_WORLD.Height.ToString() + ")", 
                                 font, Brushes.Black, OUT_ZONE_WIDTH, OUT_ZONE_WIDTH);
        }
        private void DrawLines()
        {
            int x1, y1, x2, y2;
            int w, h;
            ScaleLocation(3400, 4900, out x1, out y1);
            _gfxField.DrawRectangle(Pens.White, new Rectangle(x1, y1, FIELD_SIZE.Width, FIELD_SIZE.Height));

            ScaleLocation(0, FIELD_SIZE_WORLD.Height / 2, out x1, out y1);
            ScaleLocation(FIELD_SIZE_WORLD.Width, FIELD_SIZE_WORLD.Height / 2, out x2, out y2);
            _gfxField.DrawLine(Pens.White, new Point(x1, y1),
                                           new Point(x2, y2));

            const int RADIUS = 500;
            ScaleLocation(FIELD_SIZE_WORLD.Width / 2 + RADIUS, FIELD_SIZE_WORLD.Height / 2 + RADIUS, out x1,out  y1);
            ScaleSize(RADIUS * 2, RADIUS * 2, out w, out h);
            _gfxField.DrawEllipse(Pens.White, new Rectangle(x1, y1, w, h));

            ScaleLocation(FIELD_SIZE_WORLD.Width / 2 + RADIUS, 0 + RADIUS, out x1, out y1);
            _gfxField.DrawArc(Pens.White, new Rectangle(x1, y1, w, h), 180, 180);

            ScaleLocation(FIELD_SIZE_WORLD.Width / 2 + RADIUS, FIELD_SIZE_WORLD.Height + RADIUS, out x1, out y1);
            _gfxField.DrawArc(Pens.White, new Rectangle(x1, y1, w, h), 0, 180);
        }
        private void DrawOurRobot(VisionMessage.RobotData robot) {
            const int DIAMETER = 20;
            int x, y;
            ScaleLocation(robot.Position.X, robot.Position.Y, out x, out y);
            Brush brush = new SolidBrush(OUR_COLOR);
            Font font = new Font(FontFamily.GenericSansSerif, 6);
            _gfxField.FillEllipse(brush, new Rectangle(x - DIAMETER / 2, y - DIAMETER / 2, DIAMETER, DIAMETER));
            _gfxField.DrawLine(new Pen(OUR_COLOR, 3), new PointF(x, y), 
                               new PointF(x + DIAMETER * (float)Math.Cos(robot.Orientation), y - DIAMETER * (float)Math.Sin(robot.Orientation)));
            _gfxField.DrawString(robot.ID.ToString(), new Font(FontFamily.GenericMonospace, 8f, FontStyle.Bold), 
                                 Brushes.DarkRed, x - DIAMETER / 3, y - DIAMETER / 3);
            _gfxField.DrawString(String.Format("({0:0.0},\n{1:0.0})", robot.Position.X, robot.Position.Y), font, Brushes.Black, x - 20, y + DIAMETER / 2);
        }

        private void DrawTheirRobot(VisionMessage.RobotData robot) {
            const int DIAMETER = 20;
            int x, y;
            Font font = new Font(FontFamily.GenericSansSerif, 6);
            ScaleLocation(robot.Position.X, robot.Position.Y, out x, out y);
            Brush brush = new SolidBrush(THEIR_COLOR);
            _gfxField.FillEllipse(brush, new Rectangle(x - DIAMETER / 2, y - DIAMETER / 2, DIAMETER, DIAMETER));
            _gfxField.DrawString(robot.ID.ToString(), new Font(FontFamily.GenericMonospace, 8f, FontStyle.Bold),
                                 Brushes.DarkRed, x - DIAMETER / 3, y - DIAMETER / 3);
            _gfxField.DrawString(String.Format("({0:0.0},\n{1:0.0})", robot.Position.X, robot.Position.Y), font, Brushes.Black, x - 20, y + DIAMETER / 2);

        }

        private void DrawBall(Vector2 ballPos) {
            const int DIAMETER = 8;
            int x, y;
            Font font = new Font(FontFamily.GenericSansSerif, 6);
            ScaleLocation(ballPos.X, ballPos.Y, out x, out y);
            _gfxField.FillEllipse(new SolidBrush(BALL_COLOR), 
                                  new Rectangle(x - DIAMETER / 2, y - DIAMETER / 2, DIAMETER, DIAMETER));
            _gfxField.DrawString(String.Format("({0:0.0},\n{1:0.0})", ballPos.X, ballPos.Y), font, Brushes.Black, x - 20, y + DIAMETER / 2);
        }

        private void ScaleLocation(double worldX, double worldY, 
                                   out int x, out int y) {
            double normedX = 1 - worldX / FIELD_SIZE_WORLD.Width;
            double normedY = 1 - worldY / FIELD_SIZE_WORLD.Height;

            x = Convert.ToInt32(OUT_ZONE_WIDTH + normedX * FIELD_SIZE.Width);
            y = Convert.ToInt32(OUT_ZONE_WIDTH + normedY * FIELD_SIZE.Height);


        }
        private void ScaleSize(double worldWidth, double worldHeight, out int width, out int height)
        {
            width = (int)(worldWidth * (FIELD_SIZE.Width / FIELD_SIZE_WORLD.Width));
            height = (int)(worldHeight * (FIELD_SIZE.Height / FIELD_SIZE_WORLD.Height));
        }

        private void FieldStateForm_FormClosing(object sender, FormClosingEventArgs e) {
            e.Cancel = true;
            Hide();
        }
    
    }
}