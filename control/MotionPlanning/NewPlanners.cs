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
    //contains planners written, both for testing and practical purposes, for
    //the spring 2009 CS 199r class

    //To be distinguished from RRTPlanners.cs

    /// <summary>
    /// Give velocities towards a point with no consideration of obstacles and
    /// no use of PID feedback, RRT planning, or other intelligent paths- meant
    /// for testing WheelSpeedsExtender
    /// </summary>
    public class DumbPlanner : IMotionPlanner
    {
        //Distance to stop
        double STOP_DISTANCE = 0.1;

        // Dummy methods- required for interface
        public void ReloadConstants() { }

        public void DrawLast(System.Drawing.Graphics g, ICoordinateConverter c) { }

        /// <summary>
        /// Plan motion using straightforward WheelSpeedsExtender
        /// </summary>
        public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState,
            IPredictor predictor, double avoidBallRadius)
        {
            RobotInfo currentState = predictor.getCurrentInformation(id);

            // speeds default to zero
            WheelSpeeds speeds = new WheelSpeeds();

            // if farther away than stopping distance, get speesd to point
            if (currentState.Position.distanceSq(desiredState.Position) >
                STOP_DISTANCE * STOP_DISTANCE)
            {
                speeds = WheelSpeedsExtender.GetWheelSpeedsThrough(currentState, desiredState);
            }

            return new MotionPlanningResults(speeds);
        }
    }

    /// <summary>
    /// Planner that goes to a point by first turning to face it and then
    /// moving forward
    /// IN PROGRESS- DO NOT YET USE
    /// </summary>
    public class DumbTurnPlanner : IMotionPlanner
    {
        //Distance to stop
        double STOP_DISTANCE = 0.1;

        // Dummy methods- required for interface
        public void ReloadConstants() { }

        public void DrawLast(System.Drawing.Graphics g, ICoordinateConverter c) { }

        /// <summary>
        /// Plan motion using straightforward WheelSpeedsExtender
        /// </summary>
        public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState,
            IPredictor predictor, double avoidBallRadius)
        {
            RobotInfo currentState = predictor.getCurrentInformation(id);

            // speeds default to zero
            WheelSpeeds speeds = new WheelSpeeds();

            // find direction to point
            Vector2 dir_to_goal = (desiredState.Position - currentState.Position).normalize();
            double angle_to_goal = dir_to_goal.cartesianAngle();
            //double angle_diff = currentState.orientation - 

            // if farther away than stopping distance, get speesd to point
            if (currentState.Position.distanceSq(desiredState.Position) >
                STOP_DISTANCE * STOP_DISTANCE)
            {
                speeds = WheelSpeedsExtender.GetWheelSpeedsThrough(currentState, desiredState);
            }

            return new MotionPlanningResults(speeds);
        }
    }
}
