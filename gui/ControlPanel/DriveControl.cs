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
using Microsoft.Ccr.Core;
//using joystick = Microsoft.Robotics.Services.Samples.Drivers.Joystick.Proxy;
//using joystick = Microsoft.Robotics.Services.Samples.Drivers.Gamepad.Proxy;
//using drive = Microsoft.Robotics.Services.Drive.Proxy;
using drive = Robotics.SimDrive.Proxy;
using arm = Microsoft.Robotics.Services.ArticulatedArm.Proxy;
using sicklrf = Microsoft.Robotics.Services.Sensors.SickLRF.Proxy;
using cs = Microsoft.Dss.Services.Constructor;
using Microsoft.Dss.ServiceModel.Dssp;
using System.Drawing.Drawing2D;
using System.IO;
using Microsoft.Dss.Core;
using Microsoft.Robotics.Simulation.Physics.Proxy;
using Microsoft.Robotics.PhysicalModel.Proxy;
using Microsoft.Dss.Services.SubscriptionManager;
using Robocup.Infrastructure;
using Robocup.CoreRobotics;
using Vector2 = Robocup.Infrastructure.Vector2;

namespace Robotics.ControlPanel
{
    partial class DriveControl : Form
    {


        DriveControlEvents _eventsPort;

        string DIRECTORY_SIM;
        string DIRECTORY_VISION;
        string DIRECTORY_VISION_REMOTE;

        const int NUM_BOTS = 10;
        const float DELTA = 0.005f;

        int width = 10;
        int height = 10;
        float lastfx;
        float lastfy;

        Bitmap field = Robotics.ControlPanel.Properties.Resources.cleanfield;

        RFCSystem rfcsystem;

        public DriveControl(DriveControlEvents EventsPort, RFCSystem rfcsystem)
        {
            _eventsPort = EventsPort;

            DIRECTORY_SIM = (new UriBuilder("soap.rdmap", "lambda", 50001, "directory")).ToString();
            DIRECTORY_VISION = (new UriBuilder("soap.rdmap", "lambda", 50003, "directory")).ToString();
            DIRECTORY_VISION_REMOTE = (new UriBuilder("soap.rdmap", "omega", 50003, "directory")).ToString();

            InitializeComponent();

            this.rfcsystem = rfcsystem;

        }

        /*public void Connect(string machine, ushort port)
        {
            _eventsPort.Post(new OnConnect(this, DIRECTORY_1, DIRECTORY_2));
        }*/

        /*private void cbJoystick_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbJoystick.Tag is joystick.JoystickInstance[])
            {
                joystick.JoystickInstance[] list = (joystick.JoystickInstance[])cbJoystick.Tag;

                if (cbJoystick.SelectedIndex >= 0 && 
                    cbJoystick.SelectedIndex < list.Length)
                {
                    OnChangeJoystick change = new OnChangeJoystick(this);
                    change.Joystick = list[cbJoystick.SelectedIndex];

                    _eventsPort.Post(change);
                }
            }
        }*/

        private void DriveControl_Load(object sender, EventArgs e)
        {
            _eventsPort.Post(new OnLoad(this));

            _eventsPort.Post(new OnChangeInput(inputYellow.Name, inputYellow.SelectedItem.ToString()));
            _eventsPort.Post(new OnChangeInput(inputBlue.Name, inputBlue.SelectedItem.ToString()));
            _eventsPort.Post(new OnChangeInput(inputBall.Name, inputBall.SelectedItem.ToString()));
            //btnConnect_Click(sender,e);
            //btnConnectVision_Click(sender, e);
            //inputYellow.SelectedIndex = 1;
        }

        private void DriveControl_FormClosed(object sender, FormClosedEventArgs e)
        {
            _eventsPort.Post(new OnClosed(this));
        }

