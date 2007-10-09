



using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;

namespace Robocup.CoreRobotics
{
    /// <summary>
    /// A class that models the movement of a robot.  Each object of this class will only be responsible for modeling one robot;
    /// ie, it is not responsible for holding a different set of parameters for each robot.
    /// </summary>
    public class MovementModeler
    {
        static private Random r = new Random();
        /// <summary>
        /// Given the current state of a robot, the command most recently sent to the robot,
        /// extrapolates the state of the robot forward a given amount of time.
        /// </summary>
        /// <param name="info">The current state of the robot.</param>
        /// <param name="command">The command last sent to the robot.</param>
        /// <param name="dt">The amount of time, in seconds, to extrapolate forward.
        /// It is assumed that dt is less than 1/10 of a second.</param>
        /// <returns>Returns the state of the robot, extrapolated a time dt into the future.</returns>       
        const double rr = 0.09;
        private double velocityCoe = 127 * 4 / (2 * Math.Sqrt(2)); // assuming maximum velocity is 4m/s
        const double changeConst = 8;// k = proportional constant. we set the change is proportional to the gap. 

        private double GetNewVelocity(double command, double actual, double dt)
        {
            return actual + (command - actual) * (1 - Math.Exp(-changeConst * dt));
        }

        private WheelsInfo<double> GetNewWheel(WheelSpeeds command, WheelsInfo<double> actual, double dt)
        {
            WheelsInfo<double> newWheel = new WheelsInfo<double>();
            newWheel.lb = GetNewVelocity(command.lb, actual.lb, dt);
            newWheel.rb = GetNewVelocity(command.rb, actual.rb, dt);
            newWheel.lf = GetNewVelocity(command.lf, actual.lf, dt);
            newWheel.rf = GetNewVelocity(command.rf, actual.rf, dt);

            return newWheel;
        }

        public WheelsInfo<double> GetWheelSpeedsFromInfo(RobotInfo info)
        {
            Vector2 orientationVector = new Vector2(Math.Cos(info.Orientation), Math.Sin(info.Orientation));
            Vector2 lfv, rfv, rbv, lbv;
            lfv = orientationVector.rotate(-Math.PI / 4);
            rfv = orientationVector.rotate(Math.PI / 4);
            rbv = lfv; lbv = rfv;

            double lfprod = lfv * info.Velocity;
            double rfprod = rfv * info.Velocity;
            double wr = info.AngularVelocity * rr;

            //From Current State to Wheel Velocity
            // lfv is the unit vector pointing along the lf direction
            // rfv is the unit vector pointing along the rf direction
            // Velocity = vector sum ( lf + rf + lb + rb )
            // rf + rb - lf - lb = wr
            // lf + rb = Velocity * lfv = lfprod  (because vector lb, rf is perpendicular to lfv)
            // lb + rf = Velocity * rfv = rfprod   

            double lb, lf, rb, rf;
            lb = rfprod / 2;
            rf = rfprod / 2;
            rb = (wr + lfprod) / 2;
            lf = (lfprod - wr) / 2;

            WheelsInfo<double> Wheel = new WheelsInfo<double>();
            Wheel.lb = lb * velocityCoe;
            Wheel.rb = rb * velocityCoe;
            Wheel.lf = lf * velocityCoe;
            Wheel.rf = rf * velocityCoe;

            return Wheel;
        }

        public RobotInfo GetInfoFromWheelSpeeds(WheelsInfo<double> Wheel, double orientation, Vector2 position, int ID)
        {
            Vector2 orientationVector = new Vector2(Math.Cos(orientation), Math.Sin(orientation));
            Vector2 lfv, rfv, rbv, lbv;
            lfv = orientationVector.rotate(-Math.PI / 4);
            rfv = orientationVector.rotate(Math.PI / 4);
            rbv = lfv; lbv = rfv;

            Vector2 newVelocity = 1 / velocityCoe * (Wheel.lb * lbv + Wheel.rb * rbv + Wheel.lf * lfv + Wheel.rf * rfv);
            double AngularVelocity = (-Wheel.lf - Wheel.lb + Wheel.rf + Wheel.rb) / (velocityCoe * rr);

            return new RobotInfo(position, newVelocity, AngularVelocity, orientation, ID);
        }

        public RobotInfo ModelWheelSpeeds(RobotInfo info, WheelSpeeds command, double dt)
        {
            //this just has it move randomly:
            //Vector2 newposition = info.Position + dt * info.Velocity;
            //Vector2 newvelocity = new Vector2((double)(r.NextDouble() * 4 - 2), (double)(r.NextDouble() * 4 - 2));
            //double newrotvelocity = 20 * r.NextDouble() - 10;
            //return new RobotInfo(newposition, newvelocity, newrotvelocity, info.Orientation + (double)(info.AngularVelocity * dt), info.ID);

            WheelsInfo<double> currentWheel, newWheel;

            currentWheel = GetWheelSpeedsFromInfo(info);

            // from wheel velocity to dt later, wheel velocity

            newWheel = GetNewWheel(command, currentWheel, dt);

            double angle = info.Orientation + info.AngularVelocity * dt;
            Vector2 newPosition = info.Position + dt * info.Velocity;

            RobotInfo newRobot = GetInfoFromWheelSpeeds(newWheel, angle, newPosition, info.ID);

            return newRobot;

            //idea for implementation:
            //calculate current wheel speeds from velocity+rotational velocity (assume there is no slipping)
            //assume some function of how the wheels react (simplest is probably "the change in speed is proportional to the desired change in speed")
            //set the new wheel speeds
            //recalculate velocity+rotational velocity, and update
            //calculate position+orientation

            //it can make linear approximations all over the place, since dt will usually be 1/60 or so
            //if such approximations arent necessary, and a weaker bound on dt is sufficient, then that should be changed in the summary
        }
    }
}
