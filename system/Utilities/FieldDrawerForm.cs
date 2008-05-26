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
        FieldDrawer drawer;
        ICoordinateConverter converter = new BasicCoordinateConverter(500, 30, 30);
        public FieldDrawerForm(IPredictor predictor)
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

            drawer = new FieldDrawer(predictor, converter);
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
    }
}