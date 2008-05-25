namespace SoccerSim
{
    partial class SoccerSim
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
            this.runningStatus = new System.Windows.Forms.Label();
            this.arrowStatus = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.visionStatus = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // runningStatus
            // 
            this.runningStatus.AutoSize = true;
            this.runningStatus.BackColor = System.Drawing.Color.Red;
            this.runningStatus.Location = new System.Drawing.Point(700, 24);
            this.runningStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.runningStatus.Name = "runningStatus";
            this.runningStatus.Size = new System.Drawing.Size(67, 13);
            this.runningStatus.TabIndex = 0;
            this.runningStatus.Text = "Not Running";
            // 
            // arrowStatus
            // 
            this.arrowStatus.AutoSize = true;
            this.arrowStatus.BackColor = System.Drawing.Color.Red;
            this.arrowStatus.Location = new System.Drawing.Point(700, 37);
            this.arrowStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.arrowStatus.Name = "arrowStatus";
            this.arrowStatus.Size = new System.Drawing.Size(56, 13);
            this.arrowStatus.TabIndex = 1;
            this.arrowStatus.Text = "No Arrows";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(679, 24);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(16, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "r -";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(676, 37);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(19, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "a -";
            // 
            // visionStatus
            // 
            this.visionStatus.AutoSize = true;
            this.visionStatus.BackColor = System.Drawing.Color.Red;
            this.visionStatus.Location = new System.Drawing.Point(700, 50);
            this.visionStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.visionStatus.Name = "visionStatus";
            this.visionStatus.Size = new System.Drawing.Size(52, 13);
            this.visionStatus.TabIndex = 4;
            this.visionStatus.Text = "No Vision";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(676, 50);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(19, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "v -";
            // 
            // SoccerSim
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(792, 616);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.visionStatus);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.arrowStatus);
            this.Controls.Add(this.runningStatus);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "SoccerSim";
            this.Text = "RFC Soccer Simulator";
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.SoccerSim_MouseUp);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.SoccerSim_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SoccerSim_MouseDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.SoccerSim_KeyPress);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SoccerSim_FormClosing);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SoccerSim_MouseMove);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label runningStatus;
        private System.Windows.Forms.Label arrowStatus;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label visionStatus;
        private System.Windows.Forms.Label label4;
    }
}

