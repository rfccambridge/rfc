using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;

namespace Robocup.CoreRobotics
{
    public static class MovementModeler
    {
        static private Random r = new Random();
        static public RobotInfo ModelWheelSpeeds(RobotInfo info, WheelSpeeds command, double dt)
        {
            //this just has them move randomly:

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
        }
    }
}
