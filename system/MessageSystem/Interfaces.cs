using System;
using System.Collections.Generic;
using System.Text;

namespace Robocup.MessageSystem
{
    public interface MessageSender<T>
    {
        void Post(T t);
    }

    public delegate void ReceiveMessageDelegate<T>(T t);
    public interface MessageReceiver<T>
    {
        /// <summary>
        /// This event is called when a message is received.  Do not modify the parameter, as the same
        /// reference will be passed to all the observers.
        /// </summary>
        event ReceiveMessageDelegate<T> MessageReceived;
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
        public static MessageReceiver<T> CreateClientReceiver<T>(string hostname, int portNum)
        {
            return new ClientMessageReceiver<T>(hostname, portNum);
        }

        public static MessageReceiver<T> CreateServerReceiver<T>(int portNum)
        {
            return new ServerMessageReceiver<T>(portNum);
        }
        public static MessageSender<T> CreateClientSender<T>(string hostname, int portNum)
        {
            return new ClientMessageSender<T>(hostname, portNum);
        }
    }
}
