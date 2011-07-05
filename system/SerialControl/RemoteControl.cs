using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using Robocup.Core;
using Robocup.MessageSystem;
using Robocup.Utilities;
using System.Threading;
using Robocup.CoreRobotics;
using System.IO;

namespace Robocup.SerialControl {
    public partial class RemoteControl : Form {
        private delegate void VoidLabelStringDelegate(Label lbl, string arg);
        private delegate void VoidLabelColorDelegate(Label lbl, Color arg);
        private delegate void VoidListBoxObjectIntDelegate(ListBox lst, object item, int maxItems);
        private delegate void VoidListBoxObjectsDelegate(ListBox lst, object[] item);

        public delegate void DataAcquired(object sender);
        private event DataAcquired OnDataAcquired;

        private const int NUM_ROBOTS = 5;
        private const int DEFAULT_SPEED = 30;
        private const string DATA_IN_FILE = "data.csv";

        private int _speed = DEFAULT_SPEED;
        private bool _formClosing = false;
        private bool _active = false;
        private bool _cmdOutRemoteHost = false;
        private bool _cmdOutComPort = false;
        private bool _cmdListening = false;
        private bool _dataListening = false;
        private bool _joystickConnected = false;
        private IMessageSender<RobotCommand> _cmdSender;
        private IMessageReceiver<RobotCommand> _cmdReceiver;
        private System.Timers.Timer _rebootTimer = new System.Timers.Timer();
        private System.Timers.Timer _noCommandTimer = new System.Timers.Timer(500);
        private DateTime _lastCommandTime = DateTime.MaxValue;
        private object _lastCommandTimeLock = new object();
        private SerialPort _comPort;
        private SerialInput _serialInput = new SerialInput();
        private StreamWriter _dataInWriter;
        private KeyboardHook _keyboardHook = new KeyboardHook();
        private JoystickInterface.JoystickWrapper _joystickInterface;
        private System.Timers.Timer _joystickTimer = new System.Timers.Timer(100);
        private int _joystickTimerSync = 0;
        private bool _joystickDrivingOn = false;
        private int _curRobot;
        private RobotModel[] _robotModels = new RobotModel[NUM_ROBOTS];

        private Dictionary<string, WheelSpeedFunctions.IWheelSpeedFunction> _wheelSpeedFunctions = 
                new Dictionary<string, WheelSpeedFunctions.IWheelSpeedFunction>();
        private bool _wheelSpeedFunctionRunning = false;
        private Thread _wheelSpeedFunctionThread;
        private HighResTimer _wheelSpeedFunctionTimer = new HighResTimer();
        private WheelSpeedFunctions.IWheelSpeedFunction _wheelSpeedFunction;
        private double _lastSerialData = 0;

        private HighResTimer _receiveDurationTimer = new HighResTimer();
        private double _receiveDuration = 0.0;
        private object _receiveDurationLock = new Object();

        private System.Timers.Timer _guiTimer = new System.Timers.Timer(1000);

