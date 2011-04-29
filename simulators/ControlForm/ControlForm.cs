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
using Robocup.Geometry;
using Robocup.Plays;

using System.Runtime.InteropServices;
using System.IO;
using System.Threading;

namespace Robocup.ControlForm {

    public partial class ControlForm : Form
    {
        const string WAYPOINTS_FILE = "waypoints.csv";

        bool _controllerConnected = false;
        bool _refboxConnected = false;
        bool _visionConnected = false;
        bool _formClosing = false;

        // Vision (+predictor) and refbox are created once and shared among the players 
        // (mostly for efficiency)
        protected Vision _vision;
        protected IPredictor _predictor;
        MulticastRefBoxListener _refboxListener;
        System.Timers.Timer _refboxCommandDisplayTimer;

        Player _selectedPlayer;

        // TODO: Make it possible for multiple players to share one field drawer.
        FieldDrawer _fieldDrawer, _fieldDrawer2;

        Dictionary<Player, List<InterpreterPlay>> _plays = new Dictionary<Player, List<InterpreterPlay>>();

        // Waypoints are grouped by color. Upon creation each player is assigned a color and will get the
        // waypoints of (only) that color upon start.
        Dictionary<Color, List<RobotInfo>> _waypoints = new Dictionary<Color, List<RobotInfo>>();
        Dictionary<WaypointPlayer, Color> _waypointColors = new Dictionary<WaypointPlayer, Color>();
        Dictionary<RobotInfo, int> _waypointMarkers = new Dictionary<RobotInfo, int>();
        double STEADY_STATE_SPEED; // Needed for going through waypoints

        PIDForm _pidForm;
        DebugForm _debugForm;

        // Logging
        LogReader _logReader;
        IPredictor _logPredictor;
        FieldDrawer _logFieldDrawer;
        List<Type> _logLineFormat;
        System.Timers.Timer _timer;
        bool _logging;
        string LOG_FILE;

        public ControlForm()
        {
            InitializeComponent();

            LoadConstants();
            
            _pidForm = new PIDForm();
            _pidForm.Show();

            _debugForm = DebugConsole.getForm();
            _debugForm.Show();

            _fieldDrawer = new FieldDrawer();
            _fieldDrawer.Show();

            // TODO: Make it possible for multiple players to draw into the same field drawer
            _fieldDrawer2 = new FieldDrawer();            
            _fieldDrawer2.Show();

            _fieldDrawer.WaypointAdded += _fieldDrawer_WaypointAdded;
            _fieldDrawer.WaypointRemoved += _fieldDrawer_WaypointRemoved;
            _fieldDrawer.WaypointMoved += _fieldDrawer_WaypointMoved;
            
            _vision = new Vision();
            _vision.MessageReceived += _vision_MessageReceived;

            //_predictor = new BasicPredictor();
            _predictor = new AveragingPredictor();

            _refboxListener = new MulticastRefBoxListener();
            _refboxListener.PacketReceived += _refboxListener_PacketReceived;

            loggingInit();
            
            createRefboxCommandDisplayTimer();
            
            foreach (FieldHalf fieldHalf in Enum.GetValues(typeof(FieldHalf)))
                cmbFieldHalf.Items.Add(fieldHalf);

            createPlayers();
            reloadPlays();

            lstPlayers.SelectedIndex = 0;

            btnLogNext.Enabled = false;
            chkSelectAll.Checked = true;
            
            cmbVisionHost.SelectedIndex = 0;
            cmbControllerHost.SelectedIndex = 0;
            cmbRefboxHost.SelectedIndex = 0;

            if (File.Exists(WAYPOINTS_FILE))
                loadWaypoints(WAYPOINTS_FILE);

            // Otherwise focus goes to console window and/or other forms
            this.BringToFront();
        }

