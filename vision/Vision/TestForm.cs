using System;
using System.Windows.Forms;

namespace Vision
{
	public partial class TestForm : Form
	{
		public VisionTester Tester { get { return tester; } }
		private readonly VisionTester tester;

		public TestForm()
		{
			tester = new VisionTester();
			InitializeComponent();
		}

		private void btnTest_Click(object sender, EventArgs e)
		{
			tester.TestBall = checkBall.Checked;
			tester.RobotIDs.Clear();
			
			foreach (Control control in Controls)
			{
				if (!(control is CheckBox)) continue;
				if (!control.Name.Contains("Robot")) continue;

				if ((control as CheckBox).Checked) 
					tester.RobotIDs.Add(Convert.ToInt32(control.Tag));
			}
		}
	}
}
