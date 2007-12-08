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
            ports.Add(port, rtn);
            return rtn;
        }
    }
}
