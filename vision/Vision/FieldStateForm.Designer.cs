namespace Vision {
    partial class FieldStateForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.picField = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picField)).BeginInit();
            this.SuspendLayout();
            // 
            // picField
            // 
            this.picField.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picField.Location = new System.Drawing.Point(8, 3);
            this.picField.Name = "picField";
            this.picField.Size = new System.Drawing.Size(420, 620);
            this.picField.TabIndex = 0;
            this.picField.TabStop = false;
            // 
            // FieldStateForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(434, 631);
            this.Controls.Add(this.picField);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MinimizeBox = false;
            this.Name = "FieldStateForm";
            this.Text = "Field State";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FieldStateForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.picField)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox picField;
    }
}