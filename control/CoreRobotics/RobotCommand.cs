using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Utilities;
using Robocup.Core;

namespace Robocup.CoreRobotics
{
    public class RobotCommand : Robocup.MessageSystem.IByteSerializable<RobotCommand> {
        public enum Command
        {
            MOVE,
            KICK,            
            START_CHARGING,
            STOP_CHARGING,
            BREAKBEAM_KICK,
            START_DRIBBLER,
            STOP_DRIBBLER,
            DISCHARGE,
            RESET,
            SET_PID,
            START_SPEW,
            STOP_SPEW
        };

        static CRCTool _crcTool;
        static Command[] _iToCommand;
        static Dictionary<Command, byte> _commandToI;

        public WheelSpeeds Speeds;
        public int ID;
        public Command command;
        public byte P, I, D;
        public byte BoardID;

        static RobotCommand()
        {
            _crcTool = new CRCTool();
            _crcTool.Init(CRCTool.CRCCode.CRC8);

            // I couldn't find a built-in way to convert between enums and corresponding numbers
            _iToCommand = (Command[])Enum.GetValues(typeof(Command));
            _commandToI = new Dictionary<Command, byte>();
            for (byte i = 0; i < _iToCommand.Length; i++)
                _commandToI.Add(_iToCommand[i], i);
        }

        public RobotCommand()
        {
            // Used for serialization. 
            // Deserialize() method populates the object created by this constructor.
        }

        public RobotCommand(int ID, Command command)
        {
            init(ID, command, null);
        }
        public RobotCommand(int ID, WheelSpeeds speeds) {
            init(ID, Command.MOVE, speeds);
        }
        public RobotCommand(int ID, Command command, byte P, byte I, byte D)
        {
            this.P = P;
            this.I = I;
            this.D = D;
            init(ID, command, null);
        }
        public RobotCommand(int ID, Command command, byte boardID)
        {
            this.BoardID = boardID;
            init(ID, command, null);
        }
        public RobotCommand(int ID, Command command, WheelSpeeds speeds)
        {
            init(ID, command, speeds);
        }

        private void init(int ID, Command command, WheelSpeeds speeds)
        {
            this.Speeds = speeds;
            this.ID = ID;
            this.command = command;
        }

        public byte[] ToPacket()
        {
            byte checksum;

            switch (command)
            {
                case Command.MOVE:
                    const int MAXSPEED = 127;

                    int lf = Speeds.lf, rf = Speeds.rf, lb = Speeds.lb, rb = Speeds.rb;

                    rf = rf > MAXSPEED ? MAXSPEED : rf;
                    lf = lf > MAXSPEED ? MAXSPEED : lf;
                    lb = lb > MAXSPEED ? MAXSPEED : lb;
                    rb = rb > MAXSPEED ? MAXSPEED : rb;

                    // board bugs out if we send an unescaped slash
                    if (lb == '\\') lb++;
                    if (lf == '\\') lf++;
                    if (rf == '\\') rf++;
                    if (rb == '\\') rb++;

                    Console.WriteLine("id " + ID + ": setting speeds to: " + Speeds.ToString());

                    //robots expect wheel powers in this order:
                    //rf lf lb rb

                    checksum = (byte)_crcTool.crctablefast(new byte[] { (byte)rf, (byte)lf, (byte)lb, (byte)rb });

                    return new byte[]{(byte)'\\', (byte)'H', (byte) ('0' + ID), 
                                      (byte)'w',  // port
                                      (byte)'w',  // command for wheel speeds
                                      (byte)rf, (byte)lf, (byte)lb, (byte)rb,
                                      checksum,
                                      (byte)'\\', (byte)'E'};
                case Command.KICK:
                    return new byte[] { (byte)'\\', (byte)'H', (byte) ('0' + ID), 
                                       (byte)'v', // port
                                       (byte)'k', // command for kick
                                       (byte)'\\', (byte)'E' };
                case Command.START_CHARGING:
                    return new byte[] { (byte)'\\', (byte)'H', (byte) ('0' + ID), 
                                        (byte)'v', // port
                                        (byte)'c', // command for start charging
                                        (byte)'\\', (byte)'E' };
                case Command.STOP_CHARGING:
                    return new byte[] { (byte)'\\', (byte)'H', (byte) ('0' + ID), 
                                        (byte)'v', // port
                                        (byte)'s', // command to stop charging
                                        (byte)'\\', (byte)'E' };
                case Command.BREAKBEAM_KICK:
                    return new byte[] { (byte)'\\', (byte)'H', (byte) ('0' + ID), 
                                        (byte)'v', // port
                                        (byte)'b', // command to listen to break beam
                                        (byte)'\\', (byte)'E' };
                case Command.START_DRIBBLER:
                    return new byte[] { (byte)'\\', (byte)'H', (byte) ('0' + ID), 
                                        (byte)'v',            // port
                                        (byte)'d', (byte)'1', // command for start dribbler
                                        (byte)'\\', (byte)'E' };
                case Command.STOP_DRIBBLER:
                    return new byte[] { (byte)'\\', (byte)'H', (byte) ('0' + ID), 
                                        (byte)'v',            // port
                                        (byte)'d', (byte)'0', // command for stop dribbler
                                        (byte)'\\', (byte)'E' };
                case Command.SET_PID:
                    checksum = (byte)_crcTool.crctablefast(new byte[] { P, I, D });
                    return new byte[] {(byte)'\\', (byte)'H', (byte) ('0' + ID),
                                       (byte)'w', // port
                                       (byte)'f', // command for PID const setting
                                       P, I, D,
                                       checksum,
                                      (byte)'\\', (byte)'E'};
                case Command.START_SPEW:
                    return new byte[] { (byte)'\\', (byte)'H', (byte) ('0' + ID), 
                                        (byte)'v', // port
                                        (byte)'e', BoardID, (byte)'1',  // command to turn on spewing
                                        (byte)'\\', (byte)'E' };
                case Command.STOP_SPEW:
                    return new byte[] { (byte)'\\', (byte)'H', (byte) ('0' + ID), 
                                        (byte)'v', // port
                                        (byte)'e', BoardID, (byte)'0',  // command to turn on spewing
                                        (byte)'\\', (byte)'E' };
                case Command.DISCHARGE:
                    return new byte[] { (byte)'\\', (byte)'H', (byte) ('0' + ID), 
                                        (byte)'v', // port
                                        (byte)'p', // command to discharge
                                        (byte)'\\', (byte)'E' };
                case Command.RESET:
                    return new byte[] { (byte)'\\', (byte)'H', (byte) ('0' + ID), 
                                        (byte)'v',
                                        (byte)'r', (byte)'r', // command to reset boards
                                        (byte)'\\', (byte)'E' };
                default:
                    throw new ApplicationException("Don't know how to package command: " + command.ToString());
            }
        }        

