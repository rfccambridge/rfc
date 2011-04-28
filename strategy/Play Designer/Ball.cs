using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Geometry;

using System.Drawing;

namespace Robocup.Plays
{
    /// <summary>
    /// A class for the ball.  It doesn't do much besides draw itself to the screen
    /// and handle some other user interactions (such as being clicked or dragged).
    /// </summary>
    [Serializable]
    class DesignerBall : PlayBall, GetPointable, Clickable
    {
        Vector2 p;
        public DesignerBall(Vector2 p)
        {
            this.p = p;
        }
        public override Vector2 getPoint()
        {
            return p;
        }
        public override Vector2 getVelocity()
        {
            return Vector2.ZERO;
        }
        public void setPosition(Vector2 p)
        {
            this.p = p;
        }
        private const double radius=.06;
        static public double Radius
        {
            get { return radius; }
        }
        public string getName()
        {
            return "ball";
        }
        public string getDefinition()
        {
            return "ball";
        }
        public void rename(string name)
        {
            throw new ApplicationException("Don't rename the ball!");
        }
        public bool isDefined()
        {
            return true;
        }
        Color c = Color.Orange;
        public Color color
        {
            get { return c; }
        }
        /*public void draw(Graphics g)
        {
            Vector2 p = getPoint();
            Brush myBrush = new SolidBrush(c);
            g.FillEllipse(myBrush, p.X - radius, p.Y - radius, 2 * radius, 2 * radius);
            myBrush.Dispose();
        }*/
        public bool willClick(Vector2 p)
        {
            return p.distanceSq(getPoint()) <= radius * radius;
        }
        public void highlight()
        {
            c = Color.Blue;
        }
        public void unhighlight()
        {
            c = Color.Orange;
        }
        public void translate(double dx, double dy)
        {
            Vector2 p = getPoint();
            setPosition(new Vector2(p.X + dx, p.Y + dy));
        }
        public void translate(Vector2 diff)
        {
            translate(diff.X, diff.Y);
        }
        public override string ToString()
        {
            return "ball";
        }

    }
}
