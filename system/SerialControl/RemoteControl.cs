using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using Robocup.Core;
using Robocup.MessageSystem;
using Robocup.Utilities;

namespace Robotics.Commander {
    public partial class RemoteControl : Form {
        private int speed = 17;
        private bool active = false;
        private bool sendcommands_remotehost = false;
        private bool sendcommands_serial = false;
        private MessageSender<RobotCommand> message_sender = null;

        private SerialRobots srobots;
        /*public SerialRobots Serial
        {
            get { return srobots; }
        }*/
        // lf, rf, lb, rb  forward = all positive left = - + + -
        float[] wheel_dx = new float[] { 0.71f, -0.71f, -0.74f, 0.74f };
        float[] wheel_dy = new float[] { 0.71f, 0.71f, 0.68f, 0.68f };
        float[] wheel_baseline = new float[] { 3.23f, 3.23f, 3.23f, 3.23f };
        float[] wheel_radius = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
        // takes in a (unit) vector to translate the robot by
        float[] wheel_speeds = new float[4];

        private int curRobot;

        // HACK REBOOT WORKAROUND
        // for managing reboots
        int _rebootTime;
        System.Timers.Timer t;
       
        KeyboardHook keyboardHook = new KeyboardHook();

        System.Threading.Thread thread;

        public RemoteControl() {
            Form.CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
            textBox1.Text = "8 backspace ======== kill the robot" + "\r\n"
                           + "37 left =========== move left in x " + "\r\n"
                           + "39 right ========== move right in x" + "\r\n"
                           + "38 up ============= move forward in y" + "\r\n"
                           + "40 down =========== move backward in y" + "\r\n"
                           + "188 , ============= rotate anti-clockwise" + "\r\n"
                           + "190 . ============= rotate clockwise" + "\r\n"
                           + "66 b ============== charge kicker to do break-beam kicking" + "\r\n"
                           + "67 c ============== charge kicker (not supported)" + "\r\n"
                           + "32 space ========== fire kicker" + "\r\n"
                           + "75 k ============== stop charging (not supported)" + "\r\n"
                           + "68 d ============== dribbler on" + "\r\n"
                           + "70 f ============== dribbler off" + "\r\n"
                           + "187 = ============= speed up" + "\r\n"
                           + "189 - ============= speed down " + "\r\n"
                           + "80 p ============== stop" + "\r\n"
                           + "82 r ============== reset boards" + "\r\n"
                           + "48 0 ============== computer" + "\r\n"
                           + "49 1 ============== robot1" + "\r\n"
                           + "50 2 ============== robot2" + "\r\n"
                           + "51 3 ============== robot3" + "\r\n"
                           + "52 4 ============== robot4" + "\r\n"
                           + "53 5 ============== robot5" + "\r\n"
                           + "54 6 ============== robot6" + "\r\n"
                           + "55 7 ============== robot7" + "\r\n"
                           + "56 8 ============== robot8" + "\r\n"
                           + "57 9 ============== robot9" + "\r\n"
                           + "27 esc ============ exit";
            curRobot = 0;

            // Global hotkeys

            // register the event that is fired after the key press.
            keyboardHook.KeyPressed += new EventHandler<KeyPressedEventArgs>(backspace_GlobalHotkeyPressed);
            // register the control + alt + F12 combination as hot key.
            keyboardHook.RegisterHotKey(Robocup.Utilities.ModifierKeys.Control | Robocup.Utilities.ModifierKeys.Alt, Keys.Back);
        }

        private void backspace_GlobalHotkeyPressed(object sender, KeyPressedEventArgs e)
        {
            KeyEventArgs eventArgs = new KeyEventArgs(e.Key);
            int oldCurRobot = curRobot;
            for (int i = 0; i < srobots.NumRobots; i++)
            {
                curRobot = i;
                RemoteControl_KeyDown(sender, eventArgs);
                RemoteControl_KeyUp(sender, eventArgs);
            }
            curRobot = oldCurRobot;
        }

