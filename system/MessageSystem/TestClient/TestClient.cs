using System;
using System.Collections.Generic;
using System.Text;
using Robocup.MessageSystem;

namespace TestClient
{
    class TestClientListener
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("press any key to begin connection");
                Console.ReadKey();
                MessageReceiver<string> receiver;
                try
                {
                    receiver = Messages.CreateClientReceiver<string>("localhost", 12345);
                    receiver.MessageReceived += receiver_MessageReceived;
                }
                catch (ConnectionRefusedException)
                {
                    Console.WriteLine("connection refused -- most likely because the server isn't up");
                    continue;
                }
            }
        }

        static void receiver_MessageReceived(string t)
        {
            Console.WriteLine("received message: \"" + t + '"');
        }
    }
    class TestClientSender
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("press any key to begin connection");
                Console.ReadKey();
                MessageSender<string> sender;
                try
                {
                    sender = Messages.CreateClientSender<string>("localhost", 12345);
                }
                catch (ConnectionRefusedException)
                {
                    Console.WriteLine("connection refused -- most likely because the server isn't up");
                    continue;
                }
                int i = 0;
                while (i < 3)
                {
                    sender.Post("this is message number: " + i);
                    Console.WriteLine("posted message number " + i);
                    System.Threading.Thread.Sleep(1000);
                    i++;
                }
            }
        }
    }
}
