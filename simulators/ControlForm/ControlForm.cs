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

using Vision;
namespace Robocup.ControlForm {
    



    public partial class ControlForm : Form{
        bool serialConnected = false;
        RemoteRobots _serial;

        bool visionConnected = false;
        int MESSAGE_SENDER_PORT = Constants.get<int>("ports", "VisionDataPort");
        Robocup.MessageSystem.MessageReceiver<Robocup.Core.VisionMessage> _vision;

        FieldStateForm _field;

        bool systemStarted = false;
        RFCSystem _system;

        BasicPredictor _basicPredictor;
        ICoordinateConverter converter = new Robocup.Utilities.ControlFormConverter(400,540, 5, 5);

        public ControlForm() {
            InitializeComponent();

            _field = new FieldStateForm();
            _field.Show();

            createSystem();

            this.Focus();
        }

        private void createSystem() {
            _serial = new RemoteRobots();
            _system = new RFCSystem();

            _basicPredictor = new BasicPredictor();
            // add vision predictor hooked up to vision
            _system.registerAcceptor(_basicPredictor);
            _system.registerPredictor(_basicPredictor);
            
            // add serial commander
            _system.registerCommander(_serial);

            // todo
            _system.initialize();
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

        private void handleVisionUpdate(VisionMessage msg) {
            _field.UpdateState(msg);
            List<RobotInfo> ours = new List<RobotInfo>();
            
            foreach (VisionMessage.RobotData robot in msg.OurRobots) {
                ours.Add(new RobotInfo(robot.Position, robot.Orientation, robot.ID));
            }

            List<RobotInfo> theirs = new List<RobotInfo>();
            foreach (VisionMessage.RobotData robot in msg.TheirRobots) {
                theirs.Add(new RobotInfo(robot.Position, robot.Orientation, robot.ID));
            }

            _basicPredictor.updatePartOurRobotInfo(ours, "local");
            _basicPredictor.updatePartTheirRobotInfo(theirs, "local");
            if (msg.BallPosition!=null)
                _basicPredictor.updateBallInfo(new BallInfo(msg.BallPosition));

            _system.drawCurrent(_field.getGraphics(), converter);
        }

        private void visionConnect_Click(object sender, EventArgs e) {
            try {
                if (!visionConnected) {
                    _vision = Robocup.MessageSystem.Messages.CreateClientReceiver<Robocup.Core.VisionMessage>(visionHost.Text, MESSAGE_SENDER_PORT);
                    _vision.MessageReceived += new Robocup.MessageSystem.ReceiveMessageDelegate<VisionMessage>(handleVisionUpdate);

                    visionStatus.BackColor = Color.Green;
                    visionConnect.Text = "Disconnect";
                    visionConnected = true;      
                } else {
                    _vision.Close();
                    visionStatus.BackColor = Color.Red;
                    visionConnect.Text = "Connect";
                    visionConnected = false;
                }
            } catch (Exception except) {
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


    }

    public class RemoteRobots : IRobots {
        int SERIAL_SENDER_PORT = Constants.get<int>("ports", "RemoteControlPort");
        Robocup.MessageSystem.MessageSender<Robocup.Core.WheelCommand> _serial;

        public RemoteRobots() {
        }

        public bool start(String host) {
            _serial = Robocup.MessageSystem.Messages.CreateClientSender<Robocup.Core.WheelCommand>(host, SERIAL_SENDER_PORT);
            return (_serial != null);
        }

        public void stop() {
            _serial.Close();
        }

        #region IRobots Members
        const float scaling = 1.0f;
        public void setMotorSpeeds(int robotID, WheelSpeeds wheelSpeeds) {
            if (robotID < 0 || _serial==null) return;
            _serial.Post(new WheelCommand(robotID, new WheelSpeeds((int)(wheelSpeeds.lf / scaling), (int)(wheelSpeeds.rf / scaling), (int)(wheelSpeeds.lb / scaling), (int)(wheelSpeeds.rb / scaling))));
            Console.WriteLine("RemoteRobots::setMotorSpeeds: " + wheelSpeeds.lf / scaling + " "
                + wheelSpeeds.rf / scaling + " " + wheelSpeeds.lb / scaling + " " + wheelSpeeds.rb / scaling + " ");
        }

        public void kick(int robotID) {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}