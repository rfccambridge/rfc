using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Robocup.Core;
using Robotics.Commander;


namespace JoystickSample
{
    public partial class frmMain : Form
    {
        private JoystickInterface.Joystick jst;

        // CONSTANTS
        int JOYSTICK_AXIS_MAXIMUM = 65535;
        int NUM_ROBOTS = 5;

        // robotics
        int currentRobotID = 0;

        SerialRobots robotcommander;

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            // grab the joystick
            jst = new JoystickInterface.Joystick(this.Handle);
            string[] sticks = jst.FindJoysticks();
            jst.AcquireJoystick(sticks[0]);

            // add the axis controls to the axis container
            for (int i = 0; i < jst.AxisCount; i++)
            {
                Axis ax = new Axis();
                ax.AxisId = i + 1;
                flpAxes.Controls.Add(ax);
            }

            // add the button controls to the button container
            for (int i = 0; i < jst.Buttons.Length; i++)
            {
                JoystickSample.Button btn = new Button();
                btn.ButtonId = i + 1;
                btn.ButtonStatus = jst.Buttons[i];
                flpButtons.Controls.Add(btn);
            }

            // update the robot ID
            updateRobotID();

            // create a SerialRobots commander
            string port = "COM" + ((int)udPort.Value).ToString();
            robotcommander = new SerialRobots(port);
            robotcommander.Open();

            // start updating positions
            tmrUpdateStick.Enabled = true;
        }

        private void tmrUpdateStick_Tick(object sender, EventArgs e)
        {
            // get status
            jst.UpdateStatus();

            // update the axes positions
            foreach (Control ax in flpAxes.Controls)
            {
                if (ax is Axis)
                {
                    switch (((Axis)ax).AxisId)
                    {
                        case 1:
                            ((Axis)ax).AxisPos = jst.AxisA;
                            break;
                        case 2:
                            ((Axis)ax).AxisPos = jst.AxisB;
                            break;
                        case 3:
                            ((Axis)ax).AxisPos = jst.AxisC;
                            break;
                        case 4:
                            ((Axis)ax).AxisPos = jst.AxisD;
                            break;
                        case 5:
                            ((Axis)ax).AxisPos = jst.AxisE;
                            break;
                        case 6:
                            ((Axis)ax).AxisPos = jst.AxisF;
                            break;
                    }
                }
            }

            // update each button status
            foreach (Control btn in flpButtons.Controls)
            {
                if (btn is JoystickSample.Button)
                {
                    ((JoystickSample.Button)btn).ButtonStatus =
                        jst.Buttons[((JoystickSample.Button)btn).ButtonId - 1];
                }
            }

            sendMotorCommandsFromJoystick();
        }

        // update which robot is currently being commanded based on its ID. If
        // two buttons are being pressed, set the smaller one
        private void updateRobotID()
        {
            for (int i = 0; i < NUM_ROBOTS; i++)
            {
                // check if this 
                if (jst.Buttons[i])
                {
                    // this is the current robot
                    currentRobotID = i;

                    // the smallest button being pressed takes precedence
                    return;
                }
            }
        }

        private void sendMotorCommandsFromJoystick()
        {
            // first and foremost, check for pressed buttons
            updateRobotID();
            
            // compute parameters based on the current state of the joystick
            double forwardComponent = (1 - ((float)jst.AxisD) / JOYSTICK_AXIS_MAXIMUM) - .5;
            double lateralComponent = ((float)jst.AxisC) / JOYSTICK_AXIS_MAXIMUM - .5;
            double angularComponent = 1 - ((float)jst.AxisA) / JOYSTICK_AXIS_MAXIMUM - .5;

            Console.WriteLine("Forward component: " + forwardComponent + " lateral component " + lateralComponent +
                                "angular component" + angularComponent);

            // compute wheel speeds
            // assume that we are facing in the y direction (90 degrees), so the lateral direction is
            // x, the forward component is y, and the angular is what it is
            WheelSpeeds speeds = WheelSpeedConverter.convert(lateralComponent, forwardComponent,
                                                                angularComponent, Math.PI / 2);

            // send the actual wheel speeds using SerialRobots
            Console.WriteLine("Setting wheel speeds on robot " + currentRobotID + " to " + speeds);
            robotcommander.setMotorSpeeds(currentRobotID, speeds);
        }
    }
}