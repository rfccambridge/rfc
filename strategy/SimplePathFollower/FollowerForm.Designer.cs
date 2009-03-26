namespace SimplePathFollower
{
	partial class FollowerForm
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
            this.btnConnectVisionTop = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtVisionHostTop = new System.Windows.Forms.TextBox();
            this.btnStartStop = new System.Windows.Forms.Button();
            this.lblVisionStatusTop = new System.Windows.Forms.Label();
            this.ControlStatus = new System.Windows.Forms.Label();
            this.ControlHost = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.BtnControl = new System.Windows.Forms.Button();
            this.btnReloadPIDConstants = new System.Windows.Forms.Button();
            this.BtnKick = new System.Windows.Forms.Button();
            this.cmbMotionPlanner = new System.Windows.Forms.ComboBox();
            this.btnLogNext = new System.Windows.Forms.Button();
            this.btnStartStopLogging = new System.Windows.Forms.Button();
            this.btnLogOpenClose = new System.Windows.Forms.Button();
            this.txtRobotID = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblVisionStatusBottom = new System.Windows.Forms.Label();
            this.txtVisionHostBottom = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.btnConnectVisionBottom = new System.Windows.Forms.Button();
            this.btnSwitchGoal = new System.Windows.Forms.Button();
            this.btnLap = new System.Windows.Forms.Button();
            this.btnCalibratePID = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnConnectVisionTop
            // 
            this.btnConnectVisionTop.Location = new System.Drawing.Point(168, 40);
            this.btnConnectVisionTop.Name = "btnConnectVisionTop";
            this.btnConnectVisionTop.Size = new System.Drawing.Size(75, 23);
            this.btnConnectVisionTop.TabIndex = 1;
            this.btnConnectVisionTop.Text = "Connect";
            this.btnConnectVisionTop.UseVisualStyleBackColor = true;
            this.btnConnectVisionTop.Click += new System.EventHandler(this.btnVisionTop_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(44, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Vision (top)";
            // 
            // txtVisionHostTop
            // 
            this.txtVisionHostTop.Location = new System.Drawing.Point(47, 43);
            this.txtVisionHostTop.Name = "txtVisionHostTop";
            this.txtVisionHostTop.Size = new System.Drawing.Size(100, 20);
            this.txtVisionHostTop.TabIndex = 0;
            this.txtVisionHostTop.Text = "localhost";
            // 
            // btnStartStop
            // 
            this.btnStartStop.Location = new System.Drawing.Point(149, 259);
            this.btnStartStop.Name = "btnStartStop";
            this.btnStartStop.Size = new System.Drawing.Size(94, 23);
            this.btnStartStop.TabIndex = 22;
            this.btnStartStop.Text = "MoveToPoint";
            this.btnStartStop.UseVisualStyleBackColor = true;
            this.btnStartStop.Click += new System.EventHandler(this.BtnStartStop_Click);
            // 
            // lblVisionStatusTop
            // 
            this.lblVisionStatusTop.BackColor = System.Drawing.Color.Red;
            this.lblVisionStatusTop.Location = new System.Drawing.Point(165, 22);
            this.lblVisionStatusTop.Name = "lblVisionStatusTop";
            this.lblVisionStatusTop.Size = new System.Drawing.Size(78, 14);
            this.lblVisionStatusTop.TabIndex = 4;
            // 
            // ControlStatus
            // 
            this.ControlStatus.BackColor = System.Drawing.Color.Red;
            this.ControlStatus.Location = new System.Drawing.Point(165, 132);
            this.ControlStatus.Name = "ControlStatus";
            this.ControlStatus.Size = new System.Drawing.Size(78, 14);
            this.ControlStatus.TabIndex = 8;
            // 
            // ControlHost
            // 
            this.ControlHost.Location = new System.Drawing.Point(47, 153);
            this.ControlHost.Name = "ControlHost";
            this.ControlHost.Size = new System.Drawing.Size(100, 20);
            this.ControlHost.TabIndex = 4;
            this.ControlHost.Text = "localhost";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(44, 133);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Control";
            // 
            // BtnControl
            // 
            this.BtnControl.Location = new System.Drawing.Point(168, 150);
            this.BtnControl.Name = "BtnControl";
            this.BtnControl.Size = new System.Drawing.Size(75, 23);
            this.BtnControl.TabIndex = 5;
            this.BtnControl.Text = "Connect";
            this.BtnControl.UseVisualStyleBackColor = true;
            this.BtnControl.Click += new System.EventHandler(this.BtnControl_Click);
            // 
            // btnReloadPIDConstants
            // 
            this.btnReloadPIDConstants.Location = new System.Drawing.Point(46, 302);
            this.btnReloadPIDConstants.Name = "btnReloadPIDConstants";
            this.btnReloadPIDConstants.Size = new System.Drawing.Size(112, 23);
            this.btnReloadPIDConstants.TabIndex = 10;
            this.btnReloadPIDConstants.Text = "Reload Constants";
            this.btnReloadPIDConstants.UseVisualStyleBackColor = true;
            this.btnReloadPIDConstants.Click += new System.EventHandler(this.btnReloadPIDConstants_Click);
            // 
            // BtnKick
            // 
            this.BtnKick.Location = new System.Drawing.Point(48, 259);
            this.BtnKick.Name = "BtnKick";
            this.BtnKick.Size = new System.Drawing.Size(75, 23);
            this.BtnKick.TabIndex = 11;
            this.BtnKick.Text = "Kick";
            this.BtnKick.UseVisualStyleBackColor = true;
            this.BtnKick.Click += new System.EventHandler(this.BtnKick_Click);
            // 
            // cmbMotionPlanner
            // 
            this.cmbMotionPlanner.FormattingEnabled = true;
            this.cmbMotionPlanner.Location = new System.Drawing.Point(46, 232);
            this.cmbMotionPlanner.Name = "cmbMotionPlanner";
            this.cmbMotionPlanner.Size = new System.Drawing.Size(196, 21);
            this.cmbMotionPlanner.TabIndex = 6;
            this.cmbMotionPlanner.SelectedIndexChanged += new System.EventHandler(this.cmbMotionPlanner_SelectedIndexChanged);
            // 
            // btnLogNext
            // 
            this.btnLogNext.Location = new System.Drawing.Point(7, 48);
            this.btnLogNext.Name = "btnLogNext";
            this.btnLogNext.Size = new System.Drawing.Size(156, 23);
            this.btnLogNext.TabIndex = 13;
            this.btnLogNext.Text = "Next Log Entry";
            this.btnLogNext.UseVisualStyleBackColor = true;
            this.btnLogNext.Click += new System.EventHandler(this.btnLogNext_Click);
            // 
            // btnStartStopLogging
            // 
            this.btnStartStopLogging.Location = new System.Drawing.Point(7, 19);
            this.btnStartStopLogging.Name = "btnStartStopLogging";
            this.btnStartStopLogging.Size = new System.Drawing.Size(75, 23);
            this.btnStartStopLogging.TabIndex = 14;
            this.btnStartStopLogging.Text = "Start log";
            this.btnStartStopLogging.UseVisualStyleBackColor = true;
            this.btnStartStopLogging.Click += new System.EventHandler(this.btnStartStopLogging_Click);
            // 
            // btnLogOpenClose
            // 
            this.btnLogOpenClose.Location = new System.Drawing.Point(88, 19);
            this.btnLogOpenClose.Name = "btnLogOpenClose";
            this.btnLogOpenClose.Size = new System.Drawing.Size(75, 23);
            this.btnLogOpenClose.TabIndex = 15;
            this.btnLogOpenClose.Text = "Open log";
            this.btnLogOpenClose.UseVisualStyleBackColor = true;
            this.btnLogOpenClose.Click += new System.EventHandler(this.btnLogOpenClose_Click);
            // 
            // txtRobotID
            // 
            this.txtRobotID.Location = new System.Drawing.Point(95, 184);
            this.txtRobotID.Name = "txtRobotID";
            this.txtRobotID.Size = new System.Drawing.Size(29, 20);
            this.txtRobotID.TabIndex = 16;
            this.txtRobotID.Text = "0";
            this.txtRobotID.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(45, 190);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 17;
            this.label2.Text = "RobotID:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(43, 216);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(78, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "MotionPlanner:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnLogNext);
            this.groupBox1.Controls.Add(this.btnStartStopLogging);
            this.groupBox1.Controls.Add(this.btnLogOpenClose);
            this.groupBox1.Location = new System.Drawing.Point(46, 360);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(172, 79);
            this.groupBox1.TabIndex = 19;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Logging";
            // 
            // lblVisionStatusBottom
            // 
            this.lblVisionStatusBottom.BackColor = System.Drawing.Color.Red;
            this.lblVisionStatusBottom.Location = new System.Drawing.Point(168, 76);
            this.lblVisionStatusBottom.Name = "lblVisionStatusBottom";
            this.lblVisionStatusBottom.Size = new System.Drawing.Size(78, 14);
            this.lblVisionStatusBottom.TabIndex = 23;
            // 
            // txtVisionHostBottom
            // 
            this.txtVisionHostBottom.Location = new System.Drawing.Point(46, 92);
            this.txtVisionHostBottom.Name = "txtVisionHostBottom";
            this.txtVisionHostBottom.Size = new System.Drawing.Size(100, 20);
            this.txtVisionHostBottom.TabIndex = 2;
            this.txtVisionHostBottom.Text = "localhost";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(43, 76);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(76, 13);
            this.label6.TabIndex = 21;
            this.label6.Text = "Vision (bottom)";
            // 
            // btnConnectVisionBottom
            // 
            this.btnConnectVisionBottom.Location = new System.Drawing.Point(167, 92);
            this.btnConnectVisionBottom.Name = "btnConnectVisionBottom";
            this.btnConnectVisionBottom.Size = new System.Drawing.Size(75, 23);
            this.btnConnectVisionBottom.TabIndex = 3;
            this.btnConnectVisionBottom.Text = "Connect";
            this.btnConnectVisionBottom.UseVisualStyleBackColor = true;
            this.btnConnectVisionBottom.Click += new System.EventHandler(this.btnVisionBottom_Click);
            // 
            // btnSwitchGoal
            // 
            this.btnSwitchGoal.Location = new System.Drawing.Point(171, 302);
            this.btnSwitchGoal.Name = "btnSwitchGoal";
            this.btnSwitchGoal.Size = new System.Drawing.Size(75, 23);
            this.btnSwitchGoal.TabIndex = 24;
            this.btnSwitchGoal.Text = "Switch Goal";
            this.btnSwitchGoal.UseVisualStyleBackColor = true;
            this.btnSwitchGoal.Click += new System.EventHandler(this.btnSwitchGoal_Click);
            // 
            // btnLap
            // 
            this.btnLap.Location = new System.Drawing.Point(171, 331);
            this.btnLap.Name = "btnLap";
            this.btnLap.Size = new System.Drawing.Size(75, 23);
            this.btnLap.TabIndex = 25;
            this.btnLap.Text = "Stop lapping";
            this.btnLap.UseVisualStyleBackColor = true;
            this.btnLap.Click += new System.EventHandler(this.btnLap_Click);
            // 
            // btnCalibratePID
            // 
            this.btnCalibratePID.Location = new System.Drawing.Point(47, 331);
            this.btnCalibratePID.Name = "btnCalibratePID";
            this.btnCalibratePID.Size = new System.Drawing.Size(111, 23);
            this.btnCalibratePID.TabIndex = 26;
            this.btnCalibratePID.Text = "Calibrate PID";
            this.btnCalibratePID.UseVisualStyleBackColor = true;
            this.btnCalibratePID.Click += new System.EventHandler(this.btnCalibratePID_Click);
            // 
            // FollowerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 483);
            this.Controls.Add(this.btnCalibratePID);
            this.Controls.Add(this.btnLap);
            this.Controls.Add(this.btnSwitchGoal);
            this.Controls.Add(this.lblVisionStatusBottom);
            this.Controls.Add(this.txtVisionHostBottom);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.btnConnectVisionBottom);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtRobotID);
            this.Controls.Add(this.cmbMotionPlanner);
            this.Controls.Add(this.BtnKick);
            this.Controls.Add(this.btnReloadPIDConstants);
            this.Controls.Add(this.ControlStatus);
            this.Controls.Add(this.ControlHost);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.BtnControl);
            this.Controls.Add(this.lblVisionStatusTop);
            this.Controls.Add(this.btnStartStop);
            this.Controls.Add(this.txtVisionHostTop);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnConnectVisionTop);
            this.Name = "FollowerForm";
            this.Text = "FollowerForm";
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnConnectVisionTop;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtVisionHostTop;
		private System.Windows.Forms.Button btnStartStop;
		private System.Windows.Forms.Label lblVisionStatusTop;
		private System.Windows.Forms.Label ControlStatus;
		private System.Windows.Forms.TextBox ControlHost;
		private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button BtnControl;
        private System.Windows.Forms.Button btnReloadPIDConstants;
        private System.Windows.Forms.Button BtnKick;
        private System.Windows.Forms.ComboBox cmbMotionPlanner;
        private System.Windows.Forms.Button btnLogNext;
        private System.Windows.Forms.Button btnStartStopLogging;
        private System.Windows.Forms.Button btnLogOpenClose;
        private System.Windows.Forms.TextBox txtRobotID;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblVisionStatusBottom;
        private System.Windows.Forms.TextBox txtVisionHostBottom;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnConnectVisionBottom;
        private System.Windows.Forms.Button btnSwitchGoal;
        private System.Windows.Forms.Button btnLap;
        private System.Windows.Forms.Button btnCalibratePID;
	}
}