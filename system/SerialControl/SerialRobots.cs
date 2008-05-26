using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using Robocup.Core;
using Robocup.Utilities;

namespace Robotics.Commander
{
    public class SerialRobots : IRobots
    {
        const byte wheel = (byte)'w';
        const byte PID = (byte)'f';
        const byte reset = (byte)'r';
        string[] headsigns = { "\\H0v", "\\H1v", "\\H2v", "\\H3v", "\\H4v", "\\H5v", "\\H6v" };

        string endsign = "\\E";
        string br = "9600";
        string pr = "None";
        string db = "8";
        string sb = "One";

        SerialPort comport;
        //static SerialPort comport = new SerialPort();

        /*#region handy functions
        private byte[] ByteStringToByteArray(string s)
        {
            byte[] buffer = new byte[s.Length];
            for (int i = 0; i < s.Length; i++)
                buffer[i] = (byte)Convert.ToByte(s.Substring(i, 1), 2);
            return buffer;
        }

        private byte[] checksum(byte[] a)
        {
            int i, j;
            j = 0;
            for (i = 16; i < (a.Length - 16); i++)
                j = j + a[i];
            byte[] t = ByteStringToByteArray(Convert.ToString(j % 128, 2).PadLeft(8, '0'));
            byte[] t1 = new byte[a.Length + 8];
            for (i = 0; i < (a.Length - 16); i++)
                t1[i] = a[i];
            for (i = 0; i < 8; i++)
                t1[a.Length - 16 + i] = t[i];
            for (i = 0; i < 15; i++)
                t1[t1.Length - 15 + i] = a[a.Length - 15 + i];
            return t1;
        }
        #endregion*/

        private const string fname = "..\\..\\resources\\scaling.txt";

        public SerialRobots(string port)
        {
            comport = Robocup.Utilities.SerialPortManager.GetSerialPort(port);

            comport.BaudRate = int.Parse(br);
            comport.DataBits = int.Parse(db);
            comport.StopBits = (StopBits)Enum.Parse(typeof(StopBits), sb);
            comport.Parity = (Parity)Enum.Parse(typeof(Parity), pr);
            comport.WriteTimeout = SerialPort.InfiniteTimeout;
            comport.ReadTimeout = SerialPort.InfiniteTimeout;

            loadMotorScale(fname);
        }
        public SerialRobots()
            : this(Constants.get<string>("ports", "SerialPort"))
        {
        }

        public void Open()
        {
            if (!comport.IsOpen)
                comport.Open();
        }

        public void Close()
        {
            comport.Close();
            GC.SuppressFinalize(this);
        }
        ~SerialRobots()
        {
            this.Close();
        }

        private int maxAcceleration = Constants.get<int>("control", "MAX_ACCELERATION");
        /// <summary>
        /// This is the maximum change, per second, that can be made to any of the wheel speeds.
        /// (In the same units that commands are given in, ie max 127)
        /// </summary>
        public int MaxAcceleration
        {
            get { return maxAcceleration; }
            set { maxAcceleration = value; }
        }
        private int maxStep = Constants.get<int>("control", "MAX_STEP");
        /// <summary>
        /// The maximum that the velocity can be changed with one command.
        /// </summary>
        public int MaxVelocityStep
        {
            get { return maxStep; }
            set { maxStep = value; }
        }

        public void ReloadConstants()
        {
            maxAcceleration = Constants.get<int>("control", "MAX_ACCELERATION");
            maxStep = Constants.get<int>("control", "MAX_STEP");
        }


        /*private double MinTimeBetweenCommands
        {
            get { return 1 / MaxAcceleration; }
        }*/


        Dictionary<int, WheelSpeeds> lastSpeeds = new Dictionary<int, WheelSpeeds>();
        Dictionary<int, double> lastTime = new Dictionary<int, double>();

