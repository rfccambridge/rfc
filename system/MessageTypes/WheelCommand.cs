using System;
using System.Collections.Generic;
using System.Text;

namespace Robocup.Core
{
    [Serializable]
    public class RobotCommand {
        public enum Command
        {
            MOVE,
            KICK,
            BEAMKICK
        };
        public WheelSpeeds speeds;
        public int ID;
        public Command command;
        
        public RobotCommand(int ID, WheelSpeeds speeds) {
            init(ID, Command.MOVE, speeds);
        }
        public RobotCommand(int ID, Command command, WheelSpeeds speeds)
        {
            init(ID, command, speeds);
        }
        private void init(int ID, Command command, WheelSpeeds speeds)
        {
            this.speeds = speeds;
            this.ID = ID;
            this.command = command;
        }
    }
}
