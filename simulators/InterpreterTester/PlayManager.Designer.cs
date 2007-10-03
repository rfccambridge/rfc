namespace InterpreterTester
{
    partial class PlayManager
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
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.buttonAddRight = new System.Windows.Forms.Button();
            this.buttonAddLeft = new System.Windows.Forms.Button();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.listBoxLeft = new System.Windows.Forms.ListBox();
            this.listBoxRight = new System.Windows.Forms.ListBox();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(113, 13);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(50, 13);
            label1.TabIndex = 0;
            label1.Text = "LeftPlays";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(417, 13);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(62, 13);
            label2.TabIndex = 1;
            label2.Text = "Right plays:";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.buttonAddRight);
            this.splitContainer1.Panel1.Controls.Add(this.buttonAddLeft);
            this.splitContainer1.Panel1.Controls.Add(label2);
            this.splitContainer1.Panel1.Controls.Add(label1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(586, 421);
            this.splitContainer1.SplitterDistance = 92;
            this.splitContainer1.TabIndex = 0;
            // 
            // buttonAddRight
            // 
            this.buttonAddRight.Location = new System.Drawing.Point(409, 46);
            this.buttonAddRight.Name = "buttonAddRight";
            this.buttonAddRight.Size = new System.Drawing.Size(75, 23);
            this.buttonAddRight.TabIndex = 3;
            this.buttonAddRight.Text = "Add play";
            this.buttonAddRight.UseVisualStyleBackColor = true;
            this.buttonAddRight.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // buttonAddLeft
            // 
            this.buttonAddLeft.Location = new System.Drawing.Point(99, 45);
            this.buttonAddLeft.Name = "buttonAddLeft";
            this.buttonAddLeft.Size = new System.Drawing.Size(75, 23);
            this.buttonAddLeft.TabIndex = 2;
            this.buttonAddLeft.Text = "Add Play";
            this.buttonAddLeft.UseVisualStyleBackColor = true;
            this.buttonAddLeft.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.listBoxLeft);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.listBoxRight);
            this.splitContainer2.Size = new System.Drawing.Size(586, 325);
            this.splitContainer2.SplitterDistance = 300;
            this.splitContainer2.TabIndex = 0;
            // 
            // listBoxLeft
            // 
            this.listBoxLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxLeft.FormattingEnabled = true;
            this.listBoxLeft.Location = new System.Drawing.Point(0, 0);
            this.listBoxLeft.Name = "listBoxLeft";
            this.listBoxLeft.Size = new System.Drawing.Size(300, 316);
            this.listBoxLeft.TabIndex = 0;
            this.listBoxLeft.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listBox_MouseDoubleClick);
            this.listBoxLeft.SelectedIndexChanged += new System.EventHandler(this.listBox_SelectedIndexChanged);
            this.listBoxLeft.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBox_KeyDown);
            // 
            // listBoxRight
            // 
            this.listBoxRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxRight.FormattingEnabled = true;
            this.listBoxRight.Location = new System.Drawing.Point(0, 0);
            this.listBoxRight.Name = "listBoxRight";
            this.listBoxRight.Size = new System.Drawing.Size(282, 316);
            this.listBoxRight.TabIndex = 0;
            this.listBoxRight.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listBox_MouseDoubleClick);
            this.listBoxRight.SelectedIndexChanged += new System.EventHandler(this.listBox_SelectedIndexChanged);
            this.listBoxRight.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBox_KeyDown);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "txt";
            this.saveFileDialog1.Filter = "Robocup Plays|*.txt";
            // 
            // PlayManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(586, 421);
            this.Controls.Add(this.splitContainer1);
            this.Name = "PlayManager";
            this.Text = "PlayManager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PlayManager_FormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ListBox listBoxLeft;
        private System.Windows.Forms.ListBox listBoxRight;
        private System.Windows.Forms.Button buttonAddRight;
        private System.Windows.Forms.Button buttonAddLeft;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;

    }
}