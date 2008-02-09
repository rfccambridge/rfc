using System;
using System.Collections.Generic;
using System.Text;

namespace Robocup.Core
{
    [Serializable]
    public class WheelCommand {
        public WheelSpeeds speeds;
        public int ID;
        public WheelCommand(int ID, WheelSpeeds speeds) {
            this.speeds = speeds;
            this.ID = ID;
        }
    }
}
