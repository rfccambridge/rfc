using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robocup.Core;


namespace Robocup.Plays
{
    class ChessHelpers
    {
        public static bool inRegion(Vector2 point, Vector2 center, double radius)
		{
			// Define points
			double x = point.X;
			double y = point.Y;
			double a = center.X;
			double b = center.Y;
			
			// Check if point in circle
			bool result = (((x-a)*(x-a)+(y-b)*(y-b)) <= radius*radius);
			return result;
		}
		
		public static double numEnemyRobotsOnOurHalf(RobotInfo[] theirRobots)
		{
			double count = 0;
			foreach (RobotInfo robot in theirRobots)
			{
				if (robot.Position.X < 0)
					count++;
			}
            return count;
		}
    }
}
