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
            this.BtnVision = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.VisionHost = new System.Windows.Forms.TextBox();
            this.BtnStartStop = new System.Windows.Forms.Button();
            this.VisionStatus = new System.Windows.Forms.Label();
            this.ControlStatus = new System.Windows.Forms.Label();
            this.ControlHost = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.BtnControl = new System.Windows.Forms.Button();
            this.btnReloadPIDConstants = new System.Windows.Forms.Button();
            this.BtnKick = new System.Windows.Forms.Button();
            this.cmbMotionPlanner = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // BtnVision
            // 
            this.BtnVision.Location = new System.Drawing.Point(168, 40);
            this.BtnVision.Name = "BtnVision";
            this.BtnVision.Size = new System.Drawing.Size(75, 23);
            this.BtnVision.TabIndex = 0;
            this.BtnVision.Text = "Connect";
            this.BtnVision.UseVisualStyleBackColor = true;
            this.BtnVision.Click += new System.EventHandler(this.BtnVision_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(44, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Vision";
            // 
            // VisionHost
            // 
            this.VisionHost.Location = new System.Drawing.Point(47, 43);
            this.VisionHost.Name = "VisionHost";
            this.VisionHost.Size = new System.Drawing.Size(100, 20);
            this.VisionHost.TabIndex = 2;
            this.VisionHost.Text = "localhost";
            // 
            // BtnStartStop
            // 
            this.BtnStartStop.Location = new System.Drawing.Point(168, 186);
            this.BtnStartStop.Name = "BtnStartStop";
            this.BtnStartStop.Size = new System.Drawing.Size(75, 23);
            this.BtnStartStop.TabIndex = 3;
            this.BtnStartStop.Text = "Start";
            this.BtnStartStop.UseVisualStyleBackColor = true;
            this.BtnStartStop.Click += new System.EventHandler(this.BtnStartStop_Click);
            // 
            // VisionStatus
            // 
            this.VisionStatus.BackColor = System.Drawing.Color.Red;
            this.VisionStatus.Location = new System.Drawing.Point(165, 22);
            this.VisionStatus.Name = "VisionStatus";
            this.VisionStatus.Size = new System.Drawing.Size(78, 14);
            this.VisionStatus.TabIndex = 4;
            // 
            // ControlStatus
            // 
            this.ControlStatus.BackColor = System.Drawing.Color.Red;
            this.ControlStatus.Location = new System.Drawing.Point(165, 77);
            this.ControlStatus.Name = "ControlStatus";
            this.ControlStatus.Size = new System.Drawing.Size(78, 14);
            this.ControlStatus.TabIndex = 8;
            // 
            // ControlHost
            // 
            this.ControlHost.Location = new System.Drawing.Point(47, 98);
            this.ControlHost.Name = "ControlHost";
            this.ControlHost.Size = new System.Drawing.Size(100, 20);
            this.ControlHost.TabIndex = 7;
            this.ControlHost.Text = "localhost";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(44, 78);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Control";
            // 
            // BtnControl
            // 
            this.BtnControl.Location = new System.Drawing.Point(168, 95);
            this.BtnControl.Name = "BtnControl";
            this.BtnControl.Size = new System.Drawing.Size(75, 23);
            this.BtnControl.TabIndex = 5;
            this.BtnControl.Text = "Connect";
            this.BtnControl.UseVisualStyleBackColor = true;
            this.BtnControl.Click += new System.EventHandler(this.BtnControl_Click);
            // 
            // btnReloadPIDConstants
            // 
            this.btnReloadPIDConstants.Location = new System.Drawing.Point(12, 238);
            this.btnReloadPIDConstants.Name = "btnReloadPIDConstants";
            this.btnReloadPIDConstants.Size = new System.Drawing.Size(112, 23);
            this.btnReloadPIDConstants.TabIndex = 10;
            this.btnReloadPIDConstants.Text = "Reload PID Consts";
            this.btnReloadPIDConstants.UseVisualStyleBackColor = true;
            this.btnReloadPIDConstants.Click += new System.EventHandler(this.btnReloadPIDConstants_Click);
            // 
            // BtnKick
            // 
            this.BtnKick.Location = new System.Drawing.Point(72, 186);
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
            this.cmbMotionPlanner.Location = new System.Drawing.Point(47, 145);
            this.cmbMotionPlanner.Name = "cmbMotionPlanner";
            this.cmbMotionPlanner.Size = new System.Drawing.Size(196, 21);
            this.cmbMotionPlanner.TabIndex = 12;
            this.cmbMotionPlanner.SelectedIndexChanged += new System.EventHandler(this.cmbMotionPlanner_SelectedIndexChanged);
            // 
            // FollowerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.cmbMotionPlanner);
            this.Controls.Add(this.BtnKick);
            this.Controls.Add(this.btnReloadPIDConstants);
            this.Controls.Add(this.ControlStatus);
            this.Controls.Add(this.ControlHost);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.BtnControl);
            this.Controls.Add(this.VisionStatus);
            this.Controls.Add(this.BtnStartStop);
            this.Controls.Add(this.VisionHost);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.BtnVision);
            this.Name = "FollowerForm";
            this.Text = "FollowerForm";
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button BtnVision;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox VisionHost;
		private System.Windows.Forms.Button BtnStartStop;
		private System.Windows.Forms.Label VisionStatus;
		private System.Windows.Forms.Label ControlStatus;
		private System.Windows.Forms.TextBox ControlHost;
		private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button BtnControl;
        private System.Windows.Forms.Button btnReloadPIDConstants;
        private System.Windows.Forms.Button BtnKick;
        private System.Windows.Forms.ComboBox cmbMotionPlanner;
	}
}