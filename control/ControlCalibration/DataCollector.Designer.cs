namespace Robocup.MotionControl
{
    partial class DataCollector
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
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label4;
            System.Windows.Forms.Label labelConnectToLabel;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label5;
            System.Windows.Forms.Label label6;
            System.Windows.Forms.Label label7;
            System.Windows.Forms.Label label8;
            System.Windows.Forms.Label label9;
            System.Windows.Forms.Label label10;
            this.buttonStart = new System.Windows.Forms.Button();
            this.comboBoxFunctionList = new System.Windows.Forms.ComboBox();
            this.textBoxRobotID = new System.Windows.Forms.TextBox();
            this.textBoxPeriod = new System.Windows.Forms.TextBox();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.buttonConnectVision = new System.Windows.Forms.Button();
            this.buttonListenSerial = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.labelTime = new System.Windows.Forms.Label();
            this.labelConnectedTo = new System.Windows.Forms.Label();
            this.checkBoxDoSend = new System.Windows.Forms.CheckBox();
            this.textBoxTestDuration = new System.Windows.Forms.TextBox();
            this.listBoxCommandHistory = new System.Windows.Forms.ListBox();
            this.textBoxVisionHostname = new System.Windows.Forms.TextBox();
            this.textBoxSerialPort = new System.Windows.Forms.TextBox();
            this.textBoxconstP = new System.Windows.Forms.TextBox();
            this.textBoxconstI = new System.Windows.Forms.TextBox();
            this.textBoxconstD = new System.Windows.Forms.TextBox();
            this.buttonSendPIDConstants = new System.Windows.Forms.Button();
            this.textBoxSerialCommand = new System.Windows.Forms.TextBox();
            this.buttonSendCustomSerial = new System.Windows.Forms.Button();
            this.listBoxInputHistory = new System.Windows.Forms.ListBox();
            this.checkBoxLimitCommands = new System.Windows.Forms.CheckBox();
            this.txtRF = new System.Windows.Forms.TextBox();
            this.txtLF = new System.Windows.Forms.TextBox();
            this.txtVisionPort = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.txtLB = new System.Windows.Forms.TextBox();
            this.txtRB = new System.Windows.Forms.TextBox();
            this.txtID = new System.Windows.Forms.TextBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            labelConnectToLabel = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            label8 = new System.Windows.Forms.Label();
            label9 = new System.Windows.Forms.Label();
            label10 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(351, 10);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(53, 13);
            label1.TabIndex = 3;
            label1.Text = "Robot ID:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(351, 49);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(107, 13);
            label2.TabIndex = 4;
            label2.Text = "Command frequency:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(457, 68);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(26, 13);
            label4.TabIndex = 7;
            label4.Text = "(ms)";
            // 
            // labelConnectToLabel
            // 
            labelConnectToLabel.AutoSize = true;
            labelConnectToLabel.Location = new System.Drawing.Point(41, 78);
            labelConnectToLabel.Name = "labelConnectToLabel";
            labelConnectToLabel.Size = new System.Drawing.Size(74, 13);
            labelConnectToLabel.TabIndex = 13;
            labelConnectToLabel.Text = "Connected to:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(348, 88);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(74, 13);
            label3.TabIndex = 16;
            label3.Text = "Test Duration:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(457, 107);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(18, 13);
            label5.TabIndex = 7;
            label5.Text = "(s)";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(149, 9);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(61, 13);
            label6.TabIndex = 20;
            label6.Text = "Vision host:";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(151, 57);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(57, 13);
            label7.TabIndex = 20;
            label7.Text = "Serial port:";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(504, 23);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(14, 13);
            label8.TabIndex = 22;
            label8.Text = "P";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(504, 49);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(10, 13);
            label9.TabIndex = 22;
            label9.Text = "I";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(504, 75);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(15, 13);
            label10.TabIndex = 22;
            label10.Text = "D";
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(473, 212);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(75, 23);
            this.buttonStart.TabIndex = 0;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // comboBoxFunctionList
            // 
            this.comboBoxFunctionList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFunctionList.FormattingEnabled = true;
            this.comboBoxFunctionList.Location = new System.Drawing.Point(473, 185);
            this.comboBoxFunctionList.Name = "comboBoxFunctionList";
            this.comboBoxFunctionList.Size = new System.Drawing.Size(121, 21);
            this.comboBoxFunctionList.TabIndex = 1;
            this.comboBoxFunctionList.SelectedIndexChanged += new System.EventHandler(this.comboBoxFunctionList_SelectedIndexChanged);
            // 
            // textBoxRobotID
            // 
            this.textBoxRobotID.Location = new System.Drawing.Point(351, 26);
            this.textBoxRobotID.Name = "textBoxRobotID";
            this.textBoxRobotID.Size = new System.Drawing.Size(100, 20);
            this.textBoxRobotID.TabIndex = 2;
            this.textBoxRobotID.Text = "1";
            // 
            // textBoxPeriod
            // 
            this.textBoxPeriod.Location = new System.Drawing.Point(351, 65);
            this.textBoxPeriod.Name = "textBoxPeriod";
            this.textBoxPeriod.Size = new System.Drawing.Size(100, 20);
            this.textBoxPeriod.TabIndex = 6;
            this.textBoxPeriod.Text = "25";
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Right;
            this.propertyGrid1.Location = new System.Drawing.Point(623, 0);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(159, 434);
            this.propertyGrid1.TabIndex = 8;
            // 
            // buttonConnectVision
            // 
            this.buttonConnectVision.Location = new System.Drawing.Point(44, 9);
            this.buttonConnectVision.Name = "buttonConnectVision";
            this.buttonConnectVision.Size = new System.Drawing.Size(101, 39);
            this.buttonConnectVision.TabIndex = 9;
            this.buttonConnectVision.Text = "Connect to vision";
            this.buttonConnectVision.UseVisualStyleBackColor = true;
            this.buttonConnectVision.Click += new System.EventHandler(this.buttonConnectVision_Click);
            // 
            // buttonListenSerial
            // 
            this.buttonListenSerial.Location = new System.Drawing.Point(44, 52);
            this.buttonListenSerial.Name = "buttonListenSerial";
            this.buttonListenSerial.Size = new System.Drawing.Size(101, 23);
            this.buttonListenSerial.TabIndex = 10;
            this.buttonListenSerial.Text = "Listen to serial";
            this.buttonListenSerial.UseVisualStyleBackColor = true;
            this.buttonListenSerial.Click += new System.EventHandler(this.buttonListenSerial_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.Location = new System.Drawing.Point(473, 241);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(75, 23);
            this.buttonStop.TabIndex = 11;
            this.buttonStop.Text = "Stop";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // labelTime
            // 
            this.labelTime.AutoSize = true;
            this.labelTime.Location = new System.Drawing.Point(473, 267);
            this.labelTime.Name = "labelTime";
            this.labelTime.Size = new System.Drawing.Size(22, 13);
            this.labelTime.TabIndex = 12;
            this.labelTime.Text = "t = ";
            // 
            // labelConnectedTo
            // 
            this.labelConnectedTo.AutoSize = true;
            this.labelConnectedTo.Location = new System.Drawing.Point(121, 78);
            this.labelConnectedTo.Name = "labelConnectedTo";
            this.labelConnectedTo.Size = new System.Drawing.Size(37, 13);
            this.labelConnectedTo.TabIndex = 14;
            this.labelConnectedTo.Text = "(none)";
            // 
            // checkBoxDoSend
            // 
            this.checkBoxDoSend.AutoSize = true;
            this.checkBoxDoSend.Location = new System.Drawing.Point(44, 94);
            this.checkBoxDoSend.Name = "checkBoxDoSend";
            this.checkBoxDoSend.Size = new System.Drawing.Size(105, 17);
            this.checkBoxDoSend.TabIndex = 15;
            this.checkBoxDoSend.Text = "Send commands";
            this.checkBoxDoSend.UseVisualStyleBackColor = true;
            this.checkBoxDoSend.CheckedChanged += new System.EventHandler(this.checkBoxDoSend_CheckedChanged);
            // 
            // textBoxTestDuration
            // 
            this.textBoxTestDuration.Location = new System.Drawing.Point(351, 104);
            this.textBoxTestDuration.Name = "textBoxTestDuration";
            this.textBoxTestDuration.Size = new System.Drawing.Size(100, 20);
            this.textBoxTestDuration.TabIndex = 17;
            this.textBoxTestDuration.Text = "1000.0";
            // 
            // listBoxCommandHistory
            // 
            this.listBoxCommandHistory.FormattingEnabled = true;
            this.listBoxCommandHistory.Location = new System.Drawing.Point(292, 161);
            this.listBoxCommandHistory.Name = "listBoxCommandHistory";
            this.listBoxCommandHistory.Size = new System.Drawing.Size(175, 238);
            this.listBoxCommandHistory.TabIndex = 18;
            // 
            // textBoxVisionHostname
            // 
            this.textBoxVisionHostname.Location = new System.Drawing.Point(216, 7);
            this.textBoxVisionHostname.Name = "textBoxVisionHostname";
            this.textBoxVisionHostname.Size = new System.Drawing.Size(100, 20);
            this.textBoxVisionHostname.TabIndex = 19;
            this.textBoxVisionHostname.Text = "localhost";
            // 
            // textBoxSerialPort
            // 
            this.textBoxSerialPort.Location = new System.Drawing.Point(230, 54);
            this.textBoxSerialPort.Name = "textBoxSerialPort";
            this.textBoxSerialPort.Size = new System.Drawing.Size(100, 20);
            this.textBoxSerialPort.TabIndex = 21;
            this.textBoxSerialPort.Text = "COM5";
            // 
            // textBoxconstP
            // 
            this.textBoxconstP.Location = new System.Drawing.Point(526, 20);
            this.textBoxconstP.Name = "textBoxconstP";
            this.textBoxconstP.Size = new System.Drawing.Size(72, 20);
            this.textBoxconstP.TabIndex = 23;
            this.textBoxconstP.Text = "8";
            // 
            // textBoxconstI
            // 
            this.textBoxconstI.Location = new System.Drawing.Point(526, 46);
            this.textBoxconstI.Name = "textBoxconstI";
            this.textBoxconstI.Size = new System.Drawing.Size(72, 20);
            this.textBoxconstI.TabIndex = 24;
            this.textBoxconstI.Text = "0";
            // 
            // textBoxconstD
            // 
            this.textBoxconstD.Location = new System.Drawing.Point(526, 72);
            this.textBoxconstD.Name = "textBoxconstD";
            this.textBoxconstD.Size = new System.Drawing.Size(72, 20);
            this.textBoxconstD.TabIndex = 25;
            this.textBoxconstD.Text = "0";
            // 
            // buttonSendPIDConstants
            // 
            this.buttonSendPIDConstants.Location = new System.Drawing.Point(507, 98);
            this.buttonSendPIDConstants.Name = "buttonSendPIDConstants";
            this.buttonSendPIDConstants.Size = new System.Drawing.Size(91, 23);
            this.buttonSendPIDConstants.TabIndex = 26;
            this.buttonSendPIDConstants.Text = "Send Constants";
            this.buttonSendPIDConstants.UseVisualStyleBackColor = true;
            this.buttonSendPIDConstants.Click += new System.EventHandler(this.buttonSendPIDConstants_Click);
            // 
            // textBoxSerialCommand
            // 
            this.textBoxSerialCommand.Enabled = false;
            this.textBoxSerialCommand.Location = new System.Drawing.Point(477, 373);
            this.textBoxSerialCommand.Name = "textBoxSerialCommand";
            this.textBoxSerialCommand.Size = new System.Drawing.Size(100, 20);
            this.textBoxSerialCommand.TabIndex = 27;
            this.textBoxSerialCommand.Text = "\\H11e1\\E";
            // 
            // buttonSendCustomSerial
            // 
            this.buttonSendCustomSerial.Location = new System.Drawing.Point(477, 399);
            this.buttonSendCustomSerial.Name = "buttonSendCustomSerial";
            this.buttonSendCustomSerial.Size = new System.Drawing.Size(95, 23);
            this.buttonSendCustomSerial.TabIndex = 28;
            this.buttonSendCustomSerial.Text = "Send Command";
            this.buttonSendCustomSerial.UseVisualStyleBackColor = true;
            this.buttonSendCustomSerial.Click += new System.EventHandler(this.buttonSendCustomSerial_Click);
            // 
            // listBoxInputHistory
            // 
            this.listBoxInputHistory.FormattingEnabled = true;
            this.listBoxInputHistory.Location = new System.Drawing.Point(12, 161);
            this.listBoxInputHistory.Name = "listBoxInputHistory";
            this.listBoxInputHistory.Size = new System.Drawing.Size(274, 238);
            this.listBoxInputHistory.TabIndex = 29;
            // 
            // checkBoxLimitCommands
            // 
            this.checkBoxLimitCommands.AutoSize = true;
            this.checkBoxLimitCommands.Location = new System.Drawing.Point(476, 143);
            this.checkBoxLimitCommands.Name = "checkBoxLimitCommands";
            this.checkBoxLimitCommands.Size = new System.Drawing.Size(142, 17);
            this.checkBoxLimitCommands.TabIndex = 30;
            this.checkBoxLimitCommands.Text = "Limit Command Changes";
            this.checkBoxLimitCommands.UseVisualStyleBackColor = true;
            this.checkBoxLimitCommands.CheckedChanged += new System.EventHandler(this.checkBoxLimitCommands_CheckedChanged);
            // 
            // txtRF
            // 
            this.txtRF.Location = new System.Drawing.Point(477, 344);
            this.txtRF.Name = "txtRF";
            this.txtRF.Size = new System.Drawing.Size(34, 20);
            this.txtRF.TabIndex = 31;
            this.txtRF.Text = "0";
            // 
            // txtLF
            // 
            this.txtLF.Location = new System.Drawing.Point(513, 344);
            this.txtLF.Name = "txtLF";
            this.txtLF.Size = new System.Drawing.Size(33, 20);
            this.txtLF.TabIndex = 32;
            this.txtLF.Text = "0";
            // 
            // txtLB
            // txtVisionPort
            // 
            this.txtVisionPort.Location = new System.Drawing.Point(216, 28);
            this.txtVisionPort.Name = "txtVisionPort";
            this.txtVisionPort.Size = new System.Drawing.Size(100, 20);
            this.txtVisionPort.TabIndex = 31;
            this.txtVisionPort.Text = "50000";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(151, 31);
            this.label17.Name = "label11";
            this.label17.Size = new System.Drawing.Size(59, 13);
            this.label17.TabIndex = 32;
            this.label17.Text = "Vision port:";
            // 
            // 
            this.txtLB.Location = new System.Drawing.Point(548, 344);
            this.txtLB.Name = "txtLB";
            this.txtLB.Size = new System.Drawing.Size(33, 20);
            this.txtLB.TabIndex = 33;
            this.txtLB.Text = "0";
            // 
            // txtRB
            // 
            this.txtRB.Location = new System.Drawing.Point(584, 344);
            this.txtRB.Name = "txtRB";
            this.txtRB.Size = new System.Drawing.Size(33, 20);
            this.txtRB.TabIndex = 34;
            this.txtRB.Text = "0";
            // 
            // txtID
            // 
            this.txtID.Location = new System.Drawing.Point(477, 305);
            this.txtID.Name = "txtID";
            this.txtID.Size = new System.Drawing.Size(34, 20);
            this.txtID.TabIndex = 35;
            this.txtID.Text = "0";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(513, 305);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(33, 20);
            this.txtPort.TabIndex = 36;
            this.txtPort.Text = "w";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(474, 291);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(15, 13);
            this.label11.TabIndex = 37;
            this.label11.Text = "id";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(510, 291);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(25, 13);
            this.label12.TabIndex = 38;
            this.label12.Text = "port";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(477, 328);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(13, 13);
            this.label13.TabIndex = 39;
            this.label13.Text = "rf";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(513, 328);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(12, 13);
            this.label14.TabIndex = 40;
            this.label14.Text = "lf";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(549, 328);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(15, 13);
            this.label15.TabIndex = 41;
            this.label15.Text = "lb";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(587, 328);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(16, 13);
            this.label16.TabIndex = 42;
            this.label16.Text = "rb";
            // 
            // DataCollector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(782, 434);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.txtVisionPort);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.txtID);
            this.Controls.Add(this.txtRB);
            this.Controls.Add(this.txtLB);
            this.Controls.Add(this.txtLF);
            this.Controls.Add(this.txtRF);
            this.Controls.Add(this.checkBoxLimitCommands);
            this.Controls.Add(this.listBoxInputHistory);
            this.Controls.Add(this.buttonSendCustomSerial);
            this.Controls.Add(this.textBoxSerialCommand);
            this.Controls.Add(this.buttonSendPIDConstants);
            this.Controls.Add(this.textBoxconstD);
            this.Controls.Add(this.textBoxconstI);
            this.Controls.Add(this.textBoxconstP);
            this.Controls.Add(label10);
            this.Controls.Add(label9);
            this.Controls.Add(label8);
            this.Controls.Add(this.textBoxSerialPort);
            this.Controls.Add(label7);
            this.Controls.Add(label6);
            this.Controls.Add(this.textBoxVisionHostname);
            this.Controls.Add(this.listBoxCommandHistory);
            this.Controls.Add(this.textBoxTestDuration);
            this.Controls.Add(label3);
            this.Controls.Add(this.checkBoxDoSend);
            this.Controls.Add(this.labelConnectedTo);
            this.Controls.Add(labelConnectToLabel);
            this.Controls.Add(this.labelTime);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.buttonListenSerial);
            this.Controls.Add(this.buttonConnectVision);
            this.Controls.Add(this.propertyGrid1);
            this.Controls.Add(label5);
            this.Controls.Add(label4);
            this.Controls.Add(this.textBoxPeriod);
            this.Controls.Add(label2);
            this.Controls.Add(label1);
            this.Controls.Add(this.textBoxRobotID);
            this.Controls.Add(this.comboBoxFunctionList);
            this.Controls.Add(this.buttonStart);
            this.Name = "DataCollector";
            this.Text = "DataCollector";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DataCollector_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.ComboBox comboBoxFunctionList;
        private System.Windows.Forms.TextBox textBoxRobotID;
        private System.Windows.Forms.TextBox textBoxPeriod;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.Button buttonConnectVision;
        private System.Windows.Forms.Button buttonListenSerial;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Label labelTime;
        private System.Windows.Forms.Label labelConnectedTo;
        private System.Windows.Forms.CheckBox checkBoxDoSend;
        private System.Windows.Forms.TextBox textBoxTestDuration;
        private System.Windows.Forms.ListBox listBoxCommandHistory;
        private System.Windows.Forms.TextBox textBoxVisionHostname;
        private System.Windows.Forms.TextBox textBoxSerialPort;
        private System.Windows.Forms.TextBox textBoxconstP;
        private System.Windows.Forms.TextBox textBoxconstI;
        private System.Windows.Forms.TextBox textBoxconstD;
        private System.Windows.Forms.Button buttonSendPIDConstants;
        private System.Windows.Forms.TextBox textBoxSerialCommand;
        private System.Windows.Forms.Button buttonSendCustomSerial;
        private System.Windows.Forms.ListBox listBoxInputHistory;
        private System.Windows.Forms.CheckBox checkBoxLimitCommands;
        private System.Windows.Forms.TextBox txtRF;
        private System.Windows.Forms.TextBox txtLF;
        private System.Windows.Forms.TextBox txtVisionPort;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox txtLB;
        private System.Windows.Forms.TextBox txtRB;
        private System.Windows.Forms.TextBox txtID;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
    }
}