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

namespace SimplePathFollower {
    public delegate void VoidDelegate();
    public delegate bool BoolDelegate();
    public delegate void ButtonStringDelegate(Button btn, string text);

    public partial class FollowerForm : Form {
		private bool verbose = false;
		private int MESSAGE_SENDER_PORT;

        private MessageReceiver<VisionMessage> _visionTop;
        private MessageReceiver<VisionMessage> _visionBottom;
        private bool _visionConnectedTop;
        private bool _visionConnectedBottom;
        private bool _controlConnected;

		bool sslVisionConnected = false;
		string sslVisionHostname;
		int sslVisionPort;

		private PathFollower _pathFollower;
        private PIDCalibrator _pidCalibrator;
        private IPredictor _predictor;
        private bool _running;
    	private bool logging;

        private FieldDrawer _fieldDrawer;
        private ICoordinateConverter _converter;
        
        private Stopwatch _stopwatch = new Stopwatch();

        private Object _drawingLock = new Object();
		private Object _predictorLock = new Object();
		
		Team OUR_TEAM = (Team)Enum.Parse(typeof(Team), Constants.get<string>("configuration", "OUR_TEAM"), true);

        bool lap = true;
        private int whichGoal = 0;
        private List<Vector2> goals;
        private void setGoal() {
            // based on current value of whichGoal, set the path to go
            // towards a particular point
            // if lapping is on, always sets to the list of goals rather than any
            // particular one

            if (!lap) {
                List<Vector2> wpList = new List<Vector2>();
                wpList.Add(goals[whichGoal]);

                _pathFollower.Waypoints = wpList;
            }
            else {
                _pathFollower.Waypoints = goals;
            }
        }

        // Logging
        LogReader _logReader;
        IPredictor _logPredictor;
        FieldDrawer _logFieldDrawer;
        List<Type> _logLineFormat;

