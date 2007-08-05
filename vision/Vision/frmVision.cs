using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using VisionStatic;


namespace Vision {  
    public partial class frmVision : Form {

        private VisionCamera.Camera _camera;
        private int _cameraID = 1;
        
        private TsaiCalibration _tsaiCalibrator;
        private ColorCalibration _colorCalibrator;
        private Blobber _blobber;

        private ImageForm _imageForm;
        
        //!!! MRS
        VisionServiceOperations _visionServicePort;

        //MRS!!!
        public frmVision(VisionServiceOperations visionServicePort)
        {

            //MRS!!
            _visionServicePort = visionServicePort;
            
            InitializeComponent();

            _imageForm = new ImageForm();

            _camera = new VisionCamera.Camera();

            _tsaiCalibrator = new TsaiCalibration(_cameraID);
            _colorCalibrator = new ColorCalibration();
            _blobber = new Blobber(_colorCalibrator, _tsaiCalibrator, _camera, _imageForm);
            
            
            
            
        }

        private void frmRAWImage_Load(object sender, EventArgs e)
        {
            DefaultInitSequence();               
        }

        private void DefaultInitSequence() {

            _tsaiCalibrator.DefaultInitSequence();
            _imageForm.LoadTsaiCalibrator(_tsaiCalibrator);

            _colorCalibrator.DefaultInitSequence();
            _imageForm.LoadColorCalibrator(_colorCalibrator);

            _imageForm.LoadBlobber(_blobber);

            // MRS !!!
            _imageForm.LoadVisionServicePort(_visionServicePort);
            
         }

        private void btnHighlightBlob_Click(object sender, EventArgs e) {

       /*     if (txtHighlightBlob.Text.ToString().Length == 0) {
                MessageBox.Show("Please enter a Blob ID!");
                return;
            }

            int blobID = Convert.ToInt32(txtHighlightBlob.Text.ToString());

            if (blobID < 0 || blobID >= BlobWorkObj.totalBlobs) {
                MessageBox.Show("Blob with ID " + blobID.ToString() + " not found!");
                return;
            }

            populateBlobInfoTable(BlobWorkObj.blobs[blobID]);

            RAWImage imgHLed;
            imgHLed = frmCameraImageObj.zoomedImage.Clone();
            
            VisionStatic.UserGUI.highlightBlob(imgHLed, BlobWorkObj.blobs[blobID]);

            imgHLed.showInPictureBox(frmCameraImageObj.picImage);
        */
        }

        private void btnHighlightNBlobs_Click(object sender, EventArgs e) {
           /* int i;
            int n;
            RAWImage imgHLed;

            if (txtNBlobs.Text.ToString().Length <= 0) {
                MessageBox.Show("Enter the number of blobs to highlight!");
                return;
            }

            n = Convert.ToInt32(txtNBlobs.Text.ToString());

            if (n < 0 || n > BlobWorkObj.totalBlobs) {
                MessageBox.Show("Too few or too many blobs to highlight!");
                return;
            }

           
            imgHLed = frmCameraImageObj.zoomedImage.Clone();
            
            for (i = 0; i < n; i++) {
                VisionStatic.UserGUI.highlightBlob(imgHLed, BlobWorkObj.blobs[i]);
            }
            imgHLed.showInPictureBox(frmCameraImageObj.picImage);
            */
        }

        private void btnFindBlobByCol_Click(object sender, EventArgs e) {
            /*Blob blob;
            //blob = null;
            blob = VisionStatic.UserGUI.findBlobByColAndCenter(BlobWorkObj.blobs, BlobWorkObj.totalBlobs, 
                        lblSelBlobByCol.BackColor,
                        Convert.ToInt32(txtSelBlobByCenterX.Text.ToString()),
                        Convert.ToInt32(txtSelBlobByCenterY.Text.ToString()), 
                        _colorCalibrator); 
            if (blob == null) {
                MessageBox.Show("Blob with that color not found!");
                return;
            }

            populateBlobInfoTable(blob);

            //highlight blob:
            RAWImage imgHLed;
            imgHLed = frmCameraImageObj.zoomedImage.Clone();

            VisionStatic.UserGUI.highlightBlob(imgHLed, blob);

            imgHLed.showInPictureBox(frmCameraImageObj.picImage);
             */
        }

      /*  private void populateBlobInfoTable(Blob blob) {
            lblBlobID.Text = blob.BlobID.ToString();
            lblCenter.Text = "(" + blob.CenterX.ToString() + "; " +
                            blob.CenterY.ToString() + ")";
            lblCenterWorld.Text = String.Format("({0:N}; {1:N})", blob.CenterWorldX, blob.CenterWorldY);
            lblAvgColor.BackColor = Color.FromArgb(blob.AvgColorR,
                                                    blob.AvgColorG,
                                                    blob.AvgColorB);
            lblBlobColorClass.Text = blob.ColorClass.ToString();

            lblBlobArea.Text = blob.Area.ToString();
        }*/


