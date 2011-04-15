using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

using Robocup.Geometry;
using Robocup.Utilities;
using Robocup.Core;
using Robocup.CoreRobotics;

namespace Robocup.MotionControl
{
    public partial class ModelCalibrator : Form
    {
        public ModelCalibrator()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer, true);
            InitializeComponent();
            textBoxFile.Text = "../../resources/Control calibration data/basic.log.zip";
        }

        string fname = ".";

        private void textBoxFile_TextChanged(object sender, EventArgs e)
        {
            bool exists = File.Exists(textBoxFile.Text);
            textBoxFile.ForeColor = exists ? Color.Black : Color.Red;
            buttonLoad.Enabled = exists;
            if (exists)
                fname = textBoxFile.Text;
        }

        private void buttonPickFile_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = new FileInfo(fname).Directory.FullName;
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                fname = openFileDialog1.FileName;
                textBoxFile.Text = fname;
            }
        }

        List<List<LogMessage<VisionMessage.RobotData>>> vision = null;
        List<List<LogMessage<WheelSpeeds>>> commands = null;

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            Stream s = File.Open(fname, FileMode.Open);
            Stream gz = new System.IO.Compression.GZipStream(s, System.IO.Compression.CompressionMode.Decompress);

            //read the logs in from the file
            List<List<LogMessage<VisionOrCommand>>> logs = LogWriter<VisionOrCommand>.ReadAndBreakLog(gz, false);

            //it's logged as VisionOrCommand objects; we need to break those up into separate vision and command lists
            //this has three steps: for each data set, only take objects that are vision/command, and convert them to vision/command
            vision = logs.ConvertAll<List<LogMessage<VisionMessage.RobotData>>>(delegate(List<LogMessage<VisionOrCommand>> lst)
            {
                return lst.FindAll(delegate(LogMessage<VisionOrCommand> log)
                {
                    return log.obj.vision != null;
                }).ConvertAll<LogMessage<VisionMessage.RobotData>>(delegate(LogMessage<VisionOrCommand> log)
                {
                    LogMessage<VisionMessage.RobotData> rtn = new LogMessage<VisionMessage.RobotData>();
                    rtn.obj = log.obj.vision;
                    rtn.time = log.time;
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
                    rtn.time = log.time;
                    return rtn;
                });
            });
            MessageBox.Show(logs.Count + " runs successfully loaded");

            buttonStart.Enabled = true;

            gz.Close();
            s.Close();

            this.Invalidate();
        }

        //the thread that will run the optimizer
        System.Threading.Thread t;

        private void buttonStart_Click(object sender, EventArgs e)
        {
            RobotModelCalibrator calibrator = new RobotModelCalibrator();
            calibrator.PathScored += delegate(Pair<List<RobotModelCalibrator.SimulatedPath>, double> pair)
            {
                SetDraw(pair.First[0]);
            };
            t = new System.Threading.Thread(delegate(object o)
            {
                DateTime start = DateTime.Now;
                MovementModeler model = calibrator.CalibrateModel(vision, commands);
                MessageBox.Show(model.changeConstlb + " " + model.changeConstlf + " " + model.changeConstrb + " " + model.changeConstrf +"\n"+
                    DateTime.Now.Subtract(start));
            });
            t.Start();
        }
        ICoordinateConverter mainScreen = new BasicCoordinateConverter(500, 30, 75);
        RobotModelCalibrator.SimulatedPath lastpath = null;
        DateTime last_invalidate = DateTime.Now;
        void SetDraw(RobotModelCalibrator.SimulatedPath path)
        {
            this.lastpath = path;
            if (DateTime.Now.Subtract(last_invalidate).TotalMilliseconds > 50)
            {
                this.Invalidate();
                last_invalidate = DateTime.Now;
            }
        }

        private void DrawVisionPath(List<LogMessage<VisionMessage.RobotData>> vision, Graphics g, ICoordinateConverter c)
        {
            Brush b = new SolidBrush(Color.Black);
            foreach (LogMessage<VisionMessage.RobotData> message in vision)
            {
                Vector2 p = message.obj.Position;
                p = c.fieldtopixelPoint(p);
                g.FillRectangle(b, (float)(p.X - 1), (float)(p.Y - 1), 2, 2);
            }
        }
        private void DrawSimPath(RobotModelCalibrator.SimulatedPath path, Graphics g, ICoordinateConverter c)
        {
            Brush b = new SolidBrush(Color.Red);
            foreach (RobotModelCalibrator.PathNode node in path.route)
            {
                Vector2 p = node.position;
                p = c.fieldtopixelPoint(p);
                g.FillRectangle(b, (float)(p.X - 1), (float)(p.Y - 1), 2, 2);
            }
        }

        private void ModelCalibrator_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (vision != null && vision.Count > 0)
            {
                DrawVisionPath(vision[0], g, mainScreen);
            }
            if (lastpath != null)
            {
                DrawSimPath(lastpath, g, mainScreen);
            }
        }

        //make sure to kill the worker thread when the user closes the form
        private void ModelCalibrator_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (t != null)
                t.Abort();
        }
    }
}