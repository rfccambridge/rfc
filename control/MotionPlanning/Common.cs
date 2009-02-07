using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Core;
using Robocup.CoreRobotics;

using System.Drawing;

namespace Robocup.MotionControl
{
    public enum ExtendResultType
    {
        Success, Blocked, Destination
    }
    public class ExtendResults<T>
    {
        public T extension;
        public ExtendResultType resultType;
        public ExtendResults(T extension, ExtendResultType type)
        {
            this.extension = extension;
            this.resultType = type;
        }
    }
    //TODO shouldn't take an Object as state
    public delegate ExtendResults<T1> Extender<T1, T2>(T1 start, T2 end, object state);


    /// <summary>
    /// A simple class to represent obstacles, it gives a useful way to
    /// treat robots and the ball as similar things.  Also, setting the
    /// avoid-radius can be done once at the beginning and stored in the
    /// obstacle.
    /// </summary>
    //yes it should probably be a struct, but then you can't return null when you ask for the blocking obstacle
    public class Obstacle
    {
        public Obstacle(Vector2 pos, double size)
        {
            this.position = pos;
            this.size = size;
        }
        public Vector2 position;
        public double size;
    }
    static public class Common
    {
        const double extendDistance = .05;
        static public ExtendResults<Vector2> ExtendVV(Vector2 start, Vector2 end, object state)
        {
            List<Obstacle> obstacles = (List<Obstacle>)state;
            if (start.distanceSq(end) < extendDistance * extendDistance)
                return new ExtendResults<Vector2>(end, ExtendResultType.Destination);
            Vector2 next = (end - start).normalizeToLength(extendDistance) + start;
            if (Blocked(next, obstacles))
                return new ExtendResults<Vector2>(next, ExtendResultType.Blocked);
            return new ExtendResults<Vector2>(next, ExtendResultType.Success);
        }
        static public ExtendResults<Vector2> ExtendVR(Vector2 start, RobotInfo end, object state)
        {
            return ExtendVV(start, end.Position, state);
        }

        //TODO separate movementmodeler for each robot
        static readonly MovementModeler mm = new MovementModeler();
        const double extendTime = .5;
        static public ExtendResults<RobotInfo> ExtendRRThrough(RobotInfo start, RobotInfo end, object state)
        {
            List<Obstacle> obstacles = (List<Obstacle>)state;
            ExtendResultType result = ExtendResultType.Success;
            WheelSpeeds ws = WheelSpeedsExtender.GetWheelSpeedsThrough(start, end);
            RobotInfo newInfo = mm.ModelWheelSpeeds(start, ws, extendTime);
            if (end.Position.distanceSq(start.Position) < .1 * .1)
            {
                //newInfo = destination;
                result = ExtendResultType.Destination;
            }
            if (Blocked(newInfo.Position, obstacles))
                result = ExtendResultType.Blocked;
            return new ExtendResults<RobotInfo>(newInfo, result);
        }
        static public ExtendResults<RobotInfo> ExtendRVThrough(RobotInfo start, Vector2 end, object state)
        {
            List<Obstacle> obstacles = (List<Obstacle>)state;
            ExtendResultType result = ExtendResultType.Success;
            WheelSpeeds ws = WheelSpeedsExtender.GetWheelSpeedsThrough(start, end);
            RobotInfo newInfo = mm.ModelWheelSpeeds(start, ws, extendTime);
            if (end.distanceSq(start.Position) < .1 * .1)
            {
                //newInfo = destination;
                result = ExtendResultType.Destination;
            }
            if (Blocked(newInfo.Position, obstacles))
                result = ExtendResultType.Blocked;
            return new ExtendResults<RobotInfo>(newInfo, result);
        }

        static public bool Blocked(Vector2 point, List<Obstacle> obstacles)
        {
            foreach (Obstacle o in obstacles)
            {
                
                if (o.position.distanceSq(point) < o.size * o.size)
                    return true;
            }
            return false;
        }

        static readonly Random r = new Random();
        static public Vector2 RandomStateV()
        {
            return new Vector2((r.NextDouble() - .5) * 2 * 2.75, (r.NextDouble() - .5) * 2 * 2);
        }
        static public RobotInfo RandomStateR()
        {
            Vector2 position = new Vector2((r.NextDouble() - .5) * 2 * 2.75, (r.NextDouble() - .5) * 2 * 2);
            return new RobotInfo(position, 0, -1);
        }

