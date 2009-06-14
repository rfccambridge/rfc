using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Robocup.Utilities;
using Robocup.Core;
using Robocup.CoreRobotics;
using Robocup.Plays;

using Vision;

namespace Robocup.ControlForm {

    public partial class ControlForm : Form
    {
        bool serialConnected = false;
        RemoteRobots _serial;

        bool visionTopConnected = false;
        bool visionBottomConnected = false;
        bool refboxConnected = false;

        VisionMessage.Team OUR_TEAM;

        int MESSAGE_SENDER_PORT = Constants.get<int>("ports", "VisionDataPort");

        Robocup.MessageSystem.MessageReceiver<Robocup.Core.VisionMessage> _visionTop;
        Robocup.MessageSystem.MessageReceiver<Robocup.Core.VisionMessage> _visionBottom;

        FieldDrawerForm drawer;
        PlaySelectorForm playSelectorForm;

        bool systemStarted = false;
        RFCSystem _system;

        //IPredictor _predictor;
        BasicPredictor _predictor;
        //ICoordinateConverter converter = new Robocup.Utilities.ControlFormConverter(420,610, 5, 5);
        ICoordinateConverter converter;

        object field_lock = new object();
        object predictor_lock = new object();

        // Logging
        LogReader _logReader;
        IPredictor _logPredictor;
        FieldDrawerForm _logFieldDrawer;
        List<Type> _logLineFormat;
        Timer timer;
        bool logging;
        string LOG_FILE;

        String TOP_CAMERA = "top_cam";
        String BOTTOM_CAMERA = "bottom_cam";

        public ControlForm()
        {
            InitializeComponent();

            LoadConstants();

            playSelectorForm = new PlaySelectorForm();
            playSelectorForm.Show();

            // Defaults hosts for the GUI, for convenience only
            visionTopHost.Text = Constants.get<string>("default", "DEFAULT_HOST_VISION_TOP");
            visionBottomHost.Text = Constants.get<string>("default", "DEFAULT_HOST_VISION_BOTTOM");
            serialHost.Text = Constants.get<string>("default", "DEFAULT_HOST_SERIAL");
            txtRefbox.Text = Constants.get<string>("default", "REFBOX_ADDR");

            btnLogNext.Enabled = false;

            loggingInit();

            createSystem();

            this.Focus();
        }

        public void LoadConstants()
        {
            OUR_TEAM = (Constants.get<string>("configuration", "OUR_TEAM") == "YELLOW" ?
                VisionMessage.Team.YELLOW : VisionMessage.Team.BLUE);
            LOG_FILE = Constants.get<string>("motionplanning", "LOG_FILE");
        }

        private void createSystem()
        {
            _serial = new RemoteRobots();
            _system = new RFCSystem();

            _predictor = new BasicPredictor();
            // add vision predictor hooked up to vision
            _system.registerAcceptor(_predictor as IVisionInfoAcceptor);
            _system.registerPredictor(_predictor);

            if (drawer != null)
                drawer.Close();
            drawer = new FieldDrawerForm(_predictor);
            converter = drawer.Converter;
            drawer.Show();

            // add serial commander
            _system.registerCommander(_serial);

            // todo
            _system.initialize();
            _system.reloadPlays();

            drawer.drawer.ourPlayNames = _system.getInterpreter().ourPlayNames;
            drawer.drawer.theirPlayNames = _system.getInterpreter().theirPlayNames;

            // playSelectorForm
            playSelectorForm.LoadPlays(_system.getInterpreter().getPlays());
        }

        private void handleVisionUpdateTop(VisionMessage msg)
        {
            handleVisionUpdate(msg, TOP_CAMERA);
        }

        private void handleVisionUpdateBottom(VisionMessage msg)
        {
            handleVisionUpdate(msg, BOTTOM_CAMERA);
        }

        /*private void handleVisionUpdate(VisionMessage msg, string cameraName) {
                  
            lock (predictor_lock)
            {
                ((IVisionInfoAcceptor)_predictor).Update(msg);                
            }
            drawer.Invalidate();

            lock (field_lock)
            {                
                drawer.setPlayType(_system.getCurrentPlayType());
                _system.drawCurrent(drawer.CreateGraphics(), converter);
            }
         }*/

