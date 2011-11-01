using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.IO.Ports;
using Robocup.Utilities;

namespace Robocup.SerialControl
{
    public enum BoardType { Brushlees, Auxiliary };
    public enum MessageType { EncoderSpew, CapVoltage, BatteryVoltage };

    /// <summary>
    /// Protocol for received data:
    /// First five bytes are a header.
    /// (numgroups) groups follow, each of which is (group_size) bytes large
    /// In current implementation, group_size == 8
    /// EncoderSpew:
    /// bytes 1,2 - High and Low part of encoder packet
    /// byte 3 - Duty cycle high
    /// byte 4 - Duty cycle low
    /// byte 5 - Wheel command
    /// bytes 6,7,8 - empty (pkt stats not sent anymore)
    /// CapVoltage:
    /// bytes 1,2 - High and Low part of encoder votlage
    /// the rest - empty
    /// </summary>
    public class SerialInputMessage
    {
        public static readonly int SUBPKT_SIZE = 8;

        public SerialInputMessage(char botID, char boardID, byte[] payload, int start_ind)
        {
            RobotID = botID - '0';
            BoardType = boardID == 'v' ? BoardType.Auxiliary : BoardType.Brushlees;
            if (BoardType == BoardType.Brushlees)
                MessageType = MessageType.EncoderSpew;
            else
                MessageType = MessageType.CapVoltage; // Battery votlage niy

            switch (MessageType)
            {
                case MessageType.EncoderSpew:
                    Encoder = (Int16)(((UInt16)(payload[start_ind]) << 8) + //Hi
                                                    (UInt16)(payload[start_ind + 1])); //Lo
                    Duty = (int)(((Int16)payload[start_ind + 2] << 8) +
                                   ((Int16)payload[start_ind + 3]));
                    WheelCommand = (sbyte)payload[start_ind + 4];
                    PktsReceived = (byte)payload[start_ind + 5];
                    PktsAccepted = (byte)payload[start_ind + 6];
                    PktsMismatched = (byte)payload[start_ind + 7];
                    break;
                case MessageType.CapVoltage:
                    CapVoltage = (Int16)(((UInt16)(payload[start_ind]) << 8) + //Hi
                                                    (UInt16)(payload[start_ind + 1])); //Lo
                    CapVoltage -= 300;
                    break;
            }
        }

        public int RobotID { get; private set; }
        public BoardType BoardType { get; private set; }
        public MessageType MessageType { get; private set; }

        public int Encoder { get; private set; }
        public int Error { get; private set; }
        public int WheelCommand { get; private set; }
        public int Duty { get; private set; }
        public int Extra { get; private set; }
        public int Extra2 { get; private set; }
        public int PktsReceived { get; private set; }
        public int PktsAccepted { get; private set; }
        public int PktsMismatched { get; private set; }

        public int CapVoltage { get; private set; }

        static public string ToStringHeader()
        {
            return "enc\tduty\tcmd\trcv\tacc\tmism";
        }
        public override string ToString()
        {
            return Encoder + "\t" + Duty + "\t" + WheelCommand + "\t" +
                PktsReceived + "\t" + PktsAccepted + "\t" + PktsMismatched;
        }
    }

    class SerialInput
    {
        public event Action<SerialInputMessage[]> ValueReceived;
        SerialPort serialport = null;
        bool stopReceiving;
        uint pktsAccepted, pktsMismatched, pktsReceived;

        public static readonly int HEADER_LEN = 3; // chksum, botID, address (\\H is not counted, it doesn't end up in data variable)
        public static readonly int FOOTER_LEN = 2; // '\\', 'E'
        public static readonly int NUM_SUBPKTS = 1;
        public static readonly int PAYLOAD_SIZE = NUM_SUBPKTS * SerialInputMessage.SUBPKT_SIZE;

        public void Open(string port)
        {
            if (serialport != null)
                throw new ApplicationException("Already have a port open.");
            serialport = Robocup.Utilities.SerialPortManager.OpenSerialPort(port);
            stopReceiving = false;
            pktsAccepted = pktsMismatched = pktsReceived = 0;
            serialport.DataReceived += serial_DataReceived;
        }
        public void Close()
        {
            if (serialport == null)
                throw new ApplicationException("No port is open.");
            serialport.DataReceived -= serial_DataReceived;
            stopReceiving = true;
            Robocup.Utilities.SerialPortManager.CloseSerialPort(serialport);
            serialport = null;
        }

        void serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] data = new byte[PAYLOAD_SIZE + HEADER_LEN];
            byte[] payload = new byte[PAYLOAD_SIZE];

            try
            {
                while (serialport.BytesToRead >= HEADER_LEN + PAYLOAD_SIZE + FOOTER_LEN)
                {
                    string s = serialport.ReadTo("\\H"); // TODO: shouldn't use this                    
                    serialport.Read(data, 0, PAYLOAD_SIZE + HEADER_LEN);

                    Console.Write(pktsReceived + ": ");
                    for (int i = 0; i < data.Length; i++)
                    {
                        Console.Write(data[i] + " ");
                        if (i > HEADER_LEN - 1)
                            payload[i - HEADER_LEN] = data[i];
                    }
                    Console.WriteLine();
                    pktsReceived++;

                    // verify chksum
                    if (data[0] != Checksum.Compute(payload))
                    {
                        pktsMismatched++;
                        Console.WriteLine("Checksum mismatch. Stats: acc " + pktsAccepted +
                            " / mism " + pktsMismatched + " / rcv " + pktsReceived);
                        return;
                    }

                    // Parse messages
                    List<SerialInputMessage> rtn = new List<SerialInputMessage>();
                    for (int i = 0; i < NUM_SUBPKTS; i++)
                        rtn.Add(new SerialInputMessage((char)data[1], (char)data[2], payload, i * SerialInputMessage.SUBPKT_SIZE));

                    pktsAccepted++;
                    // And call appropriate handler
                    if (ValueReceived != null)
                        ValueReceived(rtn.ToArray());
                }
            }
            catch (IOException except)
            {
                // This is harmless, it happens when the listing thread is terminated 
                Console.WriteLine(except.Message + "\r\n" + except.StackTrace);
                return;
            }
            catch (InvalidOperationException except)
            {
                // This is harmless, it happens when the listing thread is terminated 
                Console.WriteLine(except.Message + "\r\n" + except.StackTrace);
                return;
            }
            catch (NullReferenceException except)
            {
                // The stopping functionality is not clean, (i.e. there's no synching), so this might
                // get called when stopReceiving is already true and serialport is null. In other cases,
                // something else is messed up, so rethrow.
                if (!stopReceiving)
                    throw except;
            }
        }
    }
}