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
            this.visionHost = new System.Windows.Forms.TextBox();
            this.serialHost = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.visionStatus = new System.Windows.Forms.Label();
            this.serialStatus = new System.Windows.Forms.Label();
            this.serialConnect = new System.Windows.Forms.Button();
            this.visionConnect = new System.Windows.Forms.Button();
            this.rfcStart = new System.Windows.Forms.Button();
            this.rfcStatus = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.refboxConnect = new System.Windows.Forms.Button();
            this.refboxStatus = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // visionHost
            // 
            this.visionHost.Location = new System.Drawing.Point(21, 34);
            this.visionHost.Name = "visionHost";
            this.visionHost.Size = new System.Drawing.Size(70, 20);
            this.visionHost.TabIndex = 0;
            this.visionHost.Text = "localhost";
            // 
            // serialHost
            // 
            this.serialHost.Location = new System.Drawing.Point(21, 83);
            this.serialHost.Name = "serialHost";
            this.serialHost.Size = new System.Drawing.Size(70, 20);
            this.serialHost.TabIndex = 1;
            this.serialHost.Text = "localhost";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Vision: ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Serial:";
            // 
            // visionStatus
            // 
            this.visionStatus.AutoSize = true;
            this.visionStatus.BackColor = System.Drawing.Color.Red;
            this.visionStatus.Location = new System.Drawing.Point(111, 18);
            this.visionStatus.Name = "visionStatus";
            this.visionStatus.Size = new System.Drawing.Size(70, 13);
            this.visionStatus.TabIndex = 4;
            this.visionStatus.Text = "                     ";
            // 
            // serialStatus
            // 
            this.serialStatus.AutoSize = true;
            this.serialStatus.BackColor = System.Drawing.Color.Red;
            this.serialStatus.Location = new System.Drawing.Point(110, 64);
            this.serialStatus.Name = "serialStatus";
            this.serialStatus.Size = new System.Drawing.Size(70, 13);
            this.serialStatus.TabIndex = 5;
            this.serialStatus.Text = "                     ";
            // 
            // serialConnect
            // 
            this.serialConnect.Location = new System.Drawing.Point(105, 82);
            this.serialConnect.Name = "serialConnect";
            this.serialConnect.Size = new System.Drawing.Size(78, 21);
            this.serialConnect.TabIndex = 6;
            this.serialConnect.Text = "Connect";
            this.serialConnect.UseVisualStyleBackColor = true;
            this.serialConnect.Click += new System.EventHandler(this.serialConnect_Click);
            // 
            // visionConnect
            // 
            this.visionConnect.Location = new System.Drawing.Point(106, 34);
            this.visionConnect.Name = "visionConnect";
            this.visionConnect.Size = new System.Drawing.Size(78, 20);
            this.visionConnect.TabIndex = 7;
            this.visionConnect.Text = "Connect";
            this.visionConnect.UseVisualStyleBackColor = true;
            this.visionConnect.Click += new System.EventHandler(this.visionConnect_Click);
            // 
            // rfcStart
            // 
            this.rfcStart.Location = new System.Drawing.Point(105, 185);
            this.rfcStart.Name = "rfcStart";
            this.rfcStart.Size = new System.Drawing.Size(78, 21);
            this.rfcStart.TabIndex = 12;
            this.rfcStart.Text = "Start";
            this.rfcStart.UseVisualStyleBackColor = true;
            this.rfcStart.Click += new System.EventHandler(this.rfcStart_Click);
            // 
            // rfcStatus
            // 
            this.rfcStatus.AutoSize = true;
            this.rfcStatus.BackColor = System.Drawing.Color.Red;
            this.rfcStatus.Location = new System.Drawing.Point(110, 167);
            this.rfcStatus.Name = "rfcStatus";
            this.rfcStatus.Size = new System.Drawing.Size(70, 13);
            this.rfcStatus.TabIndex = 11;
            this.rfcStatus.Text = "                     ";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(18, 167);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "RFCSystem";
            // 
            // refboxConnect
            // 
            this.refboxConnect.Location = new System.Drawing.Point(105, 133);
            this.refboxConnect.Name = "refboxConnect";
            this.refboxConnect.Size = new System.Drawing.Size(78, 21);
            this.refboxConnect.TabIndex = 15;
            this.refboxConnect.Text = "Connect";
            this.refboxConnect.UseVisualStyleBackColor = true;
            // 
            // refboxStatus
            // 
            this.refboxStatus.AutoSize = true;
            this.refboxStatus.BackColor = System.Drawing.Color.Red;
            this.refboxStatus.Location = new System.Drawing.Point(110, 115);
            this.refboxStatus.Name = "refboxStatus";
            this.refboxStatus.Size = new System.Drawing.Size(70, 13);
            this.refboxStatus.TabIndex = 14;
            this.refboxStatus.Text = "                     ";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(18, 115);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(42, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "RefBox";
            // 
            // ControlForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(201, 228);
            this.Controls.Add(this.refboxConnect);
            this.Controls.Add(this.refboxStatus);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.rfcStart);
            this.Controls.Add(this.rfcStatus);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.visionConnect);
            this.Controls.Add(this.serialConnect);
            this.Controls.Add(this.serialStatus);
            this.Controls.Add(this.visionStatus);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.serialHost);
            this.Controls.Add(this.visionHost);
            this.Name = "ControlForm";
            this.Text = "ControlForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox visionHost;
        private System.Windows.Forms.TextBox serialHost;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label visionStatus;
        private System.Windows.Forms.Label serialStatus;
        private System.Windows.Forms.Button serialConnect;
        private System.Windows.Forms.Button visionConnect;
        private System.Windows.Forms.Button rfcStart;
        private System.Windows.Forms.Label rfcStatus;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button refboxConnect;
        private System.Windows.Forms.Label refboxStatus;
        private System.Windows.Forms.Label label6;
    }
}