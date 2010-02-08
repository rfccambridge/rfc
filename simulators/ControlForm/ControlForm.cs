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
using System.IO;
using System.Threading;

namespace Robocup.ControlForm {

    public partial class ControlForm : Form
    {
        const string WAYPOINTS_FILE = "waypoints.csv";

        Team OUR_TEAM;
        FieldHalf FIELD_HALF;
        string PLAY_DIR;        

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

        // Play storage: play identifier -> list of copies of that play owned by each player
        // This is just one way to allow play enable/disable functionality *for all players at once*. At 
        // this time it seems most *convenient* to me to have enable/disable to propagate to all players 
        // (in the same way as vision/controller/refbox connectivity does), this could be changed to the 
        // more natural player-specific control if need be. Oh, and: no, you cannot share play objects!
        Dictionary<string, List<InterpreterPlay>> plays = new Dictionary<string, List<InterpreterPlay>>();

        List<RobotInfo> _waypoints = new List<RobotInfo>();
        Dictionary<RobotInfo, int> _waypointMarkers = new Dictionary<RobotInfo, int>();

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

            createPlayers();
            lstPlayers.SelectedIndex = 0;

            reloadPlays();

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
            OUR_TEAM = (Team)Enum.Parse(typeof(Team), Constants.get<string>("configuration", "OUR_TEAM"), true);
            FIELD_HALF = (FieldHalf)Enum.Parse(typeof(FieldHalf), Constants.get<string>("plays", "FIELD_HALF"), true);
            PLAY_DIR = Constants.get<string>("default", "PLAY_DIR");

            LOG_FILE = Constants.get<string>("motionplanning", "LOG_FILE");
        }

        private void createPlayers()
        {
            lstPlayers.Items.Add(new Player("RFC", OUR_TEAM, FIELD_HALF, _fieldDrawer, _predictor));

            lstPlayers.Items.Add(new Player("Sim1", Team.Yellow, FieldHalf.Left, _fieldDrawer, _predictor));
            lstPlayers.Items.Add(new Player("Sim2", Team.Blue, FieldHalf.Right, _fieldDrawer2, _predictor));

            lstPlayers.Items.Add(new PathFollowerPlayer(OUR_TEAM, FIELD_HALF, _fieldDrawer, _predictor));
            lstPlayers.Items.Add(new KickPlayer(OUR_TEAM, FIELD_HALF, _fieldDrawer, _predictor));
            lstPlayers.Items.Add(new BeamKickPlayer(OUR_TEAM, FIELD_HALF, _fieldDrawer, _predictor));
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

        private void _fieldDrawer_WaypointAdded(object sender, WaypointAddedEventArgs e)
        {
            addWaypoint(e.Object as RobotInfo, e.Color);
        }

        private void _fieldDrawer_WaypointRemoved(object sender, WaypointRemovedEventArgs e)
        {
            removeWaypoint(e.Object as RobotInfo);
        }

        private void _fieldDrawer_WaypointMoved(object sender, WaypointMovedEventArgs e)
        {
            RobotInfo waypoint = e.Object as RobotInfo;
            waypoint.Position = e.NewLocation;
        }

        private void addWaypoint(RobotInfo waypoint, Color color)
        {
            _waypoints.Add(waypoint);
            _fieldDrawer.BeginCollectState();
            int markerHandle = _fieldDrawer.AddMarker(waypoint.Position, color, waypoint);
            _waypointMarkers.Add(waypoint, markerHandle);
            _fieldDrawer.EndCollectState();
        }

        private void removeWaypoint(RobotInfo waypoint)
        {
            _fieldDrawer.BeginCollectState();
            _fieldDrawer.RemoveMarker(_waypointMarkers[waypoint]);
            _fieldDrawer.EndCollectState();

            _waypoints.Remove(waypoint);
            _waypointMarkers.Remove(waypoint);
        }

        private void saveWaypoints(string file)
        {
            TextWriter writer = new StreamWriter(file, false);
            foreach (RobotInfo waypoint in _waypoints)
            {
                string line = String.Join(",", new string[] { waypoint.Position.X.ToString(), waypoint.Position.Y.ToString(),
                                                waypoint.Velocity.X.ToString(), waypoint.Velocity.Y.ToString(),
                                                waypoint.AngularVelocity.ToString(), waypoint.Orientation.ToString(), 
                                                waypoint.Team.ToString(), waypoint.ID.ToString(),
                                                _fieldDrawer.GetMarkerColor(_waypointMarkers[waypoint]).ToArgb().ToString()});
                writer.WriteLine(line);
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
                addWaypoint(waypoint, Color.FromArgb(int.Parse(tokens[8])));
            }
            reader.Close();
        }        
        
        private void reloadPlays()
        {
            bool firstPlayer = true;

            chkSelectAll.Checked = false;
            lstPlays.Items.Clear();
            plays.Clear();

            // There's no better place than lstPlayers to get all the players
            foreach (Player player in lstPlayers.Items)
            {
                List<InterpreterPlay> playObjs = new List<InterpreterPlay>(PlayUtils.loadPlays(PLAY_DIR).Keys);
                player.LoadPlays(playObjs);

                // Store for lookup from enable/disable list
                // See comment on top for "plays" for why this is setup like this                
                foreach (InterpreterPlay play in playObjs)
                {
                    if (firstPlayer)
                    {
                        List<InterpreterPlay> playCopies = new List<InterpreterPlay>();
                        playCopies.Add(play);
                        plays.Add(play.Name, playCopies);
                        lstPlays.Items.Add(play.Name);
                    }
                    else
                    {
                        // Append copies into the bin identified by the play name                         
                        plays[play.Name].Add(play);
                    }
                }                
                firstPlayer = false;
            }

            chkSelectAll.Checked = true;
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

                Constants.Load();

                LoadConstants();

                foreach (object player in lstPlayers.Items)
                    ((Player)player).LoadConstants();

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
                foreach (RobotInfo waypoint in _waypoints)
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
            string playName = lstPlays.Items[e.Index] as string;
            
            // Enable/disable the corresponding play object *for each player*
            foreach (InterpreterPlay play in plays[playName])
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