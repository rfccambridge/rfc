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
            if (! rtn.serialport.IsOpen)
                return null;
            return rtn;
        }

        SerialPort serialport;
        private SerialInput(string port)
        {
            serialport = new SerialPort(port);
            serialport.DataReceived += serial_DataReceived;
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
            string s = serialport.ReadTo("\\E");
            int val = (int)(s[s.Length - 1]);
            ValueReceived(val);
        }

        public event Action<int> ValueReceived;
    }
}
