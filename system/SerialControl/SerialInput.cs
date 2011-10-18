using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.IO.Ports;
using Robocup.Utilities;

namespace Robocup.SerialControl
{
    class SerialInput
    {
        public class SerialInputMessage
        {
            private int encoder;
            public int Encoder
            {
                get { return encoder; }
                set { encoder = value; }
            }

            private int error;
            public int Error
            {
                get { return error; }
                set { error = value; }
            }

            private int wheelcommand;
            public int WheelCommand
            {
                get { return wheelcommand; }
                set { wheelcommand = value; }
            }

            private int duty;
            public int Duty
            {
                get { return duty; }
                set { duty = value; }
            }

            private int extra;
            public int Extra
            {
                get { return extra; }
                set { extra = value; }
            }

            private int extra2;
            public int Extra2
            {
                get { return extra2; }
                set { extra2 = value; }
            }

            private int pktsReceived;
            public int PktsReceived
            {
                get { return pktsReceived; }
                set { pktsReceived = value; }
            }

            private int pktsAccepted;
            public int PktsAccepted
            {
                get { return pktsAccepted; }
                set { pktsAccepted = value; }
            }

            private int pktsMismatched;
            public int PktsMismatched
            {
                get { return pktsMismatched; }
                set { pktsMismatched = value; }
            }


            static public string ToStringHeader()
            {
                return "enc\tduty\tcmd\trcv\tacc\tmism";
            }
            public override string ToString()
            {
                return encoder + "\t" + duty + "\t" + wheelcommand + "\t" + 
                    pktsReceived + "\t" + pktsAccepted + "\t" + pktsMismatched;
            }
        }

        public event Action<SerialInputMessage[]> ValueReceived;        
        SerialPort serialport = null;
        bool stopReceiving;
        uint pktsAccepted, pktsMismatched, pktsReceived;

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

        /// <summary>
        /// Protocol for spewed data:
        /// First three bytes are a header.
        /// (numgroups) groups follow, each of which is (group_size) bytes large
        /// In current implementation, group_size == 5
        /// bytes 1,2 - High and Low part of encoder packet (offset by 0x80 to fit it an unsigned int)
        /// byte 3 - Duty cycle high
        /// byte 4 - Duty cycle low
        /// byte 5 - Wheel command
        /// </summary>
        void serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            const int HEADER_LEN = 2; // chksum, address (\\H is not counted, it doesn't end up in data variable)
            const int FOOTER_LEN = 2; // '\\', 'E'
            const int NUM_SUBPKTS = 1;
            const int SUBPKT_SIZE = 8;
            const int PAYLOAD_SIZE = NUM_SUBPKTS * SUBPKT_SIZE;

            byte[] data = new byte[PAYLOAD_SIZE + HEADER_LEN];
            byte[] payload = new byte[PAYLOAD_SIZE];

            try
            {
                while (serialport.BytesToRead >= HEADER_LEN + PAYLOAD_SIZE + FOOTER_LEN)
                {
                    string s = serialport.ReadTo("\\H"); // TODO: shouldn't use this                    
                    serialport.Read(data, 0, PAYLOAD_SIZE+HEADER_LEN);                    

                    Console.Write(pktsReceived + ": ");
                    for (int i = 0; i < data.Length; i++)
                    {
                        Console.Write(data[i] + " ");
                        if (i > HEADER_LEN-1)
                            payload[i-HEADER_LEN] = data[i];
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

                    SerialInputMessage[] rtn = new SerialInputMessage[NUM_SUBPKTS];
                    for (int i = 0; i < NUM_SUBPKTS; i++)
                    {
                        rtn[i] = new SerialInputMessage();
                        rtn[i].Encoder = (Int16)(((UInt16)(payload[i * SUBPKT_SIZE]) << 8) + //Hi
                                            (UInt16)(payload[i * SUBPKT_SIZE + 1])); //Lo
                        //- (1 << 15)); //Off-center
                        rtn[i].Duty = (int)(((Int16)payload[i * SUBPKT_SIZE + 2] << 8) +
                                       ((Int16)payload[i * SUBPKT_SIZE + 3]));
                        rtn[i].WheelCommand = (sbyte)payload[i * SUBPKT_SIZE + 4];
                        rtn[i].PktsReceived = (byte)payload[i * SUBPKT_SIZE + 5];
                        rtn[i].PktsAccepted = (byte)payload[i * SUBPKT_SIZE + 6];
                        rtn[i].PktsMismatched = (byte)payload[i * SUBPKT_SIZE + 7];                        
                    }
                    pktsAccepted++;
                    if (ValueReceived != null)
                        ValueReceived(rtn);
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