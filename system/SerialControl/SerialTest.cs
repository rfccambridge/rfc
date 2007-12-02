using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.IO.Ports;
using System.Threading;

namespace serial_test
{
    class Program
    {
        static void Main(string[] args)
        {
            /*Thread t = new Thread(delegate(object o)
            {
                new Program("com20").Run();
            });
            t.Start();
            new Program("com21").Run();*/
            new Program().Run();
        }
        SerialPort serial;
        public Program(string port)
        {
            serial = new SerialPort(port);
        }
        public Program()
        {
        }
        private void Run()
        {
            if (serial == null)
            {
                Console.WriteLine("enter port name:");
                string s;
                s = Console.ReadLine();
                serial = new SerialPort(s);
            }
            serial.Open();
            serial.DataReceived += serial_DataReceived;
            byte[] buffer = new byte[] { (byte)'\\', (byte)'H', (byte)'1', (byte)'2', (byte)'f', (byte)'p', (byte)'i', (byte)'d', (byte)'\\', (byte)'E' };
            while (true)
            {
                Thread.Sleep(30);
                buffer[5]++;
                serial.Write(buffer, 0, buffer.Length);
            }
        }

        void serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Console.WriteLine("received data:\t" + serial.ReadTo("\\E"));
        }
    }
}
