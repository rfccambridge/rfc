using System;
using System.Collections.Generic;
using System.Text;

namespace Robocup.MessageSystem
{
    public interface MessageSender<T>
    {
        void Post(T t);
        void Close();
    }

    public delegate void ReceiveMessageDelegate<T>(T t);
    public interface MessageReceiver<T>
    {
        /// <summary>
        /// This event is called when a message is received.  Do not modify the parameter, as the same
        /// reference will be passed to all the observers (in an asynchronous manner).
        /// </summary>
        event ReceiveMessageDelegate<T> MessageReceived;
        void Close();
    }

    /// <summary>
    /// All of these return PERSISTENT things that will never go away!
    /// The sockets they open will stay open until it's closed from the other side.
    /// </summary>
    public static partial class Messages
    {
        public static MessageSender<T> CreateServerSender<T>(int portNum)
        {
            return new ServerMessageSender<T>(portNum);
        }
        /// <returns>Returns null if the connection was refused, most likely because there was no process running on the other side.</returns>
        public static MessageReceiver<T> CreateClientReceiver<T>(string hostname, int portNum)
        {
            try
            {
                return new ClientMessageReceiver<T>(hostname, portNum);
            }
            catch (ConnectionRefusedException)
            {
                return null;
            }
        }

        public static MessageReceiver<T> CreateServerReceiver<T>(int portNum)
        {
            return new ServerMessageReceiver<T>(portNum);
        }
        /// <returns>Returns null if the connection was refused, most likely because there was no process running on the other side.</returns>
        public static MessageSender<T> CreateClientSender<T>(string hostname, int portNum)
        {
            try
            {
                return new ClientMessageSender<T>(hostname, portNum);
            }
            catch (ConnectionRefusedException)
            {
                return null;
            }
        }
    }
}