        private void dlgSaveBlobInfo_FileOk(object sender, CancelEventArgs e) {
            if (_blobber == null) {
                MessageBox.Show("Image not blobbed.");
                return;
            }

            VisionStatic.UserGUI.exportBlobInfoToFile(dlgSaveBlobInfo.FileName, _blobber);

            MessageBox.Show("Blob info successfully written to file.");
        }

       
   
        private void cameraImageToolStripMenuItem_Click(object sender, EventArgs e) {
            if (!_imageForm.Visible)
                _imageForm.Show();
            else                 
                _imageForm.BringToFront();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        private void colorCalibrationToolStripMenuItem_Click(object sender, EventArgs e) {
            _colorCalibrator.ShowSlice(0);
        }

        private void saveTsaiPointsToolStripMenuItem_Click(object sender, EventArgs e) {
            dlgSaveTsaiPoints.ShowDialog();
        }

        private void loadTsaiPointsToolStripMenuItem_Click(object sender, EventArgs e) {
            dlgLoadTsaiPoints.ShowDialog();
        }

        private void dlgSaveTsaiPoints_FileOk(object sender, CancelEventArgs e) {
            _tsaiCalibrator.SaveTsaiPoints(dlgSaveTsaiPoints.FileName);
        }

        private void dlgLoadTsaiPoints_FileOk(object sender, CancelEventArgs e) {
           /* _tsaiCalibrator.loadTsaiPoints(dlgLoadTsaiPoints.FileName);
            _tsaiCalibrator.showTsaiPoints();
            chkShowHideTsaiPoints.Checked = true;*/
        }



        private void exportBlobInfoToolStripMenuItem_Click(object sender, EventArgs e) {
            dlgSaveBlobInfo.ShowDialog();
        }
  
        private void worldImageToolStripMenuItem_Click(object sender, EventArgs e) {
      /*      if (frmWorldObj.Visible == false) {
                frmWorldObj.Show();
            } else {
                frmWorldObj.BringToFront();
            } */
        }

    

        private void rbCamera2_CheckedChanged(object sender, EventArgs e) {
           /* if (rbCamera2.Checked) {
                _tsaiCalibrator = new TsaiCalibration(frmCameraImageObj.picImage, 2);
                dlgLoadTsaiPoints.FileName = VisionStatic.Config.WORK_DIR + "\\tsai_points.tps";
                dlgLoadTsaiPoints_FileOk(this, new CancelEventArgs());
                
                btnTsaiCalibrate_Click(this, new EventArgs());
                BlobWorkObj.tsaiCalibObj = _tsaiCalibrator;
                frmRealTimeImageObj.tsaiCalibObj = _tsaiCalibrator;
                frmWorldObj._tsaiCalibObj = _tsaiCalibrator;

                chkShowHideTsaiPoints.Checked = false;
                _tsaiCalibrator.showTsaiPoints();
                chkShowHideTsaiPoints.Checked = true;
            }*/
        }

        private void rbCamera1_CheckedChanged(object sender, EventArgs e) {
            /*if (rbCamera1.Checked) {
                _tsaiCalibrator = new TsaiCalibration(frmCameraImageObj.picImage, 1);
                dlgLoadTsaiPoints.FileName = VisionStatic.Config.WORK_DIR + "\\tsai_points.tps";
                dlgLoadTsaiPoints_FileOk(this, new CancelEventArgs());

                btnTsaiCalibrate_Click(this, new EventArgs());
                BlobWorkObj.tsaiCalibObj = _tsaiCalibrator;
                frmRealTimeImageObj.tsaiCalibObj = _tsaiCalibrator;
                frmWorldObj._tsaiCalibObj = _tsaiCalibrator;

                chkShowHideTsaiPoints.Checked = false;
                _tsaiCalibrator.showTsaiPoints();
                chkShowHideTsaiPoints.Checked = true;
            }*/
        }

        private void chkToggleVisionTest_CheckedChanged(object sender, EventArgs e) {
            VisionTest.toggle();
        }

        private void btnDisplayLostBalls_Click(object sender, EventArgs e) {
           /* Graphics oGraphics = frmCameraImageObj.createGraphics();            
            VisionTest.displayLostBalls(oGraphics);
            frmCameraImageObj.updateImage();
            oGraphics.Dispose();
            */
        }

        private void frmVision_FormClosing(object sender, FormClosingEventArgs e) {
            _colorCalibrator.DefaultEndSequence();
            _tsaiCalibrator.DefaultEndSequence();
        }

    }
}
