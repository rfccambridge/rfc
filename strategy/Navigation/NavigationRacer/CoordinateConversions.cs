using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;

namespace NavigationRacer
{
    public class CoordinateConversions : ICoordinateConverter
    {
        #region Coordinate Conversions
        public int fieldtopixelX(double x)
        {
            return (int)(300 + 100 * x);
        }
        public int fieldtopixelY(double y)
        {
            return (int)(250 - 100 * y);
        }
        public double fieldtopixelDistance(double f)
        {
            return f * 100; 
        }
        public double pixeltofieldDistance(double f)
        {
            return f / 100;
        }
        public Vector2 fieldtopixelPoint(Vector2 p)
        {
            return new Vector2(fieldtopixelX(p.X), fieldtopixelY(p.Y));
        }
        public double pixeltofieldX(double x)
        {
            return (x - 300d) / 100d;
        }
        public double pixeltofieldY(double y)
        {
            return (y - 250d) / -100d;
        }
        public Vector2 pixeltofieldPoint(Vector2 p)
        {
            return new Vector2(pixeltofieldX(p.X), pixeltofieldY(p.Y));
        }
        #endregion
    }
}
