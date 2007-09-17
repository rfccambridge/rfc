using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Robocup.Core;

namespace Robocup.Infrastructure
{
    /// <summary>
    /// A simple construct for drawing an arrow, given a start and end point, a color, and a width.
    /// </summary>
    //TODO put this back in
    public class Arrow
    {
        Vector2 startpoint, endpoint;
        public Vector2 StartPoint
        {
            get { return startpoint; }
        }
        public Vector2 EndPoint
        {
            get { return endpoint; }
        }
        Color c;
        public Color color
        {
            get { return c; }
        }
        float width;
        public float Width
        {
            get { return width; }
        }
        public Arrow(Vector2 start, Vector2 end, Color c, float width)
        {
            this.startpoint = start;
            this.endpoint = end;
            this.c = c;
            this.width = width;
        }
        /// <summary>
        /// Translates a point p by a point t, scaled by scale.
        /// </summary>
        private Vector2 translate(Vector2 p, Vector2 t, float scale)
        {
            return new Vector2(p.X + t.X * scale, p.Y + t.Y * scale);
        }
        /// <summary>
        /// Normalizes p, ie returns another point pointing in the same direction with magnitude 1.
        /// </summary>
        private Vector2 normalize(Vector2 p)
        {
            double magnitude = Math.Sqrt(p.X * p.X + p.Y * p.Y);
            if (magnitude == 0)
                return p;
            return new Vector2((float)(p.X / magnitude), (float)(p.Y / magnitude));
        }
        /// <summary>
        /// Draws this arrow straight onto a graphics object, without doing any coordinate conversions
        /// </summary>
        public void draw(Graphics g)
        {
            //if (startpoint == null || endpoint == null)
            if (startpoint == null || endpoint == null)
            {
                return;
            }
            Vector2 start = this.startpoint;
            Vector2 end = this.endpoint;
            float dx = end.X - start.X;
            float dy = end.Y - start.Y;
            Vector2 normal;
            if (dy != 0)
                normal = new Vector2(1, -(float)dx / dy);
            else
                normal = new Vector2(0, 1);
            normal = normalize(normal);
            Vector2 unitvector = normalize(new Vector2(dx, dy));
            Pen myPen = new Pen(Color.Black, (float)Math.Ceiling(width / 2));
            Brush myBrush = new SolidBrush(Color.FromArgb(150, c));
            //g.DrawLine(myPen, start, end);
            //g.DrawLine(myPen, Point.Round(new Vector2(start.X + normal.X, start.Y + normal.Y)), Point.Round(new Vector2(end.X + normal.X, end.Y + normal.Y)));

            float arrowheadwidth = width * 2.5f;


            PointF[] corners = new PointF[7];
            corners[0] = translate(start, normal, width);
            corners[1] = translate(start, normal, -width);
            corners[2] = translate(translate(end, normal, -width), unitvector, -arrowheadwidth * 3 / 2);
            corners[3] = translate(translate(end, normal, -arrowheadwidth), unitvector, -arrowheadwidth * 3 / 2);
            corners[4] = end;
            corners[5] = translate(translate(end, normal, arrowheadwidth), unitvector, -arrowheadwidth * 3 / 2);
            corners[6] = translate(translate(end, normal, width), unitvector, -arrowheadwidth * 3 / 2);


            try
            {
                g.FillPolygon(myBrush, corners);
                g.DrawPolygon(myPen, corners);
            }
            catch (Exception) { }
            //g.FillPolygon(myBrush, arrowhead);
            //g.DrawPolygon(myPen, arrowhead);
            myPen.Dispose();
            myBrush.Dispose();
        }
        /// <summary>
        /// Draws this arrow onto a graphics object, after using field->pixel conversions from the given converter.
        /// </summary>
        public void drawConvertToPixels(Graphics g, ICoordinateConverter c)
        {
            Arrow newArrow = new Arrow(c.fieldtopixelPoint(startpoint),
                c.fieldtopixelPoint(endpoint), this.color, c.fieldtopixelDistance(this.width));
            newArrow.draw(g);
        }
    }
}
