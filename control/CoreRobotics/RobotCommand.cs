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
            SET_CFG_FLAGS
        };

        static CRCTool _crcTool;
        static Command[] _iToCommand;
        static Dictionary<Command, byte> _commandToI;

        public static byte dribblerSpeed = 5;

        public WheelSpeeds Speeds;
        public int ID;
        public Command command;
        public byte P, I, D;
        public byte BoardID;
        public byte Flags;

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
        public RobotCommand(int ID, Command command, byte boardID, byte flags)
        {
            this.BoardID = boardID;
            this.Flags = flags;
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
            byte id = (byte)('0' + ID);
            byte chksum, source, port, arg; // source: w for brushless board, v for kicker board

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

                    //robots expect wheel powers in this order:
                    //rf lf lb rb                                       
                    
                    source = (byte)'w'; port = (byte)'w';
                    chksum = Checksum.Compute(new byte[] { id, source, port, 
                                                                         (byte)rf, (byte)lf, (byte)lb, (byte)rb });
                    Console.WriteLine("id " + ID + ": setting speeds to: " + Speeds.ToString() + " parity: " + chksum.ToString());

                    return new byte[]{(byte)'\\', (byte)'H', chksum, id, source, port,
                                      (byte)rf, (byte)lf, (byte)lb, (byte)rb,
                                      (byte)'\\', (byte)'E'};                                
                case Command.SET_PID:
                    source = (byte)'w'; port = (byte)'f';
                    chksum = Checksum.Compute(new byte[] { id, source, port, P, I, D });
                    return new byte[] {(byte)'\\', (byte)'H', chksum, id, source, port, P, I, D,
                                      (byte)'\\', (byte)'E'};
                case Command.SET_CFG_FLAGS:
                    source = (byte)'w'; port = (byte)'c';
                    chksum = Checksum.Compute(new byte[] { id, source, port, BoardID, Flags });
                    return new byte[] {(byte)'\\', (byte)'H', chksum, id, source, port, BoardID, Flags,
                                      (byte)'\\', (byte)'E'};
                case Command.START_DRIBBLER:
                    source = (byte)'v'; port = (byte)'d'; arg = (byte)('0' + (byte)dribblerSpeed);
                    chksum = Checksum.Compute(new byte[] { id, source, port, arg });
                    return new byte[] {(byte)'\\', (byte)'H', /*chksum,*/ id, source, port, arg,
                                      (byte)'\\', (byte)'E'};
                case Command.STOP_DRIBBLER:
                    source = (byte)'v'; port = (byte)'d'; arg = (byte)'0';
                    chksum = Checksum.Compute(new byte[] { id, source, port, arg });
                    return new byte[] {(byte)'\\', (byte)'H', /*chksum,*/ id, source, port, arg,
                                      (byte)'\\', (byte)'E'};
                case Command.KICK:           source = (byte)'v'; port = (byte)'k'; break;
                case Command.START_CHARGING: source = (byte)'v'; port = (byte)'c'; break;
                case Command.STOP_CHARGING:  source = (byte)'v'; port = (byte)'s'; break;
                case Command.BREAKBEAM_KICK: source = (byte)'v'; port = (byte)'b'; break;
                case Command.DISCHARGE:      source = (byte)'v'; port = (byte)'p'; break;
                case Command.RESET:          source = (byte)'v'; port = (byte)'r'; break;
                default:
                    throw new ApplicationException("Don't know how to package command: " + command.ToString());
            }

            // simplest commands fall through to here
            chksum = Checksum.Compute(new byte[] { id, source, port });
            return new byte[] { (byte)'\\', (byte)'H', /*chksum,*/ id, source, port, (byte)'\\', (byte)'E' };
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
                case Command.SET_CFG_FLAGS:
                    BoardID = buff[4];
                    Flags = buff[5];
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
                case Command.SET_CFG_FLAGS:
                    buff[4] = BoardID;
                    buff[5] = Flags;
                    break;
            }

            stream.Write(buff, 0, buff.Length);
        }

        #endregion
    }
}