        private void toggleSettings(object sender, EventArgs e) {
            active = !active;
            /*textBox1.Enabled = !textBox1.Enabled;
            statusLabel.Enabled = !statusLabel.Enabled;
            OpenCOM.Enabled = !OpenCOM.Enabled;
            reloadMotor.Enabled = !reloadMotor.Enabled;*/
            foreach (Control c in this.Controls) {
                c.Enabled = !active;
            }

            if (sendcommands_serial) {
                if (active) {
                    srobots.Open();


                    _rebootTime = Constants.get<int>("default", "REBOOT_TIME");
                    t = new System.Timers.Timer(_rebootTime);
                    t.AutoReset = true;
                    t.Elapsed += delegate(object s, System.Timers.ElapsedEventArgs eea)
                    {
                        
                        for (int i = 0; i < 6; i++)
                        {
                            //Console.WriteLine("Rebooting robot: "+i);
                            //HACK to stop reboot
                            //srobots.resetBoards(i);
                        }
                    };
                    t.Start();
                    Console.WriteLine("Reboot timer started");
                } else {
                    t.Enabled = false;
                    Console.WriteLine("Reboot timer disabled");
                    srobots.Close();
                }
            }
        }




        private void setMotorSpeeds(int lf, int rf, int lb, int rb) {
           // Console.WriteLine("SerialControl: curRobotID = " + curRobot.ToString() + ";" +
             //   "Setting wheelspeeds to " + lf.ToString() + "," + rf.ToString() + "," + lb.ToString() + "," + rb.ToString());
            sendCommand(curRobot, RobotCommand.Command.MOVE, new WheelSpeeds(lf, rf, lb, rb));
        }
        public void sendMove(int id, int lf, int rf, int lb, int rb) {
            sendCommand(id, RobotCommand.Command.MOVE, new WheelSpeeds(lf, rf, lb, rb));
        }
        
        public void sendCommand(int id, RobotCommand.Command command, WheelSpeeds speeds) {
            if (active)
            {
                if (sendcommands_serial)
                {
                    switch (command)
                    {
                        case RobotCommand.Command.MOVE:
                            srobots.setMotorSpeeds(id, speeds);
                            break;
                        case RobotCommand.Command.KICK:
                            srobots.kick(id);
                            break;
                        case RobotCommand.Command.BEAMKICK:
                            srobots.beamKick(id);
                            break;
                    }
                }
                else if (sendcommands_remotehost)
                {
                    message_sender.Post(new RobotCommand(id, command, speeds));
                    statusLabel.Text = "computercmd";
                }
            }
        }

        public void sendCommand(RobotCommand command)
        {
            sendCommand(command.ID, command.command, command.speeds);
        }

        /*public void charge(int id)
        {
            if (remotecontrol)
            {
                srobots.setCharge(id);
                statusLabel.Text = "charge computercmd";
            }
        }

        public void kick(int id) {
            if (active) {
                srobots.setKick(id);
                statusLabel.Text = "kick computercmd";
            }
        }

        public void stopcharge(int id)
        {
            if (remotecontrol)
            {
                srobots.setStopCharge(id);
                statusLabel.Text = "stopcharge computercmd";
            }
        }*/

        
        private void driveInDirection(float dx, float dy) {
            //Console.Write("speeds: ");
            for (int i = 0; i < 4; i++ ) {
                wheel_speeds[i] = speed*(dx * wheel_dx[i] + dy * wheel_dy[i]) / wheel_radius[i];
                //Console.Write(" " + wheel_speeds[i]);
            }
            // make sure signs are appropriate
            //Console.Write("\n");
            setMotorSpeeds((int)wheel_speeds[0], (int)wheel_speeds[1], (int)wheel_speeds[2], (int)wheel_speeds[3]);
        }