        public void LoadConstants()
        {
            LOG_FILE = ConstantsRaw.get<string>("motionplanning", "LOG_FILE");
            STEADY_STATE_SPEED = ConstantsRaw.get<double>("motionplanning", "STEADY_STATE_SPEED");

            if (_predictor != null)
                _predictor.LoadConstants();
        }

        private void createPlayers()
        {
            Player player1 = new Player("Player1", Team.Yellow, FieldHalf.Right, _fieldDrawer, _predictor);
            Player player2 = new Player("Player2", Team.Blue, FieldHalf.Left, _fieldDrawer2, _predictor);
            Player yplayerFollower = new PathFollowerPlayer(Team.Yellow, FieldHalf.Right, _fieldDrawer, _predictor);
            Player bplayerFollower = new PathFollowerPlayer(Team.Blue, FieldHalf.Right, _fieldDrawer, _predictor);
        	Player yplayerMeasuringFollower = new MeasuringFollowerPlayer(Team.Yellow, FieldHalf.Right, _fieldDrawer, _predictor);
            Player bplayerMeasuringFollower = new MeasuringFollowerPlayer(Team.Blue, FieldHalf.Right, _fieldDrawer, _predictor);
            Player yplayerKick = new KickPlayer(Team.Yellow, FieldHalf.Right, _fieldDrawer, _predictor);
            Player bplayerKick = new KickPlayer(Team.Blue, FieldHalf.Right, _fieldDrawer, _predictor);
            Player yplayerBeamKick = new BeamKickPlayer(Team.Yellow, FieldHalf.Right, _fieldDrawer, _predictor);
            Player bplayerBeamKick = new BeamKickPlayer(Team.Blue, FieldHalf.Right, _fieldDrawer, _predictor);

            lstPlayers.Items.Add(player1);
            lstPlayers.Items.Add(player2);
            lstPlayers.Items.Add(yplayerFollower);
        	lstPlayers.Items.Add(yplayerMeasuringFollower);
            lstPlayers.Items.Add(yplayerKick);
            lstPlayers.Items.Add(yplayerBeamKick);
            lstPlayers.Items.Add(bplayerFollower);
            lstPlayers.Items.Add(bplayerMeasuringFollower);
            lstPlayers.Items.Add(bplayerKick);
            lstPlayers.Items.Add(bplayerBeamKick);

            // Determines the waypoints of which color will be given to each player 
            // Color list is defined in FieldDrawer form.
            _waypointColors[yplayerFollower as WaypointPlayer] = Color.Cyan;
			_waypointColors[yplayerMeasuringFollower as WaypointPlayer] = Color.Cyan;
            _waypointColors[yplayerKick as WaypointPlayer] = Color.Red;
            _waypointColors[yplayerBeamKick as WaypointPlayer] = Color.Red;
            _waypointColors[bplayerFollower as WaypointPlayer] = Color.Cyan;
            _waypointColors[bplayerMeasuringFollower as WaypointPlayer] = Color.Cyan;
            _waypointColors[bplayerKick as WaypointPlayer] = Color.Red;
            _waypointColors[bplayerBeamKick as WaypointPlayer] = Color.Red;
        }        

        // A timer that periodically resets the Refbox command indicator to be able to see when no 
        // packets are received. This is not done with the other drawing, because the #1 point of 
        // this refbox state output is to have an independent and direct reading from the refbox 
        // (e.g. to *quickly* and always know whether we are getting commands from official refbox 
        // during the game)
        private void createRefboxCommandDisplayTimer()
        {
            const double INTERVAL = 1000; // ms
            _refboxCommandDisplayTimer = new System.Timers.Timer(INTERVAL);
            _refboxCommandDisplayTimer.AutoReset = true;
            _refboxCommandDisplayTimer.Elapsed += delegate(object sender, System.Timers.ElapsedEventArgs e)
            {
                if (_fieldDrawer.Visible)
                {
                    if (_refboxListener == null)
                    {
                        _fieldDrawer.UpdateRefBoxCmd("<DISCONNECTED>");
                        _fieldDrawer2.UpdateRefBoxCmd("<DISCONNECTED>");
                    }
                    else if (!_refboxListener.IsReceiving())
                    {
                        _fieldDrawer.UpdateRefBoxCmd("<NO DATA>");
                        _fieldDrawer2.UpdateRefBoxCmd("<NO DATA>");
                    }
                }
            };
            _refboxCommandDisplayTimer.Start();
        }        

