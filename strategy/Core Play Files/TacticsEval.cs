using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robocup.Core;

namespace Robocup.Plays
{
    class TacticsEval
    {
        const double KICK_SPEED = 6; //in m/s
        const double ROBOT_MAX_ACCEL = 2.5; //in m/s^2
        //const double ROBOT_MAX_SPEED = 2; //in m/s

        public static double kickBlockedness(RobotInfo[] ourRobots, RobotInfo[] theirRobots, 
            BallInfo ball, Vector2 target, double kickDT)
        {
            double totalTravelTime = (target - ball.Position).magnitude() /  KICK_SPEED;
            
            const int NUM_INCREMENTS = 60;

            double worstBlockedness = 0;
            double totalBlockedness = 0;
            for (int i = 0; i <= NUM_INCREMENTS; i++)
            {
                double prop = (double)i / NUM_INCREMENTS;
                Vector2 ballPos = ball.Position + prop * (target - ball.Position);
                double dt = kickDT + prop * totalTravelTime;

                for (int j = 0; j < theirRobots.Length; j++)
                {
                    RobotInfo robot = theirRobots[j];
                    Vector2 robotFuturePosNoAccel = robot.Position + dt * robot.Velocity;
                    Vector2 robotToBall = ballPos - robotFuturePosNoAccel;
                    double dist = robotToBall.magnitude();
                    double distWithAccel = dist - 0.5 * ROBOT_MAX_ACCEL * dt * dt;
                    if (distWithAccel < 0)
                        distWithAccel = 0;
                    double blockedness = 1.0 / (1.0 + distWithAccel * distWithAccel * 100.0);

                    totalBlockedness += blockedness;
                    if(blockedness > worstBlockedness)
                        worstBlockedness = blockedness;
                }
            }
            return worstBlockedness + totalBlockedness/(NUM_INCREMENTS*6);
        }
    }
}
