namespace JoystickSample
{
    partial class frmMain
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
            this.components = new System.ComponentModel.Container();
            this.flpAxes = new System.Windows.Forms.FlowLayoutPanel();
            this.tmrUpdateStick = new System.Windows.Forms.Timer(this.components);
            this.flpButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.udPort = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.udPort)).BeginInit();
            this.SuspendLayout();
            // 
            // flpAxes
            // 
            this.flpAxes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.flpAxes.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpAxes.Location = new System.Drawing.Point(12, 39);
            this.flpAxes.Name = "flpAxes";
            this.flpAxes.Size = new System.Drawing.Size(242, 318);
            this.flpAxes.TabIndex = 0;
            // 
            // tmrUpdateStick
            // 
            this.tmrUpdateStick.Tick += new System.EventHandler(this.tmrUpdateStick_Tick);
            // 
            // flpButtons
            // 
            this.flpButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.flpButtons.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpButtons.Location = new System.Drawing.Point(260, 39);
            this.flpButtons.Name = "flpButtons";
            this.flpButtons.Size = new System.Drawing.Size(427, 318);
            this.flpButtons.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Port COM:";
            // 
            // udPort
            // 
            this.udPort.Location = new System.Drawing.Point(81, 12);
            this.udPort.Name = "udPort";
            this.udPort.Size = new System.Drawing.Size(38, 20);
            this.udPort.TabIndex = 4;
            this.udPort.Value = new decimal(new int[] {
            9,
            0,
            0,
            0});
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(699, 369);
            this.Controls.Add(this.udPort);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.flpButtons);
            this.Controls.Add(this.flpAxes);
            this.Name = "frmMain";
            this.Text = "Joystick Test Form";
            this.Load += new System.EventHandler(this.frmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.udPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flpAxes;
        private System.Windows.Forms.Timer tmrUpdateStick;
        private System.Windows.Forms.FlowLayoutPanel flpButtons;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown udPort;
    }
}

