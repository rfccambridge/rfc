using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Vision {
    public partial class HelpForm : Form {
        public HelpForm() {
            InitializeComponent();
        }

        private void HelpForm_FormClosing(object sender, FormClosingEventArgs e) {
            e.Cancel = true;
            Hide();
        }
    }
}