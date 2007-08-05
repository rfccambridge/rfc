namespace Robotics.Commander
{
    partial class RemoteControl
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.OpenCOM = new System.Windows.Forms.Button();
            this.statusLabel = new System.Windows.Forms.Label();
            this.reloadMotor = new System.Windows.Forms.Button();
            this.lblID = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 13);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(373, 173);
            this.textBox1.TabIndex = 0;
            this.textBox1.KeyUp += new System.Windows.Forms.KeyEventHandler(this.From1_KeyUp);
            this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.From1_KeyDown);
            // 
            // OpenCOM
            // 
            this.OpenCOM.Location = new System.Drawing.Point(410, 13);
            this.OpenCOM.Name = "OpenCOM";
            this.OpenCOM.Size = new System.Drawing.Size(118, 54);
            this.OpenCOM.TabIndex = 1;
            this.OpenCOM.Text = "Open COM";
            this.OpenCOM.UseVisualStyleBackColor = true;
            this.OpenCOM.Click += new System.EventHandler(this.toggleSettings);
            this.OpenCOM.KeyUp += new System.Windows.Forms.KeyEventHandler(this.From1_KeyUp);
            this.OpenCOM.KeyDown += new System.Windows.Forms.KeyEventHandler(this.From1_KeyDown);
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 42F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.statusLabel.Location = new System.Drawing.Point(199, 211);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(329, 64);
            this.statusLabel.TabIndex = 3;
            this.statusLabel.Text = "pressed key";
            // 
            // reloadMotor
            // 
            this.reloadMotor.Location = new System.Drawing.Point(410, 120);
            this.reloadMotor.Name = "reloadMotor";
            this.reloadMotor.Size = new System.Drawing.Size(118, 66);
            this.reloadMotor.TabIndex = 44;
            this.reloadMotor.TabStop = false;
            this.reloadMotor.Text = "Reload Motor Corrections";
            this.reloadMotor.UseVisualStyleBackColor = true;
            this.reloadMotor.Click += new System.EventHandler(this.button3_Click);
            // 
            // lblID
            // 
            this.lblID.AutoSize = true;
            this.lblID.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblID.Location = new System.Drawing.Point(12, 236);
            this.lblID.Name = "lblID";
            this.lblID.Size = new System.Drawing.Size(153, 33);
            this.lblID.TabIndex = 45;
            this.lblID.Text = "RobotID: 0";
            // 
            // RemoteControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(558, 307);
            this.Controls.Add(this.lblID);
            this.Controls.Add(this.reloadMotor);
            this.Controls.Add(this.OpenCOM);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.textBox1);
            this.Name = "RemoteControl";
            this.Text = "Form1";
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.From1_KeyUp);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.From1_KeyDown);
            this.Load += new System.EventHandler(this.RemoteControl_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button OpenCOM;
        public System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Button reloadMotor;
        public System.Windows.Forms.Label lblID;
    }
}

