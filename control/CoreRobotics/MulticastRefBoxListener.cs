using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Robocup.Core;
using System.Threading;

namespace Robocup.CoreRobotics
{
    public class MulticastRefBoxListener : IRefBoxListener
    {
        public delegate void OnPacketReceivedCallback(char command);

        // Note: If you change these, don't forget to change the names in the map
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

        private static Dictionary<char, string> COMMAND_CHAR_TO_NAME;
        
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
        volatile bool closing;
        int receivingHeartbeat;
        object cvClosing = new object(); // Condition variable for closing notification

        EndPoint ep;
        StateObject so;
        RefboxPacket packet;                
        OnPacketReceivedCallback onPacketReceivedCallback;

        static MulticastRefBoxListener()
        {
            COMMAND_CHAR_TO_NAME = new Dictionary<char, string>();

            COMMAND_CHAR_TO_NAME.Add('H', "HALT");
            COMMAND_CHAR_TO_NAME.Add('S', "STOP");
            COMMAND_CHAR_TO_NAME.Add(' ', "READY");
            COMMAND_CHAR_TO_NAME.Add('s', "START");
            COMMAND_CHAR_TO_NAME.Add('k', "KICKOFF_YELLOW");
            COMMAND_CHAR_TO_NAME.Add('p', "PENALTY_YELLOW");
            COMMAND_CHAR_TO_NAME.Add('f', "DIRECT_YELLOW");
            COMMAND_CHAR_TO_NAME.Add('i', "INDIRECT_YELLOW");
            COMMAND_CHAR_TO_NAME.Add('t', "TIMEOUT_YELLOW");
            COMMAND_CHAR_TO_NAME.Add('z', "TIMEOUT_END_YELLOW");

            COMMAND_CHAR_TO_NAME.Add('K', "KICKOFF_BLUE");
            COMMAND_CHAR_TO_NAME.Add('P', "PENALTY_BLUE");
            COMMAND_CHAR_TO_NAME.Add('F', "DIRECT_BLUE");
            COMMAND_CHAR_TO_NAME.Add('I', "INDIRECT_BLUE");
            COMMAND_CHAR_TO_NAME.Add('T', "TIMEOUT_BLUE");
            COMMAND_CHAR_TO_NAME.Add('Z', "TIMEOUT_END_BLUE");

            COMMAND_CHAR_TO_NAME.Add('c', "CANCEL");

            COMMAND_CHAR_TO_NAME.Add('g', "GOAL_SCORED_YELLOW");
            COMMAND_CHAR_TO_NAME.Add('d', "DECREASE_GOAL_SCORE_YELLOW");
            COMMAND_CHAR_TO_NAME.Add('y', "YELLOW_CARD_YELLOW");
            COMMAND_CHAR_TO_NAME.Add('r', "RED_CARD_YELLOW");

            COMMAND_CHAR_TO_NAME.Add('g', "GOAL_SCORED_BLUE");
            COMMAND_CHAR_TO_NAME.Add('d', "DECREASE_GOAL_SCORE_BLUE");
            COMMAND_CHAR_TO_NAME.Add('y', "YELLOW_CARD_BLUE");
            COMMAND_CHAR_TO_NAME.Add('r', "RED_CARD_BLUE");

        }

        public static string CommandCharToName(char c) {
            string name;
            if (COMMAND_CHAR_TO_NAME.TryGetValue(c, out name))
                return name;
            return "";
        }

        public MulticastRefBoxListener(string addr, int port)
            : this(addr, port, null) { }

        public MulticastRefBoxListener(string addr, int port, OnPacketReceivedCallback onPacketReceivedCallback)
        {
            packet = new RefboxPacket();
            packet.cmd = 'H';
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ep = (EndPoint)new IPEndPoint(IPAddress.Any, port);
            s.Bind(ep);
            s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(IPAddress.Parse(addr)));
            
            so = new StateObject();
            so.sock = s;
            so.ep = ep;
            running = false;

            start();

            this.onPacketReceivedCallback = onPacketReceivedCallback;            
        }

        public void close() {
            if (running)
                throw new ApplicationException("Cannot close MulticastRefBoxListener while listening");
            if (closing)
                throw new ApplicationException("Closing already in progress!");
            
            lock (cvClosing)
            {
                // Check if the receiving threads are actually running -- sort of lame..
                int hb1 = receivingHeartbeat;
                Thread.Sleep(100);     
                int hb2 = receivingHeartbeat;

                // "Request" closing
                closing = true;                

                // If receiving threads actually run, wait until closing routines complete
                if (hb1 != hb2)
                {
                    Monitor.Wait(cvClosing);
                }
                else
                {
                    if (so.sock != null)
                        so.sock.Close();
                    so.sock = null;
                }
            }
        }

        void ReceiveRefboxPacket(IAsyncResult result)
        {
            receivingHeartbeat++;

            if (so.sock == null)
                return;

            lock (cvClosing)
            {
                if (closing)
                {

                    // I think multiple receiving threads are running -- only one needs to
                    // close the socket.
                    if (so.sock != null)
                    {
                        so.sock.Close();
                        so.sock = null;
                        Monitor.Pulse(cvClosing);
                    }
                    return;
                }
            }

            StateObject sobj = (StateObject)result.AsyncState;
            packet = new RefboxPacket();
            if (sobj.sock.EndReceive(result) == packet.getSize())
            {
                packet.setVals(sobj.buffer);

                /*Console.WriteLine("command: " + packet.cmd + " counter: " + packet.cmd_counter
                    + " blue: " + packet.goals_blue + " yellow: " + packet.goals_yellow+
                    " time left: " + packet.time_remaining);//*/

                if (onPacketReceivedCallback != null)
                    onPacketReceivedCallback(packet.cmd);
            }

            if (running && !closing)
            {                
                so.sock.BeginReceiveFrom(sobj.buffer, 0, sobj.packet.getSize(), SocketFlags.None, ref sobj.ep, new AsyncCallback(ReceiveRefboxPacket), sobj);
            }
        }


        public void start()
        {
            if (closing)
                throw new ApplicationException("Closing in progress, cannot start!");

            running = true;            
            so.sock.BeginReceiveFrom(so.buffer, 0, so.packet.getSize(), SocketFlags.None, ref ep, new AsyncCallback(ReceiveRefboxPacket), so);                        
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