        private void _fieldDrawer_WaypointAdded(object sender, EventArgs<WaypointInfo> e)
        {
            addWaypoint(e.Data.Object as RobotInfo, e.Data.Color);
        }

        private void _fieldDrawer_WaypointRemoved(object sender, EventArgs<WaypointInfo> e)
        {
            removeWaypoint(e.Data.Object as RobotInfo, e.Data.Color);
        }

        private void _fieldDrawer_WaypointMoved(object sender, EventArgs<WaypointMovedInfo> e)
        {
            RobotInfo waypoint = e.Data.Object as RobotInfo;
            List<RobotInfo> waypoints = _waypoints[e.Data.Color];
            
            int ind = waypoints.IndexOf(waypoint);
            waypoint.Position = e.Data.NewLocation;
            waypoint.Orientation = e.Data.NewOrientation;
            
            #region Adjust velocities
            if(waypoints.Count > 1)
            {
                double speed = Math.Sqrt(waypoint.Velocity.magnitudeSq());
                
                int prevInd = ind - 1;
                if(prevInd < 0) prevInd = waypoints.Count-1;
                Vector2 newPrevVelocity = (waypoint.Position - waypoints[prevInd].Position).normalizeToLength(speed);
                waypoints[prevInd].Velocity = newPrevVelocity;

                int nextInd = (ind + 1) % (waypoints.Count - 1);
                Vector2 newNextVelocity = (waypoints[nextInd].Position - waypoint.Position).normalizeToLength(speed);
                waypoint.Velocity = newNextVelocity;
            }
            #endregion

        }

        private void addWaypoint(RobotInfo waypoint, Color color)
        {                        
            if (!_waypoints.ContainsKey(color))
                _waypoints.Add(color, new List<RobotInfo>());

            List<RobotInfo> waypoints = _waypoints[color];

            #region Adjust velocities
            double speed = STEADY_STATE_SPEED;
            if (waypoints.Count > 0)
            {
                Vector2 newLastVelocity = (waypoints[0].Position - waypoint.Position).normalizeToLength(speed);
                waypoint.Velocity = newLastVelocity;

                Vector2 newPrevToLastVelocity = (waypoint.Position - waypoints[waypoints.Count - 1].Position).normalizeToLength(speed);
                waypoints[waypoints.Count - 1].Velocity = newPrevToLastVelocity;
            }
            #endregion
            
            waypoints.Add(waypoint);

            _fieldDrawer.BeginCollectState();
            int markerHandle = _fieldDrawer.AddMarker(waypoint.Position, color, waypoint);
            _waypointMarkers.Add(waypoint, markerHandle);
            _fieldDrawer.EndCollectState();
        }

        private void removeWaypoint(RobotInfo waypoint, Color color)
        {
            List<RobotInfo> waypoints = _waypoints[color];

            _fieldDrawer.BeginCollectState();
            _fieldDrawer.RemoveMarker(_waypointMarkers[waypoint]);
            _fieldDrawer.EndCollectState();

            waypoints.Remove(waypoint);
            
            #region Adjust velocities
            if (waypoints.Count > 0)
            {
                RobotInfo lastWaypoint = waypoints[waypoints.Count - 1];
                double speed = Math.Sqrt(lastWaypoint.Velocity.magnitudeSq());
                Vector2 newVelocity = (lastWaypoint.Position - waypoints[0].Position).normalizeToLength(speed);
                waypoints[waypoints.Count - 1].Velocity = newVelocity;
            }
            #endregion

            _waypointMarkers.Remove(waypoint);
        }

