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

using System.Threading;


namespace Robocup.MotionControl
{
    public partial class DataCollector : Form
    {
        MessageReceiver<VisionMessage> visionreceiver;
        SerialRobots serialoutput;
        SerialInput serialinput;
        LogWriter<VisionOrCommand> logger;
        HighResTimer timer;

        Function current;
        Dictionary<string, Function> functions = new Dictionary<string, Function>();

        StreamWriter csv;

        Robotics.Commander.RemoteControl remoteControl;

        public DataCollector()
        {
            Form.CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();

            remoteControl = new RemoteControl();
            remoteControl.Show();

            functions.Add("Ramp function", new RampFunction());
            functions.Add("Sine wave", new SineWave());
            functions.Add("Step function", new StepFunction());

            comboBoxFunctionList.Items.AddRange(new List<string>(functions.Keys).ToArray());
            comboBoxFunctionList.SelectedIndex = 0;

            textBoxSerialPort.Text = Constants.get<string>("ports", "SerialPort");

            FileStream fs = new FileStream("collecteddata.log.zip", FileMode.Create);
            GZipStream gz = new GZipStream(fs, CompressionMode.Compress);
            logger = new LogWriter<VisionOrCommand>(gz);

            timer = new HighResTimer();
            timer.Start();

            listBoxInputHistory.Items.Add(SerialInput.SerialInputMessage.ToStringHeader());

            buttonStop.Enabled = false;
            buttonSendCustomSerial.Enabled = false;
        }

        bool running = false;

        Thread thread;

        bool hasrun = false;

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (!running)
            {
                running = true;
                //if we told it to stop, then insert a "break" so that when we read the logs
                //we know that it's the end of one run
                if (!hasrun)
                    logger.InsertBreak();
                hasrun = true;

                lastSerialData = 0;
                thread = new Thread(Run);
                thread.Start();
            }
        }

        private void Run(object state)
        {
            timer.Start();
            listBoxCommandHistory.Items.Clear();
            listBoxCommandHistory.Items.Add("lf\trf\tlb\trb");
            buttonStart.Enabled = false;
            buttonStop.Enabled = true;
            while (running)
            {
                SendCommand();
                Thread.Sleep(int.Parse(textBoxPeriod.Text));
            }
            buttonStart.Enabled = true;
            buttonStop.Enabled = false;
        }

        private void SendCommand()
        {
            string idtext = textBoxRobotID.Text;
            if (idtext == null)
                return;
            int id = int.Parse(idtext);

            timer.Stop();
            double t = timer.Duration;
            labelTime.Text = "t = " + t;

            WheelSpeeds command = current.eval(t);

            if (checkBoxDoSend.Checked)
                serialoutput.setMotorSpeeds(id, command);

            logger.LogObject(new VisionOrCommand(command));

            listBoxCommandHistory.Items.Insert(1, command.lf + "\t" + command.rf + '\t' + command.lb + '\t' + command.rb);
            if (listBoxCommandHistory.Items.Count > 16)
                listBoxCommandHistory.Items.RemoveAt(16);
            if (t > double.Parse(textBoxTestDuration.Text))
                running = false;
        }

        // two overloaded functions to deal with VisionMessage and Command individually
        private void VisionMessageReceived(VisionMessage message)
        {
            if (running)
            {
                logger.LogObject(new VisionOrCommand(message.OurRobots[0]));
            }
        }

        bool closing = false;
        private void SerialValueReceived(double t, SerialInput.SerialInputMessage message)
        {
            lock (csv)
            {
                if (!closing /*&& running*/)
                {
                    csv.WriteLine(t + ", " + current.eval(t).lb + ", " +
                        message.Encoder + ", " + message.Error + ", " + message.WheelCommand + ", " + message.Extra + ", " + message.Extra2);
                    labelTime.Text = "t = " + t;
                }

                listBoxInputHistory.Items.Insert(1, message);
                if (listBoxInputHistory.Items.Count > 16)
                    listBoxInputHistory.Items.RemoveAt(16);
            }
        }

        private double lastSerialData = 0;
        private void SerialDataReceived(SerialInput.SerialInputMessage[] values)
        {
            timer.Stop();
            double t = timer.Duration;
            int n = values.Length;
            for (int i = 0; i < n; i++)
            {
                double thist = (t - lastSerialData) / n * (i + 1) + lastSerialData;
                SerialValueReceived(thist, values[i]);
            }
            lastSerialData = t;
        }

        private void DataCollector_FormClosing(object sender, FormClosingEventArgs e)
        {
            closing = true;
            logger.Close();
            if (csv != null)
                lock (csv)
                {
                    csv.Close();
                }
            running = false;
        }

        private void comboBoxFunctionList_SelectedIndexChanged(object sender, EventArgs e)
        {
            string function = comboBoxFunctionList.SelectedItem.ToString();
            current = functions[function];
            propertyGrid1.SelectedObject = current;
        }

