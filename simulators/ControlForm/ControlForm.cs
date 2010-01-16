using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Robocup.Utilities;
using Robocup.Core;
using Robocup.CoreRobotics;
using Robocup.Plays;

using System.Runtime.InteropServices;
using Robocup.Simulation;

namespace Robocup.ControlForm {
    
    public partial class ControlForm : Form {
        Team OUR_TEAM;
        FieldHalf FIELD_HALF;
        string PLAY_DIR;

        Vision _vision;
        IPredictor _predictor;
        FieldDrawer _fieldDrawer;
        RemoteRobots _serial;
        IRefBoxListener _refboxListener;
        VirtualRef _virtualReferee;
        PhysicsEngine _physicsEngine;

        bool _serialConnected = false;
        bool _refboxConnected = false;
        bool _visionConnected = false;
        
        List<InterpreterPlay> _plays;        
        Player _player;

        List<InterpreterPlay> _plays1, _plays2; // Must have separate play objects!
        Player _player1, _player2;
        FieldDrawer _fieldDrawer1, _fieldDrawer2;
        SimEngine _engine;

        Player _simplePlayer;
        List<RobotInfo> _waypoints = new List<RobotInfo>();
        Dictionary<RobotInfo, int> _waypointMarkers = new Dictionary<RobotInfo, int>();

        System.Timers.Timer _refboxCommandDisplayTimer;

        PIDForm _pidForm;
        DebugForm _debugForm;                
        
        // Logging
        LogReader _logReader;
        IPredictor _logPredictor;
        FieldDrawer _logFieldDrawer;
        List<Type> _logLineFormat;
        Timer timer;
        bool logging;
        string LOG_FILE;

        public ControlForm() {
            InitializeComponent();

            LoadConstants();

            // Defaults hosts for the GUI, for convenience only            
            sslVisionHost.Text = Constants.get<string>("default", "DEFAULT_HOST_SSL_VISION") + ":" +
                 Constants.get<int>("default", "DEFAULT_PORT_SSL_VISION").ToString();
            serialHost.Text = Constants.get<string>("default", "DEFAULT_HOST_SERIAL");
            txtRefbox.Items.Add(Constants.get<string>("default", "REFBOX_ADDR") + ":" +
                Constants.get<int>("default", "REFBOX_PORT"));
            txtRefbox.Items.Add(Constants.get<string>("default", "LOCAL_REFBOX_ADDR") + ":" +
                Constants.get<int>("default", "LOCAL_REFBOX_PORT"));
            txtRefbox.SelectedIndex = 0;

            _pidForm = new PIDForm();
            _pidForm.Show();

            _debugForm = DebugConsole.getForm();
            _debugForm.Show();
            
            _fieldDrawer = new FieldDrawer();
            _fieldDrawer.Show();

            _virtualReferee = new SimpleReferee();
            _physicsEngine = new Robocup.Simulation.PhysicsEngine(_virtualReferee);
            _engine = new SimEngine(_physicsEngine);

            loggingInit();

            reloadPlays();
            connectRefbox();

            createSystem();
            createSim();
            createSimple();
            
            createRefboxCommandDisplayTimer();

            btnLogNext.Enabled = false;            
            chkSelectAll.Checked = true;

            this.Focus();
        }        

        public void LoadConstants()
        {            
            OUR_TEAM = (Team)Enum.Parse(typeof(Team), Constants.get<string>("configuration", "OUR_TEAM"), true);
            FIELD_HALF = (FieldHalf)Enum.Parse(typeof(FieldHalf), Constants.get<string>("plays", "FIELD_HALF"), true);
            PLAY_DIR = Constants.get<string>("default", "PLAY_DIR");

            LOG_FILE = Constants.get<string>("motionplanning", "LOG_FILE");
        }

        private void createSystem()
        {            
            _serial = new RemoteRobots();

            //_predictor = new BasicPredictor();
            _predictor = new AveragingPredictor();

            _vision = new Vision();
            _vision.MessageReceived += new EventHandler<VisionMessageEventArgs>(_vision_MessageReceived);
            _vision.ErrorOccured += new EventHandler(_vision_ErrorOccured);

            _player = new Player(OUR_TEAM, FIELD_HALF, _predictor, _serial, _fieldDrawer);            
        }        

