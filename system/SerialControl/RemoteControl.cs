using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using Robocup.Core;

namespace Robotics.Commander
{
    partial class RemoteControl : Form
    {
        private int speed = 127;
        public bool remotecontrol = false;

        private SerialRobots srobots = new SerialRobots();
        /*public SerialRobots Serial
        {
            get { return srobots; }
        }*/
        private int curRobot;

        public RemoteControl()
        {
            InitializeComponent();
            textBox1.Text = "8 backspace ======= kill the robot" + "\r\n"
                           + "37 left =========== move left in x " + "\r\n"
                           + "39 right ========== move right in x" + "\r\n"
                           + "38 up ============= move forward in y" + "\r\n"
                           + "40 down =========== move backward in y" + "\r\n"
                           + "188 , ============= rotate anti-clockwise" + "\r\n"
                           + "190 . ============= rotate clockwise" + "\r\n"
                           + "67 c ============== charge kicker" + "\r\n"
                           + "32 space ========== fire kicker" + "\r\n"
                           + "75 k ============== fire chipkicker" + "\r\n"
                           + "68 d ============== dribbler on" + "\r\n"
                           + "70 f ============== dribbler off" + "\r\n"
                           + "187 = ============= speed up" + "\r\n"
                           + "189 - ============= speed down " + "\r\n"
                           + "80 p ============== stop" + "\r\n"
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
        }

        private void toggleSettings(object sender, EventArgs e)
        {
            remotecontrol = !remotecontrol;
            textBox1.Enabled = !textBox1.Enabled;
            statusLabel.Enabled = !statusLabel.Enabled;
            OpenCOM.Enabled = !OpenCOM.Enabled;
            reloadMotor.Enabled = !reloadMotor.Enabled;

            if (remotecontrol)
            {
                srobots.Open();
                //button1.Text = "Close COM";
            }
            else
            {
                srobots.Close();
                //button1.Text = "Open COM";
            }
        }



        public void sendMove(int id, int lf, int rf, int lb, int rb)
        {
            if (remotecontrol)
            {
                srobots.setMotorSpeeds(id, new WheelSpeeds(lf, rf, lb, rb));
                statusLabel.Text = "computercmd";
            }
        }
        public void sendMove(int id, WheelSpeeds speeds)
        {
            if (remotecontrol)
            {
                srobots.setMotorSpeeds(id, speeds);
                statusLabel.Text = "computercmd";
            }
        }

        /*public void charge(int id)
        {
            if (remotecontrol)
            {
                srobots.setCharge(id);
                statusLabel.Text = "charge computercmd";
            }
        }*/

        public void kick(int id)
        {
            if (remotecontrol)
            {
                srobots.setKick(id);
                statusLabel.Text = "kick computercmd";
            }
        }

        /*public void stopcharge(int id)
        {
            if (remotecontrol)
            {
                srobots.setStopCharge(id);
                statusLabel.Text = "stopcharge computercmd";
            }
        }*/

        private void setMotorSpeeds(int lf, int rf, int lb, int rb)
        {
            srobots.setMotorSpeeds(curRobot, new WheelSpeeds(lf, rf, lb, rb));
        }

        private void From1_KeyDown(object sender, KeyEventArgs e)
        {
            if (remotecontrol)
            {
                #region keys
                /*38  up     ============= move forward in y
                40  down   ============= move backward in y
                37  left   ============= move left in x 
                39  right  ============= move right in x
                188 ,      ============= rotate anti-clockwise
                190 .      ============= rotate clockwise
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
                switch (e.KeyValue)
                {
                    case 8: // backspace
                        srobots.stopAll(curRobot);
                        statusLabel.Text = "stopping everything";
                        break;
                    case 'a':
                    case 37:        // left move left in x
                        //rcom.DriveStraight(oldcommander, 0, 65535);
                        setMotorSpeeds(-speed, speed, speed, -speed);
                        statusLabel.Text = "<-x";
                        break;
                    case 'd':
                    case 39:        // right move right in x
                        //rcom.DriveStraight(oldcommander, 1, 65535);
                        setMotorSpeeds(speed, -speed, -speed, speed);
                        statusLabel.Text = "x->";
                        break;
                    case 'w':
                    case 38:        // up move forward in y
                        //rcom.DriveStraight(oldcommander, 2, 65535);
                        setMotorSpeeds(speed, speed, speed, speed);
                        //rcom.DriveDir(oldcommander, Int32.Parse(forwardDir.Text), 65535);
                        statusLabel.Text = "^y";
                        break;
                    case 's':
                    case 40:        // down move backward in y
                        //rcom.DriveStraight(oldcommander, 3, 65535);
                        setMotorSpeeds(-speed, -speed, -speed, -speed);
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
                    case 67:        // c Convert.ToStringge kicker
                        //rcon.setOther(comboTarget.SelectedIndex, comboSource.SelectedIndex, 0);
                        srobots.setCharge(curRobot);
                        statusLabel.Text = "charging";
                        break;
                    case 32:        // space fire kicker
                        srobots.setKick(curRobot);
                        statusLabel.Text = "kick";
                        break;
                    case 75:        // k stop charging
                        srobots.setStopCharge(curRobot);
                        statusLabel.Text = "stop charge";
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
                    case '=':
                    case 187:
                        speed += 10;
                        statusLabel.Text = "new speed: " + speed;
                        break;
                    case '-':
                    case 189:
                        speed -= 10;
                        statusLabel.Text = "new speed: " + speed;
                        break;
                    case 48:        // 0 computer
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
                        statusLabel.Text = "breaking: " + remotecontrol;
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

        private void From1_KeyUp(object sender, KeyEventArgs e)
        {
            if (remotecontrol)
            {
                setMotorSpeeds(0, 0, 0, 0);
                /*rcon.setAllMotor(oldcommander, curRobot, 0 //combosource
                    , 0, 0, 0, 0, 0, 65535);*/
                statusLabel.Text = "zzZz";
            }
        }


        private void RemoteControl_Load(object sender, EventArgs e)
        {
            this.toggleSettings(sender, e);

            //rcom.LoadMotorScale("C:\\Microsoft Robotics Studio (1.0)\\samples\\MasterCommander\\scaling.txt");
        }


        private void button3_Click(object sender, EventArgs e)
        {
            srobots.loadMotorScale();
        }

    }
}
