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
            //this just has it move randomly:

            Vector2 newposition = info.Position + dt * info.Velocity;
            Vector2 newvelocity = new Vector2((float)(r.NextDouble() * 4 - 2), (float)(r.NextDouble() * 4 - 2));
            double newrotvelocity = 20 * r.NextDouble() - 10;
            return new RobotInfo(newposition, newvelocity, newrotvelocity, info.Orientation + (float)(info.RotationalVelocity * dt), info.ID);
            

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
