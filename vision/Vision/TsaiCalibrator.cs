using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using Robocup.Utilities;
using System.Windows;
using Robocup.Core;

namespace Vision {
    public class DPoint
    {
        public double wx, wy;

        public DPoint(double _wx, double _wy)
        {
            wx = _wx;
            wy = _wy;
        }
    }

    public class TsaiPoint {
        public Label labelControl;
        public ToolTip toolTip;
        public Label lblCaption;

        private Control _hostControl;
        private int index;

        private bool isDragging;

        public double wx, wy, wz; //world coords
        public int ix, iy; //image coords
        public TsaiPoint(int _index, double _wx, double _wy, double _wz, int _ix, int _iy) {
            wx = _wx;
            wy = _wy;
            wz = _wz;
            ix = _ix;
            iy = _iy;
            index = _index;
        }

        public void CreateLabel(Control hostControl) {
        //public TsaiPoint(Control _hostControl, int _index, double _wx, double _wy, double _wz, int _ix, int _iy) {

            _hostControl = hostControl;

            labelControl = new Label();

            //labelControl.Enabled = false;
            labelControl.Visible = false;
            labelControl.Anchor = AnchorStyles.None;
            labelControl.Margin = new Padding(0);
            labelControl.Padding = new Padding(0);
            //labelControl.BackColor = System.Drawing.Color.Lime;
            labelControl.BackColor = Color.Transparent;
            labelControl.Image = Vision.Properties.Resources.point;
            labelControl.Font = new System.Drawing.Font("Courier New", 6.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            labelControl.ForeColor = System.Drawing.Color.Black;
            //labelControl.Text = index.ToString();
            toolTip = new ToolTip();
            toolTip.SetToolTip(labelControl, (this.wx - TsaiCalibrator.ORIGIN_OFFSET_X).ToString() + "," + (this.wy - TsaiCalibrator.ORIGIN_OFFSET_Y).ToString());
            toolTip.AutoPopDelay = 1000;
            toolTip.InitialDelay = 0;
            
            labelControl.Tag = "TsaiPointLabel";

            labelControl.AutoSize = true;
            labelControl.Size = new System.Drawing.Size(5, 5);
            labelControl.MaximumSize = new System.Drawing.Size(5, 5);
            labelControl.MinimumSize = new System.Drawing.Size(5, 5);

            labelControl.MouseDown += new MouseEventHandler(labelControl_MouseDown);
            labelControl.MouseMove += new MouseEventHandler(labelControl_MouseMove);
            labelControl.MouseUp += new MouseEventHandler(labelControl_MouseUp);

            placeControl();


            //caption with world coords

            /*lblCaption = new Label();
            //lblCaption.Enabled = false;
            lblCaption.Visible = false;
            lblCaption.Anchor = AnchorStyles.None;
            lblCaption.Margin = new Padding(0);
            lblCaption.Padding = new Padding(0);
            lblCaption.BackColor = System.Drawing.Color.Transparent;
            lblCaption.Font = new System.Drawing.Font("Courier New", 6.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            lblCaption.ForeColor = System.Drawing.Color.FromArgb(255, 0, 255); // pinkish

            lblCaption.Text = String.Format("({0:G}, {1:G})", wx, wy);

            lblCaption.Tag = "TsaiPointCaption";

            lblCaption.AutoSize = true;*/


            //placeCaption();

            _hostControl.Controls.Add(labelControl);
            //_hostControl.Controls.Add(lblCaption);

            labelControl.BringToFront();
           // lblCaption.BringToFront();

        }

        public void placeCaption() {
            lblCaption.Location = new System.Drawing.Point(labelControl.Left + labelControl.Width / 2 - lblCaption.Width / 2,
                labelControl.Top + labelControl.Height + 10);
        }

        public void placeControl() {
            labelControl.Location = new System.Drawing.Point(ix - labelControl.Width / 2, iy - labelControl.Height / 2);
        }


        private void labelControl_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
            }
            else if (e.Button == MouseButtons.Right)
            {
                toolTip.Show(toolTip.GetToolTip(labelControl), labelControl);
            }
        }

