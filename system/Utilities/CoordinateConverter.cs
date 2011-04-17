using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;

namespace Robocup.Utilities
{
    /// <summary>
    /// A basic ICoordinateConverter implementation.
    /// </summary>
    public class BasicCoordinateConverter : ICoordinateConverter
    {
        public static double FIELD_WIDTH = Constants.Field.WIDTH;
        public static double FIELD_HEIGHT = Constants.Field.HEIGHT;
        static double FIELD_BUFFER = ConstantsRaw.get<double>("plays", "FIELD_BUFFER");
        static double NORM_DIST = Math.Sqrt(FIELD_HEIGHT * FIELD_HEIGHT + FIELD_WIDTH * FIELD_WIDTH);
        int width, height;
        int offsetx, offsety;
        /// <summary>Creates a new coordinate converter; automatically picks a height to create the right ratio of width / height.</summary>
        /// <param name="width">The total width of the field</param>
        /// <param name="offsetx">How far to the right of pixel 0 the start of the field (x = -2.75) is.</param>
        /// <param name="offsety">How far down of pixel 0 the start of the field (y = 2) is.</param>
        public BasicCoordinateConverter(int width, int offsetx, int offsety)
        {
            this.width = width;
            this.height = (int)(width * (FIELD_HEIGHT / 2 + FIELD_BUFFER * 2) / (FIELD_WIDTH / 2+FIELD_BUFFER*2));
            this.offsetx = offsetx;
            this.offsety = offsety;
        }
        /// <summary>Creates a new coordinate converter; lets you choose both width and height, even though the ratio should be fixed</summary>
        /// <param name="width">The total width of the field</param>
        /// <param name="height">The total height of the field</param>
        /// <param name="offsetx">How far to the right of pixel 0 the start of the field (x = -2.75) is.</param>
        /// <param name="offsety">How far down of pixel 0 the start of the field (y = 2) is.</param>
        public BasicCoordinateConverter(int width, int height, int offsetx, int offsety)
        {
            this.width = width;
            this.height = height;
            this.offsetx = offsetx;
            this.offsety = offsety;
        }
        #region ICoordinateConverter Members

        public int fieldtopixelX(double x)
        {
            return (int)((x + FIELD_WIDTH/2) / FIELD_WIDTH * width + offsetx);
        }

        public int fieldtopixelY(double y)
        {
            return (int)((-y + FIELD_HEIGHT/2) / FIELD_HEIGHT * height + offsety);
        }

        public double fieldtopixelDistance(double f)
        {
            return f * Math.Sqrt(width * width + height * height) / NORM_DIST;
        }

        public double pixeltofieldDistance(double f)
        {
            return f / Math.Sqrt(width * width + height * height) * NORM_DIST;
        }

        public Vector2 fieldtopixelPoint(Vector2 p)
        {
            return new Vector2(fieldtopixelX(p.X), fieldtopixelY(p.Y));
        }

        public double pixeltofieldX(double x)
        {
            return (x - offsetx) * FIELD_WIDTH / width - FIELD_WIDTH/2;
        }

        public double pixeltofieldY(double y)
        {
            return FIELD_HEIGHT/2 - (y - offsety) * FIELD_HEIGHT / height;
        }

        public Vector2 pixeltofieldPoint(Vector2 p)
        {
            return new Vector2(pixeltofieldX(p.X), pixeltofieldY(p.Y));
        }

        #endregion
    }


    /// <summary>
    /// ICoordinateConverter implementation for ControlForm.
    /// </summary>
    public class ControlFormConverter : ICoordinateConverter {
        static double FIELD_WIDTH = Constants.Field.WIDTH;
        static double FIELD_HEIGHT = Constants.Field.HEIGHT;
        static double FIELD_BUFFER = ConstantsRaw.get<double>("plays", "FIELD_BUFFER");
        static double NORM_DIST = Math.Sqrt(FIELD_HEIGHT * FIELD_HEIGHT + FIELD_WIDTH * FIELD_WIDTH);

        int width, height;
        int offsetx, offsety;
        
         // General coord system (units: m)
        /* --------------------------------
        * |(-1.7, 2.45)    |      (1.7, 2.45)|
        * |                 |                |
        * |                TOP               |
        * |               CAM 1              |
        * |                 |                |
        * |                 |                |
        * |---------------(0, 0)-------------|
        * |                 |                |
        * |               BOTTOM             |
        * |               CAM 2              |
        * |                 |                |
        * |(-1.7, -2.45)    |    (1.7, -2.45)|
        * ------------------------------------
        */

       
        /// <summary>Creates a new coordinate converter; lets you choose both width and height, even though the ratio should be fixed</summary>
        /// <param name="width">The total width of the field</param>
        /// <param name="height">The total height of the field</param>
        /// <param name="offsetx">How far to the right of pixel 0 the start of the field (x = -2.75) is.</param>
        /// <param name="offsety">How far down of pixel 0 the start of the field (y = 2) is.</param>
        public ControlFormConverter(int width, int height, int offsetx, int offsety) {
            this.width = width;
            this.height = height;
            this.offsetx = offsetx;
            this.offsety = offsety;
        }
        #region ICoordinateConverter Members

        public int fieldtopixelX(double x) {
            return (int)((x + FIELD_WIDTH/2) / FIELD_WIDTH * width + offsetx);
        }

        public int fieldtopixelY(double y) {
            return (int)((-y + FIELD_HEIGHT/2) / FIELD_HEIGHT * height + offsety);
        }

        public double fieldtopixelDistance(double f) {
            return f * Math.Sqrt(width * width + height * height) / NORM_DIST;
        }

        public double pixeltofieldDistance(double f) {
            return f / Math.Sqrt(width * width + height * height) * NORM_DIST;
        }

        public Vector2 fieldtopixelPoint(Vector2 p) {
            return new Vector2(fieldtopixelX(p.X), fieldtopixelY(p.Y));
        }

        public double pixeltofieldX(double x) {
            return (x - offsetx) * FIELD_WIDTH / width - FIELD_WIDTH/2;
        }

        public double pixeltofieldY(double y) {
            return FIELD_HEIGHT/2 - (y - offsety) * FIELD_HEIGHT / height;
        }

        public Vector2 pixeltofieldPoint(Vector2 p) {
            return new Vector2(pixeltofieldX(p.X), pixeltofieldY(p.Y));
        }

        #endregion
    }
}
