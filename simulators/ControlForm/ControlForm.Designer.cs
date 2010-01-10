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
            this.label4 = new System.Windows.Forms.Label();
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
            this.label5 = new System.Windows.Forms.Label();
            this.lblSimStatus = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // serialHost
            // 
            this.serialHost.Location = new System.Drawing.Point(12, 80);
            this.serialHost.Name = "serialHost";
            this.serialHost.Size = new System.Drawing.Size(132, 20);
            this.serialHost.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 13);
            this.label2.TabIndex = 24;
            this.label2.Text = "Serial:";
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
            this.rfcStart.Size = new System.Drawing.Size(214, 34);
            this.rfcStart.TabIndex = 7;
            this.rfcStart.Text = "Start";
            this.rfcStart.UseVisualStyleBackColor = true;
            this.rfcStart.Click += new System.EventHandler(this.rfcStart_Click);
            // 
            // rfcStatus
            // 
            this.rfcStatus.AutoSize = true;
            this.rfcStatus.BackColor = System.Drawing.Color.Red;
            this.rfcStatus.Location = new System.Drawing.Point(150, 170);
            this.rfcStatus.Name = "rfcStatus";
            this.rfcStatus.Size = new System.Drawing.Size(76, 13);
            this.rfcStatus.TabIndex = 29;
            this.rfcStatus.Text = "                       ";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 170);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 13);
            this.label4.TabIndex = 28;
            this.label4.Text = "RFCSystem";
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
            this.groupBox1.Location = new System.Drawing.Point(255, 12);
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
            this.btnStartSim.Location = new System.Drawing.Point(257, 186);
            this.btnStartSim.Name = "btnStartSim";
            this.btnStartSim.Size = new System.Drawing.Size(111, 34);
            this.btnStartSim.TabIndex = 45;
            this.btnStartSim.Text = "Start Sim";
            this.btnStartSim.UseVisualStyleBackColor = true;
            this.btnStartSim.Click += new System.EventHandler(this.btnStartSim_Click);
            // 
            // lstPlays
            // 
            this.lstPlays.FormattingEnabled = true;
            this.lstPlays.Location = new System.Drawing.Point(12, 253);
            this.lstPlays.Name = "lstPlays";
            this.lstPlays.Size = new System.Drawing.Size(356, 169);
            this.lstPlays.TabIndex = 46;
            this.lstPlays.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lstPlays_ItemCheck);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 237);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 47;
            this.label1.Text = "Plays:";
            // 
            // chkSelectAll
            // 
            this.chkSelectAll.AutoSize = true;
            this.chkSelectAll.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkSelectAll.Location = new System.Drawing.Point(298, 233);
            this.chkSelectAll.Name = "chkSelectAll";
            this.chkSelectAll.Size = new System.Drawing.Size(70, 17);
            this.chkSelectAll.TabIndex = 48;
            this.chkSelectAll.Text = "Select All";
            this.chkSelectAll.UseVisualStyleBackColor = true;
            this.chkSelectAll.CheckedChanged += new System.EventHandler(this.chkSelectAll_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(254, 170);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(27, 13);
            this.label5.TabIndex = 49;
            this.label5.Text = "Sim:";
            // 
            // lblSimStatus
            // 
            this.lblSimStatus.AutoSize = true;
            this.lblSimStatus.BackColor = System.Drawing.Color.Red;
            this.lblSimStatus.Location = new System.Drawing.Point(287, 170);
            this.lblSimStatus.Name = "lblSimStatus";
            this.lblSimStatus.Size = new System.Drawing.Size(76, 13);
            this.lblSimStatus.TabIndex = 50;
            this.lblSimStatus.Text = "                       ";
            // 
            // ControlForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(380, 434);
            this.Controls.Add(this.lblSimStatus);
            this.Controls.Add(this.label5);
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
            this.Controls.Add(this.label4);
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
        private System.Windows.Forms.Label label4;
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
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblSimStatus;
    }
}