namespace Robocup.Core
{
    partial class DebugForm
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
            this.FilterLabel = new System.Windows.Forms.Label();
            this.DebugTextBox = new System.Windows.Forms.RichTextBox();
            this.RobotSelectNone = new System.Windows.Forms.Button();
            this.RobotSelectAll = new System.Windows.Forms.Button();
            this.KeywordSelectAll = new System.Windows.Forms.Button();
            this.KeywordSelectNone = new System.Windows.Forms.Button();
            this.DomainSelectAll = new System.Windows.Forms.Button();
            this.DomainSelectNone = new System.Windows.Forms.Button();
            this.KeywordSelector = new System.Windows.Forms.CheckedListBox();
            this.DomainSelector = new System.Windows.Forms.CheckedListBox();
            this.RobotSelector = new System.Windows.Forms.CheckedListBox();
            this.testbutton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // FilterLabel
            // 
            this.FilterLabel.AutoSize = true;
            this.FilterLabel.Location = new System.Drawing.Point(24, 18);
            this.FilterLabel.Name = "FilterLabel";
            this.FilterLabel.Size = new System.Drawing.Size(182, 13);
            this.FilterLabel.TabIndex = 6;
            this.FilterLabel.Text = "Domain, Robot and Keyword Filtering";
            // 
            // DebugTextBox
            // 
            this.DebugTextBox.Location = new System.Drawing.Point(376, 45);
            this.DebugTextBox.Name = "DebugTextBox";
            this.DebugTextBox.Size = new System.Drawing.Size(315, 410);
            this.DebugTextBox.TabIndex = 9;
            this.DebugTextBox.Text = "";
            // 
            // RobotSelectNone
            // 
            this.RobotSelectNone.Location = new System.Drawing.Point(281, 180);
            this.RobotSelectNone.Name = "RobotSelectNone";
            this.RobotSelectNone.Size = new System.Drawing.Size(75, 23);
            this.RobotSelectNone.TabIndex = 11;
            this.RobotSelectNone.Text = "Select None";
            this.RobotSelectNone.UseVisualStyleBackColor = true;
            this.RobotSelectNone.Click += new System.EventHandler(this.RobotSelectNone_Click);
            // 
            // RobotSelectAll
            // 
            this.RobotSelectAll.Location = new System.Drawing.Point(202, 180);
            this.RobotSelectAll.Name = "RobotSelectAll";
            this.RobotSelectAll.Size = new System.Drawing.Size(75, 23);
            this.RobotSelectAll.TabIndex = 12;
            this.RobotSelectAll.Text = "Select All";
            this.RobotSelectAll.UseVisualStyleBackColor = true;
            this.RobotSelectAll.Click += new System.EventHandler(this.RobotSelectAll_Click);
            // 
            // KeywordSelectAll
            // 
            this.KeywordSelectAll.Location = new System.Drawing.Point(10, 363);
            this.KeywordSelectAll.Name = "KeywordSelectAll";
            this.KeywordSelectAll.Size = new System.Drawing.Size(75, 23);
            this.KeywordSelectAll.TabIndex = 14;
            this.KeywordSelectAll.Text = "Select All";
            this.KeywordSelectAll.UseVisualStyleBackColor = true;
            this.KeywordSelectAll.Click += new System.EventHandler(this.KeywordSelectAll_Click);
            // 
            // KeywordSelectNone
            // 
            this.KeywordSelectNone.Location = new System.Drawing.Point(101, 363);
            this.KeywordSelectNone.Name = "KeywordSelectNone";
            this.KeywordSelectNone.Size = new System.Drawing.Size(75, 23);
            this.KeywordSelectNone.TabIndex = 13;
            this.KeywordSelectNone.Text = "Select None";
            this.KeywordSelectNone.UseVisualStyleBackColor = true;
            this.KeywordSelectNone.Click += new System.EventHandler(this.KeywordSelectNone_Click);
            // 
            // DomainSelectAll
            // 
            this.DomainSelectAll.Location = new System.Drawing.Point(12, 180);
            this.DomainSelectAll.Name = "DomainSelectAll";
            this.DomainSelectAll.Size = new System.Drawing.Size(75, 23);
            this.DomainSelectAll.TabIndex = 16;
            this.DomainSelectAll.Text = "Select All";
            this.DomainSelectAll.UseVisualStyleBackColor = true;
            this.DomainSelectAll.Click += new System.EventHandler(this.DomainSelectAll_Click);
            // 
            // DomainSelectNone
            // 
            this.DomainSelectNone.Location = new System.Drawing.Point(103, 180);
            this.DomainSelectNone.Name = "DomainSelectNone";
            this.DomainSelectNone.Size = new System.Drawing.Size(75, 23);
            this.DomainSelectNone.TabIndex = 15;
            this.DomainSelectNone.Text = "Select None";
            this.DomainSelectNone.UseVisualStyleBackColor = true;
            this.DomainSelectNone.Click += new System.EventHandler(this.DomainSelectNone_Click);
            // 
            // KeywordSelector
            // 
            this.KeywordSelector.FormattingEnabled = true;
            this.KeywordSelector.Location = new System.Drawing.Point(12, 213);
            this.KeywordSelector.Name = "KeywordSelector";
            this.KeywordSelector.Size = new System.Drawing.Size(344, 139);
            this.KeywordSelector.TabIndex = 17;
            // 
            // DomainSelector
            // 
            this.DomainSelector.FormattingEnabled = true;
            this.DomainSelector.Location = new System.Drawing.Point(10, 45);
            this.DomainSelector.Name = "DomainSelector";
            this.DomainSelector.Size = new System.Drawing.Size(184, 124);
            this.DomainSelector.TabIndex = 18;
            // 
            // RobotSelector
            // 
            this.RobotSelector.FormattingEnabled = true;
            this.RobotSelector.Location = new System.Drawing.Point(212, 45);
            this.RobotSelector.Name = "RobotSelector";
            this.RobotSelector.Size = new System.Drawing.Size(144, 124);
            this.RobotSelector.TabIndex = 19;
            // 
            // testbutton
            // 
            this.testbutton.Location = new System.Drawing.Point(56, 433);
            this.testbutton.Name = "testbutton";
            this.testbutton.Size = new System.Drawing.Size(75, 23);
            this.testbutton.TabIndex = 20;
            this.testbutton.Text = "testwrite";
            this.testbutton.UseVisualStyleBackColor = true;
            this.testbutton.Click += new System.EventHandler(this.testbutton_Click);
            // 
            // DebugForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(730, 487);
            this.Controls.Add(this.testbutton);
            this.Controls.Add(this.RobotSelector);
            this.Controls.Add(this.DomainSelector);
            this.Controls.Add(this.KeywordSelector);
            this.Controls.Add(this.DomainSelectAll);
            this.Controls.Add(this.DomainSelectNone);
            this.Controls.Add(this.KeywordSelectAll);
            this.Controls.Add(this.KeywordSelectNone);
            this.Controls.Add(this.RobotSelectAll);
            this.Controls.Add(this.RobotSelectNone);
            this.Controls.Add(this.DebugTextBox);
            this.Controls.Add(this.FilterLabel);
            this.Name = "DebugForm";
            this.Text = "DebugForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label FilterLabel;
        private System.Windows.Forms.RichTextBox DebugTextBox;
        private System.Windows.Forms.Button RobotSelectNone;
        private System.Windows.Forms.Button RobotSelectAll;
        private System.Windows.Forms.Button KeywordSelectAll;
        private System.Windows.Forms.Button KeywordSelectNone;
        private System.Windows.Forms.Button DomainSelectAll;
        private System.Windows.Forms.Button DomainSelectNone;
        private System.Windows.Forms.CheckedListBox KeywordSelector;
        private System.Windows.Forms.CheckedListBox DomainSelector;
        private System.Windows.Forms.CheckedListBox RobotSelector;
        private System.Windows.Forms.Button testbutton;
    }
}