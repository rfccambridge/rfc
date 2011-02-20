using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using Robocup.Core;

namespace Robocup.CoreRobotics
{
    abstract public class RefBoxHandler : IRefBoxHandler
    {
        public struct RefBoxPacket
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
                RefBoxPacket obj = new RefBoxPacket();
                int len = Marshal.SizeOf(obj);
                IntPtr i = Marshal.AllocHGlobal(len);
                Marshal.Copy(dataBytes, 0, i, len);
                obj = (RefBoxPacket)Marshal.PtrToStructure(i, obj.GetType());
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

        protected RefBoxHandler()
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

        public static string CommandCharToName(char c)
        {
            string name;
            if (COMMAND_CHAR_TO_NAME.TryGetValue(c, out name))
                return name;
            return "";
        }

        protected Thread _handlerThread;
        protected EndPoint _endPoint;
        protected Socket _socket;

        protected RefBoxPacket _lastPacket;
        protected DateTime _lastReceivedTime;
        protected object lastPacketLock = new object();

        abstract public void Connect(string addr, int port);
        abstract protected void Loop();
        
        public void Disconnect()
        {
            if (_socket == null)
                throw new ApplicationException("Not connected.");
            if (_handlerThread != null)
                throw new ApplicationException("Must stop before disconnecting.");

            _socket.Close();
            _socket = null;
        }

        public void Start()
        {
            _handlerThread = new Thread(new ThreadStart(Loop));
            _handlerThread.Start();
        }

        public void Stop()
        {
            _handlerThread.Abort();
            _handlerThread = null;
        }

        public int GetCmdCounter()
        {
            lock (lastPacketLock)
            {
                return _lastPacket.cmd_counter;
            }
        }

        protected RefBoxPacket GetLastPacket()
        {
            lock (lastPacketLock)
            {
                return _lastPacket;
            }
        }

        public char GetLastCommand()
        {
            lock (lastPacketLock)
            {
                return _lastPacket.cmd;
            }
        }

    }

    public class MulticastRefBoxSender : RefBoxHandler
    {
        byte cmd_counter = 0;    // counter for current command
        int goals_blue = 0;      // current score for blue team
        int goals_yellow = 0;    // current score for yellow team
        int time_remaining = 0; // seconds remaining for current game stage (network byte order)
        
        override public void Connect(String addr, int port)
        {
            if (_socket != null)
                throw new ApplicationException("Already connected.");

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPAddress mcastAddr = IPAddress.Parse(addr);
            
            _endPoint = new IPEndPoint(mcastAddr, port);
            _socket.Connect(_endPoint);

            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(mcastAddr));
        }

        protected override void Loop()
        {
            while (true)
            {
                RefBoxPacket lastPacket = GetLastPacket();
                SendPacket(lastPacket);
                System.Threading.Thread.Sleep(1000);
            }
        }

        public void SendCommand(char command)
        {
            lock (lastPacketLock)
            {
                if (command != _lastPacket.cmd)
                    cmd_counter++;
            }

            switch (command)
            {
                case 'g':
                    goals_yellow++;
                    break;
                case 'G':
                    goals_blue++;
                    break;
                default:
                    break;
            }

            RefBoxPacket packet = new RefBoxPacket();
            packet.goals_yellow = (byte)goals_yellow;
            packet.goals_blue = (byte)goals_blue;
            packet.cmd = command;
            packet.cmd_counter = cmd_counter;

            lock (lastPacketLock)
            {
                _lastPacket = packet;
            }

            SendPacket(packet);
        }

        public void SendPacket(RefBoxPacket packet)
        {
            _lastPacket = packet;
            if (_socket == null)
                throw new ApplicationException("Socket not connected.");

            byte[] message = packet.toByte();
            _socket.SendTo(message, _endPoint);
        }
    }

    public class MulticastRefBoxListener : RefBoxHandler, IRefBoxListener
    {
        public event EventHandler<EventArgs<char>> PacketReceived;

        override public void Connect(string addr, int port)
        {
            if (_socket != null)
                throw new ApplicationException("Already connected.");

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _endPoint = new IPEndPoint(IPAddress.Any, port);

            _socket.Bind(_endPoint);
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(IPAddress.Parse(addr)));
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

        override protected void Loop()
        {
            RefBoxPacket packet = new RefBoxPacket();
            byte[] buffer = new byte[packet.getSize()];

            while (true)
            {
                int rcv = _socket.ReceiveFrom(buffer, 0, packet.getSize(), SocketFlags.None, ref _endPoint);

                packet = new RefBoxPacket();
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