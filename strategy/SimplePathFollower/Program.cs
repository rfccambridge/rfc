using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;

namespace SimplePathFollower
{
	class Program
	{
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new FollowerForm());
		}
	}
}
