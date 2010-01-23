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
            this.txtCommandList = new System.Windows.Forms.TextBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.statusLabel = new System.Windows.Forms.Label();
            this.reloadMotor = new System.Windows.Forms.Button();
            this.lblID = new System.Windows.Forms.Label();
            this.textBoxRemoteHost = new System.Windows.Forms.TextBox();
            this.radioButtonSerial = new System.Windows.Forms.RadioButton();
            this.radioButtonRemote = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.udCOMPort = new System.Windows.Forms.NumericUpDown();
            this.lblSendStatus = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtRemotePort = new System.Windows.Forms.TextBox();
            this.chkRebootTimerEnabled = new System.Windows.Forms.CheckBox();
            this.txtRebootTimerInterval = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblListenStatus = new System.Windows.Forms.Label();
            this.btnListen = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtListenPort = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.udCOMPort)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(28, 40);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(70, 13);
            label1.TabIndex = 47;
            label1.Text = "Remote host:";
            // 
            // txtCommandList
            // 
            this.txtCommandList.Location = new System.Drawing.Point(12, 13);
            this.txtCommandList.Multiline = true;
            this.txtCommandList.Name = "txtCommandList";
            this.txtCommandList.ReadOnly = true;
            this.txtCommandList.Size = new System.Drawing.Size(373, 405);
            this.txtCommandList.TabIndex = 0;
            this.txtCommandList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RemoteControl_KeyDown);
            this.txtCommandList.KeyUp += new System.Windows.Forms.KeyEventHandler(this.RemoteControl_KeyUp);
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(16, 148);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(134, 30);
            this.btnConnect.TabIndex = 1;
            this.btnConnect.TabStop = false;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            this.btnConnect.KeyUp += new System.Windows.Forms.KeyEventHandler(this.RemoteControl_KeyUp);
            this.btnConnect.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RemoteControl_KeyDown);
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusLabel.Location = new System.Drawing.Point(165, 432);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(284, 55);
            this.statusLabel.TabIndex = 3;
            this.statusLabel.Text = "pressed key";
            // 
            // reloadMotor
            // 
            this.reloadMotor.Location = new System.Drawing.Point(391, 384);
            this.reloadMotor.Name = "reloadMotor";
            this.reloadMotor.Size = new System.Drawing.Size(159, 34);
            this.reloadMotor.TabIndex = 44;
            this.reloadMotor.TabStop = false;
            this.reloadMotor.Text = "Reload Motor Corrections";
            this.reloadMotor.UseVisualStyleBackColor = true;
            this.reloadMotor.Click += new System.EventHandler(this.reloadMotor_Click);
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
            this.textBoxRemoteHost.Location = new System.Drawing.Point(102, 37);
            this.textBoxRemoteHost.Name = "textBoxRemoteHost";
            this.textBoxRemoteHost.Size = new System.Drawing.Size(48, 20);
            this.textBoxRemoteHost.TabIndex = 46;
            // 
            // radioButtonSerial
            // 
            this.radioButtonSerial.AutoSize = true;
            this.radioButtonSerial.Location = new System.Drawing.Point(13, 85);
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
            this.radioButtonRemote.Location = new System.Drawing.Point(13, 14);
            this.radioButtonRemote.Name = "radioButtonRemote";
            this.radioButtonRemote.Size = new System.Drawing.Size(87, 17);
            this.radioButtonRemote.TabIndex = 50;
            this.radioButtonRemote.Text = "Remote Host";
            this.radioButtonRemote.UseVisualStyleBackColor = true;
            this.radioButtonRemote.CheckedChanged += new System.EventHandler(this.radioButtonRemote_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(33, 110);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 51;
            this.label2.Text = "Port COM";
            // 
            // udCOMPort
            // 
            this.udCOMPort.Location = new System.Drawing.Point(102, 103);
            this.udCOMPort.Name = "udCOMPort";
            this.udCOMPort.Size = new System.Drawing.Size(48, 20);
            this.udCOMPort.TabIndex = 52;
            this.udCOMPort.Value = new decimal(new int[] {
            9,
            0,
            0,
            0});
            // 
            // lblSendStatus
            // 
            this.lblSendStatus.BackColor = System.Drawing.Color.Red;
            this.lblSendStatus.Location = new System.Drawing.Point(16, 131);
            this.lblSendStatus.Name = "lblSendStatus";
            this.lblSendStatus.Size = new System.Drawing.Size(134, 14);
            this.lblSendStatus.TabIndex = 53;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(28, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 13);
            this.label3.TabIndex = 54;
            this.label3.Text = "Remote port:";
            // 
            // txtRemotePort
            // 
            this.txtRemotePort.Location = new System.Drawing.Point(102, 59);
            this.txtRemotePort.Name = "txtRemotePort";
            this.txtRemotePort.Size = new System.Drawing.Size(48, 20);
            this.txtRemotePort.TabIndex = 55;
            this.txtRemotePort.Text = "50100";
            // 
            // chkRebootTimerEnabled
            // 
            this.chkRebootTimerEnabled.AutoSize = true;
            this.chkRebootTimerEnabled.Location = new System.Drawing.Point(28, 19);
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
            this.txtRebootTimerInterval.Location = new System.Drawing.Point(84, 42);
            this.txtRebootTimerInterval.Name = "txtRebootTimerInterval";
            this.txtRebootTimerInterval.Size = new System.Drawing.Size(66, 20);
            this.txtRebootTimerInterval.TabIndex = 57;
            this.txtRebootTimerInterval.Text = "15000";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 45);
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
            this.groupBox1.Location = new System.Drawing.Point(391, 312);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(159, 66);
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
            // btnListen
            // 
            this.btnListen.Location = new System.Drawing.Point(13, 70);
            this.btnListen.Name = "btnListen";
            this.btnListen.Size = new System.Drawing.Size(137, 30);
            this.btnListen.TabIndex = 60;
            this.btnListen.TabStop = false;
            this.btnListen.Text = "Listen";
            this.btnListen.UseVisualStyleBackColor = true;
            this.btnListen.Click += new System.EventHandler(this.btnListen_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtListenPort);
            this.groupBox2.Controls.Add(this.lblListenStatus);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.btnListen);
            this.groupBox2.Location = new System.Drawing.Point(391, 11);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(159, 103);
            this.groupBox2.TabIndex = 62;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Listen";
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
            // txtListenPort
            // 
            this.txtListenPort.Location = new System.Drawing.Point(91, 23);
            this.txtListenPort.Name = "txtListenPort";
            this.txtListenPort.Size = new System.Drawing.Size(59, 20);
            this.txtListenPort.TabIndex = 1;
            this.txtListenPort.Text = "50100";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnConnect);
            this.groupBox3.Controls.Add(this.textBoxRemoteHost);
            this.groupBox3.Controls.Add(label1);
            this.groupBox3.Controls.Add(this.txtRemotePort);
            this.groupBox3.Controls.Add(this.radioButtonSerial);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.radioButtonRemote);
            this.groupBox3.Controls.Add(this.lblSendStatus);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.udCOMPort);
            this.groupBox3.Location = new System.Drawing.Point(391, 120);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(159, 186);
            this.groupBox3.TabIndex = 63;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Send";
            // 
            // RemoteControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(559, 497);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lblID);
            this.Controls.Add(this.reloadMotor);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.txtCommandList);
            this.Name = "RemoteControl";
            this.Text = "Remote Control";
            this.Load += new System.EventHandler(this.RemoteControl_Load);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.RemoteControl_KeyUp);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RemoteControl_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.udCOMPort)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtCommandList;
        private System.Windows.Forms.Button btnConnect;
        public System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Button reloadMotor;
        public System.Windows.Forms.Label lblID;
        private System.Windows.Forms.TextBox textBoxRemoteHost;
        private System.Windows.Forms.RadioButton radioButtonSerial;
        private System.Windows.Forms.RadioButton radioButtonRemote;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown udCOMPort;
        private System.Windows.Forms.Label lblSendStatus;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtRemotePort;
        private System.Windows.Forms.CheckBox chkRebootTimerEnabled;
        private System.Windows.Forms.TextBox txtRebootTimerInterval;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblListenStatus;
        private System.Windows.Forms.Button btnListen;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtListenPort;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox3;
    }
}

