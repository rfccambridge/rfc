using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robocup.Core;
using Robocup.Geometry;

namespace Robocup.Plays
{
    class TacticsEval
    {
        const double KICK_SPEED = 5; //in m/s
        const double ROBOT_MAX_ACCEL = 2.5; //in m/s^2

        public static double kickBlockedness(RobotInfo[] ourRobots, RobotInfo[] theirRobots,
            BallInfo ball, Vector2 target, double kickDT)
        {
            if (ball == null)
                return 1;
            double totalTravelTime = (target - ball.Position).magnitude() / KICK_SPEED;

            const int NUM_INCREMENTS = 60;

            double worstBlockedness = 0;
            for (int i = 0; i <= NUM_INCREMENTS; i++)
            {
                double prop = (double)i / NUM_INCREMENTS;
                Vector2 ballPos = ball.Position + prop * (target - ball.Position);
                double dt = kickDT + prop * totalTravelTime;

                for (int j = 0; j < theirRobots.Length; j++)
                {
                    RobotInfo robot = theirRobots[j];
                    if (robot == null)
                        continue;

                    Vector2 robotFuturePosNoAccel = robot.Position + dt * robot.Velocity;
                    Vector2 ballToRobot = robotFuturePosNoAccel - ballPos;
                    Vector2 ballToCur = ball.Position - ballPos;

                    //Stop counting once the ball has passed the right angle position with the robot
                    if (ballToRobot * ballToCur < 0)
                        continue;

                    double dist = ballToRobot.magnitude();
                    double distWithAccel = dist - 0.5 * ROBOT_MAX_ACCEL * dt * dt;
                    if (distWithAccel < 0)
                        distWithAccel = 0;
                    double blockedness = 2.0 / (1.0 + distWithAccel * distWithAccel * 100.0);
                    if (blockedness > worstBlockedness)
                        worstBlockedness = blockedness;
                }
            }

            double ret = worstBlockedness;
            if (ret < 0) ret = 0;     //Ensure nonneg
            ret = 2 / (1 + Math.Exp(-2 * ret)) - 1; //Map into [0,1], using half of a logistic cruve
            return ret;
        }

        public static double kickClosestDistFromThem(RobotInfo[] theirRobots,
            BallInfo ball, Vector2 target)
        {
            if (ball == null)
                return 0;

            Line line = new Line(ball.Position, target);
            double closestDist = Double.PositiveInfinity;

            for (int j = 0; j < theirRobots.Length; j++)
            {
                RobotInfo robot = theirRobots[j];
                if (robot == null || robot.Position == null)
                    continue;
                double dist = line.distFromLine(robot.Position);
                dist /= (ball.Position - robot.Position).magnitude();
                if (dist < closestDist)
                    closestDist = dist;
            }
            return closestDist;
        }
    }
}
