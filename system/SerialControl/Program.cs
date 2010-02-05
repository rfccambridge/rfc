using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;

using Robocup.Utilities;
using Robocup.Core;
using Robocup.MessageSystem;


namespace Robocup.SerialControl
{
    static class Program
    {        
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);            
            Application.Run(new RemoteControl());
        }
    }
}
