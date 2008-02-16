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
        static MessageReceiver<RobotCommand> receiver;
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            RemoteControl form = new RemoteControl();
            receiver = Messages.CreateServerReceiver<RobotCommand>(Constants.get<int>("ports", "RemoteControlPort"));
            receiver.MessageReceived += delegate(RobotCommand command)
            {
                //form.Serial.setMotorSpeeds(command.ID, command.speeds);
                form.sendCommand( command );
                
            };
            Application.Run(form);
        }
    }
}
