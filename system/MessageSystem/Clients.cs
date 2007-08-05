using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace Robocup.MessageSystem
{
    class ClientMessageSender<T> : MessageSender<T>
    {
        readonly BasicMessageSender<T> sender;
        public ClientMessageSender(string hostname, int portNum)
        {
            try
            {
                TcpClient client = new TcpClient(hostname, portNum);
                sender = new BasicMessageSender<T>(client);
            }
            catch (System.Net.Sockets.SocketException e)
            {
                if (e.SocketErrorCode == System.Net.Sockets.SocketError.ConnectionRefused)
                {
                    throw new ConnectionRefusedException("the connection was refused", e);
                }
                else
                    throw e;
            }
        }
        public void Post(T t)
        {
            sender.Post(t);
        }

        public void Close()
        {
            sender.Close();
        }
    }
    class ClientMessageReceiver<T> : MessageReceiver<T>
    {
        readonly BasicMessageReceiver<T> receiver;
        public ClientMessageReceiver(string hostname, int portNum)
        {
            try
            {
                TcpClient client = new TcpClient(hostname, portNum);
                receiver = new BasicMessageReceiver<T>(client);
                receiver.MessageReceived += OnMessageReceived;
                receiver.Start();
            }
            catch (System.Net.Sockets.SocketException e)
            {
                if (e.SocketErrorCode == System.Net.Sockets.SocketError.ConnectionRefused)
                {
                    throw new ConnectionRefusedException("the connection was refused", e);
                }
                else
                    throw e;
            }
        }
        private void OnMessageReceived(T t)
        {
            MessageReceived(t);
        }

        public void Close()
        {
            receiver.Close();
        }

        public event ReceiveMessageDelegate<T> MessageReceived;
    }
}
