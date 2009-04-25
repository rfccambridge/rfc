namespace Robocup.MotionControl
{
    partial class PlannerTester
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
            this.checkBoxDisableMovement = new System.Windows.Forms.CheckBox();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.plannerChooseBox});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(860, 25);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // plannerChooseBox
            // 
            this.plannerChooseBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
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
            // checkBoxDisableMovement
            // 
            this.checkBoxDisableMovement.AutoSize = true;
            this.checkBoxDisableMovement.Location = new System.Drawing.Point(289, 4);
            this.checkBoxDisableMovement.Name = "checkBoxDisableMovement";
            this.checkBoxDisableMovement.Size = new System.Drawing.Size(114, 17);
            this.checkBoxDisableMovement.TabIndex = 2;
            this.checkBoxDisableMovement.Text = "Disable Movement";
            this.checkBoxDisableMovement.UseVisualStyleBackColor = true;
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Right;
            this.propertyGrid1.Location = new System.Drawing.Point(690, 25);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(170, 541);
            this.propertyGrid1.TabIndex = 3;
            // 
            // PlannerTester
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(860, 566);
            this.Controls.Add(this.propertyGrid1);
            this.Controls.Add(this.checkBoxDisableMovement);
            this.Controls.Add(this.checkBoxDebugDrawing);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "PlannerTester";
            this.Text = "Form1";
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.RRTTester_MouseUp);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.RRTTester_Paint);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.RRTTester_MouseClick);
            this.Activated += new System.EventHandler(this.RRTTester_Activated);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.RRTTester_MouseDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.RRTTester_KeyPress);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.RRTTester_MouseMove);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripComboBox plannerChooseBox;
        private System.Windows.Forms.CheckBox checkBoxDebugDrawing;
        private System.Windows.Forms.CheckBox checkBoxDisableMovement;
        private System.Windows.Forms.PropertyGrid propertyGrid1;

    }
}

