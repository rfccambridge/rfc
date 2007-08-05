namespace WindowsApplication2
{
    partial class frmRAWImage
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnDisplay = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtHeight = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtWidth = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSourceFile = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.dlgOpenFile = new System.Windows.Forms.OpenFileDialog();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnBlob = new System.Windows.Forms.Button();
            this.btnZoomIn = new System.Windows.Forms.Button();
            this.txtZoomFactor = new System.Windows.Forms.TextBox();
            this.btnDrawGrid = new System.Windows.Forms.Button();
            this.btnHighlightBlob = new System.Windows.Forms.Button();
            this.txtHighlightBlob = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.lblBlobsFound = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lblBlobID = new System.Windows.Forms.Label();
            this.lblAvgColor = new System.Windows.Forms.Label();
            this.lblCenter = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.lblBlobArea = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.lblRunID = new System.Windows.Forms.Label();
            this.lblRunCoords = new System.Windows.Forms.Label();
            this.lblRunAvgColor = new System.Windows.Forms.Label();
            this.lblRunLength = new System.Windows.Forms.Label();
            this.btnSelRun = new System.Windows.Forms.Button();
            this.txtRunID = new System.Windows.Forms.TextBox();
            this.txtSelRow = new System.Windows.Forms.Button();
            this.txtRow = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.label7 = new System.Windows.Forms.Label();
            this.lblRunsInRow = new System.Windows.Forms.Label();
            this.btnHighlightNBlobs = new System.Windows.Forms.Button();
            this.txtNBlobs = new System.Windows.Forms.TextBox();
            this.lblSelBlobByCol = new System.Windows.Forms.Label();
            this.btnFindBlobByCol = new System.Windows.Forms.Button();
            this.txtSelBlobByCenterX = new System.Windows.Forms.TextBox();
            this.txtSelBlobByCenterY = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.btnNormalize = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.InitialImage = null;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(150, 100);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Resize += new System.EventHandler(this.pictureBox1_Resize);
            this.pictureBox1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseClick);
            // 
            // btnDisplay
            // 
            this.btnDisplay.Location = new System.Drawing.Point(348, 58);
            this.btnDisplay.Name = "btnDisplay";
            this.btnDisplay.Size = new System.Drawing.Size(120, 33);
            this.btnDisplay.TabIndex = 1;
            this.btnDisplay.Text = "Load and  Display";
            this.btnDisplay.UseVisualStyleBackColor = true;
            this.btnDisplay.Click += new System.EventHandler(this.btnDisplay_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtHeight);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtWidth);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtSourceFile);
            this.groupBox1.Controls.Add(this.btnBrowse);
            this.groupBox1.Location = new System.Drawing.Point(12, 26);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(330, 112);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Source";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(97, 65);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Height:";
            // 
            // txtHeight
            // 
            this.txtHeight.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.txtHeight.Enabled = false;
            this.txtHeight.Location = new System.Drawing.Point(100, 81);
            this.txtHeight.Name = "txtHeight";
            this.txtHeight.Size = new System.Drawing.Size(55, 20);
            this.txtHeight.TabIndex = 5;
            this.txtHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 65);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Width:";
            // 
            // txtWidth
            // 
            this.txtWidth.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.txtWidth.Enabled = false;
            this.txtWidth.Location = new System.Drawing.Point(22, 81);
            this.txtWidth.Name = "txtWidth";
            this.txtWidth.Size = new System.Drawing.Size(56, 20);
            this.txtWidth.TabIndex = 3;
            this.txtWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "File (*.raw):";
            // 
            // txtSourceFile
            // 
            this.txtSourceFile.Location = new System.Drawing.Point(22, 34);
            this.txtSourceFile.Name = "txtSourceFile";
            this.txtSourceFile.Size = new System.Drawing.Size(222, 20);
            this.txtSourceFile.TabIndex = 1;
            this.txtSourceFile.Text = "C:\\Documents and Settings\\Alexei\\My Documents\\Visual Studio 2005\\Projects\\Windows" +
                "Application2\\WindowsApplication2\\bin\\Debug\\test22.raw";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(248, 32);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(67, 23);
            this.btnBrowse.TabIndex = 0;
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click_1);
            // 
            // dlgOpenFile
            // 
            this.dlgOpenFile.FileName = "openFileDialog1";
            this.dlgOpenFile.Filter = "RAW Image (*.raw)|*.raw";
            this.dlgOpenFile.Title = "Select source file";
            this.dlgOpenFile.FileOk += new System.ComponentModel.CancelEventHandler(this.dlgOpenFile_FileOk);
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Location = new System.Drawing.Point(5, 175);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(500, 500);
            this.panel1.TabIndex = 4;
            // 
            // btnBlob
            // 
            this.btnBlob.Location = new System.Drawing.Point(602, 136);
            this.btnBlob.Name = "btnBlob";
            this.btnBlob.Size = new System.Drawing.Size(114, 37);
            this.btnBlob.TabIndex = 5;
            this.btnBlob.Text = "Blob";
            this.btnBlob.UseVisualStyleBackColor = true;
            this.btnBlob.Click += new System.EventHandler(this.btnBlob_Click);
            // 
            // btnZoomIn
            // 
            this.btnZoomIn.Location = new System.Drawing.Point(522, 205);
            this.btnZoomIn.Name = "btnZoomIn";
            this.btnZoomIn.Size = new System.Drawing.Size(46, 30);
            this.btnZoomIn.TabIndex = 6;
            this.btnZoomIn.Text = "Zoom";
            this.btnZoomIn.UseVisualStyleBackColor = true;
            this.btnZoomIn.Click += new System.EventHandler(this.btnZoomIn_Click);
            // 
            // txtZoomFactor
            // 
            this.txtZoomFactor.Location = new System.Drawing.Point(522, 179);
            this.txtZoomFactor.Name = "txtZoomFactor";
            this.txtZoomFactor.Size = new System.Drawing.Size(46, 20);
            this.txtZoomFactor.TabIndex = 9;
            this.txtZoomFactor.Text = "1";
            this.txtZoomFactor.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // btnDrawGrid
            // 
            this.btnDrawGrid.Location = new System.Drawing.Point(522, 241);
            this.btnDrawGrid.Name = "btnDrawGrid";
            this.btnDrawGrid.Size = new System.Drawing.Size(46, 39);
            this.btnDrawGrid.TabIndex = 10;
            this.btnDrawGrid.Text = "Draw Grid";
            this.btnDrawGrid.UseVisualStyleBackColor = true;
            this.btnDrawGrid.Click += new System.EventHandler(this.btnDrawGrid_Click);
            // 
            // btnHighlightBlob
            // 
            this.btnHighlightBlob.Enabled = false;
            this.btnHighlightBlob.Location = new System.Drawing.Point(81, 59);
            this.btnHighlightBlob.Name = "btnHighlightBlob";
            this.btnHighlightBlob.Size = new System.Drawing.Size(81, 20);
            this.btnHighlightBlob.TabIndex = 11;
            this.btnHighlightBlob.Text = "Select Blob";
            this.btnHighlightBlob.UseVisualStyleBackColor = true;
            this.btnHighlightBlob.Click += new System.EventHandler(this.btnHighlightBlob_Click);
            // 
            // txtHighlightBlob
            // 
            this.txtHighlightBlob.Enabled = false;
            this.txtHighlightBlob.Location = new System.Drawing.Point(6, 59);
            this.txtHighlightBlob.Name = "txtHighlightBlob";
            this.txtHighlightBlob.Size = new System.Drawing.Size(69, 20);
            this.txtHighlightBlob.TabIndex = 12;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 6);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(69, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Blobs Found:";
            // 
            // lblBlobsFound
            // 
            this.lblBlobsFound.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblBlobsFound.AutoSize = true;
            this.lblBlobsFound.Location = new System.Drawing.Point(84, 6);
            this.lblBlobsFound.Name = "lblBlobsFound";
            this.lblBlobsFound.Size = new System.Drawing.Size(75, 13);
            this.lblBlobsFound.TabIndex = 14;
            this.lblBlobsFound.Text = "<not blobbed>";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tableLayoutPanel2);
            this.groupBox2.Controls.Add(this.tableLayoutPanel1);
            this.groupBox2.Controls.Add(this.txtHighlightBlob);
            this.groupBox2.Controls.Add(this.btnHighlightBlob);
            this.groupBox2.Location = new System.Drawing.Point(602, 179);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(191, 277);
            this.groupBox2.TabIndex = 15;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Blob Info";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.label4, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.lblBlobsFound, 1, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(10, 20);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(162, 26);
            this.tableLayoutPanel2.TabIndex = 18;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 54.26829F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45.73171F));
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label8, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblBlobID, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblAvgColor, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblCenter, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.label9, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblBlobArea, 1, 3);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(7, 99);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 46.875F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 53.125F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(165, 105);
            this.tableLayoutPanel1.TabIndex = 17;
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(4, 7);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(58, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Blob ID#:  ";
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(4, 63);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "Center:";
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(4, 36);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(77, 13);
            this.label8.TabIndex = 16;
            this.label8.Text = "Average Color:";
            // 
            // lblBlobID
            // 
            this.lblBlobID.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblBlobID.AutoSize = true;
            this.lblBlobID.Location = new System.Drawing.Point(92, 7);
            this.lblBlobID.MinimumSize = new System.Drawing.Size(10, 10);
            this.lblBlobID.Name = "lblBlobID";
            this.lblBlobID.Size = new System.Drawing.Size(10, 13);
            this.lblBlobID.TabIndex = 17;
            // 
            // lblAvgColor
            // 
            this.lblAvgColor.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblAvgColor.AutoSize = true;
            this.lblAvgColor.BackColor = System.Drawing.Color.Transparent;
            this.lblAvgColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblAvgColor.Location = new System.Drawing.Point(92, 35);
            this.lblAvgColor.MaximumSize = new System.Drawing.Size(15, 15);
            this.lblAvgColor.MinimumSize = new System.Drawing.Size(15, 15);
            this.lblAvgColor.Name = "lblAvgColor";
            this.lblAvgColor.Size = new System.Drawing.Size(15, 15);
            this.lblAvgColor.TabIndex = 18;
            // 
            // lblCenter
            // 
            this.lblCenter.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCenter.AutoSize = true;
            this.lblCenter.Location = new System.Drawing.Point(92, 63);
            this.lblCenter.MinimumSize = new System.Drawing.Size(10, 10);
            this.lblCenter.Name = "lblCenter";
            this.lblCenter.Size = new System.Drawing.Size(10, 13);
            this.lblCenter.TabIndex = 19;
            // 
            // label9
            // 
            this.label9.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(4, 87);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(32, 13);
            this.label9.TabIndex = 20;
            this.label9.Text = "Area:";
            // 
            // lblBlobArea
            // 
            this.lblBlobArea.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblBlobArea.AutoSize = true;
            this.lblBlobArea.Location = new System.Drawing.Point(92, 87);
            this.lblBlobArea.MinimumSize = new System.Drawing.Size(10, 10);
            this.lblBlobArea.Name = "lblBlobArea";
            this.lblBlobArea.Size = new System.Drawing.Size(10, 13);
            this.lblBlobArea.TabIndex = 21;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.tableLayoutPanel4);
            this.groupBox3.Controls.Add(this.btnSelRun);
            this.groupBox3.Controls.Add(this.txtRunID);
            this.groupBox3.Controls.Add(this.txtSelRow);
            this.groupBox3.Controls.Add(this.txtRow);
            this.groupBox3.Controls.Add(this.tableLayoutPanel3);
            this.groupBox3.Location = new System.Drawing.Point(799, 179);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(197, 276);
            this.groupBox3.TabIndex = 16;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Run Info";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55.34591F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 44.65409F));
            this.tableLayoutPanel4.Controls.Add(this.label10, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.label11, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.label12, 0, 2);
            this.tableLayoutPanel4.Controls.Add(this.label13, 0, 3);
            this.tableLayoutPanel4.Controls.Add(this.lblRunID, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.lblRunCoords, 1, 1);
            this.tableLayoutPanel4.Controls.Add(this.lblRunAvgColor, 1, 2);
            this.tableLayoutPanel4.Controls.Add(this.lblRunLength, 1, 3);
            this.tableLayoutPanel4.Location = new System.Drawing.Point(18, 122);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 4;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(160, 87);
            this.tableLayoutPanel4.TabIndex = 5;
            // 
            // label10
            // 
            this.label10.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(4, 5);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(51, 13);
            this.label10.TabIndex = 0;
            this.label10.Text = "Run ID#:";
            // 
            // label11
            // 
            this.label11.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(4, 27);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(43, 13);
            this.label11.TabIndex = 1;
            this.label11.Text = "Coords:";
            // 
            // label12
            // 
            this.label12.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(4, 48);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(77, 13);
            this.label12.TabIndex = 2;
            this.label12.Text = "Average Color:";
            // 
            // label13
            // 
            this.label13.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(4, 69);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(43, 13);
            this.label13.TabIndex = 3;
            this.label13.Text = "Length:";
            // 
            // lblRunID
            // 
            this.lblRunID.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblRunID.AutoSize = true;
            this.lblRunID.Location = new System.Drawing.Point(91, 5);
            this.lblRunID.MinimumSize = new System.Drawing.Size(10, 10);
            this.lblRunID.Name = "lblRunID";
            this.lblRunID.Size = new System.Drawing.Size(10, 13);
            this.lblRunID.TabIndex = 4;
            // 
            // lblRunCoords
            // 
            this.lblRunCoords.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblRunCoords.AutoSize = true;
            this.lblRunCoords.Location = new System.Drawing.Point(91, 27);
            this.lblRunCoords.MinimumSize = new System.Drawing.Size(10, 10);
            this.lblRunCoords.Name = "lblRunCoords";
            this.lblRunCoords.Size = new System.Drawing.Size(10, 13);
            this.lblRunCoords.TabIndex = 5;
            this.lblRunCoords.Text = " ";
            // 
            // lblRunAvgColor
            // 
            this.lblRunAvgColor.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblRunAvgColor.AutoSize = true;
            this.lblRunAvgColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblRunAvgColor.Location = new System.Drawing.Point(91, 47);
            this.lblRunAvgColor.MaximumSize = new System.Drawing.Size(15, 15);
            this.lblRunAvgColor.MinimumSize = new System.Drawing.Size(15, 15);
            this.lblRunAvgColor.Name = "lblRunAvgColor";
            this.lblRunAvgColor.Size = new System.Drawing.Size(15, 15);
            this.lblRunAvgColor.TabIndex = 6;
            this.lblRunAvgColor.Text = " ";
            // 
            // lblRunLength
            // 
            this.lblRunLength.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblRunLength.AutoSize = true;
            this.lblRunLength.Location = new System.Drawing.Point(91, 69);
            this.lblRunLength.MinimumSize = new System.Drawing.Size(10, 10);
            this.lblRunLength.Name = "lblRunLength";
            this.lblRunLength.Size = new System.Drawing.Size(10, 13);
            this.lblRunLength.TabIndex = 7;
            this.lblRunLength.Text = " ";
            // 
            // btnSelRun
            // 
            this.btnSelRun.Location = new System.Drawing.Point(95, 88);
            this.btnSelRun.Name = "btnSelRun";
            this.btnSelRun.Size = new System.Drawing.Size(83, 20);
            this.btnSelRun.TabIndex = 4;
            this.btnSelRun.Text = "Select Run";
            this.btnSelRun.UseVisualStyleBackColor = true;
            this.btnSelRun.Click += new System.EventHandler(this.btnSelRun_Click);
            // 
            // txtRunID
            // 
            this.txtRunID.Location = new System.Drawing.Point(18, 88);
            this.txtRunID.Name = "txtRunID";
            this.txtRunID.Size = new System.Drawing.Size(57, 20);
            this.txtRunID.TabIndex = 3;
            // 
            // txtSelRow
            // 
            this.txtSelRow.Location = new System.Drawing.Point(92, 19);
            this.txtSelRow.Name = "txtSelRow";
            this.txtSelRow.Size = new System.Drawing.Size(86, 21);
            this.txtSelRow.TabIndex = 2;
            this.txtSelRow.Text = "Select Row";
            this.txtSelRow.UseVisualStyleBackColor = true;
            this.txtSelRow.Click += new System.EventHandler(this.txtSelRow_Click);
            // 
            // txtRow
            // 
            this.txtRow.Location = new System.Drawing.Point(18, 20);
            this.txtRow.Name = "txtRow";
            this.txtRow.Size = new System.Drawing.Size(58, 20);
            this.txtRow.TabIndex = 1;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.label7, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.lblRunsInRow, 1, 0);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(18, 55);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(160, 24);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 5);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(65, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "Runs in line:";
            // 
            // lblRunsInRow
            // 
            this.lblRunsInRow.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblRunsInRow.AutoSize = true;
            this.lblRunsInRow.Location = new System.Drawing.Point(83, 5);
            this.lblRunsInRow.Name = "lblRunsInRow";
            this.lblRunsInRow.Size = new System.Drawing.Size(73, 13);
            this.lblRunsInRow.TabIndex = 1;
            this.lblRunsInRow.Text = "<not bobbed>";
            // 
            // btnHighlightNBlobs
            // 
            this.btnHighlightNBlobs.Location = new System.Drawing.Point(627, 46);
            this.btnHighlightNBlobs.Name = "btnHighlightNBlobs";
            this.btnHighlightNBlobs.Size = new System.Drawing.Size(101, 45);
            this.btnHighlightNBlobs.TabIndex = 17;
            this.btnHighlightNBlobs.Text = "Highlight 1st N Blobs";
            this.btnHighlightNBlobs.UseVisualStyleBackColor = true;
            this.btnHighlightNBlobs.Click += new System.EventHandler(this.btnHighlightNBlobs_Click);
            // 
            // txtNBlobs
            // 
            this.txtNBlobs.Location = new System.Drawing.Point(627, 17);
            this.txtNBlobs.Name = "txtNBlobs";
            this.txtNBlobs.Size = new System.Drawing.Size(100, 20);
            this.txtNBlobs.TabIndex = 18;
            this.txtNBlobs.Text = "10";
            this.txtNBlobs.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // lblSelBlobByCol
            // 
            this.lblSelBlobByCol.AutoSize = true;
            this.lblSelBlobByCol.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblSelBlobByCol.Location = new System.Drawing.Point(19, 22);
            this.lblSelBlobByCol.MaximumSize = new System.Drawing.Size(20, 20);
            this.lblSelBlobByCol.MinimumSize = new System.Drawing.Size(20, 20);
            this.lblSelBlobByCol.Name = "lblSelBlobByCol";
            this.lblSelBlobByCol.Size = new System.Drawing.Size(20, 20);
            this.lblSelBlobByCol.TabIndex = 19;
            // 
            // btnFindBlobByCol
            // 
            this.btnFindBlobByCol.Location = new System.Drawing.Point(119, 40);
            this.btnFindBlobByCol.Name = "btnFindBlobByCol";
            this.btnFindBlobByCol.Size = new System.Drawing.Size(68, 53);
            this.btnFindBlobByCol.TabIndex = 20;
            this.btnFindBlobByCol.Text = "Find Blob By Color and Center";
            this.btnFindBlobByCol.UseVisualStyleBackColor = true;
            this.btnFindBlobByCol.Click += new System.EventHandler(this.btnFindBlobByCol_Click);
            // 
            // txtSelBlobByCenterX
            // 
            this.txtSelBlobByCenterX.Location = new System.Drawing.Point(32, 57);
            this.txtSelBlobByCenterX.Name = "txtSelBlobByCenterX";
            this.txtSelBlobByCenterX.Size = new System.Drawing.Size(69, 20);
            this.txtSelBlobByCenterX.TabIndex = 21;
            this.txtSelBlobByCenterX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtSelBlobByCenterY
            // 
            this.txtSelBlobByCenterY.Location = new System.Drawing.Point(32, 83);
            this.txtSelBlobByCenterY.Name = "txtSelBlobByCenterY";
            this.txtSelBlobByCenterY.Size = new System.Drawing.Size(69, 20);
            this.txtSelBlobByCenterY.TabIndex = 22;
            this.txtSelBlobByCenterY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label15);
            this.groupBox4.Controls.Add(this.label14);
            this.groupBox4.Controls.Add(this.txtSelBlobByCenterY);
            this.groupBox4.Controls.Add(this.txtSelBlobByCenterX);
            this.groupBox4.Controls.Add(this.btnFindBlobByCol);
            this.groupBox4.Controls.Add(this.lblSelBlobByCol);
            this.groupBox4.Location = new System.Drawing.Point(529, 508);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(220, 125);
            this.groupBox4.TabIndex = 23;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Pixel Info";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(10, 59);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(17, 13);
            this.label14.TabIndex = 23;
            this.label14.Text = "X:";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(9, 86);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(17, 13);
            this.label15.TabIndex = 24;
            this.label15.Text = "Y:";
            // 
            // btnNormalize
            // 
            this.btnNormalize.Location = new System.Drawing.Point(350, 101);
            this.btnNormalize.Name = "btnNormalize";
            this.btnNormalize.Size = new System.Drawing.Size(117, 36);
            this.btnNormalize.TabIndex = 24;
            this.btnNormalize.Text = "Normalize";
            this.btnNormalize.UseVisualStyleBackColor = true;
            this.btnNormalize.Click += new System.EventHandler(this.btnNormalize_Click);
            // 
            // frmRAWImage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1016, 741);
            this.Controls.Add(this.btnNormalize);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.txtNBlobs);
            this.Controls.Add(this.btnHighlightNBlobs);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.btnDrawGrid);
            this.Controls.Add(this.txtZoomFactor);
            this.Controls.Add(this.btnZoomIn);
            this.Controls.Add(this.btnBlob);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnDisplay);
            this.MinimumSize = new System.Drawing.Size(1024, 768);
            this.Name = "frmRAWImage";
            this.Text = "RAW Image";
            this.Load += new System.EventHandler(this.frmRAWImage_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnDisplay;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSourceFile;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.OpenFileDialog dlgOpenFile;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtWidth;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtHeight;
        private System.Windows.Forms.Button btnBlob;
        private System.Windows.Forms.Button btnZoomIn;
        private System.Windows.Forms.TextBox txtZoomFactor;
        private System.Windows.Forms.Button btnDrawGrid;
        private System.Windows.Forms.Button btnHighlightBlob;
        private System.Windows.Forms.TextBox txtHighlightBlob;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblBlobsFound;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label lblBlobID;
        private System.Windows.Forms.Label lblAvgColor;
        private System.Windows.Forms.Label lblCenter;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblRunsInRow;
        private System.Windows.Forms.Button txtSelRow;
        private System.Windows.Forms.TextBox txtRow;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button btnSelRun;
        private System.Windows.Forms.TextBox txtRunID;
        private System.Windows.Forms.Label lblRunID;
        private System.Windows.Forms.Label lblRunCoords;
        private System.Windows.Forms.Label lblRunAvgColor;
        private System.Windows.Forms.Label lblRunLength;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label lblBlobArea;
        private System.Windows.Forms.Button btnHighlightNBlobs;
        private System.Windows.Forms.TextBox txtNBlobs;
        private System.Windows.Forms.Label lblSelBlobByCol;
        private System.Windows.Forms.Button btnFindBlobByCol;
        private System.Windows.Forms.TextBox txtSelBlobByCenterX;
        private System.Windows.Forms.TextBox txtSelBlobByCenterY;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Button btnNormalize;
    }
}

