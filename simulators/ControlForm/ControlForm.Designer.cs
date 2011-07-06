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
            this.label2 = new System.Windows.Forms.Label();
            this.lblControllerStatus = new System.Windows.Forms.Label();
            this.btnController = new System.Windows.Forms.Button();
            this.btnLogNext = new System.Windows.Forms.Button();
            this.btnLogOpenClose = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtRobotID = new System.Windows.Forms.TextBox();
            this.btnStartStopLogging = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.btnRefbox = new System.Windows.Forms.Button();
            this.lblRefboxStatus = new System.Windows.Forms.Label();
            this.btnVision = new System.Windows.Forms.Button();
            this.lblVisionStatus = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.cmbRefboxHost = new System.Windows.Forms.ComboBox();
            this.lstPlays = new System.Windows.Forms.CheckedListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.chkSelectAll = new System.Windows.Forms.CheckBox();
            this.btnStartPlayer = new System.Windows.Forms.Button();
            this.cmbControllerHost = new System.Windows.Forms.ComboBox();
            this.cmbVisionHost = new System.Windows.Forms.ComboBox();
            this.lstPlayers = new System.Windows.Forms.ListBox();
            this.btnStopPlayer = new System.Windows.Forms.Button();
            this.txtPlayerRobotID = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.cmbFieldHalf = new System.Windows.Forms.ComboBox();
            this.reloadConstantsButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(104, 13);
            this.label2.TabIndex = 24;
            this.label2.Text = "Controller (host:port):";
            // 
            // lblControllerStatus
            // 
            this.lblControllerStatus.AutoSize = true;
            this.lblControllerStatus.BackColor = System.Drawing.Color.Red;
            this.lblControllerStatus.Location = new System.Drawing.Point(150, 54);
            this.lblControllerStatus.Name = "lblControllerStatus";
            this.lblControllerStatus.Size = new System.Drawing.Size(76, 13);
            this.lblControllerStatus.TabIndex = 25;
            this.lblControllerStatus.Text = "                       ";
            // 
            // btnController
            // 
            this.btnController.Location = new System.Drawing.Point(150, 70);
            this.btnController.Name = "btnController";
            this.btnController.Size = new System.Drawing.Size(80, 20);
            this.btnController.TabIndex = 5;
            this.btnController.Text = "Connect";
            this.btnController.UseVisualStyleBackColor = true;
            this.btnController.Click += new System.EventHandler(this.btnController_Click);
            // 
            // btnLogNext
            // 
            this.btnLogNext.Location = new System.Drawing.Point(58, 41);
            this.btnLogNext.Name = "btnLogNext";
            this.btnLogNext.Size = new System.Drawing.Size(72, 23);
            this.btnLogNext.TabIndex = 13;
            this.btnLogNext.Text = "Next Entry";
            this.btnLogNext.UseVisualStyleBackColor = true;
            this.btnLogNext.Click += new System.EventHandler(this.btnLogNext_Click);
            // 
            // btnLogOpenClose
            // 
            this.btnLogOpenClose.Location = new System.Drawing.Point(6, 41);
            this.btnLogOpenClose.Name = "btnLogOpenClose";
            this.btnLogOpenClose.Size = new System.Drawing.Size(46, 23);
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
            this.groupBox1.Location = new System.Drawing.Point(243, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(140, 67);
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
            this.btnStartStopLogging.Location = new System.Drawing.Point(87, 15);
            this.btnStartStopLogging.Name = "btnStartStopLogging";
            this.btnStartStopLogging.Size = new System.Drawing.Size(43, 20);
            this.btnStartStopLogging.TabIndex = 14;
            this.btnStartStopLogging.Text = "Start log";
            this.btnStartStopLogging.UseVisualStyleBackColor = true;
            this.btnStartStopLogging.Click += new System.EventHandler(this.btnStartStopLogging_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 96);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(94, 13);
            this.label6.TabIndex = 32;
            this.label6.Text = "Refbox (host:port):";
            // 
            // btnRefbox
            // 
            this.btnRefbox.Location = new System.Drawing.Point(150, 112);
            this.btnRefbox.Name = "btnRefbox";
            this.btnRefbox.Size = new System.Drawing.Size(80, 20);
            this.btnRefbox.TabIndex = 33;
            this.btnRefbox.Text = "Connect";
            this.btnRefbox.UseVisualStyleBackColor = true;
            this.btnRefbox.Click += new System.EventHandler(this.btnRefbox_Click);
            // 
            // lblRefboxStatus
            // 
            this.lblRefboxStatus.AutoSize = true;
            this.lblRefboxStatus.BackColor = System.Drawing.Color.Red;
            this.lblRefboxStatus.Location = new System.Drawing.Point(150, 96);
            this.lblRefboxStatus.Name = "lblRefboxStatus";
            this.lblRefboxStatus.Size = new System.Drawing.Size(76, 13);
            this.lblRefboxStatus.TabIndex = 34;
            this.lblRefboxStatus.Text = "                       ";
            // 
            // btnVision
            // 
            this.btnVision.Location = new System.Drawing.Point(150, 30);
            this.btnVision.Name = "btnVision";
            this.btnVision.Size = new System.Drawing.Size(80, 20);
            this.btnVision.TabIndex = 35;
            this.btnVision.Text = "Connect";
            this.btnVision.UseVisualStyleBackColor = true;
            this.btnVision.Click += new System.EventHandler(this.btnVision_Click);
            // 
            // lblVisionStatus
            // 
            this.lblVisionStatus.AutoSize = true;
            this.lblVisionStatus.BackColor = System.Drawing.Color.Red;
            this.lblVisionStatus.Location = new System.Drawing.Point(150, 13);
            this.lblVisionStatus.Name = "lblVisionStatus";
            this.lblVisionStatus.Size = new System.Drawing.Size(76, 13);
            this.lblVisionStatus.TabIndex = 36;
            this.lblVisionStatus.Text = "                       ";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 13);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(88, 13);
            this.label7.TabIndex = 38;
            this.label7.Text = "Vision (host:port):";
            // 
            // cmbRefboxHost
            // 
            this.cmbRefboxHost.FormattingEnabled = true;
            this.cmbRefboxHost.Items.AddRange(new object[] {
            "224.5.92.12:10100",
            "224.5.23.1:10001"});
            this.cmbRefboxHost.Location = new System.Drawing.Point(12, 112);
            this.cmbRefboxHost.Name = "cmbRefboxHost";
            this.cmbRefboxHost.Size = new System.Drawing.Size(132, 21);
            this.cmbRefboxHost.TabIndex = 39;
            // 
            // lstPlays
            // 
            this.lstPlays.FormattingEnabled = true;
            this.lstPlays.Location = new System.Drawing.Point(8, 57);
            this.lstPlays.Name = "lstPlays";
            this.lstPlays.Size = new System.Drawing.Size(356, 169);
            this.lstPlays.TabIndex = 46;
            this.lstPlays.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lstPlays_ItemCheck);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 47;
            this.label1.Text = "Plays:";
            // 
            // chkSelectAll
            // 
            this.chkSelectAll.AutoSize = true;
            this.chkSelectAll.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkSelectAll.Location = new System.Drawing.Point(294, 38);
            this.chkSelectAll.Name = "chkSelectAll";
            this.chkSelectAll.Size = new System.Drawing.Size(70, 17);
            this.chkSelectAll.TabIndex = 48;
            this.chkSelectAll.Text = "Select All";
            this.chkSelectAll.UseVisualStyleBackColor = true;
            this.chkSelectAll.CheckedChanged += new System.EventHandler(this.chkSelectAll_CheckedChanged);
            // 
            // btnStartPlayer
            // 
            this.btnStartPlayer.Location = new System.Drawing.Point(251, 147);
            this.btnStartPlayer.Name = "btnStartPlayer";
            this.btnStartPlayer.Size = new System.Drawing.Size(132, 43);
            this.btnStartPlayer.TabIndex = 51;
            this.btnStartPlayer.Text = "Start Player";
            this.btnStartPlayer.UseVisualStyleBackColor = true;
            this.btnStartPlayer.Click += new System.EventHandler(this.btnStartPlayer_Click);
            // 
            // cmbControllerHost
            // 
            this.cmbControllerHost.FormattingEnabled = true;
            this.cmbControllerHost.Items.AddRange(new object[] {
            "localhost:50100"});
            this.cmbControllerHost.Location = new System.Drawing.Point(12, 69);
            this.cmbControllerHost.Name = "cmbControllerHost";
            this.cmbControllerHost.Size = new System.Drawing.Size(132, 21);
            this.cmbControllerHost.TabIndex = 54;
            // 
            // cmbVisionHost
            // 
            this.cmbVisionHost.FormattingEnabled = true;
            this.cmbVisionHost.Items.AddRange(new object[] {
            "224.5.23.2:10002"});
            this.cmbVisionHost.Location = new System.Drawing.Point(12, 29);
            this.cmbVisionHost.Name = "cmbVisionHost";
            this.cmbVisionHost.Size = new System.Drawing.Size(132, 21);
            this.cmbVisionHost.TabIndex = 55;
            // 
            // lstPlayers
            // 
            this.lstPlayers.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstPlayers.FormattingEnabled = true;
            this.lstPlayers.Location = new System.Drawing.Point(12, 147);
            this.lstPlayers.Name = "lstPlayers";
            this.lstPlayers.Size = new System.Drawing.Size(233, 82);
            this.lstPlayers.TabIndex = 56;
            this.lstPlayers.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lstPlayers_DrawItem);
            this.lstPlayers.SelectedIndexChanged += new System.EventHandler(this.lstPlayers_SelectedIndexChanged_1);
            // 
            // btnStopPlayer
            // 
            this.btnStopPlayer.Location = new System.Drawing.Point(251, 196);
            this.btnStopPlayer.Name = "btnStopPlayer";
            this.btnStopPlayer.Size = new System.Drawing.Size(132, 33);
            this.btnStopPlayer.TabIndex = 57;
            this.btnStopPlayer.Text = "Stop Player";
            this.btnStopPlayer.UseVisualStyleBackColor = true;
            this.btnStopPlayer.Click += new System.EventHandler(this.btnStopPlayer_Click);
            // 
            // txtPlayerRobotID
            // 
            this.txtPlayerRobotID.Location = new System.Drawing.Point(294, 13);
            this.txtPlayerRobotID.Name = "txtPlayerRobotID";
            this.txtPlayerRobotID.Size = new System.Drawing.Size(50, 20);
            this.txtPlayerRobotID.TabIndex = 58;
            this.txtPlayerRobotID.Text = "0";
            this.txtPlayerRobotID.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtPlayerRobotID.TextChanged += new System.EventHandler(this.txtSimplePlayerRobotID_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(236, 19);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 59;
            this.label4.Text = "Robot ID:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.cmbFieldHalf);
            this.groupBox2.Controls.Add(this.lstPlays);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.txtPlayerRobotID);
            this.groupBox2.Controls.Add(this.chkSelectAll);
            this.groupBox2.Location = new System.Drawing.Point(12, 235);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(371, 234);
            this.groupBox2.TabIndex = 60;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Player";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 16);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(54, 13);
            this.label8.TabIndex = 63;
            this.label8.Text = "Field Half:";
            // 
            // cmbFieldHalf
            // 
            this.cmbFieldHalf.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFieldHalf.FormattingEnabled = true;
            this.cmbFieldHalf.Location = new System.Drawing.Point(66, 13);
            this.cmbFieldHalf.Name = "cmbFieldHalf";
            this.cmbFieldHalf.Size = new System.Drawing.Size(121, 21);
            this.cmbFieldHalf.TabIndex = 61;
            this.cmbFieldHalf.SelectedIndexChanged += new System.EventHandler(this.cmbFieldHalf_SelectedIndexChanged);
            // 
            // reloadConstantsButton
            // 
            this.reloadConstantsButton.Location = new System.Drawing.Point(251, 96);
            this.reloadConstantsButton.Name = "reloadConstantsButton";
            this.reloadConstantsButton.Size = new System.Drawing.Size(107, 23);
            this.reloadConstantsButton.TabIndex = 18;
            this.reloadConstantsButton.Text = "Reload Constants";
            this.reloadConstantsButton.UseVisualStyleBackColor = true;
            this.reloadConstantsButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // ControlForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(395, 476);
            this.Controls.Add(this.reloadConstantsButton);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.btnStopPlayer);
            this.Controls.Add(this.lstPlayers);
            this.Controls.Add(this.cmbVisionHost);
            this.Controls.Add(this.cmbControllerHost);
            this.Controls.Add(this.btnStartPlayer);
            this.Controls.Add(this.cmbRefboxHost);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.lblVisionStatus);
            this.Controls.Add(this.btnVision);
            this.Controls.Add(this.lblRefboxStatus);
            this.Controls.Add(this.btnRefbox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnController);
            this.Controls.Add(this.lblControllerStatus);
            this.Controls.Add(this.label2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.Name = "ControlForm";
            this.Text = "ControlForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ControlForm_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblControllerStatus;
        private System.Windows.Forms.Button btnController;
        private System.Windows.Forms.Button btnLogNext;
        private System.Windows.Forms.Button btnLogOpenClose;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnStartStopLogging;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtRobotID;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnRefbox;
        private System.Windows.Forms.Label lblRefboxStatus;
        private System.Windows.Forms.Button btnVision;
        private System.Windows.Forms.Label lblVisionStatus;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cmbRefboxHost;
        private System.Windows.Forms.CheckedListBox lstPlays;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkSelectAll;
        private System.Windows.Forms.Button btnStartPlayer;
        private System.Windows.Forms.ComboBox cmbControllerHost;
        private System.Windows.Forms.ComboBox cmbVisionHost;
        private System.Windows.Forms.ListBox lstPlayers;
        private System.Windows.Forms.Button btnStopPlayer;
        private System.Windows.Forms.TextBox txtPlayerRobotID;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cmbFieldHalf;
        private System.Windows.Forms.Button reloadConstantsButton;
    }
}