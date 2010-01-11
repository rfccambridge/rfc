using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace JoystickSample
{
    public partial class Button : UserControl
    {
        public Button()
        {
            InitializeComponent();
        }

        private int buttonId;
        public int ButtonId
        {
            get { return buttonId; }
            set 
            {
                buttonId = value; 
                btnStatus.Text = "Button " + value;
            }
        }

        private bool buttonStatus;
        public bool ButtonStatus
        {
            get { return buttonStatus; }
            set
            {
                buttonStatus = value;
                btnStatus.Checked = value;
            }
        }
    }
}
