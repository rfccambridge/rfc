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
            this.SuspendLayout();
            // 
            // SoccerSim
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(953, 623);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "SoccerSim";
            this.Text = "RFC Soccer Simulator";
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.SoccerSim_MouseUp);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.SoccerSim_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SoccerSim_MouseDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.SoccerSim_KeyPress);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SoccerSim_FormClosing);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SoccerSim_MouseMove);
            this.ResumeLayout(false);

        }

        #endregion
    }
}

