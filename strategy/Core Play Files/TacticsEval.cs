using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robocup.Core;
using Robocup.Geometry;

namespace Robocup.Plays
{
    public static class TacticsEval
    {

        static double FIELD_WIDTH;
        static double FIELD_HEIGHT;
        
        public static void LoadConstants()
        {
            FIELD_WIDTH = ConstantsRaw.get<double>("plays", "FIELD_WIDTH");
            FIELD_HEIGHT = ConstantsRaw.get<double>("plays", "FIELD_HEIGHT");
        }

        public static bool InField(Vector2 point)
        {
            if (point == null)
                return false;

            bool result = ((point.X <= FIELD_WIDTH / 2) && (point.X >= - FIELD_WIDTH / 2)
                        && (point.Y <= FIELD_HEIGHT / 2) && (point.Y >= - FIELD_HEIGHT / 2));
            return result;
        }
        //For reference, 
        //                   O
        //                   | x
        // Me ------>------>------>
        // 0m                1m
        // Me is kicking to the right, O is an opposing robot 1m away horizontally, and vertical distance x
        //
        // The blockedness for different values of x:
        // 
        // 0.1m - 0.93    (ball would just about graze opposing robot even if it didn't move)
        // 0.2m - 0.65    (about 10 cm of leeway if opposing robot doesn't move)
        // 0.3m - 0.37
        // 0.5m - 0.12
        // 1.0m - 0.01
        public static double kickBlockednessLinear(RobotInfo[] theirRobots, BallInfo ball, Vector2 target)
        {
            if (ball == null)
                return 0;

            Vector2 trajectory = target - ball.Position;
            if(trajectory.magnitudeSq() < 0.005 * 0.005)
                return 0;
            Vector2 trajectoryUnit = trajectory.normalizeToLength(1.0);

            double closestScaledDist = 10000000;
            for (int j = 0; j < theirRobots.Length; j++)
            {
                RobotInfo robot = theirRobots[j];
                if (robot == null || robot.Position == null)
                    continue;
                Vector2 ballToRobot = robot.Position - ball.Position;
                double parallelDist = ballToRobot * trajectoryUnit;
                double perpDist;
                if (parallelDist > 0)
                    perpDist = Math.Abs(Vector2.cross(ballToRobot, trajectoryUnit));
                else
                    perpDist = ballToRobot.magnitude();

                if (parallelDist < 0)
                    parallelDist = 0;
                parallelDist += 0.2; //Arbitarily add a little small value, to remove weird effects around 0                 
                perpDist += 0.01; //Arbitarily add a cm, to remove weird effects around 0

                double scaledDist = perpDist / parallelDist;
                if (scaledDist < closestScaledDist)
                    closestScaledDist = scaledDist;
            }

            if (closestScaledDist < 0)
                closestScaledDist = 0;

            return 1 / (1 + closestScaledDist * closestScaledDist * closestScaledDist * 100);

        }
    }
}
