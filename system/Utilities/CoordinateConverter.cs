using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;

namespace Robocup.Utilities
{
    /// <summary>
    /// A basic ICoordinateConverter implementation.
    /// Assumes that the screen's (0,0) is the top left of the drawable area
    /// (not true for instance, when there is a menubar).
    /// </summary>
    public class BasicCoordinateConverter : ICoordinateConverter
    {
        int width, height;
        public BasicCoordinateConverter(int width, int height)
        {
            this.width = width;
            this.height = height;
        }
        #region ICoordinateConverter Members

        public int fieldtopixelX(double x)
        {
            return (int)((x + 2.75) / 5.5 * width);
        }

        public int fieldtopixelY(double y)
        {
            return (int)((-y + 2.0) / 4.0 * height);
        }

        public double fieldtopixelDistance(double f)
        {
            return f * Math.Sqrt(width * width + height * height) / Math.Sqrt(5.5 * 5.5 + 4.0 * 4.0);
        }

        public double pixeltofieldDistance(double f)
        {
            return f / Math.Sqrt(width * width + height * height) * Math.Sqrt(5.5 * 5.5 + 4.0 * 4.0);
        }

        public Vector2 fieldtopixelPoint(Vector2 p)
        {
            return new Vector2(fieldtopixelX(p.X), fieldtopixelY(p.Y));
        }

        public double pixeltofieldX(double x)
        {
            return x * 5.5 / width - 2.75;
        }

        public double pixeltofieldY(double y)
        {
            return 2 - y * 4.0 / height;
        }

        public Vector2 pixeltofieldPoint(Vector2 p)
        {
            return new Vector2(pixeltofieldX(p.X), pixeltofieldY(p.Y));
        }

        #endregion
    }
}
