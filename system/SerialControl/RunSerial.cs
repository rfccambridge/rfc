using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;

namespace Robotics.Commander {
    static class RunSerial {
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new RemoteControl());
        }
    }
}
