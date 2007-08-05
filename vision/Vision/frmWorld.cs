using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Vision {
    
    public partial class frmWorld : Form {
        public RAWImage rawImage;
        public TsaiCalibrator _tsaiCalibObj;
        public frmWorld(TsaiCalibrator tsaiCalibObj) {
            InitializeComponent();
            _tsaiCalibObj = tsaiCalibObj;
        }


        public void loadWorld() {
            int row, col, i, j;
            double wx, wy;
            int pixWX, pixWY;
            int index;


            Bitmap bitmap;
            byte[] bBitmap;
            BitmapData bdBitmap;
            IntPtr ptrBitmap;

            picWorld.Width = 1024;
            picWorld.Height = 768;


            bitmap = new Bitmap(picWorld.Width, picWorld.Height);
            bBitmap = new byte[bitmap.Width * bitmap.Height * 3];

         
            i = 0;
            for (row = 0; row < VisionStatic.Field.HEIGHT; row++) {
                for (col = 0; col < VisionStatic.Field.WIDTH; col++) {
                    _tsaiCalibObj.ImageCoordToWorldCoord(col, row, 0, out wx, out wy);
                    pixWX = Convert.ToInt32(wx);
                    pixWY = Convert.ToInt32(wy);
                    pixWX = picWorld.Width - (int)(picWorld.Width * ((float)pixWX / TsaiCalibrator.TSAIWIDTH));
                    pixWY = picWorld.Height - (int)(picWorld.Height * ((float)(pixWY - TsaiCalibrator.TSAIHEIGHT/2)/ (TsaiCalibrator.TSAIHEIGHT/2)));
                    index = coordsToIndex(pixWX, pixWY, picWorld.Width);
                    if (pixWX >= 0 && pixWX < picWorld.Width && pixWY >= 0 && pixWY < picWorld.Height) {
                        //bitmap.SetPixel(pixWX, pixWY, Color.FromArgb(rawImage.RawData[i + 2], rawImage.RawData[i + 1], rawImage.RawData[i]));
                        bBitmap[index] = rawImage.rawData[i];
                        bBitmap[index + 1] = rawImage.rawData[i + 1];
                        bBitmap[index + 2] = rawImage.rawData[i + 2];
                    }
                    i += 3;
                }
            }

            //paint tsaiPoints
            foreach (TsaiPoint tP in _tsaiCalibObj.tsaiPoints) {
                pixWX = Convert.ToInt32(tP.wx);
                pixWY = Convert.ToInt32(tP.wy);
                pixWX = (int)(picWorld.Width - picWorld.Width * ((float)pixWX / TsaiCalibrator.TSAIWIDTH));
                pixWY = (int)(picWorld.Height * ((float)(pixWY - TsaiCalibrator.TSAIHEIGHT / 2) / (TsaiCalibrator.TSAIHEIGHT / 2)));
                for (i = pixWY - 5; i < pixWY + 5; i++) {
                    for (j = pixWX - 5; j < pixWX + 5; j++) {
                        if (j >= 0 && j < picWorld.Width && i >= 0 && i < picWorld.Height) {
                            index = coordsToIndex(j, i, picWorld.Width);
                            //bitmap.SetPixel(j, i, Color.Lime);
                            bBitmap[index] = Color.Lime.B;
                            bBitmap[index + 1] = Color.Lime.G;
                            bBitmap[index + 2] = Color.Lime.R;
                        }
                    }
                }
            }

           
            

            bdBitmap = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                            ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            ptrBitmap = bdBitmap.Scan0;

            Marshal.Copy(bBitmap, 0, ptrBitmap, bBitmap.Length);

            bitmap.UnlockBits(bdBitmap);

            picWorld.BackgroundImage = bitmap;
            picWorld.Refresh();
        }

        private int coordsToIndex(int x, int y, int width) {
            return (y * width * 3 + x * 3);
        }
    }
}