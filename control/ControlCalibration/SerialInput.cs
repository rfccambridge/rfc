using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.IO.Ports;

namespace Robocup.MotionControl
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
                return "encoder\terror\tduty\tcommand\tencoder";
            }
            public override string ToString()
            {
                return encoder + "\t" + error + "\t" + wheelcommand + "\t" + extra+"\t"+extra2;
            }
        }

        void serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            while (serialport.BytesToRead > 50)
            {
                string s = serialport.ReadTo("\\H");
                int numgroups = 4;
                byte[] data = new byte[3+8*numgroups];
                serialport.Read(data, 0, 3+8*numgroups);
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
                    rtn[i].Encoder = 256 * (int)(data[3 + i * 8]) + (int)(data[4 + i * 8]) - (1 << 15);
                    rtn[i].Error = (sbyte)data[5 + i * 8];
                    rtn[i].WheelCommand = (short)(256 * (int)(data[6 + i * 8]) + (int)(data[7 + i * 8]));
                    rtn[i].Extra = 256 * (int)(data[8 + i * 8]) + (int)(data[9 + i * 8]);
                    rtn[i].Extra2 = (sbyte)data[10 + i * 8];
                }
                ValueReceived(rtn);
            }
        }

        /*public delegate void ValueReceivedDel(double t, int val);
        public event ValueReceivedDel ValueReceived;*/
        public event Action<SerialInputMessage[]> ValueReceived;
    }
}