        private void saveWaypoints(string file)
        {
            TextWriter writer = new StreamWriter(file, false);
            foreach (KeyValuePair<Color, List<RobotInfo>> pair in _waypoints)
            {
                foreach (RobotInfo waypoint in pair.Value)
                {
                    string line = String.Join(",", new string[] { waypoint.Position.X.ToString(), waypoint.Position.Y.ToString(),
                                                waypoint.Velocity.X.ToString(), waypoint.Velocity.Y.ToString(),
                                                waypoint.AngularVelocity.ToString(), waypoint.Orientation.ToString(), 
                                                waypoint.Team.ToString(), waypoint.ID.ToString(),
                                                pair.Key.Name});
                    writer.WriteLine(line);
                }
            }
            writer.Close();
        }

        private void loadWaypoints(string file)
        {
            TextReader reader = new StreamReader(file);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] tokens = line.Split(new char[] { ',' });
                RobotInfo waypoint = new RobotInfo(new Vector2(double.Parse(tokens[0]), double.Parse(tokens[1])),
                                                   new Vector2(double.Parse(tokens[2]), double.Parse(tokens[3])),
                                                   double.Parse(tokens[4]), double.Parse(tokens[5]),
                                                   (Team)Enum.Parse(typeof(Team), tokens[6]), int.Parse(tokens[7]));
                addWaypoint(waypoint, Color.FromName(tokens[8]));
            }
            reader.Close();
        }        
        
        private void reloadPlays()
        {
            _plays.Clear();

            // There's no better place than lstPlayers to get all the players
            foreach (Player player in lstPlayers.Items)
            {
                string TACTIC_DIR = ConstantsRaw.get<string>("default", "TACTIC_DIR");

                Dictionary<string, InterpreterTactic> tacticBook = PlayUtils.loadTactics(TACTIC_DIR);

                // Currently indexing by team, but could index by name or whatever here
                string PLAY_DIR = ConstantsRaw.get<string>("default", "PLAY_DIR_" + player.Team.ToString().ToUpper());

                List<InterpreterPlay> playObjs = new List<InterpreterPlay>(PlayUtils.loadPlays(PLAY_DIR, tacticBook).Keys);
                player.LoadPlays(playObjs);

                // Store for lookup from enable/disable list
                _plays[player] = playObjs;
            }            
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

        private bool checkAnyPlayerRunning()
        {
            foreach (Player player in lstPlayers.Items)
            {
                if (player.Running)
                {
                    MessageBox.Show("Cannot disconnect while a player is running.");
                    return true;
                }
            }
            return false;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.R))
            {
                if (_selectedPlayer.Running)
                {
                    MessageBox.Show("System running. Need to stop system to reload contants.");
                    return false;
                }

                ConstantsRaw.Load();

                LoadConstants();

                foreach (Player player in lstPlayers.Items)
                    player.LoadConstants();

                reloadPlays();

                Console.WriteLine("Constants and plays reloaded.");
                return true;
            }
            else if (keyData == (Keys.Control | Keys.S))
            {
                saveWaypoints(WAYPOINTS_FILE);
                Console.WriteLine("Waypoints saved.");
            }
            else if (keyData == (Keys.Control | Keys.L))
            {
                loadWaypoints(WAYPOINTS_FILE);
                Console.WriteLine("Waypoints loaded.");
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void _vision_MessageReceived(object sender, EventArgs<VisionMessage> e)
        {
            ((IVisionInfoAcceptor)_predictor).Update(e.Data);
        }

        private void _refboxListener_PacketReceived(object sender, EventArgs<char> e)
        {
            _fieldDrawer.UpdateRefBoxCmd(MulticastRefBoxListener.CommandCharToName(e.Data));
            _fieldDrawer2.UpdateRefBoxCmd(MulticastRefBoxListener.CommandCharToName(e.Data));
        }

        private void btnVision_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_visionConnected)
                {
                    int port;
                    string hostname;
                    if (!parseHost(cmbVisionHost.Text, out hostname, out port)) return;

                    _vision.Connect(hostname, port);
                    _vision.Start();

                    _visionConnected = true;
                    lblVisionStatus.BackColor = Color.Green;
                    btnVision.Text = "Disconnect";
                }
                else
                {
                    if (checkAnyPlayerRunning()) return;

                    _vision.Stop();
                    _vision.Disconnect();

                    _visionConnected = false;
                    lblVisionStatus.BackColor = Color.Red;
                    btnVision.Text = "Connect";
                }
            }
            catch (Exception except)
            {
                MessageBox.Show(except.Message + "\r\n" + except.StackTrace);
            }
        }

        private void btnController_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_controllerConnected)
                {
                    string hostname;
                    int port;
                    if (!parseHost(cmbControllerHost.Text, out hostname, out port)) return;

                    foreach (Player player in lstPlayers.Items)
                        player.ConnectToController(hostname, port);

                    lblControllerStatus.BackColor = Color.Green;
                    btnController.Text = "Disconnect";
                    _controllerConnected = true;
                }
                else
                {
                    if (checkAnyPlayerRunning()) return;

                    foreach (object player in lstPlayers.Items)
                        ((Player)player).DisconnectFromController();

                    lblControllerStatus.BackColor = Color.Red;
                    btnController.Text = "Connect";
                    _controllerConnected = false;
                }
            }
            catch (Exception except)
            {
                MessageBox.Show(except.Message + "\r\n" + except.StackTrace);
            }
        }

        private void btnRefbox_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_refboxConnected)
                {
                    string hostname;
                    int port;
                    if (!parseHost(cmbRefboxHost.Text, out hostname, out port)) return;

                    // Listener can only be created once, so have to do it here as opposed to inside RefBoxState
                    _refboxListener.Connect(hostname, port);                    

                    foreach (Player player in lstPlayers.Items)
                        player.ConnectToRefbox(_refboxListener);

                    _refboxListener.Start();

                    lblRefboxStatus.BackColor = Color.Green;
                    btnRefbox.Text = "Disconnect";
                    _refboxConnected = true;
                }
                else
                {
                    if (checkAnyPlayerRunning()) return;

                    _refboxListener.Stop();
                    _refboxListener.Disconnect();

                    foreach (Player player in lstPlayers.Items)
                        player.DisconnectFromRefbox();
                    
                    lblRefboxStatus.BackColor = Color.Red;
                    btnRefbox.Text = "Connect";
                    _refboxConnected = false;
                }
            }
            catch (Exception except)
            {
                MessageBox.Show(except.Message + "\r\n" + except.StackTrace);
            }
        }

        private void btnStartPlayer_Click(object sender, EventArgs e)
        {
            if (_selectedPlayer.Running)
            {
                MessageBox.Show("Selected player already running.");
                return;
            }

            if (!_controllerConnected)
            {
                MessageBox.Show("Controller must be connected to run a player.");
                return;
            }

            if (_selectedPlayer is WaypointPlayer)
            {
                WaypointPlayer waypointPlayer = _selectedPlayer as WaypointPlayer;
                waypointPlayer.ClearWaypoints();
                if (_waypoints.ContainsKey(_waypointColors[waypointPlayer]))
                    foreach (RobotInfo waypoint in _waypoints[_waypointColors[waypointPlayer]])
                        waypointPlayer.AddWaypoint(waypoint);
            }

            _selectedPlayer.Start();

            // Running status has changed for a player, so make sure it is (un)bold-ed in the list
            lstPlayers.Invalidate();
        }

        private void btnStopPlayer_Click(object sender, EventArgs e)
        {
            if (!_selectedPlayer.Running)
            {
                MessageBox.Show("Selected player not running.");
                return;
            }

            _selectedPlayer.Stop();            

            // Running status has changed for a player, so make sure it is (un)bold-ed in the list
            lstPlayers.Invalidate();
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


            _timer = new System.Timers.Timer();
            _timer.Interval = 500;
            //timer.Tick += new EventHandler(InvalidateTimerHanlder);
            _timer.Start();
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

            // TODO: Maybe resurrect logging?
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

            if (_logging)
            {
                _logging = false;
                btnStartStopLogging.Text = "Start log";
                btnLogOpenClose.Enabled = true;
            }
            else
            {
                _logging = true;
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

        private void lstPlayers_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            _selectedPlayer = lstPlayers.SelectedItem as Player;

            // We don't want to trigger the event from here
            cmbFieldHalf.SelectedIndexChanged -= cmbFieldHalf_SelectedIndexChanged;
            cmbFieldHalf.SelectedItem = _selectedPlayer.FieldHalf;
            cmbFieldHalf.SelectedIndexChanged += cmbFieldHalf_SelectedIndexChanged;

            if (_selectedPlayer is WaypointPlayer)
                txtPlayerRobotID.Text = ((WaypointPlayer)_selectedPlayer).RobotID.ToString();

            // Replace the content of plays list box with the selected player's plays
            lstPlays.Items.Clear();

            foreach (InterpreterPlay play in _plays[_selectedPlayer])
            {
                int idx = lstPlays.Items.Add(play);
                lstPlays.SetItemChecked(idx, play.isEnabled);
            }
        }

        // For displaying the running players in bold
        private void lstPlayers_DrawItem(object sender, DrawItemEventArgs e)
        {
            // Draw the background of the ListBox control for each item.
            e.DrawBackground();

            Player player = (Player)lstPlayers.Items[e.Index];

            Font font = e.Font;
            if (player.Running)                
                 font = new Font(e.Font.FontFamily, e.Font.SizeInPoints, FontStyle.Bold);

            e.Graphics.DrawString(player.ToString(),
                font, Brushes.Black, e.Bounds, StringFormat.GenericDefault);
            e.DrawFocusRectangle();
        }

        private void cmbFieldHalf_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_selectedPlayer.Running)
            {
                MessageBox.Show("Cannot change field half while player is running.");
                return;
            }
            _selectedPlayer.FieldHalf = (FieldHalf)cmbFieldHalf.SelectedItem;
        }

        private void txtSimplePlayerRobotID_TextChanged(object sender, EventArgs e)
        {
            if (_selectedPlayer.Running)
            {
                MessageBox.Show("Cannot change robot ID while player is running.");
                return;
            }
            if (!(_selectedPlayer is WaypointPlayer))
            {
                MessageBox.Show("Selected player does not need a robot ID.");
                return;
            }

            WaypointPlayer wpPlayer = _selectedPlayer as WaypointPlayer;
            wpPlayer.RobotID = int.Parse(txtPlayerRobotID.Text);
        }        

        private void ControlForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_formClosing) return;

            // Stop everything for clean exit (emulate user actions, to not have to worry about
            // editting this method)
            object[] players = new object[lstPlayers.Items.Count];
            lstPlayers.Items.CopyTo(players, 0);
            foreach (Player player in players)
            {
                if (player.Running)
                {
                    lstPlayers.SelectedItem = player;
                    btnStopPlayer_Click(null, null);
                }
            }

            if (_refboxConnected)
                btnRefbox_Click(null, null);
            if (_visionConnected)
                btnVision_Click(null, null);
            if (_controllerConnected)
                btnController_Click(null, null);                


            // Need to give some time for the threads that the above calls spawn to 
            // complete, *without stalling this thread*, hence this bizzare code here
            _formClosing = true;
            e.Cancel = true;

            Thread shutdownThread = new Thread(delegate(object state)
            {
                Thread.Sleep(2000);
                Application.Exit();
            });
            shutdownThread.Start();
        }
    }
}