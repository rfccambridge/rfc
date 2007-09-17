using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace Robocup.MessageSystem
{
    class BasicMessageSender<T> : MessageSender<T>
    {
        readonly TcpClient client;
        readonly NetworkStream stream;
        private object post_lock = new object();
        volatile bool done = false;
        public BasicMessageSender(TcpClient client)
        {
            this.client = client;
            stream = client.GetStream();
        }
        public void Post(T t)
        {
            lock (post_lock)
            {
                if (!done)
                {
                    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter f =
                        new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                    try
                    {
                        f.Serialize(stream, t);
                    }
                    catch (System.IO.IOException e)
                    {
                        SocketException se = e.InnerException as SocketException;
                        if (se == null)
                            throw e;
                        if (se.SocketErrorCode == SocketError.ConnectionReset
                             || se.SocketErrorCode == SocketError.ConnectionAborted)
                        {
                            done = true;
                            if (OnDone != null)
                                OnDone.BeginInvoke(this, null, null);
                        }
                        else
                            throw e;
                    }
                }
            }
        }

        public void Close()
        {
            stream.Close();
            client.Close();
            client.Client.Close();
            Console.WriteLine("worker closed");
            GC.SuppressFinalize(this);
        }
        ~BasicMessageSender()
        {
            this.Close();
        }

        public delegate void DoneHandler(BasicMessageSender<T> doneItem);
        public event DoneHandler OnDone;
    }
    class BasicMessageReceiver<T> : MessageReceiver<T>
    {
        public event ReceiveMessageDelegate<T> MessageReceived;
        readonly TcpClient client;
        readonly Thread thread;
        readonly NetworkStream stream;
        public BasicMessageReceiver(TcpClient client)
        {
            this.client = client;
            stream = client.GetStream();
            thread = new Thread(Run);
        }
        public void Start()
        {
            thread.Start();
        }
        private void Run(object o)
        {
            Console.WriteLine("worker instantiated, listening for data...");

            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter f =
                new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            while (true)
            {
                T obj;
                try
                {
                    obj = (T)f.Deserialize(stream);
                }
                catch (System.IO.IOException e)
                {
                    SocketException inner = e.InnerException as SocketException;
                    if (inner == null)
                        throw e;
                    if (inner.SocketErrorCode == SocketError.ConnectionReset ||
                        inner.SocketErrorCode == SocketError.Interrupted)
                    {
                        if (OnDone != null)
                            OnDone.BeginInvoke(this, null, null);
                        return;
                    }
                    throw e;
                }
                //This is a very brittle way of saying "if the client has closed the connection, then
                //this guy's job is done".
                catch (System.Runtime.Serialization.SerializationException se)
                {
                    if (se.Message == "End of Stream encountered before parsing was completed.")
                    {
                        if (OnDone != null)
                            OnDone.BeginInvoke(this, null, null);
                        return;
                    }
                    else
                        throw se;
                }
                ReceiveMessageDelegate<T> sendTo = MessageReceived;
                if (sendTo != null)
                {
                    sendTo(obj);
                }
            }
        }

        public void Close()
        {
            stream.Close();
            client.Close();
            client.Client.Close();
            Console.WriteLine("worker closed");
            GC.SuppressFinalize(this);
        }
        ~BasicMessageReceiver()
        {
            this.Close();
        }

        public delegate void DoneHandler(BasicMessageReceiver<T> doneItem);
        public event DoneHandler OnDone;
    }
}