        public RemoteControl() {           
            InitializeComponent();
            txtCommandList.Text = 
                  "up,down,left,right = move (raw wheel speeds)\r\n" +
                  "numpad directions == move (DriveInDirection)\r\n" +
                  ",      ============= rotate anti-clockwise\r\n" +
                  ".      ============= rotate clockwise\r\n" +
                  "b      ============= enable break-beam\r\n" +
                  "f      ============= full-strength bb kick\r\n" +
                  "m      ============= min-strength bb kick\r\n" +
                  "c      ============= start charging kicker\r\n" +
                  "k      ============= stop charging kicker\r\n" +
                  "space  ============= fire kicker\r\n" +
                  "d      ============= dribbler on\r\n" +
                  "=      ============= speed up\r\n" +
                  "-      ============= speed down\r\n" +
                  "p      ============= stop\r\n" +
                  "0      ============= robot0\r\n" +
                  "1      ============= robot1\r\n" +
                  "2      ============= robot2\r\n" +
                  "3      ============= robot3\r\n" +
                  "4      ============= robot4\r\n";

            _curRobot = 0;

            // Non-raw driving uses models to convert forward+lateral velocity into wheelspeeds
            for (int i = 0; i < NUM_ROBOTS; i++)
                _robotModels[i] = new FailSafeModel(i);

            _joystickInterface = new JoystickInterface.JoystickWrapper(this);
            _joystickTimer.AutoReset = true;
            _joystickTimer.Elapsed += joystickTimer_Elapsed;

            // WheelSpeed Functions
            _wheelSpeedFunctions.Add("Ramp function", new WheelSpeedFunctions.Ramp());
            _wheelSpeedFunctions.Add("Sine wave", new WheelSpeedFunctions.Sine());
            _wheelSpeedFunctions.Add("Changing sine wave", new WheelSpeedFunctions.ChangingSine());
            _wheelSpeedFunctions.Add("Step function", new WheelSpeedFunctions.Step());
            _wheelSpeedFunctions.Add("Square function", new WheelSpeedFunctions.Square());

            cmbWheelSpeedFunction.Items.AddRange(new List<string>(_wheelSpeedFunctions.Keys).ToArray());
            cmbWheelSpeedFunction.SelectedIndex = 0;

            lblSpeed.Text = _speed.ToString();
            lblID.Text = _curRobot.ToString();

            // reboot timer            
            _rebootTimer.AutoReset = true;
            _rebootTimer.Elapsed += rebootTimer_Elapsed;

            // Clear the sent command label to be able to distinguish when nothing is being sent
            _noCommandTimer.AutoReset = true;
            _noCommandTimer.Elapsed += _noCommandTimer_Elapsed;
            _noCommandTimer.Start();

            // Update gui timer
            _guiTimer.AutoReset = true;
            _guiTimer.Elapsed += guiTimer_Elapsed;

            OnDataAcquired = this.OnDataAcquiredEvent;

            // Global hotkeys
            _keyboardHook.KeyPressed += backspace_GlobalHotkeyPressed;
            _keyboardHook.RegisterHotKey(Robocup.Utilities.ModifierKeys.Control | Robocup.Utilities.ModifierKeys.Alt, Keys.Back);

            radioButtonSerial.Checked = true;

            btnCmdListen_Click(null, null);
            btnConnectJoystick_Click(null, null);
        }        

        private void sendCommand(RobotCommand command)
        {
            if (_active)
            {
                if (_cmdOutComPort)
                {
                    byte[] packet = command.ToPacket();
                    _comPort.Write(packet, 0, packet.Length);
                }
                else if (_cmdOutRemoteHost)
                {
                    _cmdSender.Post(command);                    
                }

                string status = command.command.ToString();
                if (command.command == RobotCommand.Command.MOVE)
                    status += ": " + command.Speeds.ToString();
                updateLabel(lblSentCommand, status);
                lock (_lastCommandTimeLock)
                {
                    _lastCommandTime = DateTime.Now;
                }
            }
        }        

        private void stopEverything(int robotID)
        {
            sendCommand(new RobotCommand(robotID, RobotCommand.Command.MOVE,
                          new WheelSpeeds(0, 0, 0, 0)));
            sendCommand(new RobotCommand(robotID, RobotCommand.Command.STOP_CHARGING));
            sendCommand(new RobotCommand(robotID, RobotCommand.Command.STOP_DRIBBLER));
            sendCommand(new RobotCommand(robotID, RobotCommand.Command.KICK)); // to discharge
        }

        private void updateLabel(Label label, string text)
        {
            this.Invoke(new VoidLabelStringDelegate(delegate(Label lbl, string txt)
            {
                lbl.Text = text;
            }), new object[] { label, text });
        }

        private void updateLabel(Label label, Color backColor)
        {
            this.Invoke(new VoidLabelColorDelegate(delegate(Label lbl, Color color)
            {
                lbl.BackColor = color;
            }), new object[] { label, backColor });
        }

