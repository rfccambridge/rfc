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
            while (true)
            {
                MessageSender<string> sender = Messages.CreateServerSender<string>(12345);
                for (int i = 0; i < 5; i++)
                {
                    sender.Post("this is message number: " + i);
                    Console.WriteLine("posted message number " + i);
                    System.Threading.Thread.Sleep(500);
                }
                sender.Close();
                Console.ReadKey();
            }
        }
    }
    class TestServerReceiver
    {
        static void Main(string[] args)
        {
            MessageReceiver<string> receiver = Messages.CreateServerReceiver<string>(12345);
            receiver.MessageReceived += receiver_MessageReceived;
            Console.WriteLine("receiving -- press any key to start another");
            Console.ReadKey();
            receiver.Close();
            MessageReceiver<string> receiver2 = Messages.CreateServerReceiver<string>(12345);
            receiver2.MessageReceived += receiver_MessageReceived;
            Console.ReadKey();
            receiver2.Close();
        }

        static void receiver_MessageReceived(string t)
        {
            Console.WriteLine("received message: \"" + t + '"');
        }
    }
}
