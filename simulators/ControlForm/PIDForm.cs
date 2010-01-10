using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Robocup.MotionControl;
using Robocup.CoreRobotics;

namespace Robocup.ControlForm
{
    public partial class PIDForm : Form
    {
        public PIDForm()
        {
            InitializeComponent();
        }

        private void BtnSetPID_Click(object sender, EventArgs e)
        {
            DOF_Constants XYPID = new DOF_Constants(EditXYP.Text, EditXYI.Text, EditXYD.Text, XYVelocity.Checked ? "0" : "1");
            DOF_Constants ThetaPID = new DOF_Constants(EditTP.Text, EditTI.Text, EditTD.Text, "1");

            TangentBugFeedbackMotionPlanner.pathdriver.UpdateConstants(Int32.Parse(EditID.Text), XYPID, ThetaPID, IsShort.Checked, false);
        }

        private void BtnGetPID_Click(object sender, EventArgs e)
        {
            DOF_Constants XYPID, ThetaPID;
            TangentBugFeedbackMotionPlanner.pathdriver.GetConstants(Int32.Parse(EditID.Text), IsShort.Checked, out XYPID, out ThetaPID);

            EditXYP.Text = XYPID.P.ToString();
            EditXYI.Text = XYPID.I.ToString();
            EditXYD.Text = XYPID.D.ToString();

            XYVelocity.Checked = (XYPID.ALPHA == 0.0);

            EditTP.Text = ThetaPID.P.ToString();
            EditTI.Text = ThetaPID.I.ToString();
            EditTD.Text = ThetaPID.D.ToString();
        }

        private void EditID_TextChanged(object sender, EventArgs e)
        {
            BtnGetPID_Click(sender, new EventArgs());
        }

        private void PIDForm_Shown(object sender, EventArgs e)
        {
            BtnGetPID_Click(sender, new EventArgs());
        }

        private void btnSavePID_Click(object sender, EventArgs e)
        {
            DOF_Constants XYPID = new DOF_Constants(EditXYP.Text, EditXYI.Text, EditXYD.Text, XYVelocity.Checked ? "0" : "1");
            DOF_Constants ThetaPID = new DOF_Constants(EditTP.Text, EditTI.Text, EditTD.Text, "1");

            TangentBugFeedbackMotionPlanner.pathdriver.UpdateConstants(Int32.Parse(EditID.Text), XYPID, ThetaPID, IsShort.Checked, true);
        }

    }
}