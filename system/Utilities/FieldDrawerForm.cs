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
    public partial class FieldDrawerForm : Form
    {
        public FieldDrawer drawer;
        ICoordinateConverter converter = new BasicCoordinateConverter(500, 30, 30);
        
        public FieldDrawerForm(IPredictor predictor)
            : this(predictor, null)
        {
        }

        public FieldDrawerForm(IPredictor predictor, String [] playNames)
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

            drawer = new FieldDrawer(predictor, converter, playNames);
            this.Size = new Size(560, 500);
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

    }
}