using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Robocup.Core;

namespace Robocup.CoreRobotics
{
    public class MulticastRefBoxListener : IRefBoxListener
    {
        public const char HALT = 'H';
        public const char STOP = 'S';
        public const char READY = ' ';
        public const char START = 's';
        public const char KICKOFF_YELLOW = 'k';
        public const char PENALTY_YELLOW = 'p';
        public const char DIRECT_YELLOW = 'f';
        public const char INDIRECT_YELLOW = 'i';
        public const char TIMEOUT_YELLOW = 't';
        public const char TIMEOUT_END_YELLOW = 'z';
        
        public const char KICKOFF_BLUE = 'K';
        public const char PENALTY_BLUE = 'P';
        public const char DIRECT_BLUE = 'F';
        public const char INDIRECT_BLUE = 'I';
        public const char TIMEOUT_BLUE = 'T';
        public const char TIMEOUT_END_BLUE = 'Z';
        
        public const char CANCEL = 'c';

        public struct RefboxPacket
        {
            public char cmd;
            public byte cmd_counter;    // counter for current command
            public byte goals_blue;      // current score for blue team
            public byte goals_yellow;    // current score for yellow team
            public short time_remaining; // seconds remaining for current game stage (network byte order)
            
            public byte[] toByte()
            {
                int len = Marshal.SizeOf(this);
                byte[] arr = new byte[len];
                IntPtr ptr = Marshal.AllocHGlobal(len);
                Marshal.StructureToPtr(this, ptr, true);
                Marshal.Copy(ptr, arr, 0, len);
                Marshal.FreeHGlobal(ptr);
                return arr;

            }

            public void setVals(byte[] dataBytes)
            {
                RefboxPacket obj = new RefboxPacket();
                int len = Marshal.SizeOf(obj);
                IntPtr i = Marshal.AllocHGlobal(len);
                Marshal.Copy(dataBytes, 0, i, len);
                obj = (RefboxPacket)Marshal.PtrToStructure(i, obj.GetType());
                Marshal.FreeHGlobal(i);
                this = obj;
            }

            public int getSize()
            {
                return Marshal.SizeOf(this);
            }
        };


        class StateObject
        {
            public Socket sock;
            public byte[] buffer;
            public RefboxPacket packet;
            public EndPoint ep;

            public StateObject()
            {
                packet = new RefboxPacket();
                buffer = new byte[packet.getSize()];
            }
        }


        volatile bool running;
        Socket s;
        EndPoint ep;
        StateObject so;
        RefboxPacket packet;

        public MulticastRefBoxListener(string addr, int port)
        {
            packet = new RefboxPacket();
            packet.cmd = 'H';
            s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ep = (EndPoint)new IPEndPoint(IPAddress.Any, port);
            s.Bind(ep);
            s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(IPAddress.Parse(addr)));

            so = new StateObject();
            so.sock = s;
            so.ep = ep;
            running = false;

            start();
        }

        void CloseListener() {

        }

        void ReceiveRefboxPacket(IAsyncResult result)
        {
            StateObject so = (StateObject)result.AsyncState;
            packet = new RefboxPacket();
            if (so.sock.EndReceive(result) == packet.getSize())
            {
                

                packet.setVals(so.buffer);
                /*Console.WriteLine("command: " + packet.cmd + " counter: " + packet.cmd_counter
                    + " blue: " + packet.goals_blue + " yellow: " + packet.goals_yellow+
                    " time left: " + packet.time_remaining);//*/
                
            }

            if (running)
            {                
                so.sock.BeginReceiveFrom(so.buffer, 0, so.packet.getSize(), SocketFlags.None, ref so.ep, new AsyncCallback(ReceiveRefboxPacket), so);                
            }
        }


        public void start()
        {
            running = true;            
            s.BeginReceiveFrom(so.buffer, 0, so.packet.getSize(), SocketFlags.None, ref ep, new AsyncCallback(ReceiveRefboxPacket), so);                        
        }

        public void stop()
        {
            running = false;
        }

        public int getCmdCounter()
        {
            return packet.cmd_counter;
        }

        public char getLastCommand()
        {
            return packet.cmd;
        }

    }
}
