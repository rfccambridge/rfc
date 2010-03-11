using System;
using System.Collections.Generic;
using System.Text;

using System.IO.Ports;

namespace Robocup.Utilities
{
    static public class SerialPortManager
    {
        static readonly string br = "9600";
        static readonly string pr = "None";
        static readonly string db = "8";
        static readonly string sb = "One";
        static private Dictionary<string, SerialPort> ports = new Dictionary<string, SerialPort>();
        static private Dictionary<SerialPort, int> refcounts = new Dictionary<SerialPort, int>();
        static public SerialPort OpenSerialPort(string port)
        {
            SerialPort serialPort;
            if (ports.ContainsKey(port))
            {
                serialPort = ports[port];
            }
            else
            {
                serialPort = new SerialPort(port);

                serialPort.BaudRate = int.Parse(br);
                serialPort.DataBits = int.Parse(db);
                serialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), sb);
                serialPort.Parity = (Parity)Enum.Parse(typeof(Parity), pr);
                serialPort.WriteTimeout = SerialPort.InfiniteTimeout;
                serialPort.ReadTimeout = SerialPort.InfiniteTimeout;
                //serialPort.Encoding = NullEncoding.Encoding;
                ports.Add(port, serialPort);
                refcounts.Add(serialPort, 0);
                serialPort.Open();
            }
            refcounts[serialPort]++;
            return serialPort;
        }
        static public void CloseSerialPort(SerialPort port)
        {
            if (refcounts[port] == 0)
                throw new ApplicationException("Somebody is double closing port!");
            refcounts[port]--;
            if (refcounts[port] == 0)
            {
                refcounts.Remove(port);
                ports.Remove(port.PortName);
                port.Close();                
            }
        }
    }
    public class NullEncoding : Encoding
    {
        static private Encoding  encoding = new NullEncoding();
        static public Encoding  Encoding
        {
            get { return encoding; }
        }

        private NullEncoding() { }

        public override int GetByteCount(char[] chars, int index, int count)
        {
            return chars.Length;
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            int n = Math.Min(charCount, Math.Min(chars.Length - charIndex, bytes.Length - byteIndex));
            for (int i = 0; i < n; i++)
            {
                bytes[byteIndex + i] = (byte)chars[charIndex + i];
            }
            return n;
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return bytes.Length;
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            int n = Math.Min(byteCount, Math.Min(chars.Length - charIndex, bytes.Length - byteIndex));
            for (int i = 0; i < n; i++)
            {
                chars[charIndex + i] = (char)bytes[byteIndex + i];
            }
            return n;
        }

        public override int GetMaxByteCount(int charCount)
        {
            return charCount;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            return byteCount;
        }
    }
}
