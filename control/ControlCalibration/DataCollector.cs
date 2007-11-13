using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.IO.Compression;

using Robocup.MessageSystem;
using Robocup.Core;
using Robocup.Utilities;
using Robocup.CoreRobotics;
using Robotics.Commander;

namespace Robocup.MotionControl
{
    public partial class DataCollector : Form
    {

        MessageReceiver<VisionMessage> receiver;
        SerialRobots serial;
        LogWriter<VisionOrCommand> logger;
        HighResTimer timer;

        public DataCollector()
        {
            InitializeComponent();

            comboBoxCollectionType.Items.Add("1 second constant pulse");

            receiver = Messages.CreateClientReceiver<VisionMessage>("localhost", Constants.get<int>("ports", "VisionDataPort"));
            receiver.MessageReceived += VisionMessageReceived;

            serial = new SerialRobots();

            FileStream fs = new FileStream("../../collecteddata.log.zip", FileMode.Create);
            GZipStream gz = new GZipStream(fs, CompressionMode.Compress);
            logger = new LogWriter<VisionOrCommand>(gz);

            timer = new HighResTimer();
        }

        bool running = false;
        private void buttonStart_Click(object sender, EventArgs e)
        {
            running = !running;

            //if we told it to stop, then insert a "break" so that when we read the logs
            //we know that it's the end of one run
            if (!running)
                logger.InsertBreak();
            //if we told it to start, start the timer and start sending commands
            if (running)
            {
                timer.Start();
                //TODO start a loop (via a thread or a timer) that calls SendCommand() periodically
            }

            if (running)
                buttonStart.Text = "Stop";
            else
                buttonStart.Text = "Start";
        }

        private void SendCommand()
        {
            int id = int.Parse(textBoxRobotID.Text);
            string function = comboBoxCollectionType.SelectedItem.ToString();
            double t = timer.Duration;
            //TODO compute some function of t and send that command to the robot,
            //via a call like this
            serial.setMotorSpeeds(id, new WheelSpeeds());
        }

        private void VisionMessageReceived(VisionMessage message)
        {
            if (running)
            {
                //TODO log the vision message
            }
        }

        private void DataCollector_FormClosing(object sender, FormClosingEventArgs e)
        {
            logger.Close();
        }
    }
}