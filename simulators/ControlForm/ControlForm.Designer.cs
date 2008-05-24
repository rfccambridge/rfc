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
            this.visionTopHost = new System.Windows.Forms.TextBox();
            this.serialHost = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.visionTopStatus = new System.Windows.Forms.Label();
            this.serialStatus = new System.Windows.Forms.Label();
            this.serialConnect = new System.Windows.Forms.Button();
            this.visionTopConnect = new System.Windows.Forms.Button();
            this.rfcStart = new System.Windows.Forms.Button();
            this.rfcStatus = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.refboxConnect = new System.Windows.Forms.Button();
            this.refboxStatus = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.visionBottomConnect = new System.Windows.Forms.Button();
            this.visionBottomStatus = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.visionBottomHost = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // visionTopHost
            // 
            this.visionTopHost.Location = new System.Drawing.Point(28, 42);
            this.visionTopHost.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.visionTopHost.Name = "visionTopHost";
            this.visionTopHost.Size = new System.Drawing.Size(92, 22);
            this.visionTopHost.TabIndex = 0;
            this.visionTopHost.Text = "localhost";
            // 
            // serialHost
            // 
            this.serialHost.Location = new System.Drawing.Point(28, 153);
            this.serialHost.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.serialHost.Name = "serialHost";
            this.serialHost.Size = new System.Drawing.Size(92, 22);
            this.serialHost.TabIndex = 4;
            this.serialHost.Text = "localhost";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 22);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 17);
            this.label1.TabIndex = 20;
            this.label1.Text = "Vision (Top): ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 130);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 17);
            this.label2.TabIndex = 24;
            this.label2.Text = "Serial:";
            // 
            // visionTopStatus
            // 
            this.visionTopStatus.AutoSize = true;
            this.visionTopStatus.BackColor = System.Drawing.Color.Red;
            this.visionTopStatus.Location = new System.Drawing.Point(148, 22);
            this.visionTopStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.visionTopStatus.Name = "visionTopStatus";
            this.visionTopStatus.Size = new System.Drawing.Size(92, 17);
            this.visionTopStatus.TabIndex = 21;
            this.visionTopStatus.Text = "                     ";
            // 
            // serialStatus
            // 
            this.serialStatus.AutoSize = true;
            this.serialStatus.BackColor = System.Drawing.Color.Red;
            this.serialStatus.Location = new System.Drawing.Point(147, 130);
            this.serialStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.serialStatus.Name = "serialStatus";
            this.serialStatus.Size = new System.Drawing.Size(92, 17);
            this.serialStatus.TabIndex = 25;
            this.serialStatus.Text = "                     ";
            // 
            // serialConnect
            // 
            this.serialConnect.Location = new System.Drawing.Point(140, 152);
            this.serialConnect.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.serialConnect.Name = "serialConnect";
            this.serialConnect.Size = new System.Drawing.Size(104, 26);
            this.serialConnect.TabIndex = 5;
            this.serialConnect.Text = "Connect";
            this.serialConnect.UseVisualStyleBackColor = true;
            this.serialConnect.Click += new System.EventHandler(this.serialConnect_Click);
            // 
            // visionTopConnect
            // 
            this.visionTopConnect.Location = new System.Drawing.Point(141, 42);
            this.visionTopConnect.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.visionTopConnect.Name = "visionTopConnect";
            this.visionTopConnect.Size = new System.Drawing.Size(104, 25);
            this.visionTopConnect.TabIndex = 1;
            this.visionTopConnect.Text = "Connect";
            this.visionTopConnect.UseVisualStyleBackColor = true;
            this.visionTopConnect.Click += new System.EventHandler(this.visionConnect_Click);
            // 
            // rfcStart
            // 
            this.rfcStart.Location = new System.Drawing.Point(140, 275);
            this.rfcStart.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rfcStart.Name = "rfcStart";
            this.rfcStart.Size = new System.Drawing.Size(104, 26);
            this.rfcStart.TabIndex = 7;
            this.rfcStart.Text = "Start";
            this.rfcStart.UseVisualStyleBackColor = true;
            this.rfcStart.Click += new System.EventHandler(this.rfcStart_Click);
            // 
            // rfcStatus
            // 
            this.rfcStatus.AutoSize = true;
            this.rfcStatus.BackColor = System.Drawing.Color.Red;
            this.rfcStatus.Location = new System.Drawing.Point(147, 253);
            this.rfcStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.rfcStatus.Name = "rfcStatus";
            this.rfcStatus.Size = new System.Drawing.Size(92, 17);
            this.rfcStatus.TabIndex = 29;
            this.rfcStatus.Text = "                     ";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(24, 253);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(81, 17);
            this.label4.TabIndex = 28;
            this.label4.Text = "RFCSystem";
            // 
            // refboxConnect
            // 
            this.refboxConnect.Location = new System.Drawing.Point(140, 211);
            this.refboxConnect.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.refboxConnect.Name = "refboxConnect";
            this.refboxConnect.Size = new System.Drawing.Size(104, 26);
            this.refboxConnect.TabIndex = 6;
            this.refboxConnect.Text = "Connect";
            this.refboxConnect.UseVisualStyleBackColor = true;
            // 
            // refboxStatus
            // 
            this.refboxStatus.AutoSize = true;
            this.refboxStatus.BackColor = System.Drawing.Color.Red;
            this.refboxStatus.Location = new System.Drawing.Point(147, 189);
            this.refboxStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.refboxStatus.Name = "refboxStatus";
            this.refboxStatus.Size = new System.Drawing.Size(92, 17);
            this.refboxStatus.TabIndex = 27;
            this.refboxStatus.Text = "                     ";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(24, 189);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 17);
            this.label6.TabIndex = 26;
            this.label6.Text = "RefBox";
            // 
            // visionBottomConnect
            // 
            this.visionBottomConnect.Location = new System.Drawing.Point(139, 93);
            this.visionBottomConnect.Margin = new System.Windows.Forms.Padding(4);
            this.visionBottomConnect.Name = "visionBottomConnect";
            this.visionBottomConnect.Size = new System.Drawing.Size(104, 25);
            this.visionBottomConnect.TabIndex = 3;
            this.visionBottomConnect.Text = "Connect";
            this.visionBottomConnect.UseVisualStyleBackColor = true;
            this.visionBottomConnect.Click += new System.EventHandler(this.visionBottomConnect_Click);
            // 
            // visionBottomStatus
            // 
            this.visionBottomStatus.AutoSize = true;
            this.visionBottomStatus.BackColor = System.Drawing.Color.Red;
            this.visionBottomStatus.Location = new System.Drawing.Point(146, 73);
            this.visionBottomStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.visionBottomStatus.Name = "visionBottomStatus";
            this.visionBottomStatus.Size = new System.Drawing.Size(92, 17);
            this.visionBottomStatus.TabIndex = 23;
            this.visionBottomStatus.Text = "                     ";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(22, 73);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(112, 17);
            this.label5.TabIndex = 22;
            this.label5.Text = "Vision (Bottom): ";
            // 
            // visionBottomHost
            // 
            this.visionBottomHost.Location = new System.Drawing.Point(26, 93);
            this.visionBottomHost.Margin = new System.Windows.Forms.Padding(4);
            this.visionBottomHost.Name = "visionBottomHost";
            this.visionBottomHost.Size = new System.Drawing.Size(92, 22);
            this.visionBottomHost.TabIndex = 2;
            this.visionBottomHost.Text = "localhost";
            // 
            // ControlForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(264, 349);
            this.Controls.Add(this.visionBottomConnect);
            this.Controls.Add(this.visionBottomStatus);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.visionBottomHost);
            this.Controls.Add(this.refboxConnect);
            this.Controls.Add(this.refboxStatus);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.rfcStart);
            this.Controls.Add(this.rfcStatus);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.visionTopConnect);
            this.Controls.Add(this.serialConnect);
            this.Controls.Add(this.serialStatus);
            this.Controls.Add(this.visionTopStatus);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.serialHost);
            this.Controls.Add(this.visionTopHost);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "ControlForm";
            this.Text = "ControlForm";
            this.Load += new System.EventHandler(this.ControlForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox visionTopHost;
        private System.Windows.Forms.TextBox serialHost;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label visionTopStatus;
        private System.Windows.Forms.Label serialStatus;
        private System.Windows.Forms.Button serialConnect;
        private System.Windows.Forms.Button visionTopConnect;
        private System.Windows.Forms.Button rfcStart;
        private System.Windows.Forms.Label rfcStatus;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button refboxConnect;
        private System.Windows.Forms.Label refboxStatus;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button visionBottomConnect;
        private System.Windows.Forms.Label visionBottomStatus;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox visionBottomHost;
    }
}