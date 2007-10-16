//-----------------------------------------------------------------------
//  This file is part of the Microsoft Robotics Studio Code Samples.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: DriveControl.cs $ $Revision: 7 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Drawing.Drawing2D;
using System.IO;

using Robocup.Core;
using Robocup.CoreRobotics;
using Robocup.Utilities;

namespace Robotics.ControlPanel
{
    partial class DriveControl : Form
    {

        const int NUM_BOTS = 10;
        const float DELTA = 0.005f;

        int width = 10;
        int height = 10;
        float lastfx;
        float lastfy;

        Bitmap field = new Bitmap(413, 385);

        ControlPanel control;

        public DriveControl(ControlPanel control)
        {
            this.control = control;
            InitializeComponent();
            

            

        }
        

        private void DriveControl_Load(object sender, EventArgs e)
        {
            control.OnChangeInputSource(inputYellow.Name, inputYellow.SelectedItem.ToString());
            control.OnChangeInputSource(inputBlue.Name, inputBlue.SelectedItem.ToString());
            control.OnChangeInputSource(inputBall.Name, inputBall.SelectedItem.ToString());
        }

        private void DriveControl_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

 

        public void UpdateJoystick(InputState data)
        {
            lblX.Text = data.X.ToString();
            lblY.Text = data.Y.ToString();
            lblZ.Text = data.Z.ToString();


            DrawJoystick(data.X, data.Y);

            if (!chkStop.Checked)
            {
                int left;
                int right;

                if (chkDrive.Checked == true)
                {
                    if (data.Y > -100)
                    {
                        left = data.Y + data.X / 4;
                        right = data.Y - data.X / 4;
                    }
                    else
                    {
                        left = data.Y - data.X / 4;
                        right = data.Y + data.X / 4;
                    }
                }
                else
                {
                    left = right = 0;
                }
                control.sendMove(0, left, right, left, right);
            }
        }

        private void DrawJoystick(int x, int y)
        {
            Bitmap bmp = new Bitmap(picJoystick.Width, picJoystick.Height);
            Graphics g = Graphics.FromImage(bmp);

            width = bmp.Width - 1;
            height = bmp.Height - 1;

            g.Clear(Color.Transparent);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(0, 0, width, height);

            PathGradientBrush pathBrush = new PathGradientBrush(path);
            pathBrush.CenterPoint = new PointF(width / 3f, height / 3f);
            pathBrush.CenterColor = Color.White;
            pathBrush.SurroundColors = new Color[] { Color.LightGray };

            g.FillPath(pathBrush, path);
            g.DrawPath(Pens.Black, path);

            int partial = y * height / 2200;
            if (partial > 0)
            {
                g.DrawArc(Pens.Black,
                    0,
                    height / 2 - partial,
                    width,
                    2 * partial,
                    180,
                    180);
            }
            else if (partial == 0)
            {
                g.DrawLine(Pens.Black, 0, height / 2, width, height / 2);
            }
            else
            {
                g.DrawArc(Pens.Black,
                    0,
                    height / 2 + partial,
                    width,
                    -2 * partial,
                    0,
                    180);
            }

            partial = x * width / 2200;
            if (partial > 0)
            {
                g.DrawArc(Pens.Black,
                    width / 2 - partial,
                    0,
                    2 * partial,
                    height,
                    270,
                    180);
            }
            else if (partial == 0)
            {
                g.DrawLine(Pens.Black, width / 2, 0, width / 2, height);
            }
            else
            {
                g.DrawArc(Pens.Black,
                    width / 2 + partial,
                    0,
                    -2 * partial,
                    height,
                    90,
                    180);
            }

            picJoystick.Image = bmp;
        }