        static public void DrawVector2Tree(Vector2Tree tree, Color color, Graphics g, ICoordinateConverter c)
        {
            if (tree == null)
                return;
            Brush b = new SolidBrush(color);

            foreach (Vector2 v in tree.AllNodes())
            {
                g.FillRectangle(b, c.fieldtopixelX(v.X) - 1, c.fieldtopixelY(v.Y) - 1, 2, 2);
            }

            b.Dispose();
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
        // hack to add in orientation information: add PID here
        static public WheelSpeeds addOrientation(double startOrientation, double goalOrientation, WheelSpeeds speeds)
        {
            const double turnSpeed = 3.0;
            // we need orientation and speed information 
            int turnIncrement = (int)(turnSpeed * getTurnDirection(startOrientation, goalOrientation));
            //Console.WriteLine("TURNINCREMENT: " + turnIncrement);

            speeds.lf += -1 * turnIncrement;
            speeds.rf += turnIncrement;
            speeds.lb += -1 * turnIncrement;
            speeds.rb += turnIncrement;

            return speeds;
        }
        static public void DrawRobotInfoTree(RobotInfoTree tree, Color color, Graphics g, ICoordinateConverter c)
        {
            if (tree == null)
                return;
            Brush b = new SolidBrush(color);
            Pen p = new Pen(Color.Black);

            foreach (RobotInfo info in tree.AllNodes())
            {
                Vector2 v = info.Position;
                g.FillRectangle(b, c.fieldtopixelX(v.X) - 1, c.fieldtopixelY(v.Y) - 1, 3, 3);
                RobotInfo prev = tree.ParentNode(info);
                if (prev != null)
                {
                    Vector2 v2 = prev.Position;
                    g.DrawLine(p, (float)c.fieldtopixelX(v.X), (float)c.fieldtopixelY(v.Y),
                        (float)c.fieldtopixelX(v2.X), (float)c.fieldtopixelY(v2.Y));
                }
            }

            p.Dispose();
            b.Dispose();
        }

        static public void DrawPath(RobotPath path, Color color1, Color color2, Graphics g, ICoordinateConverter c)
        {
            Pair<List<RobotInfo>, List<Vector2>> pairPath = new Pair<List<RobotInfo>,List<Vector2>>(path.Waypoints, new List<Vector2>());            
            DrawPath(pairPath, color1, color2, g, c);
        }

        static public void DrawPath(Pair<List<RobotInfo>, List<Vector2>> path, Color color1, Color color2, Graphics g, ICoordinateConverter c) {
            if (path == null)
                return;
            Brush b = new SolidBrush(color1);
            Pen p = new Pen(Color.Black);

            RobotInfo prev = null;
            foreach (RobotInfo info in path.First) {
                Vector2 v = info.Position;
                if (!(double.IsNaN(v.X) || double.IsNaN(v.Y))) {
                    g.FillRectangle(b, c.fieldtopixelX(v.X) - 1, c.fieldtopixelY(v.Y) - 1, 3, 3);

                    if (prev != null) {
                        Vector2 v2 = prev.Position;
                        g.DrawLine(p, (float)c.fieldtopixelX(v.X), (float)c.fieldtopixelY(v.Y),
                            (float)c.fieldtopixelX(v2.X), (float)c.fieldtopixelY(v2.Y));
                    }

                    prev = info;
                }
            }

            Brush b2 = new SolidBrush(color2);
            Vector2 prevVector = null;
            foreach (Vector2 v in path.Second) {
                if (!(double.IsNaN(v.X) || double.IsNaN(v.Y))) {
                    g.FillRectangle(b2, c.fieldtopixelX(v.X) - 1, c.fieldtopixelY(v.Y) - 1, 3, 3);

                    if (prevVector != null) {
                        g.DrawLine(p, (float)c.fieldtopixelX(v.X), (float)c.fieldtopixelY(v.Y),
                            (float)c.fieldtopixelX(prevVector.X), (float)c.fieldtopixelY(prevVector.Y));
                    }

                    prevVector = v;
                }
            }

            p.Dispose();
            b.Dispose();
        }
    }
}