        private void createSim()
        {
            _fieldDrawer1 = new FieldDrawer();
            _fieldDrawer2 = new FieldDrawer();

            _player1 = new Player(Team.Yellow, FieldHalf.Left, _physicsEngine, _physicsEngine, _fieldDrawer1);
            _player2 = new Player(Team.Blue, FieldHalf.Right, _physicsEngine, _physicsEngine, _fieldDrawer2);           
        }

        private void createSimple()
        {
            // Change the controller and predictor to the real ones (_predictor, _serial) to do real life
            lstSimplePlayer.Items.Add(new PathFollowerPlayer(OUR_TEAM, FIELD_HALF, _physicsEngine, _physicsEngine, _fieldDrawer));
            lstSimplePlayer.Items.Add(new KickPlayer(OUR_TEAM, FIELD_HALF, _physicsEngine, _physicsEngine, _fieldDrawer));
            lstSimplePlayer.Items.Add(new BeamKickPlayer(OUR_TEAM, FIELD_HALF, _physicsEngine, _physicsEngine, _fieldDrawer));
            lstSimplePlayer.SelectedIndex = 0;
            
            _fieldDrawer.WaypointAdded += _fieldDrawer_WaypointAdded;
            _fieldDrawer.WaypointRemoved += _fieldDrawer_WaypointRemoved;
            _fieldDrawer.WaypointMoved += _fieldDrawer_WaypointMoved;
        }      

        void _fieldDrawer_WaypointAdded(object sender, WaypointAddedEventArgs e)
        {
            RobotInfo waypoint = e.Object as RobotInfo;

            _waypoints.Add(waypoint);
            _fieldDrawer.BeginCollectState();
            int markerHandle = _fieldDrawer.AddMarker(waypoint.Position, e.Color, waypoint);            
            _waypointMarkers.Add(waypoint, markerHandle);
            _fieldDrawer.EndCollectState();
        }

        void _fieldDrawer_WaypointRemoved(object sender, WaypointRemovedEventArgs e)
        {
            RobotInfo waypoint = e.Object as RobotInfo;

            _fieldDrawer.BeginCollectState();
            _fieldDrawer.RemoveMarker(_waypointMarkers[waypoint]);
            _fieldDrawer.EndCollectState();

            _waypoints.Remove(waypoint);
            _waypointMarkers.Remove(waypoint);
        }

        void _fieldDrawer_WaypointMoved(object sender, WaypointMovedEventArgs e)
        {
            RobotInfo waypoint = e.Object as RobotInfo;
            waypoint.Position = e.NewLocation;
        }

        private void createRefboxCommandDisplayTimer()
        {
            const double interval = 1000; // ms
            _refboxCommandDisplayTimer = new System.Timers.Timer(interval);
            _refboxCommandDisplayTimer.AutoReset = true;
            _refboxCommandDisplayTimer.Elapsed += delegate(object sender, System.Timers.ElapsedEventArgs e)
            {                
                if (_refboxListener == null)
                    _fieldDrawer.UpdateRefBoxCmd("<DISCONNECTED>");
                else if (!_refboxListener.isReceiving())
                    _fieldDrawer.UpdateRefBoxCmd("<NO DATA>");
            };
            _refboxCommandDisplayTimer.Start();
        }

        private void reloadPlays()
        {
            _plays = new List<InterpreterPlay>(PlayUtils.loadPlays(PLAY_DIR).Keys);
            chkSelectAll.Checked = false;
            lstPlays.Items.Clear();
            lstPlays.Items.AddRange(_plays.ToArray());
            chkSelectAll.Checked = true;

            _plays1 = new List<InterpreterPlay>(PlayUtils.loadPlays(PLAY_DIR).Keys);
            _plays2 = new List<InterpreterPlay>(PlayUtils.loadPlays(PLAY_DIR).Keys);
        }

