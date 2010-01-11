namespace JoystickSample
{
    partial class Axis
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
            this.lblAxisName = new System.Windows.Forms.Label();
            this.tbAxisPos = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.tbAxisPos)).BeginInit();
            this.SuspendLayout();
            // 
            // lblAxisName
            // 
            this.lblAxisName.AutoSize = true;
            this.lblAxisName.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblAxisName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAxisName.Location = new System.Drawing.Point(0, 0);
            this.lblAxisName.Name = "lblAxisName";
            this.lblAxisName.Size = new System.Drawing.Size(118, 13);
            this.lblAxisName.TabIndex = 0;
            this.lblAxisName.Text = "Axis: #  Pos: 32767";
            // 
            // tbAxisPos
            // 
            this.tbAxisPos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbAxisPos.LargeChange = 10000;
            this.tbAxisPos.Location = new System.Drawing.Point(0, 13);
            this.tbAxisPos.Maximum = 65535;
            this.tbAxisPos.Name = "tbAxisPos";
            this.tbAxisPos.Size = new System.Drawing.Size(179, 36);
            this.tbAxisPos.SmallChange = 5000;
            this.tbAxisPos.TabIndex = 1;
            this.tbAxisPos.TickFrequency = 5000;
            this.tbAxisPos.Value = 32767;
            // 
            // Axis
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tbAxisPos);
            this.Controls.Add(this.lblAxisName);
            this.Name = "Axis";
            this.Size = new System.Drawing.Size(179, 49);
            ((System.ComponentModel.ISupportInitialize)(this.tbAxisPos)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblAxisName;
        private System.Windows.Forms.TrackBar tbAxisPos;
    }
}
