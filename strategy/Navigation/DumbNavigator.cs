using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Infrastructure;

namespace Navigation
{
    namespace Examples
    {
        public class DumbNavigator : INavigator
        {
            //for labeling purposes only
            const string NAME = "DumbNavigator";

            public string Name
            {
                get { return NAME; }
            }

            public NavigationResults navigate(int id, Vector2 position, Vector2 destination,
                RobotInfo[] teamPositions, RobotInfo[] enemyPositions, BallInfo ballPosition, float avoidBallDist)
            {
                return new NavigationResults(destination);
            }


            public void drawLast(System.Drawing.Graphics g, ICoordinateConverter c)
            {
            }
        }
    }
}
