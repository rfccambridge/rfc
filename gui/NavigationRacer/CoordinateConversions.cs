using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Infrastructure;

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
        public float fieldtopixelDistance(float f)
        {
            return f * 100;
        }
        public float pixeltofieldDistance(float f)
        {
            return f / 100;
        }
        public Vector2 fieldtopixelPoint(Vector2 p)
        {
            return new Vector2(fieldtopixelX(p.X), fieldtopixelY(p.Y));
        }
        public float pixeltofieldX(float x)
        {
            return (float)((x - 300f) / 100f);
        }
        public float pixeltofieldY(float y)
        {
            return (float)((y - 250f) / -100f);
        }
        public Vector2 pixeltofieldPoint(Vector2 p)
        {
            return new Vector2(pixeltofieldX(p.X), pixeltofieldY(p.Y));
        }
        #endregion
    }
}
