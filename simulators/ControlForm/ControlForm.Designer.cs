namespace Robocup.ControlForm {
    partial class ControlForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.serialHost = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.serialStatus = new System.Windows.Forms.Label();
            this.serialConnect = new System.Windows.Forms.Button();
            this.rfcStart = new System.Windows.Forms.Button();
            this.rfcStatus = new System.Windows.Forms.Label();
            this.btnLogNext = new System.Windows.Forms.Button();
            this.btnLogOpenClose = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtRobotID = new System.Windows.Forms.TextBox();
            this.btnStartStopLogging = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.btnRefbox = new System.Windows.Forms.Button();
            this.lblRefbox = new System.Windows.Forms.Label();
            this.sslVisionConnect = new System.Windows.Forms.Button();
            this.sslVisionStatus = new System.Windows.Forms.Label();
            this.sslVisionHost = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtRefbox = new System.Windows.Forms.ComboBox();
            this.btnStartSim = new System.Windows.Forms.Button();
            this.lstPlays = new System.Windows.Forms.CheckedListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.chkSelectAll = new System.Windows.Forms.CheckBox();
            this.lblSimStatus = new System.Windows.Forms.Label();
            this.btnStartStopPlayer = new System.Windows.Forms.Button();
            this.lstSimplePlayer = new System.Windows.Forms.ComboBox();
            this.lblSimplePlayerStatus = new System.Windows.Forms.Label();
            this.serialPort = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // serialHost
            // 
            this.serialHost.Location = new System.Drawing.Point(12, 80);
            this.serialHost.Name = "serialHost";
            this.serialHost.Size = new System.Drawing.Size(86, 20);
            this.serialHost.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 13);
            this.label2.TabIndex = 24;
            this.label2.Text = "Serial (host/port):";
            // 
            // serialStatus
            // 
            this.serialStatus.AutoSize = true;
            this.serialStatus.BackColor = System.Drawing.Color.Red;
            this.serialStatus.Location = new System.Drawing.Point(150, 63);
            this.serialStatus.Name = "serialStatus";
            this.serialStatus.Size = new System.Drawing.Size(76, 13);
            this.serialStatus.TabIndex = 25;
            this.serialStatus.Text = "                       ";
            // 
            // serialConnect
            // 
            this.serialConnect.Location = new System.Drawing.Point(150, 79);
            this.serialConnect.Name = "serialConnect";
            this.serialConnect.Size = new System.Drawing.Size(80, 20);
            this.serialConnect.TabIndex = 5;
            this.serialConnect.Text = "Connect";
            this.serialConnect.UseVisualStyleBackColor = true;
            this.serialConnect.Click += new System.EventHandler(this.serialConnect_Click);
            // 
            // rfcStart
            // 
            this.rfcStart.Location = new System.Drawing.Point(12, 186);
            this.rfcStart.Name = "rfcStart";
            this.rfcStart.Size = new System.Drawing.Size(188, 34);
            this.rfcStart.TabIndex = 7;
            this.rfcStart.Text = "Start Interpret";
            this.rfcStart.UseVisualStyleBackColor = true;
            this.rfcStart.Click += new System.EventHandler(this.rfcStart_Click);
            // 
            // rfcStatus
            // 
            this.rfcStatus.BackColor = System.Drawing.Color.Red;
            this.rfcStatus.Location = new System.Drawing.Point(12, 170);
            this.rfcStatus.Name = "rfcStatus";
            this.rfcStatus.Size = new System.Drawing.Size(188, 13);
            this.rfcStatus.TabIndex = 29;
            this.rfcStatus.Text = "         ";
            // 
            // btnLogNext
            // 
            this.btnLogNext.Location = new System.Drawing.Point(7, 100);
            this.btnLogNext.Name = "btnLogNext";
            this.btnLogNext.Size = new System.Drawing.Size(75, 23);
            this.btnLogNext.TabIndex = 13;
            this.btnLogNext.Text = "Next Entry";
            this.btnLogNext.UseVisualStyleBackColor = true;
            this.btnLogNext.Click += new System.EventHandler(this.btnLogNext_Click);
            // 
            // btnLogOpenClose
            // 
            this.btnLogOpenClose.Location = new System.Drawing.Point(7, 70);
            this.btnLogOpenClose.Name = "btnLogOpenClose";
            this.btnLogOpenClose.Size = new System.Drawing.Size(75, 23);
            this.btnLogOpenClose.TabIndex = 15;
            this.btnLogOpenClose.Text = "Open log";
            this.btnLogOpenClose.UseVisualStyleBackColor = true;
            this.btnLogOpenClose.Click += new System.EventHandler(this.btnLogOpenClose_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtRobotID);
            this.groupBox1.Controls.Add(this.btnLogNext);
            this.groupBox1.Controls.Add(this.btnStartStopLogging);
            this.groupBox1.Controls.Add(this.btnLogOpenClose);
            this.groupBox1.Location = new System.Drawing.Point(260, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(91, 132);
            this.groupBox1.TabIndex = 30;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Logging";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 17;
            this.label3.Text = "RobotID:";
            // 
            // txtRobotID
            // 
            this.txtRobotID.Location = new System.Drawing.Point(54, 15);
            this.txtRobotID.Name = "txtRobotID";
            this.txtRobotID.Size = new System.Drawing.Size(27, 20);
            this.txtRobotID.TabIndex = 16;
            this.txtRobotID.Text = "0";
            // 
            // btnStartStopLogging
            // 
            this.btnStartStopLogging.Location = new System.Drawing.Point(8, 41);
            this.btnStartStopLogging.Name = "btnStartStopLogging";
            this.btnStartStopLogging.Size = new System.Drawing.Size(75, 23);
            this.btnStartStopLogging.TabIndex = 14;
            this.btnStartStopLogging.Text = "Start log";
            this.btnStartStopLogging.UseVisualStyleBackColor = true;
            this.btnStartStopLogging.Click += new System.EventHandler(this.btnStartStopLogging_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 116);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(44, 13);
            this.label6.TabIndex = 32;
            this.label6.Text = "Refbox:";
            // 
            // btnRefbox
            // 
            this.btnRefbox.Location = new System.Drawing.Point(150, 132);
            this.btnRefbox.Name = "btnRefbox";
            this.btnRefbox.Size = new System.Drawing.Size(80, 20);
            this.btnRefbox.TabIndex = 33;
            this.btnRefbox.Text = "Connect";
            this.btnRefbox.UseVisualStyleBackColor = true;
            this.btnRefbox.Click += new System.EventHandler(this.btnRefbox_Click);
            // 
            // lblRefbox
            // 
            this.lblRefbox.AutoSize = true;
            this.lblRefbox.BackColor = System.Drawing.Color.Red;
            this.lblRefbox.Location = new System.Drawing.Point(150, 116);
            this.lblRefbox.Name = "lblRefbox";
            this.lblRefbox.Size = new System.Drawing.Size(76, 13);
            this.lblRefbox.TabIndex = 34;
            this.lblRefbox.Text = "                       ";
            // 
            // sslVisionConnect
            // 
            this.sslVisionConnect.Location = new System.Drawing.Point(150, 30);
            this.sslVisionConnect.Name = "sslVisionConnect";
            this.sslVisionConnect.Size = new System.Drawing.Size(80, 20);
            this.sslVisionConnect.TabIndex = 35;
            this.sslVisionConnect.Text = "SSL Vision";
            this.sslVisionConnect.UseVisualStyleBackColor = true;
            this.sslVisionConnect.Click += new System.EventHandler(this.btnSSLVision_Click);
            // 
            // sslVisionStatus
            // 
            this.sslVisionStatus.AutoSize = true;
            this.sslVisionStatus.BackColor = System.Drawing.Color.Red;
            this.sslVisionStatus.Location = new System.Drawing.Point(150, 13);
            this.sslVisionStatus.Name = "sslVisionStatus";
            this.sslVisionStatus.Size = new System.Drawing.Size(76, 13);
            this.sslVisionStatus.TabIndex = 36;
            this.sslVisionStatus.Text = "                       ";
            // 
            // sslVisionHost
            // 
            this.sslVisionHost.Location = new System.Drawing.Point(12, 29);
            this.sslVisionHost.Name = "sslVisionHost";
            this.sslVisionHost.Size = new System.Drawing.Size(132, 20);
            this.sslVisionHost.TabIndex = 37;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 13);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(61, 13);
            this.label7.TabIndex = 38;
            this.label7.Text = "SSL Vision:";
            // 
            // txtRefbox
            // 
            this.txtRefbox.FormattingEnabled = true;
            this.txtRefbox.Location = new System.Drawing.Point(12, 132);
            this.txtRefbox.Name = "txtRefbox";
            this.txtRefbox.Size = new System.Drawing.Size(132, 21);
            this.txtRefbox.TabIndex = 39;
            // 
            // btnStartSim
            // 
            this.btnStartSim.Location = new System.Drawing.Point(213, 186);
            this.btnStartSim.Name = "btnStartSim";
            this.btnStartSim.Size = new System.Drawing.Size(138, 34);
            this.btnStartSim.TabIndex = 45;
            this.btnStartSim.Text = "Start Sim";
            this.btnStartSim.UseVisualStyleBackColor = true;
            this.btnStartSim.Click += new System.EventHandler(this.btnStartSim_Click);
            // 
            // lstPlays
            // 
            this.lstPlays.FormattingEnabled = true;
            this.lstPlays.Location = new System.Drawing.Point(12, 313);
            this.lstPlays.Name = "lstPlays";
            this.lstPlays.Size = new System.Drawing.Size(356, 169);
            this.lstPlays.TabIndex = 46;
            this.lstPlays.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lstPlays_ItemCheck);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 297);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 47;
            this.label1.Text = "Plays:";
            // 
            // chkSelectAll
            // 
            this.chkSelectAll.AutoSize = true;
            this.chkSelectAll.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkSelectAll.Location = new System.Drawing.Point(298, 290);
            this.chkSelectAll.Name = "chkSelectAll";
            this.chkSelectAll.Size = new System.Drawing.Size(70, 17);
            this.chkSelectAll.TabIndex = 48;
            this.chkSelectAll.Text = "Select All";
            this.chkSelectAll.UseVisualStyleBackColor = true;
            this.chkSelectAll.CheckedChanged += new System.EventHandler(this.chkSelectAll_CheckedChanged);
            // 
            // lblSimStatus
            // 
            this.lblSimStatus.BackColor = System.Drawing.Color.Red;
            this.lblSimStatus.Location = new System.Drawing.Point(213, 170);
            this.lblSimStatus.Name = "lblSimStatus";
            this.lblSimStatus.Size = new System.Drawing.Size(138, 13);
            this.lblSimStatus.TabIndex = 50;
            this.lblSimStatus.Text = "                 ";
            // 
            // btnStartStopPlayer
            // 
            this.btnStartStopPlayer.Location = new System.Drawing.Point(213, 248);
            this.btnStartStopPlayer.Name = "btnStartStopPlayer";
            this.btnStartStopPlayer.Size = new System.Drawing.Size(138, 21);
            this.btnStartStopPlayer.TabIndex = 51;
            this.btnStartStopPlayer.Text = "Start Simple Player";
            this.btnStartStopPlayer.UseVisualStyleBackColor = true;
            this.btnStartStopPlayer.Click += new System.EventHandler(this.btnStartStopPlayer_Click);
            // 
            // lstSimplePlayer
            // 
            this.lstSimplePlayer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.lstSimplePlayer.FormattingEnabled = true;
            this.lstSimplePlayer.Location = new System.Drawing.Point(12, 248);
            this.lstSimplePlayer.Name = "lstSimplePlayer";
            this.lstSimplePlayer.Size = new System.Drawing.Size(188, 21);
            this.lstSimplePlayer.TabIndex = 52;
            this.lstSimplePlayer.SelectedIndexChanged += new System.EventHandler(this.lstSimplePlayer_SelectedIndexChanged);
            // 
            // lblSimplePlayerStatus
            // 
            this.lblSimplePlayerStatus.BackColor = System.Drawing.Color.Red;
            this.lblSimplePlayerStatus.Location = new System.Drawing.Point(12, 232);
            this.lblSimplePlayerStatus.Name = "lblSimplePlayerStatus";
            this.lblSimplePlayerStatus.Size = new System.Drawing.Size(339, 13);
            this.lblSimplePlayerStatus.TabIndex = 53;
            this.lblSimplePlayerStatus.Text = "                       ";
            // 
            // serialPort
            // 
            this.serialPort.Location = new System.Drawing.Point(100, 80);
            this.serialPort.Name = "serialPort";
            this.serialPort.Size = new System.Drawing.Size(44, 20);
            this.serialPort.TabIndex = 54;
            // 
            // ControlForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(380, 492);
            this.Controls.Add(this.serialPort);
            this.Controls.Add(this.lblSimplePlayerStatus);
            this.Controls.Add(this.lstSimplePlayer);
            this.Controls.Add(this.btnStartStopPlayer);
            this.Controls.Add(this.lblSimStatus);
            this.Controls.Add(this.chkSelectAll);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lstPlays);
            this.Controls.Add(this.btnStartSim);
            this.Controls.Add(this.txtRefbox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.sslVisionHost);
            this.Controls.Add(this.sslVisionStatus);
            this.Controls.Add(this.sslVisionConnect);
            this.Controls.Add(this.lblRefbox);
            this.Controls.Add(this.btnRefbox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.rfcStart);
            this.Controls.Add(this.rfcStatus);
            this.Controls.Add(this.serialConnect);
            this.Controls.Add(this.serialStatus);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.serialHost);
            this.KeyPreview = true;
            this.Name = "ControlForm";
            this.Text = "ControlForm";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox serialHost;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label serialStatus;
        private System.Windows.Forms.Button serialConnect;
        private System.Windows.Forms.Button rfcStart;
        private System.Windows.Forms.Label rfcStatus;
        private System.Windows.Forms.Button btnLogNext;
        private System.Windows.Forms.Button btnLogOpenClose;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnStartStopLogging;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtRobotID;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnRefbox;
        private System.Windows.Forms.Label lblRefbox;
        private System.Windows.Forms.Button sslVisionConnect;
        private System.Windows.Forms.Label sslVisionStatus;
        private System.Windows.Forms.TextBox sslVisionHost;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox txtRefbox;
        private System.Windows.Forms.Button btnStartSim;
        private System.Windows.Forms.CheckedListBox lstPlays;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkSelectAll;
        private System.Windows.Forms.Label lblSimStatus;
        private System.Windows.Forms.Button btnStartStopPlayer;
        private System.Windows.Forms.ComboBox lstSimplePlayer;
        private System.Windows.Forms.Label lblSimplePlayerStatus;
        private System.Windows.Forms.TextBox serialPort;
    }
}