        private void connectRefbox()
        {
            string[] items = txtRefbox.Text.Split(new char[] { ':' });
            if (items.Length != 2)
            {
                MessageBox.Show("Refbox address must be in format: ip:port");
                return;
            }
            _refboxListener = new MulticastRefBoxListener(items[0], int.Parse(items[1]),
                delegate(char command)
                {
                    _fieldDrawer.UpdateRefBoxCmd(MulticastRefBoxListener.CommandCharToName(command));
                });

            lblRefbox.BackColor = Color.Green;
            btnRefbox.Text = "Disconnect";
            _refboxConnected = true;
        }
        private void disconnectRefbox()
        {
            _refboxListener.stop();
            _refboxListener.close();
            lblRefbox.BackColor = Color.Red;
            btnRefbox.Text = "Connect";
            _refboxConnected = false;
        }

        void _vision_MessageReceived(object sender, VisionMessageEventArgs e)
        {
            ((IVisionInfoAcceptor)_predictor).Update(e.VisionMessage);
        }

        void _vision_ErrorOccured(object sender, EventArgs e)
        {
            _visionConnected = false;
            sslVisionStatus.BackColor = Color.Red;
            sslVisionConnect.Text = "Connect";
        }

        private void btnSSLVision_Click(object sender, EventArgs e)
        {
            if (!_visionConnected)
            {
                int port;
                string hostname;
                string[] tokens = sslVisionHost.Text.Split(new char[] { ':' });
                if (tokens.Length != 2 || !int.TryParse(tokens[1], out port))
                {
                    MessageBox.Show("Invalid format of SSL Vision host. It must be \"hostname:port\"");
                    return;
                }
                hostname = tokens[0];
                
                _vision.Start(port, hostname);

                _visionConnected = true;
                sslVisionStatus.BackColor = Color.Green;
                sslVisionConnect.Text = "Disconnect";                
            }
            else
            {
                _visionConnected = false;
                sslVisionStatus.BackColor = Color.Red;
                sslVisionConnect.Text = "Connect";
            }
        }                  

