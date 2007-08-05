using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using VisionStatic;

namespace Vision {
    public partial class frmRealTimeImage : Form {

        public RAWImage rawImage;
        public RAWImage zoomedImage;
        VisionCamera.Camera camera;
        Boolean started;
        //MultiSampleCodeTimer timer;

        public ColorCalibration colorCalibObj;
        public TsaiCalibration tsaiCalibObj;

        frmGameObjects frmGameObjectsObj;

        BlobWork blobWorkObj;

        public frmVision parentForm;

        public int region_left, region_right, region_top, region_bottom;
        private Mode mode;
        private MouseEventHandler picImage_Click_Handler;

        DateTime time1, time2;
        int numPasses;
        TimeSpan elapsed;


        public frmRealTimeImage(ColorCalibration _colorCalibObj, TsaiCalibration _tsaiCalibObj, frmVision _parentForm, frmGameObjects _frmGameObjectsObj) {
            InitializeComponent();

            parentForm = _parentForm;
            colorCalibObj = _colorCalibObj;
            tsaiCalibObj = _tsaiCalibObj;

            frmGameObjectsObj = _frmGameObjectsObj;

            mode = Mode.DEFAULT;
            parentForm.lblMode.Text = mode.ToString();

            picImage_Click_Handler = new MouseEventHandler(picImage_MouseClick);

            picImage.Width = VisionStatic.Field.WIDTH;
            picImage.Height = VisionStatic.Field.HEIGHT;

            region_left = 0;
            region_right = VisionStatic.Field.WIDTH;
            region_top = 0;
            region_bottom = VisionStatic.Field.HEIGHT;

            camera = new VisionCamera.Camera();
            started = false;
            //timer = new MultiSampleCodeTimer(1,1);

           

            blobWorkObj = new BlobWork(colorCalibObj, tsaiCalibObj);


            picImage.MouseClick += picImage_Click_Handler;
            mode = Mode.DEFAULT;
           
        }

        public void beginSelectRegion()
        {
            mode = Mode.SEL_REGION_LEFT;
            parentForm.lblMode.Text = mode.ToString();
        }
        public void endSelectRegion()
        {
            /*region_left = 0;
            region_right = rawImage.Width;
            region_top = 0;
            region_bottom = rawImage.Height */

            mode = Mode.DEFAULT;
            parentForm.lblMode.Text = mode.ToString();
            parentForm.chkSelectRegionMode.Checked = false;
        }


        private void picImage_MouseClick(object sender, MouseEventArgs e) {

            if (mode == Mode.DEFAULT) {
                return;
            }

            switch (mode) {
                case Mode.SEL_REGION_LEFT:
                    
                    rawImage.region_left = e.X;
                    rawImage.crop();
                    rawImage.showInPictureBox(picImage);

                    mode = Mode.SEL_REGION_RIGHT;
                    
                    break;
                case Mode.SEL_REGION_RIGHT:
                    rawImage.region_right = e.X;
                    
                    rawImage.crop();
                    rawImage.showInPictureBox(picImage);

                    mode = Mode.SEL_REGION_TOP;
                    break;
                case Mode.SEL_REGION_TOP:
                    rawImage.region_top = e.Y;
                    
                    rawImage.crop();
                    rawImage.showInPictureBox(picImage);

                    mode = Mode.SEL_REGION_BOTTOM;
                    break;
                case Mode.SEL_REGION_BOTTOM:
                    rawImage.region_bottom = e.Y;

                    rawImage.crop();
                    rawImage.showInPictureBox(picImage);

                    endSelectRegion();

                    //parentForm.chkSelectRegionMode.Checked = false;
                    //zoomedImage = rawImage.zoom(1);

                    region_left = rawImage.region_left;
                    region_right = rawImage.region_right;
                    region_top = rawImage.region_top;
                    region_bottom = rawImage.region_bottom;

                    break;               
            }
            parentForm.lblMode.Text = mode.ToString();
        }

        public void frmCameraImage_KeyPress(object sender, KeyPressEventArgs e) {
            switch (e.KeyChar) {
                case 'a':
                    Console.WriteLine("keyboard a pressed, started camera");

                    picImage.MouseClick -= picImage_Click_Handler;
                    mode = Mode.DEFAULT;

                    camera.startCapture();
                    started = true;

                    numPasses = 0;
                    time1 = DateTime.Now;


                    break;
                case 's':
                    Console.WriteLine("keyboard s pressed, stopping camera");

                    if (started == true)
                    {

                        picImage.MouseClick += picImage_Click_Handler;
                        mode = Mode.DEFAULT;
                    
                        started = false;
                        camera.stopCapture();

                        time2 = DateTime.Now;
                        elapsed = time2 - time1;

                        Console.WriteLine("Avg. time: " + ((elapsed.TotalMilliseconds) / numPasses - 1000).ToString());

                        if (rawImage != null) {
                            rawImage.showInPictureBox(picImage);
                        }
                    }
                    break;
            }

        }

        
        public void updateImage() {
            int zoomFactor;
            if (rawImage == null)
            {
                return;
            }
            if (zoomedImage == null)
            {
                zoomFactor = 1;
            }
            else
            {
                zoomFactor = zoomedImage.zoomFactor;
            }
            zoomedImage = rawImage.zoom(zoomFactor);
            zoomedImage.showInPictureBox(picImage);
        }


          

    }
}