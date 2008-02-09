using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;

using Robocup.Utilities;
using Robocup.Core;
using Robocup.MessageSystem;


namespace Robotics.Commander
{
    static class RunSerial
    {
        static MessageReceiver<WheelCommand> receiver;
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            RemoteControl form = new RemoteControl();
            receiver = Messages.CreateServerReceiver<WheelCommand>(Constants.get<int>("ports", "RemoteControlPort"));
            receiver.MessageReceived += delegate(WheelCommand command)
            {
                //form.Serial.setMotorSpeeds(command.ID, command.speeds);
                form.sendMove(command.ID, command.speeds);
            };
            Application.Run(form);
        }
    }
}
