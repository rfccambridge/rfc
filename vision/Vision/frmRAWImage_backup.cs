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
using RAWImageApp;


namespace WindowsApplication2
{
    public partial class frmRAWImage : Form
    {
        public byte[] data;
        private RAWImage rawImage;
        private RAWImage zoomedImage;
        private RAWImage normedRawImage;
        private BlobWork BlobWorkObj;

        private byte[] rawData;



        public frmRAWImage()
        {
            InitializeComponent();
        }

       
        private void btnDisplay_Click(object sender, EventArgs e)
        {
            string file = txtSourceFile.Text;
            int width = Field.WIDTH, height = Field.HEIGHT;
           
            if (File.Exists(file))
            {
                if (width > 0 && height > 0)
                {
                    rawImage = new RAWImage(file, width, height);
                    rawImage.RGBtoBGR();
                    rawImage.showInPictureBox(pictureBox1);

                    zoomedImage = rawImage.zoom(1);

                    //zoomedImage.RGBtoBGR();
                    

                    data = rawImage.RawData;
                }
                else
                {
                    MessageBox.Show("Invalid dimensions!", "RAW Image", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("File not found!", "RAW Image", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dlgOpenFile_FileOk(object sender, CancelEventArgs e)
        {
            txtSourceFile.Text = dlgOpenFile.FileName;
        }

        private void btnBrowse_Click_1(object sender, EventArgs e)
        {
            dlgOpenFile.ShowDialog();
        }

        private void btnBlob_Click(object sender, EventArgs e)
        {
            //BlobWorkObj = new BlobWork(data);
            BlobWorkObj = new BlobWork(normedRawImage.rawData);
            BlobWorkObj.doBlob();

            lblBlobsFound.Text = BlobWorkObj.totalBlobs.ToString();

            txtHighlightBlob.Enabled = true;
            btnHighlightBlob.Enabled = true;
        }

        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            
            //pictureBox1.Width *= 2;
            //pictureBox1.Height *= 2;
           //pictureBox1.Size = new Size(pictureBox1.Width * 2, pictureBox1.Height * 2);
            //pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            
            zoomedImage = rawImage.zoom(Convert.ToInt32(txtZoomFactor.Text));
            zoomedImage.showInPictureBox(pictureBox1);

        }

        private void pictureBox1_Resize(object sender, EventArgs e) {

        }

        private void btnDrawGrid_Click(object sender, EventArgs e) {

            RAWImage imgWithGrid;

            imgWithGrid = zoomedImage.drawGrid(Convert.ToInt32(txtZoomFactor.Text), new RgbColor(200, 200, 200));

            imgWithGrid.showInPictureBox(pictureBox1);
            
        }

        private void btnHighlightBlob_Click(object sender, EventArgs e) {

            if (txtHighlightBlob.Text.ToString().Length == 0) {
                MessageBox.Show("Please enter a Blob ID!");
                return;
            }

            int blobID = Convert.ToInt32(txtHighlightBlob.Text.ToString());

            if (blobID < 0 || blobID >= BlobWorkObj.totalBlobs) {
                MessageBox.Show("Blob with ID " + blobID.ToString() + " not found!");
                return;
            }

            lblBlobID.Text = blobID.ToString();
            lblCenter.Text = "(" + BlobWorkObj.blobs[blobID].CenterX.ToString() + ", " +
                            BlobWorkObj.blobs[blobID].CenterY.ToString() + ")";
            lblAvgColor.BackColor = Color.FromArgb(BlobWorkObj.blobs[blobID].AvgColor.R,
                BlobWorkObj.blobs[blobID].AvgColor.G, BlobWorkObj.blobs[blobID].AvgColor.B);

            lblBlobArea.Text = BlobWorkObj.blobs[blobID].Area.ToString();

            rawData = new byte[zoomedImage.RawDataLength];
            int i = 0;
            do {
                rawData[i] = zoomedImage.RawData[i];
                i++;
            } while (i < zoomedImage.RawDataLength);

            RAWImage imgHLed;
            imgHLed = new RAWImage(rawData, zoomedImage.Width, zoomedImage.Height);
            UserGUI.highlightBlob(imgHLed, BlobWorkObj.blobs[blobID], Convert.ToInt32(txtZoomFactor.Text));
            
            imgHLed.showInPictureBox(pictureBox1);
        }

        private void txtSelRow_Click(object sender, EventArgs e) {
           

            if (txtRow.Text.ToString().Length == 0) {
                MessageBox.Show("Please enter a Row #!");
                return;
            }

            int row = Convert.ToInt32(txtRow.Text.ToString());

            if (row < 0 || row >= Field.HEIGHT) {
                MessageBox.Show("Row # " + row.ToString() + " does not exist!");
                return;
            }

            lblRunsInRow.Text = BlobWorkObj.numRunsInRow[row].ToString();
            
        }

        private void btnSelRun_Click(object sender, EventArgs e) {
            if (txtRunID.Text.ToString().Length == 0) {
                MessageBox.Show("Please enter a Run # in line!");
                return;
            }

            int row = Convert.ToInt32(txtRow.Text.ToString());
            int runID = Convert.ToInt32(txtRunID.Text.ToString());

            if (runID < 0 || runID >= BlobWorkObj.numRunsInRow[row]) {
                MessageBox.Show("Run # " + runID.ToString() + " does not exist in selected line!");
                return;
            }

            int runGlobID = 0;
            int i = 0;
            while (i < row) {
                runGlobID += BlobWorkObj.numRunsInRow[i];
                i++;
            }
            runGlobID += runID;

            lblRunID.Text = runGlobID.ToString();
            lblRunCoords.Text = "(" + row.ToString() + ", " + runID.ToString() + ")";
            lblRunAvgColor.BackColor = Color.FromArgb(BlobWorkObj.runs[runGlobID].AvgColor.R,
                                                    BlobWorkObj.runs[runGlobID].AvgColor.G,
                                                    BlobWorkObj.runs[runGlobID].AvgColor.B);
            lblRunLength.Text = Convert.ToString(BlobWorkObj.runs[runGlobID].RightEnd / 3 - BlobWorkObj.runs[runGlobID].LeftEnd / 3);
        }

        private void frmRAWImage_Load(object sender, EventArgs e) {
            txtWidth.Text = Field.WIDTH.ToString();
            txtHeight.Text = Field.HEIGHT.ToString();
            pictureBox1.Width = Field.WIDTH;
            pictureBox1.Height = Field.HEIGHT;
        }

        private void btnHighlightNBlobs_Click(object sender, EventArgs e) {

            if (txtNBlobs.Text.ToString().Length <= 0) {
                MessageBox.Show("Enter the number of blobs to highlight!");
                return;
            }

            int n = Convert.ToInt32(txtNBlobs.Text.ToString());

            if (n < 0 || n >= BlobWorkObj.totalBlobs) {
                MessageBox.Show("Too few or too many blobs to highlight!");
                return;
            }


            rawData = new byte[zoomedImage.RawDataLength];
            int i = 0;
            do {
                rawData[i] = zoomedImage.RawData[i];
                i++;
            } while (i < zoomedImage.RawDataLength);

            RAWImage imgHLed;
            imgHLed = new RAWImage(rawData, zoomedImage.Width, zoomedImage.Height);
            for (i = 0; i < n; i++) {
                UserGUI.highlightBlob(imgHLed, BlobWorkObj.blobs[i], Convert.ToInt32(txtZoomFactor.Text));
            }
            imgHLed.showInPictureBox(pictureBox1);
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e) {
           // zoomedImage.RGBtoBGR();
            lblSelBlobByCol.BackColor = Color.FromArgb(zoomedImage.ImgBitmap.GetPixel(e.X, e.Y).R, 
                                                       zoomedImage.ImgBitmap.GetPixel(e.X, e.Y).G,
                                                       zoomedImage.ImgBitmap.GetPixel(e.X, e.Y).B);
            txtSelBlobByCenterX.Text = Convert.ToString(e.X / Convert.ToInt32(txtZoomFactor.Text.ToString()));
            txtSelBlobByCenterY.Text = Convert.ToString(e.Y / Convert.ToInt32(txtZoomFactor.Text.ToString()));

        }

        private void btnFindBlobByCol_Click(object sender, EventArgs e) {
            Blob blob;
            blob = UserGUI.findBlobByColAndCenter(BlobWorkObj.blobs, BlobWorkObj.totalBlobs, new RgbColor(lblSelBlobByCol.BackColor.R,
                                                                    lblSelBlobByCol.BackColor.G,
                                                                    lblSelBlobByCol.BackColor.B),
                                                                  Convert.ToInt32(txtSelBlobByCenterX.Text.ToString()), 
                                                                  Convert.ToInt32(txtSelBlobByCenterY.Text.ToString()));
            if (blob == null) {
                MessageBox.Show("Blob with that color not found!");
            }

            //highlight blob:
            rawData = new byte[zoomedImage.RawDataLength];
            int i = 0;
            do {
                rawData[i] = zoomedImage.RawData[i];
                i++;
            } while (i < zoomedImage.RawDataLength);

            RAWImage imgHLed;
            imgHLed = new RAWImage(rawData, zoomedImage.Width, zoomedImage.Height);
            UserGUI.highlightBlob(imgHLed, blob, Convert.ToInt32(txtZoomFactor.Text));
            
            imgHLed.showInPictureBox(pictureBox1);
        }

        private void btnNormalize_Click(object sender, EventArgs e) {
            RgbColor[] normColors = new RgbColor[7];

            normColors[0] = new RgbColor(0, 0, 0); //black
            normColors[1] = new RgbColor(255, 255, 255); //white
            normColors[2] = new RgbColor(255, 0, 0); //red
            normColors[3] = new RgbColor(0, 255, 0); //green
            normColors[4] = new RgbColor(0, 0, 255); //blue
            normColors[5] = new RgbColor(255, 255, 0); //yellow
            normColors[6] = new RgbColor(255, 0, 255); //pink

            normedRawImage = zoomedImage.normalize(normColors);

            normedRawImage.showInPictureBox(pictureBox1);
            
        }








    }
}