        private int ChangeUpTo(int start, int end, int maxChange)
        {
            if (end > start)
                return start + Math.Min(maxChange, end - start);
            else
                return start - Math.Min(maxChange, start - end);
        }
        //private int last_lf = 0, last_rf = 0, last_lb = 0, last_rb = 0;
        //private Robocup.Utilities.HighResTimer timer = new HighResTimer();
        private void setAllMotor(int id, int source, int lf, int rf, int lb, int rb, int duration)
        {
            if (id >= headsigns.Length || id < 0)
            {
                Console.WriteLine("ids not in headsigns");
                return; //don't throw exception
            }

            //Here we have to convert from our convention (positive values->robot forward)
            //to the EE convention (positive values->clockwise)
            rf *= -1;
            rb *= -1;

            // board bugs out if we send an unescaped slash
            if (lb == '\\')
                lb++;
            if (lf == '\\')
                lf++;
            if (rf == '\\')
                rf++;
            if (rb == '\\')
                rb++;

            Console.WriteLine("setting speeds to: " + new WheelSpeeds(lf,rf,lb,rb).ToString());

            //robots expect wheel powers in this order:
            //rf lf lb rb
            byte[] msg = new byte[]{(byte)'\\',(byte)'H', (byte) ('0'+id),
                (byte)'w',wheel,(byte)rf,(byte)lf, (byte)lb, (byte)rb,(byte)'\\',(byte)'E'};

            comport.Write(msg, 0, msg.Length);
        }

        private List<int> canKick = new List<int>();
        private List<int> charging = new List<int>();

        private Dictionary<int, double> lastCharge = new Dictionary<int, double>();
        private Dictionary<int, System.Threading.Timer> timers = new Dictionary<int, System.Threading.Timer>();

        internal void setCharge(int robotID)
        {
            lock (canKick)
            {
                if (charging.Contains(robotID))
                    return;

                //make sure that we never kick after sending a setCharge command
                //but before sending the stopcharge command
                canKick.Remove(robotID);
                charging.Add(robotID);
                lastCharge[robotID] = HighResTimer.SecondsSinceStart();
                System.Threading.Timer t = new System.Threading.Timer(delegate(object o)
                {
                    this.setStopCharge(robotID);
                    timers.Remove(robotID);
                }, null, 10000, System.Threading.Timeout.Infinite);
                timers[robotID] = t;
            }
            string smsg = headsigns[robotID] + "E" + endsign;
            //Console.WriteLine("charge:" + smsg);
            comport.Write(smsg);
            Console.WriteLine("robot " + robotID + " is now charging");
        }

        internal void setStopCharge(int robotID)
        {
            lock (canKick)
            {
                if (!charging.Contains(robotID))
                    return;

                try
                {
                    timers[robotID].Dispose();
                    timers.Remove(robotID);
                }
                catch (Exception) { }
                charging.Remove(robotID);
                canKick.Add(robotID);
            }
            string smsg = headsigns[robotID] + "D" + endsign;
            //Console.WriteLine("stopcharge:" + smsg);
            comport.Write(smsg);
            Console.WriteLine("robot " + robotID + " has stopped charging");
        }

        public void SetPIDConstants(int robotID, byte P, byte I, byte D)
        {
            byte[] msg = new byte[]{(byte)'\\',(byte)'H', (byte) ('0'+robotID),
                (byte)'w',PID,P,I,D,(byte)'\\',(byte)'E'};
            comport.Write(msg, 0, msg.Length);
        }

        public void setKick(int robotID)
        {
            lock (canKick)
            {
                if (charging.Contains(robotID) && (HighResTimer.SecondsSinceStart() - lastCharge[robotID]) > 3)
                {
                    setStopCharge(robotID);
                    System.Threading.Thread.Sleep(35);
                    lastCharge[robotID] = HighResTimer.SecondsSinceStart();
                }
                if (!canKick.Contains(robotID))
                    return;
                canKick.Remove(robotID);
                Console.WriteLine("robot " + robotID + " is kicking!");
                string smsg = headsigns[robotID] + "K" + endsign;
                //Console.WriteLine("kick:" + smsg);
                comport.Write(smsg);

                //uncomment this to have the robot start charging after kicking
                System.Threading.Thread.Sleep(100);
                this.setCharge(robotID);
            }
        }

        public void startDribbler(int target)
        {
            string smsg = headsigns[target] + "d1" + endsign;
            comport.Write(smsg);
        }

        public void stopDribbler(int target)
        {
            string smsg = headsigns[target] + "d0" + endsign;
            comport.Write(smsg);
        }

        public void resetBoards(int target)
        {
            comport.Write(headsigns[target] + "rr" + endsign);
        }

        /// <summary>
        /// tries to completely disable a robot
        /// </summary>
        public void stopAll(int target)
        {
            //stop charging:
            Console.WriteLine("stopping charging");
            string smsg = headsigns[target] + "DD" + endsign;
            comport.Write(smsg);
            System.Threading.Thread.Sleep(50);
            Console.WriteLine("discharging the capacitors (aka kicking)");
            //stop the dribbler:
            stopDribbler(target);
            System.Threading.Thread.Sleep(50);
            Console.WriteLine("stopping the motors");
            setMotorSpeeds(target, new WheelSpeeds(0, 0, 0, 0));
            System.Threading.Thread.Sleep(50);
            Console.WriteLine("stopping the dribbler");
            //kick to discharge
            string smsg2 = headsigns[target] + "KK" + endsign;
            comport.Write(smsg2);
        }

