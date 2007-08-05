namespace Vision
{
    partial class HSVSliceForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HSVSliceForm));
            this.panSlice = new System.Windows.Forms.Panel();
            this.lblHighlight = new System.Windows.Forms.Label();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsbLoadTables = new System.Windows.Forms.ToolStripButton();
            this.tsbSaveTables = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbPencil = new System.Windows.Forms.ToolStripButton();
            this.tsbEraser = new System.Windows.Forms.ToolStripButton();
            this.tsbPaintBucket = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbUndoLastFill = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tmrDraw = new System.Windows.Forms.Timer(this.components);
            this.txtV = new System.Windows.Forms.TextBox();
            this.txtS = new System.Windows.Forms.TextBox();
            this.txtH = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnNextSlice = new System.Windows.Forms.Button();
            this.btnPrevSlice = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.panSlice.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panSlice
            // 
            this.panSlice.BackColor = System.Drawing.Color.Black;
            this.panSlice.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.panSlice.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panSlice.Controls.Add(this.lblHighlight);
            this.panSlice.Location = new System.Drawing.Point(4, 39);
            this.panSlice.Margin = new System.Windows.Forms.Padding(4);
            this.panSlice.Name = "panSlice";
            this.panSlice.Size = new System.Drawing.Size(360, 100);
            this.panSlice.TabIndex = 0;
            this.panSlice.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panSlice_MouseDown);
            this.panSlice.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panSlice_MouseMove);
            this.panSlice.Paint += new System.Windows.Forms.PaintEventHandler(this.panSlice_Paint);
            this.panSlice.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panSlice_MouseUp);
            // 
            // lblHighlight
            // 
            this.lblHighlight.BackColor = System.Drawing.Color.Transparent;
            this.lblHighlight.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblHighlight.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblHighlight.Location = new System.Drawing.Point(457, 98);
            this.lblHighlight.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblHighlight.Name = "lblHighlight";
            this.lblHighlight.Size = new System.Drawing.Size(13, 12);
            this.lblHighlight.TabIndex = 0;
            this.lblHighlight.Visible = false;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbLoadTables,
            this.tsbSaveTables,
            this.toolStripSeparator3,
            this.tsbPencil,
            this.tsbEraser,
            this.tsbPaintBucket,
            this.toolStripSeparator1,
            this.tsbUndoLastFill,
            this.toolStripSeparator2});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(530, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tsbLoadTables
            // 
            this.tsbLoadTables.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbLoadTables.Image = global::Vision.Properties.Resources.openIcon;
            this.tsbLoadTables.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbLoadTables.Name = "tsbLoadTables";
            this.tsbLoadTables.Size = new System.Drawing.Size(23, 22);
            this.tsbLoadTables.Text = "toolStripButton1";
            this.tsbLoadTables.Click += new System.EventHandler(this.tsbLoadTables_Click);
            // 
            // tsbSaveTables
            // 
            this.tsbSaveTables.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbSaveTables.Image = global::Vision.Properties.Resources.saveIcon;
            this.tsbSaveTables.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbSaveTables.Name = "tsbSaveTables";
            this.tsbSaveTables.Size = new System.Drawing.Size(23, 22);
            this.tsbSaveTables.Text = "toolStripButton2";
            this.tsbSaveTables.Click += new System.EventHandler(this.tsbSaveTables_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbPencil
            // 
            this.tsbPencil.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbPencil.Image = global::Vision.Properties.Resources.pencilIcon;
            this.tsbPencil.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbPencil.Name = "tsbPencil";
            this.tsbPencil.Size = new System.Drawing.Size(23, 22);
            this.tsbPencil.Text = "toolStripButton1";
            this.tsbPencil.Click += new System.EventHandler(this.tsbPencil_Click);
            // 
            // tsbEraser
            // 
            this.tsbEraser.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbEraser.Image = ((System.Drawing.Image)(resources.GetObject("tsbEraser.Image")));
            this.tsbEraser.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbEraser.Name = "tsbEraser";
            this.tsbEraser.Size = new System.Drawing.Size(23, 22);
            this.tsbEraser.Text = "toolStripButton1";
            this.tsbEraser.Click += new System.EventHandler(this.tsbEraser_Click);
            // 
            // tsbPaintBucket
            // 
            this.tsbPaintBucket.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbPaintBucket.Image = global::Vision.Properties.Resources.paintBucketIcon;
            this.tsbPaintBucket.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbPaintBucket.Name = "tsbPaintBucket";
            this.tsbPaintBucket.Size = new System.Drawing.Size(23, 22);
            this.tsbPaintBucket.Text = "toolStripButton2";
            this.tsbPaintBucket.Click += new System.EventHandler(this.tsbPaintBucket_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbUndoLastFill
            // 
            this.tsbUndoLastFill.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbUndoLastFill.Enabled = false;
            this.tsbUndoLastFill.Image = global::Vision.Properties.Resources.image035;
            this.tsbUndoLastFill.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbUndoLastFill.Name = "tsbUndoLastFill";
            this.tsbUndoLastFill.Size = new System.Drawing.Size(23, 22);
            this.tsbUndoLastFill.Text = "toolStripButton1";
            this.tsbUndoLastFill.Click += new System.EventHandler(this.tsbUndoLastFill_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // tmrDraw
            // 
            this.tmrDraw.Interval = 30;
            this.tmrDraw.Tick += new System.EventHandler(this.tmrDraw_Tick);
            // 
            // txtV
            // 
            this.txtV.Location = new System.Drawing.Point(191, 147);
            this.txtV.Margin = new System.Windows.Forms.Padding(4);
            this.txtV.Name = "txtV";
            this.txtV.ReadOnly = true;
            this.txtV.Size = new System.Drawing.Size(45, 22);
            this.txtV.TabIndex = 40;
            this.txtV.TabStop = false;
            // 
            // txtS
            // 
            this.txtS.Location = new System.Drawing.Point(115, 147);
            this.txtS.Margin = new System.Windows.Forms.Padding(4);
            this.txtS.Name = "txtS";
            this.txtS.ReadOnly = true;
            this.txtS.Size = new System.Drawing.Size(45, 22);
            this.txtS.TabIndex = 39;
            this.txtS.TabStop = false;
            // 
            // txtH
            // 
            this.txtH.Location = new System.Drawing.Point(35, 147);
            this.txtH.Margin = new System.Windows.Forms.Padding(4);
            this.txtH.Name = "txtH";
            this.txtH.ReadOnly = true;
            this.txtH.Size = new System.Drawing.Size(51, 22);
            this.txtH.TabIndex = 38;
            this.txtH.TabStop = false;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(170, 152);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(21, 17);
            this.label10.TabIndex = 37;
            this.label10.Text = "V:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(92, 152);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(21, 17);
            this.label7.TabIndex = 36;
            this.label7.Text = "S:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 153);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(22, 17);
            this.label2.TabIndex = 35;
            this.label2.Text = "H:";
            // 
            // btnNextSlice
            // 
            this.btnNextSlice.Location = new System.Drawing.Point(320, 142);
            this.btnNextSlice.Margin = new System.Windows.Forms.Padding(4);
            this.btnNextSlice.Name = "btnNextSlice";
            this.btnNextSlice.Size = new System.Drawing.Size(44, 28);
            this.btnNextSlice.TabIndex = 41;
            this.btnNextSlice.Text = ">>";
            this.btnNextSlice.UseVisualStyleBackColor = true;
            this.btnNextSlice.Click += new System.EventHandler(this.btnNextSlice_Click);
            // 
            // btnPrevSlice
            // 
            this.btnPrevSlice.Location = new System.Drawing.Point(275, 142);
            this.btnPrevSlice.Margin = new System.Windows.Forms.Padding(4);
            this.btnPrevSlice.Name = "btnPrevSlice";
            this.btnPrevSlice.Size = new System.Drawing.Size(44, 28);
            this.btnPrevSlice.TabIndex = 42;
            this.btnPrevSlice.Text = "<<";
            this.btnPrevSlice.UseVisualStyleBackColor = true;
            this.btnPrevSlice.Click += new System.EventHandler(this.btnPrevSlice_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 56.57895F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 43.42105F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.label8, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label9, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label11, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.label12, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.label13, 1, 4);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(371, 39);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(152, 100);
            this.tableLayoutPanel1.TabIndex = 43;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Undo";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 17);
            this.label3.TabIndex = 1;
            this.label3.Text = "Pencil";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 40);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(50, 17);
            this.label4.TabIndex = 2;
            this.label4.Text = "Eraser";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 60);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(63, 17);
            this.label5.TabIndex = 3;
            this.label5.Text = "Highlight";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 80);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(75, 20);
            this.label6.TabIndex = 4;
            this.label6.Text = "Regenerate";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Courier New", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(89, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(56, 16);
            this.label8.TabIndex = 5;
            this.label8.Text = "Ctrl+Z";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Bold);
            this.label9.Location = new System.Drawing.Point(89, 20);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(19, 20);
            this.label9.TabIndex = 6;
            this.label9.Text = "W";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Bold);
            this.label11.Location = new System.Drawing.Point(89, 40);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(19, 20);
            this.label11.TabIndex = 7;
            this.label11.Text = "E";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Bold);
            this.label12.Location = new System.Drawing.Point(89, 60);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(19, 20);
            this.label12.TabIndex = 8;
            this.label12.Text = "H";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Courier New", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(89, 80);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(56, 16);
            this.label13.TabIndex = 9;
            this.label13.Text = "Ctrl+G";
            // 
            // HSVSliceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(530, 180);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.btnPrevSlice);
            this.Controls.Add(this.btnNextSlice);
            this.Controls.Add(this.txtV);
            this.Controls.Add(this.txtS);
            this.Controls.Add(this.txtH);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.panSlice);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "HSVSliceForm";
            this.ShowInTaskbar = false;
            this.Text = "Editing Color Map";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HSVSliceForm_FormClosing);
            this.panSlice.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }



        #endregion

        public System.Windows.Forms.Panel panSlice;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbPencil;
        private System.Windows.Forms.ToolStripButton tsbPaintBucket;
        private System.Windows.Forms.ToolStripButton tsbUndoLastFill;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Timer tmrDraw;
        public System.Windows.Forms.TextBox txtV;
        public System.Windows.Forms.TextBox txtS;
        public System.Windows.Forms.TextBox txtH;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.Label lblHighlight;
        private System.Windows.Forms.Button btnNextSlice;
        private System.Windows.Forms.Button btnPrevSlice;
        private System.Windows.Forms.ToolStripButton tsbEraser;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton tsbLoadTables;
        private System.Windows.Forms.ToolStripButton tsbSaveTables;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
    }
}