using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using VisionStatic;
using System.Drawing.Drawing2D;

namespace Vision {

    public enum Mode { DEFAULT, SEL_REGION_LEFT, SEL_REGION_RIGHT, SEL_REGION_TOP, SEL_REGION_BOTTOM };

    public partial class frmCameraImage : Form {
        public frmVision parentForm;
        public Mode mode;

        public RAWImage rawImage;
        public RAWImage zoomedImage;

        public RAWImage subjectImage;
        
        private Bitmap _bitmap;
        //private bool _colorClassView = false;

        public frmCameraImage() {
            InitializeComponent();

            picImage.Width = VisionStatic.Field.WIDTH;
            picImage.Height = VisionStatic.Field.HEIGHT;

            
           
        }

        /* Graphics object for drawing on the image */

        public Graphics createGraphics() {
            return Graphics.FromImage(_bitmap);
        }
        
        /* END: Graphics object */

       /* public void beginSelectRegion(RAWImage _subjectImage) {
            subjectImage = _subjectImage;

            mode = Mode.SEL_REGION_LEFT;
            parentForm.lblMode.Text = mode.ToString();
        }
        public void endSelectRegion() {

            mode = Mode.DEFAULT;
            parentForm.lblMode.Text = mode.ToString();
            parentForm.chkSelRegionForCalib.Checked = false;
        }*/


        private void picImage_MouseClick(object sender, MouseEventArgs e) {

     /*       int x, y;
            double wx, wy;

            switch (mode) {
                case Mode.SEL_REGION_LEFT:

                    subjectImage.region_left = e.X;
                    subjectImage.crop();
                    
                    _bitmap = subjectImage.toBitmap();
                    updateImage();

                    mode = Mode.SEL_REGION_RIGHT;

                    break;
                case Mode.SEL_REGION_RIGHT:
                    subjectImage.region_right = e.X;

                    subjectImage.crop();
                    _bitmap = subjectImage.toBitmap();
                    updateImage();

                    mode = Mode.SEL_REGION_TOP;
                    break;
                case Mode.SEL_REGION_TOP:
                    subjectImage.region_top = e.Y;

                    subjectImage.crop();
                    _bitmap = subjectImage.toBitmap();
                    updateImage();

                    mode = Mode.SEL_REGION_BOTTOM;
                    break;
                case Mode.SEL_REGION_BOTTOM:
                    subjectImage.region_bottom = e.Y;

                    subjectImage.crop();

                    _bitmap = subjectImage.toBitmap();
                    updateImage();

                    endSelectRegion();

                    break;
                default:
                    if (zoomedImage != null) {

                        parentForm.lblSelBlobByCol.BackColor = Color.FromArgb(
                            zoomedImage.ImgBitmap.GetPixel(e.X, e.Y).R,
                            zoomedImage.ImgBitmap.GetPixel(e.X, e.Y).G,
                            zoomedImage.ImgBitmap.GetPixel(e.X, e.Y).B);
                        int h, s, v;

                        parentForm._colorCalibrator.RGBtoHSV(
                            zoomedImage.ImgBitmap.GetPixel(e.X, e.Y).R,
                            zoomedImage.ImgBitmap.GetPixel(e.X, e.Y).G,
                            zoomedImage.ImgBitmap.GetPixel(e.X, e.Y).B,
                            out h,
                            out s,
                            out v);

                        parentForm.txtH.Text = ""+h;
                        parentForm.txtS.Text = ""+s;
                        parentForm.txtV.Text = ""+v;

                        //Added by rishi 2/17
                        int vslice = (int) (v / ColorCalibration.V_SLICE_DEPTH);
                        foreach(frmHSVSlice hsvS in parentForm._colorCalibrator.slicesForms)
                            hsvS.Hide();
                        frmHSVSlice hsvSlice = parentForm._colorCalibrator.slicesForms[vslice];
                        hsvSlice.Show();
                        hsvSlice.BringToFront();
                        hsvSlice.currPixelBox.Show();
                        hsvSlice.currPixelBox.SetBounds(h-3,s-3,6,6);
                        hsvSlice.txtH.Text = ""+h;
                        hsvSlice.txtS.Text = ""+s;
                        hsvSlice.txtV.Text = ""+v;


                        x = e.X / zoomedImage.zoomFactor;
                        y = e.Y / zoomedImage.zoomFactor;
                        parentForm.txtSelBlobByCenterX.Text = x.ToString();
                        parentForm.txtSelBlobByCenterY.Text = y.ToString();
                        parentForm._tsaiCalibrator.imageCoordToWorldCoord(x, y, 0, out wx, out wy);
                        parentForm.txtWorldX.Text = wx.ToString();
                        parentForm.txtWorldY.Text = wy.ToString();
                    }
                    break;
            }
            parentForm.lblMode.Text = mode.ToString();
      * */
        }


        private void frmCameraImage_KeyPress(object sender, KeyPressEventArgs e) {
           /* switch (e.KeyChar) {
                case '=': // zoomIn
                    zoomedImage = rawImage.zoom(++zoomedImage.zoomFactor);
                    break;
                case '-': // zoomOut
                    if (zoomedImage.zoomFactor > 1) {
                        zoomedImage = rawImage.zoom(--zoomedImage.zoomFactor);
                    }
                    break;
                case 'c': // toggle between original view and color class view
                    if (_colorClassView)
                        rawImage = parentForm.rawOrigImage;
                    else
                        rawImage = parentForm.rawOrigImage.toColorClass(parentForm._colorCalibrator);

                    _colorClassView = !_colorClassView;

                    zoomedImage = rawImage.zoom(zoomedImage.zoomFactor);
                    break;
                case 'r':
                    /*slbRegion.Visible = true;
                    slbRegion.BringToFront();
                    slbRegion.Refresh();*
                    break;

            }

            _bitmap = zoomedImage.toBitmap();
            updateImage();*/
        }

        public void updateImage() {
            //RAWImage temp = null;
        
   
           //zoomedImage.showInPictureBox(picImage);
           // _bitmap = zoomedImage.toBitmap();

            picImage.Width = _bitmap.Width;
            picImage.Height = _bitmap.Height;
            picImage.BackgroundImage = _bitmap;
            picImage.Refresh();

            
        }

        public void loadImage(RAWImage _rawImage) {
            rawImage = _rawImage;
            zoomedImage = _rawImage.zoom(1);

            _bitmap = zoomedImage.toBitmap();

            updateImage();
           
        }

       /* private void frmCameraImage_Activated(object sender, EventArgs e)
        {
            if (objGraphics == null)
                return;

            objGraphics.Restore(objGraphicsState);
            updateGraphicsState();

            Console.WriteLine("Paint event called!");
        }
        */
          
       private void picImage_Paint(object sender, PaintEventArgs e) {


            //Console.WriteLine("Drawing balls!");
            //VisionTest.displayLostBalls(objGraphics);

            picImage.BackgroundImage = _bitmap;
            
        }


    }
}