        public FollowerForm() {
            InitializeComponent();

            // LIST OF POSSIBLE GOALS
            goals = new List<Vector2>();
            //goals.Add(new Vector2(-.5, -1.3));
            /*goals.Add(new Vector2(-2, 1.5));
            goals.Add(new Vector2(2, 1.5));
            goals.Add(new Vector2(-2, -1.5));
            goals.Add(new Vector2(2, -1.5)); */

            goals.Add(new Vector2(2, 1.5));
            goals.Add(new Vector2(0.5, 1.5));
            goals.Add(new Vector2(0.5, -1.5));
            goals.Add(new Vector2(2, -1.5));
            

            // TODO: FollowerForm is dead, so just commenting out for the sake of compilation
            // MESSAGE_SENDER_PORT = Constants.get<int>("ports", "VisionDataPort");
            MESSAGE_SENDER_PORT = 0;

			txtVisionHostTop.Text = Constants.get<string>("default", "DEFAULT_HOST_VISION_TOP");
			txtVisionHostBottom.Text = Constants.get<string>("default", "DEFAULT_HOST_VISION_BOTTOM");
			txtSSLVisionHost.Text = Constants.get<string>("default", "DEFAULT_HOST_SSL_VISION") + ":" +
				 Constants.get<int>("default", "DEFAULT_PORT_SSL_VISION");

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
            _predictor = _pathFollower.Predictor;

            _fieldDrawer = new FieldDrawer();                        
            _fieldDrawer.Show();
            _fieldDrawer.UpdateTeam(OUR_TEAM);

            _stopwatch.Start();

            // Logging
            _logReader = new LogReader();
            _logPredictor = new StaticPredictor();
            _logFieldDrawer = new FieldDrawer();

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

		String TOP_CAMERA = "top_cam";
		String BOTTOM_CAMERA = "bottom_cam";

		#region RFC Vision
		private void btnVisionTop_Click(object sender, EventArgs e) {
            try {
                if (!_visionConnectedTop) {
                    _visionTop = Robocup.MessageSystem.Messages.CreateClientReceiver<Robocup.Core.VisionMessage>(
                        txtVisionHostTop.Text, MESSAGE_SENDER_PORT);
                    _visionTop.MessageReceived +=
                        new Robocup.MessageSystem.ReceiveMessageDelegate<VisionMessage>(
                            handleVisionUpdateTop);

                    lblVisionStatusTop.BackColor = Color.Green;
                    btnConnectVisionTop.Text = "Disconnect";
                    _visionConnectedTop = true;
                }
                else {
                    _visionTop.Close();
                    lblVisionStatusTop.BackColor = Color.Red;
                    btnConnectVisionTop.Text = "Connect";
                    _visionConnectedTop = false;
                }
            }
            catch (Exception except) {
                Console.WriteLine("Problem connecting to vision: " + except.ToString());
                Console.WriteLine(except.StackTrace);
            }
        }
        private void btnVisionBottom_Click(object sender, EventArgs e) {
            try {
                if (!_visionConnectedBottom) {
                    _visionBottom = Robocup.MessageSystem.Messages.CreateClientReceiver<Robocup.Core.VisionMessage>(
                        txtVisionHostBottom.Text, MESSAGE_SENDER_PORT);
                    _visionBottom.MessageReceived +=
                        new Robocup.MessageSystem.ReceiveMessageDelegate<VisionMessage>(
                            handleVisionUpdateBottom);

                    lblVisionStatusBottom.BackColor = Color.Green;
                    btnConnectVisionBottom.Text = "Disconnect";
                    _visionConnectedBottom = true;
                }
                else {
                    _visionBottom.Close();
                    lblVisionStatusBottom.BackColor = Color.Red;
                    btnConnectVisionBottom.Text = "Connect";
                    _visionConnectedBottom = false;
                }
            }
            catch (Exception except) {
                Console.WriteLine("Problem connecting to vision: " + except.ToString());
                Console.WriteLine(except.StackTrace);
            }
        }

        private void handleVisionUpdateTop(VisionMessage msg) {
            handleRFCVisionUpdate(msg, TOP_CAMERA);
        }
        private void handleVisionUpdateBottom(VisionMessage msg) {
            // OMEGA is hard-coded
            handleRFCVisionUpdate(msg, BOTTOM_CAMERA);
        }

        private void handleRFCVisionUpdate(VisionMessage msg, string cameraName) {
            List<RobotInfo> ours = new List<RobotInfo>();
            List<RobotInfo> theirs = new List<RobotInfo>();


			foreach (VisionMessage.RobotData robot in msg.Robots)
			{
				RobotInfo robotInfo = new RobotInfo(robot.Position, robot.Orientation, robot.ID);
                robotInfo.Team = robot.Team;
				(robot.Team == OUR_TEAM ? ours : theirs).Add(robotInfo);
			}

			lock (_predictorLock)
			{
				ISplitInfoAcceptor predictor = _predictor as ISplitInfoAcceptor;
				predictor.updatePartOurRobotInfo(ours, cameraName);
				predictor.updatePartTheirRobotInfo(theirs, cameraName);
				if (msg.Ball != null)
				{
					//Vector2 ballposition = new Vector2(2 + 1.01 * (msg.BallPosition.X - 2), msg.BallPosition.Y);                    
					predictor.updateBallInfo(new BallInfo(msg.Ball.Position));
				}
				else
				{
					predictor.updateBallInfo(null);
				}
			}
			
            //if (_stopwatch.ElapsedMilliseconds > 200) {
            //    _fieldDrawerForm.Invalidate();
            //    _stopwatch.Start();
            //    _stopwatch.Reset();
            //    _stopwatch.Start();
            //}          
            // TODO: Bring the drawing back to Sim
            /*
            lock (_drawingLock) {
                if (_fieldDrawerForm != null) {
                    _fieldDrawerForm.drawer.UpdateString("PlayType", "Play type: " + PlayTypes.Halt.ToString());                        
                    _pathFollower.drawCurrent(_fieldDrawerForm.CreateGraphics(), _converter);
                }
                //_pathFollower.clearArrows();
            }
             */

		}
		#endregion


		#region SSL Vision
		private void handleSSLVisionUpdate(VisionMessage msg)
		{
			lock (_predictorLock)
			{
				((IVisionInfoAcceptor)_predictor).Update(msg);
			}
		}
		void printRobotInfo(SSLVision.SSL_DetectionRobotManaged robot)
		{
			if (verbose) Console.Write(String.Format("CONF={0,4:F2} ", robot.confidence()));
			if (robot.has_robot_id())
			{
				if (verbose) Console.Write(String.Format("ID={0,3:G} ", robot.robot_id()));
			}
			else
			{
				if (verbose) Console.Write(String.Format("ID=N/A "));
			}
			if (verbose) Console.Write(String.Format(" HEIGHT={0,6:F2} POS=<{1,9:F2},{2,9:F2}> ", robot.height(), robot.x(), robot.y()));
			if (robot.has_orientation())
			{
				if (verbose) Console.Write(String.Format("ANGLE={0,6:F3} ", robot.orientation()));
			}
			else
			{
				if (verbose) Console.Write(String.Format("ANGLE=N/A    "));
			}
			if (verbose) Console.Write(String.Format("RAW=<{0,8:F2},{1,8:F2}>\n", robot.pixel_x(), robot.pixel_y()));
		}

		private void btnConnectSSLVision_Click(object sender, EventArgs e)
		{

			if (!sslVisionConnected)
			{
				string[] tokens = txtSSLVisionHost.Text.Split(new char[] { ':' });
				if (tokens.Length != 2 || !int.TryParse(tokens[1], out sslVisionPort))
				{
					MessageBox.Show("Invalid format of SSL Vision host. It must be \"hostname:port\"");
					return;
				}
				sslVisionHostname = tokens[0];

				VoidDelegate sslVisionLoopDelegate = new VoidDelegate(SSLVisionLoop);
				AsyncCallback sslVisionErrorHandler = new AsyncCallback(SSLVisionLoopErrorHandler);

				sslVisionConnected = true;

				IAsyncResult sslVisionLoopHandle = sslVisionLoopDelegate.BeginInvoke(sslVisionErrorHandler, null);

				lblSSLVisionStatus.BackColor = Color.Green;
				btnConnectSSLVision.Text = "Disconnect";
			}
			else
			{
				sslVisionConnected = false;
				lblSSLVisionStatus.BackColor = Color.Red;
				btnConnectSSLVision.Text = "Connect";
			}
		}

		private Vector2 ConvertFromSSLVisionCoords(Vector2 v)
		{
			return new Vector2(v.X / 1000, v.Y / 1000);
		}

		private void SSLVisionLoop()
		{
			Console.WriteLine("Starting SSL Vision Loop..");

			SSLVision.SSL_WrapperPacketManaged packet = new SSLVision.SSL_WrapperPacketManaged();
			SSLVision.RoboCupSSLClientManaged client = new SSLVision.RoboCupSSLClientManaged(sslVisionPort, sslVisionHostname, "");
			client.open(true);

			while (sslVisionConnected)
			{
				if (!client.receive(packet))
					continue;

				if (verbose) Console.Write(String.Format("-----Received Wrapper Packet---------------------------------------------\n"));
				//see if the packet contains a robot detection frame:
				if (packet.has_detection())
				{
					SSLVision.SSL_DetectionFrameManaged detection = packet.detection();
					//Display the contents of the robot detection results:
					//double t_now = GetTimeSec();
					double t_now = 0;

					if (verbose) Console.Write(String.Format("-[Detection Data]-------\n"));
					//Frame info:
					if (verbose) Console.Write(String.Format("Camera ID={0:G} FRAME={1:G} T_CAPTURE={2:F4}\n", detection.camera_id(), detection.frame_number(), detection.t_capture()));

					if (verbose) Console.Write(String.Format("SSL-Vision Processing Latency                   {0,7:F3}ms\n", (detection.t_sent() - detection.t_capture()) * 1000.0));
					if (verbose) Console.Write(String.Format("Network Latency (assuming synched system clock) {0,7:F3}ms\n", (t_now - detection.t_sent()) * 1000.0));
					if (verbose) Console.Write(String.Format("Total Latency   (assuming synched system clock) {0,7:F3}ms\n", (t_now - detection.t_capture()) * 1000.0));
					int balls_n = detection.balls_size();
					int robots_blue_n = detection.robots_blue_size();
					int robots_yellow_n = detection.robots_yellow_size();

					VisionMessage msg = new VisionMessage((int)detection.camera_id());

					//Ball info:
					float maxBallConfidence = float.MinValue;
					for (int i = 0; i < balls_n; i++)
					{
						SSLVision.SSL_DetectionBallManaged ball = detection.balls(i);
						if (verbose) Console.Write(String.Format("-Ball ({0,2:G}/{1,2:G}): CONF={2,4:F2} POS=<{3,9:F2},{4,9:F2}> ", i + 1, balls_n, ball.confidence(), ball.x(), ball.y()));
						if (ball.has_z())
						{
							if (verbose) Console.Write(String.Format("Z={0,7:F2} ", ball.z()));
						}
						else
						{
							if (verbose) Console.Write(String.Format("Z=N/A   "));
						}
						if (verbose) Console.Write(String.Format("RAW=<{0,8:F2},{1,8:F2}>\n", ball.pixel_x(), ball.pixel_y()));

						if (ball.has_confidence() && ball.confidence() > maxBallConfidence)
						{
							msg.Ball = new BallInfo(ConvertFromSSLVisionCoords(new Vector2(ball.x(), ball.y())));
							maxBallConfidence = ball.confidence();
						}
					}

					//Blue robot info:
					for (int i = 0; i < robots_blue_n; i++)
					{
						SSLVision.SSL_DetectionRobotManaged robot = detection.robots_blue(i);
						if (verbose) Console.Write(String.Format("-Robot(B) ({0,2:G}/{1,2:G}): ", i + 1, robots_blue_n));
						printRobotInfo(robot);
						msg.Robots.Add(new VisionMessage.RobotData((int)robot.robot_id(), Team.Blue,
							ConvertFromSSLVisionCoords(new Vector2(robot.x(), robot.y())), robot.orientation()));

					}

					//Yellow robot info:
					for (int i = 0; i < robots_yellow_n; i++)
					{
						SSLVision.SSL_DetectionRobotManaged robot = detection.robots_yellow(i);
						if (verbose) Console.Write(String.Format("-Robot(Y) ({0,2:G}/{1,2:G}): ", i + 1, robots_yellow_n));
						printRobotInfo(robot);
						msg.Robots.Add(new VisionMessage.RobotData((int)robot.robot_id(), Team.Yellow,
						  ConvertFromSSLVisionCoords(new Vector2(robot.x(), robot.y())), robot.orientation()));
					}

					handleSSLVisionUpdate(msg);
				}

				//see if packet contains geometry data:
				if (packet.has_geometry())
				{
					SSLVision.SSL_GeometryDataManaged geom = packet.geometry();
					if (verbose) Console.Write(String.Format("-[Geometry Data]-------\n"));

					SSLVision.SSL_GeometryFieldSizeManaged field = geom.field();
					if (verbose) Console.Write(String.Format("Field Dimensions:\n"));
					if (verbose) Console.Write(String.Format("  -line_width={0:G} (mm)\n", field.line_width()));
					if (verbose) Console.Write(String.Format("  -field_length={0:G} (mm)\n", field.field_length()));
					if (verbose) Console.Write(String.Format("  -field_width={0:G} (mm)\n", field.field_width()));
					if (verbose) Console.Write(String.Format("  -boundary_width={0:G} (mm)\n", field.boundary_width()));
					if (verbose) Console.Write(String.Format("  -referee_width={0:G} (mm)\n", field.referee_width()));
					if (verbose) Console.Write(String.Format("  -goal_width={0:G} (mm)\n", field.goal_width()));
					if (verbose) Console.Write(String.Format("  -goal_depth={0:G} (mm)\n", field.goal_depth()));
					if (verbose) Console.Write(String.Format("  -goal_wall_width={0:G} (mm)\n", field.goal_wall_width()));
					if (verbose) Console.Write(String.Format("  -center_circle_radius={0:G} (mm)\n", field.center_circle_radius()));
					if (verbose) Console.Write(String.Format("  -defense_radius={0:G} (mm)\n", field.defense_radius()));
					if (verbose) Console.Write(String.Format("  -defense_stretch={0:G} (mm)\n", field.defense_stretch()));
					if (verbose) Console.Write(String.Format("  -free_kick_from_defense_dist={0:G} (mm)\n", field.free_kick_from_defense_dist()));
					if (verbose) Console.Write(String.Format("  -penalty_spot_from_field_line_dist={0:G} (mm)\n", field.penalty_spot_from_field_line_dist()));
					if (verbose) Console.Write(String.Format("  -penalty_line_from_spot_dist={0:G} (mm)\n", field.penalty_line_from_spot_dist()));

					int calib_n = geom.calib_size();
					for (int i = 0; i < calib_n; i++)
					{
						SSLVision.SSL_GeometryCameraCalibrationManaged calib = geom.calib(i);
						if (verbose) Console.Write(String.Format("Camera Geometry for Camera ID {0:G}:\n", calib.camera_id()));
						if (verbose) Console.Write(String.Format("  -focal_length={0:F2}\n", calib.focal_length()));
						if (verbose) Console.Write(String.Format("  -principal_point_x={0:F2}\n", calib.principal_point_x()));
						if (verbose) Console.Write(String.Format("  -principal_point_y={0:F2}\n", calib.principal_point_y()));
						if (verbose) Console.Write(String.Format("  -distortion={0:F2}\n", calib.distortion()));
						if (verbose) Console.Write(String.Format("  -q0={0:F2}\n", calib.q0()));
						if (verbose) Console.Write(String.Format("  -q1={0:F2}\n", calib.q1()));
						if (verbose) Console.Write(String.Format("  -q2={0:F2}\n", calib.q2()));
						if (verbose) Console.Write(String.Format("  -q3={0:F2}\n", calib.q3()));
						if (verbose) Console.Write(String.Format("  -tx={0:F2}\n", calib.tx()));
						if (verbose) Console.Write(String.Format("  -ty={0:F2}\n", calib.ty()));
						if (verbose) Console.Write(String.Format("  -tz={0:F2}\n", calib.tz()));

						if (calib.has_derived_camera_world_tx() && calib.has_derived_camera_world_ty() && calib.has_derived_camera_world_tz())
						{
							if (verbose) Console.Write(String.Format("  -derived_camera_world_tx={0:F}\n", calib.derived_camera_world_tx()));
							if (verbose) Console.Write(String.Format("  -derived_camera_world_ty={0:F}\n", calib.derived_camera_world_ty()));
							if (verbose) Console.Write(String.Format("  -derived_camera_world_tz={0:F}\n", calib.derived_camera_world_tz()));
						}
					}
				}
			}
		}

		private void SSLVisionLoopErrorHandler(IAsyncResult result)
		{
			sslVisionConnected = false;
			lblSSLVisionStatus.BackColor = Color.Red;
			btnConnectSSLVision.Text = "Connect";
		}
		#endregion

		Stopwatch invalidateStopwatch = new Stopwatch();



        private void BtnControl_Click(object sender, EventArgs e) {
            try {
                if (!_controlConnected) {
                    // TODO: Follower is dead, so just fixing for sake of compilation
                    if ((_pathFollower.Commander as RemoteRobots).start(ControlHost.Text, 50100))
                    {
                        ControlStatus.BackColor = Color.Green;
                        BtnControl.Text = "Disconnect";
                        _controlConnected = true;
                    }
                }
                else {
                    (_pathFollower.Commander as RemoteRobots).stop();
                    ControlStatus.BackColor = Color.Red;
                    BtnControl.Text = "Connect";
                    _controlConnected = false;
                }
            }
            catch (Exception except) {
                Console.WriteLine(except.StackTrace);
            }
        }

        private void BtnStartStop_Click(object sender, EventArgs e) {
            if (!_running) {

                _pathFollower.RobotID = int.Parse(txtRobotID.Text);
                setGoal();

                _running = true;
                btnStartStop.Text = "Stop";

                // start the path follower in a new thread                 
                BoolDelegate followLoopDelegate = new BoolDelegate(_pathFollower.Follow);
                AsyncCallback followErrorHandler = new AsyncCallback(ErrorHandler);
                IAsyncResult FollowLoopHandle = followLoopDelegate.BeginInvoke(followErrorHandler, null);

            }
            else {
                _pathFollower.Stop();
                _running = false;
                btnStartStop.Text = "Start";
            }
        }

        private void ErrorHandler(IAsyncResult res) {

            AsyncResult result = (AsyncResult)res; // access the implementation of the interface
            BoolDelegate callerDelegate = (BoolDelegate)result.AsyncDelegate;

            bool error = callerDelegate.EndInvoke(res);

            if (error) {
                MessageBox.Show("Error! Follow() method failed.");
            }


            _pathFollower.Stop();
            _running = false;

            // Cross thread operation:           
            SetButtonText(btnStartStop, "Start");

        }

        private void SetButtonText(Button btn, string text) {
            if (btn.InvokeRequired) {
                // This is a worker thread so delegate the task.
                btn.Invoke(new ButtonStringDelegate(SetButtonText), btn, text);
            }
            else {
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

            VoidDelegate kickLoopDelegate = new VoidDelegate(_pathFollower.BeamKick);
            IAsyncResult kickLoopHandle = kickLoopDelegate.BeginInvoke(null, null);

            _running = true;
        }

        enum MotionPlanners {
            DefaultMotionPlanner, TangentBugModelFeedbackMotionPlanner, TangentBugFeedbackMotionPlanner, 
			FeedbackVeerMotionPlanner, BugFeedbackVeerMotionPlanner,
            BugExtendMotionPlanner, PointChargeExtendMotionPlanner,
            PointChargeFeedbackVeerMotionPlanner, DumbTranslatePlanner,
            DumbPlanner, DumbTurnPlanner, CircleFeedbackMotionPlanner,
            BugFeedbackMotionPlanner, MixedBiRRTMotionPlanner, StickyRRTFeedbackMotionPlanner,
            StickyDumbMotionPlanner
        };
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
				//case MotionPlanners.MixedBiRRTMotionPlanner:
				//    planner = new MixedBiRRTMotionPlanner();
				//    if (_pathFollower.setPlanner(planner)) {
				//        _currentPlannerSelection = MotionPlanners.MixedBiRRTMotionPlanner;
				//    }
				//    break;
				//case MotionPlanners.StickyRRTFeedbackMotionPlanner:
				//    planner = new StickyRRTFeedbackMotionPlanner();
				//    if (_pathFollower.setPlanner(planner)) {
				//        _currentPlannerSelection = MotionPlanners.StickyRRTFeedbackMotionPlanner;
				//    }
				//    break;
                case MotionPlanners.DumbTurnPlanner:
                    planner = new DumbTurnPlanner();
                    if (_pathFollower.setPlanner(planner)) {
                        _currentPlannerSelection = MotionPlanners.DumbTurnPlanner;
                    }
                    break;
                case MotionPlanners.FeedbackVeerMotionPlanner:
                    planner = new FeedbackVeerMotionPlanner();
                    if (_pathFollower.setPlanner(planner)) {
                        _currentPlannerSelection = MotionPlanners.FeedbackVeerMotionPlanner;
                    }
                    break;
                case MotionPlanners.PointChargeExtendMotionPlanner:
                    planner = new PointChargeExtendMotionPlanner();
                    if (_pathFollower.setPlanner(planner)) {
                        _currentPlannerSelection = MotionPlanners.PointChargeExtendMotionPlanner;
                    }
                    break;
                case MotionPlanners.PointChargeFeedbackVeerMotionPlanner:
                    planner = new PointChargeFeedbackVeerMotionPlanner();
                    if (_pathFollower.setPlanner(planner)) {
                        _currentPlannerSelection = MotionPlanners.PointChargeFeedbackVeerMotionPlanner;
                    }
                    break;
                case MotionPlanners.BugExtendMotionPlanner:
                    planner = new BugExtendMotionPlanner();
                    if (_pathFollower.setPlanner(planner)) {
                        _currentPlannerSelection = MotionPlanners.BugExtendMotionPlanner;
                    }
                    break;
                case MotionPlanners.BugFeedbackVeerMotionPlanner:
                    planner = new TangentBugVeerMotionPlanner();
                    if (_pathFollower.setPlanner(planner)) {
                        _currentPlannerSelection = MotionPlanners.BugFeedbackVeerMotionPlanner;
                    }
                    break;
                case MotionPlanners.DumbTranslatePlanner:
                    planner = new DumbTranslatePlanner();
                    if (_pathFollower.setPlanner(planner)) {
                        _currentPlannerSelection = MotionPlanners.DumbTranslatePlanner;
                    }
                    break;
                case MotionPlanners.DefaultMotionPlanner:
                    planner = new DefaultMotionPlanner();
                    if (_pathFollower.setPlanner(planner)) {
                        _currentPlannerSelection = MotionPlanners.DefaultMotionPlanner;
                    }
                    break;
                case MotionPlanners.TangentBugFeedbackMotionPlanner:
                    planner = new TangentBugFeedbackMotionPlanner();
                    if (_pathFollower.setPlanner(planner)) {
                        _currentPlannerSelection = MotionPlanners.TangentBugFeedbackMotionPlanner;
                    }
                    break;
				case MotionPlanners.TangentBugModelFeedbackMotionPlanner:
					planner = new TangentBugModelFeedbackMotionPlanner();
					if (_pathFollower.setPlanner(planner))
					{
						_currentPlannerSelection = MotionPlanners.TangentBugModelFeedbackMotionPlanner;
					}
					break;

            }
            cmbMotionPlanner.SelectedItem = _currentPlannerSelection;
        }

