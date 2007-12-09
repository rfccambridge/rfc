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
        string wheel = "w" ;
        string [] headsigns = { "\\Hvv", "\\H2v", "\\H3v", "\\H4v", "\\H5v", "\\H6v", "\\H7v" };

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
        //private const string fname = "C:\\Debug\\scaling.txt";

        public SerialRobots(string port)
        {
            comport = Robocup.Utilities.SerialPortManager.GetSerialPort(port);

            comport.Encoding = NullEncoding.Encoding;
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

        private void setAllMotor(int target, int source, int lf, int rf, int lb, int rb, int duration)
        {
            if (target >= headsigns.Length || target < 0)
                return; //don't throw exception

            //Console.WriteLine(target + ": lf rf lb rb: " + lf + " " + rf + " " + lb + " " + rb);

            int dir = 0;
            //Here we have to convert from our convention (positive values->robot forward)
            //to the EE convention (positive values->counterclockwise)
            if (lb < 0) dir += 1;
            if (lf < 0) dir += 2;
            if (rf > 0) dir += 4;
            if (rb > 0) dir += 8;

            #region convert input to byte string
            string sdir, slf, srf, slb, srb;

            char cdir, clf, crf, clb, crb;
            cdir = (char)dir;
            clf = (char)Math.Abs(lf);
            crf = (char)Math.Abs(rf);
            clb = (char)Math.Abs(lb % 256);
            crb = (char)Math.Abs(rb);
            sdir = Convert.ToString(cdir);
            slf = Convert.ToString(clf);
            srf = Convert.ToString(crf);
            slb = Convert.ToString(clb);
            srb = Convert.ToString(crb);


            string smsg;
            //robots expect wheel powers in this order:
            //lb lf rf rb
            //smsg = headsigns[target] + wheel + slb + slf + srf + srb + sdir + endsign;

            byte[] msg = new byte[]{(byte)'\\',(byte)'H', (byte) ('0'+target),
                (byte)'w',(byte)'w',(byte)Math.Abs(lb/256),
                (byte)Math.Abs(lb%256),(byte)'\\',(byte)'E'};

            /*smsg = "\\H" + target + "ww" + Convert.ToString((char)Math.Abs(lb / 256)) + slb + "\\E";

            Console.WriteLine((int)Convert.ToString((char)Math.Abs(lb / 256))[0] + "\t" + (int)slb[0]);*/
            comport.Write(msg, 0, msg.Length);

            //Console.WriteLine(smsg);
            #endregion

            //comport.Write(smsg);

        }

        private List<int> canKick = new List<int>();
        private List<int> charging = new List<int>();

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
            }
            string smsg = headsigns[robotID] + "EE" + endsign;
            //Console.WriteLine("charge:" + smsg);
            comport.Write(smsg);
            System.Threading.Timer t = new System.Threading.Timer(delegate(object o)
            {
                this.setStopCharge(robotID);
            }, null, 10000, System.Threading.Timeout.Infinite);
            Console.WriteLine("robot " + robotID + " is now charging");
        }

        internal void setStopCharge(int robotID)
        {
            lock (canKick)
            {
                if (!charging.Contains(robotID))
                    return;

                charging.Remove(robotID);
                canKick.Add(robotID);
            }
            string smsg = headsigns[robotID] + "DD" + endsign;
            //Console.WriteLine("stopcharge:" + smsg);
            comport.Write(smsg);
            Console.WriteLine("robot " + robotID + " has stopped charging");
        }

        public void setKick(int robotID)
        {
            //uncomment this to have the robot start charging after kicking
            /*System.Threading.Timer t = new System.Threading.Timer(delegate(object o)
            {
                this.setCharge(robotID);
            }, null, 1000, System.Threading.Timeout.Infinite);*/
            lock (canKick)
            {
                if (!canKick.Contains(robotID))
                    return;

                canKick.Remove(robotID);
            }
            Console.WriteLine("robot " + robotID + " is kicking!");
            string smsg = headsigns[robotID] + "KK" + endsign;
            //Console.WriteLine("kick:" + smsg);
            comport.Write(smsg);
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

        public void setMotorSpeeds(int robotID, WheelSpeeds wheelSpeeds)
        {
            int frontLeft, frontRight, backLeft, backRight;

            /////
            //process motor scalings
            int maxspeed = 25500000;
            if (wheelSpeeds.lf > 0)
                frontLeft = (int)Math.Min(wheelSpeeds.lf * forwardpower[robotID].lf, maxspeed);
            else
                frontLeft = (int)Math.Max(wheelSpeeds.lf * backwardspower[robotID].lf, -maxspeed);

            if (wheelSpeeds.rf > 0)
                frontRight = (int)Math.Min(wheelSpeeds.rf * forwardpower[robotID].rf, maxspeed);
            else
                frontRight = (int)Math.Max(wheelSpeeds.rf * backwardspower[robotID].rf, -maxspeed);

            if (wheelSpeeds.lb > 0)
                backLeft = (int)Math.Min(wheelSpeeds.lb * forwardpower[robotID].lb, maxspeed);
            else
                backLeft = (int)Math.Max(wheelSpeeds.lb * backwardspower[robotID].lb, -maxspeed);

            if (wheelSpeeds.rb > 0)
                backRight = (int)Math.Min(wheelSpeeds.rb * forwardpower[robotID].rb, maxspeed);
            else
                backRight = (int)Math.Max(wheelSpeeds.rb * backwardspower[robotID].rb, -maxspeed);


            if (frontLeft * frontLeft + frontRight * frontRight + backLeft * backLeft + backRight * backRight > 10)
                setAllMotor(robotID, 0, frontLeft, frontRight, backLeft, backRight, 1000);
            else
                setAllMotor(robotID, 0, 0, 0, 0, 0, 65535);
        }

        public void kick(int robotID)
        {
            throw new Exception("The method or operation is not implemented.");
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
