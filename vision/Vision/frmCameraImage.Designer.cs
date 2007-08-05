namespace Vision {
    partial class frmCameraImage {
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
            this.picImage = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.selectionBox1 = new SelectionBox.SelectionBox();
            ((System.ComponentModel.ISupportInitialize)(this.picImage)).BeginInit();
            this.picImage.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // picImage
            // 
            this.picImage.Controls.Add(this.selectionBox1);
            this.picImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picImage.InitialImage = null;
            this.picImage.Location = new System.Drawing.Point(0, 0);
            this.picImage.Margin = new System.Windows.Forms.Padding(0);
            this.picImage.Name = "picImage";
            this.picImage.Size = new System.Drawing.Size(150, 100);
            this.picImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picImage.TabIndex = 0;
            this.picImage.TabStop = false;
            this.picImage.Paint += new System.Windows.Forms.PaintEventHandler(this.picImage_Paint);
            this.picImage.MouseClick += new System.Windows.Forms.MouseEventHandler(this.picImage_MouseClick);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.picImage);
            this.panel1.AutoScroll = true;
            this.panel1.Location = new System.Drawing.Point(2, 3);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1024, 768);
            this.panel1.TabIndex = 5;
            // 
            // selectionBox1
            // 
            this.selectionBox1.Location = new System.Drawing.Point(27, 32);
            this.selectionBox1.Name = "selectionBox1";
            this.selectionBox1.Size = new System.Drawing.Size(81, 49);
            this.selectionBox1.TabIndex = 1;
            // 
            // frmCameraImage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1032, 773);
            this.ControlBox = false;
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmCameraImage";
            this.ShowInTaskbar = false;
            this.Text = "Image from camera";
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.frmCameraImage_KeyPress);
            ((System.ComponentModel.ISupportInitialize)(this.picImage)).EndInit();
            this.picImage.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            //this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.PictureBox picImage;
        private System.Windows.Forms.Panel panel1;
        private SelectionBox.SelectionBox selectionBox1;

    }
}