        const string LOG_FILE = "testlog.txt";

        private void btnLogNext_Click(object sender, EventArgs e) {

            if (!_logReader.LogOpen) {
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

            ((IInfoAcceptor)_logPredictor).UpdateRobot(curState);

            // Draw the path, and the arrows            
           
            _logFieldDrawer.BeginCollectState();
            
            // TODO: Implement paths in FieldDrawer
            /*_logFieldDrawer.ClearPaths();
            _logFieldDrawer.AddPath(path);*/            

            _logFieldDrawer.UpdateRobotsAndBall(_logPredictor.GetRobots(), _logPredictor.GetBall());

            _logFieldDrawer.DrawArrow(curState.Team, curState.ID, ArrowType.Destination, destState.Position);
            _logFieldDrawer.DrawArrow(curState.Team, curState.ID, ArrowType.Waypoint, waypointInfo.Position);            

            _logFieldDrawer.EndCollectState();

        }

        private void btnStartStopLogging_Click(object sender, EventArgs e) {
            /*if (!(_pathFollower.Planner is ILogger))
            {
                MessageBox.Show("Selected MotionPlanner does not implement ILogger interface.");
                return;
            }*/

            if (logging) {
                logging = false;
                btnStartStopLogging.Text = "Start log";
            }
            else {
                logging = true;
                btnStartStopLogging.Text = "Stop log";
            }

            // if enabled, start motion planner's logging
            if (_pathFollower.Planner is ILogger) {
                ILogger logger = _pathFollower.Planner as ILogger;

                if (!logger.Logging) {
                    logger.LogFile = LOG_FILE;
                    logger.StartLogging(int.Parse(txtRobotID.Text));

                }
                else {
                    logger.StopLogging();
                }
            }
        }

        private void btnLogOpenClose_Click(object sender, EventArgs e) {
            if (_logReader.LogOpen) {
                _logReader.CloseLogFile();
                btnLogOpenClose.Text = "Open log";
            }
            else {
                _logReader.OpenLogFile(LOG_FILE, _logLineFormat);
                btnLogOpenClose.Text = "Close log";
            }

        }

        private void btnSwitchGoal_Click(object sender, EventArgs e) {
            // SWITCH GOAL
            whichGoal = (whichGoal + 1) % goals.Count;
            setGoal();
        }

        private void btnLap_Click(object sender, EventArgs e) {
            if (lap) {
                btnLap.Text = "Lap";
                lap = false;
            }
            else {
                btnLap.Text = "Stop lapping";
                lap = true;
            }
        }

        private void btnCalibratePID_Click(object sender, EventArgs e) {
            if (_running)
                return;

            _pidCalibrator = new PIDCalibrator(_pathFollower);
            _pathFollower.OnEndLap = _pidCalibrator.EndLap;
            _pathFollower.OnStartLap = _pidCalibrator.StartLap;
            _running = true;


            _pathFollower.RobotID = int.Parse(txtRobotID.Text);
            setGoal();

            btnStartStop.Text = "Stop";

            // start the PID Calibrator in a new thread                 
            VoidDelegate calibratorLoopDelegate = new VoidDelegate(_pidCalibrator.ExploreAround);
            AsyncCallback calibratorErrorHandler = new AsyncCallback(CalibratorError);
            IAsyncResult FollowLoopHandle = calibratorLoopDelegate.BeginInvoke(calibratorErrorHandler, null);
        }

        private void CalibratorError(IAsyncResult res) {
            _pidCalibrator.EndLap(false, true);
            _running = false;

            // Cross thread operation:           
            SetButtonText(btnStartStop, "Start");

        }

    }

}
