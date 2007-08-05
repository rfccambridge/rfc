namespace InterpreterTester
{
    partial class InterpreterTester
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
            this.SuspendLayout();
            // 
            // InterpreterTester
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(644, 463);
            this.Name = "InterpreterTester";
            this.Text = "Interpreter Tester";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.InterpreterTester_Paint);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.InterpreterTester_MouseClick);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.InterpreterTester_KeyPress);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.InterpreterTester_FormClosing);
            this.Load += new System.EventHandler(this.InterpreterTester_Load);
            this.ResumeLayout(false);

        }

        #endregion
    }
}

