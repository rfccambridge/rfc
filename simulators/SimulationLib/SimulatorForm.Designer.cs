namespace Robocup.Simulation
{
    partial class SimulatorForm
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
            this.label8 = new System.Windows.Forms.Label();
            this.txtSimRefereeHost = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtSimVisionHost = new System.Windows.Forms.TextBox();
            this.btnSimStartStop = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.lblSimListenStatus = new System.Windows.Forms.Label();
            this.txtSimCmdPort = new System.Windows.Forms.TextBox();
            this.btnReset = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Enabled = false;
            this.label8.Location = new System.Drawing.Point(39, 62);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(86, 13);
            this.label8.TabIndex = 61;
            this.label8.Text = "Referee (ip:port):";
            // 
            // txtSimRefereeHost
            // 
            this.txtSimRefereeHost.Enabled = false;
            this.txtSimRefereeHost.Location = new System.Drawing.Point(51, 78);
            this.txtSimRefereeHost.Name = "txtSimRefereeHost";
            this.txtSimRefereeHost.Size = new System.Drawing.Size(117, 20);
            this.txtSimRefereeHost.TabIndex = 60;
            this.txtSimRefereeHost.Text = "224.5.92.12:10001";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(39, 109);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(76, 13);
            this.label5.TabIndex = 59;
            this.label5.Text = "Vision (ip:port):";
            // 
            // txtSimVisionHost
            // 
            this.txtSimVisionHost.Location = new System.Drawing.Point(51, 125);
            this.txtSimVisionHost.Name = "txtSimVisionHost";
            this.txtSimVisionHost.Size = new System.Drawing.Size(117, 20);
            this.txtSimVisionHost.TabIndex = 58;
            this.txtSimVisionHost.Text = "224.5.23.2:10002";
            // 
            // btnSimStartStop
            // 
            this.btnSimStartStop.Location = new System.Drawing.Point(42, 180);
            this.btnSimStartStop.Name = "btnSimStartStop";
            this.btnSimStartStop.Size = new System.Drawing.Size(126, 39);
            this.btnSimStartStop.TabIndex = 54;
            this.btnSimStartStop.Text = "Start Sim";
            this.btnSimStartStop.UseVisualStyleBackColor = true;
            this.btnSimStartStop.Click += new System.EventHandler(this.btnSimStartStop_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(39, 35);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 13);
            this.label4.TabIndex = 57;
            this.label4.Text = "Cmd port:";
            // 
            // lblSimListenStatus
            // 
            this.lblSimListenStatus.BackColor = System.Drawing.Color.Red;
            this.lblSimListenStatus.Location = new System.Drawing.Point(42, 164);
            this.lblSimListenStatus.Name = "lblSimListenStatus";
            this.lblSimListenStatus.Size = new System.Drawing.Size(126, 13);
            this.lblSimListenStatus.TabIndex = 55;
            this.lblSimListenStatus.Text = "                 ";
            // 
            // txtSimCmdPort
            // 
            this.txtSimCmdPort.Location = new System.Drawing.Point(97, 32);
            this.txtSimCmdPort.Name = "txtSimCmdPort";
            this.txtSimCmdPort.Size = new System.Drawing.Size(71, 20);
            this.txtSimCmdPort.TabIndex = 56;
            this.txtSimCmdPort.Text = "50100";
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(42, 236);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(126, 23);
            this.btnReset.TabIndex = 62;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // SimulatorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(215, 271);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtSimRefereeHost);
            this.Controls.Add(this.btnSimStartStop);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtSimCmdPort);
            this.Controls.Add(this.txtSimVisionHost);
            this.Controls.Add(this.lblSimListenStatus);
            this.Controls.Add(this.label4);
            this.Name = "SimulatorForm";
            this.Text = "Simulator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SimulatorForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtSimRefereeHost;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtSimVisionHost;
        private System.Windows.Forms.Button btnSimStartStop;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblSimListenStatus;
        private System.Windows.Forms.TextBox txtSimCmdPort;
        private System.Windows.Forms.Button btnReset;
    }
}