using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using Robocup.Infrastructure;

namespace Robotics.Commander
{
    public class SerialRobots : IRobots
    {
        string wheel = "w";
        string[] headsigns = { "\\Hvv", "\\H2v", "\\H3v", "\\H4v", "\\H5v", "\\H6v", "\\H7v" };

        string endsign = "\\E";
        string pn = "COM4";
        string br = "9600";
        string pr = "None";
        string db = "8";
        string sb = "One";

        #region construction
        static SerialPort comport = new SerialPort();
        #endregion

        #region handy functions
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
        #endregion

        private const string fname = "C:\\Microsoft Robotics Studio (1.0)\\simulator\\MasterCommander\\scaling.txt";
        //private const string fname = "C:\\Debug\\scaling.txt";

        public SerialRobots()
        {
            comport.BaudRate = int.Parse(br);
            comport.DataBits = int.Parse(db);
            comport.StopBits = (StopBits)Enum.Parse(typeof(StopBits), sb);
            comport.Parity = (Parity)Enum.Parse(typeof(Parity), pr);
            comport.PortName = pn;
            comport.WriteTimeout = SerialPort.InfiniteTimeout;
            comport.ReadTimeout = SerialPort.InfiniteTimeout;

            loadMotorScale(fname);
        }

        public void Open()
        {
            comport.Open();
        }

        public void Close()
        {
            comport.Close();
        }

        const int WHEELCMDLEN = 7;
        const int WHEELCMD = 1;

        private void setAllMotor(int target, int source, int lf, int rf, int lb, int rb, int duration)
        {
            if (target >= headsigns.Length || target < 0)
                return; //don't throw exception

            Console.WriteLine(target + ": lf rf lb rb: " + lf + " " + rf + " " + lb + " " + rb);

            int dir = 0;
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
            clb = (char)Math.Abs(lb);
            crb = (char)Math.Abs(rb);
            sdir = Convert.ToString(cdir);
            slf = Convert.ToString(clf);
            srf = Convert.ToString(crf);
            slb = Convert.ToString(clb);
            srb = Convert.ToString(crb);


            string smsg;
            //smsg = headsign + star + ssou + allmotorsign + sdir + ssp1 + ssp2 + ssp3 + ssp4 + sdur + endsign;
            //robots expect wheel powers in this order:
            //lb lf rf rb
            smsg = headsigns[target] + wheel + slb + slf + srf + srb + sdir + endsign;
            //Console.WriteLine(smsg);
            #endregion

            comport.Write(smsg);

        }

        private List<int> canKick = new List<int>();

        internal void setCharge(int target)
        {
            /*string smsg = headsigns[target] + "EE" + endsign;
            //Console.WriteLine("charge:" + smsg);
            comport.Write(smsg);
            System.Threading.Timer t = new System.Threading.Timer(delegate(object o)
            {
                this.setStopCharge(target);
            }, null, 10000, System.Threading.Timeout.Infinite);
            lock (canKick)
            {
                //make sure that we never kick after sending a setCharge command
                //but before sending the stopcharge command
                canKick.Remove(target);
            }
            Console.WriteLine("robot " + target + " is now charging");*/
        }

        internal void setStopCharge(int target)
        {
            /*string smsg = headsigns[target] + "DD" + endsign;
            //Console.WriteLine("stopcharge:" + smsg);
            comport.Write(smsg);
            lock (canKick)
            {
                canKick.Add(target);
            }
            Console.WriteLine("robot " + target + " is stopped charging");*/
        }

        public void setKick(int target)
        {
            /*System.Threading.Timer t = new System.Threading.Timer(delegate(object o)
            {
                this.setCharge(target);
            }, null, 1000, System.Threading.Timeout.Infinite);
            lock (canKick)
            {
                if (!canKick.Contains(target))
                    return;

                canKick.Remove(target);
            }
            Console.WriteLine("robot " + target+" is kicking!");
            string smsg = headsigns[target] + "KK" + endsign;
            //Console.WriteLine("kick:" + smsg);
            comport.Write(smsg);*/
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


        #region IRobots Members

        public void setMotorSpeeds(int robotID, WheelSpeeds wheelSpeeds)
        {
            int frontLeft, frontRight, backLeft, backRight;

            /////
            //process motor scalings
            int maxspeed = 127;
            if (wheelSpeeds.lf > 0)
                frontLeft = (int)Math.Min(wheelSpeeds.lf * scaling[robotID, 0, 1], maxspeed);
            else
                frontLeft = (int)Math.Max(wheelSpeeds.lf * scaling[robotID, 0, 0], -maxspeed);

            if (wheelSpeeds.rf > 0)
                frontRight = (int)Math.Min(wheelSpeeds.rf * scaling[robotID, 1, 1], maxspeed);
            else
                frontRight = (int)Math.Max(wheelSpeeds.rf * scaling[robotID, 1, 0], -maxspeed);

            if (wheelSpeeds.lb > 0)
                backLeft = (int)Math.Min(wheelSpeeds.lb * scaling[robotID, 2, 1], maxspeed);
            else
                backLeft = (int)Math.Max(wheelSpeeds.lb * scaling[robotID, 2, 0], -maxspeed);

            if (wheelSpeeds.rb > 0)
                backRight = (int)Math.Min(wheelSpeeds.rb * scaling[robotID, 3, 1], maxspeed);
            else
                backRight = (int)Math.Max(wheelSpeeds.rb * scaling[robotID, 3, 0], -maxspeed);


            if (frontLeft * frontLeft + frontRight * frontRight + backLeft * backLeft + backRight * backRight > 50)
                setAllMotor(robotID, 0, frontLeft, frontRight, backLeft, backRight, 1000);
            else
                setAllMotor(robotID, 0, 0, 0, 0, 0, 65535);
        }

        public void kick(int robotID)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        private const int NUMROBOTS = 10;
        /// <summary>
        /// robot, wheel, direction
        /// wheels go lf, rf, lb, rb
        /// </summary>
        float[, ,] scaling = new float[NUMROBOTS, 4, 2];

        public void loadMotorScale()
        {
            loadMotorScale(fname);
        }
        public void loadMotorScale(string filename)
        {
            //System.IO.FileStream stream = new System.IO.FileStream(filename, System.IO.FileMode.Open);
            System.IO.StreamReader reader = new System.IO.StreamReader(filename);
            string s = "";
            while (s != "*****")
            {
                s = reader.ReadLine();
            }
            string wholething = reader.ReadToEnd();
            string[] values = wholething.Split(new char[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            int count = 0;
            for (int id = 0; id < NUMROBOTS; id++)
            {
                for (int direction = 0; direction < 2; direction++)
                {
                    for (int wheel = 0; wheel < 4; wheel++)
                    {
                        scaling[id, wheel, direction] = float.Parse(values[count]);
                        count++;
                    }
                }
            }
            reader.Close();
        }
    }
}
