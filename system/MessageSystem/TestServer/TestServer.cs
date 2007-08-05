using System;
using System.Collections.Generic;
using System.Text;
using Robocup.MessageSystem;

namespace TestServer
{
    class TestServerSender
    {
        static void Main(string[] args)
        {
            MessageSender<string> sender = Messages.CreateServerSender<string>(12345);
            int i=0;
            while (true)
            {
                sender.Post("this is message number: " + i);
                Console.WriteLine("posted message number " + i);
                System.Threading.Thread.Sleep(1000);
                i++;
            }
        }
    }
    class TestServerReceiver
    {
        static void Main(string[] args)
        {
            MessageReceiver<string> receiver = Messages.CreateServerReceiver<string>(12345);
            receiver.MessageReceived += receiver_MessageReceived;
            Console.WriteLine("receiving -- press any key to quit");
            Console.ReadKey();
        }

        static void receiver_MessageReceived(string t)
        {
            Console.WriteLine("received message: \"" + t + '"');
        }
    }
}
