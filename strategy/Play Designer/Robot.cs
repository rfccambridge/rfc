using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Robocup.Infrastructure;

namespace RobocupPlays
{
    class DesignerRobot : GetPointable, Clickable
    {
        private const float radius = 9;
        private DesignerRobotDefinition definition;
        /// <summary>
        /// Whether or not this robot is ours, or theirs.
        /// </summary>
        private bool ours = true;
        public bool Ours
        {
            get { return ours; }
        }
        private Vector2 center;
        public DesignerRobot(Vector2 p, bool ours)
        {
            this.ours = ours;
            center = p;

            rename(this.generateName());
        }
        private string name;
        //DesignerRobotDefinition definition = null;
        public string getName()
        {
            return name;
        }
        static int numrobots = 0;
        public string generateName()
        {
            numrobots++;
            return "robot" + numrobots;
        }
        public string getRobotDefinition()
        {
            if (definition != null)
            {
                return ((DesignerRobotDefinition)definition).getDefinition();
            }
            return "<undefined>";
        }
        public string getDefinition()
        {
            return "(robotpoint " + name + ")";
        }
        public void rename(string name)
        {
            this.name = name;
        }
        //end of ConditionObject methods and fields
        public void setDefinition(DesignerRobotDefinition def)
        {
            definition = def;
        }

        // interface Clickable methods
        public bool willClick(Vector2 p)
        {
            return ((p.X - center.X) * (p.X - center.X) + (p.Y - center.Y) * (p.Y - center.Y) <= radius * radius);
        }
        public void highlight()
        {
            c = Color.Blue;
        }
        public void unhighlight()
        {
            c = Color.Black;
        }
        //end of interface Clickable methods
        Color c = Color.Black;
        public void draw(Graphics g)
        {
            //if (center == null)
            if (center == null)
                return;
            //bool defined = isDefined();
            Color c2;
            if (ours)// && defined)
                c2 = Color.FromArgb(35, 35, 35);
            /*else if (ours && !defined)
                c2 = Color.FromArgb(100, 100, 100);*/
            /*else if (!ours && !defined)
                c2 = Color.FromArgb(255, 100, 100);*/
            else if (!ours)// && defined)
                c2 = Color.Red;
            else
            {
                c2 = Color.Yellow;
                //this is bad!
            }
            Brush myBrush = new SolidBrush(c2);
            Pen myPen = new Pen(c, 2);
            Vector2 p = MainForm.FieldPointToPixelPoint(center);
            g.FillEllipse(myBrush, p.X - radius, p.Y - radius, 2 * radius, 2 * radius);
            g.DrawEllipse(myPen, p.X - radius, p.Y - radius, 2 * radius, 2 * radius);
            myBrush.Dispose();
            myPen.Dispose();
        }
        public void translate(float x, float y)
        {
            //center.X += x;
            //center.Y += y;
            center += new Vector2(x, y);
        }
        public void translate(Vector2 p)
        {
            translate(p.X, p.Y);
        }
        public Vector2 getPoint()
        {
            return center;
        }
        public override string ToString()
        {
            return getName();
        }
    }
}