        /// <summary>
        /// Sends an arbitrary command to the port -- for debugging purposes only
        /// </summary>
        public void sendCommand(string command)
        {
            comport.Write(command);
        }

        #region IRobots Members

        public void setMotorSpeeds(int id, WheelSpeeds wheelSpeeds)
        {
            int lf=wheelSpeeds.lf, rf=wheelSpeeds.rf, lb=wheelSpeeds.lb, rb=wheelSpeeds.rb;



            double time = HighResTimer.SecondsSinceStart();
            if (!lastSpeeds.ContainsKey(id))
            {
                lastSpeeds[id] = new WheelSpeeds();
                lastTime[id] = 0;
            }
            if (lf != 0 || rf != 0 || lb != 0 || rb != 0)
            {
                double dt = time - lastTime[id];
                int maxstep = Math.Min(MaxVelocityStep, (int)(dt * maxAcceleration + .5));
                if (maxstep < 1)
                {
                    Console.WriteLine("maxstep too small: " + maxstep);
                    return;
                }
                WheelSpeeds last = lastSpeeds[id];
                lf = ChangeUpTo(last.lf, lf, maxstep);
                rf = ChangeUpTo(last.rf, rf, maxstep);
                lb = ChangeUpTo(last.lb, lb, maxstep);
                rb = ChangeUpTo(last.rb, rb, maxstep);
            }
            lastSpeeds[id] = new WheelSpeeds(lf, rf, lb, rb);
            lastTime[id] = time;

            /////
            //process motor scalings
            int maxspeed = 127;
            if (wheelSpeeds.lf > 0)
                lf = (int)Math.Min(lf * forwardpower[id].lf, maxspeed);
            else
                lf = (int)Math.Max(lf * backwardspower[id].lf, -maxspeed);

            if (wheelSpeeds.rf > 0)
                rf = (int)Math.Min(rf * forwardpower[id].rf, maxspeed);
            else
                rf = (int)Math.Max(rf * backwardspower[id].rf, -maxspeed);

            if (wheelSpeeds.lb > 0)
                lb = (int)Math.Min(lb * forwardpower[id].lb, maxspeed);
            else
                lb = (int)Math.Max(lb * backwardspower[id].lb, -maxspeed);

            if (wheelSpeeds.rb > 0)
                rb = (int)Math.Min(rb * forwardpower[id].rb, maxspeed);
            else
                rb = (int)Math.Max(rb * backwardspower[id].rb, -maxspeed);


            //if (frontLeft * frontLeft + frontRight * frontRight + backLeft * backLeft + backRight * backRight > 10)
            setAllMotor(id, 0, lf, rf, lb, rb, 1000);
            /*else
                setAllMotor(robotID, 0, 0, 0, 0, 0, 65535);*/
        }

        public void kick(int robotID)
        {
            setKick(robotID);
        }

        #endregion


        //TODO: this is not the right place to scale powers
        Dictionary<int, WheelsInfo<float>> forwardpower = new Dictionary<int, WheelsInfo<float>>();
        Dictionary<int, WheelsInfo<float>> backwardspower = new Dictionary<int, WheelsInfo<float>>();

        public void loadMotorScale()
        {
            loadMotorScale(fname);
        }
        public void loadMotorScale(string filename)
        {
            System.IO.StreamReader reader = new System.IO.StreamReader(filename);
            string s = "";
            while (s != "*****")
            {
                s = reader.ReadLine();
            }
            string wholething = reader.ReadToEnd();
            string[] values = wholething.Split(new char[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            int count = 0;
            while (count < values.Length)
            {
                int id = int.Parse(values[count++]);
                {
                    float lf = float.Parse(values[count++]);
                    float rf = float.Parse(values[count++]);
                    float lb = float.Parse(values[count++]);
                    float rb = float.Parse(values[count++]);
                    backwardspower[id] = new WheelsInfo<float>(lf, rf, lb, rb);
                }
                {
                    float lf = float.Parse(values[count++]);
                    float rf = float.Parse(values[count++]);
                    float lb = float.Parse(values[count++]);
                    float rb = float.Parse(values[count++]);
                    forwardpower[id] = new WheelsInfo<float>(lf, rf, lb, rb);
                }
            }
            reader.Close();
        }
    }
}
