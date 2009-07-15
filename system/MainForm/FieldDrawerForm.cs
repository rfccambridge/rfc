using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Robocup.Core;

namespace Robocup.Utilities
{
    public partial class FieldDrawerForm : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        private MainForm _mainForm;
        public FieldDrawer drawer;
        ICoordinateConverter converter = new BasicCoordinateConverter(500, 30, 30);

        public FieldDrawerForm(MainForm mainForm)
        {
            InitializeComponent();

            _mainForm = mainForm;

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            drawer = new FieldDrawer(null, converter);
            this.Size = new Size(570, 500);
            this.BackColor = Color.Green;
        }

        public FieldDrawerForm(IPredictor predictor)
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

            drawer = new FieldDrawer(predictor, converter);
            this.Size = new Size(570, 500);

            this.BackColor = Color.Green;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            drawer.paintField(e.Graphics);            
        }

        public ICoordinateConverter Converter
        {
            get { return converter; }
        }

        //public void GetGraphics() {
        //    return this.cr

        public int AddArrow(Arrow arrow)
        {
            return drawer.AddArrow(arrow);
        }

        public void ClearArrows()
        {
            drawer.ClearArrows();
        }

        public void RemoveArrow(int arrowID)
        {
            drawer.RemoveArrow(arrowID);
        }

        public int AddPath(RobotPath path)
        {
            return drawer.AddPath(path);
        }

        public void ClearPaths()
        {
            drawer.ClearPaths();
        }

        public void RemovePath(int pathID)
        {
            drawer.RemovePath(pathID);
        }

        private void FieldDrawerForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case 'r': // reload constants
                    Constants.Load();
                    drawer.LoadParameters();
                    break;
            }
        }

        private void FieldDrawerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

    }
}