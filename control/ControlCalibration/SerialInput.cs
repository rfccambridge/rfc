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

        void serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            while (serialport.BytesToRead > 50)
            {
                string s = serialport.ReadTo("\\H");
                byte[] data = new byte[35];
                serialport.Read(data, 0, 35);
                foreach (byte b in data)
                {
                    Console.Write(b + " ");
                }
                Console.WriteLine();
                int[] rtn = new int[16];
                for (int i = 0; i < 16; i++)
                {
                    int val = 256 * (int)(data[3 + i * 2]) + (int)(data[4 + i * 2]);
                    rtn[i] = val;
                }
                ValueReceived(rtn);
            }
        }

        /*public delegate void ValueReceivedDel(double t, int val);
        public event ValueReceivedDel ValueReceived;*/
        public event Action<int[]> ValueReceived;
    }
}