        private void checkBoxDoSend_CheckedChanged(object sender, EventArgs e)
        {
            bool check = checkBoxDoSend.Checked;
            if (check)
            {
                serialoutput = new SerialRobots(textBoxSerialPort.Text);
                serialoutput.Open();
                buttonSendCustomSerial.Enabled = true;
            }
            else
            {
                serialoutput.Close();
                serialoutput = null;
                buttonSendCustomSerial.Enabled = false;
            }
        }

        void CloseConnections()
        {
            if (serialinput != null)
            {
                serialinput.Close();
                serialinput = null;
            }
            if (visionreceiver != null)
            {
                visionreceiver.Close();
                visionreceiver = null;
            }
        }
        private void buttonConnectVision_Click(object sender, EventArgs e)
        {
            CloseConnections();
            string host = textBoxVisionHostname.Text;
            visionreceiver = Messages.CreateClientReceiver<VisionMessage>(host, Constants.get<int>("ports", "VisionDataPort"));
            if (visionreceiver != null)
            {
                visionreceiver.MessageReceived += VisionMessageReceived;
                labelConnectedTo.Text = "vision on " + host;
            }
            else
                labelConnectedTo.Text = "(none)";
        }

        private void buttonListenSerial_Click(object sender, EventArgs e)
        {
            string port = textBoxSerialPort.Text;
            CloseConnections();
            serialinput = SerialInput.CreateSerialInput(port);

            if (serialinput != null)
            {
                if (csv == null)
                {
                    csv = new StreamWriter("data.csv", false);

                    csv.WriteLine("time, cs command, encoder, error, ee command, extra, extra2");
                }

                serialinput.ValueReceived += SerialDataReceived;
                labelConnectedTo.Text = "serial port " + port;
                timer.Start();
            }
            else
                labelConnectedTo.Text = "(none)";
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            running = false;
        }

        private void buttonSendCustomSerial_Click(object sender, EventArgs e)
        {
            if (serialoutput != null)
            {
                serialoutput.sendCommand(textBoxSerialCommand.Text);
            }
        }

        private void buttonSendPIDConstants_Click(object sender, EventArgs e)
        {
            serialoutput.SetPIDConstants(int.Parse(textBoxRobotID.Text), byte.Parse(textBoxconstP.Text),
                byte.Parse(textBoxconstI.Text), byte.Parse(textBoxconstD.Text));
        }

        private void checkBoxLimitCommands_CheckedChanged(object sender, EventArgs e)
        {

            if (serialoutput != null)
            {
                //sets whether or not the serial controller will do any rate limiting on the commands
                if (checkBoxLimitCommands.Checked)
                {
                    serialoutput.MaxAcceleration = 50;
                    serialoutput.MaxVelocityStep = 20;
                }
                else
                {
                    serialoutput.MaxAcceleration = int.MaxValue;
                    serialoutput.MaxVelocityStep = int.MaxValue;
                }
            }
        }
    }

    interface Function
    {
        WheelSpeeds eval(double t);
    }
    class SineWave : Function
    {
        private double amplitude = 127;
        public double Amplitude
        {
            get { return amplitude; }
            set { amplitude = value; }
        }
        private double period = 2.0;
        [Description("The period of the sine wave, in seconds")]
        public double Period
        {
            get { return period; }
            set { period = value; }
        }

        public WheelSpeeds eval(double t)
        {
            int speed = (int)(amplitude * Math.Sin(t / period * 2 * Math.PI));
            return new WheelSpeeds(speed, speed, speed, speed);
        }
    }
    class StepFunction : Function
    {
        private double initialpower = 0;
        public double InitialPower
        {
            get { return initialpower; }
            set { initialpower = value; }
        }

        private double finalpower = 127;
        public double Finalpower
        {
            get { return finalpower; }
            set { finalpower = value; }
        }

        private double transition = 2;
        [Description("The time at which it transitions from the initial value to the final value")]
        public double TransitionTime
        {
            get { return transition; }
            set { transition = value; }
        }

        public WheelSpeeds eval(double t)
        {
            int speed = (int)(t > transition ? finalpower : initialpower);
            return new WheelSpeeds(speed, speed, speed, speed);
        }
    }
    public class RampFunction : Function
    {
        private double waittime = .1;

        public double WaitTime
        {
            get { return waittime; }
            set { waittime = value; }
        }
        private int start = 0;

        public int Start
        {
            get { return start; }
            set { start = value; }
        }
        private int change = 0;

        public int Change
        {
            get { return change; }
            set { change = value; }
        }



        public WheelSpeeds eval(double t)
        {
            int num = (int)(t / waittime);
            int speed = start + num * change;
            return new WheelSpeeds(speed, speed, speed, speed);
        }
    }
}
