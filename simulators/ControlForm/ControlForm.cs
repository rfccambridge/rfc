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
    
    public partial class ControlForm : Form{
        bool serialConnected = false;
        RemoteRobots _serial;

        bool visionTopConnected = false;
        bool visionBottomConnected = false;

        VisionMessage.Team OUR_TEAM;

        int MESSAGE_SENDER_PORT = Constants.get<int>("ports", "VisionDataPort");
        
        Robocup.MessageSystem.MessageReceiver<Robocup.Core.VisionMessage> _visionTop;
        Robocup.MessageSystem.MessageReceiver<Robocup.Core.VisionMessage> _visionBottom;
        
        FieldDrawerForm drawer;
        PlaySelectorForm playSelectorForm;

        bool systemStarted = false;
        RFCSystem _system;

        IPredictor _predictor;
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

        public ControlForm() {
            InitializeComponent();

            LoadConstants();

            playSelectorForm = new PlaySelectorForm();
            playSelectorForm.Show();

            // Defaults hosts for the GUI, for convenience only
            visionTopHost.Text = Constants.get<string>("default", "DEFAULT_HOST_VISION_TOP");
            visionBottomHost.Text = Constants.get<string>("default", "DEFAULT_HOST_VISION_BOTTOM");
            serialHost.Text = Constants.get<string>("default", "DEFAULT_HOST_SERIAL");

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

        private void createSystem() {
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
       
        private void handleVisionUpdate(VisionMessage msg) {
                  
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
        }

        private void visionTopConnect_Click(object sender, EventArgs e) {
            try {
                if (!visionTopConnected) {
                    _visionTop = Robocup.MessageSystem.Messages.CreateClientReceiver<Robocup.Core.VisionMessage>(visionTopHost.Text, MESSAGE_SENDER_PORT);
                    _visionTop.MessageReceived += new Robocup.MessageSystem.ReceiveMessageDelegate<VisionMessage>(handleVisionUpdate);

                    visionTopStatus.BackColor = Color.Green;
                    visionTopConnect.Text = "Disconnect";
                    visionTopConnected = true;      
                } else {
                    _visionTop.Close();
                    visionTopStatus.BackColor = Color.Red;
                    visionTopConnect.Text = "Connect";
                    visionTopConnected = false;
                }
            } catch (Exception except) {
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
                    _visionBottom.MessageReceived += new Robocup.MessageSystem.ReceiveMessageDelegate<VisionMessage>(handleVisionUpdate);

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
        
        private void rfcStart_Click(object sender, EventArgs e) {
            if (!systemStarted) {
                _system.start();
                rfcStatus.BackColor = Color.Green;
                rfcStart.Text = "Stop";
                systemStarted = true;
            } else {
                _system.stop();
                rfcStatus.BackColor = Color.Red;
                rfcStart.Text = "Start";
                systemStarted = false;
            }
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if (keyData == (Keys.Control | Keys.R)) {
                if (systemStarted) {
                    MessageBox.Show("System running. Need to stop system to reload contants.");
                    return false;
                }

                Constants.Load();
                
                LoadConstants();
                if (_predictor is BasicPredictor) {
                    ((BasicPredictor)_predictor).LoadConstants();
                }
                else if (_predictor is AveragingPredictor)
                {
                    ((AveragingPredictor)_predictor).LoadConstants();
                }

                _system.LoadConstants();
                _system.reloadPlays();
                
                playSelectorForm.LoadPlays(_system.getInterpreter().getPlays());
                
                Console.WriteLine("Constants and plays reloaded.");
            }

            if (keyData == (Keys.Control | Keys.C)) {
                if (systemStarted) {
                    MessageBox.Show("System running. Need to stop system to set refbox listener.");
                    return false;
                }
                _system.setRefBoxListener();
                Console.WriteLine("Refbox listener set.");
            }

            return false;
        }        

        #region Logging
        private void loggingInit() {
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

        private void InvalidateTimerHanlder(object obj, EventArgs e) {
            _logFieldDrawer.Invalidate();
        }

        private void btnLogNext_Click(object sender, EventArgs e) {

            if (!_logReader.LogOpen) {
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

        private void btnStartStopLogging_Click(object sender, EventArgs e) {
            /*if (!(_pathFollower.Planner is ILogger))
            {
                MessageBox.Show("Selected MotionPlanner does not implement ILogger interface.");
                return;
            }*/

            if (logging) {
                logging = false;
                btnStartStopLogging.Text = "Start log";
                btnLogOpenClose.Enabled = true;
            }
            else {
                logging = true;
                btnStartStopLogging.Text = "Stop log";
                btnLogOpenClose.Enabled = false;
            }

            // if enabled, start motion planner's logging
            if (_system.Controller is RFCController && ((RFCController)_system.Controller).Planner is ILogger) {
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

        private void btnLogOpenClose_Click(object sender, EventArgs e) {            
            if (_logReader.LogOpen) {
                _logReader.CloseLogFile();
                btnLogOpenClose.Text = "Open log";
                btnLogNext.Enabled = false;
                btnStartStopLogging.Enabled = true;
            }
            else {
                _logReader.OpenLogFile(LOG_FILE, _logLineFormat);
                btnLogOpenClose.Text = "Close log";
                btnLogNext.Enabled = true;
                btnStartStopLogging.Enabled = false;
            }


        }

        #endregion

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