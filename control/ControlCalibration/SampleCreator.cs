using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using Robocup.Utilities;
using Robocup.MotionControl;
using Robocup.CoreRobotics;
using System.IO;

namespace Robocup.MotionControl
{
    class SampleCreator
    {
        static void WriteVisionData(RobotInfo info, LogWriter<VisionOrCommand> writer, double t)
        {
            VisionOrCommand message = new VisionOrCommand(
                new VisionMessage.RobotData(info.ID, true, info.Position, info.Orientation));
            writer.SimulateTimedLog(message, t);
        }
        static void WriteCommand(WheelSpeeds command, LogWriter<VisionOrCommand> writer, double t)
        {
            VisionOrCommand message = new VisionOrCommand(command);
            writer.SimulateTimedLog(message, t);
        }
        /// <summary>
        /// Writes a "nice" calibration log: exactly determined from a model, and no odd timing effects.
        /// </summary>
        void WriteNiceLog()
        {
            MovementModeler modeler = new MovementModeler();

            //open the stream and the log writer
            Stream s = File.Open("../../resources/Control calibration data/nice.log.zip", FileMode.Create);
            Stream gz = new System.IO.Compression.GZipStream(s, System.IO.Compression.CompressionMode.Compress);
            LogWriter<VisionOrCommand> writer = new LogWriter<VisionOrCommand>(gz);

            //initial conditions
            WheelSpeeds command = new WheelSpeeds(80, 90, 100, 70);
            RobotInfo curinfo = new RobotInfo(Vector2.ZERO, 0, 0);
            //send initial conditions
            WriteCommand(command, writer, 0);
            WriteVisionData(curinfo, writer, 0);

            double lastsimtime = 0;

            for (double t = 0; t <= 5; t += .05)
            {
                //model until the command is given
                curinfo = modeler.ModelWheelSpeeds(curinfo, command, t - lastsimtime);
                lastsimtime = t;
                //say that we sent the command
                WriteCommand(command, writer, t);
                //model until the vision is sent
                curinfo = modeler.ModelWheelSpeeds(curinfo, command, t + .001 - lastsimtime);
                lastsimtime = t + .001;
                //say that we received vision
                WriteVisionData(curinfo, writer, t + .001);
            }
            gz.Close();
            s.Close();
        }
        /// <summary>
        /// Gets the command to be sent out at time t under the "BasicLog"
        /// </summary>
        WheelSpeeds BasicCommands(double t)
        {
            WheelSpeeds command = new WheelSpeeds();
            if (t < 2)
            {
                command.lb = 100;
                command.lf = 80;
                command.rb = 70;
                command.rf = 90;
            }
            else
            {
                command.rb = (int)(100 * Math.Cos(t - 2));
                command.rf = (int)(80 + 20 * Math.Sin(1.5 * t - 2));
                command.lb = (int)(80 + 20 * Math.Sin(1.9 * t - 2));
                command.lf = (int)(100 * Math.Cos(2.3 * t - 2));
            }
            return command;
        }
        void WriteBasicLog()
        {
            //set up the modeler
            MovementModeler modeler = new MovementModeler();
            modeler.changeConstlf = 5;
            modeler.changeConstlb = 5;
            modeler.changeConstrf = 7;
            modeler.changeConstrb = 7;

            //open the stream and the writer
            Stream s = File.Open("../../resources/Control calibration data/basic.log.zip", FileMode.Create);
            Stream gz = new System.IO.Compression.GZipStream(s, System.IO.Compression.CompressionMode.Compress);
            LogWriter<VisionOrCommand> writer = new LogWriter<VisionOrCommand>(gz);

            {
                WheelSpeeds command = BasicCommands(0);
                RobotInfo curinfo = new RobotInfo(Vector2.ZERO, 0, 0);
                WriteCommand(command, writer, 0);
                WriteVisionData(curinfo, writer, 0);
                double lastsimtime = 0;
                for (double t = 0; t <= 5; t += .05)
                {
                    command = BasicCommands(t);
                    curinfo = modeler.ModelWheelSpeeds(curinfo, command, t - lastsimtime);
                    lastsimtime = t;
                    WriteCommand(command, writer, t);
                    curinfo = modeler.ModelWheelSpeeds(curinfo, command, t + .001 - lastsimtime);
                    lastsimtime = t + .001;
                    WriteVisionData(curinfo, writer, t + .001);
                }
            }
            writer.InsertBreak();
            {
                WheelSpeeds command = BasicCommands(10);
                RobotInfo curinfo = new RobotInfo(Vector2.ZERO, 0, 0);
                WriteCommand(command, writer, 10);
                WriteVisionData(curinfo, writer, 10);
                double lastsimtime = 10;
                for (double t = 10; t <= 25; t += .05)
                {
                    command = BasicCommands(t);
                    curinfo = modeler.ModelWheelSpeeds(curinfo, command, t - lastsimtime);
                    lastsimtime = t;
                    WriteCommand(command, writer, t);
                    curinfo = modeler.ModelWheelSpeeds(curinfo, command, t + .001 - lastsimtime);
                    lastsimtime = t + .001;
                    WriteVisionData(curinfo, writer, t + .001);
                }
            }
            gz.Close();

            s.Close();
        }
        static void Main(string[] args)
        {
            SampleCreator creator = new SampleCreator();
            creator.WriteNiceLog();
            creator.WriteBasicLog();
        }
    }
}
