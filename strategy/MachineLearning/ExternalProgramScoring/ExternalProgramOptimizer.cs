using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace MachineLearning.ExternalProgramScoring
{
    class ExternalProgramOptimizer
    {
        static int Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            return 0;
        }
    }
}
