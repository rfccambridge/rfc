using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using Robocup.Geometry;

namespace Navigation
{
    namespace Examples
    {
        public class DumbNavigator : INavigator
        {
            public NavigationResults navigate(int id, Vector2 position, Vector2 destination,
                RobotInfo[] teamPositions, RobotInfo[] enemyPositions, BallInfo ballPosition, double avoidBallDist)
            {
                return new NavigationResults(destination);
            }


            public void drawLast(System.Drawing.Graphics g, ICoordinateConverter c)
            {
            }
        }
    }
}
