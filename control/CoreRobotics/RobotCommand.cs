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
            MOVE,                       // Send wheelspeeds for 4 wheels
            KICK,                       // Charge & plunge, regardless of break-beam
            START_CHARGING,             // Start charging to full capacitor voltage (XXX: deprecated)
            START_VARIABLE_CHARGING,    // Start charging to a set target voltage
            STOP_CHARGING,              // Stop charging and maintain charge
            BREAKBEAM_KICK,             // Charge to full voltage & plunge whenever beam broken (XXX: deprecated)
            FULL_BREAKBEAM_KICK,        // Charge to target voltage & plunge whenever targetV reached and beam broken
            MIN_BREAKBEAM_KICK,         // Charge to target voltage & plunge whenever V > minV and beam broken
            START_DRIBBLER,             // Start the dribbler with a set speed
            STOP_DRIBBLER,              // Stop the dribbler
            DISCHARGE,                  // Discharge capacitors without plunging
            RESET,                      // Reset brushless board PIC
            SET_PID,                    // Set PID constants for single-speed control (TOOD: check if deprecated?)
            SET_CFG_FLAGS               // Set configuration flags
        };

        static CRCTool _crcTool;
        static Command[] _iToCommand;
        static Dictionary<Command, byte> _commandToI;

        public static byte DribblerSpeed = 10;
        // Equal to capacitor voltage we charge to / 10
        public byte KickerStrength = MAX_KICKER_STRENGTH;
        public byte MinKickerStrength = 8; // XXX: SK: This should be calibrated

        public const int MAX_KICKER_STRENGTH = 25;
        public const int MIN_KICKER_STRENGTH = 1;

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

        private int clampWheelSpeed(double speed)
        {
            const int MAXSPEED = 127;
            int s = (int)Math.Round((double)speed);
            if (s > MAXSPEED)
                s = MAXSPEED;
            if (s < -MAXSPEED)
                s = -MAXSPEED;
            return s;
        }

        public byte[] ToPacket()
        {
            byte id = (byte)('0' + ID);
            byte chksum, source, port, arg0, arg1; // source: w for brushless board, v for kicker board

            switch (command)
            {
                case Command.MOVE:

                    int lf = clampWheelSpeed(Speeds.lf),
                        rf = clampWheelSpeed(Speeds.rf),
                        lb = clampWheelSpeed(Speeds.lb),
                        rb = clampWheelSpeed(Speeds.rb);

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
                    source = (byte)'v'; port = (byte)'d'; arg0 = (byte)('0' + (byte)DribblerSpeed);
                    chksum = Checksum.Compute(new byte[] { id, source, port, arg0 });
                    return new byte[] {(byte)'\\', (byte)'H', /*chksum,*/ id, source, port, arg0,
                                      (byte)'\\', (byte)'E'};
                case Command.STOP_DRIBBLER:
                    source = (byte)'v'; port = (byte)'d'; arg0 = (byte)'0';
                    chksum = Checksum.Compute(new byte[] { id, source, port, arg0 });
                    return new byte[] {(byte)'\\', (byte)'H', /*chksum,*/ id, source, port, arg0,
                                      (byte)'\\', (byte)'E'};
                case Command.START_VARIABLE_CHARGING:
                    if (KickerStrength > MAX_KICKER_STRENGTH) KickerStrength = MAX_KICKER_STRENGTH;
                    if (KickerStrength < MIN_KICKER_STRENGTH) KickerStrength = MIN_KICKER_STRENGTH;
                    source = (byte)'v'; port = (byte)'v'; arg0 = (byte)KickerStrength;
                    chksum = Checksum.Compute(new byte[] { id, source, port, arg0 });
                    return new byte[] {(byte)'\\', (byte)'H', /*chksum,*/ id, source, port, arg0,
                                      (byte)'\\', (byte)'E'};
                case Command.FULL_BREAKBEAM_KICK:
                    if (KickerStrength > MAX_KICKER_STRENGTH) KickerStrength = MAX_KICKER_STRENGTH;
                    if (KickerStrength < MIN_KICKER_STRENGTH) KickerStrength = MIN_KICKER_STRENGTH;
                    source = (byte)'v'; port = (byte)'f'; arg0 = (byte) KickerStrength;
                    chksum = Checksum.Compute(new byte[] { id, source, port, arg0 });
                    return new byte[] {(byte)'\\', (byte)'H', /*chksum,*/ id, source, port, arg0,
                                      (byte)'\\', (byte)'E'};
                case Command.MIN_BREAKBEAM_KICK:
                    if (KickerStrength > MAX_KICKER_STRENGTH) KickerStrength = MAX_KICKER_STRENGTH;
                    if (KickerStrength < MIN_KICKER_STRENGTH) KickerStrength = MIN_KICKER_STRENGTH;
                    if (MinKickerStrength > MAX_KICKER_STRENGTH) MinKickerStrength = MAX_KICKER_STRENGTH;
                    if (MinKickerStrength < MIN_KICKER_STRENGTH) MinKickerStrength = MIN_KICKER_STRENGTH;
                    source = (byte)'v'; port = (byte)'m';
                    arg0 = (byte)KickerStrength; arg1 = (byte) MinKickerStrength;
                    chksum = Checksum.Compute(new byte[] { id, source, port, arg0, arg1 });
                    return new byte[] {(byte)'\\', (byte)'H', /*chksum,*/ id, source, port, arg0, arg1,
                                      (byte)'\\', (byte)'E'};
                case Command.KICK:              source = (byte)'v'; port = (byte)'k'; break;
                case Command.START_CHARGING:    source = (byte)'v'; port = (byte)'c'; break;
                case Command.STOP_CHARGING:     source = (byte)'v'; port = (byte)'s'; break;
                case Command.BREAKBEAM_KICK:    source = (byte)'v'; port = (byte)'b'; break;
                case Command.DISCHARGE:         source = (byte)'v'; port = (byte)'p'; break;
                case Command.RESET:             source = (byte)'v'; port = (byte)'r'; break;
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
                case Command.FULL_BREAKBEAM_KICK:
                case Command.START_VARIABLE_CHARGING:
                    KickerStrength = buff[4];
                    break;
                case Command.MIN_BREAKBEAM_KICK:
                    KickerStrength = buff[4];
                    MinKickerStrength = buff[5];
                    break;
                case Command.START_DRIBBLER:
                    DribblerSpeed = buff[4];
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
                    int lf = clampWheelSpeed(Speeds.lf),
                        rf = clampWheelSpeed(Speeds.rf),
                        lb = clampWheelSpeed(Speeds.lb),
                        rb = clampWheelSpeed(Speeds.rb);

                    buff[4] = (byte)(Speeds.rf < 0 ? (byte)Math.Abs(rf) | 0x80 : (byte)rf);
                    buff[5] = (byte)(Speeds.lf < 0 ? (byte)Math.Abs(lf) | 0x80 : (byte)lf);
                    buff[6] = (byte)(Speeds.lb < 0 ? (byte)Math.Abs(lb) | 0x80 : (byte)lb);
                    buff[7] = (byte)(Speeds.rb < 0 ? (byte)Math.Abs(rb) | 0x80 : (byte)rb);
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
                case Command.FULL_BREAKBEAM_KICK:
                case Command.START_VARIABLE_CHARGING:
                    buff[4] = KickerStrength;
                    break;
                case Command.MIN_BREAKBEAM_KICK:
                    buff[4] = KickerStrength;
                    buff[5] = MinKickerStrength;
                    break;
                case Command.START_DRIBBLER:
                    buff[4] = DribblerSpeed;
                    break;
            }

            stream.Write(buff, 0, buff.Length);
        }

        #endregion
    }
}
