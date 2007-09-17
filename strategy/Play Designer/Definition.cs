using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Robocup.Core;

namespace Robocup.Plays
{
    
    abstract class DesignerRobotDefinition:PlayRobotDefinition
    {
        public override int getID()
        {
            return -1;
        }
        //abstract public bool isDefined();
        abstract public string getDefinition();
    }
    class ClosestDefinition : DesignerRobotDefinition
    {
        private DesignerExpression point;
        private DesignerExpression robot;
        public ClosestDefinition(DesignerExpression r, DesignerExpression fp)
        {
            point = fp;
            robot = r;
        }
        public override Vector2 getPoint()
        {
            //return ((DesignerRobot)robot.getValue(0)).getPoint();
            return ((DesignerRobot)robot.StoredValue).getPoint();
        }
        public override int getID()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override bool Ours
        {
            //get { return ((DesignerRobot)robot.getValue(0)).Ours; }
            get { return ((DesignerRobot)robot.StoredValue).Ours; }
        }
        public override string getDefinition()
        {
            return "(closest " + (Ours ? "friendly " : "enemy ") + point.ToString() + " loose)";
        }
        public void draw(Graphics g, int tick)
        {
            Vector2[] points = new Vector2[2];
            points[0] = MainForm.FieldPointToPixelPoint(((DesignerRobot)robot.getValue(tick, null)).getPoint());
            points[1] = MainForm.FieldPointToPixelPoint((Vector2)point.getValue(tick, null));
            float ddx = points[1].X - points[0].X;
            float ddy = points[1].Y - points[0].Y;
            float dx, dy;
            if (ddy == 0)
            {
                dx = 0;
                dy = 1;
            }
            else
            {
                dx = 1;
                dy = -ddx / ddy;
                float mag = (float)Math.Sqrt(dx * dx + dy * dy);
                dx /= mag;
                dy /= mag;
            }
            float scale=5;
            PointF[] vertices = new PointF[3] { (PointF)points[1], new PointF(points[0].X + dx * scale, points[0].Y + dy * scale), new PointF(points[0].X - dx * scale, points[0].Y - dy * scale) };
            Brush myBrush = new SolidBrush(Color.White);
            g.FillPolygon(myBrush, vertices);
            myBrush.Dispose();
        }
        /*public override bool isDefined()
        {
            return (point != null && point.isDefined());
        }*/
    }
}
