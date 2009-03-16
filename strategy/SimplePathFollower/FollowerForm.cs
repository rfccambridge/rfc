using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Robocup.MessageSystem;
using Robocup.Core;
using Robocup.CoreRobotics;
using Robocup.ControlForm;
using Robocup.Utilities;
using System.Diagnostics;
using Robocup.MotionControl;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace SimplePathFollower
{
    public delegate void VoidDelegate();
    public delegate bool BoolDelegate();    
    public delegate void ButtonStringDelegate(Button btn, string text);

	public partial class FollowerForm : Form
	{
		private int MESSAGE_SENDER_PORT;

        private int whichGoal = 0;
        private List<Vector2> goals;

		private MessageReceiver<VisionMessage> _visionTop;
        private MessageReceiver<VisionMessage> _visionBottom;
		private bool _visionConnectedTop;
        private bool _visionConnectedBottom;
		private bool _controlConnected;
		private PathFollower _pathFollower;
		private BasicPredictor _predictor;
        private Object _predictorLock = new Object();
        private bool _running;

        private FieldDrawerForm _fieldDrawerForm;
        private ICoordinateConverter _converter;
        private Object _drawingLock = new Object();
        private Stopwatch _stopwatch = new Stopwatch();

        // Logging
        LogReader _logReader;
        IPredictor _logPredictor;
        FieldDrawerForm _logFieldDrawer;
        List<Type> _logLineFormat;

		public FollowerForm()
		{
			InitializeComponent();

            // LIST OF POSSIBLE GOALS
            goals = new List<Vector2>();
            goals.Add(new Vector2(-.5, -1.3));
            goals.Add(new Vector2(-2.2, -1.5));
            goals.Add(new Vector2(-2.0, 1.2));
                       
            MESSAGE_SENDER_PORT = Constants.get<int>("ports", "VisionDataPort");

            _pathFollower = new PathFollower();

			_visionConnectedTop = false;
            _visionConnectedBottom = false;
			_controlConnected = false;
            _running = false;

            // Populate the MotionPlanner combobox
            Array motionPlannersArr = Enum.GetValues(typeof(MotionPlanners));
            object[] motionPlanners = new object[motionPlannersArr.Length];
            motionPlannersArr.CopyTo(motionPlanners, 0);

            cmbMotionPlanner.Items.AddRange(motionPlanners);

            btnStartStop.Text = "Start";
            

            // Default MotionPlanner selection
            _currentPlannerSelection = MotionPlanners.DefaultMotionPlanner;
            cmbMotionPlanner.SelectedItem = _currentPlannerSelection;
            IMotionPlanner planner = new DefaultMotionPlanner();
			
			_pathFollower.Init(planner);
			_predictor = (BasicPredictor)_pathFollower.Predictor;

            if (_fieldDrawerForm != null)
                _fieldDrawerForm.Close();

            _fieldDrawerForm = new FieldDrawerForm(_predictor);
            _converter = _fieldDrawerForm.Converter;
            _fieldDrawerForm.Show();

            _stopwatch.Start();


            // Logging
            _logReader = new LogReader();
            _logPredictor = new StaticPredictor();
            _logFieldDrawer = new FieldDrawerForm(_logPredictor);

            // Log Line format
            // timestamp current_state desired_state next_waypoint wheel_speeds path
            _logLineFormat = new List<Type>();
            _logLineFormat.Add(typeof(DateTime));
            _logLineFormat.Add(typeof(RobotInfo));
            _logLineFormat.Add(typeof(RobotInfo));
            _logLineFormat.Add(typeof(RobotInfo));
            _logLineFormat.Add(typeof(WheelSpeeds));
            _logLineFormat.Add(typeof(RobotPath));


            timer = new Timer();
            timer.Interval = 500;
            //timer.Tick += new EventHandler(InvalidateTimerHanlder);
            timer.Start();
		}

        Timer timer;

        private void InvalidateTimerHanlder(object obj, EventArgs e) {            
            _logFieldDrawer.Invalidate();
        }

		private void btnVisionTop_Click(object sender, EventArgs e)
		{
			try
			{
				if (!_visionConnectedTop)
				{
					_visionTop = Robocup.MessageSystem.Messages.CreateClientReceiver<Robocup.Core.VisionMessage>(
						txtVisionHostTop.Text, MESSAGE_SENDER_PORT);
					_visionTop.MessageReceived += 
						new Robocup.MessageSystem.ReceiveMessageDelegate<VisionMessage>(
							handleVisionUpdateTop);

					lblVisionStatusTop.BackColor = Color.Green;
					btnConnectVisionTop.Text = "Disconnect";
					_visionConnectedTop = true;
				}
				else
				{
					_visionTop.Close();
					lblVisionStatusTop.BackColor = Color.Red;
					btnConnectVisionTop.Text = "Connect";
					_visionConnectedTop = false;
				}
			}
			catch (Exception except)
			{
				Console.WriteLine("Problem connecting to vision: " + except.ToString());
				Console.WriteLine(except.StackTrace);
			}
		}

        private void btnVisionBottom_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_visionConnectedBottom)
                {
                    _visionBottom = Robocup.MessageSystem.Messages.CreateClientReceiver<Robocup.Core.VisionMessage>(
                        txtVisionHostBottom.Text, MESSAGE_SENDER_PORT);
                    _visionBottom.MessageReceived +=
                        new Robocup.MessageSystem.ReceiveMessageDelegate<VisionMessage>(
                            handleVisionUpdateBottom);

                    lblVisionStatusBottom.BackColor = Color.Green;
                    btnConnectVisionBottom.Text = "Disconnect";
                    _visionConnectedBottom = true;
                }
                else
                {
                    _visionBottom.Close();
                    lblVisionStatusBottom.BackColor = Color.Red;
                    btnConnectVisionBottom.Text = "Connect";
                    _visionConnectedBottom = false;
                }
            }
            catch (Exception except)
            {
                Console.WriteLine("Problem connecting to vision: " + except.ToString());
                Console.WriteLine(except.StackTrace);
            }
        }

        private void handleVisionUpdateTop(VisionMessage msg)
        {
            // OMEGA is hard-coded
            handleVisionUpdate(msg, "OMEGA");
        }

        private void handleVisionUpdateBottom(VisionMessage msg)
        {
            // OMEGA is hard-coded
            handleVisionUpdate(msg, "NOT_OMEGA");
        }

		private void handleVisionUpdate(VisionMessage msg, string computerName)
		{			
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
				_predictor.updatePartOurRobotInfo(ours, computerName);
				_predictor.updatePartTheirRobotInfo(theirs, computerName);
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

        Stopwatch invalidateStopwatch = new Stopwatch();



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

        private void setGoal()
        {
            // based on current value of whichGoal, set the path to go
            // towards a particular point
            List<Vector2> wpList = new List<Vector2>();
            wpList.Add(goals[whichGoal]);

            _pathFollower.Waypoints = wpList;
        }

		private void BtnStartStop_Click(object sender, EventArgs e)
		{
            if (!_running) {

                //Waypoints are now set in constructor, and goal 
                //is set in setGoal()
                //List<Vector2> wpList = new List<Vector2>();
                //wpList.Add(goals[whichGoal]);
                //wpList.Add(new Vector2(-1.2, -0.5));
                //wpList.Add(new Vector2(-0.5, -0.5));
                //wpList.Add(new Vector2(0.5, -0.5));
                //wpList.Add(new Vector2(0.5, 0.5));
                //wpList.Add(new Vector2(-0.5, 0.5));

                _pathFollower.RobotID = int.Parse(txtRobotID.Text);
                setGoal();

                //_pathFollower.Waypoints = wpList;

                _running = true;
                btnStartStop.Text = "Stop";

                // start the path follower in a new thread                 
                BoolDelegate followLoopDelegate = new BoolDelegate(_pathFollower.Follow);
                AsyncCallback followErrorHandler = new AsyncCallback(ErrorHandler);
                IAsyncResult FollowLoopHandle = followLoopDelegate.BeginInvoke(followErrorHandler, null);
               
            } else {
                _pathFollower.Stop();
                _running = false;
                btnStartStop.Text = "Start";
            }
		}

        private void ErrorHandler(IAsyncResult res) {

            AsyncResult result = (AsyncResult)res; // access the implementation of the interface
            BoolDelegate callerDelegate = (BoolDelegate)result.AsyncDelegate;

            bool error = callerDelegate.EndInvoke(res);

            if (error)
            {
                MessageBox.Show("Error! Follow() method failed.");
            }


            _pathFollower.Stop();
            _running = false;

            // Cross thread operation:           
            SetButtonText(btnStartStop, "Start");
          
        }

        delegate void StringDelegate(string str);
        private void SetButtonText(Button btn, string text)
        {                        
            if (btn.InvokeRequired)
            {
                // This is a worker thread so delegate the task.
                btn.Invoke(new ButtonStringDelegate(SetButtonText), btn, text);
            }
            else
            {
                // This is the UI thread so perform the task.
                btn.Text = text;
            }            
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

            btnStartStop.Text = "Stop";

            VoidDelegate kickLoopDelegate = new VoidDelegate(_pathFollower.Kick);            
            IAsyncResult kickLoopHandle = kickLoopDelegate.BeginInvoke(null, null);	

            _running = true;
        }

        enum MotionPlanners
        {
            DefaultMotionPlanner, FeedbackVeerMotionPlanner, BugFeedbackVeerMotionPlanner, 
            BugExtendMotionPlanner, PointChargeExtendMotionPlanner,
            PointChargeFeedbackVeerMotionPlanner, DumbTranslatePlanner,
            DumbPlanner, DumbTurnPlanner, CircleFeedbackMotionPlanner, 
            BugFeedbackMotionPlanner, MixedBiRRTMotionPlanner, StickyRRTFeedbackMotionPlanner,
            StickyDumbMotionPlanner};
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
                case MotionPlanners.StickyRRTFeedbackMotionPlanner:
                    planner = new StickyRRTFeedbackMotionPlanner();
                    if (_pathFollower.setPlanner(planner)) {
                        _currentPlannerSelection = MotionPlanners.StickyRRTFeedbackMotionPlanner;
                    }
                    break;
				case MotionPlanners.StickyDumbMotionPlanner:
					planner = new StickyDumbMotionPlanner();
					if (_pathFollower.setPlanner(planner))
					{
						_currentPlannerSelection = MotionPlanners.StickyDumbMotionPlanner;
					}
					break;
                case MotionPlanners.DumbTurnPlanner:
                    planner = new DumbTurnPlanner();
                    if (_pathFollower.setPlanner(planner))
                    {
                        _currentPlannerSelection = MotionPlanners.DumbTurnPlanner;
                    }
                    break;
                case MotionPlanners.FeedbackVeerMotionPlanner:
                    planner = new FeedbackVeerMotionPlanner();
                    if (_pathFollower.setPlanner(planner))
                    {
                        _currentPlannerSelection = MotionPlanners.FeedbackVeerMotionPlanner;
                    }
                    break;
                case MotionPlanners.PointChargeExtendMotionPlanner:
                    planner = new PointChargeExtendMotionPlanner();
                    if (_pathFollower.setPlanner(planner))
                    {
                        _currentPlannerSelection = MotionPlanners.PointChargeExtendMotionPlanner;
                    }
                    break;
                case MotionPlanners.PointChargeFeedbackVeerMotionPlanner:
                    planner = new PointChargeFeedbackVeerMotionPlanner();
                    if (_pathFollower.setPlanner(planner))
                    {
                        _currentPlannerSelection = MotionPlanners.PointChargeFeedbackVeerMotionPlanner;
                    }
                    break;
                case MotionPlanners.BugExtendMotionPlanner:
                    planner = new BugExtendMotionPlanner();
                    if (_pathFollower.setPlanner(planner))
                    {
                        _currentPlannerSelection = MotionPlanners.BugExtendMotionPlanner;
                    }
                    break;
                case MotionPlanners.BugFeedbackVeerMotionPlanner:
                    planner = new BugFeedbackVeerMotionPlanner();
                    if (_pathFollower.setPlanner(planner))
                    {
                        _currentPlannerSelection = MotionPlanners.BugFeedbackVeerMotionPlanner;
                    }
                    break;
                case MotionPlanners.DumbTranslatePlanner:
                    planner = new DumbTranslatePlanner();
                    if (_pathFollower.setPlanner(planner))
                    {
                        _currentPlannerSelection = MotionPlanners.DumbTranslatePlanner;
                    }
                    break;
                case MotionPlanners.DefaultMotionPlanner:
                    planner = new DefaultMotionPlanner();
                    if (_pathFollower.setPlanner(planner))
                    {
                        _currentPlannerSelection = MotionPlanners.DefaultMotionPlanner;
                    }
                    break;

            }
            cmbMotionPlanner.SelectedItem = _currentPlannerSelection;
        }

        const string LOG_FILE = "testlog.txt";
        
        private void btnLogNext_Click(object sender, EventArgs e) {

            if (!_logReader.LogOpen)
            {
                MessageBox.Show("Log file not open.");
                return;
            }

            if (!_logFieldDrawer.Visible)
                _logFieldDrawer.Show();

            // Get logged info

            _logReader.Next();
            List<Object> loggedItems = _logReader.GetLoggedItems();


            // Log Line format
            // timestamp current_state desired_state next_waypoint wheel_speeds path
            RobotInfo curState = (RobotInfo)loggedItems[1];
            RobotInfo waypointInfo = (RobotInfo)loggedItems[3];
            RobotInfo destState = (RobotInfo)loggedItems[2];
            RobotPath path = (RobotPath)loggedItems[5];                                   

            // Update predictor (this will also draw the robot positions)

            ((IInfoAcceptor)_logPredictor).updateRobot(curState.ID, curState);
            
            // Draw the path, and the arrows            
            
            // Since we are the only ones who are using this FieldDrawer, we can monopolize 
            // its arrows and paths holder
            _logFieldDrawer.ClearPaths();
            _logFieldDrawer.AddPath(path);
            
            _logFieldDrawer.ClearArrows();
            _logFieldDrawer.AddArrow(new Arrow(curState.Position, destState.Position, Color.Red, 0.04));
            _logFieldDrawer.AddArrow(new Arrow(curState.Position, waypointInfo.Position, Color.Green, 0.04));

            _logFieldDrawer.Invalidate();
           
        }       

        private void btnStartStopLogging_Click(object sender, EventArgs e) {
            if (!(_pathFollower.Planner is ILogger))
            {
                MessageBox.Show("Selected MotionPlanner does not implement ILogger interface.");
                return;
            }

            ILogger logger = _pathFollower.Planner as ILogger;

            if (!logger.Logging) {
                logger.LogFile = LOG_FILE;
                logger.StartLogging();
                btnStartStopLogging.Text = "Stop log";
            }
            else {
                logger.StopLogging();
                btnStartStopLogging.Text = "Start log";
            }
        }
        
        private void btnLogOpenClose_Click(object sender, EventArgs e)
        {
            if (_logReader.LogOpen)
            {
                _logReader.CloseLogFile();
                btnLogOpenClose.Text = "Open log";
            }
            else
            {
                _logReader.OpenLogFile(LOG_FILE, _logLineFormat);
                btnLogOpenClose.Text = "Close log";
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // SWITCH GOAL
            whichGoal = (whichGoal + 1) % goals.Count;
            setGoal();
        }
	}

}
