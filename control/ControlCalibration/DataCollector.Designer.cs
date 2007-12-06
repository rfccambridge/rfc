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
            this.listBoxHistory = new System.Windows.Forms.ListBox();
            this.textBoxVisionHostname = new System.Windows.Forms.TextBox();
            this.textBoxSerialPort = new System.Windows.Forms.TextBox();
            this.textBoxconstP = new System.Windows.Forms.TextBox();
            this.textBoxconstI = new System.Windows.Forms.TextBox();
            this.textBoxconstD = new System.Windows.Forms.TextBox();
            this.buttonSendPIDConstants = new System.Windows.Forms.Button();
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
            label1.Location = new System.Drawing.Point(394, 74);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(53, 13);
            label1.TabIndex = 3;
            label1.Text = "Robot ID:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(394, 113);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(107, 13);
            label2.TabIndex = 4;
            label2.Text = "Command frequency:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(500, 132);
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
            label3.Location = new System.Drawing.Point(391, 152);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(74, 13);
            label3.TabIndex = 16;
            label3.Text = "Test Duration:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(500, 171);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(18, 13);
            label5.TabIndex = 7;
            label5.Text = "(s)";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(151, 28);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(73, 13);
            label6.TabIndex = 20;
            label6.Text = "Vision source:";
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
            label8.Location = new System.Drawing.Point(246, 215);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(14, 13);
            label8.TabIndex = 22;
            label8.Text = "P";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(246, 241);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(10, 13);
            label9.TabIndex = 22;
            label9.Text = "I";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(246, 267);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(15, 13);
            label10.TabIndex = 22;
            label10.Text = "D";
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(394, 275);
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
            this.comboBoxFunctionList.Location = new System.Drawing.Point(394, 248);
            this.comboBoxFunctionList.Name = "comboBoxFunctionList";
            this.comboBoxFunctionList.Size = new System.Drawing.Size(121, 21);
            this.comboBoxFunctionList.TabIndex = 1;
            this.comboBoxFunctionList.SelectedIndexChanged += new System.EventHandler(this.comboBoxFunctionList_SelectedIndexChanged);
            // 
            // textBoxRobotID
            // 
            this.textBoxRobotID.Location = new System.Drawing.Point(394, 90);
            this.textBoxRobotID.Name = "textBoxRobotID";
            this.textBoxRobotID.Size = new System.Drawing.Size(100, 20);
            this.textBoxRobotID.TabIndex = 2;
            this.textBoxRobotID.Text = "0";
            // 
            // textBoxPeriod
            // 
            this.textBoxPeriod.Location = new System.Drawing.Point(394, 129);
            this.textBoxPeriod.Name = "textBoxPeriod";
            this.textBoxPeriod.Size = new System.Drawing.Size(100, 20);
            this.textBoxPeriod.TabIndex = 6;
            this.textBoxPeriod.Text = "25";
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Right;
            this.propertyGrid1.Location = new System.Drawing.Point(552, 0);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(159, 455);
            this.propertyGrid1.TabIndex = 8;
            // 
            // buttonConnectVision
            // 
            this.buttonConnectVision.Location = new System.Drawing.Point(44, 23);
            this.buttonConnectVision.Name = "buttonConnectVision";
            this.buttonConnectVision.Size = new System.Drawing.Size(101, 23);
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
            this.buttonStop.Location = new System.Drawing.Point(394, 304);
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
            this.labelTime.Location = new System.Drawing.Point(394, 330);
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
            this.textBoxTestDuration.Location = new System.Drawing.Point(394, 168);
            this.textBoxTestDuration.Name = "textBoxTestDuration";
            this.textBoxTestDuration.Size = new System.Drawing.Size(100, 20);
            this.textBoxTestDuration.TabIndex = 17;
            this.textBoxTestDuration.Text = "10.0";
            // 
            // listBoxHistory
            // 
            this.listBoxHistory.FormattingEnabled = true;
            this.listBoxHistory.Location = new System.Drawing.Point(44, 148);
            this.listBoxHistory.Name = "listBoxHistory";
            this.listBoxHistory.Size = new System.Drawing.Size(175, 238);
            this.listBoxHistory.TabIndex = 18;
            // 
            // textBoxVisionHostname
            // 
            this.textBoxVisionHostname.Location = new System.Drawing.Point(230, 25);
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
            this.textBoxSerialPort.Text = "COM4";
            // 
            // textBoxconstP
            // 
            this.textBoxconstP.Location = new System.Drawing.Point(268, 212);
            this.textBoxconstP.Name = "textBoxconstP";
            this.textBoxconstP.Size = new System.Drawing.Size(72, 20);
            this.textBoxconstP.TabIndex = 23;
            this.textBoxconstP.Text = "0";
            // 
            // textBoxconstI
            // 
            this.textBoxconstI.Location = new System.Drawing.Point(268, 238);
            this.textBoxconstI.Name = "textBoxconstI";
            this.textBoxconstI.Size = new System.Drawing.Size(72, 20);
            this.textBoxconstI.TabIndex = 24;
            this.textBoxconstI.Text = "0";
            // 
            // textBoxconstD
            // 
            this.textBoxconstD.Location = new System.Drawing.Point(268, 264);
            this.textBoxconstD.Name = "textBoxconstD";
            this.textBoxconstD.Size = new System.Drawing.Size(72, 20);
            this.textBoxconstD.TabIndex = 25;
            this.textBoxconstD.Text = "0";
            // 
            // buttonSendPIDConstants
            // 
            this.buttonSendPIDConstants.Location = new System.Drawing.Point(249, 290);
            this.buttonSendPIDConstants.Name = "buttonSendPIDConstants";
            this.buttonSendPIDConstants.Size = new System.Drawing.Size(91, 23);
            this.buttonSendPIDConstants.TabIndex = 26;
            this.buttonSendPIDConstants.Text = "Send Constants";
            this.buttonSendPIDConstants.UseVisualStyleBackColor = true;
            // 
            // DataCollector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(711, 455);
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
            this.Controls.Add(this.listBoxHistory);
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
        private System.Windows.Forms.ListBox listBoxHistory;
        private System.Windows.Forms.TextBox textBoxVisionHostname;
        private System.Windows.Forms.TextBox textBoxSerialPort;
        private System.Windows.Forms.TextBox textBoxconstP;
        private System.Windows.Forms.TextBox textBoxconstI;
        private System.Windows.Forms.TextBox textBoxconstD;
        private System.Windows.Forms.Button buttonSendPIDConstants;
    }
}