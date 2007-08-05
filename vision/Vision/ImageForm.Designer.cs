namespace Vision {
    partial class ImageForm {
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
            this.imagePanel = new System.Windows.Forms.Panel();
            this.imagePicBox = new System.Windows.Forms.PictureBox();
            this.regionSelBox = new SelectionBox.SelectionBox();
            this.stStatus = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.imagePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imagePicBox)).BeginInit();
            this.imagePicBox.SuspendLayout();
            this.stStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // imagePanel
            // 
            this.imagePanel.AutoScroll = true;
            this.imagePanel.Controls.Add(this.imagePicBox);
            this.imagePanel.Location = new System.Drawing.Point(5, 5);
            this.imagePanel.Name = "imagePanel";
            this.imagePanel.Size = new System.Drawing.Size(228, 202);
            this.imagePanel.TabIndex = 0;
            // 
            // imagePicBox
            // 
            this.imagePicBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.imagePicBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.imagePicBox.Controls.Add(this.regionSelBox);
            this.imagePicBox.Location = new System.Drawing.Point(0, 0);
            this.imagePicBox.Name = "imagePicBox";
            this.imagePicBox.Size = new System.Drawing.Size(197, 168);
            this.imagePicBox.TabIndex = 0;
            this.imagePicBox.TabStop = false;
            this.imagePicBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.imagePicBox_MouseClick);
            // 
            // regionSelBox
            //             
            this.regionSelBox.Location = new System.Drawing.Point(-1, -1);
            this.regionSelBox.Name = "regionSelBox";
            this.regionSelBox.RegionLocation = new System.Drawing.Point(0, 0);
            this.regionSelBox.RegionSize = new System.Drawing.Size(153, 136);
            this.regionSelBox.Resizable = true;
            this.regionSelBox.Size = new System.Drawing.Size(155, 138);
            this.regionSelBox.TabIndex = 1;
            // 
            // stStatus
            // 
            this.stStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.stStatus.Location = new System.Drawing.Point(0, 244);
            this.stStatus.Name = "stStatus";
            this.stStatus.Size = new System.Drawing.Size(292, 22);
            this.stStatus.TabIndex = 1;
            this.stStatus.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(48, 17);
            this.lblStatus.Text = "lblStatus";
            // 
            // ImageForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Controls.Add(this.stStatus);
            this.Controls.Add(this.imagePanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ImageForm";
            this.Text = "Image";
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ImageForm_KeyPress);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ImageForm_FormClosing);
            this.imagePanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.imagePicBox)).EndInit();
            this.imagePicBox.ResumeLayout(false);
            this.stStatus.ResumeLayout(false);
            this.stStatus.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel imagePanel;
        private SelectionBox.SelectionBox regionSelBox;
        private System.Windows.Forms.PictureBox imagePicBox;
        private System.Windows.Forms.StatusStrip stStatus;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
    }
}