        Bitmap bmp;
        int fwidth;
        int fheight;
        public void DrawField(RFCSystem rfcsystem)
        {
            float[] radius = { 0.12f, 0.09f, 0.09f, 0.09f, 0.12f };

            List<RobotInfo> ourinfo = rfcsystem.Predictor.getOurTeamInfo();
            List<RobotInfo> theirinfo = rfcsystem.Predictor.getTheirTeamInfo();

            BallInfo binfo = rfcsystem.Predictor.getBallInfo();
            double ballx = binfo.Position.X, bally = binfo.Position.Y;
            bool usBlue = Constants.get<string>("OUR_TEAM_COLOR") == "BLUE";


            //float diameter = 20.0f;
            bmp = new Bitmap(field.Width, field.Height);
            fwidth = bmp.Width - 1;
            fheight = bmp.Height - 1;

            Graphics g = Graphics.FromImage(bmp);

            g.Clear(Color.Transparent);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            g.FillRectangle(Brushes.GreenYellow, 0, 0, fwidth, fheight);
            g.FillRectangle(Brushes.Yellow,
                fwidth * (-2.63f + 2.75f) / 5.5f,
                fheight * (-0.35f + 2.0f) / 4.0f,
                fwidth * 0.18f / 5.5f,
                fheight * 0.7f / 4.0f);
            g.FillRectangle(Brushes.Blue,
                fwidth * (2.45f + 2.75f) / 5.5f,
                fheight * (-0.35f + 2.0f) / 4.0f,
                fwidth * 0.18f / 5.5f,
                fheight * 0.7f / 4.0f);

            PointF[] poly = new PointF[5];

            foreach (RobotInfo info in ourinfo)
            {
                double x = info.Position.X;
                double y = info.Position.Y;
                double orientation = info.Orientation;
                for (int j = 0; j < 5; j++)
                {
                    poly[j] = new PointF(
                        (float)(fwidth * ((x + radius[j] * Math.Cos(orientation + (36.0f + 72.0f * j) * Math.PI / 180.0f) + 2.75f) / 5.5f)),
                        (float)(fheight * (1 - (y + radius[j] * Math.Sin(orientation + (36.0f + 72.0f * j) * Math.PI / 180.0f) + 2.0f) / 4.0f))
                        );

                }
                g.FillPolygon(usBlue ? Brushes.Blue : Brushes.Yellow, poly);
                g.DrawString(info.ID.ToString(), new Font(FontFamily.GenericMonospace, 11f), Brushes.Black,
                    new PointF((float)((x + 2.75f) / 5.5f * fwidth), (float)(fheight * (1 - (y + 2.0f) / 4.0f))));
            }
            foreach (RobotInfo info in theirinfo)
            {
                double x = info.Position.X;
                double y = info.Position.Y;
                double orientation = info.Orientation;
                for (int j = 0; j < 5; j++)
                {
                    poly[j] = new PointF(
                        (float)(fwidth * ((x + radius[j] * Math.Cos(orientation + (36.0f + 72.0f * j) * Math.PI / 180.0f) + 2.75f) / 5.5f)),
                        (float)(fheight * (1 - (y + radius[j] * Math.Sin(orientation + (36.0f + 72.0f * j) * Math.PI / 180.0f) + 2.0f) / 4.0f))
                        );

                }
                g.FillPolygon(usBlue ? Brushes.Yellow : Brushes.Blue, poly);
                g.DrawString(info.ID.ToString(), new Font(FontFamily.GenericMonospace, 11f), Brushes.Black,
                    new PointF((float)((x + 2.75f) / 5.5f * fwidth), (float)(fheight * (1 - (y + 2.0f) / 4.0f))));
            }

            //ball
            float ballrad = fwidth * 0.04f / 5.5f;
            g.FillEllipse(Brushes.Orange,
                (float)(fwidth * ((ballx + 2.75f) / 5.5f) - ballrad),
                (float)(fheight * (1 - (bally + 2.0f) / 4.0f) - ballrad),2 * ballrad, 2 * ballrad);

            //rfcsystem.drawCurrent(g, new BasicCoordinateConverter(fwidth, fheight));
            try
            {
                picField.Image = bmp;
            }
            catch (Exception e)
            {
                Console.WriteLine("ignoring" + e.ToString());
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
        }

        private Color LinearColor(Color nearColor, Color farColor, int nearLimit, int farLimit, int value)
        {
            if (value <= nearLimit)
            {
                return nearColor;
            }
            else if (value >= farLimit)
            {
                return farColor;
            }

            int span = farLimit - nearLimit;
            int pos = value - nearLimit;

            int r = (nearColor.R * (span - pos) + farColor.R * pos) / span;
            int g = (nearColor.G * (span - pos) + farColor.G * pos) / span;
            int b = (nearColor.B * (span - pos) + farColor.B * pos) / span;

            return Color.FromArgb(r, g, b);
        }


        private void picJoystick_MouseLeave(object sender, EventArgs e)
        {
            InputState dummy = new InputState();
            UpdateJoystick(dummy);
        }

        private void picJoystick_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int x, y;
                x = Math.Min(picJoystick.Width, Math.Max(e.X, 0));
                y = Math.Min(picJoystick.Height, Math.Max(e.Y, 0));

                x = x * 2000 / picJoystick.Width - 1000;
                y = 1000 - y * 2000 / picJoystick.Height;

                InputState dummy = new InputState();
                dummy.X = x;
                dummy.Y = y;

                UpdateJoystick(dummy);
            }
        }

