using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace NavigationRacer {
    public partial class FrmBatchTesting : Form {
        private int totalFiles;
        private int totalTests;
        private int currFileIndex;
        private int currTestIndex;

        private RacerForm hostForm;

        public FrmBatchTesting() {
            InitializeComponent();
        }
        public FrmBatchTesting(int numFiles, int totalTests, RacerForm _hostForm) {
            InitializeComponent();

            hostForm = _hostForm;

            totalFiles = numFiles;
            currFileIndex = 1;
            currTestIndex = 0;

            lblFileNum.Text = (currFileIndex + 1).ToString();
            lblTotalFiles.Text = totalFiles.ToString();

            this.totalTests = totalTests;
        }

        public void incrProgress() {
            currTestIndex++;
            if ((currTestIndex * totalFiles) % totalTests == 0)
                currFileIndex++;
            progBar.Value =(int)(((double)currTestIndex / totalTests) * progBar.Maximum);

            lblFileNum.Text = currFileIndex.ToString();
        }

        private void btnStop_Click(object sender, EventArgs e) {
            btnStop.Text = "Stopping...";
            btnStop.Enabled = false;
            hostForm.stopTesting();
        }
    }
}