        /*public void ReplaceJoystickList(joystick.StateType data)
        {
            cbJoystick.BeginUpdate();
            try
            {
                cbJoystick.Items.Clear();
                foreach (joystick.JoystickInstance instance in data.Available)
                {
                    cbJoystick.Items.Add(instance.Name);
                    if (data.Current != null &&
                        instance.Guid == data.Current.Guid)
                    {
                        cbJoystick.SelectedIndex = cbJoystick.Items.Count - 1;
                    }
                }
                cbJoystick.Tag = data.Available;
            }
            finally
            {
                cbJoystick.EndUpdate();
            }
        }*/

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
                _eventsPort.Post(new OnMove(this, left, right, left, right));
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
            //int numbots = ourinfo.Count + theirinfo.Count;
            //float[] xcoords = new float[numbots];
            //float[] ycoords = new float[numbots];
            //float[] orients = new float[numbots];
            //bool[] isyellow = new bool[numbots];
            BallInfo binfo = rfcsystem.Predictor.getBallInfo();
            float ballx = binfo.Position.X, bally = binfo.Position.Y;
            bool usBlue = Robocup.Constants.Constants.get<string>("OUR_TEAM_COLOR") == "BLUE";
            /*for (int i = 0; i < ourinfo.Count; i++)
            {
                xcoords[i] = ourinfo[i].Position.X;
                ycoords[i] = ourinfo[i].Position.Y;
                orients[i] = ourinfo[i].Orientation;
                isyellow[i] = !usBlue;
            }
            for (int i = 0; i < theirinfo.Count; i++)
            {
                xcoords[i + ourinfo.Count] = theirinfo[i].Position.X;
                ycoords[i + ourinfo.Count] = theirinfo[i].Position.Y;
                orients[i + ourinfo.Count] = theirinfo[i].Orientation;
                isyellow[i + ourinfo.Count] = usBlue;
            }*/

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
                float x = info.Position.X;
                float y = info.Position.Y;
                float orientation = info.Orientation;
                for (int j = 0; j < 5; j++)
                {
                    poly[j] = new PointF(
                        (float)(fwidth * ((x + radius[j] * Math.Cos(orientation + (36.0f + 72.0f * j) * Math.PI / 180.0f) + 2.75f) / 5.5f)),
                        (float)(fheight * (1 - (y + radius[j] * Math.Sin(orientation + (36.0f + 72.0f * j) * Math.PI / 180.0f) + 2.0f) / 4.0f))
                        );

                }
                g.FillPolygon(usBlue ? Brushes.Blue : Brushes.Yellow, poly);
                g.DrawString(info.ID.ToString(), new Font(FontFamily.GenericMonospace, 11f), Brushes.Black,
                    new PointF((x + 2.75f) / 5.5f * fwidth, fheight * (1 - (y + 2.0f) / 4.0f)));
            }
            foreach (RobotInfo info in theirinfo)
            {
                float x = info.Position.X;
                float y = info.Position.Y;
                float orientation = info.Orientation;
                for (int j = 0; j < 5; j++)
                {
                    poly[j] = new PointF(
                        (float)(fwidth * ((x + radius[j] * Math.Cos(orientation + (36.0f + 72.0f * j) * Math.PI / 180.0f) + 2.75f) / 5.5f)),
                        (float)(fheight * (1 - (y + radius[j] * Math.Sin(orientation + (36.0f + 72.0f * j) * Math.PI / 180.0f) + 2.0f) / 4.0f))
                        );

                }
                g.FillPolygon(usBlue ? Brushes.Yellow : Brushes.Blue, poly);
                g.DrawString(info.ID.ToString(), new Font(FontFamily.GenericMonospace, 11f), Brushes.Black,
                    new PointF((x + 2.75f) / 5.5f * fwidth, fheight * (1 - (y + 2.0f) / 4.0f)));
            }

            //ball
            float ballrad = fwidth * 0.04f / 5.5f;
            g.FillEllipse(Brushes.Orange,
                fwidth * ((ballx + 2.75f) / 5.5f) - ballrad,
                fheight * (1 - (bally + 2.0f) / 4.0f) - ballrad, 2 * ballrad, 2 * ballrad);

            rfcsystem.drawCurrent(g, new BasicCoordinateConverter(fwidth, fheight));
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
            /*string machine = txtMachine.Text;
            
            if (machine.Length == 0)
            {
                txtMachine.Focus();
                return;
            }

            object obj = txtPort.ValidateText();

            if (obj == null)
            {
                obj = (ushort)0;
            }

            ushort port = (ushort)obj;*/


