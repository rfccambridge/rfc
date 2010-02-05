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
        struct RefboxPacket
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

        public event EventHandler<EventArgs<char>> PacketReceived;

        Thread _receiveThread;        
        EndPoint _endPoint;
        Socket _socket;
        RefboxPacket _lastPacket;
        DateTime _lastReceivedTime;
        object lastPacketLock = new object();

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

            COMMAND_CHAR_TO_NAME.Add('G', "GOAL_SCORED_BLUE");
            COMMAND_CHAR_TO_NAME.Add('D', "DECREASE_GOAL_SCORE_BLUE");
            COMMAND_CHAR_TO_NAME.Add('Y', "YELLOW_CARD_BLUE");
            COMMAND_CHAR_TO_NAME.Add('R', "RED_CARD_BLUE");

        }

        public static string CommandCharToName(char c) {
            string name;
            if (COMMAND_CHAR_TO_NAME.TryGetValue(c, out name))
                return name;
            return "";
        }

        public void Connect(string addr, int port)
        {
            if (_socket != null)
                throw new ApplicationException("Already connected.");

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _endPoint = new IPEndPoint(IPAddress.Any, port);            

            _socket.Bind(_endPoint);
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(IPAddress.Parse(addr)));
        }

        public void Disconnect()
        {
            if (_socket == null)
                throw new ApplicationException("Not connected.");
            if (_receiveThread != null)
                throw new ApplicationException("Must stop before disconnecting.");

            _socket.Close();
            _socket = null;
        }

        public void Start()
        {
            _receiveThread = new Thread(new ThreadStart(loop));
            _receiveThread.Start();
        }

        public void Stop()
        {
            _receiveThread.Abort();
            _receiveThread = null;
        }

        public bool IsReceiving()
        {   
            const int MAX_ELAPSED = 3; // seconds
            lock (lastPacketLock)
            {
                TimeSpan elapsed = DateTime.Now - _lastReceivedTime;
                return elapsed.TotalSeconds <= MAX_ELAPSED;
            }
        }  

        public int GetCmdCounter()
        {
            lock (lastPacketLock)
            {
                return _lastPacket.cmd_counter;
            }
        }

        public char GetLastCommand()
        {
            lock (lastPacketLock)
            {
                return _lastPacket.cmd;
            }
        }

        private void loop()
        {
            RefboxPacket packet = new RefboxPacket();
            byte[] buffer = new byte[packet.getSize()];

            while (true)
            {
                int rcv = _socket.ReceiveFrom(buffer, 0, packet.getSize(), SocketFlags.None, ref _endPoint);

                packet = new RefboxPacket();
                if (rcv == packet.getSize())
                {                    
                    packet.setVals(buffer);

                    lock (lastPacketLock)
                    {
                        _lastReceivedTime = DateTime.Now;
                        _lastPacket = packet;
                    }

                    /*Console.WriteLine("command: " + packet.cmd + " counter: " + packet.cmd_counter
                        + " blue: " + packet.goals_blue + " yellow: " + packet.goals_yellow+
                        " time left: " + packet.time_remaining);*/

                    if (PacketReceived != null)
                        PacketReceived(this, new EventArgs<char>(packet.cmd));
                }
                else
                {
                    Console.WriteLine("MulticastRefBoxListener: received a packet of wrong size:" + rcv +
                                      " (expecting " + packet.getSize() + ")");
                }
            }
        }     
    }
}
