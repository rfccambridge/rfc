using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace RAWImageApp {
    public class TsaiPoint {
        public Label labelControl;
        private Control hostControl;
        private int index;

        private bool isDragging;

        public double wx, wy, wz; //world coords
        public int ix, iy; //image coords

        public TsaiPoint(Control _hostControl, int _index, double _wx, double _wy, double _wz, int _ix, int _iy) {

            wx = _wx;
            wy = _wy;
            wz = _wz;
            ix = _ix;
            iy = _iy;

            hostControl = _hostControl;
            index = _index;

            labelControl = new Label();

            labelControl.Enabled = false;
            labelControl.Visible = false;
            labelControl.Anchor = AnchorStyles.None;
            labelControl.Margin = new Padding(0);
            labelControl.Padding = new Padding(0);
            labelControl.BackColor = System.Drawing.Color.Lime;
            labelControl.Image = RAWImageApp.Properties.Resources.point;
            labelControl.Font = new System.Drawing.Font("Courier New", 6.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            labelControl.ForeColor = System.Drawing.Color.Black;
            labelControl.Text = index.ToString();

            labelControl.AutoSize = true;
            labelControl.Size = new System.Drawing.Size(10, 10);
            labelControl.MaximumSize = new System.Drawing.Size(15, 10);
            labelControl.MinimumSize = new System.Drawing.Size(10, 10);

            labelControl.Location = new System.Drawing.Point(ix - labelControl.Width / 2, iy - labelControl.Height / 2);

            labelControl.MouseDown += new MouseEventHandler(labelControl_MouseDown);
            labelControl.MouseMove += new MouseEventHandler(labelControl_MouseMove);
            labelControl.MouseUp += new MouseEventHandler(labelControl_MouseUp);
            
            hostControl.Controls.Add(labelControl);

            labelControl.BringToFront();
            
        }

        private void labelControl_MouseDown(object sender, MouseEventArgs e) {
            isDragging = true;
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

                if (newLeft > hostControl.Width - labelControl.Width / 2) {
                    newLeft = hostControl.Width - labelControl.Width / 2;
                } else {
                    if (newLeft < 0 - labelControl.Width / 2) {
                        newLeft = 0 - labelControl.Width / 2;
                    }
                }
                if (newTop > hostControl.Height - labelControl.Height / 2) {
                    newTop = hostControl.Height - labelControl.Height / 2;
                } else {
                    if (newTop < 0 - labelControl.Height / 2) {
                        newTop = 0 - labelControl.Height / 2;
                    }
                }

                labelControl.Left = newLeft;
                labelControl.Top = newTop;

                ix = labelControl.Left + labelControl.Width / 2;
                iy = labelControl.Top + labelControl.Height / 2;
            }
        }
        private void labelControl_MouseUp(object sender, MouseEventArgs e) {
            isDragging = false;
            //Console.WriteLine(ix.ToString() + ", " + iy.ToString());
        }
    }

public class Calibration {
    
    //set manufacturer camera parameters here:
    private struct MANUFACTURER_CAMERA_PARAMETERS {
        public const double    NCX = 1024;		/* [sel]   Number of sensor elements in camera's x direction */
        public const double    NFX = 1024;		/* [pix]   Number of pixels in frame grabber's x direction   */
        public const double    DX = 10.0;		/* [mm/sel] X dimension of camera's sensor element (in mm)    */
        public const double    DY = 10.000654;	/* [mm/sel]  Y dimension of camera's sensor element (in mm)   */
        public const double    DPX = 10.000654;	/* [mm/pix]  Effective X dimension of pixel in frame grabber  */
        public const double    DPY = 10.0;	/* [mm/pix]  Effective Y dimension of pixel in frame grabber   */
        public const double    CX = 1024 / 2;		/* [pix] Z axis intercept of camera coordinate system    */
        public const double    CY = 768 / 2;		/* [pix] Z axis intercept of camera coordinate system    */
        public const double    SX = 1.0;
    }
    
    private struct camera_parameters {
        public double    Ncx;		/* [sel]     Number of sensor elements in camera's x direction */
        public double    Nfx;		/* [pix]     Number of pixels in frame grabber's x direction   */
        public double    dx;		/* [mm/sel]  X dimension of camera's sensor element (in mm)    */
        public double    dy;		/* [mm/sel]  Y dimension of camera's sensor element (in mm)    */
        public double    dpx;		/* [mm/pix]  Effective X dimension of pixel in frame grabber   */
        public double    dpy;		/* [mm/pix]  Effective Y dimension of pixel in frame grabber   */
        public double    Cx;		/* [pix]     Z axis intercept of camera coordinate system      */
        public double    Cy;		/* [pix]     Z axis intercept of camera coordinate system      */
        public double    sx;		/* []        Scale factor to compensate for any error in dpx   */
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
       
        public double    f;		/* [mm]          */
        public double    kappa1;	/* [1/mm^2]      */
        public double    p1;		/* [1/mm]        */
        public double    p2;		/* [1/mm]        */
        public double    Tx;		/* [mm]          */
        public double    Ty;		/* [mm]          */
        public double    Tz;		/* [mm]          */
        public double    Rx;		/* [rad]	     */
        public double    Ry;		/* [rad]	     */
        public double    Rz;		/* [rad]	     */
        public double    r1;		/* []            */
        public double    r2;		/* []            */
        public double    r3;		/* []            */
        public double    r4;		/* []            */
        public double    r5;		/* []            */
        public double    r6;		/* []            */
        public double    r7;		/* []            */
        public double    r8;		/* []            */
        public double    r9;		/* []            */
    };

    
        private const int TSAI_COLS = 5;
        private const int TSAI_ROWS = 3;

        private Control hostControl;

        public TsaiPoint[] tsaiPoints;
        //public int regionLeft, regionRight, regionTop, regionBottom;

        camera_parameters cp;
        calibration_constants cc;

        public Calibration(Control _hostControl) {
            
            int i, j;
            int index;
            int wx, wy, wz;
            int ix, iy;
            int w_interval_w, w_interval_l;
            int interval_w, interval_h;
           
            const int PADDING = 100;

            /*
            regionLeft = 0;
            regionRight = Field.WIDTH;
            regionTop = 0;
            regionBottom = Field.HEIGHT;
            */

            tsaiPoints = new TsaiPoint[TSAI_ROWS * TSAI_COLS];

            hostControl = _hostControl;

            interval_w = (Field.WIDTH - 2 * PADDING) / 4;
            interval_h = (Field.HEIGHT - 2 * PADDING) / 2;

            w_interval_w = Field.WORLD_WIDTH / 4;
            w_interval_l = Field.WORLD_LENGTH / 2;
            
            
            wx = 0;
            wy = 0;
            wz = 0;


            iy = 100;
            for (i = 0; i < TSAI_ROWS; i++) {
                ix = 100;
                wx = 0;
                for (j = 0; j < TSAI_COLS; j++) {
                    index = i * TSAI_COLS + j;
                    tsaiPoints[index] = new TsaiPoint(hostControl, index, wx, wy, wz, ix, iy);
                    wx += w_interval_w;
                    ix += interval_w;
                }
                wy += w_interval_l;
                iy += interval_h;
            }
        }

        public void showTsaiPoints() {
            foreach (TsaiPoint tsaiPoint in tsaiPoints) {
                tsaiPoint.labelControl.Left = tsaiPoint.ix - tsaiPoint.labelControl.Width / 2;
                tsaiPoint.labelControl.Top = tsaiPoint.iy - tsaiPoint.labelControl.Height / 2;
                tsaiPoint.labelControl.Enabled = true;
                tsaiPoint.labelControl.Visible = true;
            }
        }

        public void hideTsaiPoints() {
            foreach (TsaiPoint tsaiPoint in tsaiPoints) {
                tsaiPoint.labelControl.Enabled = false;
                tsaiPoint.labelControl.Visible = false;
            }
        }

    public void saveTsaiPoints(string filename) {
        StreamWriter fout;

        fout = new StreamWriter(filename);

        foreach (TsaiPoint tP in tsaiPoints) {
            fout.WriteLine(String.Format("{0:G} {1:G}", tP.ix, tP.iy));
        }

        fout.Close();
    }

    public void loadTsaiPoints(string filename) {
        StreamReader fin;
        string[] line;

        fin = new StreamReader(filename);

        foreach (TsaiPoint tP in tsaiPoints) {
            line = fin.ReadLine().Split(new char[] { ' ' });

            tP.ix = Convert.ToInt32(line[0]);
            tP.iy = Convert.ToInt32(line[1]);
        }

        fin.Close();
    }

  
        /* initialize the camera parameters (cp) with the appropriate camera constants */
        public void getCalibrationConstants() {
                
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
           // ccal_fo.StartInfo.FileName = "C:\\Debug\\ccal_fo.exe";
            ccal_fo.StartInfo.FileName = "cmd.exe";
            ccal_fo.StartInfo.Arguments = "/c " + Environment.CurrentDirectory + "\\ccal_fo.exe cp.txt cd.txt";
            //ccal_fo.StartInfo.Arguments = "echo test";
            //ccal_fo.StartInfo.Arguments = "cp.txt cd.txt";
            ccal_fo.StartInfo.UseShellExecute = false;
            
            ccal_fo.StartInfo.RedirectStandardInput = true;
            ccal_fo.StartInfo.RedirectStandardOutput = true;
            ccal_fo.StartInfo.RedirectStandardError = true;

            //ccal_fo.
            
            try {
                ccal_fo.Start();
                //ccal_fo.WaitForExit();
                
            } catch {
                
               MessageBox.Show("Error starting cmd.exe with ccal_fo.exe!");
            }
            

           /* write to cp.txt - for input to cal_fo.exe */
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
                return;
            }
            
        
             double    sa, ca, sb, cb, sg, cg;

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


        }


        /***********************************************************************\
* This routine performs an inverse perspective projection to determine	*
* the position of a point in world coordinates that corresponds to a 	*
* given position in image coordinates.  To constrain the inverse	*
* projection to a single point the routine requires a Z world	 	*
* coordinate for the point in addition to the X and Y image coordinates.* 
\***********************************************************************/
public void imageCoordToWorldCoord(int Xfd, int Yfd, double zw, out double xw, out double yw) {

    double    Xd,
              Yd,
              Xu,
              Yu,
              common_denominator;

    /* convert from image to distorted sensor coordinates */
    Xd = cp.dpx * (Xfd - cp.Cx) / cp.sx;
    Yd = cp.dpy * (Yfd - cp.Cy);

    /* convert from distorted sensor to undistorted sensor plane coordinates */
    distorted_to_undistorted_sensor_coord (Xd, Yd, out Xu, out Yu);

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
private void  distorted_to_undistorted_sensor_coord (double Xd, double Yd, out double Xu, out double Yu) {

    double    distortion_factor;

    /* convert from distorted to undistorted sensor plane coordinates */
    distortion_factor = 1 + cc.kappa1 * (Xd*Xd + Yd*Yd);
    Xu = Xd * distortion_factor;
    Yu = Yd * distortion_factor;
}
    }

    

}
