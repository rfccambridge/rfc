using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Robocup.Utilities;
using Robocup.Core;

namespace Robocup.MotionControl
{
    public partial class ModelCalibrator : Form
    {
        public ModelCalibrator()
        {
            InitializeComponent();
            textBoxDirectory.Text = "../../resources/Control calibration data/nice.log";
        }

        string path = ".";

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            bool exists = File.Exists(textBoxDirectory.Text);
            textBoxDirectory.ForeColor = exists ? Color.Black : Color.Red;
            buttonLoad.Enabled = exists;
            if (exists)
                path = textBoxDirectory.Text;
        }

        private void buttonPickDirectory_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = new FileInfo(path).Directory.FullName;
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                path = openFileDialog1.FileName;
                textBoxDirectory.Text = path;
            }
        }

        List<List<LogMessage<VisionMessage.RobotData>>> vision = null;
        List<List<LogMessage<WheelSpeeds>>> commands = null;

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            Stream s = File.Open(path, FileMode.Open);
            Stream gz = new System.IO.Compression.GZipStream(s, System.IO.Compression.CompressionMode.Decompress);

            List<List<LogMessage<VisionOrCommand>>> logs = LogWriter<VisionOrCommand>.ReadAndBreakLog(gz, false);
            vision = logs.ConvertAll<List<LogMessage<VisionMessage.RobotData>>>(delegate(List<LogMessage<VisionOrCommand>> lst)
            {
                return lst.FindAll(delegate(LogMessage<VisionOrCommand> log)
                {
                    return log.obj.vision != null;
                }).ConvertAll<LogMessage<VisionMessage.RobotData>>(delegate(LogMessage<VisionOrCommand> log)
                {
                    LogMessage<VisionMessage.RobotData> rtn = new LogMessage<VisionMessage.RobotData>();
                    rtn.obj = log.obj.vision;
                    return rtn;
                });
            });
            commands = logs.ConvertAll<List<LogMessage<WheelSpeeds>>>(delegate(List<LogMessage<VisionOrCommand>> lst)
            {
                return lst.FindAll(delegate(LogMessage<VisionOrCommand> log)
                {
                    return log.obj.command != null;
                }).ConvertAll<LogMessage<WheelSpeeds>>(delegate(LogMessage<VisionOrCommand> log)
                {
                    LogMessage<WheelSpeeds> rtn = new LogMessage<WheelSpeeds>();
                    rtn.obj = log.obj.command;
                    return rtn;
                });
            });
            MessageBox.Show(logs.Count + " runs successfully loaded");

            gz.Close();
            s.Close();
        }
    }
}