namespace JoystickSample
{
    partial class JoystickAxis
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblAxis = new System.Windows.Forms.Label();
            this.trkPos = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.trkPos)).BeginInit();
            this.SuspendLayout();
            // 
            // lblAxis
            // 
            this.lblAxis.AutoSize = true;
            this.lblAxis.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAxis.Location = new System.Drawing.Point(3, 0);
            this.lblAxis.Name = "lblAxis";
            this.lblAxis.Size = new System.Drawing.Size(43, 13);
            this.lblAxis.TabIndex = 0;
            this.lblAxis.Text = "Axis N";
            // 
            // trkPos
            // 
            this.trkPos.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.trkPos.LargeChange = 3120;
            this.trkPos.Location = new System.Drawing.Point(3, 16);
            this.trkPos.Maximum = 65535;
            this.trkPos.Name = "trkPos";
            this.trkPos.Size = new System.Drawing.Size(190, 45);
            this.trkPos.SmallChange = 1500;
            this.trkPos.TabIndex = 1;
            this.trkPos.TickFrequency = 3120;
            this.trkPos.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            this.trkPos.Value = 32500;
            // 
            // JoystickAxis
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.trkPos);
            this.Controls.Add(this.lblAxis);
            this.Name = "JoystickAxis";
            this.Size = new System.Drawing.Size(196, 60);
            ((System.ComponentModel.ISupportInitialize)(this.trkPos)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblAxis;
        private System.Windows.Forms.TrackBar trkPos;
    }
}
