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
			this.BtnStart = new System.Windows.Forms.Button();
			this.VisionStatus = new System.Windows.Forms.Label();
			this.ControlStatus = new System.Windows.Forms.Label();
			this.ControlHost = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.BtnControl = new System.Windows.Forms.Button();
			this.BtnStop = new System.Windows.Forms.Button();
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
			// BtnStart
			// 
			this.BtnStart.Location = new System.Drawing.Point(168, 186);
			this.BtnStart.Name = "BtnStart";
			this.BtnStart.Size = new System.Drawing.Size(75, 23);
			this.BtnStart.TabIndex = 3;
			this.BtnStart.Text = "Start";
			this.BtnStart.UseVisualStyleBackColor = true;
			this.BtnStart.Click += new System.EventHandler(this.BtnStart_Click);
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
			this.ControlStatus.Location = new System.Drawing.Point(168, 116);
			this.ControlStatus.Name = "ControlStatus";
			this.ControlStatus.Size = new System.Drawing.Size(78, 14);
			this.ControlStatus.TabIndex = 8;
			// 
			// ControlHost
			// 
			this.ControlHost.Location = new System.Drawing.Point(50, 137);
			this.ControlHost.Name = "ControlHost";
			this.ControlHost.Size = new System.Drawing.Size(100, 20);
			this.ControlHost.TabIndex = 7;
			this.ControlHost.Text = "localhost";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(47, 117);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(40, 13);
			this.label3.TabIndex = 6;
			this.label3.Text = "Control";
			// 
			// BtnControl
			// 
			this.BtnControl.Location = new System.Drawing.Point(171, 134);
			this.BtnControl.Name = "BtnControl";
			this.BtnControl.Size = new System.Drawing.Size(75, 23);
			this.BtnControl.TabIndex = 5;
			this.BtnControl.Text = "Connect";
			this.BtnControl.UseVisualStyleBackColor = true;
			this.BtnControl.Click += new System.EventHandler(this.BtnControl_Click);
			// 
			// BtnStop
			// 
			this.BtnStop.Location = new System.Drawing.Point(168, 215);
			this.BtnStop.Name = "BtnStop";
			this.BtnStop.Size = new System.Drawing.Size(75, 23);
			this.BtnStop.TabIndex = 9;
			this.BtnStop.Text = "Stop";
			this.BtnStop.UseVisualStyleBackColor = true;
			this.BtnStop.Click += new System.EventHandler(this.BtnStop_Click);
			// 
			// FollowerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(292, 273);
			this.Controls.Add(this.BtnStop);
			this.Controls.Add(this.ControlStatus);
			this.Controls.Add(this.ControlHost);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.BtnControl);
			this.Controls.Add(this.VisionStatus);
			this.Controls.Add(this.BtnStart);
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
		private System.Windows.Forms.Button BtnStart;
		private System.Windows.Forms.Label VisionStatus;
		private System.Windows.Forms.Label ControlStatus;
		private System.Windows.Forms.TextBox ControlHost;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button BtnControl;
		private System.Windows.Forms.Button BtnStop;
	}
}