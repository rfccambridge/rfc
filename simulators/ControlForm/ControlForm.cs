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

        //FieldStateForm _field;
        FieldDrawerForm drawer;
        PlaySelectorForm playSelectorForm;

        bool systemStarted = false;
        RFCSystem _system;

        BasicPredictor _predictor;
        //ICoordinateConverter converter = new Robocup.Utilities.ControlFormConverter(420,610, 5, 5);
        ICoordinateConverter converter;

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

            createSystem();

            this.Focus();
        }

        public void LoadConstants()
        {            
            OUR_TEAM = (Constants.get<string>("configuration", "OUR_TEAM") == "YELLOW" ? VisionMessage.Team.YELLOW : VisionMessage.Team.BLUE);
        }

        private void createSystem() {
            _serial = new RemoteRobots();
            _system = new RFCSystem();

            _predictor = new BasicPredictor();
            // add vision predictor hooked up to vision
            _system.registerAcceptor(_predictor);
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

        private void serialConnect_Click(object sender, EventArgs e) {
            try {
                if (!serialConnected) {
                    if (_serial.start(serialHost.Text)) {
                        serialStatus.BackColor = Color.Green;
                        serialConnect.Text = "Disconnect";
                        serialConnected = true;
                    }
                } else {
                    _serial.stop();
                    serialStatus.BackColor = Color.Red;
                    serialConnect.Text = "Connect";
                    serialConnected = false;
                }
            } catch (Exception except) {
                Console.WriteLine(except.StackTrace);
            }

        }


        private void handleVisionUpdateTop(VisionMessage msg)
        {
            handleVisionUpdate(msg, TOP_CAMERA);
        }
        private void handleVisionUpdateBottom(VisionMessage msg)
        {
            handleVisionUpdate(msg, BOTTOM_CAMERA);
        }

        object field_lock = new object();
        object predictor_lock = new object();
        private void handleVisionUpdate(VisionMessage msg, String cameraName) {
          
            List<RobotInfo> ours = new List<RobotInfo>();
            List<RobotInfo> theirs = new List<RobotInfo>();

            foreach (VisionMessage.RobotData robot in msg.Robots)
            {
                (robot.Team == OUR_TEAM ? ours : theirs).Add(new RobotInfo(robot.Position, robot.Orientation, robot.ID));
            }

            lock (predictor_lock)
            {
                _predictor.updatePartOurRobotInfo(ours, cameraName);
                _predictor.updatePartTheirRobotInfo(theirs, cameraName);
                if (msg.BallPosition != null) {
                    //Vector2 ballposition = new Vector2(2 + 1.01 * (msg.BallPosition.X - 2), msg.BallPosition.Y);                    
                    _predictor.updateBallInfo(new BallInfo(msg.BallPosition));
                }
                else {
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

        private void visionTopConnect_Click(object sender, EventArgs e) {
            try {
                if (!visionTopConnected) {
                    _visionTop = Robocup.MessageSystem.Messages.CreateClientReceiver<Robocup.Core.VisionMessage>(visionTopHost.Text, MESSAGE_SENDER_PORT);
                    _visionTop.MessageReceived += new Robocup.MessageSystem.ReceiveMessageDelegate<VisionMessage>(handleVisionUpdateTop);

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
                Console.WriteLine("Problem connecting to vision: " + except.ToString());
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

        private void ControlForm_Load(object sender, EventArgs e)
        {
            Console.WriteLine("test");
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            // save Tsai points
            if (keyData == (Keys.Control | Keys.R)) {
                if (systemStarted) {
                    MessageBox.Show("System running. Need to stop system to reload contants.");
                    return false;
                }

                Constants.Load();
                
                LoadConstants();
                _predictor.LoadConstants();
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
                Console.WriteLine("Problem connecting to vision: " + except.ToString());
                Console.WriteLine(except.StackTrace);
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