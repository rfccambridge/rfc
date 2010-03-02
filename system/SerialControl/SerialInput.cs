using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.IO.Ports;

namespace Robocup.SerialControl
{
    class SerialInput
    {
        static public SerialInput CreateSerialInput(string port)
        {
            SerialInput rtn = new SerialInput(port);
            if (!rtn.serialport.IsOpen)
                return null;
            return rtn;
        }

        SerialPort serialport;
        private SerialInput(string port)
        {
            //serialport = new SerialPort(port);
            serialport = Robocup.Utilities.SerialPortManager.GetSerialPort(port);
            serialport.DataReceived += serial_DataReceived;
            if (!serialport.IsOpen)
                serialport.Open();
        }
        public void Close()
        {
            serialport.Close();
            GC.SuppressFinalize(this);
        }
        ~SerialInput()
        {
            this.Close();
        }

        public class SerialInputMessage
        {
            private Int16 encoder;
            public Int16 Encoder
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

            private int dutyHigh;
            public int DutyHigh
            {
                get { return dutyHigh; }
                set { dutyHigh = value; }
            }

            private int dutyLow;
            public int DutyLow
            {
                get { return dutyLow; }
                set { dutyLow = value; }
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
                return "encoder\tdutyHigh\tdutyLow\tcommand";
            }
            public override string ToString()
            {
                return encoder + "\t" + dutyHigh + "\t" + dutyLow + "\t" + wheelcommand ;
            }
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

            while (serialport.BytesToRead > 50)
            {
                string s = serialport.ReadTo("\\H");
                byte[] data = new byte[3+group_size*numgroups];
                serialport.Read(data, 0, 3+group_size*numgroups);
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
                    rtn[i].Encoder = (Int16)(((Int16)(data[3 + i * group_size]) << 8) + //Hi
                                        (Int16)(data[4 + i * group_size])); //Lo
                                        //- (1 << 15)); //Off-center
                    rtn[i].DutyHigh = (sbyte)data[5 + i * group_size];
                    rtn[i].DutyLow = (sbyte)data[6 + i * group_size];
                    rtn[i].WheelCommand = (sbyte)data[7 + i * group_size];        
                }
                if(ValueReceived != null)
                    ValueReceived(rtn);
            }
        }

        /*public delegate void ValueReceivedDel(double t, int val);
        public event ValueReceivedDel ValueReceived;*/
        public event Action<SerialInputMessage[]> ValueReceived;
    }
}