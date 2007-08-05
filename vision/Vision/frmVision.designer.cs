namespace Vision {
    partial class frmVision {
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
            this.dlgSaveBlobInfo = new System.Windows.Forms.SaveFileDialog();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.rbCamera2 = new System.Windows.Forms.RadioButton();
            this.rbCamera1 = new System.Windows.Forms.RadioButton();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportBlobInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cameraImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorCalibrationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.worldImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gameObjectsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.calibrationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveTsaiPointsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadTsaiPointsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dlgSaveTsaiPoints = new System.Windows.Forms.SaveFileDialog();
            this.dlgLoadTsaiPoints = new System.Windows.Forms.OpenFileDialog();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.btnDisplayLostBalls = new System.Windows.Forms.Button();
            this.chkToggleVisionTest = new System.Windows.Forms.CheckBox();
            this.groupBox3.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // dlgSaveBlobInfo
            // 
            this.dlgSaveBlobInfo.Filter = "Blob Info (*.bi) | *.bi";
            this.dlgSaveBlobInfo.RestoreDirectory = true;
            this.dlgSaveBlobInfo.Title = "Export Blob Info to file";
            this.dlgSaveBlobInfo.FileOk += new System.ComponentModel.CancelEventHandler(this.dlgSaveBlobInfo_FileOk);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.rbCamera2);
            this.groupBox3.Controls.Add(this.rbCamera1);
            this.groupBox3.Location = new System.Drawing.Point(12, 37);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(230, 41);
            this.groupBox3.TabIndex = 32;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Camera";
            // 
            // rbCamera2
            // 
            this.rbCamera2.AutoSize = true;
            this.rbCamera2.Location = new System.Drawing.Point(141, 19);
            this.rbCamera2.Name = "rbCamera2";
            this.rbCamera2.Size = new System.Drawing.Size(62, 17);
            this.rbCamera2.TabIndex = 38;
            this.rbCamera2.Text = "Cam #2";
            this.rbCamera2.UseVisualStyleBackColor = true;
            this.rbCamera2.CheckedChanged += new System.EventHandler(this.rbCamera2_CheckedChanged);
            // 
            // rbCamera1
            // 
            this.rbCamera1.AutoSize = true;
            this.rbCamera1.Checked = true;
            this.rbCamera1.Location = new System.Drawing.Point(36, 19);
            this.rbCamera1.Name = "rbCamera1";
            this.rbCamera1.Size = new System.Drawing.Size(62, 17);
            this.rbCamera1.TabIndex = 37;
            this.rbCamera1.TabStop = true;
            this.rbCamera1.Text = "Cam #1";
            this.rbCamera1.UseVisualStyleBackColor = true;
            this.rbCamera1.CheckedChanged += new System.EventHandler(this.rbCamera1_CheckedChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.calibrationToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(259, 24);
            this.menuStrip1.TabIndex = 33;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportBlobInfoToolStripMenuItem,
            this.toolStripSeparator2,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exportBlobInfoToolStripMenuItem
            // 
            this.exportBlobInfoToolStripMenuItem.Name = "exportBlobInfoToolStripMenuItem";
            this.exportBlobInfoToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.exportBlobInfoToolStripMenuItem.Text = "Export Blob Info...";
            this.exportBlobInfoToolStripMenuItem.Click += new System.EventHandler(this.exportBlobInfoToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(172, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cameraImageToolStripMenuItem,
            this.colorCalibrationToolStripMenuItem,
            this.worldImageToolStripMenuItem,
            this.gameObjectsToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(41, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // cameraImageToolStripMenuItem
            // 
            this.cameraImageToolStripMenuItem.Name = "cameraImageToolStripMenuItem";
            this.cameraImageToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.cameraImageToolStripMenuItem.Text = "Image from camera";
            this.cameraImageToolStripMenuItem.Click += new System.EventHandler(this.cameraImageToolStripMenuItem_Click);
            // 
            // colorCalibrationToolStripMenuItem
            // 
            this.colorCalibrationToolStripMenuItem.Name = "colorCalibrationToolStripMenuItem";
            this.colorCalibrationToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.colorCalibrationToolStripMenuItem.Text = "Color Calibration";
            this.colorCalibrationToolStripMenuItem.Click += new System.EventHandler(this.colorCalibrationToolStripMenuItem_Click);
            // 
            // worldImageToolStripMenuItem
            // 
            this.worldImageToolStripMenuItem.Name = "worldImageToolStripMenuItem";
            this.worldImageToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.worldImageToolStripMenuItem.Text = "World Image";
            this.worldImageToolStripMenuItem.Click += new System.EventHandler(this.worldImageToolStripMenuItem_Click);
            // 
            // gameObjectsToolStripMenuItem
            // 
            this.gameObjectsToolStripMenuItem.Name = "gameObjectsToolStripMenuItem";
            this.gameObjectsToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.gameObjectsToolStripMenuItem.Text = "Game Objects";
            // 
            // calibrationToolStripMenuItem
            // 
            this.calibrationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveTsaiPointsToolStripMenuItem,
            this.loadTsaiPointsToolStripMenuItem});
            this.calibrationToolStripMenuItem.Name = "calibrationToolStripMenuItem";
            this.calibrationToolStripMenuItem.Size = new System.Drawing.Size(70, 20);
            this.calibrationToolStripMenuItem.Text = "Calibration";
            // 
            // saveTsaiPointsToolStripMenuItem
            // 
            this.saveTsaiPointsToolStripMenuItem.Name = "saveTsaiPointsToolStripMenuItem";
            this.saveTsaiPointsToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.saveTsaiPointsToolStripMenuItem.Text = "Save Tsai Points...";
            this.saveTsaiPointsToolStripMenuItem.Click += new System.EventHandler(this.saveTsaiPointsToolStripMenuItem_Click);
            // 
            // loadTsaiPointsToolStripMenuItem
            // 
            this.loadTsaiPointsToolStripMenuItem.Name = "loadTsaiPointsToolStripMenuItem";
            this.loadTsaiPointsToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.loadTsaiPointsToolStripMenuItem.Text = "Load Tsai Points...";
            this.loadTsaiPointsToolStripMenuItem.Click += new System.EventHandler(this.loadTsaiPointsToolStripMenuItem_Click);
            // 
            // dlgSaveTsaiPoints
            // 
            this.dlgSaveTsaiPoints.FileName = "tsai_points";
            this.dlgSaveTsaiPoints.Filter = "Tsai Points (*.tps) | *.tps";
            this.dlgSaveTsaiPoints.RestoreDirectory = true;
            this.dlgSaveTsaiPoints.Title = "Save Tsai Points";
            this.dlgSaveTsaiPoints.FileOk += new System.ComponentModel.CancelEventHandler(this.dlgSaveTsaiPoints_FileOk);
            // 
            // dlgLoadTsaiPoints
            // 
            this.dlgLoadTsaiPoints.Filter = "Tsai Points (*.tps) | *.tps";
            this.dlgLoadTsaiPoints.RestoreDirectory = true;
            this.dlgLoadTsaiPoints.Title = "Load Tsai Points";
            this.dlgLoadTsaiPoints.FileOk += new System.ComponentModel.CancelEventHandler(this.dlgLoadTsaiPoints_FileOk);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.btnDisplayLostBalls);
            this.groupBox5.Controls.Add(this.chkToggleVisionTest);
            this.groupBox5.Location = new System.Drawing.Point(12, 84);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(230, 54);
            this.groupBox5.TabIndex = 37;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Vision Test";
            // 
            // btnDisplayLostBalls
            // 
            this.btnDisplayLostBalls.Enabled = false;
            this.btnDisplayLostBalls.Location = new System.Drawing.Point(101, 19);
            this.btnDisplayLostBalls.Name = "btnDisplayLostBalls";
            this.btnDisplayLostBalls.Size = new System.Drawing.Size(92, 23);
            this.btnDisplayLostBalls.TabIndex = 39;
            this.btnDisplayLostBalls.Text = "ShowLostBalls";
            this.btnDisplayLostBalls.UseVisualStyleBackColor = true;
            this.btnDisplayLostBalls.Click += new System.EventHandler(this.btnDisplayLostBalls_Click);
            // 
            // chkToggleVisionTest
            // 
            this.chkToggleVisionTest.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkToggleVisionTest.AutoSize = true;
            this.chkToggleVisionTest.Enabled = false;
            this.chkToggleVisionTest.Location = new System.Drawing.Point(9, 19);
            this.chkToggleVisionTest.Name = "chkToggleVisionTest";
            this.chkToggleVisionTest.Size = new System.Drawing.Size(69, 23);
            this.chkToggleVisionTest.TabIndex = 37;
            this.chkToggleVisionTest.Text = "Vision Test";
            this.chkToggleVisionTest.UseVisualStyleBackColor = true;
            this.chkToggleVisionTest.CheckedChanged += new System.EventHandler(this.chkToggleVisionTest_CheckedChanged);
            // 
            // frmVision
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(259, 156);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "frmVision";
            this.Text = "Vision";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmVision_FormClosing);
            this.Load += new System.EventHandler(this.frmRAWImage_Load);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SaveFileDialog dlgSaveBlobInfo;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cameraImageToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem colorCalibrationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem calibrationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveTsaiPointsToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog dlgSaveTsaiPoints;
        private System.Windows.Forms.ToolStripMenuItem loadTsaiPointsToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog dlgLoadTsaiPoints;
        private System.Windows.Forms.ToolStripMenuItem exportBlobInfoToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem worldImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gameObjectsToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.RadioButton rbCamera2;
        private System.Windows.Forms.RadioButton rbCamera1;
        private System.Windows.Forms.CheckBox chkToggleVisionTest;
        private System.Windows.Forms.Button btnDisplayLostBalls;
    }
}

