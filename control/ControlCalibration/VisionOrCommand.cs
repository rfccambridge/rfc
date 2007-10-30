using System;
using System.Collections.Generic;
using System.Text;

namespace Robocup.MotionControl
{
    //Here is the way the control calibration data is stored:
    //as logs of these things
    //in a gzip-compressed file
    //using the LogWriter<VisionOrCommand> class

    [Serializable]
    public class VisionOrCommand
    {
        public VisionMessage.RobotData vision;
        public WheelSpeeds command;
    }
}