        private void RemoteControl_KeyDown(object sender, KeyEventArgs e) {
            if (active) {
                #region keys
                /*38  up     ============= move forward in y
                40  down   ============= move backward in y
                37  left   ============= move left in x 
                39  right  ============= move right in x
                188 ,      ============= rotate anti-clockwise
                190 .      ============= rotate clockwise
                66  b      ============= break-beam kick
                67  c      ============= charging kicker
                75  k      ============= stop charging kicker
                32  space  ============= fire kicker
                68  d      ============= dribbler on
                70  f      ============= dribbler off
                187 =      ============= speed up
                189 -      ============= speed down 
                80  p      ============= stop  
                48  0      ============= computer
                49  1      ============= robot1
                50  2      ============= robot2
                51  3      ============= robot3
                52  4      ============= robot4
                53  5      ============= robot5
                54  6      ============= robot6
                55  7      ============= robot7
                56  8      ============= robot8
                57  9      ============= robot9
                27  esc    ============= quit*/
                #endregion

                #region keyboard control
                //label1.Text = Convert.ToString(e.KeyValue);
                switch (e.KeyValue) {
                    case 8: // backspace
                        if (srobots == null)
                        {
                            statusLabel.Text = "no registered robots";
                        }
                        else
                        {
                            srobots.stopAll(curRobot);
                            statusLabel.Text = "stopping everything";
                        }
                        break;
                    case 'a':
                    case 37:        // left move left in x
                        //rcom.DriveStraight(oldcommander, 0, 65535);
                        setMotorSpeeds(-speed, speed, speed, -speed);
                        //driveInDirection(-1.0f, 0.0f);
                        statusLabel.Text = "<-x";
                        break;
                    case 'd':
                    case 39:        // right move right in x
                        //rcom.DriveStraight(oldcommander, 1, 65535);
                        setMotorSpeeds(speed, -speed, -speed, speed);
                        //driveInDirection(1.0f, 0.0f);
                        statusLabel.Text = "x->";
                        break;
                    case 'w':
                    case 38:        // up move forward in y
                        //rcom.DriveStraight(oldcommander, 2, 65535);
                        setMotorSpeeds(speed, speed, speed, speed);
                        //driveInDirection(0.0f, 1.0f);
                        //rcom.DriveDir(oldcommander, Int32.Parse(forwardDir.Text), 65535);
                        statusLabel.Text = "^y";
                        break;
                    case 's':
                    case 40:        // down move backward in y
                        //rcom.DriveStraight(oldcommander, 3, 65535);
                        setMotorSpeeds(-speed, -speed, -speed, -speed);
                        //driveInDirection(0.0f, -1.0f);
                        statusLabel.Text = "yv";
                        break;
                    case 'q':
                    case 188:       // , rotate anti-clockwise
                        //rcom.Rotate(oldcommander, 0, 65535);
                        setMotorSpeeds(-speed, speed, -speed, speed);
                        statusLabel.Text = "anti clock";
                        break;
                    case 'e':
                    case 190:       // . rotate clockwise
                        //rcom.Rotate(oldcommander, 1, 65535);
                        setMotorSpeeds(speed, -speed, speed, -speed);
                        statusLabel.Text = "clock";
                        break;
                    case 66:        // b break-beam kick
                        statusLabel.Text = "Charging for break-beam";
                        srobots.beamKick(curRobot);
                        break;
                    case 67:        // c charge kicker
                        //rcon.setOther(comboTarget.SelectedIndex, comboSource.SelectedIndex, 0);
                        //srobots.setCharge(curRobot);
                        //statusLabel.Text = "charging";
                        statusLabel.Text = "no charging";
                        break;
                    case 32:        // space fire kicker
                        //srobots.setStopCharge(curRobot);
                        srobots.setKick(curRobot);
                        //srobots.setStopCharge(curRobot);
                        statusLabel.Text = "kick";
                        break;
                    case 75:        // k stop charging
                        //srobots.setStopCharge(curRobot);
                        //statusLabel.Text = "stop charge";
                        statusLabel.Text = "no chargingi";
                        break;
                    case 68:        // d dribbler on
                        srobots.startDribbler(curRobot);
                        //rcon.setOther(comboTarget.SelectedIndex, comboSource.SelectedIndex, 3);
                        statusLabel.Text = "dribbler is on";
                        break;
                    case 70:        // f dribbler off
                        srobots.stopDribbler(curRobot);
                        //rcon.setOther(comboTarget.SelectedIndex, comboSource.SelectedIndex, 4);
                        statusLabel.Text = "dribbler is dead";
                        break;
                    case 80:        // p stop   
                        //rcon.setAllMotor(oldcommander,comboTarget.SelectedIndex, comboSource.SelectedIndex, 0, 0, 0, 0, 0, 65535);
                        setMotorSpeeds(0, 0, 0, 0);
                        statusLabel.Text = "zzZz";
                        break;
                    case 'r':
                        if (sendcommands_serial)
                            srobots.resetBoards(curRobot);
                        break;
                    case '=':
                    case 187:
                        speed += 1;
                        statusLabel.Text = "new speed: " + speed;
                        break;
                    case '-':
                    case 189:
                        speed -= 1;
                        statusLabel.Text = "new speed: " + speed;
                        break;
                    case 48:        // 0 robot0
                    case 49:        // 1 robot1
                    case 50:        // 2 robot2
                    case 51:        // 3 robot3
                    case 52:        // 4 robot4
                    case 53:        // 5 robot5
                    case 54:        // 6 robot6
                    case 55:        // 7 robot7
                    case 56:        // 8 robot8
                    case 57:        // 9 robot9
                        curRobot = e.KeyValue - 48;
                        //rcom.SwitchRobot(curRobot);
                        lblID.Text = "RobotID: " + curRobot;
                        break;
                    case 27: // exit
                        toggleSettings(null, null);
                        statusLabel.Text = "breaking: " + active;
                        break;
                    case 79:
                    case 'o':
                        driveInDirection(-1.0f, -1.0f);
                        statusLabel.Text = "o /";
                        break;
                    case 76:
                    case 'l':
                        driveInDirection(1.0f, 1.0f);
                        statusLabel.Text = "l /";
                        break;
                    case 84:
                        //rcom.DriveStraight(oldcommander, 2, 65535);
                        //setMotorSpeeds(speed, speed, speed, speed);
                        driveInDirection(0.0f, 1.0f);
                        //srobots.setCharge(curRobot);
                        //rcom.DriveDir(oldcommander, Int32.Parse(forwardDir.Text), 65535);
                        statusLabel.Text = "^y";
                        break;
                    default:
                        //if(e.KeyCode==Keys.Escape)        // exit
                        //    toggleSettings();
                        setMotorSpeeds(0, 0, 0, 0);
                        statusLabel.Text = "other: " + e.KeyValue.ToString();
                        break;
                }
                #endregion

                return;

            }

        }

        
        private void RemoteControl_KeyUp(object sender, KeyEventArgs e) {
            if (active) {
                setMotorSpeeds(0, 0, 0, 0);
                (thread = new System.Threading.Thread(delegate()
                {
                    System.Threading.Thread.Sleep(100);
                    setMotorSpeeds(0, 0, 0, 0);
                })).Start();
                /*rcon.setAllMotor(oldcommander, curRobot, 0 //combosource
                    , 0, 0, 0, 0, 0, 65535);*/
                statusLabel.Text = "zzZz";
            }
        }


        private void RemoteControl_Load(object sender, EventArgs e) {
            //this.toggleSettings(sender, e);

            //rcom.LoadMotorScale("C:\\Microsoft Robotics Studio (1.0)\\samples\\MasterCommander\\scaling.txt");
        }


        private void button3_Click(object sender, EventArgs e) {
            srobots.loadMotorScale();
            Constants.Load();
            srobots.ReloadConstants();
        }

        private void radioButtonSerial_CheckedChanged(object sender, EventArgs e) {
            sendcommands_serial = radioButtonSerial.Checked;
            if (sendcommands_serial)
                srobots = new SerialRobots();
            else
                srobots = null;
        }

        private void radioButtonRemote_CheckedChanged(object sender, EventArgs e) {
            sendcommands_remotehost = radioButtonRemote.Checked;
            if (radioButtonRemote.Checked) {
                if (textBoxRemoteHost.Text == "localhost") {
                    MessageBox.Show("don't create a loop like that!");
                }
                this.message_sender = Messages.CreateClientSender<RobotCommand>(
                    textBoxRemoteHost.Text, Constants.get<int>("ports", "RemoteControlPort"));
            } else {
                this.message_sender.Close();
                this.message_sender = null;
            }
        }

    }
}
