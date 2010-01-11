using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace JoystickSample
{
    public partial class Axis : UserControl
    {
        public Axis()
        {
            InitializeComponent();
        }

        private int axisPos = 32767;
        public int AxisPos
        {
            set 
            {
                lblAxisName.Text = "Axis: " + axisId + "  Value: " + value;
                tbAxisPos.Value = value;
                axisPos = value; 
            }
        }

        private int axisId = 0;
        public int AxisId
        {
            set 
            {
                lblAxisName.Text = "Axis: " + value + "  Value: " + axisPos;
                axisId = value; 
            }
            get
            {
                return axisId;
            }
        }


    }
}
