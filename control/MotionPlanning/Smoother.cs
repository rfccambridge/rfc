using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Robocup.Core;
using Robocup.CoreRobotics;

namespace Robocup.MotionControl
{
    static class Smoother
    {
        const double valueTol = .01;

        static private List<Vector2> smoothed_path = new List<Vector2>();
        static int last_reason = 0;
        static public void DrawLast(Graphics g, ICoordinateConverter c)
        {
            Brush ind = new SolidBrush(Color.Black);
            if (last_reason == 1)
                ind = new SolidBrush(Color.Red);
            if (last_reason == 2)
                ind = new SolidBrush(Color.Green);
            if (last_reason == 3)
                ind = new SolidBrush(Color.Blue);
            g.FillRectangle(ind, c.fieldtopixelX(-2), c.fieldtopixelY(-1), c.fieldtopixelX(-1.9) - c.fieldtopixelX(-1.95), c.fieldtopixelY(-1) - c.fieldtopixelY(-.95));

            Brush b = new SolidBrush(Color.Black);

            foreach (Vector2 v in smoothed_path)
            {
                g.FillRectangle(b, c.fieldtopixelX(v.X) - 1, c.fieldtopixelY(v.Y) - 1, 2, 2);
            }

            b.Dispose();
        }

        static private RobotInfo Extend(RobotInfo start, Vector2 end)
        {
            return Common.ExtendRV(start, end, new List<Obstacle>()).extension;
        }
        static private WheelSpeeds SmoothWithValue(RobotInfo startState, List<Vector2> waypoints,
            List<Obstacle> obstacles, double maxDistance, bool save)
        {
            WheelSpeeds rtn = null;
            RobotInfo cur = startState;
            List<Vector2> smoothed = new List<Vector2>();
            smoothed.Add(cur.Position);
            for (int i = 0; i < waypoints.Count; i++)
            {
                Vector2 v = waypoints[i];
                double dist = v.distanceSq(cur.Position);
                while (dist > maxDistance * maxDistance || (i == waypoints.Count - 1 && dist > .01 * .01))
                {
                    cur = Extend(cur, v);
                    smoothed.Add(cur.Position);
                    if (/*i == waypoints.Count-1 &&*/ cur.Position.distanceSq(v) > dist)
                        break;
                    if (Common.Blocked(cur.Position, obstacles))
                        return null;
                    if (rtn == null)
                        rtn = WheelSpeedsExtender.GetWheelSpeeds(startState, v);
                        //rtn = new MotionPlanningResults(WheelSpeedsExtender.GetWheelSpeeds(startState, cur.Position));
                    dist = v.distanceSq(cur.Position);
                }
            }
            if (save)
                smoothed_path = smoothed;
            return rtn;
        }

        static MotionPlanningResults last_rtn = null;
        static public MotionPlanningResults Smooth(RobotInfo startState, RobotInfo desiredState, List<Vector2> waypoints,
            List<Obstacle> obstacles)
        {

            //Console.WriteLine("SMOOTHER.SMOOTH(): going from: " + startState.ToString() + " to: " + desiredState.ToString());
            /*if (waypoints.Count > 5)
            {
                return new MotionPlanningResults(WheelSpeedsExtender.GetWheelSpeeds(startState, waypoints[5]));
            }*/

            RobotInfo start = startState;
            last_reason = 0;
            if (start.Position.distanceSq(waypoints[waypoints.Count - 1]) < .18 * .18)
            { // going directly towards goal
                last_reason = 1;
                //return new MotionPlanningResults(WheelSpeedsExtender.GetWheelSpeeds(start, waypoints[waypoints.Count - 1]));
                return new MotionPlanningResults(addOrientation( startState.Orientation, desiredState.Orientation,
                    WheelSpeedsExtender.GetWheelSpeeds(start, desiredState)));
            }

            double lowerBound = .01;
            double test = .1;


            if (SmoothWithValue(start, waypoints, obstacles, lowerBound, false) == null)
            {
                start = new RobotInfo(start.Position, new Vector2(), start.AngularVelocity, start.Orientation, start.ID);
            }

            MotionPlanningResults rtn = null;
            WheelSpeeds tempSpeeds = null;
            while ((tempSpeeds = SmoothWithValue(start, waypoints, obstacles, test, true)) != null)
            {
                rtn = new MotionPlanningResults(addOrientation(startState.Orientation, desiredState.Orientation, 
                    tempSpeeds));

                lowerBound = test;
                if (waypoints[waypoints.Count - 1].distanceSq(start.Position) < test * test)
                {
                    last_reason = 2;
                    return rtn;
                }
                test *= 2;
            }
            double upperBound = test;
            while (upperBound - lowerBound > valueTol)
            {
                double guess = (upperBound + lowerBound) / 2;
                if (SmoothWithValue(start, waypoints, obstacles, guess, false) == null)
                    upperBound = guess;
                else
                    lowerBound = guess;
            }

            tempSpeeds = SmoothWithValue(start, waypoints, obstacles, lowerBound, true);
            
            if (tempSpeeds == null)
            {
                if (last_rtn != null)
                {
                    rtn = last_rtn;
                    last_rtn = null;
                    return rtn;
                }
                last_reason = 3;
                return new MotionPlanningResults(addOrientation(startState.Orientation, desiredState.Orientation, 
                    WheelSpeedsExtender.GetWheelSpeeds(start, waypoints[Math.Min(5, waypoints.Count - 1)])));
            }
            rtn = new MotionPlanningResults(addOrientation(startState.Orientation, desiredState.Orientation, 
                tempSpeeds));
            last_rtn = rtn;
            return rtn;
        }

        // hack to add in orientation information: add PID here
        static private WheelSpeeds addOrientation(double startOrientation, double goalOrientation, WheelSpeeds speeds)
        {
            const double turnSpeed = 5.0;
            // we need orientation and speed information 
            int turnIncrement = (int)(turnSpeed * getTurnDirection(startOrientation, goalOrientation));
            //Console.WriteLine("TURNINCREMENT: " + turnIncrement);

            speeds.lf += -1*turnIncrement;
            speeds.rf += turnIncrement;
            speeds.lb += -1*turnIncrement;
            speeds.rb += turnIncrement;

            return speeds;
        }
        
        // constrains to between [-pi, pi]
        static private double constrainAngle(double angle)
        {
            while (angle > Math.PI)
            {
                angle -= 2 * Math.PI;
            }
            while (angle < -Math.PI)
            {
                angle += 2 * Math.PI;
            }
            return angle;
        }

        // return +1 if we need to turn counter-clockwise to get to goal.
        // later make this do PID on angle?
        const double ANGLE_THRESHOLD = 0.15;
        static private double getTurnDirection(double startOrient, double goalOrient)
        {

            // change goalOrient to startOrient frame
            double orientationDelta = constrainAngle(goalOrient - startOrient);
            if (Math.Abs(orientationDelta) < ANGLE_THRESHOLD)
                return 0.0;
            else if (orientationDelta > 0)
                return 1.0;
            else 
                return -1.0;

        }

    }
}
