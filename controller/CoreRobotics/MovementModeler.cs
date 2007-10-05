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
        public RobotInfo ModelWheelSpeeds(RobotInfo info, WheelSpeeds command, double dt)
        {
            const double fractionold = .4;
            double oldfraction = Math.Exp(dt * Math.Log(fractionold));

            Vector2 fl = new Vector2(1, 1).normalize();
            Vector2 fr = new Vector2(1, -1).normalize();
            Vector2 newvelocity = (command.rf + command.lb) * fl + (command.lf + command.rb) * fr;
            newvelocity = (2.0 / 512) * newvelocity.rotate(info.Orientation);
            double newrotvelocity = (10.0 / 256) * (command.rf - command.lb - command.lf + command.rb);

            newrotvelocity = oldfraction * info.AngularVelocity + (1 - oldfraction) * newrotvelocity;
            newvelocity = oldfraction * info.Velocity.rotate(info.AngularVelocity * dt) + (1 - oldfraction) * newvelocity;
            Vector2 newposition = info.Position + dt * .5 * (info.Velocity + newvelocity);
            double neworientation = info.Orientation + dt * .5 * (info.AngularVelocity + newrotvelocity);
            return new RobotInfo(newposition, newvelocity, newrotvelocity, neworientation, info.ID);


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
