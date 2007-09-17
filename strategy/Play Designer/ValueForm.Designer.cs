namespace Robocup.Plays
{
    partial class ValueForm
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
            this.SelectObjectButton = new System.Windows.Forms.Button();
            this.UseValueButton = new System.Windows.Forms.Button();
            this.enterValueBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonSelectPrevious = new System.Windows.Forms.Button();
            this.nameLabel = new System.Windows.Forms.Label();
            this.nameInputBox = new System.Windows.Forms.TextBox();
            this.useFunctionButton = new System.Windows.Forms.Button();
            this.contentPanel = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.functionSelector = new System.Windows.Forms.ComboBox();
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
            this.splitContainer1.Panel1.Controls.Add(this.SelectObjectButton);
            this.splitContainer1.Panel1.Controls.Add(this.UseValueButton);
            this.splitContainer1.Panel1.Controls.Add(this.enterValueBox);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.buttonSelectPrevious);
            this.splitContainer1.Panel2.Controls.Add(this.nameLabel);
            this.splitContainer1.Panel2.Controls.Add(this.nameInputBox);
            this.splitContainer1.Panel2.Controls.Add(this.useFunctionButton);
            this.splitContainer1.Panel2.Controls.Add(this.contentPanel);
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Panel2.Controls.Add(this.functionSelector);
            this.splitContainer1.Size = new System.Drawing.Size(292, 424);
            this.splitContainer1.SplitterDistance = 33;
            this.splitContainer1.TabIndex = 0;
            // 
            // SelectObjectButton
            // 
            this.SelectObjectButton.Location = new System.Drawing.Point(97, 3);
            this.SelectObjectButton.Name = "SelectObjectButton";
            this.SelectObjectButton.Size = new System.Drawing.Size(104, 23);
            this.SelectObjectButton.TabIndex = 3;
            this.SelectObjectButton.Text = "Select Field Object";
            this.SelectObjectButton.UseVisualStyleBackColor = true;
            this.SelectObjectButton.Visible = false;
            this.SelectObjectButton.Click += new System.EventHandler(this.SelectObjectButton_click);
            // 
            // UseValueButton
            // 
            this.UseValueButton.Location = new System.Drawing.Point(214, 4);
            this.UseValueButton.Name = "UseValueButton";
            this.UseValueButton.Size = new System.Drawing.Size(66, 23);
            this.UseValueButton.TabIndex = 2;
            this.UseValueButton.Text = "Use Value";
            this.UseValueButton.UseVisualStyleBackColor = true;
            this.UseValueButton.Visible = false;
            this.UseValueButton.Click += new System.EventHandler(this.doneButton_Click);
            // 
            // enterValueBox
            // 
            this.enterValueBox.Location = new System.Drawing.Point(91, 6);
            this.enterValueBox.Name = "enterValueBox";
            this.enterValueBox.Size = new System.Drawing.Size(117, 20);
            this.enterValueBox.TabIndex = 1;
            this.enterValueBox.Visible = false;
            this.enterValueBox.Click += new System.EventHandler(this.enterValueBox_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Enter a value:";
            this.label2.Visible = false;
            // 
            // buttonSelectPrevious
            // 
            this.buttonSelectPrevious.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonSelectPrevious.Location = new System.Drawing.Point(90, 13);
            this.buttonSelectPrevious.Name = "buttonSelectPrevious";
            this.buttonSelectPrevious.Size = new System.Drawing.Size(119, 23);
            this.buttonSelectPrevious.TabIndex = 10;
            this.buttonSelectPrevious.Text = "Select Created Object";
            this.buttonSelectPrevious.UseVisualStyleBackColor = true;
            this.buttonSelectPrevious.Click += new System.EventHandler(this.buttonSelectPrevious_Click);
            // 
            // nameLabel
            // 
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(9, 338);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(121, 13);
            this.nameLabel.TabIndex = 9;
            this.nameLabel.Text = "Name of created object:";
            // 
            // nameInputBox
            // 
            this.nameInputBox.Location = new System.Drawing.Point(48, 354);
            this.nameInputBox.Name = "nameInputBox";
            this.nameInputBox.Size = new System.Drawing.Size(138, 20);
            this.nameInputBox.TabIndex = 8;
            // 
            // useFunctionButton
            // 
            this.useFunctionButton.Enabled = false;
            this.useFunctionButton.Location = new System.Drawing.Point(197, 352);
            this.useFunctionButton.Name = "useFunctionButton";
            this.useFunctionButton.Size = new System.Drawing.Size(83, 23);
            this.useFunctionButton.TabIndex = 7;
            this.useFunctionButton.Text = "Use Function";
            this.useFunctionButton.UseVisualStyleBackColor = true;
            this.useFunctionButton.Click += new System.EventHandler(this.doneButton_Click);
            // 
            // contentPanel
            // 
            this.contentPanel.Location = new System.Drawing.Point(12, 77);
            this.contentPanel.Name = "contentPanel";
            this.contentPanel.Size = new System.Drawing.Size(268, 243);
            this.contentPanel.TabIndex = 6;
            this.contentPanel.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Choose a function";
            // 
            // functionSelector
            // 
            this.functionSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.functionSelector.FormattingEnabled = true;
            this.functionSelector.Location = new System.Drawing.Point(113, 50);
            this.functionSelector.Name = "functionSelector";
            this.functionSelector.Size = new System.Drawing.Size(167, 21);
            this.functionSelector.TabIndex = 4;
            this.functionSelector.SelectedIndexChanged += new System.EventHandler(this.conditionSelector_SelectedIndexChanged);
            // 
            // ValueForm
            // 
            this.AcceptButton = this.UseValueButton;
            this.ClientSize = new System.Drawing.Size(292, 424);
            this.Controls.Add(this.splitContainer1);
            this.Name = "ValueForm";
            this.Text = "Edit Condition";
            this.Resize += new System.EventHandler(this.NewConditionForm_Resize);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button useFunctionButton;
        private System.Windows.Forms.GroupBox contentPanel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox functionSelector;
        private System.Windows.Forms.TextBox enterValueBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button UseValueButton;
        private System.Windows.Forms.Button SelectObjectButton;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.TextBox nameInputBox;
        private System.Windows.Forms.Button buttonSelectPrevious;

    }
}