        private void picJoystick_MouseUp(object sender, MouseEventArgs e)
        {
            picJoystick_MouseLeave(sender, e);
        }


        private void picField_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                float x, y;
                x = Math.Min(picField.Width, Math.Max(e.X, 0));
                y = Math.Min(picField.Height, Math.Max(e.Y, 0));

                //initialize mouse position
                /*lastfx = (float)(5.5f * (1 - x / (float)picField.Width) - 2.75f);
                lastfy = (float)(4.0f * (1 - (y/ (float)picField.Height)) - 2.0f);*/
                lastfx = (float)(5.5f * (x / (float)picField.Width) - 2.75f);
                lastfy = (float)(4.0f * (1 - (y / (float)picField.Height)) - 2.0f);

                //change motor?
            }
        }

        private void picField_MouseLeave(object sender, EventArgs e)
        {
        }

        private void picField_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {

                float x, y, fx, fy;
                x = Math.Min(picField.Width, Math.Max(e.X, 0));
                y = Math.Min(picField.Height, Math.Max(e.Y, 0));

                fx = (float)(5.5f * (x / (float)picField.Width) - 2.75f);
                fy = (float)(4.0f * (1 - (y / (float)picField.Height)) - 2.0f);
                /*fx = (float)(5.5f * (1 - x / (float)picField.Width) - 2.75f);
                fy = (float)(4.0f * (1 - (y/ (float)picField.Height)) - 2.0f);*/

                if ((lastfx - fx) * (lastfx - fx) + (lastfy - fy) * (lastfy - fy) > DELTA)
                {
                    //control.OnDrag(new Vector3(fx, 0.09f, fy));
                    lastfx = fx;
                    lastfy = fy;
                }

            }
        }

        private void picField_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {

                float x, y, fx, fy;
                x = Math.Min(picField.Width, Math.Max(e.X, 0));
                y = Math.Min(picField.Height, Math.Max(e.Y, 0));

                fx = (float)(5.5f * (x / (float)picField.Width) - 2.75f);
                fy = (float)(4.0f * (1 - (y / (float)picField.Height)) - 2.0f);
                /*fx = (float)(5.5f * (1 - x / (float)picField.Width) - 2.75f);
                fy = (float)(4.0f * (1-(y / (float)picField.Height)) - 2.0f);*/
                //Console.WriteLine("X: " + x + " Y: " + x + " x: " + fx + " y: " + fy);

                //control.OnSetPose();
            }
        }

        public void PerformedRoundTrip(bool roundTrip)
        {
            string title = roundTrip ? "Remote Drive Control" : "Remote Drive Control - Connection Down";

            if (Text != title)
            {
                Text = title;
            }
        }

        private void btnConnectRemoteVision_Click(object sender, EventArgs e)
        {
            //_eventsPort.Post(new OnConnect(this, DIRECTORY_VISION_REMOTE));
        }

        private void inputChanged(object sender, EventArgs e)
        {
            ComboBox s = (ComboBox)sender;
            //_eventsPort.Post(new OnChangeInput(s.Name, s.SelectedItem.ToString()));
        }

        private void btnConnectVision_Click(object sender, EventArgs e)
        {
            //_eventsPort.Post(new OnFindServices(this, DIRECTORY_1, DIRECTORY_2));
            if (toggleRFC.Text == "Start RFC")
                startRFC_Click(toggleRFC, null);
           // _eventsPort.Post(new OnConnect(this, DIRECTORY_VISION));
        }

        private bool isOn;
        private void startRFC_Click(object sender, EventArgs e)
        {
            isOn = ((Button)sender).Text != "Start RFC";
            if (isOn)
            { // need to turn off
                ((Button)sender).Text = "Start RFC";
                //_eventsPort.Post(new ToggleRFC(this, false));
            }
            else
            { // off now, need to turn on
                ((Button)sender).Text = "Stop RFC";
                //_eventsPort.Post(new ToggleRFC(this, true));
            }
        }

        private void buttonReloadConstants_Click(object sender, EventArgs e)
        {
            //Robocup.Constants.Constants.Load();
            //rfcsystem.initialize();
        }

        private void buttonUnusedConstants_Click(object sender, EventArgs e)
        {
            MessageBox.Show(string.Join("\n", Constants.getUnused().ToArray()));
        }


    }

    class InputState
    {
        int x;

        public int X
        {
            get { return x; }
            set { x = value; }
        }

        int y;

        public int Y
        {
            get { return y; }
            set { y = value; }
        }

        int z;

        public int Z
        {
            get { return z; }
            set { z = value; }
        }
    }


}