        private void handleVisionUpdate(VisionMessage msg, String cameraName)
        {

            List<RobotInfo> ours = new List<RobotInfo>();
            List<RobotInfo> theirs = new List<RobotInfo>();

            foreach (VisionMessage.RobotData robot in msg.Robots)
            {
                RobotInfo robotInfo = new RobotInfo(robot.Position, robot.Orientation, robot.ID);
                robotInfo.Team = (robot.Team == VisionMessage.Team.YELLOW) ? 0 : 1;
                (robot.Team == OUR_TEAM ? ours : theirs).Add(robotInfo);
            }

            lock (predictor_lock)
            {
                _predictor.updatePartOurRobotInfo(ours, cameraName);
                _predictor.updatePartTheirRobotInfo(theirs, cameraName);
                if (msg.Ball != null)
                {
                    //Vector2 ballposition = new Vector2(2 + 1.01 * (msg.BallPosition.X - 2), msg.BallPosition.Y);                    
                    _predictor.updateBallInfo(new BallInfo(msg.Ball.Position));
                }
                else
                {
                    _predictor.updateBallInfo(null);
                }
            }
            drawer.Invalidate();

            lock (field_lock)
            {
                //_system.drawCurrent(_field.getGraphics(), converter);                
                drawer.setPlayType(_system.getCurrentPlayType());
                _system.drawCurrent(drawer.CreateGraphics(), converter);
            }
        }

