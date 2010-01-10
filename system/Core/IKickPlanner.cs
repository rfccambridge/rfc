using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using System.Drawing;

namespace Robocup.CoreRobotics
{
    /// <summary>
    /// This is an interface for classes that are similar to IMotionPlanner but are used
    /// exclusively for kicking- a planner gets a robot ID, a target, and a predictor and
    /// returns a KickPlanningResults object containing wheel speeds and whether to turn
    /// on the breakbeam
    /// </summary>
    public interface IKickPlanner
    {
        KickPlanningResults kick(Team team, int id, Vector2 target, IPredictor predictor);
        void DrawLast(Graphics g, ICoordinateConverter c);
        void LoadConstants();

        // Distance at which to activate this motion planner
        double getDistanceActivate();
    }

    /// <summary>
    /// Contains wheel speeds to send to the robot and whether to turn on the break beam
    /// </summary>
    public class KickPlanningResults
    {
        /// <summary>
        /// Initialized only with wheel speeds- do not turn on break beam
        /// </summary>
        /// <param name="wheel_speeds"></param>
        public KickPlanningResults(WheelSpeeds wheel_speeds)
        {
            this.wheel_speeds = wheel_speeds;
            this.turnOnBreakBeam = false;
        }

        /// <summary>
        /// Initialized with wheel speeds and whether to turn on the break beam
        /// </summary>
        /// <param name="wheel_speeds"></param>
        /// <param name="turnOnBreakBeam"></param>
        public KickPlanningResults(WheelSpeeds wheel_speeds, bool turnOnBreakBeam)
        {
            this.wheel_speeds = wheel_speeds;
            this.turnOnBreakBeam = turnOnBreakBeam;
        }

        public WheelSpeeds getWheelSpeeds()
        {
            return wheel_speeds;
        }

        public WheelSpeeds wheel_speeds;
        public bool turnOnBreakBeam;
    }
}
