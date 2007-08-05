using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing.Imaging;
using VisionStatic;


namespace Vision {
    public class RAWImage {
        /* fields */
        private int width;
        private int height;
        private int rawDataLength;
        public byte[] rawData;
        private Bitmap imgBitmap;
        public int zoomFactor;

        /* properties */
        public int Width {
            get { return width; }
            set { width = value; }
        }
        public int Height {
            get { return height; }
            set { height = value; }
        }
        public int RawDataLength {
            get { return rawDataLength; }
            set { rawDataLength = value; }
        }
        public byte[] RawData {
            get { return rawData; }
            set { rawData = value; }
        }
        public Bitmap ImgBitmap {
            get { return imgBitmap; }
            set { imgBitmap = value; }
        }

        /* constructors */
        public RAWImage(int newWidth, int newHeight) {
            width = newWidth;
            height = newHeight;
            rawDataLength = width * height * 3;
            rawData = new byte[width * height * 3];
            imgBitmap = null;

            zoomFactor = 1;
        }

        public RAWImage(byte[] newRawData, int newWidth, int newHeight, int _zoomFactor)
            : this(newWidth, newHeight) {
            rawData = newRawData;
            zoomFactor = _zoomFactor;
        }
        /*public RAWImage(char* newRawData, int newWidth, int newHeight) : this(newWidth, newHeight);
        {
            Marshal.Copy((IntPtr)newRawData, rawData, 0, width * height * 3);
        }*/
        public RAWImage(RAWImage rawImage, int _width, int _height)
            : this(_width, _height) {
            zoomFactor = rawImage.zoomFactor;
        }
        public RAWImage(RAWImage rawImage)
            : this(rawImage, rawImage.width, rawImage.height) {
        }
        public RAWImage(string filename, int newWidth, int newHeight)
            : this(newWidth, newHeight) {
            rawData = new byte[rawDataLength];

            FileStream fStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            BinaryReader binReader = new BinaryReader(fStream);
            binReader.Read(rawData, 0, rawDataLength);
            binReader.Close();
            fStream.Close();

            RGBtoBGR();

        }

        RAWImage rawClone;
        public RAWImage Clone() {
            int i;
            byte[] cloneRawData;
            

            cloneRawData = new byte[width * height * 3];
            for (i = 0; i < rawDataLength; i++) {
                cloneRawData[i] = rawData[i];
            }

            rawClone = null;
            rawClone = new RAWImage(cloneRawData, width, height, zoomFactor);

            return rawClone;

        }

        public void RGBtoBGR() {

            byte swapSpace;

            for (int i = 0; i <= rawDataLength - 3; i += 3) {
                swapSpace = rawData[i];
                rawData[i] = rawData[i + 2];
                rawData[i + 2] = swapSpace;
            }

        }
        public Bitmap toBitmap() {
            System.Drawing.Imaging.PixelFormat format = PixelFormat.Format24bppRgb;
            imgBitmap = new Bitmap(width, height, format);

            System.Drawing.Imaging.BitmapData bitmapData;
            bitmapData = imgBitmap.LockBits(new Rectangle(0, 0, imgBitmap.Width, imgBitmap.Height),
                                            ImageLockMode.WriteOnly, format);

            IntPtr ptrBitmapData = bitmapData.Scan0;

            Marshal.Copy(rawData, 0, ptrBitmapData, rawDataLength);

            imgBitmap.UnlockBits(bitmapData);

            return imgBitmap;
        }
        public void showInPictureBox(PictureBox picBox) {
            this.toBitmap();

            picBox.Width = imgBitmap.Width;
            picBox.Height = imgBitmap.Height;
            picBox.BackgroundImage = imgBitmap;

        }

        RAWImage zoomedImage;
        public RAWImage zoom(int factor) {
            zoomedImage = null;
            zoomedImage = new RAWImage(this, width * factor, height * factor);
            zoomedImage.zoomFactor = factor;

            int f1, f2;
            int i, j;
            int iAtStart;
            int row, column;
            i = 0;
            j = 0;

            row = 0;
            do {
                for (f1 = 0; f1 < factor; f1++) {

                    iAtStart = i;

                    column = 0;
                    do {
                        for (f2 = 0; f2 < factor; f2++) {
                            zoomedImage.rawData[j] = rawData[i];
                            zoomedImage.rawData[j + 1] = rawData[i + 1];
                            zoomedImage.rawData[j + 2] = rawData[i + 2];
                            j += 3;
                        }
                        i += 3;
                        column++;
                    } while (column < width);

                    i = iAtStart;

                }
                i += width * 3;
                row++;
            } while (row < height);

            //zoomedImage.toBitmap();

            return zoomedImage;

        }

        RAWImage imgG;
        public RAWImage drawGrid(int step, Color gridColor) {
            byte[] rawDataG = new byte[rawDataLength];
            int i;
            int row;
            row = 0;
            i = 0;
            do {
                rawDataG[i] = rawData[i];
                rawDataG[i + 1] = rawData[i + 1];
                rawDataG[i + 2] = rawData[i + 2];
                if (i % (step * 3) == 0 || row % step == 0) {
                    rawDataG[i] = Convert.ToByte(gridColor.R);
                    rawDataG[i + 1] = Convert.ToByte(gridColor.G);
                    rawDataG[i + 2] = Convert.ToByte(gridColor.B);
                }
                if (i % width == 0) {
                    row++;
                }
                i += 3;
            } while (i < rawDataLength);

            imgG = null;
            imgG = new RAWImage(rawDataG, width, height, zoomFactor);
            return imgG;
        }


        RAWImage colClassImg;
        public RAWImage toColorClass(ColorCalibrator colorCalibObj) {
            int h, s, v;
            int i;
            
            int colClass;
            byte[, ,] HSVLookupTable;

            HSVLookupTable = colorCalibObj.HSVtoCCTable;

            colClassImg = null;
            colClassImg = new RAWImage(width, height);

            i = 0;
            do {
                colorCalibObj.RGBtoHSVLookup(rawData[i + 2], rawData[i + 1], rawData[i], out h, out s, out v);
                v = v / ColorCalibrator.V_SLICE_DEPTH;
                colClass = HSVLookupTable[h, s, v];

                colClassImg.rawData[i + 2] = ColorClasses.COLOR_CLASSES[colClass].R;
                colClassImg.rawData[i + 1] = ColorClasses.COLOR_CLASSES[colClass].G;
                colClassImg.rawData[i] = ColorClasses.COLOR_CLASSES[colClass].B;
                i += 3;
            } while (i < rawDataLength);

            return colClassImg;
        }

    }
}
