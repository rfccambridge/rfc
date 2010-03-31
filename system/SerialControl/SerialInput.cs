using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.IO.Ports;

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



            static public string ToStringHeader()
            {
                return "encoder\tduty\tcommand";
            }
            public override string ToString()
            {
                return encoder + "\t" + duty + "\t" + wheelcommand;
            }
        }

        public event Action<SerialInputMessage[]> ValueReceived;        
        SerialPort serialport = null;
        bool stopReceiving;

        public void Open(string port)
        {
            if (serialport != null)
                throw new ApplicationException("Already have a port open.");
            serialport = Robocup.Utilities.SerialPortManager.OpenSerialPort(port);
            stopReceiving = false;
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
            const int numgroups = 6;
            const int group_size = 5; //bytes
            if (stopReceiving)
                return;

            try
            {
                while (serialport.BytesToRead > 50)
                {
                    string s = serialport.ReadTo("\\H");
                    byte[] data = new byte[3 + group_size * numgroups];
                    serialport.Read(data, 0, 3 + group_size * numgroups);
                    foreach (byte b in data)
                    {
                        Console.Write(b + " ");
                    }
                    Console.WriteLine();
                    //foreach (byte b in data)
                    //{
                    //    Console.Write((char)b);
                    //}
                    //Console.WriteLine();
                    SerialInputMessage[] rtn = new SerialInputMessage[numgroups];
                    for (int i = 0; i < numgroups; i++)
                    {
                        rtn[i] = new SerialInputMessage();
                        rtn[i].Encoder = (Int16)(((UInt16)(data[3 + i * group_size]) << 8) + //Hi
                                            (UInt16)(data[4 + i * group_size])); //Lo
                                            //- (1 << 15)); //Off-center
                        rtn[i].Duty = (int)(((Int16)data[5 + i * group_size] << 8) +
                                       ((Int16)data[6 + i * group_size]));
                        rtn[i].WheelCommand = (sbyte)data[7 + i * group_size];
                    }
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
        }        
    }
}