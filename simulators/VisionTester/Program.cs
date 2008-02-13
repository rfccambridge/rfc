using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Robocup.Utilities;
using Robocup.Core;
namespace Robocup.MotionControl
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        
        static void Main()
        {
            int MESSAGE_SENDER_PORT = Constants.get<int>("ports", "VisionDataPort");
            Robocup.MessageSystem.MessageReceiver<Robocup.Core.VisionMessage> _messageReceiver =
                Robocup.MessageSystem.Messages.CreateClientReceiver<Robocup.Core.VisionMessage>("localhost", MESSAGE_SENDER_PORT);
            _messageReceiver.MessageReceived += new Robocup.MessageSystem.ReceiveMessageDelegate<VisionMessage>(printHandler);
            while (true) { }
        }

        static void printHandler(VisionMessage v) {
            foreach( VisionMessage.RobotData robot in v.OurRobots) {
                Console.WriteLine(robot.ID + " x: " + robot.Position.X + " y: " + robot.Position.Y);
            }
        }

        
    }
}