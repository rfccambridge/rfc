namespace RobocupPlays
{
    partial class ShowExpressionsForm
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.conditionBox = new System.Windows.Forms.ListBox();
            this.actionBox = new System.Windows.Forms.ListBox();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
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
            this.splitContainer1.Panel1.Controls.Add(this.conditionBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.actionBox);
            this.splitContainer1.Size = new System.Drawing.Size(459, 340);
            this.splitContainer1.SplitterDistance = 149;
            this.splitContainer1.TabIndex = 0;
            // 
            // conditionBox
            // 
            this.conditionBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.conditionBox.FormattingEnabled = true;
            this.conditionBox.Location = new System.Drawing.Point(0, 0);
            this.conditionBox.Name = "conditionBox";
            this.conditionBox.Size = new System.Drawing.Size(459, 147);
            this.conditionBox.TabIndex = 0;
            this.conditionBox.DoubleClick += new System.EventHandler(this.listboxDoubleClicked);
            this.conditionBox.SelectedIndexChanged += new System.EventHandler(this.listboxSelectedIndexChanged);
            // 
            // actionBox
            // 
            this.actionBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.actionBox.FormattingEnabled = true;
            this.actionBox.Location = new System.Drawing.Point(0, 0);
            this.actionBox.Name = "actionBox";
            this.actionBox.Size = new System.Drawing.Size(459, 186);
            this.actionBox.TabIndex = 0;
            this.actionBox.DoubleClick += new System.EventHandler(this.listboxDoubleClicked);
            this.actionBox.SelectedIndexChanged += new System.EventHandler(this.listboxSelectedIndexChanged);
            // 
            // ShowExpressionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(459, 340);
            this.Controls.Add(this.splitContainer1);
            this.Name = "ShowExpressionsForm";
            this.Text = "ShowExpressionsForm";
            this.Deactivate += new System.EventHandler(this.ShowCommandsForm_Deactivate);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox conditionBox;
        private System.Windows.Forms.ListBox actionBox;
    }
}