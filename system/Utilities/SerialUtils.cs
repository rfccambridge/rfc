using System;
using System.Collections.Generic;
using System.Text;

using System.IO.Ports;

namespace Robocup.Utilities
{
    static public class SerialPortManager
    {
        static private Dictionary<string, SerialPort> ports = new Dictionary<string, SerialPort>();
        static public SerialPort GetSerialPort(string port)
        {
            if (ports.ContainsKey(port))
                return ports[port];
            SerialPort rtn = new SerialPort(port);
            //rtn.Encoding = NullEncoding.Encoding;
            ports.Add(port, rtn);
            return rtn;
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
