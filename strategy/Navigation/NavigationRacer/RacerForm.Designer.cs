namespace NavigationRacer
{
    partial class RacerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RacerForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.calculateReferenceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.navigatorChooseBox = new System.Windows.Forms.ComboBox();
            this.textBoxTestLength = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxDebugDrawing = new System.Windows.Forms.CheckBox();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.setupToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(585, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem,
            this.openToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("saveToolStripMenuItem.Image")));
            this.saveToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.saveToolStripMenuItem.Text = "&Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripMenuItem.Image")));
            this.openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // setupToolStripMenuItem
            // 
            this.setupToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.calculateReferenceToolStripMenuItem,
            this.testAllToolStripMenuItem});
            this.setupToolStripMenuItem.Name = "setupToolStripMenuItem";
            this.setupToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
            this.setupToolStripMenuItem.Text = "&Race";
            // 
            // calculateReferenceToolStripMenuItem
            // 
            this.calculateReferenceToolStripMenuItem.Name = "calculateReferenceToolStripMenuItem";
            this.calculateReferenceToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.calculateReferenceToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.calculateReferenceToolStripMenuItem.Text = "Calculate &Reference";
            this.calculateReferenceToolStripMenuItem.Click += new System.EventHandler(this.calculateReferenceToolStripMenuItem_Click);
            // 
            // testAllToolStripMenuItem
            // 
            this.testAllToolStripMenuItem.Name = "testAllToolStripMenuItem";
            this.testAllToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.testAllToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.testAllToolStripMenuItem.Text = "&Test all";
            this.testAllToolStripMenuItem.Click += new System.EventHandler(this.testAllToolStripMenuItem_Click);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.DefaultExt = "txt";
            this.saveFileDialog.FileName = "test";
            this.saveFileDialog.Filter = "Test state (*.txt) | *.txt";
            this.saveFileDialog.Title = "Save state";
            this.saveFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.saveFileDialog_FileOk);
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog";
            this.openFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog_FileOk);
            // 
            // navigatorChooseBox
            // 
            this.navigatorChooseBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.navigatorChooseBox.FormattingEnabled = true;
            this.navigatorChooseBox.Location = new System.Drawing.Point(91, 1);
            this.navigatorChooseBox.Name = "navigatorChooseBox";
            this.navigatorChooseBox.Size = new System.Drawing.Size(121, 21);
            this.navigatorChooseBox.TabIndex = 2;
            this.navigatorChooseBox.SelectedIndexChanged += new System.EventHandler(this.navigatorChooseBox_SelectedIndexChanged);
            // 
            // textBoxTestLength
            // 
            this.textBoxTestLength.Location = new System.Drawing.Point(340, 2);
            this.textBoxTestLength.Name = "textBoxTestLength";
            this.textBoxTestLength.Size = new System.Drawing.Size(47, 20);
            this.textBoxTestLength.TabIndex = 3;
            this.textBoxTestLength.Text = "2";
            this.textBoxTestLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxTestLength.TextChanged += new System.EventHandler(this.textBoxTestLength_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Location = new System.Drawing.Point(237, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Duration of test (s):";
            // 
            // checkBoxDebugDrawing
            // 
            this.checkBoxDebugDrawing.AutoSize = true;
            this.checkBoxDebugDrawing.Checked = true;
            this.checkBoxDebugDrawing.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxDebugDrawing.Location = new System.Drawing.Point(413, 3);
            this.checkBoxDebugDrawing.Name = "checkBoxDebugDrawing";
            this.checkBoxDebugDrawing.Size = new System.Drawing.Size(100, 17);
            this.checkBoxDebugDrawing.TabIndex = 5;
            this.checkBoxDebugDrawing.Text = "Debug Drawing";
            this.checkBoxDebugDrawing.UseVisualStyleBackColor = true;
            this.checkBoxDebugDrawing.CheckedChanged += new System.EventHandler(this.checkBoxDebugDrawing_CheckedChanged);
            // 
            // RacerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(585, 511);
            this.Controls.Add(this.checkBoxDebugDrawing);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxTestLength);
            this.Controls.Add(this.navigatorChooseBox);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "RacerForm";
            this.Text = "Form1";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.RacerForm_Paint);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.RacerForm_MouseClick);
            this.Activated += new System.EventHandler(this.RacerForm_Activated);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.RacerForm_MouseUp);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.RacerForm_KeyPress);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.RacerForm_MouseMove);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.RacerForm_MouseDown);
            this.Load += new System.EventHandler(this.RacerForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.ToolStripMenuItem setupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem calculateReferenceToolStripMenuItem;
        private System.Windows.Forms.ComboBox navigatorChooseBox;
        private System.Windows.Forms.TextBox textBoxTestLength;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripMenuItem testAllToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBoxDebugDrawing;
    }
}

