namespace MachineLearning.ExternalProgramScoring
{
    partial class MainForm
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
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label4;
            System.Windows.Forms.Label label5;
            System.Windows.Forms.Label label6;
            System.Windows.Forms.Label label7;
            System.Windows.Forms.Label label8;
            System.Windows.Forms.Label label9;
            System.Windows.Forms.Label label10;
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.buttonToggleShow = new System.Windows.Forms.Button();
            this.textBoxProgram = new System.Windows.Forms.TextBox();
            this.textBoxConfigDir = new System.Windows.Forms.TextBox();
            this.textBoxArgs = new System.Windows.Forms.TextBox();
            this.textBoxExtensions = new System.Windows.Forms.TextBox();
            this.textBoxTemp = new System.Windows.Forms.TextBox();
            this.textBoxCoolingSpeed = new System.Windows.Forms.TextBox();
            this.textBoxNumIdentical = new System.Windows.Forms.TextBox();
            this.buttonReload = new System.Windows.Forms.Button();
            this.checkBoxRemoveTags = new System.Windows.Forms.CheckBox();
            this.checkBoxStochastic = new System.Windows.Forms.CheckBox();
            this.textBoxComputationUnit = new System.Windows.Forms.TextBox();
            this.labelComputationUnit = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            label8 = new System.Windows.Forms.Label();
            label9 = new System.Windows.Forms.Label();
            label10 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(33, 309);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(117, 13);
            label1.TabIndex = 1;
            label1.Text = "Communication system:";
            label1.Visible = false;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(64, 41);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(49, 13);
            label2.TabIndex = 7;
            label2.Text = "Program:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(9, 93);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(104, 13);
            label3.TabIndex = 9;
            label3.Text = "Config File Directory:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(53, 67);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(60, 13);
            label4.TabIndex = 10;
            label4.Text = "Arguments:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(33, 119);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(80, 13);
            label5.TabIndex = 13;
            label5.Text = "File Extensions:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(88, 18);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(140, 13);
            label6.TabIndex = 14;
            label6.Text = "External Program Properties:";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(432, 18);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(156, 13);
            label7.TabIndex = 15;
            label7.Text = "Simulated Annealing Properties:";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(430, 41);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(109, 13);
            label8.TabIndex = 17;
            label8.Text = "Starting Temperature:";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(460, 67);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(79, 13);
            label9.TabIndex = 19;
            label9.Text = "Cooling Speed:";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(442, 93);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(97, 13);
            label10.TabIndex = 21;
            label10.Text = "# Identical to Stop:";
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(36, 325);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(46, 17);
            this.radioButton1.TabIndex = 0;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Files";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.Visible = false;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Enabled = false;
            this.radioButton2.Location = new System.Drawing.Point(36, 348);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(64, 17);
            this.radioButton2.TabIndex = 2;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "Sockets";
            this.radioButton2.UseVisualStyleBackColor = true;
            this.radioButton2.Visible = false;
            // 
            // buttonStart
            // 
            this.buttonStart.Enabled = false;
            this.buttonStart.Location = new System.Drawing.Point(494, 163);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(75, 23);
            this.buttonStart.TabIndex = 3;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.Location = new System.Drawing.Point(494, 192);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(75, 23);
            this.buttonStop.TabIndex = 4;
            this.buttonStop.Text = "Stop && Save";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // buttonToggleShow
            // 
            this.buttonToggleShow.Location = new System.Drawing.Point(170, 214);
            this.buttonToggleShow.Name = "buttonToggleShow";
            this.buttonToggleShow.Size = new System.Drawing.Size(129, 23);
            this.buttonToggleShow.TabIndex = 5;
            this.buttonToggleShow.Text = "Show External Window";
            this.buttonToggleShow.UseVisualStyleBackColor = true;
            this.buttonToggleShow.Click += new System.EventHandler(this.buttonToggleShow_Click);
            // 
            // textBoxProgram
            // 
            this.textBoxProgram.Location = new System.Drawing.Point(119, 38);
            this.textBoxProgram.Name = "textBoxProgram";
            this.textBoxProgram.Size = new System.Drawing.Size(241, 20);
            this.textBoxProgram.TabIndex = 6;
            this.textBoxProgram.Text = "D:\\My Projects\\C sharp\\Robocup\\InterpreterTester\\bin\\Release\\InterpreterTester.ex" +
                "e";
            // 
            // textBoxConfigDir
            // 
            this.textBoxConfigDir.Location = new System.Drawing.Point(119, 90);
            this.textBoxConfigDir.Name = "textBoxConfigDir";
            this.textBoxConfigDir.Size = new System.Drawing.Size(241, 20);
            this.textBoxConfigDir.TabIndex = 8;
            this.textBoxConfigDir.Text = "C:\\Microsoft Robotics Studio (1.0)\\samples\\Simulator\\Plays\\";
            // 
            // textBoxArgs
            // 
            this.textBoxArgs.Location = new System.Drawing.Point(119, 64);
            this.textBoxArgs.Name = "textBoxArgs";
            this.textBoxArgs.Size = new System.Drawing.Size(241, 20);
            this.textBoxArgs.TabIndex = 11;
            // 
            // textBoxExtensions
            // 
            this.textBoxExtensions.Location = new System.Drawing.Point(119, 116);
            this.textBoxExtensions.Name = "textBoxExtensions";
            this.textBoxExtensions.Size = new System.Drawing.Size(241, 20);
            this.textBoxExtensions.TabIndex = 12;
            this.textBoxExtensions.Text = "txt cfg";
            // 
            // textBoxTemp
            // 
            this.textBoxTemp.Location = new System.Drawing.Point(545, 38);
            this.textBoxTemp.Name = "textBoxTemp";
            this.textBoxTemp.Size = new System.Drawing.Size(43, 20);
            this.textBoxTemp.TabIndex = 16;
            this.textBoxTemp.Text = ".1";
            // 
            // textBoxCoolingSpeed
            // 
            this.textBoxCoolingSpeed.Location = new System.Drawing.Point(545, 64);
            this.textBoxCoolingSpeed.Name = "textBoxCoolingSpeed";
            this.textBoxCoolingSpeed.Size = new System.Drawing.Size(43, 20);
            this.textBoxCoolingSpeed.TabIndex = 18;
            this.textBoxCoolingSpeed.Text = ".02";
            // 
            // textBoxNumIdentical
            // 
            this.textBoxNumIdentical.Location = new System.Drawing.Point(545, 90);
            this.textBoxNumIdentical.Name = "textBoxNumIdentical";
            this.textBoxNumIdentical.Size = new System.Drawing.Size(43, 20);
            this.textBoxNumIdentical.TabIndex = 20;
            this.textBoxNumIdentical.Text = "5";
            // 
            // buttonReload
            // 
            this.buttonReload.Location = new System.Drawing.Point(341, 280);
            this.buttonReload.Name = "buttonReload";
            this.buttonReload.Size = new System.Drawing.Size(63, 23);
            this.buttonReload.TabIndex = 22;
            this.buttonReload.Text = "Load";
            this.buttonReload.UseVisualStyleBackColor = true;
            this.buttonReload.Click += new System.EventHandler(this.buttonSetProperties_Click);
            // 
            // checkBoxRemoveTags
            // 
            this.checkBoxRemoveTags.AutoSize = true;
            this.checkBoxRemoveTags.Checked = true;
            this.checkBoxRemoveTags.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxRemoveTags.Location = new System.Drawing.Point(119, 142);
            this.checkBoxRemoveTags.Name = "checkBoxRemoveTags";
            this.checkBoxRemoveTags.Size = new System.Drawing.Size(93, 17);
            this.checkBoxRemoveTags.TabIndex = 23;
            this.checkBoxRemoveTags.Text = "Remove Tags";
            this.checkBoxRemoveTags.UseVisualStyleBackColor = true;
            // 
            // checkBoxStochastic
            // 
            this.checkBoxStochastic.AutoSize = true;
            this.checkBoxStochastic.Location = new System.Drawing.Point(119, 165);
            this.checkBoxStochastic.Name = "checkBoxStochastic";
            this.checkBoxStochastic.Size = new System.Drawing.Size(114, 17);
            this.checkBoxStochastic.TabIndex = 24;
            this.checkBoxStochastic.Text = "Stochastic Version";
            this.checkBoxStochastic.UseVisualStyleBackColor = true;
            this.checkBoxStochastic.CheckedChanged += new System.EventHandler(this.checkBoxStochastic_CheckedChanged);
            // 
            // textBoxComputationUnit
            // 
            this.textBoxComputationUnit.Enabled = false;
            this.textBoxComputationUnit.Location = new System.Drawing.Point(119, 188);
            this.textBoxComputationUnit.Name = "textBoxComputationUnit";
            this.textBoxComputationUnit.Size = new System.Drawing.Size(241, 20);
            this.textBoxComputationUnit.TabIndex = 25;
            this.textBoxComputationUnit.Text = "1";
            // 
            // labelComputationUnit
            // 
            this.labelComputationUnit.AutoSize = true;
            this.labelComputationUnit.Enabled = false;
            this.labelComputationUnit.Location = new System.Drawing.Point(22, 191);
            this.labelComputationUnit.Name = "labelComputationUnit";
            this.labelComputationUnit.Size = new System.Drawing.Size(91, 13);
            this.labelComputationUnit.TabIndex = 26;
            this.labelComputationUnit.Text = "Computation Unit:";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(625, 395);
            this.Controls.Add(this.labelComputationUnit);
            this.Controls.Add(this.textBoxComputationUnit);
            this.Controls.Add(this.checkBoxStochastic);
            this.Controls.Add(this.checkBoxRemoveTags);
            this.Controls.Add(this.buttonReload);
            this.Controls.Add(label10);
            this.Controls.Add(this.textBoxNumIdentical);
            this.Controls.Add(label9);
            this.Controls.Add(this.textBoxCoolingSpeed);
            this.Controls.Add(label8);
            this.Controls.Add(this.textBoxTemp);
            this.Controls.Add(label7);
            this.Controls.Add(label6);
            this.Controls.Add(label5);
            this.Controls.Add(this.textBoxExtensions);
            this.Controls.Add(this.textBoxArgs);
            this.Controls.Add(label4);
            this.Controls.Add(label3);
            this.Controls.Add(this.textBoxConfigDir);
            this.Controls.Add(label2);
            this.Controls.Add(this.textBoxProgram);
            this.Controls.Add(this.buttonToggleShow);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.radioButton2);
            this.Controls.Add(label1);
            this.Controls.Add(this.radioButton1);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Button buttonToggleShow;
        private System.Windows.Forms.TextBox textBoxProgram;
        private System.Windows.Forms.TextBox textBoxConfigDir;
        private System.Windows.Forms.TextBox textBoxArgs;
        private System.Windows.Forms.TextBox textBoxExtensions;
        private System.Windows.Forms.TextBox textBoxTemp;
        private System.Windows.Forms.TextBox textBoxCoolingSpeed;
        private System.Windows.Forms.TextBox textBoxNumIdentical;
        private System.Windows.Forms.Button buttonReload;
        private System.Windows.Forms.CheckBox checkBoxRemoveTags;
        private System.Windows.Forms.CheckBox checkBoxStochastic;
        private System.Windows.Forms.TextBox textBoxComputationUnit;
        private System.Windows.Forms.Label labelComputationUnit;
    }
}