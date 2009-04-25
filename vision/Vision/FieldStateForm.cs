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

        // Actual field
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
        // Practice MD 3rd -- 20081206
        /* --------------------------------
 * |(4050, 6050)        (0, 6050) |
 * |                              |
 * |           TOP                |
 * |           CAM 1              |
 * |                              |
 * |                              |
 * |                     (0, 3025)|
 * |------------------------------|
 * |                              |
 * |            BOTTOM            |
 * |            CAM 2             |
 * |                              |
 * |                              |
 * |(4050, 0)                (0,0)|
 * --------------------------------
 */


        // private Size FIELD_SIZE = new Size(420, 610);
        //private SizeF FIELD_SIZE_WORLD = new SizeF(4.2f, 6.1f);

        //values for new MD field
        private Size FIELD_SIZE = new Size(405, 605);
        private SizeF FIELD_SIZE_WORLD = new SizeF(4.05f, 6.05f);

        private int OUT_ZONE_WIDTH = 30;                

        private Bitmap _bmpField;
        private Graphics _gfxField;

        public FieldStateForm() {
            InitializeComponent();

            picField.Width = 2 * OUT_ZONE_WIDTH + FIELD_SIZE.Width;
            picField.Height = 2 * OUT_ZONE_WIDTH + FIELD_SIZE.Height;
            _bmpField = new Bitmap(2 * OUT_ZONE_WIDTH + FIELD_SIZE.Width, 2 * OUT_ZONE_WIDTH + FIELD_SIZE.Height);
            _gfxField = Graphics.FromImage(_bmpField);
            picField.BackgroundImage = _bmpField;

            this.Width = 3 * OUT_ZONE_WIDTH + FIELD_SIZE.Width;
            this.Height = 3 * OUT_ZONE_WIDTH + FIELD_SIZE.Height;

        }

        public Graphics getGraphics() {
            return _gfxField;
        }

        public void UpdateState(VisionMessage visionMessage) {
            _gfxField.Clear(Color.DarkGreen);
            DrawCoords();
            DrawLines();

            int botsOnField = 0;
            int ballsOnField = 0;

            foreach (VisionMessage.RobotData robot in visionMessage.Robots) {                
                DrawRobot(robot);
                botsOnField++;                
            }
            
            if (visionMessage.Ball != null && visionMessage.Ball.Position != null &&
                (visionMessage.Ball.Position.X != 0 && visionMessage.Ball.Position.Y != 0)) {
                DrawBall(visionMessage.Ball.Position);
                ballsOnField++;              
            }

            // if (ballsOnField == 0)
            //     Console.WriteLine("Warning: No balls on field!");
            //if (botsOnField == 0)
            //   Console.WriteLine("Warning: No bots on field!");

            picField.Invalidate();

        } 

        private void DrawCoords() {
            Font font = new Font(FontFamily.GenericSansSerif, 8);
            _gfxField.DrawString("(0, 0)", font, Brushes.Black,
                                 OUT_ZONE_WIDTH + FIELD_SIZE.Width - 40, OUT_ZONE_WIDTH + FIELD_SIZE.Height - 20);
            _gfxField.DrawString("(" + FIELD_SIZE_WORLD.Width.ToString() + ", " + FIELD_SIZE_WORLD.Height.ToString() + ")",
                                 font, Brushes.Black, OUT_ZONE_WIDTH, OUT_ZONE_WIDTH);
        }
        private void DrawLines() {
            int x1, y1, x2, y2;
            int w, h;

            // outline
            ScaleLocation(-FIELD_SIZE_WORLD.Width / 2, FIELD_SIZE_WORLD.Height / 2, out x1, out y1);
            _gfxField.DrawRectangle(Pens.White, new Rectangle(x1, y1, FIELD_SIZE.Width, FIELD_SIZE.Height));

            // midfield
            ScaleLocation(-FIELD_SIZE_WORLD.Width / 2, 0, out x1, out y1);
            ScaleLocation(FIELD_SIZE_WORLD.Width / 2, 0, out x2, out y2);
            _gfxField.DrawLine(Pens.White, new Point(x1, y1),
                                           new Point(x2, y2));

            const float RADIUS = 0.25f;
            // center ring
            ScaleLocation(0 - RADIUS, 0 + RADIUS, out x1, out  y1);
            ScaleSize(RADIUS * 2, RADIUS * 2, out w, out h);
            _gfxField.DrawEllipse(Pens.White, new Rectangle(x1, y1, w, h));

            // top goal
            ScaleLocation(0 - RADIUS, FIELD_SIZE_WORLD.Height / 2 + RADIUS, out x1, out y1);
            _gfxField.DrawArc(Pens.White, new Rectangle(x1, y1, w, h), 180, 180);

            // bottom goal
            ScaleLocation(0 - RADIUS, -FIELD_SIZE_WORLD.Height / 2 + RADIUS, out x1, out y1);
            _gfxField.DrawArc(Pens.White, new Rectangle(x1, y1, w, h), 0, 180);
        }
        private void DrawRobot(VisionMessage.RobotData robot) {
            const int DIAMETER = 20;
            int x, y;
            Brush brush;
            Pen pen;            
            Font font = new Font(FontFamily.GenericSansSerif, 6);

            // Pixel coords
            StandardToPixelScale(robot.Position.X, robot.Position.Y, out x, out y);

            // Brush for drawing the robot circle and pen for drawing the orientation indicator            
            if (robot.Team == VisionMessage.Team.BLUE) {
                brush = new SolidBrush(Color.Blue);
                pen = new Pen(Color.Blue);
            }
            else if (robot.Team == VisionMessage.Team.YELLOW) {
                brush = new SolidBrush(Color.Yellow);
                pen = new Pen(Color.Yellow);
            } else {
                brush = new SolidBrush(Color.Gray);
                pen = new Pen(Color.Gray);
            }
                      
            _gfxField.FillEllipse(brush, new Rectangle(x - DIAMETER / 2, y - DIAMETER / 2, DIAMETER, DIAMETER));
            _gfxField.DrawLine(pen, new PointF(x, y),
                               new PointF(x + DIAMETER * (float)Math.Cos(robot.Orientation + Math.PI / 2), y - DIAMETER * (float)Math.Sin(robot.Orientation + Math.PI / 2)));
            _gfxField.DrawString(robot.ID.ToString(), new Font(FontFamily.GenericMonospace, 8f, FontStyle.Bold),
                                 Brushes.DarkRed, x - DIAMETER / 3, y - DIAMETER / 3);
            _gfxField.DrawString(String.Format("({0:0.0},\n{1:0.0})", robot.Position.X, robot.Position.Y), font, Brushes.Black, x - 20, y + DIAMETER / 2);

        }
        private void DrawBall(Vector2 ballPos) {
            if (double.IsNaN(ballPos.X) || double.IsNaN(ballPos.Y)) return;
            const int DIAMETER = 8;
            int x, y;
            Font font = new Font(FontFamily.GenericSansSerif, 6);
            StandardToPixelScale(ballPos.X, ballPos.Y, out x, out y);
            _gfxField.FillEllipse(new SolidBrush(Color.Orange),
                                  new Rectangle(x - DIAMETER / 2, y - DIAMETER / 2, DIAMETER, DIAMETER));
            _gfxField.DrawString(String.Format("({0:0.0},\n{1:0.0})", ballPos.X, ballPos.Y), font, Brushes.Black, x - 20, y + DIAMETER / 2);
        }

        private void ScaleLocation(double worldX, double worldY,
                                   out int x, out int y) {
            double normedX = (FIELD_SIZE_WORLD.Width / 2 + worldX) / FIELD_SIZE_WORLD.Width;
            double normedY = (FIELD_SIZE_WORLD.Height / 2 - worldY) / FIELD_SIZE_WORLD.Height;

            x = Convert.ToInt32(OUT_ZONE_WIDTH + normedX * FIELD_SIZE.Width);
            y = Convert.ToInt32(OUT_ZONE_WIDTH + normedY * FIELD_SIZE.Height);
        }
        private void StandardToPixelScale(double worldX, double worldY, out int x, out int y) {
            double normedX = (FIELD_SIZE_WORLD.Width / 2 - worldY) / FIELD_SIZE_WORLD.Width;
            double normedY = (FIELD_SIZE_WORLD.Height / 2 - worldX) / FIELD_SIZE_WORLD.Height;

            x = Convert.ToInt32(OUT_ZONE_WIDTH + normedX * FIELD_SIZE.Width);
            y = Convert.ToInt32(OUT_ZONE_WIDTH + normedY * FIELD_SIZE.Height);
        }
        private void ScaleSize(double worldWidth, double worldHeight, out int width, out int height) {
            width = (int)(worldWidth * (FIELD_SIZE.Width / FIELD_SIZE_WORLD.Width));
            height = (int)(worldHeight * (FIELD_SIZE.Height / FIELD_SIZE_WORLD.Height));
        }

        private void FieldStateForm_FormClosing(object sender, FormClosingEventArgs e) {
            e.Cancel = true;
            Hide();
        }

    }
}