        private void labelControl_MouseMove(object sender, MouseEventArgs e) {
            int newLeft, newTop;

            if (isDragging == true) {
                // The control coordinates are converted into form coordinates
                // by adding the label position offset.
                // The offset where the user clicked in the control is also
                // accounted for. Otherwise, it looks like the top-left corner
                // of the label is attached to the mouse.



                newLeft = e.X + labelControl.Left - labelControl.Width / 2;
                newTop = e.Y + labelControl.Top - labelControl.Height / 2;

                //Console.WriteLine(e.X + ", " + e.Y + ";  " + labelControl.Left + ", " + labelControl.Top);

                if (newLeft > _hostControl.Width - labelControl.Width / 2) {
                    newLeft = _hostControl.Width - labelControl.Width / 2;
                } else {
                    if (newLeft < 0 - labelControl.Width / 2) {
                        newLeft = 0 - labelControl.Width / 2;
                    }
                }
                if (newTop > _hostControl.Height - labelControl.Height / 2) {
                    newTop = _hostControl.Height - labelControl.Height / 2;
                } else {
                    if (newTop < 0 - labelControl.Height / 2) {
                        newTop = 0 - labelControl.Height / 2;
                    }
                }

                labelControl.Left = newLeft;
                labelControl.Top = newTop;

                ix = labelControl.Left + labelControl.Width / 2;
                iy = labelControl.Top + labelControl.Height / 2;

                //placeCaption();
            }
        }
        private void labelControl_MouseUp(object sender, MouseEventArgs e) {
            isDragging = false;

            //Console.WriteLine(ix.ToString() + ", " + iy.ToString());
        }
    }

    public class TsaiCalibrator {

        //set manufacturer camera parameters here:
        private struct MANUFACTURER_CAMERA_PARAMETERS {
            public const double NCX = 1024;		/* [sel]   Number of sensor elements in camera's x direction */
            public const double NFX = 1024;		/* [pix]   Number of pixels in frame grabber's x direction   */
            public const double DX = 10.0;		/* [mm/sel] X dimension of camera's sensor element (in mm)    */
            public const double DY = 10.000654;	/* [mm/sel]  Y dimension of camera's sensor element (in mm)   */
            public const double DPX = 10.000654;	/* [mm/pix]  Effective X dimension of pixel in frame grabber  */
            public const double DPY = 10.0;	/* [mm/pix]  Effective Y dimension of pixel in frame grabber   */
            //public const double DPX = 4.88;	/* [mm/pix]  Effective X dimension of pixel in frame grabber  */
            //public const double DPY = 4.58;	/* [mm/pix]  Effective Y dimension of pixel in frame grabber   */
            //public const double CX = 1024 / 2;		/* [pix] Z axis intercept of camera coordinate system    */
           // public const double CY = 768 / 2;		/* [pix] Z axis intercept of camera coordinate system    */
            public static double CX;		/* [pix] Z axis intercept of camera coordinate system    */
            public static double CY;		/* [pix] Z axis intercept of camera coordinate system    */
            public const double SX = 1.0;
        }

        private struct camera_parameters {
            public double Ncx;		/* [sel]     Number of sensor elements in camera's x direction */
            public double Nfx;		/* [pix]     Number of pixels in frame grabber's x direction   */
            public double dx;		/* [mm/sel]  X dimension of camera's sensor element (in mm)    */
            public double dy;		/* [mm/sel]  Y dimension of camera's sensor element (in mm)    */
            public double dpx;		/* [mm/pix]  Effective X dimension of pixel in frame grabber   */
            public double dpy;		/* [mm/pix]  Effective Y dimension of pixel in frame grabber   */
            public double Cx;		/* [pix]     Z axis intercept of camera coordinate system      */
            public double Cy;		/* [pix]     Z axis intercept of camera coordinate system      */
            public double sx;		/* []        Scale factor to compensate for any error in dpx   */
        };


        /*******************************************************************************\
        *										*
        * Calibration data consists of the (x,y,z) world coordinates of a feature	*
        * point	(in mm) and the corresponding coordinates (Xf,Yf) (in pixels) of the	*
        * feature point in the image.  Point count is the number of points in the data	*
        * set.										*
        *										*
        *										*
        * Coplanar calibration:								*
        *										*
        * For coplanar calibration the z world coordinate of the data must be zero.	*
        * In addition, for numerical stability the coordinates of the feature points	*
        * should be placed at some arbitrary distance from the origin (0,0,0) of the	*
        * world coordinate system.  Finally, the plane containing the feature points	*
        * should not be parallel to the imaging plane.  A relative angle of 30 degrees	*
        * is recommended.								*
        *										*
        *										*
        * Noncoplanar calibration:							*
        *										*
        * For noncoplanar calibration the data must not lie in a single plane.		*
        *										*
        \*******************************************************************************/
        // REPLACED BY TSAI_POINT ARRAY
        /* private struct calibration_data {
            int         point_count;	/* [points] 	 */
        //  double[]    xw;	            /* [mm]          */
        //   double[]    yw;	            /* [mm]          */
        //    double[]    zw;	            /* [mm]          */
        //    double[]    Xf;	            /* [pix]         */
        //    double[]    Yf;	            /* [pix]         */
        //};



        /*******************************************************************************\
        *										*
        * Calibration constants are the model constants that are determined from the 	*
        * calibration data.								*
        *										*
        \*******************************************************************************/
        private struct calibration_constants {

            public double f;		/* [mm]          */
            public double kappa1;	/* [1/mm^2]      */
            public double p1;		/* [1/mm]        */
            public double p2;		/* [1/mm]        */
            public double Tx;		/* [mm]          */
            public double Ty;		/* [mm]          */
            public double Tz;		/* [mm]          */
            public double Rx;		/* [rad]	     */
            public double Ry;		/* [rad]	     */
            public double Rz;		/* [rad]	     */
            public double r1;		/* []            */
            public double r2;		/* []            */
            public double r3;		/* []            */
            public double r4;		/* []            */
            public double r5;		/* []            */
            public double r6;		/* []            */
            public double r7;		/* []            */
            public double r8;		/* []            */
            public double r9;		/* []            */
        };

        /* --------------------------------
         * |(3400, 4900)        (0, 4900) |
         * |                              |
         * |           TOP                |
         * |           CAM 1              |
         * |                              |
         * |                              |
         * |------------------------------|
         * |                              |
         * |            BOTTOM            |
         * |            CAM 2             |
         * |                              |
         * |(3400, 0)                (0,0)|
         * --------------------------------
         */

        private const string WORK_DIR = "../../resources/vision/";
        private const string DEFAULT_TSAI_POINTS_FILE = WORK_DIR + "tsai_points.txt";
        private const string DEFAULT_IMAGE_TO_WORLD_TABLE = WORK_DIR + "image_to_world_table.dat";
        
        public const double ORIGIN_OFFSET_X = 11000;
        public const double ORIGIN_OFFSET_Y = 12000;
        private static double TSAI_TOP_OFFSET_Y;

        //private Control hostControl;
        //private int _cameraID;

        public TsaiPoint[] tsaiPoints;
        public bool _tsaiPointsVisible = false;

        private System.Drawing.Size _imageSize;

        camera_parameters cp;
        calibration_constants cc;

        public DPoint[] imgToWorldLookup;

        //private frmWorld frmWorldObj;

        public System.Drawing.Size ImageSize
        {
            get { return _imageSize; }
            set { _imageSize = value; }
        }

        public static void LoadParameters() {
            MANUFACTURER_CAMERA_PARAMETERS.CX= Constants.get<double>("vision", "TSAI_ORIGIN_IMAGE_CENTER_X");
            MANUFACTURER_CAMERA_PARAMETERS.CY = Constants.get<double>("vision", "TSAI_ORIGIN_IMAGE_CENTER_Y");
            TSAI_TOP_OFFSET_Y = Constants.get<double>("vision", "TSAI_TOP_OFFSET_Y");
        }

        public TsaiCalibrator() {
            //_cameraID = cameraID;

            //frmWorldObj = new frmWorld(this);

            //tsaiPoints = new TsaiPoint[TSAI_ROWS * TSAI_COLS];
            // only when calibrating corners only
            //tsaiPoints = new TsaiPoint[TSAI_ROWS * TSAI_COLS * 4];

            // default
            _imageSize = new System.Drawing.Size(1024, 768);
            //_imageSize = new System.Drawing.Size(800, 600);

            //CreateDefaultTsaiPoints();
            LoadParameters();
        }

        public void DefaultInitSequence(Control hostControl) {
            
            // if tsaipoint file is not found, it's ok, default locations were generated in contructor
            if (File.Exists(DEFAULT_TSAI_POINTS_FILE))
            {
                LoadTsaiPoints(DEFAULT_TSAI_POINTS_FILE);
                CreateLabels(hostControl);
                getCalibrationConstants();
            }

            if (File.Exists(DEFAULT_IMAGE_TO_WORLD_TABLE))
                LoadImageToWorldTable(DEFAULT_IMAGE_TO_WORLD_TABLE);
            
            
        }
        public void DefaultEndSequence() {
            SaveFileDialog saveDlg = new SaveFileDialog();
            saveDlg.RestoreDirectory = true;
            saveDlg.InitialDirectory = WORK_DIR;

            DialogResult res;

            res = MessageBox.Show("Save Tsai Points?", "Ending session...", MessageBoxButtons.YesNo);
            if (res == DialogResult.Yes) {
                saveDlg.Title = "Save Tsai Points...";
                saveDlg.Filter = "Tsai Points (*.txt)|*.txt";
                saveDlg.FileName = Path.GetFileName(DEFAULT_TSAI_POINTS_FILE);
                saveDlg.FileOk += new System.ComponentModel.CancelEventHandler(delegate {
                    SaveTsaiPoints(saveDlg.FileName);
                });
                saveDlg.ShowDialog();
            }

            res = MessageBox.Show("Save Image to World lookup table?", "Ending session...", MessageBoxButtons.YesNo);
            if (res == DialogResult.Yes) {
                saveDlg.Title = "Save Image to World lookup table...";
                saveDlg.Filter = "Tsai Lookup Table (*.dat)|*.dat";
                saveDlg.FileName = Path.GetFileName(DEFAULT_IMAGE_TO_WORLD_TABLE);
                saveDlg.FileOk += new System.ComponentModel.CancelEventHandler(delegate {
                    SaveImageToWorldTable(saveDlg.FileName);
                });
                saveDlg.ShowDialog();
            }
        }
        /*
        public void CreateDefaultTsaiPoints()
        {
            int i, j;
            int index;
            double wx, wy, wz;
            int ix, iy;
            double w_interval_w, w_interval_l;
            int interval_w, interval_h;
            double wx_start = 0, wy_start = 0;

            const int PADDING = 100;


            interval_w = (_imageSize.Width - 2 * PADDING) / (TSAI_COLS - 1);
            interval_h = (_imageSize.Height - 2 * PADDING) / (TSAI_ROWS - 1);

            w_interval_w = -TSAIWIDTH / (TSAI_COLS - 1);
            w_interval_l = -TSAIHEIGHT / (2 * (TSAI_ROWS - 1));

            //start at - depends on camera
            switch (_cameraID)
            {
                case 1:
                    wx_start = TSAIWIDTH + ORIGIN_OFFSET_X;
                    wy_start = TSAIHEIGHT + ORIGIN_OFFSET_Y;
                    break;
                case 2:
                    wx_start = TSAIWIDTH + ORIGIN_OFFSET_X;
                    wy_start = TSAIHEIGHT / 2 + ORIGIN_OFFSET_Y;
                    break;
            }

            wz = 0;


            wy = wy_start;

            iy = 100;
            for (i = 0; i < TSAI_ROWS; i++)
            {
                ix = 100;
                wx = wx_start;
                for (j = 0; j < TSAI_COLS; j++)
                {
                    index = i * TSAI_COLS + j;
                    tsaiPoints[index] = new TsaiPoint(index, wx, wy, wz, ix, iy);
                    wx += w_interval_w;
                    ix += interval_w;
                }
                wy += w_interval_l;
                iy += interval_h;
            }
        } 
        */
#if false
        public void CreateDefaultTsaiPoints() {
            int i, j;
            int index;
            double wx, wy, wz;
            int ix, iy;
            double w_interval_w, w_interval_l;
            int interval_w, interval_h;
            double wx_start = 0, wy_start = 0;

            const int PADDING = 100;

            
            interval_w = (_imageSize.Width - 2 * PADDING) / (TSAI_COLS - 1);
            interval_h = (_imageSize.Height - 2 * PADDING) / (TSAI_ROWS - 1);

            w_interval_w = -TSAIWIDTH / (TSAI_COLS - 1);
            w_interval_l = -TSAIHEIGHT / (2 * (TSAI_ROWS - 1));

            //start at - depends on camera
            switch (_cameraID) {
                case 1:
                    wx_start = TSAIWIDTH;
                    wy_start = TSAIHEIGHT;
                    break;
                case 2:
                    wx_start = TSAIWIDTH;
                    wy_start = TSAIHEIGHT / 2;
                    break;
            }

            wz = 0;

            
            wy = wy_start;

            iy = 100;
            for (i = 0; i < TSAI_ROWS; i++) {
                ix = 100;
                wx = wx_start;
                for (j = 0; j < TSAI_COLS; j++) {
                    index = i * TSAI_COLS + j;
                    tsaiPoints[index] = new TsaiPoint(index, wx, wy, wz, ix, iy);
                    wx += w_interval_w;
                    ix += interval_w;
                }
                wy += w_interval_l;
                iy += interval_h;
            }
        } 
#endif

#if false
        public void CreateDefaultTsaiPoints()
        {
            int i, j;
            int index = 0;
            double wx, wy, wz;
            int ix, iy;
            double w_interval_w, w_interval_l;
            int interval_w, interval_h;
            double wx_start = 0, wy_start = 0;

            const int PADDING = 100;


           // int lastIndex = 0;

          

            //start at - depends on camera
            switch (_cameraID)
            {
                case 1:
                case 2:
                    interval_w = (_imageSize.Width / 2 - 2 * PADDING) / (TSAI_COLS - 1);
                    interval_h = (_imageSize.Height / 2 - 2 * PADDING) / (TSAI_ROWS - 1);

                    wx_start = TSAIWIDTH / 2;
                    wy_start = TSAIHEIGHT / 2;
                    wz = 0;

                    w_interval_w = -100;
                    w_interval_l = -100;

                    wy = wy_start;

                    //top left
                    iy = 100;
                    for (i = 0; i < TSAI_ROWS; i++)
                    {
                        ix = 100;
                        wx = wx_start;
                        for (j = 0; j < TSAI_COLS; j++)
                        {
                            tsaiPoints[index] = new TsaiPoint(index, wx, wy, wz, ix, iy);
                            index++;
                            wx += w_interval_w;
                            ix += interval_w;
                        }
                        wy += w_interval_l;
                        iy += interval_h;
                    }

                    //top right
                    wx_start = -TSAIWIDTH / 2;
                    wy_start = TSAIHEIGHT / 2;
                    wz = 0;

                    w_interval_w = 100;
                    w_interval_l = 100;

                    wy = wy_start;
                    iy = 100;
                    for (i = 0; i < TSAI_ROWS; i++)
                    {
                        ix = _imageSize.Width - 100;
                        wx = wx_start;
                        for (j = 0; j < TSAI_COLS; j++)
                        {
                            tsaiPoints[index] = new TsaiPoint(index, wx, wy, wz, ix, iy);
                            index++;
                            wx += w_interval_w;
                            ix -= interval_w;
                        }
                        wy += w_interval_l;
                        iy += interval_h;
                    }

                    //bottom left
                    wx_start = TSAIWIDTH / 2;
                    wy_start = -TSAIHEIGHT / 2;
                    wz = 0;

                    w_interval_w = -100;
                    w_interval_l = 100;

                    wy = wy_start;
                    iy = _imageSize.Height - 100;
                    for (i = 0; i < TSAI_ROWS; i++)
                    {
                        ix = 100;
                        wx = wx_start;
                        for (j = 0; j < TSAI_COLS; j++)
                        {
                            tsaiPoints[index] = new TsaiPoint(index, wx, wy, wz, ix, iy);
                            index++;
                            wx += w_interval_w;
                            ix += interval_w;
                        }
                        wy += w_interval_l;
                        iy -= interval_h;
                    }

                    //bottom right
                    wx_start = -TSAIWIDTH / 2;
                    wy_start = -TSAIHEIGHT / 2;
                    wz = 0;

                    w_interval_w = 100;
                    w_interval_l = 100;

                    wy = wy_start;
                    iy = _imageSize.Height - 100;
                    for (i = 0; i < TSAI_ROWS; i++)
                    {
                        ix = _imageSize.Width - 100;
                        wx = wx_start;
                        for (j = 0; j < TSAI_COLS; j++)
                        {
                            tsaiPoints[index] = new TsaiPoint(index, wx, wy, wz, ix, iy);
                            index++;
                            wx += w_interval_w;
                            ix -= interval_w;
                        }
                        wy += w_interval_l;
                        iy -= interval_h;
                    }

                    break;
              //  case 2:
              //      wx_start = TSAIWIDTH;
               //     wy_start = TSAIHEIGHT / 2;
               //     break;
            }




            
        }
#endif

        public void CreateLabels(Control hostControl) {

            foreach (Control control in hostControl.Controls)
                if ((string)control.Tag == "TsaiPointLabel" || (string)control.Tag == "TsaiPointCaption")
                    hostControl.Controls.Remove(control);

            foreach (TsaiPoint tP in tsaiPoints) {
                tP.CreateLabel(hostControl);              
            }
        }

        public void showTsaiPoints() {
            if (tsaiPoints == null) return;

            foreach (TsaiPoint tsaiPoint in tsaiPoints) {
                tsaiPoint.placeControl();
                tsaiPoint.labelControl.Visible = true;

                //tsaiPoint.placeCaption();
                //tsaiPoint.lblCaption.Visible = true;
            }

            _tsaiPointsVisible = true;
        }

        public void hideTsaiPoints() {
            if (tsaiPoints == null) return;
            foreach (TsaiPoint tsaiPoint in tsaiPoints) {
                tsaiPoint.labelControl.Visible = false;
                //tsaiPoint.lblCaption.Visible = false;
            }

            _tsaiPointsVisible = false;
        }

        public void ToggleTsaiPoints() {
            if (_tsaiPointsVisible)
                hideTsaiPoints();
            else
                showTsaiPoints();
        }

        public void SaveTsaiPointsDlg() {
            SaveFileDialog saveDlg = new SaveFileDialog();
            saveDlg.RestoreDirectory = true;
            saveDlg.InitialDirectory = WORK_DIR;
                saveDlg.Title = "Save Tsai Points...";
                saveDlg.Filter = "Tsai Points (*.txt)|*.txt";
                saveDlg.FileName = Path.GetFileName(DEFAULT_TSAI_POINTS_FILE);
                saveDlg.FileOk += new System.ComponentModel.CancelEventHandler(delegate
                {
                    SaveTsaiPoints(saveDlg.FileName);
                });
         }
        public void SaveTsaiPoints(string filename) {
            StreamWriter fout = new StreamWriter(filename);
            foreach (TsaiPoint tP in tsaiPoints)
                fout.WriteLine(String.Format("{0:G} {1:G} {2} {3} {4}", tP.ix, tP.iy, tP.wx, tP.wy, tP.wz));
            fout.Close();
        }
        public void SaveTsaiPoints()
        {
            SaveTsaiPoints(DEFAULT_TSAI_POINTS_FILE);
        }


        public void LoadTsaiPoints()
        {
            LoadTsaiPoints(DEFAULT_TSAI_POINTS_FILE);
        }
        public void LoadTsaiPoints(string filename) {
            string[] line;
            StreamReader fin = new StreamReader(filename);
            List<TsaiPoint> tPs = new List<TsaiPoint>();
            int k = 0;
            while (!fin.EndOfStream) {
                    line = fin.ReadLine().Split(new char[] { ' ' });
                    TsaiPoint tP = new TsaiPoint(k++, Convert.ToDouble(line[2]), Convert.ToDouble(line[3]), Convert.ToDouble(line[4]),

                        Convert.ToInt32(line[0]), Convert.ToInt32(line[1]));
                    tPs.Add(tP);
            }
            tsaiPoints = new TsaiPoint[tPs.Count];
            tPs.CopyTo(tsaiPoints);
            
            fin.Close();
        }
        public void AppendTsaiPoints(List<Pair<System.Drawing.Point, DPoint>> points)
        {
            int k = (tsaiPoints == null ? 0 : tsaiPoints.Length);
            TsaiPoint[] newTsaiPoints = new TsaiPoint[k + points.Count];
            
            if (k > 0)
                tsaiPoints.CopyTo(newTsaiPoints, 0);
            foreach (Pair<System.Drawing.Point, DPoint> pt in points)
            {
                newTsaiPoints[k++] = new TsaiPoint(k, pt.Second.wx, pt.Second.wy, 0, pt.First.X, pt.First.Y);
            }
            ClearTsaiPoints();
            tsaiPoints = newTsaiPoints;
        }

        public void ClearTsaiPoints()
        {
            if (tsaiPoints == null) return;
            foreach (TsaiPoint tp in tsaiPoints)
                if (tp.labelControl != null)
                    tp.labelControl.Dispose();
            tsaiPoints = null;
        }


        /* initialize the camera parameters (cp) with the appropriate camera constants */
        public int getCalibrationConstants() {

            /* cp: hardware specific params */
            cp.Ncx = MANUFACTURER_CAMERA_PARAMETERS.NCX;
            cp.Nfx = MANUFACTURER_CAMERA_PARAMETERS.NFX;
            cp.dx = MANUFACTURER_CAMERA_PARAMETERS.DX;
            cp.dy = MANUFACTURER_CAMERA_PARAMETERS.DY;
            cp.dpx = MANUFACTURER_CAMERA_PARAMETERS.DPX;
            cp.dpy = MANUFACTURER_CAMERA_PARAMETERS.DPY;
            cp.Cx = MANUFACTURER_CAMERA_PARAMETERS.CX;
            cp.Cy = MANUFACTURER_CAMERA_PARAMETERS.CY;
            cp.sx = MANUFACTURER_CAMERA_PARAMETERS.SX;



            /* cc: calibration constants from calibration data (cal_fo.exe) */


            Process ccal_fo = new Process();
            ccal_fo.StartInfo.FileName = "cmd.exe";
            ccal_fo.StartInfo.Arguments = "/c \"" + Path.GetFullPath(WORK_DIR) + "ccal_fo.exe\"";
            ccal_fo.StartInfo.UseShellExecute = false;

            ccal_fo.StartInfo.RedirectStandardInput = true;
            ccal_fo.StartInfo.RedirectStandardOutput = true;
            ccal_fo.StartInfo.RedirectStandardError = true;


            try {
                ccal_fo.Start();
            } catch {
                MessageBox.Show("Error starting cmd.exe with ccal_fo.exe!");
                return 1;
            }


            /* ccal_fo.exe reads input data from the standard input */
            ccal_fo.StandardInput.WriteLine(String.Format("{0:E}", cp.Ncx));
            ccal_fo.StandardInput.WriteLine(String.Format("{0:E}", cp.Nfx));
            ccal_fo.StandardInput.WriteLine(String.Format("{0:E}", cp.dx));
            ccal_fo.StandardInput.WriteLine(String.Format("{0:E}", cp.dy));
            ccal_fo.StandardInput.WriteLine(String.Format("{0:E}", cp.dpx));
            ccal_fo.StandardInput.WriteLine(String.Format("{0:E}", cp.dpy));
            ccal_fo.StandardInput.WriteLine(String.Format("{0:E}", cp.Cx));
            ccal_fo.StandardInput.WriteLine(String.Format("{0:E}", cp.Cy));
            ccal_fo.StandardInput.WriteLine(String.Format("{0:E}", cp.sx));



            foreach (TsaiPoint tsaiPoint in tsaiPoints) {
                ccal_fo.StandardInput.WriteLine(String.Format("{0:E} {1:E} {2:E} {3:E} {4:E}",
                    tsaiPoint.wx, tsaiPoint.wy, tsaiPoint.wz, (double)tsaiPoint.ix, (double)tsaiPoint.iy));
            }
            ccal_fo.StandardInput.Close();

            if (ccal_fo.StandardOutput.EndOfStream == true) {
                MessageBox.Show("Failed to read data from ccal_fo.exe! Try again.\r\n" + ccal_fo.StandardError.ReadToEnd());
                ccal_fo.Close();
                return 1;
            }

            double sa, ca, sb, cb, sg, cg;

            cp.Ncx = Convert.ToDouble(ccal_fo.StandardOutput.ReadLine());
            cp.Nfx = Convert.ToDouble(ccal_fo.StandardOutput.ReadLine());
            cp.dx = Convert.ToDouble(ccal_fo.StandardOutput.ReadLine());
            cp.dy = Convert.ToDouble(ccal_fo.StandardOutput.ReadLine());
            cp.dpx = Convert.ToDouble(ccal_fo.StandardOutput.ReadLine());
            cp.dpy = Convert.ToDouble(ccal_fo.StandardOutput.ReadLine());
            cp.Cx = Convert.ToDouble(ccal_fo.StandardOutput.ReadLine());
            cp.Cy = Convert.ToDouble(ccal_fo.StandardOutput.ReadLine());
            cp.sx = Convert.ToDouble(ccal_fo.StandardOutput.ReadLine());

            cc.f = Convert.ToDouble(ccal_fo.StandardOutput.ReadLine());
            cc.kappa1 = Convert.ToDouble(ccal_fo.StandardOutput.ReadLine());
            cc.Tx = Convert.ToDouble(ccal_fo.StandardOutput.ReadLine());
            cc.Ty = Convert.ToDouble(ccal_fo.StandardOutput.ReadLine());
            cc.Tz = Convert.ToDouble(ccal_fo.StandardOutput.ReadLine());
            cc.Rx = Convert.ToDouble(ccal_fo.StandardOutput.ReadLine());
            cc.Ry = Convert.ToDouble(ccal_fo.StandardOutput.ReadLine());
            cc.Rz = Convert.ToDouble(ccal_fo.StandardOutput.ReadLine());

            sa = Math.Sin(cc.Rx);
            ca = Math.Cos(cc.Rx);
            sb = Math.Sin(cc.Ry);
            cb = Math.Cos(cc.Ry);
            sg = Math.Sin(cc.Rz);
            cg = Math.Cos(cc.Rz);

            cc.r1 = cb * cg;
            cc.r2 = cg * sa * sb - ca * sg;
            cc.r3 = sa * sg + ca * cg * sb;
            cc.r4 = cb * sg;
            cc.r5 = sa * sb * sg + ca * cg;
            cc.r6 = ca * sb * sg - cg * sa;
            cc.r7 = -sb;
            cc.r8 = cb * sa;
            cc.r9 = ca * cb;

            cc.p1 = Convert.ToDouble(ccal_fo.StandardOutput.ReadLine());
            cc.p2 = Convert.ToDouble(ccal_fo.StandardOutput.ReadLine());


            ccal_fo.Close();

            return 0;


        }

        public void ImageCoordToWorldCoord(int x, int y, double wz, out double xw, out double yw)
        {
            double worldX, worldY;
            imageCoordToWorldCoord(x, y, wz, out worldX, out worldY);
            //xw = worldX;
            //yw = worldY;
            TranslateCoords(worldX, worldY, out xw, out yw);
        }

        /***********************************************************************\
* This routine performs an inverse perspective projection to determine	*
* the position of a point in world coordinates that corresponds to a 	*
* given position in image coordinates.  To constrain the inverse	*
* projection to a single point the routine requires a Z world	 	*
* coordinate for the point in addition to the X and Y image coordinates.* 
\***********************************************************************/
        private void imageCoordToWorldCoord(int Xfd, int Yfd, double zw, out double xw, out double yw) {

            double Xd,
                      Yd,
                      Xu,
                      Yu,
                      common_denominator;

            /* convert from image to distorted sensor coordinates */
            Xd = cp.dpx * (Xfd - cp.Cx) / cp.sx;
            Yd = cp.dpy * (Yfd - cp.Cy);

            /* convert from distorted sensor to undistorted sensor plane coordinates */
            distorted_to_undistorted_sensor_coord(Xd, Yd, out Xu, out Yu);

            /* calculate the corresponding xw and yw world coordinates	 */
            /* (these equations were derived by simply inverting	 */
            /* the perspective projection equations using Macsyma)	 */
            common_denominator = ((cc.r1 * cc.r8 - cc.r2 * cc.r7) * Yu +
                      (cc.r5 * cc.r7 - cc.r4 * cc.r8) * Xu -
                      cc.f * cc.r1 * cc.r5 + cc.f * cc.r2 * cc.r4);

            xw = (((cc.r2 * cc.r9 - cc.r3 * cc.r8) * Yu +
                (cc.r6 * cc.r8 - cc.r5 * cc.r9) * Xu -
                cc.f * cc.r2 * cc.r6 + cc.f * cc.r3 * cc.r5) * zw +
               (cc.r2 * cc.Tz - cc.r8 * cc.Tx) * Yu +
               (cc.r8 * cc.Ty - cc.r5 * cc.Tz) * Xu -
               cc.f * cc.r2 * cc.Ty + cc.f * cc.r5 * cc.Tx) / common_denominator;

            yw = -(((cc.r1 * cc.r9 - cc.r3 * cc.r7) * Yu +
                 (cc.r6 * cc.r7 - cc.r4 * cc.r9) * Xu -
                 cc.f * cc.r1 * cc.r6 + cc.f * cc.r3 * cc.r4) * zw +
                (cc.r1 * cc.Tz - cc.r7 * cc.Tx) * Yu +
                (cc.r7 * cc.Ty - cc.r4 * cc.Tz) * Xu -
                cc.f * cc.r1 * cc.Ty + cc.f * cc.r4 * cc.Tx) / common_denominator;


        }

        /************************************************************************/
        private void distorted_to_undistorted_sensor_coord(double Xd, double Yd, out double Xu, out double Yu) {

            double distortion_factor;

            /* convert from distorted to undistorted sensor plane coordinates */
            distortion_factor = 1 + cc.kappa1 * (Xd * Xd + Yd * Yd);
            Xu = Xd * distortion_factor;
            Yu = Yd * distortion_factor;
        }

        public void GenerateImageToWorldTable() {
            int k;
            int ix, iy;
            double wx, wy;
            //double wxt, wyt;
            int error = getCalibrationConstants();

            if (error != 0) {
                MessageBox.Show("Failed to generate Image to World Lookup Table!");
                return;
            }

            float ROBOT_Height_TSAI = Constants.get<float>("vision", "ROBOT_HEIGHT_TSAI");

            imgToWorldLookup = new DPoint[_imageSize.Width * _imageSize.Height];
            
            k = 0;
            for (iy = 0; iy < _imageSize.Height; iy++)
            {
                for (ix = 0; ix < _imageSize.Width; ix++)
                {
                    ImageCoordToWorldCoord(ix, iy, ROBOT_Height_TSAI, out wx, out wy);
                    imgToWorldLookup[k++] = new DPoint(wx, wy);
                }
            }    
        }

        //this is if we use the corner calibration
     /*   private DPoint TranslateCoords(double wx, double wy) {
            return new DPoint(wx + TSAIWIDTH / 2, wy + TSAIHEIGHT / 2);
        }*/
        private void TranslateCoords(double wx, double wy, out double wxt, out double wyt)
        {
            wxt = wx - ORIGIN_OFFSET_X;
            wyt = wy - ORIGIN_OFFSET_Y + TSAI_TOP_OFFSET_Y;
        }


        public void LoadImageToWorldTable(string filename) {
            imgToWorldLookup = new DPoint[_imageSize.Width * _imageSize.Height];

            int k = 0;
            int ix, iy;
            double wx, wy;
            FileStream fStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            BinaryReader binReader = new BinaryReader(fStream);

            for (iy = 0; iy < _imageSize.Height; iy++)
            {
                for (ix = 0; ix < _imageSize.Width; ix++)
                {
                    wx = binReader.ReadDouble();
                    wy = binReader.ReadDouble();
                    imgToWorldLookup[k++] = new DPoint(wx, wy);
                }
            }    

            binReader.Close();
            fStream.Close();            
        }

        public void SaveImageToWorldTable(string filename) {
            int ix, iy;
            FileStream fStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
            BinaryWriter binWriter = new BinaryWriter(fStream);
            DPoint point;

            for (iy = 0; iy < _imageSize.Height; iy++)
            {
                for (ix = 0; ix < _imageSize.Width; ix++)
                {
                    point = imgToWorldLookup[iy * _imageSize.Width + ix];
                    binWriter.Write(point.wx);
                    binWriter.Write(point.wy);
                }
            }

            binWriter.Close();
            fStream.Close();
        }

        public float GetAreaScalingCoeff(int x, int y)
        {
            int ix = x;
            int iy = y;

            if (x == 0)
                ix = 1;
            if (y == 0)
                iy = 1;
            if (x == _imageSize.Width - 1)
                ix = x - 1;
            if (y == _imageSize.Height - 1)
                iy = y - 1;
            
            DPoint w1 = imgToWorldLookup[(iy - 1) * _imageSize.Width + (ix - 1)];
            DPoint w2 = imgToWorldLookup[(iy - 1) * _imageSize.Width + (ix + 1)];
            DPoint w3 = imgToWorldLookup[(iy + 1) * _imageSize.Width + (ix - 1)];
    

            Vector v1 = new Vector(w3.wx - w1.wx, w3.wy - w1.wy);
            Vector v2 = new Vector(w2.wx - w1.wx, w2.wy - w1.wy);
            
            double area2x2 = Math.Abs(v2.X * v1.Y - v1.X * v2.Y);

            return (float)(area2x2 / 4);
        }

    }



}