        private void serialConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_serialConnected)
                {
                    if (_serial.start(serialHost.Text))
                    {
                        serialStatus.BackColor = Color.Green;
                        serialConnect.Text = "Disconnect";
                        _serialConnected = true;
                    }
                }
                else
                {
                    _serial.stop();
                    serialStatus.BackColor = Color.Red;
                    serialConnect.Text = "Connect";
                    _serialConnected = false;
                }
            }
            catch (Exception except)
            {
                Console.WriteLine(except.StackTrace);
            }

        }

        private void btnRefbox_Click(object sender, EventArgs e)
        {
            if (_player.Running || _player1.Running || _simplePlayer.Running)
            {
                MessageBox.Show("Cannot connect/disconnect from refbox while system is running");
                return;
            }

            if (!_refboxConnected)
                connectRefbox();
            else
                disconnectRefbox();
        }

        private void rfcStart_Click(object sender, EventArgs e) {
            if (!_player.Running) {
                _player.LoadPlays(_plays);
                _player.SetRefBoxListener(_refboxListener);
                _player.Start();
                rfcStatus.BackColor = Color.Green;
                rfcStart.Text = "Stop";
            } else {
                _player.Stop();
                rfcStatus.BackColor = Color.Red;
                rfcStart.Text = "Start";
            }
        }
   
        private void btnStartSim_Click(object sender, EventArgs e)
        {
            if (!_player1.Running && !_player2.Running)
            {
                _fieldDrawer1.Show();
                _fieldDrawer2.Show();

                _player1.LoadPlays(_plays1);
                _player2.LoadPlays(_plays2);

                _player1.SetRefBoxListener(_refboxListener);
                _player2.SetRefBoxListener(_refboxListener);

                _player1.Start();
                _player2.Start();
                _engine.start();

                lblSimStatus.BackColor = Color.Green;
                btnStartSim.Text = "Stop Sim";
            }
            else
            {
                _engine.stop();
                _player2.Stop();
                _player1.Stop();

                _fieldDrawer1.Hide();
                _fieldDrawer2.Hide();

                lblSimStatus.BackColor = Color.Red;
                btnStartSim.Text = "Start Sim";
            }
        }

        private void btnStartStopPlayer_Click(object sender, EventArgs e)
        {            
            if (!_simplePlayer.Running)
            {                
                if (_simplePlayer is PathFollowerPlayer)
                {
                    PathFollowerPlayer pathFollowerPlayer = _simplePlayer as PathFollowerPlayer;                    
                    pathFollowerPlayer.ClearWaypoints();
                    foreach (RobotInfo waypoint in _waypoints)
                        pathFollowerPlayer.AddWaypoint(waypoint);
                }
                _simplePlayer.Start();
                _engine.start();
                lstSimplePlayer.Enabled = false;
                lblSimplePlayerStatus.BackColor = Color.Green;
                btnStartStopPlayer.Text = "Stop Simple Player";
            }
            else
            {
                _engine.stop();
                _simplePlayer.Stop();
                lstSimplePlayer.Enabled = true;
                lblSimplePlayerStatus.BackColor = Color.Red;
                btnStartStopPlayer.Text = "Start Simple Player";                
            }
        }
        
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.R))
            {
                if (_player.Running || _player1.Running || _player2.Running || _simplePlayer.Running)
                {
                    MessageBox.Show("System running. Need to stop system to reload contants.");
                    return false;
                }

                Constants.Load();

                LoadConstants();

                _predictor.LoadConstants();
                _player.LoadConstants();

                reloadPlays();
                _player.LoadPlays(_plays);
                _player1.LoadPlays(_plays1);
                _player2.LoadPlays(_plays2);

                Console.WriteLine("Constants and plays reloaded.");
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }      
     
        private void lstPlays_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            InterpreterPlay play = lstPlays.Items[e.Index] as InterpreterPlay;
            play.isEnabled = e.NewValue == CheckState.Checked;
        }

        private void chkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < lstPlays.Items.Count; i++)
                lstPlays.SetItemChecked(i, chkSelectAll.Checked);
        }

        #region Logging
        private void loggingInit()
        {
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

        private void btnLogNext_Click(object sender, EventArgs e)
        {

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

        private void btnStartStopLogging_Click(object sender, EventArgs e)
        {
            /*if (!(_pathFollower.Planner is ILogger))
            {
                MessageBox.Show("Selected MotionPlanner does not implement ILogger interface.");
                return;
            }*/

            if (logging)
            {
                logging = false;
                btnStartStopLogging.Text = "Start log";
                btnLogOpenClose.Enabled = true;
            }
            else
            {
                logging = true;
                btnStartStopLogging.Text = "Stop log";
                btnLogOpenClose.Enabled = false;
            }

            // TODO: fix logging?
            // if enabled, start motion planner's logging
            //COMMENTED OUT- problems with designer...               
            /* if (_player.Controller is RFCController && ((RFCController)_player.Controller).Planner is ILogger) {
                ILogger logger = ((RFCController)_player.Controller).Planner as ILogger;

               
                if (!logger.Logging) {
                    logger.LogFile = LOG_FILE;
                    logger.StartLogging(int.Parse(txtRobotID.Text));

                }
                else {
                    logger.StopLogging();
                }
                 
            }*/
        }

        private void btnLogOpenClose_Click(object sender, EventArgs e)
        {
            if (_logReader.LogOpen)
            {
                _logReader.CloseLogFile();
                btnLogOpenClose.Text = "Open log";
                btnLogNext.Enabled = false;
                btnStartStopLogging.Enabled = true;
            }
            else
            {
                _logReader.OpenLogFile(LOG_FILE, _logLineFormat);
                btnLogOpenClose.Text = "Close log";
                btnLogNext.Enabled = true;
                btnStartStopLogging.Enabled = false;
            }


        }

        #endregion

        private void lstSimplePlayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_simplePlayer != null && _simplePlayer.Running)
                throw new ApplicationException("Cannot change player while running!");
            _simplePlayer = lstSimplePlayer.SelectedItem as Player;
        }     
    }
}