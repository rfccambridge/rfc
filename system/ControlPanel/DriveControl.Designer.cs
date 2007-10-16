//-----------------------------------------------------------------------
//  This file is part of the Microsoft Robotics Studio Code Samples.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: DriveControl.Designer.cs $ $Revision: 4 $
//-----------------------------------------------------------------------

namespace Robotics.ControlPanel
{
    partial class DriveControl
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
            System.Windows.Forms.Label label5;
            System.Windows.Forms.Label label6;
            System.Windows.Forms.Label label7;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label lblref;
            this.cbJoystick = new System.Windows.Forms.ComboBox();
            this.lblX = new System.Windows.Forms.Label();
            this.lblY = new System.Windows.Forms.Label();
            this.lblZ = new System.Windows.Forms.Label();
            this.lblButtons = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkStop = new System.Windows.Forms.CheckBox();
            this.chkDrive = new System.Windows.Forms.CheckBox();
            this.picJoystick = new System.Windows.Forms.PictureBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.toggleRFC = new System.Windows.Forms.Button();
            this.btnConnectVision = new System.Windows.Forms.Button();
            this.btnDisconnectVision = new System.Windows.Forms.Button();
            this.linkDirectory = new System.Windows.Forms.LinkLabel();
            this.lblNode = new System.Windows.Forms.Label();
            this.lstDirectory = new System.Windows.Forms.ListBox();
            this.btnConnectSim = new System.Windows.Forms.Button();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.lblCmd = new System.Windows.Forms.Label();
            this.picField = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.inputBall = new System.Windows.Forms.ComboBox();
            this.inputBlue = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.inputYellow = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.buttonReloadConstants = new System.Windows.Forms.Button();
            this.buttonUnusedConstants = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            lblref = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picJoystick)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picField)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(8, 31);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(55, 17);
            label1.TabIndex = 1;
            label1.Text = "Device:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(45, 62);
            label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(21, 17);
            label5.TabIndex = 5;
            label5.Text = "X:";
            label5.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(45, 82);
            label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(21, 17);
            label6.TabIndex = 6;
            label6.Text = "Y:";
            label6.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(45, 103);
            label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(21, 17);
            label7.TabIndex = 7;
            label7.Text = "Z:";
            label7.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(8, 124);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(60, 17);
            label2.TabIndex = 8;
            label2.Text = "Buttons:";
            label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblref
            // 
            lblref.AutoSize = true;
            lblref.Location = new System.Drawing.Point(16, 549);
            lblref.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblref.Name = "lblref";
            lblref.Size = new System.Drawing.Size(61, 17);
            lblref.TabIndex = 20;
            lblref.Text = "LastRef:";
            lblref.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // cbJoystick
            // 
            this.cbJoystick.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cbJoystick.FormattingEnabled = true;
            this.cbJoystick.Location = new System.Drawing.Point(80, 27);
            this.cbJoystick.Margin = new System.Windows.Forms.Padding(4);
            this.cbJoystick.Name = "cbJoystick";
            this.cbJoystick.Size = new System.Drawing.Size(171, 24);
            this.cbJoystick.TabIndex = 0;
            // 
            // lblX
            // 
            this.lblX.Location = new System.Drawing.Point(80, 62);
            this.lblX.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblX.Name = "lblX";
            this.lblX.Size = new System.Drawing.Size(47, 16);
            this.lblX.TabIndex = 2;
            this.lblX.Text = "0";
            this.lblX.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblY
            // 
            this.lblY.Location = new System.Drawing.Point(80, 82);
            this.lblY.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblY.Name = "lblY";
            this.lblY.Size = new System.Drawing.Size(47, 16);
            this.lblY.TabIndex = 3;
            this.lblY.Text = "0";
            this.lblY.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblZ
            // 
            this.lblZ.Location = new System.Drawing.Point(80, 103);
            this.lblZ.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblZ.Name = "lblZ";
            this.lblZ.Size = new System.Drawing.Size(47, 16);
            this.lblZ.TabIndex = 4;
            this.lblZ.Text = "0";
            this.lblZ.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblButtons
            // 
            this.lblButtons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblButtons.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblButtons.Location = new System.Drawing.Point(84, 124);
            this.lblButtons.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblButtons.Name = "lblButtons";
            this.lblButtons.Size = new System.Drawing.Size(168, 16);
            this.lblButtons.TabIndex = 9;
            this.lblButtons.Text = "O";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkStop);
            this.groupBox1.Controls.Add(this.chkDrive);
            this.groupBox1.Controls.Add(this.picJoystick);
            this.groupBox1.Controls.Add(label1);
            this.groupBox1.Controls.Add(this.lblButtons);
            this.groupBox1.Controls.Add(this.cbJoystick);
            this.groupBox1.Controls.Add(label2);
            this.groupBox1.Controls.Add(this.lblX);
            this.groupBox1.Controls.Add(label7);
            this.groupBox1.Controls.Add(this.lblY);
            this.groupBox1.Controls.Add(label6);
            this.groupBox1.Controls.Add(this.lblZ);
            this.groupBox1.Controls.Add(label5);
            this.groupBox1.Location = new System.Drawing.Point(16, 15);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(260, 186);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Direct Input Device";
            // 
            // chkStop
            // 
            this.chkStop.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkStop.Location = new System.Drawing.Point(149, 144);
            this.chkStop.Margin = new System.Windows.Forms.Padding(4);
            this.chkStop.Name = "chkStop";
            this.chkStop.Size = new System.Drawing.Size(103, 30);
            this.chkStop.TabIndex = 12;
            this.chkStop.Text = "Stop";
            this.chkStop.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkStop.UseVisualStyleBackColor = true;
            // 
            // chkDrive
            // 
            this.chkDrive.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkDrive.Checked = true;
            this.chkDrive.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDrive.Location = new System.Drawing.Point(12, 144);
            this.chkDrive.Margin = new System.Windows.Forms.Padding(4);
            this.chkDrive.Name = "chkDrive";
            this.chkDrive.Size = new System.Drawing.Size(101, 30);
            this.chkDrive.TabIndex = 11;
            this.chkDrive.Text = "Drive";
            this.chkDrive.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkDrive.UseVisualStyleBackColor = true;
            // 
            // picJoystick
            // 
            this.picJoystick.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.picJoystick.Location = new System.Drawing.Point(172, 60);
            this.picJoystick.Margin = new System.Windows.Forms.Padding(4);
            this.picJoystick.Name = "picJoystick";
            this.picJoystick.Size = new System.Drawing.Size(65, 60);
            this.picJoystick.TabIndex = 10;
            this.picJoystick.TabStop = false;
            this.picJoystick.MouseLeave += new System.EventHandler(this.picJoystick_MouseLeave);
            this.picJoystick.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picJoystick_MouseMove);
            this.picJoystick.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picJoystick_MouseUp);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.toggleRFC);
            this.groupBox2.Controls.Add(this.btnConnectVision);
            this.groupBox2.Controls.Add(this.btnDisconnectVision);
            this.groupBox2.Controls.Add(this.linkDirectory);
            this.groupBox2.Controls.Add(this.lblNode);
            this.groupBox2.Controls.Add(this.lstDirectory);
            this.groupBox2.Controls.Add(this.btnConnectSim);
            this.groupBox2.Location = new System.Drawing.Point(16, 330);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox2.Size = new System.Drawing.Size(260, 463);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Remote Node";
            // 
            // toggleRFC
            // 
            this.toggleRFC.Location = new System.Drawing.Point(16, 138);
            this.toggleRFC.Margin = new System.Windows.Forms.Padding(4);
            this.toggleRFC.Name = "toggleRFC";
            this.toggleRFC.Size = new System.Drawing.Size(97, 43);
            this.toggleRFC.TabIndex = 11;
            this.toggleRFC.Text = "Start RFC";
            this.toggleRFC.UseVisualStyleBackColor = true;
            this.toggleRFC.Click += new System.EventHandler(this.startRFC_Click);
            // 
            // btnConnectVision
            // 
            this.btnConnectVision.Location = new System.Drawing.Point(116, 86);
            this.btnConnectVision.Margin = new System.Windows.Forms.Padding(4);
            this.btnConnectVision.Name = "btnConnectVision";
            this.btnConnectVision.Size = new System.Drawing.Size(97, 44);
            this.btnConnectVision.TabIndex = 10;
            this.btnConnectVision.Text = "Connect Local Vision";
            this.btnConnectVision.UseVisualStyleBackColor = true;
            this.btnConnectVision.Click += new System.EventHandler(this.btnConnectVision_Click);
            // 
            // btnDisconnectVision
            // 
            this.btnDisconnectVision.Location = new System.Drawing.Point(116, 138);
            this.btnDisconnectVision.Margin = new System.Windows.Forms.Padding(4);
            this.btnDisconnectVision.Name = "btnDisconnectVision";
            this.btnDisconnectVision.Size = new System.Drawing.Size(97, 43);
            this.btnDisconnectVision.TabIndex = 9;
            this.btnDisconnectVision.Text = "Connect Remote Vision";
            this.btnDisconnectVision.UseVisualStyleBackColor = true;
            this.btnDisconnectVision.Click += new System.EventHandler(this.btnConnectRemoteVision_Click);
            // 
            // linkDirectory
            // 
            this.linkDirectory.AutoSize = true;
            this.linkDirectory.Enabled = false;
            this.linkDirectory.LinkArea = new System.Windows.Forms.LinkArea(8, 9);
            this.linkDirectory.Location = new System.Drawing.Point(9, 231);
            this.linkDirectory.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkDirectory.Name = "linkDirectory";
            this.linkDirectory.Size = new System.Drawing.Size(110, 20);
            this.linkDirectory.TabIndex = 8;
            this.linkDirectory.TabStop = true;
            this.linkDirectory.Text = "Service Directory:";
            this.linkDirectory.UseCompatibleTextRendering = true;
            // 
            // lblNode
            // 
            this.lblNode.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lblNode.AutoEllipsis = true;
            this.lblNode.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblNode.Location = new System.Drawing.Point(36, 252);
            this.lblNode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblNode.Name = "lblNode";
            this.lblNode.Size = new System.Drawing.Size(177, 23);
            this.lblNode.TabIndex = 7;
            // 
            // lstDirectory
            // 
            this.lstDirectory.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lstDirectory.FormattingEnabled = true;
            this.lstDirectory.ItemHeight = 16;
            this.lstDirectory.Location = new System.Drawing.Point(36, 306);
            this.lstDirectory.Margin = new System.Windows.Forms.Padding(4);
            this.lstDirectory.Name = "lstDirectory";
            this.lstDirectory.Size = new System.Drawing.Size(176, 148);
            this.lstDirectory.TabIndex = 5;
            // 
            // btnConnectSim
            // 
            this.btnConnectSim.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnConnectSim.Location = new System.Drawing.Point(16, 86);
            this.btnConnectSim.Margin = new System.Windows.Forms.Padding(4);
            this.btnConnectSim.Name = "btnConnectSim";
            this.btnConnectSim.Size = new System.Drawing.Size(97, 44);
            this.btnConnectSim.TabIndex = 4;
            this.btnConnectSim.Text = "Connect Sim";
            this.btnConnectSim.UseVisualStyleBackColor = true;
            this.btnConnectSim.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.Filter = "Xml log file|*.log;*.xml|All files|*.*";
            this.saveFileDialog.Title = "Log File";
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(lblref);
            this.groupBox7.Controls.Add(this.lblCmd);
            this.groupBox7.Controls.Add(this.picField);
            this.groupBox7.Location = new System.Drawing.Point(284, 15);
            this.groupBox7.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox7.Size = new System.Drawing.Size(597, 579);
            this.groupBox7.TabIndex = 16;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Field";
            // 
            // lblCmd
            // 
            this.lblCmd.Location = new System.Drawing.Point(77, 549);
            this.lblCmd.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCmd.Name = "lblCmd";
            this.lblCmd.Size = new System.Drawing.Size(96, 16);
            this.lblCmd.TabIndex = 19;
            this.lblCmd.Text = "<None>";
            this.lblCmd.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // picField
            // 
            this.picField.InitialImage = null;
            this.picField.Location = new System.Drawing.Point(20, 59);
            this.picField.Margin = new System.Windows.Forms.Padding(4);
            this.picField.Name = "picField";
            this.picField.Size = new System.Drawing.Size(551, 474);
            this.picField.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picField.TabIndex = 0;
            this.picField.TabStop = false;
            this.picField.MouseLeave += new System.EventHandler(this.picField_MouseLeave);
            this.picField.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picField_MouseDown);
            this.picField.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picField_MouseMove);
            this.picField.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picField_MouseUp);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 42.05128F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 57.94872F));
            this.tableLayoutPanel1.Controls.Add(this.inputBall, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.inputBlue, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label10, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label9, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.inputYellow, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label8, 0, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(16, 219);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(260, 103);
            this.tableLayoutPanel1.TabIndex = 17;
            // 
            // inputBall
            // 
            this.inputBall.FormattingEnabled = true;
            this.inputBall.Items.AddRange(new object[] {
            "Simulation",
            "Vision",
            "Replay"});
            this.inputBall.Location = new System.Drawing.Point(113, 72);
            this.inputBall.Margin = new System.Windows.Forms.Padding(4);
            this.inputBall.MaxDropDownItems = 3;
            this.inputBall.Name = "inputBall";
            this.inputBall.Size = new System.Drawing.Size(143, 24);
            this.inputBall.TabIndex = 7;
            this.inputBall.Text = "Vision";
            this.inputBall.SelectionChangeCommitted += new System.EventHandler(this.inputChanged);
            // 
            // inputBlue
            // 
            this.inputBlue.FormattingEnabled = true;
            this.inputBlue.Items.AddRange(new object[] {
            "Simulation",
            "Vision",
            "Replay"});
            this.inputBlue.Location = new System.Drawing.Point(113, 38);
            this.inputBlue.Margin = new System.Windows.Forms.Padding(4);
            this.inputBlue.MaxDropDownItems = 3;
            this.inputBlue.Name = "inputBlue";
            this.inputBlue.Size = new System.Drawing.Size(143, 24);
            this.inputBlue.TabIndex = 6;
            this.inputBlue.Text = "Vision";
            this.inputBlue.SelectionChangeCommitted += new System.EventHandler(this.inputChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(4, 34);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(36, 17);
            this.label10.TabIndex = 5;
            this.label10.Text = "Blue";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(4, 0);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(48, 17);
            this.label9.TabIndex = 4;
            this.label9.Text = "Yellow";
            // 
            // inputYellow
            // 
            this.inputYellow.FormattingEnabled = true;
            this.inputYellow.Items.AddRange(new object[] {
            "Simulation",
            "Vision",
            "Replay"});
            this.inputYellow.Location = new System.Drawing.Point(113, 4);
            this.inputYellow.Margin = new System.Windows.Forms.Padding(4);
            this.inputYellow.MaxDropDownItems = 3;
            this.inputYellow.Name = "inputYellow";
            this.inputYellow.Size = new System.Drawing.Size(143, 24);
            this.inputYellow.TabIndex = 0;
            this.inputYellow.Text = "Vision";
            this.inputYellow.SelectionChangeCommitted += new System.EventHandler(this.inputChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(4, 68);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(31, 17);
            this.label8.TabIndex = 3;
            this.label8.Text = "Ball";
            // 
            // buttonReloadConstants
            // 
            this.buttonReloadConstants.Location = new System.Drawing.Point(455, 664);
            this.buttonReloadConstants.Name = "buttonReloadConstants";
            this.buttonReloadConstants.Size = new System.Drawing.Size(139, 23);
            this.buttonReloadConstants.TabIndex = 18;
            this.buttonReloadConstants.Text = "Reload All";
            this.buttonReloadConstants.UseVisualStyleBackColor = true;
            this.buttonReloadConstants.Click += new System.EventHandler(this.buttonReloadConstants_Click);
            // 
            // buttonUnusedConstants
            // 
            this.buttonUnusedConstants.Location = new System.Drawing.Point(481, 712);
            this.buttonUnusedConstants.Name = "buttonUnusedConstants";
            this.buttonUnusedConstants.Size = new System.Drawing.Size(180, 23);
            this.buttonUnusedConstants.TabIndex = 19;
            this.buttonUnusedConstants.Text = "Show Unused Constants";
            this.buttonUnusedConstants.UseVisualStyleBackColor = true;
            this.buttonUnusedConstants.Click += new System.EventHandler(this.buttonUnusedConstants_Click);
            // 
            // DriveControl
            // 
            this.AcceptButton = this.btnConnectSim;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(907, 807);
            this.Controls.Add(this.buttonUnusedConstants);
            this.Controls.Add(this.buttonReloadConstants);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.groupBox7);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(539, 611);
            this.Name = "DriveControl";
            this.Text = "RFC Control Panel";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DriveControl_FormClosed);
            this.Load += new System.EventHandler(this.DriveControl_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picJoystick)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picField)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cbJoystick;
        private System.Windows.Forms.Label lblX;
        private System.Windows.Forms.Label lblY;
        private System.Windows.Forms.Label lblZ;
        private System.Windows.Forms.Label lblButtons;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        public System.Windows.Forms.ListBox lstDirectory;
        private System.Windows.Forms.Button btnConnectSim;
        private System.Windows.Forms.PictureBox picJoystick;
        private System.Windows.Forms.CheckBox chkStop;
        private System.Windows.Forms.CheckBox chkDrive;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.Label lblNode;
        private System.Windows.Forms.LinkLabel linkDirectory;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.PictureBox picField;
        private System.Windows.Forms.Label lblCmd;
        private System.Windows.Forms.Button btnDisconnectVision;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox inputYellow;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox inputBall;
        private System.Windows.Forms.ComboBox inputBlue;
        private System.Windows.Forms.Button btnConnectVision;
        private System.Windows.Forms.Button toggleRFC;
        private System.Windows.Forms.Button buttonReloadConstants;
        private System.Windows.Forms.Button buttonUnusedConstants;
    }
}
