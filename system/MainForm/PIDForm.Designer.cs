namespace Robocup.ControlForm
{
    partial class PIDForm
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
            this.BtnSetPID = new System.Windows.Forms.Button();
            this.BtnGetPID = new System.Windows.Forms.Button();
            this.XYP = new System.Windows.Forms.Label();
            this.EditXYP = new System.Windows.Forms.TextBox();
            this.EditXYI = new System.Windows.Forms.TextBox();
            this.XYI = new System.Windows.Forms.Label();
            this.EditXYD = new System.Windows.Forms.TextBox();
            this.XYD = new System.Windows.Forms.Label();
            this.EditTD = new System.Windows.Forms.TextBox();
            this.ThetaD = new System.Windows.Forms.Label();
            this.EditTI = new System.Windows.Forms.TextBox();
            this.ThetaI = new System.Windows.Forms.Label();
            this.EditTP = new System.Windows.Forms.TextBox();
            this.ThetaP = new System.Windows.Forms.Label();
            this.XYVelocity = new System.Windows.Forms.CheckBox();
            this.EditID = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.IsShort = new System.Windows.Forms.CheckBox();
            this.btnSavePID = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // BtnSetPID
            // 
            this.BtnSetPID.Location = new System.Drawing.Point(443, 201);
            this.BtnSetPID.Name = "BtnSetPID";
            this.BtnSetPID.Size = new System.Drawing.Size(75, 23);
            this.BtnSetPID.TabIndex = 0;
            this.BtnSetPID.Text = "SetPID";
            this.BtnSetPID.UseVisualStyleBackColor = true;
            this.BtnSetPID.Click += new System.EventHandler(this.BtnSetPID_Click);
            // 
            // BtnGetPID
            // 
            this.BtnGetPID.Location = new System.Drawing.Point(443, 172);
            this.BtnGetPID.Name = "BtnGetPID";
            this.BtnGetPID.Size = new System.Drawing.Size(75, 23);
            this.BtnGetPID.TabIndex = 1;
            this.BtnGetPID.Text = "GetPID";
            this.BtnGetPID.UseVisualStyleBackColor = true;
            this.BtnGetPID.Click += new System.EventHandler(this.BtnGetPID_Click);
            // 
            // XYP
            // 
            this.XYP.AutoSize = true;
            this.XYP.Location = new System.Drawing.Point(48, 64);
            this.XYP.Name = "XYP";
            this.XYP.Size = new System.Drawing.Size(28, 13);
            this.XYP.TabIndex = 2;
            this.XYP.Text = "XYP";
            // 
            // EditXYP
            // 
            this.EditXYP.Location = new System.Drawing.Point(127, 61);
            this.EditXYP.Name = "EditXYP";
            this.EditXYP.Size = new System.Drawing.Size(100, 20);
            this.EditXYP.TabIndex = 3;
            // 
            // EditXYI
            // 
            this.EditXYI.Location = new System.Drawing.Point(127, 98);
            this.EditXYI.Name = "EditXYI";
            this.EditXYI.Size = new System.Drawing.Size(100, 20);
            this.EditXYI.TabIndex = 5;
            // 
            // XYI
            // 
            this.XYI.AutoSize = true;
            this.XYI.Location = new System.Drawing.Point(48, 101);
            this.XYI.Name = "XYI";
            this.XYI.Size = new System.Drawing.Size(24, 13);
            this.XYI.TabIndex = 4;
            this.XYI.Text = "XYI";
            // 
            // EditXYD
            // 
            this.EditXYD.Location = new System.Drawing.Point(127, 134);
            this.EditXYD.Name = "EditXYD";
            this.EditXYD.Size = new System.Drawing.Size(100, 20);
            this.EditXYD.TabIndex = 7;
            // 
            // XYD
            // 
            this.XYD.AutoSize = true;
            this.XYD.Location = new System.Drawing.Point(48, 137);
            this.XYD.Name = "XYD";
            this.XYD.Size = new System.Drawing.Size(29, 13);
            this.XYD.TabIndex = 6;
            this.XYD.Text = "XYD";
            // 
            // EditTD
            // 
            this.EditTD.Location = new System.Drawing.Point(361, 134);
            this.EditTD.Name = "EditTD";
            this.EditTD.Size = new System.Drawing.Size(100, 20);
            this.EditTD.TabIndex = 13;
            // 
            // ThetaD
            // 
            this.ThetaD.AutoSize = true;
            this.ThetaD.Location = new System.Drawing.Point(282, 137);
            this.ThetaD.Name = "ThetaD";
            this.ThetaD.Size = new System.Drawing.Size(43, 13);
            this.ThetaD.TabIndex = 12;
            this.ThetaD.Text = "ThetaD";
            // 
            // EditTI
            // 
            this.EditTI.Location = new System.Drawing.Point(361, 98);
            this.EditTI.Name = "EditTI";
            this.EditTI.Size = new System.Drawing.Size(100, 20);
            this.EditTI.TabIndex = 11;
            // 
            // ThetaI
            // 
            this.ThetaI.AutoSize = true;
            this.ThetaI.Location = new System.Drawing.Point(282, 101);
            this.ThetaI.Name = "ThetaI";
            this.ThetaI.Size = new System.Drawing.Size(38, 13);
            this.ThetaI.TabIndex = 10;
            this.ThetaI.Text = "ThetaI";
            // 
            // EditTP
            // 
            this.EditTP.Location = new System.Drawing.Point(361, 61);
            this.EditTP.Name = "EditTP";
            this.EditTP.Size = new System.Drawing.Size(100, 20);
            this.EditTP.TabIndex = 9;
            // 
            // ThetaP
            // 
            this.ThetaP.AutoSize = true;
            this.ThetaP.Location = new System.Drawing.Point(282, 64);
            this.ThetaP.Name = "ThetaP";
            this.ThetaP.Size = new System.Drawing.Size(42, 13);
            this.ThetaP.TabIndex = 8;
            this.ThetaP.Text = "ThetaP";
            // 
            // XYVelocity
            // 
            this.XYVelocity.AutoSize = true;
            this.XYVelocity.Location = new System.Drawing.Point(127, 176);
            this.XYVelocity.Name = "XYVelocity";
            this.XYVelocity.Size = new System.Drawing.Size(77, 17);
            this.XYVelocity.TabIndex = 14;
            this.XYVelocity.Text = "XYVelocity";
            this.XYVelocity.UseVisualStyleBackColor = true;
            // 
            // EditID
            // 
            this.EditID.Location = new System.Drawing.Point(127, 12);
            this.EditID.Name = "EditID";
            this.EditID.Size = new System.Drawing.Size(100, 20);
            this.EditID.TabIndex = 16;
            this.EditID.Text = "0";
            this.EditID.TextChanged += new System.EventHandler(this.EditID_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(48, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(18, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "ID";
            // 
            // IsShort
            // 
            this.IsShort.AutoSize = true;
            this.IsShort.Location = new System.Drawing.Point(295, 17);
            this.IsShort.Name = "IsShort";
            this.IsShort.Size = new System.Drawing.Size(59, 17);
            this.IsShort.TabIndex = 17;
            this.IsShort.Text = "IsShort";
            this.IsShort.UseVisualStyleBackColor = true;
            // 
            // btnSavePID
            // 
            this.btnSavePID.Location = new System.Drawing.Point(361, 172);
            this.btnSavePID.Name = "btnSavePID";
            this.btnSavePID.Size = new System.Drawing.Size(75, 23);
            this.btnSavePID.TabIndex = 18;
            this.btnSavePID.Text = "SavePID";
            this.btnSavePID.UseVisualStyleBackColor = true;
            this.btnSavePID.Visible = false;
            this.btnSavePID.Click += new System.EventHandler(this.btnSavePID_Click);
            // 
            // PIDForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(529, 238);
            this.Controls.Add(this.btnSavePID);
            this.Controls.Add(this.IsShort);
            this.Controls.Add(this.EditID);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.XYVelocity);
            this.Controls.Add(this.EditTD);
            this.Controls.Add(this.ThetaD);
            this.Controls.Add(this.EditTI);
            this.Controls.Add(this.ThetaI);
            this.Controls.Add(this.EditTP);
            this.Controls.Add(this.ThetaP);
            this.Controls.Add(this.EditXYD);
            this.Controls.Add(this.XYD);
            this.Controls.Add(this.EditXYI);
            this.Controls.Add(this.XYI);
            this.Controls.Add(this.EditXYP);
            this.Controls.Add(this.XYP);
            this.Controls.Add(this.BtnGetPID);
            this.Controls.Add(this.BtnSetPID);
            this.Name = "PIDForm";
            this.Text = "PIDForm";
            this.Shown += new System.EventHandler(this.PIDForm_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BtnSetPID;
        private System.Windows.Forms.Button BtnGetPID;
        private System.Windows.Forms.Label XYP;
        private System.Windows.Forms.TextBox EditXYP;
        private System.Windows.Forms.TextBox EditXYI;
        private System.Windows.Forms.Label XYI;
        private System.Windows.Forms.TextBox EditXYD;
        private System.Windows.Forms.Label XYD;
        private System.Windows.Forms.TextBox EditTD;
        private System.Windows.Forms.Label ThetaD;
        private System.Windows.Forms.TextBox EditTI;
        private System.Windows.Forms.Label ThetaI;
        private System.Windows.Forms.TextBox EditTP;
        private System.Windows.Forms.Label ThetaP;
        private System.Windows.Forms.CheckBox XYVelocity;
        private System.Windows.Forms.TextBox EditID;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox IsShort;
        private System.Windows.Forms.Button btnSavePID;
    }
}