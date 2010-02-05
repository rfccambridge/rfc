using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Robocup.Core;

namespace Robocup.Simulation
{
    public partial class SimulatorForm : Form
    {
        private bool _formClosing = false;
        private bool _simRunning = false;
        private PhysicsEngine _physicsEngine = new PhysicsEngine();

        public SimulatorForm()
        {
            InitializeComponent();

            // Otherwise focus goes to the console window
            this.BringToFront();
        }

        private bool parseHost(string host, out string hostname, out int port)
        {
            string[] tokens = host.Split(new char[] { ':' });
            if (tokens.Length != 2 || !int.TryParse(tokens[1], out port) ||
                tokens[0].Length == 0 || tokens[1].Length == 0)
            {
                MessageBox.Show("Invalid format of host ('" + host + "'). It must be \"hostname:port\"");
                hostname = null;
                port = 0;
                return false;
            }
            hostname = tokens[0];
            return true;
        }

        private void btnSimStartStop_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_simRunning)
                {

                    string visionIp;
                    int visionPort;
                    int cmdPort = int.Parse(txtSimCmdPort.Text);
                    if (!parseHost(txtSimVisionHost.Text, out visionIp, out visionPort)) return;

                    // For convenience reload constants on every restart
                    Constants.Load();
                    _physicsEngine.LoadConstants();

                    _physicsEngine.StartCommander(cmdPort);
                    _physicsEngine.StartVision(visionIp, visionPort);

                    _physicsEngine.Start();

                    _simRunning = true;
                    btnSimStartStop.Text = "Stop Sim";
                    lblSimListenStatus.BackColor = Color.Green;
                }
                else
                {
                    _physicsEngine.Stop();

                    _physicsEngine.StopVision();
                    _physicsEngine.StopCommander();

                    _simRunning = false;
                    btnSimStartStop.Text = "Start Sim";
                    lblSimListenStatus.BackColor = Color.Red;
                }
            }
            catch (ApplicationException except)
            {
                MessageBox.Show(except.Message + "\r\n" + except.StackTrace);
                return;
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            _physicsEngine.ResetPositions();
        }

        private void SimulatorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_formClosing) return;

            // Cleanup before exiting
            if (_simRunning)
                btnSimStartStop_Click(null, null);

            // Need to give some time for the threads that the above calls spawn to 
            // complete, *without stalling this thread*, hence this bizzare code here
            _formClosing = true;
            e.Cancel = true;

            Thread shutdownThread = new Thread(delegate(object state)
            {
                Thread.Sleep(300);
                Application.Exit();
            });
            shutdownThread.Start();
        }        
    }
}