        private void visionTopConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (!visionTopConnected)
                {
                    _visionTop = Robocup.MessageSystem.Messages.CreateClientReceiver<Robocup.Core.VisionMessage>(visionTopHost.Text, MESSAGE_SENDER_PORT);
                    _visionTop.MessageReceived += new Robocup.MessageSystem.ReceiveMessageDelegate<VisionMessage>(handleVisionUpdateTop);

                    visionTopStatus.BackColor = Color.Green;
                    visionTopConnect.Text = "Disconnect";
                    visionTopConnected = true;
                }
                else
                {
                    _visionTop.Close();
                    visionTopStatus.BackColor = Color.Red;
                    visionTopConnect.Text = "Connect";
                    visionTopConnected = false;
                }
            }
            catch (Exception except)
            {
                MessageBox.Show("Problem connecting to vision on host: " + visionTopHost.Text);
                Console.WriteLine("Problem connecting to vision: " + except.ToString());
                Console.WriteLine(except.StackTrace);
            }

        }
        private void visionBottomConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (!visionBottomConnected)
                {
                    _visionBottom = Robocup.MessageSystem.Messages.CreateClientReceiver<Robocup.Core.VisionMessage>(visionBottomHost.Text, MESSAGE_SENDER_PORT);
                    _visionBottom.MessageReceived += new Robocup.MessageSystem.ReceiveMessageDelegate<VisionMessage>(handleVisionUpdateBottom);

                    visionBottomStatus.BackColor = Color.Green;
                    visionBottomConnect.Text = "Disconnect";
                    visionBottomConnected = true;
                }
                else
                {
                    _visionBottom.Close();
                    visionBottomStatus.BackColor = Color.Red;
                    visionBottomConnect.Text = "Connect";
                    visionBottomConnected = false;
                }
            }
            catch (Exception except)
            {
                MessageBox.Show("Problem connecting to vision on host: " + visionBottomHost.Text);
                Console.WriteLine("Problem connecting to vision: " + except.ToString());
                Console.WriteLine(except.StackTrace);
            }
        }

        private void serialConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (!serialConnected)
                {
                    if (_serial.start(serialHost.Text))
                    {
                        serialStatus.BackColor = Color.Green;
                        serialConnect.Text = "Disconnect";
                        serialConnected = true;
                    }
                }
                else
                {
                    _serial.stop();
                    serialStatus.BackColor = Color.Red;
                    serialConnect.Text = "Connect";
                    serialConnected = false;
                }
            }
            catch (Exception except)
            {
                Console.WriteLine(except.StackTrace);
            }

        }

        private void rfcStart_Click(object sender, EventArgs e)
        {
            if (!systemStarted)
            {
                _system.start();
                rfcStatus.BackColor = Color.Green;
                rfcStart.Text = "Stop";
                systemStarted = true;
            }
            else
            {
                _system.stop();
                rfcStatus.BackColor = Color.Red;
                rfcStart.Text = "Start";
                systemStarted = false;
            }
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.R))
            {
                if (systemStarted)
                {
                    MessageBox.Show("System running. Need to stop system to reload contants.");
                    return false;
                }

                Constants.Load();

                LoadConstants();
                if (_predictor is BasicPredictor)
                {
                    ((BasicPredictor)_predictor).LoadConstants();
                }
                //else if (_predictor is AveragingPredictor)
                //{
                //    ((AveragingPredictor)_predictor).LoadConstants();
                //}

                _system.LoadConstants();
                _system.reloadPlays();

                playSelectorForm.LoadPlays(_system.getInterpreter().getPlays());

                Console.WriteLine("Constants and plays reloaded.");
            }

            if (keyData == (Keys.Control | Keys.C))
            {
                if (systemStarted)
                {
                    MessageBox.Show("System running. Need to stop system to set refbox listener.");
                    return false;
                }
                _system.setRefBoxListener(txtRefbox.Text);
                Console.WriteLine("Refbox listener set.");
            }

            return false;
        }

        #region Logging
        private void loggingInit()
        {
            // Logging
            _logReader = new LogReader();
            _logPredictor = new StaticPredictor();
            _logFieldDrawer = new FieldDrawerForm(_logPredictor);

            // Log Line format
            // timestamp current_state desired_state next_waypoint wheel_speeds path
            _logLineFormat = new List<Type>();
            _logLineFormat.Add(typeof(DateTime));
            _logLineFormat.Add(typeof(RobotInfo));
            _logLineFormat.Add(typeof(RobotInfo));
            _logLineFormat.Add(typeof(RobotInfo));
            _logLineFormat.Add(typeof(WheelSpeeds));
            _logLineFormat.Add(typeof(RobotPath));


            timer = new Timer();
            timer.Interval = 500;
            //timer.Tick += new EventHandler(InvalidateTimerHanlder);
            timer.Start();
        }

        private void InvalidateTimerHanlder(object obj, EventArgs e)
        {
            _logFieldDrawer.Invalidate();
        }

        private void btnLogNext_Click(object sender, EventArgs e)
        {

            if (!_logReader.LogOpen)
            {
                MessageBox.Show("Log file not open.");
                return;
            }

            if (!_logFieldDrawer.Visible)
                _logFieldDrawer.Show();

            // Get logged info

            _logReader.Next();
            List<Object> loggedItems = _logReader.GetLoggedItems();


            // Log Line format
            // timestamp current_state desired_state next_waypoint wheel_speeds path
            RobotInfo curState = (RobotInfo)loggedItems[1];
            RobotInfo waypointInfo = (RobotInfo)loggedItems[3];
            RobotInfo destState = (RobotInfo)loggedItems[2];
            RobotPath path = (RobotPath)loggedItems[5];

            // Update predictor (this will also draw the robot positions)

            ((IInfoAcceptor)_logPredictor).updateRobot(curState.ID, curState);

            // Draw the path, and the arrows            

            // Since we are the only ones who are using this FieldDrawer, we can monopolize 
            // its arrows and paths holder
            _logFieldDrawer.ClearPaths();
            _logFieldDrawer.AddPath(path);

            _logFieldDrawer.ClearArrows();
            _logFieldDrawer.AddArrow(new Arrow(curState.Position, destState.Position, Color.Red, 0.04));
            _logFieldDrawer.AddArrow(new Arrow(curState.Position, waypointInfo.Position, Color.Yellow, 0.04));

            _logFieldDrawer.Invalidate();

        }

        private void btnStartStopLogging_Click(object sender, EventArgs e)
        {
            /*if (!(_pathFollower.Planner is ILogger))
            {
                MessageBox.Show("Selected MotionPlanner does not implement ILogger interface.");
                return;
            }*/

            if (logging)
            {
                logging = false;
                btnStartStopLogging.Text = "Start log";
                btnLogOpenClose.Enabled = true;
            }
            else
            {
                logging = true;
                btnStartStopLogging.Text = "Stop log";
                btnLogOpenClose.Enabled = false;
            }

            // if enabled, start motion planner's logging
            if (_system.Controller is RFCController && ((RFCController)_system.Controller).Planner is ILogger)
            {
                ILogger logger = ((RFCController)_system.Controller).Planner as ILogger;

                //COMMENTED OUT- problems with designer...
                /*
                if (!logger.Logging) {
                    logger.LogFile = LOG_FILE;
                    logger.StartLogging(int.Parse(txtRobotID.Text));

                }
                else {
                    logger.StopLogging();
                }
                 */
            }
        }

        private void btnLogOpenClose_Click(object sender, EventArgs e)
        {
            if (_logReader.LogOpen)
            {
                _logReader.CloseLogFile();
                btnLogOpenClose.Text = "Open log";
                btnLogNext.Enabled = false;
                btnStartStopLogging.Enabled = true;
            }
            else
            {
                _logReader.OpenLogFile(LOG_FILE, _logLineFormat);
                btnLogOpenClose.Text = "Close log";
                btnLogNext.Enabled = true;
                btnStartStopLogging.Enabled = false;
            }


        }

        #endregion

        private void btnRefbox_Click(object sender, EventArgs e)
        {
            try
            {
                if (!refboxConnected)
                {
                    _system.setRefBoxListener(txtRefbox.Text);
                    lblRefbox.BackColor = Color.Green;
                    btnRefbox.Text = "Disconnect";
                    refboxConnected = true;
                }
                else
                {
                    //_visionTop.Close();

                    lblRefbox.BackColor = Color.Red;
                    btnRefbox.Text = "Connect";
                    refboxConnected = false;
                }
            }
            catch (Exception except)
            {
                MessageBox.Show("Problem connecting to refbox on host: " + txtRefbox.Text);
                Console.WriteLine("Problem connecting to refbox: " + except.ToString());
                Console.WriteLine(except.StackTrace);
            }

        }


        void printRobotInfo(SSLVision.SSL_DetectionRobotManaged robot)
        {
            Console.Write(String.Format("CONF={0,4:F2} ", robot.confidence()));
            if (robot.has_robot_id())
            {
                Console.Write(String.Format("ID={0,3:G} ", robot.robot_id()));
            }
            else
            {
                Console.Write(String.Format("ID=N/A "));
            }
            Console.Write(String.Format(" HEIGHT={0,6:F2} POS=<{1,9:F2},{2,9:F2}> ", robot.height(), robot.x(), robot.y()));
            if (robot.has_orientation())
            {
                Console.Write(String.Format("ANGLE={0,6:F3} ", robot.orientation()));
            }
            else
            {
                Console.Write(String.Format("ANGLE=N/A    "));
            }
            Console.Write(String.Format("RAW=<{0,8:F2},{1,8:F2}>\n", robot.pixel_x(), robot.pixel_y()));
        }

        private void btnSSLVision_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Starting..");
            SSLVision.SSL_WrapperPacketManaged packet = new SSLVision.SSL_WrapperPacketManaged();
            SSLVision.RoboCupSSLClientManaged client = new SSLVision.RoboCupSSLClientManaged();
            client.open(true);            
            
            while(true) {
                if (client.receive(packet)) {
                Console.Write(String.Format("-----Received Wrapper Packet---------------------------------------------\n"));
            //see if the packet contains a robot detection frame:
            if (packet.has_detection()) {
                SSLVision.SSL_DetectionFrameManaged detection = packet.detection();
                //Display the contents of the robot detection results:
                //double t_now = GetTimeSec();
				double t_now = 0;

                Console.Write(String.Format("-[Detection Data]-------\n"));
                //Frame info:
                Console.Write(String.Format("Camera ID={0:G} FRAME={1:G} T_CAPTURE={2:F4}\n",detection.camera_id(),detection.frame_number(),detection.t_capture()));

                Console.Write(String.Format("SSL-Vision Processing Latency                   {0,7:F3}ms\n",(detection.t_sent()-detection.t_capture())*1000.0));
                Console.Write(String.Format("Network Latency (assuming synched system clock) {0,7:F3}ms\n", (t_now - detection.t_sent()) * 1000.0));
                Console.Write(String.Format("Total Latency   (assuming synched system clock) {0,7:F3}ms\n", (t_now - detection.t_capture()) * 1000.0));
                int balls_n = detection.balls_size();
                int robots_blue_n =  detection.robots_blue_size();
                int robots_yellow_n =  detection.robots_yellow_size();

                //Ball info:
                for (int i = 0; i < balls_n; i++) {
                    SSLVision.SSL_DetectionBallManaged ball = detection.balls(i);
                    Console.Write(String.Format("-Ball ({0,2:G}/{1,2:G}): CONF={2,4:F2} POS=<{3,9:F2},{4,9:F2}> ", i+1, balls_n, ball.confidence(),ball.x(),ball.y()));
                    if (ball.has_z()) {
                        Console.Write(String.Format("Z={0,7:F2} ",ball.z()));
                    } else {
                        Console.Write(String.Format("Z=N/A   "));
                    }
                    Console.Write(String.Format("RAW=<{0,8:F2},{1,8:F2}>\n",ball.pixel_x(),ball.pixel_y()));
                }

                //Blue robot info:
                for (int i = 0; i < robots_blue_n; i++) {
                    SSLVision.SSL_DetectionRobotManaged robot = detection.robots_blue(i);
                    Console.Write(String.Format("-Robot(B) ({0,2:G}/{1,2:G}): ",i+1, robots_blue_n));
                    printRobotInfo(robot);
                }

                //Yellow robot info:
                for (int i = 0; i < robots_yellow_n; i++) {
                    SSLVision.SSL_DetectionRobotManaged robot = detection.robots_yellow(i);
                    Console.Write(String.Format("-Robot(Y) ({0,2:G}/{1,2:G}): ", i + 1, robots_yellow_n));
                    printRobotInfo(robot);
                }

            }

            //see if packet contains geometry data:
            if (packet.has_geometry()) {
                SSLVision.SSL_GeometryDataManaged geom = packet.geometry();
                Console.Write(String.Format("-[Geometry Data]-------\n"));

                SSLVision.SSL_GeometryFieldSizeManaged field = geom.field();
                Console.Write(String.Format("Field Dimensions:\n"));
                Console.Write(String.Format("  -line_width={0:G} (mm)\n",field.line_width()));
                Console.Write(String.Format("  -field_length={0:G} (mm)\n",field.field_length()));
                Console.Write(String.Format("  -field_width={0:G} (mm)\n", field.field_width()));
                Console.Write(String.Format("  -boundary_width={0:G} (mm)\n",field.boundary_width()));
                Console.Write(String.Format("  -referee_width={0:G} (mm)\n",field.referee_width()));
                Console.Write(String.Format("  -goal_width={0:G} (mm)\n",field.goal_width()));
                Console.Write(String.Format("  -goal_depth={0:G} (mm)\n",field.goal_depth()));
                Console.Write(String.Format("  -goal_wall_width={0:G} (mm)\n",field.goal_wall_width()));
                Console.Write(String.Format("  -center_circle_radius={0:G} (mm)\n",field.center_circle_radius()));
                Console.Write(String.Format("  -defense_radius={0:G} (mm)\n",field.defense_radius()));
                Console.Write(String.Format("  -defense_stretch={0:G} (mm)\n",field.defense_stretch()));
                Console.Write(String.Format("  -free_kick_from_defense_dist={0:G} (mm)\n",field.free_kick_from_defense_dist()));
                Console.Write(String.Format("  -penalty_spot_from_field_line_dist={0:G} (mm)\n",field.penalty_spot_from_field_line_dist()));
                Console.Write(String.Format("  -penalty_line_from_spot_dist={0:G} (mm)\n",field.penalty_line_from_spot_dist()));

                int calib_n = geom.calib_size();
                for (int i=0; i< calib_n; i++) {
                    SSLVision.SSL_GeometryCameraCalibrationManaged calib = geom.calib(i);
                    Console.Write(String.Format("Camera Geometry for Camera ID {0:G}:\n", calib.camera_id()));
                    Console.Write(String.Format("  -focal_length={0:F2}\n",calib.focal_length()));
                    Console.Write(String.Format("  -principal_point_x={0:F2}\n",calib.principal_point_x()));
                    Console.Write(String.Format("  -principal_point_y={0:F2}\n",calib.principal_point_y()));
                    Console.Write(String.Format("  -distortion={0:F2}\n",calib.distortion()));
                    Console.Write(String.Format("  -q0={0:F2}\n",calib.q0()));
                    Console.Write(String.Format("  -q1={0:F2}\n",calib.q1()));
                    Console.Write(String.Format("  -q2={0:F2}\n",calib.q2()));
                    Console.Write(String.Format("  -q3={0:F2}\n",calib.q3()));
                    Console.Write(String.Format("  -tx={0:F2}\n",calib.tx()));
                    Console.Write(String.Format("  -ty={0:F2}\n",calib.ty()));
                    Console.Write(String.Format("  -tz={0:F2}\n",calib.tz()));

                    if (calib.has_derived_camera_world_tx() && calib.has_derived_camera_world_ty() && calib.has_derived_camera_world_tz()) {
                      Console.Write(String.Format("  -derived_camera_world_tx={0:F}\n",calib.derived_camera_world_tx()));
                      Console.Write(String.Format("  -derived_camera_world_ty={0:F}\n",calib.derived_camera_world_ty()));
                      Console.Write(String.Format("  -derived_camera_world_tz={0:F}\n",calib.derived_camera_world_tz()));
                    }

                }
            }
        }
    }
        }

    }
    public class RemoteRobots : IRobots {
        int SERIAL_SENDER_PORT = Constants.get<int>("ports", "RemoteControlPort");
        Robocup.MessageSystem.MessageSender<Robocup.Core.RobotCommand> _serial;

        public RemoteRobots() {
        }

        public bool start(String host) {
            _serial = Robocup.MessageSystem.Messages.CreateClientSender<Robocup.Core.RobotCommand>(host, SERIAL_SENDER_PORT);
            return (_serial != null);
        }

        public void stop() {
            _serial.Close();
        }

   
        #region IRobots Members
        const float scaling = 1.0f;
        public void setMotorSpeeds(int robotID, WheelSpeeds wheelSpeeds) {
            if (robotID < 0 || _serial==null) return;
            _serial.Post(new RobotCommand(robotID, new WheelSpeeds((int)(wheelSpeeds.lf / scaling), (int)(wheelSpeeds.rf / scaling), (int)(wheelSpeeds.lb / scaling), (int)(wheelSpeeds.rb / scaling))));
            //Console.WriteLine("RemoteRobots::setMotorSpeeds: " + wheelSpeeds.lf / scaling + " "
            //    + wheelSpeeds.rf / scaling + " " + wheelSpeeds.lb / scaling + " " + wheelSpeeds.rb / scaling + " ");
        }
        public void charge(int robotID) {
            if (robotID < 0 || _serial == null) return;
            _serial.Post(new RobotCommand(robotID, RobotCommand.Command.CHARGE, null));
        }
        public void kick(int robotID)
        {
            if (robotID < 0 || _serial == null) return;
            _serial.Post(new RobotCommand(robotID, RobotCommand.Command.KICK, null));
        }

        public void beamKick(int robotID) 
        {
            if (robotID < 0 || _serial == null) return;
            _serial.Post(new RobotCommand(robotID, RobotCommand.Command.BEAMKICK, null));
        }

        #endregion
    }
}