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
using Robocup.MotionControl;

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

            _pathFollower = new PathFollower();

			_visionConnected = false;
			_controlConnected = false;
            _running = false;

            // Populate the MotionPlanner combobox
            Array motionPlannersArr = Enum.GetValues(typeof(MotionPlanners));
            object[] motionPlanners = new object[motionPlannersArr.Length];
            motionPlannersArr.CopyTo(motionPlanners, 0);

            cmbMotionPlanner.Items.AddRange(motionPlanners);

            BtnStartStop.Text = "Start";
            

            // Default MotionPlanner selection
            _currentPlannerSelection = MotionPlanners.CircleFeedbackMotionPlanner;
            cmbMotionPlanner.SelectedItem = _currentPlannerSelection;
            IMotionPlanner planner = new CircleFeedbackMotionPlanner();
			
			_pathFollower.Init(planner);
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

		private void BtnStartStop_Click(object sender, EventArgs e)
		{
            if (!_running) {
              
                List<Vector2> wpList = new List<Vector2>();
                wpList.Add(new Vector2(1.2, -0.5));
                //wpList.Add(new Vector2(-0.5, -0.5));
                //wpList.Add(new Vector2(0.5, -0.5));
                //wpList.Add(new Vector2(0.5, 0.5));
                //wpList.Add(new Vector2(-0.5, 0.5));

                _pathFollower.RobotID = 0;
                _pathFollower.Waypoints = wpList;

                _running = true;
                // start the path follower in a new thread                 
                VoidDelegate followLoopDelegate = new VoidDelegate(_pathFollower.Follow);
                AsyncCallback followErrorHandler = new AsyncCallback(ErrorHandler);
                IAsyncResult FollowLoopHandle = followLoopDelegate.BeginInvoke(followErrorHandler, null);

                BtnStartStop.Text = "Stop";
                
            } else {
                _pathFollower.Stop();
                _running = false;
                BtnStartStop.Text = "Start";
            }
		}

        private void ErrorHandler(IAsyncResult res) {            
            _pathFollower.Stop();
            _running = false;
        }

        private void btnReloadPIDConstants_Click(object sender, EventArgs e) {
            //implement chain of methods to allow _pathFollower.reloadConstants();
            _pathFollower.reloadConstants();
        }

        private void BtnKick_Click(object sender, EventArgs e) {
            if (_running) {
                MessageBox.Show("Already running. Press stop first.");
                return;
            }

            VoidDelegate kickLoopDelegate = new VoidDelegate(_pathFollower.Kick);
            AsyncCallback kickErrorHandler = new AsyncCallback(ErrorHandler);
            IAsyncResult kickLoopHandle = kickLoopDelegate.BeginInvoke(ErrorHandler, null);	

            _running = true;
        }

        enum MotionPlanners { CircleFeedbackMotionPlanner, BugFeedbackMotionPlanner, MixedBiRRTMotionPlanner };
        private MotionPlanners _currentPlannerSelection;

        private void cmbMotionPlanner_SelectedIndexChanged(object sender, EventArgs e) {

            // Don't do anything if pathFollower is not initialized
            // if (!_pathFollower.Initialized) { ... }
            if (_pathFollower.Predictor == null) {
                return;
            }

            IMotionPlanner planner;
            switch ((MotionPlanners)cmbMotionPlanner.SelectedItem) {
                case MotionPlanners.BugFeedbackMotionPlanner:
                    planner = new BugFeedbackMotionPlanner();
                    if (_pathFollower.setPlanner(planner)) {
                        _currentPlannerSelection = MotionPlanners.BugFeedbackMotionPlanner;                     
                    }
                    break;
                case MotionPlanners.CircleFeedbackMotionPlanner:
                    planner = new CircleFeedbackMotionPlanner();
                    if (_pathFollower.setPlanner(planner)) {
                        _currentPlannerSelection = MotionPlanners.CircleFeedbackMotionPlanner;
                    }                    
                    break;
                case MotionPlanners.MixedBiRRTMotionPlanner:
                    planner = new MixedBiRRTMotionPlanner();
                    if (_pathFollower.setPlanner(planner)) {
                        _currentPlannerSelection = MotionPlanners.MixedBiRRTMotionPlanner;
                    }
                    break;
            }
            cmbMotionPlanner.SelectedItem = _currentPlannerSelection;
        }

	}
}
