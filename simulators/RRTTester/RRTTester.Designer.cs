namespace Robocup.RRT
{
    partial class RRTTester
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.plannerChooseBox = new System.Windows.Forms.ToolStripComboBox();
            this.checkBoxDebugDrawing = new System.Windows.Forms.CheckBox();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.plannerChooseBox});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(692, 25);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // plannerChooseBox
            // 
            this.plannerChooseBox.Name = "plannerChooseBox";
            this.plannerChooseBox.Size = new System.Drawing.Size(121, 21);
            this.plannerChooseBox.SelectedIndexChanged += new System.EventHandler(this.navigatorChooseBox_SelectedIndexChanged);
            // 
            // checkBoxDebugDrawing
            // 
            this.checkBoxDebugDrawing.AutoSize = true;
            this.checkBoxDebugDrawing.Checked = true;
            this.checkBoxDebugDrawing.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxDebugDrawing.Location = new System.Drawing.Point(443, 4);
            this.checkBoxDebugDrawing.Name = "checkBoxDebugDrawing";
            this.checkBoxDebugDrawing.Size = new System.Drawing.Size(100, 17);
            this.checkBoxDebugDrawing.TabIndex = 1;
            this.checkBoxDebugDrawing.Text = "Debug Drawing";
            this.checkBoxDebugDrawing.UseVisualStyleBackColor = true;
            // 
            // RRTTester
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(692, 566);
            this.Controls.Add(this.checkBoxDebugDrawing);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "RRTTester";
            this.Text = "Form1";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.RRTTester_Paint);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.RRTTester_MouseClick);
            this.Activated += new System.EventHandler(this.RRTTester_Activated);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.RRTTester_MouseUp);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.RRTTester_KeyPress);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.RRTTester_MouseMove);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.RRTTester_MouseDown);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripComboBox plannerChooseBox;
        private System.Windows.Forms.CheckBox checkBoxDebugDrawing;

    }
}

