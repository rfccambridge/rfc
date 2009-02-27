using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Core;
using Robocup.CoreRobotics;
using Robocup.Geometry;

using System.Drawing;

using Navigation.Examples;
using System.IO;
using Robocup.Utilities;


namespace Robocup.MotionControl
{
    // Implementations of IPathPlanners- meant to design paths for robot
    // to follow

    public class StickyFeedbackPathPlanner
    {
        public WheelSpeeds followPath(RobotPath path, IPredictor predictor)
        {
            return new WheelSpeeds();
        }
        void ReloadConstants() { }
    }
}