            _eventsPort.Post(new OnConnect(this, DIRECTORY_SIM));
        }



        public void ReplaceServiceList(ServiceInfoType[] list)
        {
            lstDirectory.BeginUpdate();
            try
            {
                lstDirectory.Tag = list;
                lstDirectory.Items.Clear();

                if (list.Length > 0)
                {
                    UriBuilder node = new UriBuilder(list[0].Service);
                    node.Path = null;
                    lblNode.Text = node.Host + ":" + node.Port;

                    linkDirectory.Enabled = true;
                }
                else
                {
                    lblNode.Text = string.Empty;
                    linkDirectory.Enabled = false;
                }

                foreach (ServiceInfoType info in list)
                {
                    if (info.Contract == drive.Contract.Identifier)
                    {
                        Uri serviceUri = new Uri(info.Service);
                        lstDirectory.Items.Add(serviceUri.AbsolutePath);
                    }
                }
            }
            finally
            {
                lstDirectory.EndUpdate();
            }
        }

        private void linkDirectory_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (lstDirectory.Tag is ServiceInfoType[])
            {
                ServiceInfoType[] list = (ServiceInfoType[])lstDirectory.Tag;

                if (list.Length > 0)
                {
                    UriBuilder node;

                    if (list[0].AliasList != null &&
                        list[0].AliasList.Count > 0)
                    {
                        node = new UriBuilder(list[0].AliasList[0]);
                    }
                    else
                    {
                        node = new UriBuilder(list[0].Service);
                    }

                    node.Path = "directory";
                    System.Diagnostics.Process.Start(node.ToString());
                }
            }
        }

        string ServiceByContract(string contract)
        {
            if (lstDirectory.Tag is ServiceInfoType[])
            {
                ServiceInfoType[] list = (ServiceInfoType[])lstDirectory.Tag;

                foreach (ServiceInfoType service in list)
                {
                    if (service.Contract == contract)
                    {
                        return service.Service;
                    }
                }
            }
            return null;
        }



        private void chkDrive_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void chkStop_CheckedChanged(object sender, EventArgs e)
        {
            if (chkStop.Checked)
            {
                _eventsPort.Post(new OnEStop(this));
            }
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

        /*private void btnDisconnect_Click(object sender, EventArgs e)
        {
            _eventsPort.Post(new OnDisconnectSickLRF(this));
            btnConnectLRF.Enabled = true;
            btnDisconnect.Enabled = false;
        }*/

        DateTime _lastMotor = DateTime.Now;

        public void UpdateMotorData(Robotics.SimDrive.Proxy.SimDriveState data)
        {
            if (data.TimeStamp > _lastMotor)
            {
                _lastMotor = data.TimeStamp;
                TimeSpan lag = DateTime.Now - data.TimeStamp;

                tmpW = data.Orientation.W;
                direction = (float)(data.Orientation.Y / Math.Sqrt(1 - tmpW * tmpW));
                if (direction > 0)
                {
                    tmpAngle = (float)(2 * Math.Acos(tmpW) + Math.PI / 2);
                }
                else
                {
                    tmpAngle = (float)(2 * Math.PI - 2 * Math.Acos(tmpW) + Math.PI / 2);
                }

            }
        }

        float tmpW, tmpAngle, direction;
        public void UpdateBallData(Robotics.SimBall.Proxy.SimBallState data)
        {
            if (data.TimeStamp > _lastMotor)
            {
                _lastMotor = data.TimeStamp;
                TimeSpan lag = DateTime.Now - data.TimeStamp;

            }
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

                _eventsPort.Post(
                    new OnChangeMotor(this, e.X, e.Y, picField.Height, picField.Width));
            }
        }

        private void picField_MouseLeave(object sender, EventArgs e)
        {
            //joystick.StateType dummy = new joystick.StateType();

            //UpdateJoystick(dummy);
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
                    _eventsPort.Post(
                        new OnDrag(this, new Vector3(fx, 0.09f, fy)));
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

                _eventsPort.Post(
                    new OnSetPose(this, new Vector3(fx, 0.09f, fy)));
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

        private void listDirectory_DoubleClick(object sender, EventArgs e)
        {
            ServiceInfoType[] list = lstDirectory.Tag as ServiceInfoType[];

            if (list != null &&
                lstDirectory.SelectedIndex >= 0 &&
                lstDirectory.SelectedIndex < list.Length)
            {
                ServiceInfoType info = FindServiceInfoFromServicePath((string)lstDirectory.SelectedItem);
                if (info == null)
                    return;
                if (info.Contract == drive.Contract.Identifier)
                {
                    _eventsPort.Post(new OnChangeMotor(this, info.Service));
                }
            }
        }

        ServiceInfoType FindServiceInfoFromServicePath(string path)
        {
            ServiceInfoType[] list = (ServiceInfoType[])lstDirectory.Tag;

            UriBuilder builder = new UriBuilder(list[0].Service);
            builder.Path = path;

            string uri = builder.ToString();

            return FindServiceInfoFromServiceUri(uri);
        }

        ServiceInfoType FindServiceInfoFromServiceUri(string uri)
        {
            ServiceInfoType[] list = (ServiceInfoType[])lstDirectory.Tag;

            foreach (ServiceInfoType si in list)
            {
                if (si.Service == uri)
                    return si;
            }
            return null;
        }

        private void saveFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            string path = Path.GetFullPath(saveFileDialog.FileName);
            if (!path.StartsWith(saveFileDialog.InitialDirectory))
            {
                MessageBox.Show("Log file must be in a subdirectory of the store", Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                e.Cancel = true;
            }
        }

        private void btnConnectRemoteVision_Click(object sender, EventArgs e)
        {
            _eventsPort.Post(new OnConnect(this, DIRECTORY_VISION_REMOTE));
        }

        private void inputChanged(object sender, EventArgs e)
        {
            ComboBox s = (ComboBox)sender;
            _eventsPort.Post(new OnChangeInput(s.Name, s.SelectedItem.ToString()));
        }

        private void btnConnectVision_Click(object sender, EventArgs e)
        {
            //_eventsPort.Post(new OnFindServices(this, DIRECTORY_1, DIRECTORY_2));
            if (toggleRFC.Text == "Start RFC")
                startRFC_Click(toggleRFC, null);
            _eventsPort.Post(new OnConnect(this, DIRECTORY_VISION));
        }

        private bool isOn;
        private void startRFC_Click(object sender, EventArgs e)
        {
            isOn = ((Button)sender).Text != "Start RFC";
            if (isOn)
            { // need to turn off
                ((Button)sender).Text = "Start RFC";
                _eventsPort.Post(new ToggleRFC(this, false));
            }
            else
            { // off now, need to turn on
                ((Button)sender).Text = "Stop RFC";
                _eventsPort.Post(new ToggleRFC(this, true));
            }
        }

        private void buttonReloadConstants_Click(object sender, EventArgs e)
        {
            Robocup.Constants.Constants.Load();
            rfcsystem.initialize();
        }

        private void buttonUnusedConstants_Click(object sender, EventArgs e)
        {
            MessageBox.Show(string.Join("\n", Robocup.Constants.Constants.getUnused().ToArray()));
        }


    }


    class WebCamView : Form
    {
        PictureBox picture;

        public WebCamView()
        {
            SuspendLayout();
            Text = "WebCam";
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            ControlBox = false;
            ClientSize = new Size(320, 240);
            ShowIcon = false;
            ShowInTaskbar = false;

            picture = new PictureBox();
            picture.Name = "Picture";
            picture.Dock = DockStyle.Fill;

            Controls.Add(picture);

            ResumeLayout();
        }

        public Image Image
        {
            get { return picture.Image; }
            set
            {
                picture.Image = value;
                ClientSize = value.Size;
            }
        }

        private DateTime _timestamp;

        public DateTime TimeStamp
        {
            get { return _timestamp; }
            set
            {
                _timestamp = value;
                Text = "WebCam - " + _timestamp.ToString("hh:mm:ss.ff");
            }
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

    class DriveControlEvents
        : PortSet<
            OnLoad,
            OnClosed,
        //OnChangeJoystick,
            OnConnect,

            OnConnectMotor,
            OnConnectVision,
            OnConnectCommander,
            OnUnsubscribeVision,

            //OnConnectSickLRF,
        //OnConnectArticulatedArm,
        //OnConnectWebCam,
            OnStartService,
            OnChangeMotor,
            OnMove,
            OnEStop,
        //OnApplyJointParameters,
        //OnDisconnectSickLRF,
        //OnDisconnectWebCam,
            OnLogSetting,
        //OnQueryFrame,
            OnSetPose,
            OnDrag,
            OnConnectBall,
            OnKick,
            OnChangeInput,
            ToggleRFC
        >
    {
    }

    class DriveControlEvent
    {
        private DriveControl _driveControl;

        public DriveControl DriveControl
        {
            get { return _driveControl; }
            set { _driveControl = value; }
        }

        public DriveControlEvent(DriveControl driveControl)
        {
            _driveControl = driveControl;
        }
    }

    class OnLoad : DriveControlEvent
    {
        public OnLoad(DriveControl form)
            : base(form)
        {
        }
    }

    class OnChangeInput
    {
        string _objects;

        public string Objects
        {
            get { return _objects; }
            set { _objects = value; }
        }

        string _result;

        public string Result
        {
            get { return _result; }
            set { _result = value; }
        }
        public OnChangeInput(string objects, string result)
        {
            _objects = objects;
            _result = result;
        }
    }

    /*class OnQueryDirectory : DriveControlEvent
    {
        string _serviceSim;
        string _serviceVision;

        public string ServiceSim
        {
            get { return _serviceSim; }
            set { _serviceSim = value; }
        }

        public string ServiceVision
        {
            get { return _serviceVision; }
            set { _serviceVision = value; }
        }

        public OnQueryDirectory(DriveControl form, string serviceSim, string serviceVision)
            : base(form)
        {
            _serviceSim = serviceSim;
            _serviceVision = serviceVision;
        }
    }*/

    class OnConnect : DriveControlEvent
    {
        string _directory;

        public string Directory
        {
            get { return _directory; }
            set { _directory = value; }
        }

        //better name for classes that inherit from this one
        public string Service
        {
            get { return _directory; }
            set { _directory = value; }
        }

        public OnConnect(DriveControl form, string directory)
            : base(form)
        {
            _directory = directory;
        }
    }


    class ToggleRFC : DriveControlEvent
    {
        bool _turnOn;

        public bool TurnOn
        {
            get { return _turnOn; }
            set { _turnOn = value; }
        }

        public ToggleRFC(DriveControl form, bool turnOn)
            : base(form)
        {
            _turnOn = turnOn;
        }
    }


    class OnConnectVision : OnConnect
    {
        public OnConnectVision(DriveControl form, string service)
            : base(form, service)
        {
        }
    }

    class OnConnectCommander : OnConnect
    {
        public OnConnectCommander(DriveControl form, string service)
            : base(form, service)
        {
        }
    }

    class OnConnectMotor : OnConnect
    {
        public OnConnectMotor(DriveControl form, string service)
            : base(form, service)
        {
        }
    }

    class OnConnectBall : OnConnect
    {
        public OnConnectBall(DriveControl form, string service)
            : base(form, service)
        {
        }
    }


    class OnChangeMotor : OnConnect
    {
        private float _x;
        private float _y;
        private float _width;
        private float _height;

        public float X
        {
            get { return _x; }
            set { _x = value; }
        }

        public float Y
        {
            get { return _y; }
            set { _y = value; }
        }

        public float Height
        {
            get { return _height; }
            set { _height = value; }
        }

        public float Width
        {
            get { return _width; }
            set { _width = value; }
        }


        public OnChangeMotor(DriveControl form, string service)
            : base(form, service)
        {
        }

        public OnChangeMotor(DriveControl form, float x, float y, float height, float width)
            : base(form, null)
        {
            X = x;
            Y = y;
            Height = height;
            Width = width;
        }
    }

    /*class OnConnectSickLRF : OnConnect
    {
        public OnConnectSickLRF(DriveControl form, string service)
            : base(form, service)
        {
        }
    }*/

    class OnConnectArticulatedArm : OnConnect
    {
        public OnConnectArticulatedArm(DriveControl form, string service)
            : base(form, service)
        {
        }
    }

    class OnStartService : DriveControlEvent
    {
        string _contract;
        string _constructor;

        public string Contract
        {
            get { return _contract; }
            set { _contract = value; }
        }

        public string Constructor
        {
            get { return _constructor; }
            set { _constructor = value; }
        }


        public OnStartService(DriveControl form, string contract)
            : base(form)
        {
            _contract = contract;
        }
    }

    class OnClosed : DriveControlEvent
    {
        public OnClosed(DriveControl form)
            : base(form)
        {
        }
    }

    class OnKick : DriveControlEvent
    {
        public OnKick(DriveControl form)
            : base(form)
        {
        }
    }

    class OnMove : DriveControlEvent
    {
        int _robotID;
        public int ID
        {
            get { return _robotID; }
            set { _robotID = value; }
        }

        int _leftFront;

        public int LeftFront
        {
            get { return _leftFront; }
            set { _leftFront = value; }
        }

        int _rightFront;

        public int RightFront
        {
            get { return _rightFront; }
            set { _rightFront = value; }
        }

        int _leftBack;

        public int LeftBack
        {
            get { return _leftBack; }
            set { _leftBack = value; }
        }

        int _rightBack;

        public int RightBack
        {
            get { return _rightBack; }
            set { _rightBack = value; }
        }

        public OnMove(DriveControl form, int leftf, int rightf, int leftb, int rightb, int id)
            : base(form)
        {
            _leftFront = leftf * 750 / 1250;
            _rightFront = rightf * 750 / 1250;
            _leftBack = leftb * 750 / 1250;
            _rightBack = rightb * 750 / 1250;
            _robotID = id;
        }

        public OnMove(DriveControl form, int leftf, int rightf, int leftb, int rightb)
            : this(form, leftf, rightf, leftb, rightb, -1)
        {
        }
    }

    class OnSetPose : DriveControlEvent
    {
        Vector3 _position;
        float _orientation;

        public Vector3 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public float Orientation
        {
            get { return _orientation; }
            set { _orientation = value; }
        }


        public OnSetPose(DriveControl form, Vector3 pos, float orient)
            : base(form)
        {
            _position = pos;
            _orientation = orient;
        }

        public OnSetPose(DriveControl form, Vector3 pos)
            : base(form)
        {
            _position = pos;
            _orientation = 0.0f; //sentinel flags
        }

        public OnSetPose(DriveControl form, float orient)
            : base(form)
        {
            _position = new Vector3(0.0f, -1.0f, 0.0f); //sentinel position
            _orientation = orient;
        }
    }

    class OnDrag : DriveControlEvent
    {
        Vector3 _position;
        float _orientation;

        public Vector3 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public float Orientation
        {
            get { return _orientation; }
            set { _orientation = value; }
        }


        public OnDrag(DriveControl form, Vector3 pos, float orient)
            : base(form)
        {
            _position = pos;
            _orientation = orient;
        }

        public OnDrag(DriveControl form, Vector3 pos)
            : base(form)
        {
            _position = pos;
            _orientation = 0.0f; //sentinel flags
        }

        public OnDrag(DriveControl form, float orient)
            : base(form)
        {
            _position = new Vector3(0.0f, -1.0f, 0.0f); //sentinel position
            _orientation = orient;
        }
    }

    class OnEStop : DriveControlEvent
    {
        public OnEStop(DriveControl form)
            : base(form)
        {
        }
    }

    /*class OnApplyJointParameters : DriveControlEvent
    {
        int _angle;

        public int Angle
        {
            get { return _angle; }
            set { _angle = value; }
        }

        string _jointName;

        public string JointName
        {
            get { return _jointName; }
            set { _jointName = value; }
        }

        public OnApplyJointParameters(DriveControl form, int angle, string name)
            : base(form)
        {
            _angle = angle;
            _jointName = name;
        }
    }

    class OnDisconnectSickLRF : DriveControlEvent
    {
        public OnDisconnectSickLRF(DriveControl form)
            : base(form)
        {
        }
    }*/


    class OnLogSetting : DriveControlEvent
    {
        bool _log;
        string _file;

        public bool Log
        {
            get { return _log; }
            set { _log = value; }
        }

        public string File
        {
            get { return _file; }
            set { _file = value; }
        }

        public OnLogSetting(DriveControl form, bool log, string file)
            : base(form)
        {
            _log = log;
            _file = file;
        }
    }

    /*class OnConnectWebCam : OnConnect
    {
        public OnConnectWebCam(DriveControl form, string service)
            : base(form, service)
        {
        }
    }

    class OnDisconnectWebCam : DriveControlEvent
    {
        public OnDisconnectWebCam(DriveControl form)
            : base(form)
        {
        }
    }

    class OnQueryFrame : DriveControlEvent
    {
        public OnQueryFrame(DriveControl form)
            : base(form)
        {
        }
    }*/


}
