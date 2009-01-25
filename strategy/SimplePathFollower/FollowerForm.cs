using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Robocup.MessageSystem;
using Robocup.Core;
using Robocup.CoreRobotics;
using Robocup.ControlForm;
using Robocup.Utilities;
using System.Diagnostics;

namespace SimplePathFollower
{
    public delegate void VoidDelegate();

	public partial class FollowerForm : Form
	{
		private int MESSAGE_SENDER_PORT;

		private MessageReceiver<VisionMessage> _vision;
		private bool _visionConnected;
		private bool _controlConnected;
		private PathFollower _pathFollower;
		private BasicPredictor _predictor;
        private Object _predictorLock = new Object();
        private bool _running;

        private FieldDrawerForm _fieldDrawerForm;
        private ICoordinateConverter _converter;
        private Object _drawingLock = new Object();
        private Stopwatch _stopwatch = new Stopwatch();
		
		public FollowerForm()
		{
			InitializeComponent();

             MESSAGE_SENDER_PORT = Constants.get<int>("ports", "VisionDataPort");

			_visionConnected = false;
			_controlConnected = false;
            _running = false;

			_pathFollower = new PathFollower();
			_pathFollower.Init();
			_predictor = (BasicPredictor)_pathFollower.Predictor;

            if (_fieldDrawerForm != null)
                _fieldDrawerForm.Close();

            _fieldDrawerForm = new FieldDrawerForm(_predictor);
            _converter = _fieldDrawerForm.Converter;
            _fieldDrawerForm.Show();

            _stopwatch.Start();

		}

		private void BtnVision_Click(object sender, EventArgs e)
		{
			try
			{
				if (!_visionConnected)
				{
					_vision = Robocup.MessageSystem.Messages.CreateClientReceiver<Robocup.Core.VisionMessage>(
						VisionHost.Text, MESSAGE_SENDER_PORT);
					_vision.MessageReceived += 
						new Robocup.MessageSystem.ReceiveMessageDelegate<VisionMessage>(
							handleVisionUpdate);

					VisionStatus.BackColor = Color.Green;
					BtnVision.Text = "Disconnect";
					_visionConnected = true;
				}
				else
				{
					_vision.Close();
					VisionStatus.BackColor = Color.Red;
					BtnVision.Text = "Connect";
					_visionConnected = false;
				}
			}
			catch (Exception except)
			{
				Console.WriteLine("Problem connecting to vision: " + except.ToString());
				Console.WriteLine(except.StackTrace);
			}
		}

		
		private void handleVisionUpdate(VisionMessage msg)
		{
			String cameraName = "top_cam";

			List<RobotInfo> ours = new List<RobotInfo>();

			foreach (VisionMessage.RobotData robot in msg.OurRobots)
			{
				ours.Add(new RobotInfo(robot.Position, robot.Orientation, robot.ID));
			}

			List<RobotInfo> theirs = new List<RobotInfo>();
			foreach (VisionMessage.RobotData robot in msg.TheirRobots)
			{
				theirs.Add(new RobotInfo(robot.Position, robot.Orientation, robot.ID));
			}

			lock (_predictorLock)
			{
				_predictor.updatePartOurRobotInfo(ours, cameraName);
				_predictor.updatePartTheirRobotInfo(theirs, cameraName);
				if (msg.BallPosition != null)
				{
					Vector2 ballposition = new Vector2(2 + 1.01 * (msg.BallPosition.X - 2), msg.BallPosition.Y);
					_predictor.updateBallInfo(new BallInfo(ballposition));
				}
			}

            if (_stopwatch.ElapsedMilliseconds > 200) {
                _fieldDrawerForm.Invalidate();
                _stopwatch.Start();
                _stopwatch.Reset();
                _stopwatch.Start();
            }
                lock (_drawingLock) {
                    _pathFollower.drawCurrent(_fieldDrawerForm.CreateGraphics(), _converter);
                    //_pathFollower.clearArrows();
                }                
           
		}



		private void BtnControl_Click(object sender, EventArgs e)
		{
			try
			{
				if (!_controlConnected)
				{
					if ((_pathFollower.Commander as RemoteRobots).start(ControlHost.Text))
					{
						ControlStatus.BackColor = Color.Green;
						BtnControl.Text = "Disconnect";
						_controlConnected = true;
					}
				}
				else
				{
					(_pathFollower.Commander as RemoteRobots).stop();
					ControlStatus.BackColor = Color.Red;
					BtnControl.Text = "Connect";
					_controlConnected = false;
				}
			}
			catch (Exception except)
			{
				Console.WriteLine(except.StackTrace);
			}
		}

		private void BtnStart_Click(object sender, EventArgs e)
		{
            if (_running) {
                MessageBox.Show("Already running. Press stop first.");
                return;
            }
            
            List<Vector2> wpList = new List<Vector2>();
            wpList.Add(new Vector2(1.2, -0.5));
            //wpList.Add(new Vector2(-0.5, -0.5));
			//wpList.Add(new Vector2(0.5, -0.5));
			//wpList.Add(new Vector2(0.5, 0.5));
			//wpList.Add(new Vector2(-0.5, 0.5));

			_pathFollower.RobotID = 0;
			_pathFollower.Waypoints = wpList;

            // start the path follower in a new thread 
            _running = true;
            VoidDelegate followLoopDelegate = new VoidDelegate(_pathFollower.Follow);
            AsyncCallback followErrorHandler = new AsyncCallback(FollowErrorHandler);
            IAsyncResult FollowLoopHandle = followLoopDelegate.BeginInvoke(followErrorHandler, null);			
		}
		
		private void BtnStop_Click(object sender, EventArgs e)
		{
            if (!_running) {
                MessageBox.Show("Path Follower not running. Press start first.");
                return;
            }
			_pathFollower.Stop();
            _running = false;
		}

        private void FollowErrorHandler(IAsyncResult res) {            
            _pathFollower.Stop();
            _running = false;
        }

        private void btnReloadPIDConstants_Click(object sender, EventArgs e) {
            //implement chain of methods to allow _pathFollower.reloadConstants();
            _pathFollower.reloadConstants();
        }

	}
}
