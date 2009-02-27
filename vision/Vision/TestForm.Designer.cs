namespace Vision
{
	partial class TestForm
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
			this.checkRobot0 = new System.Windows.Forms.CheckBox();
			this.checkRobot1 = new System.Windows.Forms.CheckBox();
			this.checkRobot2 = new System.Windows.Forms.CheckBox();
			this.checkRobot3 = new System.Windows.Forms.CheckBox();
			this.checkRobot4 = new System.Windows.Forms.CheckBox();
			this.checkBall = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.btnTest = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// checkRobot0
			// 
			this.checkRobot0.AutoSize = true;
			this.checkRobot0.Location = new System.Drawing.Point(24, 38);
			this.checkRobot0.Name = "checkRobot0";
			this.checkRobot0.Size = new System.Drawing.Size(61, 17);
			this.checkRobot0.TabIndex = 0;
			this.checkRobot0.Tag = "0";
			this.checkRobot0.Text = "Robot0";
			this.checkRobot0.UseVisualStyleBackColor = true;
			// 
			// checkRobot1
			// 
			this.checkRobot1.AutoSize = true;
			this.checkRobot1.Location = new System.Drawing.Point(24, 61);
			this.checkRobot1.Name = "checkRobot1";
			this.checkRobot1.Size = new System.Drawing.Size(61, 17);
			this.checkRobot1.TabIndex = 1;
			this.checkRobot1.Tag = "1";
			this.checkRobot1.Text = "Robot1";
			this.checkRobot1.UseVisualStyleBackColor = true;
			// 
			// checkRobot2
			// 
			this.checkRobot2.AutoSize = true;
			this.checkRobot2.Location = new System.Drawing.Point(24, 84);
			this.checkRobot2.Name = "checkRobot2";
			this.checkRobot2.Size = new System.Drawing.Size(61, 17);
			this.checkRobot2.TabIndex = 2;
			this.checkRobot2.Tag = "2";
			this.checkRobot2.Text = "Robot2";
			this.checkRobot2.UseVisualStyleBackColor = true;
			// 
			// checkRobot3
			// 
			this.checkRobot3.AutoSize = true;
			this.checkRobot3.Location = new System.Drawing.Point(24, 107);
			this.checkRobot3.Name = "checkRobot3";
			this.checkRobot3.Size = new System.Drawing.Size(61, 17);
			this.checkRobot3.TabIndex = 3;
			this.checkRobot3.Tag = "3";
			this.checkRobot3.Text = "Robot3";
			this.checkRobot3.UseVisualStyleBackColor = true;
			// 
			// checkRobot4
			// 
			this.checkRobot4.AutoSize = true;
			this.checkRobot4.Location = new System.Drawing.Point(24, 130);
			this.checkRobot4.Name = "checkRobot4";
			this.checkRobot4.Size = new System.Drawing.Size(61, 17);
			this.checkRobot4.TabIndex = 4;
			this.checkRobot4.Tag = "4";
			this.checkRobot4.Text = "Robot4";
			this.checkRobot4.UseVisualStyleBackColor = true;
			// 
			// checkBall
			// 
			this.checkBall.AutoSize = true;
			this.checkBall.Location = new System.Drawing.Point(128, 38);
			this.checkBall.Name = "checkBall";
			this.checkBall.Size = new System.Drawing.Size(43, 17);
			this.checkBall.TabIndex = 5;
			this.checkBall.Text = "Ball";
			this.checkBall.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(21, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(172, 13);
			this.label1.TabIndex = 6;
			this.label1.Text = "Select items seen in the sequence:";
			// 
			// btnTest
			// 
			this.btnTest.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnTest.Location = new System.Drawing.Point(118, 124);
			this.btnTest.Name = "btnTest";
			this.btnTest.Size = new System.Drawing.Size(75, 23);
			this.btnTest.TabIndex = 7;
			this.btnTest.Text = "Test";
			this.btnTest.UseVisualStyleBackColor = true;
			this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(205, 124);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 8;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// TestForm
			// 
			this.AcceptButton = this.btnTest;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(292, 161);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnTest);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.checkBall);
			this.Controls.Add(this.checkRobot4);
			this.Controls.Add(this.checkRobot3);
			this.Controls.Add(this.checkRobot2);
			this.Controls.Add(this.checkRobot1);
			this.Controls.Add(this.checkRobot0);
			this.Location = new System.Drawing.Point(400, 400);
			this.Name = "TestForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "TestForm";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.CheckBox checkRobot0;
		private System.Windows.Forms.CheckBox checkRobot1;
		private System.Windows.Forms.CheckBox checkRobot2;
		private System.Windows.Forms.CheckBox checkRobot3;
		private System.Windows.Forms.CheckBox checkRobot4;
		private System.Windows.Forms.CheckBox checkBall;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnTest;
		private System.Windows.Forms.Button btnCancel;
	}
}