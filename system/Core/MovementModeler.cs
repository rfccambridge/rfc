using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Geometry;
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
        const double rr = 0.0783;
        double velocityCoe = 127 * 3 / (2 * Math.Sqrt(2)); // assuming maximum velocity is 3m/s
        double angVelocityCoe = 10 * 3 / (2 * Math.Sqrt(2)); 
        public double changeConstlf = 5;//proportional constant. we set the change is proportional to the gap. 
        public double changeConstlb = 5;
        public double changeConstrf = 5;
        public double changeConstrb = 5;

        private double GetNewVelocity(double command, double actual, double dt, double changek)
        {
            return actual + (command - actual) * (1 - Math.Exp(-changek * dt));
            //return command;
        }

        private WheelsInfo<double> GetNewWheel(WheelSpeeds command, WheelsInfo<double> actual, double dt)
        {
            WheelsInfo<double> newWheel = new WheelsInfo<double>();
            newWheel.lb = GetNewVelocity(command.lb, actual.lb, dt, changeConstlb);
            newWheel.rb = GetNewVelocity(command.rb, actual.rb, dt, changeConstrb);
            newWheel.lf = GetNewVelocity(command.lf, actual.lf, dt, changeConstlf);
            newWheel.rf = GetNewVelocity(command.rf, actual.rf, dt, changeConstrf);

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

            // from actual velocity to -127 to 127 scale
            WheelsInfo<double> Wheel = new WheelsInfo<double>();
            Wheel.lb = -lb * velocityCoe;
            Wheel.rb = rb * velocityCoe;
            Wheel.lf = -lf * velocityCoe;
            Wheel.rf = rf * velocityCoe;

            return Wheel;
        }

        public double GetAngFromWheel(WheelsInfo<double> Wheel)
        {
            return (Wheel.lf + Wheel.lb + Wheel.rf + Wheel.rb) / (angVelocityCoe * rr);
        }

        public Vector2 GetVelocityFromWheel(WheelsInfo<double> Wheel, double orientation)
        {
            Vector2 orientationVector = new Vector2(Math.Cos(orientation), Math.Sin(orientation));
            Vector2 lfv, rfv, rbv, lbv;  // unit vector pointing to each wheel's direction
            lfv = orientationVector.rotate(-Math.PI / 4);
            rfv = orientationVector.rotate(Math.PI / 4);
            rbv = lfv; lbv = rfv;

            return 1 / velocityCoe * (-Wheel.lb * lbv + Wheel.rb * rbv + -Wheel.lf * lfv + Wheel.rf * rfv);
        }

        public Pair<Vector2, double> GetInfoFromWheelSpeeds(WheelsInfo<double> Wheel, double orientation)
        {
            double AngularVelocity = GetAngFromWheel(Wheel);
            Vector2 newVelocity = GetVelocityFromWheel(Wheel, orientation);

            return new Pair<Vector2, double>(newVelocity, AngularVelocity);
        }

        /// <summary>
        /// Given the current state of a robot, the command most recently sent to the robot,
        /// extrapolates the state of the robot forward a given amount of time.
        /// </summary>
        /// <param name="info">The current state of the robot.</param>
        /// <param name="command">The command last sent to the robot.</param>
        /// <param name="dt">The amount of time, in seconds, to extrapolate forward.
        /// It is assumed that dt is less than 1/10 of a second.</param>
        /// <returns>Returns the state of the robot, extrapolated a time dt into the future.</returns>       
        public RobotInfo ModelWheelSpeeds(RobotInfo info, WheelSpeeds command, double dt)
        {
            WheelsInfo<double> currentWheel = GetWheelSpeedsFromInfo(info);
            WheelsInfo<double> newWheel = GetNewWheel(command, currentWheel, dt);

            double newAngularVelocity = GetAngFromWheel(newWheel);
            double angle = info.Orientation + (info.AngularVelocity + newAngularVelocity) / 2 * dt;

            Pair<Vector2, double> vwpair = GetInfoFromWheelSpeeds(newWheel, angle);
            Vector2 newVelocity = vwpair.First;
            Vector2 newPosition = info.Position + 0.5 * dt * (newVelocity + info.Velocity);

            if ((newPosition - info.Position).magnitudeSq() > 1)
            {
                Console.WriteLine("CRAP");
            }
            return new RobotInfo(newPosition, newVelocity, newAngularVelocity, angle, info.Team, info.ID);
        }
    }
}
