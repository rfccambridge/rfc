using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Utilities;
using Robocup.Core;

namespace Robocup.CoreRobotics
{
    [Serializable]
    public class RobotCommand {
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

        public WheelSpeeds Speeds;
        public int ID;
        public Command command;
        public byte P, I, D;
        public byte BoardID;

        static RobotCommand()
        {
            _crcTool = new CRCTool();
            _crcTool.Init(CRCTool.CRCCode.CRC8);
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

        private void init(int ID, Command command, WheelSpeeds speeds)
        {
            this.Speeds = speeds;
            this.ID = ID;
            this.command = command;
        }
    }
}