        private void addItem(ListBox lstBox, object item, int maxItems)
        {
            this.Invoke(new VoidListBoxObjectIntDelegate(delegate(ListBox lst, object obj, int maxCount)
            {
                lstBox.Items.Insert(1,obj);
                if (lstBox.Items.Count > maxCount)
                    lstBox.Items.RemoveAt(maxCount);
            }), new object[] { lstBox, item, maxItems });
        }

        private void showItemRange(ListBox lstBox, object items)
        {
            this.Invoke(new VoidListBoxObjectsDelegate(delegate(ListBox lst, object[] obj)
            {
                lstBox.Items.Clear();
                lstBox.Items.AddRange(obj);
            }), new object[] { lstBox, items});
        }

        private void backspace_GlobalHotkeyPressed(object sender, KeyPressedEventArgs e)
        {
            KeyEventArgs eventArgs = new KeyEventArgs(e.Key);
            int oldCurRobot = _curRobot;
            for (int i = 0; i < NUM_ROBOTS; i++)
                stopEverything(i);
        }

        private void btnConnectSending_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_active)
                {
                    if (_cmdOutComPort)
                    {
                        string port = "COM" + ((int)udCmdOutCOMPort.Value).ToString();
                        _comPort = Robocup.Utilities.SerialPortManager.OpenSerialPort(port);

                        // To be able to get key events immediately
                        txtCommandList.Focus();
                    }
                    else if (_cmdOutRemoteHost)
                    {
                        int port;
                        string hostname;
                        string[] tokens = textBoxRemoteHost.Text.Split(new char[] { ':' });
                        if (tokens.Length != 2 || !int.TryParse(tokens[1], out port))
                        {
                            MessageBox.Show("Invalid format of remote host. It must be \"hostname:port\"");
                            return;
                        }
                        hostname = tokens[0];

                        if (hostname != "localhost" && hostname != "127.0.0.1")
                            this._cmdSender = Messages.CreateClientSender<RobotCommand>(hostname, port);
                        else
                            MessageBox.Show("don't create a loop like that!");
                    }

                    _active = true;
                    lblSendStatus.BackColor = Color.Green;
                    btnConnectSending.Text = "Disconnect";
                }
                else
                {
                    if (_cmdOutComPort)
                    {
                        Robocup.Utilities.SerialPortManager.CloseSerialPort(_comPort);
                    }
                    else if (_cmdOutRemoteHost)
                    {
                        _cmdSender.Close();
                        _cmdSender = null;
                    }

                    _active = false;
                    lblSendStatus.BackColor = Color.Red;
                    btnConnectSending.Text = "Connect";
                }
            }
            catch (Exception except)
            {
                MessageBox.Show(except.Message + "\r\n" + except.StackTrace);
                return;
            }
        }

        private void RemoteControl_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_formClosing) return;

            // Cleanly disconnect/stop everything that was running
            if (_cmdListening)
                btnCmdListen_Click(null, null);
            if (_dataListening)
                btnDataListen_Click(null, null);
            if (_wheelSpeedFunctionRunning)
                btnStartStopWheelSpeedFunction_Click(null, null);
            if (_active)
                btnConnectSending_Click(null, null);
            if (_joystickConnected)
                btnConnectJoystick_Click(null, null);

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

        private void RemoteControl_KeyDown(object sender, KeyEventArgs e) {
            if (_active) {
                #region keyboard control
                RobotCommand command;
                switch (e.KeyCode) {
                    case Keys.Back:
                        stopEverything(_curRobot);
                        break;
                    case Keys.Left: // left move left in x
                        sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.MOVE,
                                     new WheelSpeeds(_speed, _speed, -_speed, -_speed)));
                        break;
                    case Keys.Right: // right move right in x
                        sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.MOVE,
                                     new WheelSpeeds(-_speed, -_speed, _speed, _speed)));
                        break;
                    case Keys.Up: // up move forward in y
                        sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.MOVE,
                                     new WheelSpeeds(_speed, -_speed, -_speed, _speed)));
                        break;
                    case Keys.Down: // down move backward in y
                        sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.MOVE,
                                     new WheelSpeeds(-_speed, _speed, _speed, -_speed)));
                        break;
                    case Keys.Oemcomma: // , rotate anti-clockwise
                        sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.MOVE,
                                     new WheelSpeeds(_speed, _speed, _speed, _speed)));
                        break;
                    case Keys.OemPeriod: // . rotate clockwise
                        sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.MOVE,
                                     new WheelSpeeds(-_speed, -_speed, -_speed, -_speed)));
                        break;
                    case Keys.NumPad8: // up arrow (8) drive forwards
                        sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.MOVE,
                                     _robotModels[_curRobot].DriveInDirection(_speed, 1.0, 0)));
                        break;
                    case Keys.NumPad2: // down arrow (2) drive backwards
                        sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.MOVE,
                                     _robotModels[_curRobot].DriveInDirection(_speed, -1.0, 0)));
                        break;
                    case Keys.NumPad4: // left arrow (4) drive left
                        sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.MOVE,
                                     _robotModels[_curRobot].DriveInDirection(_speed, 0.0, 1.0)));
                        break;
                    case Keys.NumPad6: // right arrow (6) drive right
                        sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.MOVE,
                                     _robotModels[_curRobot].DriveInDirection(_speed, 0.0, -1.0)));                        
                        break;
                    case Keys.NumPad7: // numpad 7 positive x, positive y
                        sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.MOVE,
                                     _robotModels[_curRobot].DriveInDirection(_speed, 1.0, 1.0)));
                        break;
                    case Keys.NumPad3: // numpad 3 negative x, negative y
                        sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.MOVE,
                                     _robotModels[_curRobot].DriveInDirection(_speed, -1.0, -1.0)));
                        break;
                    case Keys.NumPad9: // numpad 9 positive x, negative  y
                        sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.MOVE,
                                     _robotModels[_curRobot].DriveInDirection(_speed, 1.0, -1.0)));
                        break;
                    case Keys.NumPad1: // numpad 1 negative x, positive y
                        sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.MOVE,
                                     _robotModels[_curRobot].DriveInDirection(_speed, -1.0, 1.0)));
                        break;
                    case Keys.B: // b break-beam kick
                        sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.BREAKBEAM_KICK));
                        break;
                    case Keys.F: // f full break-beam kick
                        command = new RobotCommand(_curRobot, RobotCommand.Command.FULL_BREAKBEAM_KICK);
                        command.KickerStrength = (byte)udKickStrength.Value;
                        sendCommand(command);
                        break;
                    case Keys.M: // m break-beam kick with a minimum charge to kick
                        command = new RobotCommand(_curRobot, RobotCommand.Command.MIN_BREAKBEAM_KICK);
                        command.KickerStrength = (byte)udKickStrength.Value;
                        sendCommand(command);
                        break;
                    case Keys.C: // c charge kicker
                        sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.START_CHARGING));
                        break;
                    case Keys.K: // k stop charging
                        sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.STOP_CHARGING));
                        break;
                    case Keys.Space: // space fire kicker
                        sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.KICK));
                        break;                    
                    case Keys.D: // d dribbler on
                        sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.START_DRIBBLER));
                        break;
                    case Keys.P: // p stop   
                        sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.MOVE,
                                     new WheelSpeeds(0,0,0,0)));
                        break;
                    case Keys.R: // r reset boards
                        sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.RESET));
                        break;
                    case Keys.Oemplus: // = increase speed
                        _speed += 1;
                        lblSpeed.Text = _speed.ToString();
                        break;                        
                    case Keys.OemMinus: // - decrease speed
                        _speed -= 1;
                        lblSpeed.Text = _speed.ToString();
                        break;
                    case Keys.D0: // digit key robot ids
                    case Keys.D1:
                    case Keys.D2:
                    case Keys.D3:
                    case Keys.D4:
                    case Keys.D5:
                    case Keys.D6:
                    case Keys.D7:
                    case Keys.D8:
                    case Keys.D9:
                        _curRobot = e.KeyValue - 48;  // 48 is value for Keys.D0
                        lblID.Text = _curRobot.ToString();
                        break;
                    default:
                        sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.MOVE,
                                     new WheelSpeeds(0, 0, 0, 0)));
                        break;
                }
                #endregion
            }
        }
        
        private void RemoteControl_KeyUp(object sender, KeyEventArgs e) {
            if (_active) {
                sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.MOVE,
                    new WheelSpeeds(0, 0, 0, 0)));
                Thread thread = new System.Threading.Thread(delegate()
                {
                    System.Threading.Thread.Sleep(100);
                    sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.MOVE,
                                 new WheelSpeeds(0, 0, 0, 0)));
                });
                thread.Start();
            }
        }

        private void RemoteControl_Load(object sender, EventArgs e) {
            //this.toggleSettings(sender, e);

            //rcom.LoadMotorScale("C:\\Microsoft Robotics Studio (1.0)\\samples\\MasterCommander\\scaling.txt");
        }

        private void radioButtonSerial_CheckedChanged(object sender, EventArgs e) {
            _cmdOutComPort = radioButtonSerial.Checked;
        }

        private void radioButtonRemote_CheckedChanged(object sender, EventArgs e) {
            _cmdOutRemoteHost = radioButtonRemote.Checked;
        }

        private void chkRebootTimerEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRebootTimerEnabled.Checked)
            {
                _rebootTimer.Interval = int.Parse(txtRebootTimerInterval.Text);
                _rebootTimer.Start();
                Console.WriteLine("Reboot timer started");
            }
            else
            {             
                _rebootTimer.Stop();             
                Console.WriteLine("Reboot timer stopped");
            }
        }

        private void rebootTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            for (int i = 0; i < NUM_ROBOTS; i++)
            {
                Console.WriteLine("Rebooting robot: " + i);
                sendCommand(new RobotCommand(i, RobotCommand.Command.RESET));
            }
        }

        private void _noCommandTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            TimeSpan timeSinceLast;
            lock (_lastCommandTimeLock)
            {
                timeSinceLast = DateTime.Now - _lastCommandTime;
            }
            if (timeSinceLast.TotalMilliseconds > 500)
                updateLabel(lblSentCommand, "<NO CMD>");
        }

        private void btnCmdListen_Click(object sender, EventArgs e)
        {
            if (!_cmdListening)
            {
                _cmdReceiver = Messages.CreateServerReceiver<RobotCommand>(int.Parse(txtListenPort.Text));
                _cmdReceiver.MessageReceived += delegate(RobotCommand command)
                {
                    _receiveDurationTimer.Start();
                    sendCommand(command);
                    _receiveDurationTimer.Stop();
                    lock (_receiveDurationLock)
                    {
                        _receiveDuration = _receiveDurationTimer.Duration;
                    }
                };
                _cmdListening = true;
                btnCmdListen.Text = "Stop listening";
                lblListenStatus.BackColor = Color.Green;

                _guiTimer.Start();

                // To immediately listen for key events
                txtCommandList.Focus();
            }
            else
            {
                _cmdReceiver.Close();
                _cmdReceiver = null;
                _cmdListening = false;
                btnCmdListen.Text = "Listen";
                lblListenStatus.BackColor = Color.Red;

                _guiTimer.Stop();
            }
        }

        private void btnKick_Click(object sender, EventArgs e)
        {
            sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.KICK));
        }        

        private void btnStartCharging_Click(object sender, EventArgs e)
        {
            RobotCommand command = new RobotCommand(_curRobot, RobotCommand.Command.START_VARIABLE_CHARGING);
            command.KickerStrength = (byte)udKickStrength.Value;
            sendCommand(command);
        }        

        private void btnStopCharging_Click(object sender, EventArgs e)
        {
            sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.STOP_CHARGING));
        }

        private void btnBreakBeamKick_Click(object sender, EventArgs e)
        {
            RobotCommand command = new RobotCommand(_curRobot, RobotCommand.Command.MIN_BREAKBEAM_KICK);
            command.KickerStrength = (byte)udKickStrength.Value;
            sendCommand(command);
        }

        private void btnStartDribbler_Click(object sender, EventArgs e)
        {
            sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.START_DRIBBLER));
        }

        private void btnStopDribbler_Click(object sender, EventArgs e)
        {
            sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.STOP_DRIBBLER));
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.RESET));
        }

        private void btnDischarge_Click(object sender, EventArgs e)
        {
            sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.DISCHARGE));
        }

        private void btnSetPID_Click(object sender, EventArgs e)
        {
            sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.SET_PID,
                         byte.Parse(txtP.Text), byte.Parse(txtI.Text), byte.Parse(txtD.Text)));
        }

        private void btnSetCfgFlags_Click(object sender, EventArgs e)
        {
            byte flags = 0;
            flags |= (byte)(chkCfgSpewEncoder.Checked ?  (1 << 0) : 0);
            flags |= (byte)(chkCfgSpewPktStats.Checked ? (1 << 1) : 0);
            flags |= (byte)(chkCfgFeedback.Checked ?     (1 << 2) : 0);
            sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.SET_CFG_FLAGS, 
                        (byte)udBoardID.Value, flags));
        }
        
        private void btnSetWheelSpeeds_Click(object sender, EventArgs e)
        {
            sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.MOVE,
                         new WheelSpeeds(sbyte.Parse(txtRF.Text), sbyte.Parse(txtLF.Text),
                                         sbyte.Parse(txtLB.Text), sbyte.Parse(txtRB.Text))));
        }

        private void btnSendPacket_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void btnConnectJoystick_Click(object sender, EventArgs e)
        {
            if (!_joystickConnected)
            {
                string[] sticks = _joystickInterface.FindJoysticks();
                if (sticks != null && sticks.Length > 0)
                {
                    _joystickInterface.AcquireJoystick(sticks[0]);                    
                    _joystickTimer.Start();
                    lblJoystickStatus.BackColor = Color.Green;
                    btnConnectJoystick.Text = "Disconnect Joystick";
                    _joystickConnected = true;

                    // To immediately listen for key events
                    txtCommandList.Focus();
                }
            }
            else
            {
                _joystickTimer.Stop();
                _joystickInterface.ReleaseJoystick();
                lblJoystickStatus.BackColor = Color.Red;
                btnConnectJoystick.Text = "Connect Joystick";
                _joystickConnected = false;
            }
        }

        private void joystickTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Skip event if previous one is still executing
            if (Interlocked.CompareExchange(ref _joystickTimerSync, 1, 0) == 0)
            {
                // Joystick needs to be polled
                _joystickInterface.UpdateStatus();
                processJoystickState();
                _joystickTimerSync = 0;
            }
        }

        private void processJoystickState()
        {
            // Joystick button assignments
            // ***NUMBERS ARE AS PHYSICALLY LABELED ON JOYSTICK***
            const int DRIVING_ON_OFF = 9;
            const int ID_UP = 10;
            const int SPEED_UP = 5;
            const int SPEED_DOWN = 7;
            const int KICK = 1;
            const int START_CHARGING = 4;
            const int STOP_CHARGING = 2;
            const int ENABLE_BREAKBEAM = 3;
            const int START_DRIBBLER = 6;
            const int STOP_DRIBBLER = 8;            

            // Joystick specific parameters
            const int JOYSTICK_AXIS_MAXIMUM = 65535;
            // Enough to make the speed generated here match "speed" setting
            const double JOYSTICK_DIR_SPEED_SCALE = 2.7;
            const double JOYSTICK_ANG_SPEED_SCALE = 30;

            // Update current robot ID
            if (_joystickInterface.Buttons[ID_UP - 1])
                _curRobot = (_curRobot + 1) % NUM_ROBOTS;
            updateLabel(lblID, _curRobot.ToString());

            // Update speed
            if (_joystickInterface.Buttons[SPEED_UP - 1])
                _speed++;
            if (_joystickInterface.Buttons[SPEED_DOWN - 1])
                _speed--;
            updateLabel(lblSpeed, _speed.ToString());

            if (_joystickInterface.Buttons[KICK - 1])
                sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.KICK));
            if (_joystickInterface.Buttons[START_CHARGING - 1])
                sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.START_CHARGING));
            if (_joystickInterface.Buttons[STOP_CHARGING - 1])
                sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.STOP_CHARGING));
            if (_joystickInterface.Buttons[ENABLE_BREAKBEAM - 1])
                sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.BREAKBEAM_KICK));
            if (_joystickInterface.Buttons[START_DRIBBLER - 1])
                sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.START_DRIBBLER));
            if (_joystickInterface.Buttons[STOP_DRIBBLER - 1])
                sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.STOP_DRIBBLER));

            if (_joystickInterface.Buttons[DRIVING_ON_OFF - 1])
            {
                _joystickDrivingOn = !_joystickDrivingOn;
                updateLabel(lblJoystickDriving, _joystickDrivingOn ? Color.Green : Color.Red);
            }

            if (_joystickDrivingOn)
            {
                // compute parameters based on the current state of the joystick
                double forwardComponent = (1 - ((float)_joystickInterface.AxisD) / JOYSTICK_AXIS_MAXIMUM) - .5;
                double lateralComponent = ((float)_joystickInterface.AxisC) / JOYSTICK_AXIS_MAXIMUM - .5;
                double angularComponent = 1 - ((float)_joystickInterface.AxisA) / JOYSTICK_AXIS_MAXIMUM - .5;

                //Console.WriteLine("Forward component: " + forwardComponent + " lateral component " + lateralComponent +
                //                    "angular component" + angularComponent);

                // assume that we are facing in the y direction (90 degrees), so the lateral direction is
                // x, the forward component is y, and the angular is what it is

                //I assume the x command is effectively in m/s, so r the radius of the wheels from the center of
                //the robot is in meters

                //change from the x and y of the field to forward and lateral(right is positive) used below

                // TODO: why is theta randomly set here?
                double theta = Math.PI / 2;

                double forward = JOYSTICK_DIR_SPEED_SCALE * _speed * (-(Math.Cos(theta) * lateralComponent +
                                                                 Math.Sin(theta) * forwardComponent));
                double lateral = JOYSTICK_DIR_SPEED_SCALE * _speed * (-(-Math.Sin(theta) * lateralComponent +
                                                                 Math.Cos(theta) * forwardComponent));
                double angular = JOYSTICK_ANG_SPEED_SCALE * _speed * angularComponent;

                WheelSpeeds speeds = _robotModels[_curRobot].Convert(forward, lateral, angular);

                sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.MOVE, speeds));
            }
        }        

        private void btnStartStopWheelSpeedFunction_Click(object sender, EventArgs e)
        {
            if (!_wheelSpeedFunctionRunning)
            {
                _wheelSpeedFunctionRunning = true;

                listBoxCommandHistory.Items.Clear();
                listBoxCommandHistory.Items.Add("rf\tlf\tlb\trb");

                _lastSerialData = 0;

                _wheelSpeedFunctionThread = new Thread(WheelSpeedFunctionLoop);
                _wheelSpeedFunctionThread.Start();
                
                btnStartStopWheelSpeedFunction.Text = "Stop";
            }
            else
            {
                _wheelSpeedFunctionRunning = false;
                btnStartStopWheelSpeedFunction.Text = "Start";
                _wheelSpeedFunction.ClearState();
            }
        }

        private void WheelSpeedFunctionLoop(object state)
        {            
            int period = int.Parse(textBoxPeriod.Text);

            _wheelSpeedFunctionTimer.Start();

            while (_wheelSpeedFunctionRunning)
            {
                _wheelSpeedFunctionTimer.Stop();
                double t = _wheelSpeedFunctionTimer.Duration;
                updateLabel(label12, String.Format("{0:F2} s", t));

                WheelSpeeds speeds = _wheelSpeedFunction.Eval(t);
                sendCommand(new RobotCommand(_curRobot, RobotCommand.Command.MOVE, speeds));

                addItem(listBoxCommandHistory, speeds.rf + "\t" + speeds.lf + "\t" + 
                                               speeds.lb + "\t" + speeds.rb, 10);

                if (t > double.Parse(textBoxTestDuration.Text))
                    _wheelSpeedFunctionRunning = false;

                Thread.Sleep(period);
            }
        }

        private void cmbWheelSpeedFunction_SelectedIndexChanged(object sender, EventArgs e)
        {
            string function = cmbWheelSpeedFunction.SelectedItem.ToString();
            _wheelSpeedFunction = _wheelSpeedFunctions[function];
            wheelSpeedFunctionSettings.SelectedObject = _wheelSpeedFunction;
        }        

        private void btnDataListen_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_dataListening)
                {
                    string port = "COM" + udDataInCOMPort.Value.ToString();

                    if (_dataInWriter == null)
                    {
                        _dataInWriter = new StreamWriter(DATA_IN_FILE, false);
                        _dataInWriter.WriteLine("time, sent cmd, enc, duty, " +
                             "rcved cmd,  pkts rcved, pkts acc, pkts mism");
                    }

                    _serialInput.Open(port);
                    _serialInput.ValueReceived += serialDataReceived;
                    
                    _wheelSpeedFunctionTimer.Start();

                    lblDataInStatus.BackColor = Color.Green;
                    btnDataListen.Text = "Stop listening";
                    _dataListening = true;
                }
                else
                {
                    _serialInput.ValueReceived -= serialDataReceived; 
                    _serialInput.Close();
                    lblDataInStatus.BackColor = Color.Red;
                    btnDataListen.Text = "Listen";
                    _dataListening = false;
                }
            }
            catch (Exception except)
            {
                MessageBox.Show(except.Message + "\r\n" + except.StackTrace);
                return;
            }
        }

        private void serialDataReceived(SerialInput.SerialInputMessage[] values)
        {
            _wheelSpeedFunctionTimer.Stop();
            double t = _wheelSpeedFunctionTimer.Duration;

            int n = values.Length;

            
            for (int i = 0; i < n; i++)
            {
                double thist = (t - _lastSerialData) / n * (i + 1) + _lastSerialData;
                lock (_dataInWriter)
                {
                    _dataInWriter.WriteLine(thist + ", " + _wheelSpeedFunction.Eval(thist).ToString() + ", " +
                                           values[i].Encoder + ", " + values[i].Duty + ", " +
                                           values[i].WheelCommand + ", " + values[i].PktsReceived + ", " +
                                           values[i].PktsAccepted + ", " + values[i].PktsMismatched);

                    updateLabel(labelTime, String.Format("{0:F2} s", thist));
                    //addItem(listBoxInputHistory, values[i], 7);
                    
                }
            }

            String[] itemsArray = new string[values.Length+1];

            itemsArray[0] = SerialInput.SerialInputMessage.ToStringHeader();
            for(int i=1; i<=values.Length; i++)
                itemsArray[i] = values[i-1].ToString();

            showItemRange(listBoxInputHistory, itemsArray);
            _lastSerialData = t;
        }

        private void udDribblerPower_ValueChanged(object sender, EventArgs e)
        {
            RobotCommand.DribblerSpeed = (byte) udDribblerPower.Value;
        }

        private void guiTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Invoke(this.OnDataAcquired, new object[] { null });
        }

        private void OnDataAcquiredEvent(object sender)
        {
            double duration;
            lock (_receiveDurationLock)
            {
                duration = _receiveDuration;
            }

            lblDuration.Text = String.Format("{0} ms", duration);
        }
    }
}