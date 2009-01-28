namespace Robotics.Commander
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
            System.Windows.Forms.Label label1;
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.OpenCOM = new System.Windows.Forms.Button();
            this.statusLabel = new System.Windows.Forms.Label();
            this.reloadMotor = new System.Windows.Forms.Button();
            this.lblID = new System.Windows.Forms.Label();
            this.textBoxRemoteHost = new System.Windows.Forms.TextBox();
            this.radioButtonNone = new System.Windows.Forms.RadioButton();
            this.radioButtonSerial = new System.Windows.Forms.RadioButton();
            this.radioButtonRemote = new System.Windows.Forms.RadioButton();
            label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(407, 231);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(70, 13);
            label1.TabIndex = 47;
            label1.Text = "Remote host:";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 13);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(373, 408);
            this.textBox1.TabIndex = 0;
            this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RemoteControl_KeyDown);
            this.textBox1.KeyUp += new System.Windows.Forms.KeyEventHandler(this.RemoteControl_KeyUp);
            // 
            // OpenCOM
            // 
            this.OpenCOM.Location = new System.Drawing.Point(410, 13);
            this.OpenCOM.Name = "OpenCOM";
            this.OpenCOM.Size = new System.Drawing.Size(118, 54);
            this.OpenCOM.TabIndex = 1;
            this.OpenCOM.TabStop = false;
            this.OpenCOM.Text = "Open COM";
            this.OpenCOM.UseVisualStyleBackColor = true;
            this.OpenCOM.Click += new System.EventHandler(this.toggleSettings);
            this.OpenCOM.KeyUp += new System.Windows.Forms.KeyEventHandler(this.RemoteControl_KeyUp);
            this.OpenCOM.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RemoteControl_KeyDown);
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 42F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.statusLabel.Location = new System.Drawing.Point(199, 424);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(329, 64);
            this.statusLabel.TabIndex = 3;
            this.statusLabel.Text = "pressed key";
            // 
            // reloadMotor
            // 
            this.reloadMotor.Location = new System.Drawing.Point(410, 73);
            this.reloadMotor.Name = "reloadMotor";
            this.reloadMotor.Size = new System.Drawing.Size(118, 66);
            this.reloadMotor.TabIndex = 44;
            this.reloadMotor.TabStop = false;
            this.reloadMotor.Text = "Reload Motor Corrections";
            this.reloadMotor.UseVisualStyleBackColor = true;
            this.reloadMotor.Click += new System.EventHandler(this.button3_Click);
            // 
            // lblID
            // 
            this.lblID.AutoSize = true;
            this.lblID.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblID.Location = new System.Drawing.Point(6, 449);
            this.lblID.Name = "lblID";
            this.lblID.Size = new System.Drawing.Size(153, 33);
            this.lblID.TabIndex = 45;
            this.lblID.Text = "RobotID: 0";
            // 
            // textBoxRemoteHost
            // 
            this.textBoxRemoteHost.Location = new System.Drawing.Point(410, 247);
            this.textBoxRemoteHost.Name = "textBoxRemoteHost";
            this.textBoxRemoteHost.Size = new System.Drawing.Size(100, 20);
            this.textBoxRemoteHost.TabIndex = 46;
            this.textBoxRemoteHost.Text = "localhost";
            // 
            // radioButtonNone
            // 
            this.radioButtonNone.AutoSize = true;
            this.radioButtonNone.Checked = true;
            this.radioButtonNone.Location = new System.Drawing.Point(414, 304);
            this.radioButtonNone.Name = "radioButtonNone";
            this.radioButtonNone.Size = new System.Drawing.Size(96, 17);
            this.radioButtonNone.TabIndex = 48;
            this.radioButtonNone.TabStop = true;
            this.radioButtonNone.Text = "No Connection";
            this.radioButtonNone.UseVisualStyleBackColor = true;
            // 
            // radioButtonSerial
            // 
            this.radioButtonSerial.AutoSize = true;
            this.radioButtonSerial.Location = new System.Drawing.Point(414, 327);
            this.radioButtonSerial.Name = "radioButtonSerial";
            this.radioButtonSerial.Size = new System.Drawing.Size(73, 17);
            this.radioButtonSerial.TabIndex = 49;
            this.radioButtonSerial.Text = "Serial Port";
            this.radioButtonSerial.UseVisualStyleBackColor = true;
            this.radioButtonSerial.CheckedChanged += new System.EventHandler(this.radioButtonSerial_CheckedChanged);
            // 
            // radioButtonRemote
            // 
            this.radioButtonRemote.AutoSize = true;
            this.radioButtonRemote.Location = new System.Drawing.Point(414, 350);
            this.radioButtonRemote.Name = "radioButtonRemote";
            this.radioButtonRemote.Size = new System.Drawing.Size(87, 17);
            this.radioButtonRemote.TabIndex = 50;
            this.radioButtonRemote.Text = "Remote Host";
            this.radioButtonRemote.UseVisualStyleBackColor = true;
            this.radioButtonRemote.CheckedChanged += new System.EventHandler(this.radioButtonRemote_CheckedChanged);
            // 
            // RemoteControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(568, 497);
            this.Controls.Add(this.radioButtonRemote);
            this.Controls.Add(this.radioButtonSerial);
            this.Controls.Add(this.radioButtonNone);
            this.Controls.Add(label1);
            this.Controls.Add(this.textBoxRemoteHost);
            this.Controls.Add(this.lblID);
            this.Controls.Add(this.reloadMotor);
            this.Controls.Add(this.OpenCOM);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.textBox1);
            this.Name = "RemoteControl";
            this.Text = "Remote Control";
            this.Load += new System.EventHandler(this.RemoteControl_Load);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.RemoteControl_KeyUp);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RemoteControl_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button OpenCOM;
        public System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Button reloadMotor;
        public System.Windows.Forms.Label lblID;
        private System.Windows.Forms.TextBox textBoxRemoteHost;
        private System.Windows.Forms.RadioButton radioButtonNone;
        private System.Windows.Forms.RadioButton radioButtonSerial;
        private System.Windows.Forms.RadioButton radioButtonRemote;
    }
}

