namespace Robocup.SerialControl
{
    partial class RemoteControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.Label label7;
            System.Windows.Forms.Label label10;
            this.txtCommandList = new System.Windows.Forms.TextBox();
            this.btnConnectSending = new System.Windows.Forms.Button();
            this.lblSentCommand = new System.Windows.Forms.Label();
            this.lblID = new System.Windows.Forms.Label();
            this.textBoxRemoteHost = new System.Windows.Forms.TextBox();
            this.radioButtonSerial = new System.Windows.Forms.RadioButton();
            this.radioButtonRemote = new System.Windows.Forms.RadioButton();
            this.udCmdOutCOMPort = new System.Windows.Forms.NumericUpDown();
            this.lblSendStatus = new System.Windows.Forms.Label();
            this.chkRebootTimerEnabled = new System.Windows.Forms.CheckBox();
            this.txtRebootTimerInterval = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblListenStatus = new System.Windows.Forms.Label();
            this.btnCmdListen = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lblJoystickDriving = new System.Windows.Forms.Label();
            this.lblJoystickStatus = new System.Windows.Forms.Label();
            this.txtListenPort = new System.Windows.Forms.TextBox();
            this.btnConnectJoystick = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.udKickStrength = new System.Windows.Forms.NumericUpDown();
            this.udDribblerPower = new System.Windows.Forms.NumericUpDown();
            this.udBoardID = new System.Windows.Forms.NumericUpDown();
            this.chkCfgFeedback = new System.Windows.Forms.CheckBox();
            this.chkCfgSpewPktStats = new System.Windows.Forms.CheckBox();
            this.chkCfgSpewEncoder = new System.Windows.Forms.CheckBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.btnSetCfgFlags = new System.Windows.Forms.Button();
            this.btnDischarge = new System.Windows.Forms.Button();
            this.btnSetWheelSpeeds = new System.Windows.Forms.Button();
            this.label16 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.txtRB = new System.Windows.Forms.TextBox();
            this.txtLB = new System.Windows.Forms.TextBox();
            this.txtLF = new System.Windows.Forms.TextBox();
            this.txtRF = new System.Windows.Forms.TextBox();
            this.btnSendPacket = new System.Windows.Forms.Button();
            this.txtPacket = new System.Windows.Forms.TextBox();
            this.btnSetPID = new System.Windows.Forms.Button();
            this.txtD = new System.Windows.Forms.TextBox();
            this.txtI = new System.Windows.Forms.TextBox();
            this.txtP = new System.Windows.Forms.TextBox();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnStopDribbler = new System.Windows.Forms.Button();
            this.btnStartDribbler = new System.Windows.Forms.Button();
            this.btnBreakBeamKick = new System.Windows.Forms.Button();
            this.btnStopCharging = new System.Windows.Forms.Button();
            this.btnStartCharging = new System.Windows.Forms.Button();
            this.btnKick = new System.Windows.Forms.Button();
            this.wheelSpeedFunctionSettings = new System.Windows.Forms.PropertyGrid();
            this.textBoxTestDuration = new System.Windows.Forms.TextBox();
            this.textBoxPeriod = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.cmbWheelSpeedFunction = new System.Windows.Forms.ComboBox();
            this.btnStartStopWheelSpeedFunction = new System.Windows.Forms.Button();
            this.listBoxCommandHistory = new System.Windows.Forms.ListBox();
            this.label5 = new System.Windows.Forms.Label();
            this.labelTime = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.btnDataListen = new System.Windows.Forms.Button();
            this.lblDataInStatus = new System.Windows.Forms.Label();
            this.udDataInCOMPort = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.listBoxInputHistory = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lblSpeed = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.lblDuration = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            label10 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.udCmdOutCOMPort)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udKickStrength)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udDribblerPower)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udBoardID)).BeginInit();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udDataInCOMPort)).BeginInit();
            this.SuspendLayout();
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(71, 50);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(64, 13);
            label7.TabIndex = 60;
            label7.Text = "Duration (s):";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(6, 50);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(62, 13);
            label10.TabIndex = 56;
            label10.Text = "Period (ms):";
            // 
            // txtCommandList
            // 
            this.txtCommandList.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCommandList.Location = new System.Drawing.Point(12, 285);
            this.txtCommandList.Multiline = true;
            this.txtCommandList.Name = "txtCommandList";
            this.txtCommandList.ReadOnly = true;
            this.txtCommandList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtCommandList.Size = new System.Drawing.Size(324, 103);
            this.txtCommandList.TabIndex = 0;
            this.txtCommandList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RemoteControl_KeyDown);
            this.txtCommandList.KeyUp += new System.Windows.Forms.KeyEventHandler(this.RemoteControl_KeyUp);
            // 
            // btnConnectSending
            // 
            this.btnConnectSending.Location = new System.Drawing.Point(13, 121);
            this.btnConnectSending.Name = "btnConnectSending";
            this.btnConnectSending.Size = new System.Drawing.Size(137, 24);
            this.btnConnectSending.TabIndex = 1;
            this.btnConnectSending.TabStop = false;
            this.btnConnectSending.Text = "Connect";
            this.btnConnectSending.UseVisualStyleBackColor = true;
            this.btnConnectSending.Click += new System.EventHandler(this.btnConnectSending_Click);
            this.btnConnectSending.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RemoteControl_KeyDown);
            this.btnConnectSending.KeyUp += new System.Windows.Forms.KeyEventHandler(this.RemoteControl_KeyUp);
            // 
            // lblSentCommand
            // 
            this.lblSentCommand.AutoSize = true;
            this.lblSentCommand.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSentCommand.Location = new System.Drawing.Point(6, 237);
            this.lblSentCommand.Name = "lblSentCommand";
            this.lblSentCommand.Size = new System.Drawing.Size(138, 29);
            this.lblSentCommand.TabIndex = 3;
            this.lblSentCommand.Text = "<NO CMD>";
            // 
            // lblID
            // 
            this.lblID.AutoSize = true;
            this.lblID.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblID.Location = new System.Drawing.Point(120, 208);
            this.lblID.Name = "lblID";
            this.lblID.Size = new System.Drawing.Size(26, 29);
            this.lblID.TabIndex = 45;
            this.lblID.Text = "0";
            // 
            // textBoxRemoteHost
            // 
            this.textBoxRemoteHost.Location = new System.Drawing.Point(36, 33);
            this.textBoxRemoteHost.Name = "textBoxRemoteHost";
            this.textBoxRemoteHost.Size = new System.Drawing.Size(114, 20);
            this.textBoxRemoteHost.TabIndex = 46;
            // 
            // radioButtonSerial
            // 
            this.radioButtonSerial.AutoSize = true;
            this.radioButtonSerial.Location = new System.Drawing.Point(13, 57);
            this.radioButtonSerial.Name = "radioButtonSerial";
            this.radioButtonSerial.Size = new System.Drawing.Size(113, 17);
            this.radioButtonSerial.TabIndex = 49;
            this.radioButtonSerial.Text = "Serial Port (COM#)";
            this.radioButtonSerial.UseVisualStyleBackColor = true;
            this.radioButtonSerial.CheckedChanged += new System.EventHandler(this.radioButtonSerial_CheckedChanged);
            // 
            // radioButtonRemote
            // 
            this.radioButtonRemote.AutoSize = true;
            this.radioButtonRemote.Location = new System.Drawing.Point(13, 14);
            this.radioButtonRemote.Name = "radioButtonRemote";
            this.radioButtonRemote.Size = new System.Drawing.Size(112, 17);
            this.radioButtonRemote.TabIndex = 50;
            this.radioButtonRemote.Text = "Remote (host:port)";
            this.radioButtonRemote.UseVisualStyleBackColor = true;
            this.radioButtonRemote.CheckedChanged += new System.EventHandler(this.radioButtonRemote_CheckedChanged);
            // 
            // udCmdOutCOMPort
            // 
            this.udCmdOutCOMPort.Location = new System.Drawing.Point(36, 77);
            this.udCmdOutCOMPort.Name = "udCmdOutCOMPort";
            this.udCmdOutCOMPort.Size = new System.Drawing.Size(114, 20);
            this.udCmdOutCOMPort.TabIndex = 52;
            this.udCmdOutCOMPort.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // lblSendStatus
            // 
            this.lblSendStatus.BackColor = System.Drawing.Color.Red;
            this.lblSendStatus.Location = new System.Drawing.Point(14, 104);
            this.lblSendStatus.Name = "lblSendStatus";
            this.lblSendStatus.Size = new System.Drawing.Size(136, 14);
            this.lblSendStatus.TabIndex = 53;
            // 
            // chkRebootTimerEnabled
            // 
            this.chkRebootTimerEnabled.AutoSize = true;
            this.chkRebootTimerEnabled.Location = new System.Drawing.Point(6, 15);
            this.chkRebootTimerEnabled.Name = "chkRebootTimerEnabled";
            this.chkRebootTimerEnabled.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.chkRebootTimerEnabled.Size = new System.Drawing.Size(68, 17);
            this.chkRebootTimerEnabled.TabIndex = 56;
            this.chkRebootTimerEnabled.Text = "Enabled:";
            this.chkRebootTimerEnabled.UseVisualStyleBackColor = true;
            this.chkRebootTimerEnabled.CheckedChanged += new System.EventHandler(this.chkRebootTimerEnabled_CheckedChanged);
            // 
            // txtRebootTimerInterval
            // 
            this.txtRebootTimerInterval.Location = new System.Drawing.Point(165, 12);
            this.txtRebootTimerInterval.Name = "txtRebootTimerInterval";
            this.txtRebootTimerInterval.Size = new System.Drawing.Size(66, 20);
            this.txtRebootTimerInterval.TabIndex = 57;
            this.txtRebootTimerInterval.Text = "15000";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(88, 15);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(67, 13);
            this.label4.TabIndex = 58;
            this.label4.Text = "Interval (ms):";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtRebootTimerInterval);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.chkRebootTimerEnabled);
            this.groupBox1.Location = new System.Drawing.Point(12, 169);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(324, 36);
            this.groupBox1.TabIndex = 59;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Reboot timer";
            // 
            // lblListenStatus
            // 
            this.lblListenStatus.BackColor = System.Drawing.Color.Red;
            this.lblListenStatus.Location = new System.Drawing.Point(13, 53);
            this.lblListenStatus.Name = "lblListenStatus";
            this.lblListenStatus.Size = new System.Drawing.Size(137, 14);
            this.lblListenStatus.TabIndex = 61;
            // 
            // btnCmdListen
            // 
            this.btnCmdListen.Location = new System.Drawing.Point(13, 70);
            this.btnCmdListen.Name = "btnCmdListen";
            this.btnCmdListen.Size = new System.Drawing.Size(137, 23);
            this.btnCmdListen.TabIndex = 60;
            this.btnCmdListen.TabStop = false;
            this.btnCmdListen.Text = "Listen";
            this.btnCmdListen.UseVisualStyleBackColor = true;
            this.btnCmdListen.Click += new System.EventHandler(this.btnCmdListen_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lblJoystickDriving);
            this.groupBox2.Controls.Add(this.lblJoystickStatus);
            this.groupBox2.Controls.Add(this.txtListenPort);
            this.groupBox2.Controls.Add(this.btnConnectJoystick);
            this.groupBox2.Controls.Add(this.lblListenStatus);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.btnCmdListen);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(159, 151);
            this.groupBox2.TabIndex = 62;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Command Input";
            // 
            // lblJoystickDriving
            // 
            this.lblJoystickDriving.BackColor = System.Drawing.Color.Red;
            this.lblJoystickDriving.Location = new System.Drawing.Point(88, 104);
            this.lblJoystickDriving.Name = "lblJoystickDriving";
            this.lblJoystickDriving.Size = new System.Drawing.Size(61, 14);
            this.lblJoystickDriving.TabIndex = 62;
            // 
            // lblJoystickStatus
            // 
            this.lblJoystickStatus.BackColor = System.Drawing.Color.Red;
            this.lblJoystickStatus.Location = new System.Drawing.Point(13, 104);
            this.lblJoystickStatus.Name = "lblJoystickStatus";
            this.lblJoystickStatus.Size = new System.Drawing.Size(61, 14);
            this.lblJoystickStatus.TabIndex = 56;
            // 
            // txtListenPort
            // 
            this.txtListenPort.Location = new System.Drawing.Point(91, 23);
            this.txtListenPort.Name = "txtListenPort";
            this.txtListenPort.Size = new System.Drawing.Size(59, 20);
            this.txtListenPort.TabIndex = 1;
            this.txtListenPort.Text = "50100";
            // 
            // btnConnectJoystick
            // 
            this.btnConnectJoystick.Location = new System.Drawing.Point(13, 121);
            this.btnConnectJoystick.Name = "btnConnectJoystick";
            this.btnConnectJoystick.Size = new System.Drawing.Size(137, 24);
            this.btnConnectJoystick.TabIndex = 0;
            this.btnConnectJoystick.Text = "Connect Joystick";
            this.btnConnectJoystick.UseVisualStyleBackColor = true;
            this.btnConnectJoystick.Click += new System.EventHandler(this.btnConnectJoystick_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(11, 26);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(74, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "Listen on port:";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnConnectSending);
            this.groupBox3.Controls.Add(this.textBoxRemoteHost);
            this.groupBox3.Controls.Add(this.radioButtonSerial);
            this.groupBox3.Controls.Add(this.radioButtonRemote);
            this.groupBox3.Controls.Add(this.lblSendStatus);
            this.groupBox3.Controls.Add(this.udCmdOutCOMPort);
            this.groupBox3.Location = new System.Drawing.Point(177, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(159, 151);
            this.groupBox3.TabIndex = 63;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Command Output";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.udKickStrength);
            this.groupBox4.Controls.Add(this.udDribblerPower);
            this.groupBox4.Controls.Add(this.udBoardID);
            this.groupBox4.Controls.Add(this.chkCfgFeedback);
            this.groupBox4.Controls.Add(this.chkCfgSpewPktStats);
            this.groupBox4.Controls.Add(this.chkCfgSpewEncoder);
            this.groupBox4.Controls.Add(this.label17);
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.Controls.Add(this.btnSetCfgFlags);
            this.groupBox4.Controls.Add(this.btnDischarge);
            this.groupBox4.Controls.Add(this.btnSetWheelSpeeds);
            this.groupBox4.Controls.Add(this.label16);
            this.groupBox4.Controls.Add(this.label15);
            this.groupBox4.Controls.Add(this.label14);
            this.groupBox4.Controls.Add(this.label13);
            this.groupBox4.Controls.Add(this.txtRB);
            this.groupBox4.Controls.Add(this.txtLB);
            this.groupBox4.Controls.Add(this.txtLF);
            this.groupBox4.Controls.Add(this.txtRF);
            this.groupBox4.Controls.Add(this.btnSendPacket);
            this.groupBox4.Controls.Add(this.txtPacket);
            this.groupBox4.Controls.Add(this.btnSetPID);
            this.groupBox4.Controls.Add(this.txtD);
            this.groupBox4.Controls.Add(this.txtI);
            this.groupBox4.Controls.Add(this.txtP);
            this.groupBox4.Controls.Add(this.btnReset);
            this.groupBox4.Controls.Add(this.btnStopDribbler);
            this.groupBox4.Controls.Add(this.btnStartDribbler);
            this.groupBox4.Controls.Add(this.btnBreakBeamKick);
            this.groupBox4.Controls.Add(this.btnStopCharging);
            this.groupBox4.Controls.Add(this.btnStartCharging);
            this.groupBox4.Controls.Add(this.btnKick);
            this.groupBox4.Location = new System.Drawing.Point(342, 12);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(290, 374);
            this.groupBox4.TabIndex = 64;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Robot";
            // 
            // udKickStrength
            // 
            this.udKickStrength.Location = new System.Drawing.Point(30, 69);
            this.udKickStrength.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.udKickStrength.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udKickStrength.Name = "udKickStrength";
            this.udKickStrength.Size = new System.Drawing.Size(110, 20);
            this.udKickStrength.TabIndex = 63;
            this.udKickStrength.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // udDribblerPower
            // 
            this.udDribblerPower.Location = new System.Drawing.Point(166, 69);
            this.udDribblerPower.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.udDribblerPower.Name = "udDribblerPower";
            this.udDribblerPower.Size = new System.Drawing.Size(110, 20);
            this.udDribblerPower.TabIndex = 62;
            this.udDribblerPower.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.udDribblerPower.ValueChanged += new System.EventHandler(this.udDribblerPower_ValueChanged);
            // 
            // udBoardID
            // 
            this.udBoardID.Location = new System.Drawing.Point(160, 187);
            this.udBoardID.Name = "udBoardID";
            this.udBoardID.Size = new System.Drawing.Size(114, 20);
            this.udBoardID.TabIndex = 54;
            // 
            // chkCfgFeedback
            // 
            this.chkCfgFeedback.AutoSize = true;
            this.chkCfgFeedback.Location = new System.Drawing.Point(26, 213);
            this.chkCfgFeedback.Name = "chkCfgFeedback";
            this.chkCfgFeedback.Size = new System.Drawing.Size(74, 17);
            this.chkCfgFeedback.TabIndex = 61;
            this.chkCfgFeedback.Text = "Feedback";
            this.chkCfgFeedback.UseVisualStyleBackColor = true;
            // 
            // chkCfgSpewPktStats
            // 
            this.chkCfgSpewPktStats.AutoSize = true;
            this.chkCfgSpewPktStats.Location = new System.Drawing.Point(106, 213);
            this.chkCfgSpewPktStats.Name = "chkCfgSpewPktStats";
            this.chkCfgSpewPktStats.Size = new System.Drawing.Size(99, 17);
            this.chkCfgSpewPktStats.TabIndex = 60;
            this.chkCfgSpewPktStats.Text = "Spew Pkt Stats";
            this.chkCfgSpewPktStats.UseVisualStyleBackColor = true;
            // 
            // chkCfgSpewEncoder
            // 
            this.chkCfgSpewEncoder.AutoSize = true;
            this.chkCfgSpewEncoder.Location = new System.Drawing.Point(26, 186);
            this.chkCfgSpewEncoder.Name = "chkCfgSpewEncoder";
            this.chkCfgSpewEncoder.Size = new System.Drawing.Size(75, 17);
            this.chkCfgSpewEncoder.TabIndex = 59;
            this.chkCfgSpewEncoder.Text = "Spew Enc";
            this.chkCfgSpewEncoder.UseVisualStyleBackColor = true;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(12, 306);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(28, 13);
            this.label17.TabIndex = 58;
            this.label17.Text = "PID:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(157, 171);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(145, 13);
            this.label8.TabIndex = 56;
            this.label8.Text = "Brushless cfg flags, board ID:";
            // 
            // btnSetCfgFlags
            // 
            this.btnSetCfgFlags.Location = new System.Drawing.Point(17, 238);
            this.btnSetCfgFlags.Name = "btnSetCfgFlags";
            this.btnSetCfgFlags.Size = new System.Drawing.Size(261, 23);
            this.btnSetCfgFlags.TabIndex = 53;
            this.btnSetCfgFlags.Text = "Set Config Flags";
            this.btnSetCfgFlags.UseVisualStyleBackColor = true;
            this.btnSetCfgFlags.Click += new System.EventHandler(this.btnSetCfgFlags_Click);
            // 
            // btnDischarge
            // 
            this.btnDischarge.Location = new System.Drawing.Point(15, 117);
            this.btnDischarge.Name = "btnDischarge";
            this.btnDischarge.Size = new System.Drawing.Size(139, 23);
            this.btnDischarge.TabIndex = 52;
            this.btnDischarge.Text = "Discharge";
            this.btnDischarge.UseVisualStyleBackColor = true;
            this.btnDischarge.Click += new System.EventHandler(this.btnDischarge_Click);
            // 
            // btnSetWheelSpeeds
            // 
            this.btnSetWheelSpeeds.Location = new System.Drawing.Point(160, 278);
            this.btnSetWheelSpeeds.Name = "btnSetWheelSpeeds";
            this.btnSetWheelSpeeds.Size = new System.Drawing.Size(120, 23);
            this.btnSetWheelSpeeds.TabIndex = 51;
            this.btnSetWheelSpeeds.Text = "Set Wheel Speeds";
            this.btnSetWheelSpeeds.UseVisualStyleBackColor = true;
            this.btnSetWheelSpeeds.Click += new System.EventHandler(this.btnSetWheelSpeeds_Click);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(124, 264);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(16, 13);
            this.label16.TabIndex = 50;
            this.label16.Text = "rb";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(86, 264);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(15, 13);
            this.label15.TabIndex = 49;
            this.label15.Text = "lb";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(50, 264);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(12, 13);
            this.label14.TabIndex = 48;
            this.label14.Text = "lf";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(14, 264);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(13, 13);
            this.label13.TabIndex = 47;
            this.label13.Text = "rf";
            // 
            // txtRB
            // 
            this.txtRB.Location = new System.Drawing.Point(121, 280);
            this.txtRB.Name = "txtRB";
            this.txtRB.Size = new System.Drawing.Size(33, 20);
            this.txtRB.TabIndex = 46;
            this.txtRB.Text = "0";
            // 
            // txtLB
            // 
            this.txtLB.Location = new System.Drawing.Point(85, 280);
            this.txtLB.Name = "txtLB";
            this.txtLB.Size = new System.Drawing.Size(33, 20);
            this.txtLB.TabIndex = 45;
            this.txtLB.Text = "0";
            // 
            // txtLF
            // 
            this.txtLF.Location = new System.Drawing.Point(50, 280);
            this.txtLF.Name = "txtLF";
            this.txtLF.Size = new System.Drawing.Size(33, 20);
            this.txtLF.TabIndex = 44;
            this.txtLF.Text = "0";
            // 
            // txtRF
            // 
            this.txtRF.Location = new System.Drawing.Point(14, 280);
            this.txtRF.Name = "txtRF";
            this.txtRF.Size = new System.Drawing.Size(34, 20);
            this.txtRF.TabIndex = 43;
            this.txtRF.Text = "0";
            // 
            // btnSendPacket
            // 
            this.btnSendPacket.Enabled = false;
            this.btnSendPacket.Location = new System.Drawing.Point(160, 327);
            this.btnSendPacket.Name = "btnSendPacket";
            this.btnSendPacket.Size = new System.Drawing.Size(120, 23);
            this.btnSendPacket.TabIndex = 12;
            this.btnSendPacket.Text = "Send Packet";
            this.btnSendPacket.UseVisualStyleBackColor = true;
            this.btnSendPacket.Click += new System.EventHandler(this.btnSendPacket_Click);
            // 
            // txtPacket
            // 
            this.txtPacket.Enabled = false;
            this.txtPacket.Location = new System.Drawing.Point(14, 329);
            this.txtPacket.Name = "txtPacket";
            this.txtPacket.Size = new System.Drawing.Size(140, 20);
            this.txtPacket.TabIndex = 11;
            // 
            // btnSetPID
            // 
            this.btnSetPID.Location = new System.Drawing.Point(160, 302);
            this.btnSetPID.Name = "btnSetPID";
            this.btnSetPID.Size = new System.Drawing.Size(120, 23);
            this.btnSetPID.TabIndex = 10;
            this.btnSetPID.Text = "Set PID";
            this.btnSetPID.UseVisualStyleBackColor = true;
            this.btnSetPID.Click += new System.EventHandler(this.btnSetPID_Click);
            // 
            // txtD
            // 
            this.txtD.Location = new System.Drawing.Point(123, 303);
            this.txtD.Name = "txtD";
            this.txtD.Size = new System.Drawing.Size(31, 20);
            this.txtD.TabIndex = 9;
            // 
            // txtI
            // 
            this.txtI.Location = new System.Drawing.Point(83, 303);
            this.txtI.Name = "txtI";
            this.txtI.Size = new System.Drawing.Size(34, 20);
            this.txtI.TabIndex = 8;
            // 
            // txtP
            // 
            this.txtP.Location = new System.Drawing.Point(41, 303);
            this.txtP.Name = "txtP";
            this.txtP.Size = new System.Drawing.Size(37, 20);
            this.txtP.TabIndex = 7;
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(15, 157);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(118, 23);
            this.btnReset.TabIndex = 6;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnStopDribbler
            // 
            this.btnStopDribbler.Location = new System.Drawing.Point(160, 93);
            this.btnStopDribbler.Name = "btnStopDribbler";
            this.btnStopDribbler.Size = new System.Drawing.Size(120, 23);
            this.btnStopDribbler.TabIndex = 5;
            this.btnStopDribbler.Text = "Stop Dribbler";
            this.btnStopDribbler.UseVisualStyleBackColor = true;
            this.btnStopDribbler.Click += new System.EventHandler(this.btnStopDribbler_Click);
            // 
            // btnStartDribbler
            // 
            this.btnStartDribbler.Location = new System.Drawing.Point(160, 45);
            this.btnStartDribbler.Name = "btnStartDribbler";
            this.btnStartDribbler.Size = new System.Drawing.Size(120, 23);
            this.btnStartDribbler.TabIndex = 4;
            this.btnStartDribbler.Text = "Start Dribbler";
            this.btnStartDribbler.UseVisualStyleBackColor = true;
            this.btnStartDribbler.Click += new System.EventHandler(this.btnStartDribbler_Click);
            // 
            // btnBreakBeamKick
            // 
            this.btnBreakBeamKick.Location = new System.Drawing.Point(160, 21);
            this.btnBreakBeamKick.Name = "btnBreakBeamKick";
            this.btnBreakBeamKick.Size = new System.Drawing.Size(120, 23);
            this.btnBreakBeamKick.TabIndex = 3;
            this.btnBreakBeamKick.Text = "Min Break Beam Kick";
            this.btnBreakBeamKick.UseVisualStyleBackColor = true;
            this.btnBreakBeamKick.Click += new System.EventHandler(this.btnBreakBeamKick_Click);
            // 
            // btnStopCharging
            // 
            this.btnStopCharging.Location = new System.Drawing.Point(15, 93);
            this.btnStopCharging.Name = "btnStopCharging";
            this.btnStopCharging.Size = new System.Drawing.Size(139, 23);
            this.btnStopCharging.TabIndex = 2;
            this.btnStopCharging.Text = "Stop Charging";
            this.btnStopCharging.UseVisualStyleBackColor = true;
            this.btnStopCharging.Click += new System.EventHandler(this.btnStopCharging_Click);
            // 
            // btnStartCharging
            // 
            this.btnStartCharging.Location = new System.Drawing.Point(15, 45);
            this.btnStartCharging.Name = "btnStartCharging";
            this.btnStartCharging.Size = new System.Drawing.Size(140, 23);
            this.btnStartCharging.TabIndex = 1;
            this.btnStartCharging.Text = "Start Charging";
            this.btnStartCharging.UseVisualStyleBackColor = true;
            this.btnStartCharging.Click += new System.EventHandler(this.btnStartCharging_Click);
            // 
            // btnKick
            // 
            this.btnKick.Location = new System.Drawing.Point(15, 21);
            this.btnKick.Name = "btnKick";
            this.btnKick.Size = new System.Drawing.Size(139, 23);
            this.btnKick.TabIndex = 0;
            this.btnKick.Text = "Kick";
            this.btnKick.UseVisualStyleBackColor = true;
            this.btnKick.Click += new System.EventHandler(this.btnKick_Click);
            // 
            // wheelSpeedFunctionSettings
            // 
            this.wheelSpeedFunctionSettings.HelpVisible = false;
            this.wheelSpeedFunctionSettings.Location = new System.Drawing.Point(141, 16);
            this.wheelSpeedFunctionSettings.Name = "wheelSpeedFunctionSettings";
            this.wheelSpeedFunctionSettings.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.wheelSpeedFunctionSettings.Size = new System.Drawing.Size(183, 117);
            this.wheelSpeedFunctionSettings.TabIndex = 63;
            this.wheelSpeedFunctionSettings.ToolbarVisible = false;
            // 
            // textBoxTestDuration
            // 
            this.textBoxTestDuration.Location = new System.Drawing.Point(74, 66);
            this.textBoxTestDuration.Name = "textBoxTestDuration";
            this.textBoxTestDuration.Size = new System.Drawing.Size(61, 20);
            this.textBoxTestDuration.TabIndex = 61;
            this.textBoxTestDuration.Text = "1000.0";
            // 
            // textBoxPeriod
            // 
            this.textBoxPeriod.Location = new System.Drawing.Point(9, 66);
            this.textBoxPeriod.Name = "textBoxPeriod";
            this.textBoxPeriod.Size = new System.Drawing.Size(53, 20);
            this.textBoxPeriod.TabIndex = 57;
            this.textBoxPeriod.Text = "25";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(11, 93);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(51, 13);
            this.label12.TabIndex = 55;
            this.label12.Text = "Elapsed: ";
            // 
            // cmbWheelSpeedFunction
            // 
            this.cmbWheelSpeedFunction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbWheelSpeedFunction.FormattingEnabled = true;
            this.cmbWheelSpeedFunction.Location = new System.Drawing.Point(9, 19);
            this.cmbWheelSpeedFunction.Name = "cmbWheelSpeedFunction";
            this.cmbWheelSpeedFunction.Size = new System.Drawing.Size(126, 21);
            this.cmbWheelSpeedFunction.TabIndex = 53;
            this.cmbWheelSpeedFunction.SelectedIndexChanged += new System.EventHandler(this.cmbWheelSpeedFunction_SelectedIndexChanged);
            // 
            // btnStartStopWheelSpeedFunction
            // 
            this.btnStartStopWheelSpeedFunction.Location = new System.Drawing.Point(9, 110);
            this.btnStartStopWheelSpeedFunction.Name = "btnStartStopWheelSpeedFunction";
            this.btnStartStopWheelSpeedFunction.Size = new System.Drawing.Size(126, 23);
            this.btnStartStopWheelSpeedFunction.TabIndex = 52;
            this.btnStartStopWheelSpeedFunction.Text = "Start";
            this.btnStartStopWheelSpeedFunction.UseVisualStyleBackColor = true;
            this.btnStartStopWheelSpeedFunction.Click += new System.EventHandler(this.btnStartStopWheelSpeedFunction_Click);
            // 
            // listBoxCommandHistory
            // 
            this.listBoxCommandHistory.FormattingEnabled = true;
            this.listBoxCommandHistory.Location = new System.Drawing.Point(9, 159);
            this.listBoxCommandHistory.Name = "listBoxCommandHistory";
            this.listBoxCommandHistory.Size = new System.Drawing.Size(268, 134);
            this.listBoxCommandHistory.TabIndex = 62;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(7, 208);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(107, 29);
            this.label5.TabIndex = 66;
            this.label5.Text = "RobotID:";
            // 
            // labelTime
            // 
            this.labelTime.AutoSize = true;
            this.labelTime.Location = new System.Drawing.Point(68, 93);
            this.labelTime.Name = "labelTime";
            this.labelTime.Size = new System.Drawing.Size(21, 13);
            this.labelTime.TabIndex = 64;
            this.labelTime.Text = "0 s";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.label9);
            this.groupBox5.Controls.Add(this.btnDataListen);
            this.groupBox5.Controls.Add(this.lblDataInStatus);
            this.groupBox5.Controls.Add(this.udDataInCOMPort);
            this.groupBox5.Controls.Add(this.label2);
            this.groupBox5.Controls.Add(this.label1);
            this.groupBox5.Controls.Add(this.listBoxInputHistory);
            this.groupBox5.Controls.Add(this.listBoxCommandHistory);
            this.groupBox5.Controls.Add(this.wheelSpeedFunctionSettings);
            this.groupBox5.Controls.Add(this.labelTime);
            this.groupBox5.Controls.Add(this.btnStartStopWheelSpeedFunction);
            this.groupBox5.Controls.Add(this.cmbWheelSpeedFunction);
            this.groupBox5.Controls.Add(this.textBoxTestDuration);
            this.groupBox5.Controls.Add(this.label12);
            this.groupBox5.Controls.Add(label7);
            this.groupBox5.Controls.Add(label10);
            this.groupBox5.Controls.Add(this.textBoxPeriod);
            this.groupBox5.Location = new System.Drawing.Point(12, 392);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(620, 299);
            this.groupBox5.TabIndex = 67;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Wheel Speed Function";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(327, 16);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(99, 13);
            this.label9.TabIndex = 71;
            this.label9.Text = "Data input (COM#):";
            // 
            // btnDataListen
            // 
            this.btnDataListen.Location = new System.Drawing.Point(328, 82);
            this.btnDataListen.Name = "btnDataListen";
            this.btnDataListen.Size = new System.Drawing.Size(137, 24);
            this.btnDataListen.TabIndex = 69;
            this.btnDataListen.TabStop = false;
            this.btnDataListen.Text = "Listen";
            this.btnDataListen.UseVisualStyleBackColor = true;
            this.btnDataListen.Click += new System.EventHandler(this.btnDataListen_Click);
            // 
            // lblDataInStatus
            // 
            this.lblDataInStatus.BackColor = System.Drawing.Color.Red;
            this.lblDataInStatus.Location = new System.Drawing.Point(327, 66);
            this.lblDataInStatus.Name = "lblDataInStatus";
            this.lblDataInStatus.Size = new System.Drawing.Size(136, 14);
            this.lblDataInStatus.TabIndex = 70;
            // 
            // udDataInCOMPort
            // 
            this.udDataInCOMPort.Location = new System.Drawing.Point(330, 32);
            this.udDataInCOMPort.Name = "udDataInCOMPort";
            this.udDataInCOMPort.Size = new System.Drawing.Size(133, 20);
            this.udDataInCOMPort.TabIndex = 54;
            this.udDataInCOMPort.Value = new decimal(new int[] {
            9,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(280, 143);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 13);
            this.label2.TabIndex = 68;
            this.label2.Text = "Data in:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 143);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 67;
            this.label1.Text = "Data out:";
            // 
            // listBoxInputHistory
            // 
            this.listBoxInputHistory.FormattingEnabled = true;
            this.listBoxInputHistory.Location = new System.Drawing.Point(283, 159);
            this.listBoxInputHistory.Name = "listBoxInputHistory";
            this.listBoxInputHistory.Size = new System.Drawing.Size(328, 134);
            this.listBoxInputHistory.TabIndex = 66;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(162, 208);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(91, 29);
            this.label3.TabIndex = 69;
            this.label3.Text = "Speed:";
            // 
            // lblSpeed
            // 
            this.lblSpeed.AutoSize = true;
            this.lblSpeed.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpeed.Location = new System.Drawing.Point(259, 208);
            this.lblSpeed.Name = "lblSpeed";
            this.lblSpeed.Size = new System.Drawing.Size(26, 29);
            this.lblSpeed.TabIndex = 68;
            this.lblSpeed.Text = "0";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(15, 266);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(91, 13);
            this.label11.TabIndex = 63;
            this.label11.Text = "Receive duration:";
            // 
            // lblDuration
            // 
            this.lblDuration.AutoSize = true;
            this.lblDuration.Location = new System.Drawing.Point(112, 266);
            this.lblDuration.Name = "lblDuration";
            this.lblDuration.Size = new System.Drawing.Size(29, 13);
            this.lblDuration.TabIndex = 63;
            this.lblDuration.Text = "0 ms";
            // 
            // RemoteControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(646, 703);
            this.Controls.Add(this.lblDuration);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblSpeed);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lblID);
            this.Controls.Add(this.lblSentCommand);
            this.Controls.Add(this.txtCommandList);
            this.Name = "RemoteControl";
            this.Text = "Remote Control";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RemoteControl_FormClosing);
            this.Load += new System.EventHandler(this.RemoteControl_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RemoteControl_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.RemoteControl_KeyUp);
            ((System.ComponentModel.ISupportInitialize)(this.udCmdOutCOMPort)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udKickStrength)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udDribblerPower)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udBoardID)).EndInit();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udDataInCOMPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtCommandList;
        private System.Windows.Forms.Button btnConnectSending;
        public System.Windows.Forms.Label lblSentCommand;
        public System.Windows.Forms.Label lblID;
        private System.Windows.Forms.TextBox textBoxRemoteHost;
        private System.Windows.Forms.RadioButton radioButtonSerial;
        private System.Windows.Forms.RadioButton radioButtonRemote;
        private System.Windows.Forms.NumericUpDown udCmdOutCOMPort;
        private System.Windows.Forms.Label lblSendStatus;
        private System.Windows.Forms.CheckBox chkRebootTimerEnabled;
        private System.Windows.Forms.TextBox txtRebootTimerInterval;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblListenStatus;
        private System.Windows.Forms.Button btnCmdListen;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtListenPort;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button btnKick;
        private System.Windows.Forms.TextBox txtD;
        private System.Windows.Forms.TextBox txtI;
        private System.Windows.Forms.TextBox txtP;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Button btnStopDribbler;
        private System.Windows.Forms.Button btnStartDribbler;
        private System.Windows.Forms.Button btnBreakBeamKick;
        private System.Windows.Forms.Button btnStopCharging;
        private System.Windows.Forms.Button btnStartCharging;
        private System.Windows.Forms.Button btnSetPID;
        private System.Windows.Forms.Button btnSendPacket;
        private System.Windows.Forms.TextBox txtPacket;
        private System.Windows.Forms.Label lblJoystickStatus;
        private System.Windows.Forms.Button btnConnectJoystick;
        public System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnSetWheelSpeeds;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtRB;
        private System.Windows.Forms.TextBox txtLB;
        private System.Windows.Forms.TextBox txtLF;
        private System.Windows.Forms.TextBox txtRF;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ComboBox cmbWheelSpeedFunction;
        private System.Windows.Forms.Button btnStartStopWheelSpeedFunction;
        private System.Windows.Forms.TextBox textBoxTestDuration;
        private System.Windows.Forms.TextBox textBoxPeriod;
        private System.Windows.Forms.ListBox listBoxCommandHistory;
        private System.Windows.Forms.PropertyGrid wheelSpeedFunctionSettings;
        private System.Windows.Forms.Label labelTime;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.ListBox listBoxInputHistory;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.Label label3;
        public System.Windows.Forms.Label lblSpeed;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button btnDataListen;
        private System.Windows.Forms.Label lblDataInStatus;
        private System.Windows.Forms.NumericUpDown udDataInCOMPort;
        private System.Windows.Forms.Button btnDischarge;
        private System.Windows.Forms.Button btnSetCfgFlags;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label lblJoystickDriving;
        private System.Windows.Forms.CheckBox chkCfgSpewPktStats;
        private System.Windows.Forms.CheckBox chkCfgSpewEncoder;
        private System.Windows.Forms.CheckBox chkCfgFeedback;
        private System.Windows.Forms.NumericUpDown udBoardID;
        private System.Windows.Forms.NumericUpDown udDribblerPower;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label lblDuration;
        private System.Windows.Forms.NumericUpDown udKickStrength;
    }
}

