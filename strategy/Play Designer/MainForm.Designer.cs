namespace RobocupPlays
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.definitionsToolStrip = new System.Windows.Forms.ToolStrip();
            this.toolstripAddRobot = new System.Windows.Forms.ToolStripButton();
            this.toolstripAddBall = new System.Windows.Forms.ToolStripButton();
            this.toolstripAddPoint = new System.Windows.Forms.ToolStripButton();
            this.toolstripAddCircle = new System.Windows.Forms.ToolStripButton();
            this.toolstripMoveObjects = new System.Windows.Forms.ToolStripButton();
            this.toolstripDrawLine = new System.Windows.Forms.ToolStripButton();
            this.toolstripPlaceIntersection = new System.Windows.Forms.ToolStripButton();
            this.toolstriponsAddClosestDefinition = new System.Windows.Forms.ToolStripButton();
            this.toolstripAddCondition = new System.Windows.Forms.ToolStripButton();
            this.toolstripAddAction = new System.Windows.Forms.ToolStripButton();
            this.toolstripEditObject = new System.Windows.Forms.ToolStripButton();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.drawLinesButton = new System.Windows.Forms.ToolStripMenuItem();
            this.drawCirclesButton = new System.Windows.Forms.ToolStripMenuItem();
            this.drawPointsButton = new System.Windows.Forms.ToolStripMenuItem();
            this.drawDefinitionsButton = new System.Windows.Forms.ToolStripMenuItem();
            this.drawActionsButton = new System.Windows.Forms.ToolStripMenuItem();
            this.drawConditionsButton = new System.Windows.Forms.ToolStripMenuItem();
            this.toolstripPlayType = new System.Windows.Forms.ToolStripComboBox();
            this.saveToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.openToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.toolStripContainer2 = new System.Windows.Forms.ToolStripContainer();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.definitionsToolStrip.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.toolStripContainer2.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // definitionsToolStrip
            // 
            this.definitionsToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.definitionsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolstripAddRobot,
            this.toolstripAddBall,
            this.toolstripAddPoint,
            this.toolstripAddCircle,
            this.toolstripMoveObjects,
            this.toolstripDrawLine,
            this.toolstripPlaceIntersection,
            this.toolstriponsAddClosestDefinition,
            this.toolstripAddCondition,
            this.toolstripAddAction,
            this.toolstripEditObject,
            this.toolStripDropDownButton1,
            this.toolstripPlayType,
            this.saveToolStripButton,
            this.openToolStripButton});
            this.definitionsToolStrip.Location = new System.Drawing.Point(3, 0);
            this.definitionsToolStrip.Name = "definitionsToolStrip";
            this.definitionsToolStrip.Size = new System.Drawing.Size(510, 25);
            this.definitionsToolStrip.TabIndex = 0;
            this.definitionsToolStrip.Text = "toolStrip1";
            // 
            // toolstripAddRobot
            // 
            this.toolstripAddRobot.Checked = true;
            this.toolstripAddRobot.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolstripAddRobot.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolstripAddRobot.Image = ((System.Drawing.Image)(resources.GetObject("toolstripAddRobot.Image")));
            this.toolstripAddRobot.ImageTransparentColor = System.Drawing.Color.White;
            this.toolstripAddRobot.Name = "toolstripAddRobot";
            this.toolstripAddRobot.Size = new System.Drawing.Size(23, 22);
            this.toolstripAddRobot.Text = "Add Robot";
            this.toolstripAddRobot.Click += new System.EventHandler(this.actionToolstripButton_Click);
            // 
            // toolstripAddBall
            // 
            this.toolstripAddBall.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolstripAddBall.Image = global::RobocupPlays.Properties.Resources.ballicon;
            this.toolstripAddBall.ImageTransparentColor = System.Drawing.Color.White;
            this.toolstripAddBall.Name = "toolstripAddBall";
            this.toolstripAddBall.Size = new System.Drawing.Size(23, 22);
            this.toolstripAddBall.Text = "Place Ball";
            this.toolstripAddBall.Click += new System.EventHandler(this.actionToolstripButton_Click);
            // 
            // toolstripAddPoint
            // 
            this.toolstripAddPoint.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolstripAddPoint.Image = global::RobocupPlays.Properties.Resources.pointicon;
            this.toolstripAddPoint.ImageTransparentColor = System.Drawing.Color.White;
            this.toolstripAddPoint.Name = "toolstripAddPoint";
            this.toolstripAddPoint.Size = new System.Drawing.Size(23, 22);
            this.toolstripAddPoint.Text = "Add a Fixed Point";
            this.toolstripAddPoint.Click += new System.EventHandler(this.actionToolstripButton_Click);
            // 
            // toolstripAddCircle
            // 
            this.toolstripAddCircle.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolstripAddCircle.Image = global::RobocupPlays.Properties.Resources.circleicon;
            this.toolstripAddCircle.ImageTransparentColor = System.Drawing.Color.White;
            this.toolstripAddCircle.Name = "toolstripAddCircle";
            this.toolstripAddCircle.Size = new System.Drawing.Size(23, 22);
            this.toolstripAddCircle.Text = "Add Circle";
            this.toolstripAddCircle.Click += new System.EventHandler(this.actionToolstripButton_Click);
            // 
            // toolstripMoveObjects
            // 
            this.toolstripMoveObjects.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolstripMoveObjects.Image = global::RobocupPlays.Properties.Resources.handicon;
            this.toolstripMoveObjects.ImageTransparentColor = System.Drawing.Color.White;
            this.toolstripMoveObjects.Name = "toolstripMoveObjects";
            this.toolstripMoveObjects.Size = new System.Drawing.Size(23, 22);
            this.toolstripMoveObjects.Text = "Move Objects";
            this.toolstripMoveObjects.Click += new System.EventHandler(this.actionToolstripButton_Click);
            // 
            // toolstripDrawLine
            // 
            this.toolstripDrawLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolstripDrawLine.Image = global::RobocupPlays.Properties.Resources.lineicon;
            this.toolstripDrawLine.ImageTransparentColor = System.Drawing.Color.White;
            this.toolstripDrawLine.Name = "toolstripDrawLine";
            this.toolstripDrawLine.Size = new System.Drawing.Size(23, 22);
            this.toolstripDrawLine.Text = "Draw Line";
            this.toolstripDrawLine.Click += new System.EventHandler(this.actionToolstripButton_Click);
            // 
            // toolstripPlaceIntersection
            // 
            this.toolstripPlaceIntersection.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolstripPlaceIntersection.Image = global::RobocupPlays.Properties.Resources.intersectionicon;
            this.toolstripPlaceIntersection.ImageTransparentColor = System.Drawing.Color.White;
            this.toolstripPlaceIntersection.Name = "toolstripPlaceIntersection";
            this.toolstripPlaceIntersection.Size = new System.Drawing.Size(23, 22);
            this.toolstripPlaceIntersection.Text = "Place Intersection";
            this.toolstripPlaceIntersection.Click += new System.EventHandler(this.actionToolstripButton_Click);
            // 
            // toolstriponsAddClosestDefinition
            // 
            this.toolstriponsAddClosestDefinition.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolstriponsAddClosestDefinition.Image = ((System.Drawing.Image)(resources.GetObject("toolstriponsAddClosestDefinition.Image")));
            this.toolstriponsAddClosestDefinition.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolstriponsAddClosestDefinition.Name = "toolstriponsAddClosestDefinition";
            this.toolstriponsAddClosestDefinition.Size = new System.Drawing.Size(23, 22);
            this.toolstriponsAddClosestDefinition.Text = "Add \"Closest\" Definition";
            this.toolstriponsAddClosestDefinition.Click += new System.EventHandler(this.actionToolstripButton_Click);
            // 
            // toolstripAddCondition
            // 
            this.toolstripAddCondition.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolstripAddCondition.Image = ((System.Drawing.Image)(resources.GetObject("toolstripAddCondition.Image")));
            this.toolstripAddCondition.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolstripAddCondition.Name = "toolstripAddCondition";
            this.toolstripAddCondition.Size = new System.Drawing.Size(56, 22);
            this.toolstripAddCondition.Text = "Condition";
            this.toolstripAddCondition.Click += new System.EventHandler(this.toolstripAddCommand_Click);
            // 
            // toolstripAddAction
            // 
            this.toolstripAddAction.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolstripAddAction.Image = ((System.Drawing.Image)(resources.GetObject("toolstripAddAction.Image")));
            this.toolstripAddAction.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolstripAddAction.Name = "toolstripAddAction";
            this.toolstripAddAction.Size = new System.Drawing.Size(41, 22);
            this.toolstripAddAction.Text = "Action";
            this.toolstripAddAction.Click += new System.EventHandler(this.toolstripAddCommand_Click);
            // 
            // toolstripEditObject
            // 
            this.toolstripEditObject.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolstripEditObject.Image = ((System.Drawing.Image)(resources.GetObject("toolstripEditObject.Image")));
            this.toolstripEditObject.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolstripEditObject.Name = "toolstripEditObject";
            this.toolstripEditObject.Size = new System.Drawing.Size(23, 22);
            this.toolstripEditObject.Text = "Edit an Object";
            this.toolstripEditObject.Click += new System.EventHandler(this.actionToolstripButton_Click);
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.drawLinesButton,
            this.drawCirclesButton,
            this.drawPointsButton,
            this.drawDefinitionsButton,
            this.drawActionsButton,
            this.drawConditionsButton});
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(45, 22);
            this.toolStripDropDownButton1.Text = "Draw";
            // 
            // drawLinesButton
            // 
            this.drawLinesButton.Checked = true;
            this.drawLinesButton.CheckOnClick = true;
            this.drawLinesButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.drawLinesButton.Name = "drawLinesButton";
            this.drawLinesButton.Size = new System.Drawing.Size(124, 22);
            this.drawLinesButton.Text = "Lines";
            this.drawLinesButton.Click += new System.EventHandler(this.selectdraw_click);
            // 
            // drawCirclesButton
            // 
            this.drawCirclesButton.Checked = true;
            this.drawCirclesButton.CheckOnClick = true;
            this.drawCirclesButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.drawCirclesButton.Name = "drawCirclesButton";
            this.drawCirclesButton.Size = new System.Drawing.Size(124, 22);
            this.drawCirclesButton.Text = "Circles";
            this.drawCirclesButton.Click += new System.EventHandler(this.selectdraw_click);
            // 
            // drawPointsButton
            // 
            this.drawPointsButton.Checked = true;
            this.drawPointsButton.CheckOnClick = true;
            this.drawPointsButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.drawPointsButton.Name = "drawPointsButton";
            this.drawPointsButton.Size = new System.Drawing.Size(124, 22);
            this.drawPointsButton.Text = "Points";
            this.drawPointsButton.Click += new System.EventHandler(this.selectdraw_click);
            // 
            // drawDefinitionsButton
            // 
            this.drawDefinitionsButton.Checked = true;
            this.drawDefinitionsButton.CheckOnClick = true;
            this.drawDefinitionsButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.drawDefinitionsButton.Name = "drawDefinitionsButton";
            this.drawDefinitionsButton.Size = new System.Drawing.Size(124, 22);
            this.drawDefinitionsButton.Text = "Definitions";
            this.drawDefinitionsButton.Click += new System.EventHandler(this.selectdraw_click);
            // 
            // drawActionsButton
            // 
            this.drawActionsButton.Checked = true;
            this.drawActionsButton.CheckOnClick = true;
            this.drawActionsButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.drawActionsButton.Name = "drawActionsButton";
            this.drawActionsButton.Size = new System.Drawing.Size(124, 22);
            this.drawActionsButton.Text = "Actions";
            this.drawActionsButton.Click += new System.EventHandler(this.selectdraw_click);
            // 
            // drawConditionsButton
            // 
            this.drawConditionsButton.Name = "drawConditionsButton";
            this.drawConditionsButton.Size = new System.Drawing.Size(124, 22);
            this.drawConditionsButton.Text = "Conditions";
            // 
            // toolstripPlayType
            // 
            this.toolstripPlayType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolstripPlayType.Items.AddRange(new object[] {
            "Normal",
            "Set Play",
            "Offense",
            "Defense",
            "Goalie"});
            this.toolstripPlayType.Name = "toolstripPlayType";
            this.toolstripPlayType.Size = new System.Drawing.Size(101, 25);
            this.toolstripPlayType.SelectedIndexChanged += new System.EventHandler(this.toolstripPlayType_SelectedIndexChanged);
            // 
            // saveToolStripButton
            // 
            this.saveToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.saveToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("saveToolStripButton.Image")));
            this.saveToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveToolStripButton.Name = "saveToolStripButton";
            this.saveToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.saveToolStripButton.Text = "&Save";
            this.saveToolStripButton.Click += new System.EventHandler(this.toolstripSave_Click);
            // 
            // openToolStripButton
            // 
            this.openToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.openToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripButton.Image")));
            this.openToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openToolStripButton.Name = "openToolStripButton";
            this.openToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.openToolStripButton.Text = "&Open";
            this.openToolStripButton.Click += new System.EventHandler(this.openToolStripButton_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 433);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(551, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.DefaultExt = "txt";
            this.saveFileDialog.Filter = "Outputted Files (*.txt) | *.txt|All Files | *.*";
            this.saveFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.saveFileDialog_FileOk);
            // 
            // toolStripContainer2
            // 
            this.toolStripContainer2.BottomToolStripPanelVisible = false;
            // 
            // toolStripContainer2.ContentPanel
            // 
            this.toolStripContainer2.ContentPanel.Size = new System.Drawing.Size(551, 0);
            this.toolStripContainer2.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolStripContainer2.LeftToolStripPanelVisible = false;
            this.toolStripContainer2.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer2.Name = "toolStripContainer2";
            this.toolStripContainer2.RightToolStripPanelVisible = false;
            this.toolStripContainer2.Size = new System.Drawing.Size(551, 25);
            this.toolStripContainer2.TabIndex = 3;
            this.toolStripContainer2.Text = "toolStripContainer2";
            // 
            // toolStripContainer2.TopToolStripPanel
            // 
            this.toolStripContainer2.TopToolStripPanel.Controls.Add(this.definitionsToolStrip);
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "txt";
            this.openFileDialog.Filter = "Plays (*.txt) |*.txt|All Files | *.*";
            this.openFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog_FileOk);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::RobocupPlays.Properties.Resources.field_drawing_quer;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(551, 455);
            this.Controls.Add(this.toolStripContainer2);
            this.Controls.Add(this.statusStrip1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "Play Designer";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.MainForm_Paint);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseUp);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseMove);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseDown);
            this.definitionsToolStrip.ResumeLayout(false);
            this.definitionsToolStrip.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.toolStripContainer2.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer2.TopToolStripPanel.PerformLayout();
            this.toolStripContainer2.ResumeLayout(false);
            this.toolStripContainer2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip definitionsToolStrip;
        private System.Windows.Forms.ToolStripButton toolstripAddRobot;
        private System.Windows.Forms.ToolStripButton toolstripMoveObjects;
        private System.Windows.Forms.ToolStripButton toolstripDrawLine;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripButton toolstripPlaceIntersection;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.ToolStripButton toolstriponsAddClosestDefinition;
        private System.Windows.Forms.ToolStripContainer toolStripContainer2;
        private System.Windows.Forms.ToolStripButton toolstripAddBall;
        private System.Windows.Forms.ToolStripButton toolstripAddCondition;
        private System.Windows.Forms.ToolStripButton toolstripAddAction;
        private System.Windows.Forms.ToolStripButton toolstripAddPoint;
        private System.Windows.Forms.ToolStripButton toolstripAddCircle;
        private System.Windows.Forms.ToolStripComboBox toolstripPlayType;
        private System.Windows.Forms.ToolStripButton saveToolStripButton;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem drawLinesButton;
        private System.Windows.Forms.ToolStripMenuItem drawCirclesButton;
        private System.Windows.Forms.ToolStripMenuItem drawPointsButton;
        private System.Windows.Forms.ToolStripMenuItem drawDefinitionsButton;
        private System.Windows.Forms.ToolStripMenuItem drawConditionsButton;
        private System.Windows.Forms.ToolStripMenuItem drawActionsButton;
        private System.Windows.Forms.ToolStripButton toolstripEditObject;
        private System.Windows.Forms.ToolStripButton openToolStripButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
    }
}

