namespace Robocup.MotionControl
{
    partial class DataCollector
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
            this.buttonStart = new System.Windows.Forms.Button();
            this.comboBoxCollectionType = new System.Windows.Forms.ComboBox();
            this.textBoxRobotID = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(205, 39);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(75, 23);
            this.buttonStart.TabIndex = 0;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // comboBoxCollectionType
            // 
            this.comboBoxCollectionType.FormattingEnabled = true;
            this.comboBoxCollectionType.Location = new System.Drawing.Point(25, 41);
            this.comboBoxCollectionType.Name = "comboBoxCollectionType";
            this.comboBoxCollectionType.Size = new System.Drawing.Size(121, 21);
            this.comboBoxCollectionType.TabIndex = 1;
            // 
            // textBoxRobotID
            // 
            this.textBoxRobotID.Location = new System.Drawing.Point(180, 12);
            this.textBoxRobotID.Name = "textBoxRobotID";
            this.textBoxRobotID.Size = new System.Drawing.Size(100, 20);
            this.textBoxRobotID.TabIndex = 2;
            this.textBoxRobotID.Text = "0";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(22, 15);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(35, 13);
            label1.TabIndex = 3;
            label1.Text = "label1";
            // 
            // DataCollector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Controls.Add(label1);
            this.Controls.Add(this.textBoxRobotID);
            this.Controls.Add(this.comboBoxCollectionType);
            this.Controls.Add(this.buttonStart);
            this.Name = "DataCollector";
            this.Text = "DataCollector";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.ComboBox comboBoxCollectionType;
        private System.Windows.Forms.TextBox textBoxRobotID;
    }
}