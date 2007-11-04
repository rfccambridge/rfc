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
            Stream s = File.Open("../../resources/Control calibration data/nice.log.zip", FileMode.Create);
            Stream gz = new System.IO.Compression.GZipStream(s, System.IO.Compression.CompressionMode.Compress);
            LogWriter<VisionOrCommand> writer = new LogWriter<VisionOrCommand>(gz);
            WheelSpeeds command = new WheelSpeeds(100, 80, 80, 100);
            RobotInfo curinfo = new RobotInfo(Vector2.ZERO, 0, 0);
            double lastsimtime = 0;
            for (double t = 0; t <= 10; t += .05)
            {
                curinfo = modeler.ModelWheelSpeeds(curinfo, command, t - lastsimtime);
                WriteCommand(command, writer, t);
                WriteVisionData(curinfo, writer, t + .001);
            }
            gz.Close();
            s.Close();
        }
        static void Main(string[] args)
        {
            SampleCreator creator = new SampleCreator();
            creator.WriteNiceLog();
        }
    }
}