        #region IByteSerializable<RobotCommand> Members

        // Serialized format: 
        // Byte 0: Command
        // Byte 1: ID
        // Byte 2-3: <unused>
        // Byte 4-7: Data

        private const int SERIALIZED_LENGTH = 8; // bytes
        private byte[] buff = new byte[SERIALIZED_LENGTH]; // I don't think we need to lock this

        public void Deserialize(System.Net.Sockets.NetworkStream stream)
        {
            int read = stream.Read(buff, 0, buff.Length);
            if (read > 0 && read != buff.Length)
            {
                throw new ApplicationException(String.Format("RobotCommand.Deserialize: read {0:G} " +
                    "but expecting {1:G}.",  read, buff.Length));
            }

            command = _iToCommand[buff[0]];
            ID = buff[1];
            switch (command)
            {
                case Command.MOVE:
                    Speeds = new WheelSpeeds((buff[4] & 0x80) > 0 ? -1 * (buff[4] & 0x7F) : buff[4],
                                             (buff[5] & 0x80) > 0 ? -1 * (buff[5] & 0x7F) : buff[5],
                                             (buff[6] & 0x80) > 0 ? -1 * (buff[6] & 0x7F) : buff[6],
                                             (buff[7] & 0x80) > 0 ? -1 * (buff[7] & 0x7F) : buff[7]);
                    break;
                case Command.SET_PID:
                    P = buff[4];
                    I = buff[5];
                    D = buff[6];
                    break;
                case Command.START_SPEW:
                case Command.STOP_SPEW:
                    BoardID = buff[4];
                    break;
            }
        }

        public void Serialize(System.Net.Sockets.NetworkStream stream)
        {
            buff.Initialize(); // Clear

            buff[0] = _commandToI[command];
            buff[1] = (byte)ID;
            switch (command)
            {
                case Command.MOVE:
                    buff[4] = (byte)(Speeds.rf < 0 ? (byte)Math.Abs(Speeds.rf) | 0x80 : (byte)Speeds.rf);
                    buff[5] = (byte)(Speeds.lf < 0 ? (byte)Math.Abs(Speeds.lf) | 0x80 : (byte)Speeds.lf);
                    buff[6] = (byte)(Speeds.lb < 0 ? (byte)Math.Abs(Speeds.lb) | 0x80 : (byte)Speeds.lb);
                    buff[7] = (byte)(Speeds.rb < 0 ? (byte)Math.Abs(Speeds.rb) | 0x80 : (byte)Speeds.rb);
                    break;
                case Command.SET_PID:
                    buff[4] = P;
                    buff[5] = I;
                    buff[6] = D;
                    break;
                case Command.START_SPEW:
                case Command.STOP_SPEW:
                    buff[4] = BoardID;
                    break;
            }

            stream.Write(buff, 0, buff.Length);
        }

        #endregion
    }
}
