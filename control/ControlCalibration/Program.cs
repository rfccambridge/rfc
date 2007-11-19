using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Robocup.MotionControl
{
    static class ModelCalibratorMain
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ModelCalibrator());
        }
    }
    static class DataCollectorMain
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DataCollector());
        }
    }
}