using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Collections;
using System.IO;
using Vision;
using Robocup.Utilities;


namespace VisionStatic {

    public static class ColorClasses {
        
        public const int COLOR_UNKNOWN = 0;
        public const int COLOR_FIELD = 1;
        public const int COLOR_BALL = 2;
        public const int COLOR_BLUE_CENTER_DOT = 3;
        public const int COLOR_YELLOW_CENTER_DOT = 4;
        public const int COLOR_DOT_PINK = 5;
        public const int COLOR_DOT_GREEN = 6;
        public const int COLOR_DOT_CYAN = 7;

        private const int TOTAL_COLORS = 8; // don't forget to change the total if you add/remove a game color
        
        private const int TOTAL_ROBOTS = 10;

        public static int OUR_CENTER_DOT;
        public static int THEIR_CENTER_DOT;
        
        public static Color[] COLOR_CLASSES;

        //WHATCH the order here and in the array right bellow:
        public static int[, , ,] DOT_PATTERNS;

        private static Dictionary<char, int> CHAR_TO_COLOR;

        static ColorClasses() {

            // this is constant and is used for convenient notation of dot pattern in the constants.txt file
            CHAR_TO_COLOR = new Dictionary<char, int>();
            CHAR_TO_COLOR.Add('P', COLOR_DOT_PINK);
            CHAR_TO_COLOR.Add('C', COLOR_DOT_CYAN);
            CHAR_TO_COLOR.Add('G', COLOR_DOT_GREEN);

            COLOR_CLASSES = new Color[TOTAL_COLORS];

            //symbolic colors
            COLOR_CLASSES[COLOR_UNKNOWN] = Color.FromArgb(0x00, 0x00, 0x00, 0x00); // void/unassigned
            COLOR_CLASSES[COLOR_FIELD] = Color.FromArgb(0xFF, 0x00, 0x40, 0x00);   //field green - 25574C
            COLOR_CLASSES[COLOR_BALL] = Color.FromArgb(0xFF, 0xF0, 0x46, 0x0A);    //orange ball
            COLOR_CLASSES[COLOR_DOT_PINK] = Color.FromArgb(0xFF, 0xFF, 0x00, 0x7E);   //pink dot
            COLOR_CLASSES[COLOR_DOT_GREEN] = Color.FromArgb(0xFF, 0x3E, 0xFF, 0x6F); //light green dot
            COLOR_CLASSES[COLOR_DOT_CYAN] = Color.FromArgb(0xFF, 0x00, 0xFF, 0xFF);    //cyan dot
            COLOR_CLASSES[COLOR_BLUE_CENTER_DOT] = Color.FromArgb(0xFF, 0x3A, 0x51, 0xED);    //blue dot - 2FC8FF
            COLOR_CLASSES[COLOR_YELLOW_CENTER_DOT] = Color.FromArgb(0xFF, 0xFF, 0xFF, 0x00);    //yellow center dot

            DOT_PATTERNS = new int[TOTAL_COLORS, TOTAL_COLORS, TOTAL_COLORS, TOTAL_COLORS];

            LoadParameters();

        }

        public static void LoadParameters()
        {
            if (Constants.get<string>("configuration", "OUR_TEAM_COLOR").ToUpper() == "BLUE")
            {
                OUR_CENTER_DOT = COLOR_BLUE_CENTER_DOT;
                THEIR_CENTER_DOT = COLOR_YELLOW_CENTER_DOT;
            }
            else
            {
                THEIR_CENTER_DOT = COLOR_BLUE_CENTER_DOT;
                OUR_CENTER_DOT = COLOR_YELLOW_CENTER_DOT;
            }

            int i, j, k, l;

            // 0 is a valid robot id, so we initialize the map dotpattern->robot id with sentinels -1
            for (i = 0; i < TOTAL_COLORS; i++)
                for (j = 0; j < TOTAL_COLORS; j++)
                    for (k = 0; k < TOTAL_COLORS; k++)
                        for (l = 0; l < TOTAL_COLORS; l++)
                            DOT_PATTERNS[i, j, k, l] = -1;

            string dotPattern;
            for (i = 0; i < TOTAL_ROBOTS; i++)
            {
                if (Constants.nondestructiveGet<string>("configuration", "DOTS_R_" + i.ToString(), out dotPattern))
                {
                    //front left, front right, rear left, rear right                     
                    DOT_PATTERNS[CHAR_TO_COLOR[dotPattern[0]], CHAR_TO_COLOR[dotPattern[1]],
                                CHAR_TO_COLOR[dotPattern[2]], CHAR_TO_COLOR[dotPattern[3]]] = i;
                }

            }
        }

        public static string GetName(byte colorClass)
        {
            int iColClass = (int)colorClass;
            if (iColClass == COLOR_UNKNOWN)
                return "UNKNOWN";
            if (iColClass == COLOR_FIELD)
                return "FIELD";
            if (iColClass == COLOR_BALL)
                return "BALL";
            if (iColClass == COLOR_BLUE_CENTER_DOT)
                return "BLUE_CENTER_DOT";
            if (iColClass == COLOR_YELLOW_CENTER_DOT)
                return "YELLOW_CENTER_DOT";
            if (iColClass == COLOR_DOT_CYAN)
                return "DOT_CYAN";
            if (iColClass == COLOR_DOT_PINK)
                return "DOT_PINK";
            if (iColClass == COLOR_DOT_GREEN)
                return "DOT_GREEN";

            return "INVALID";
        }
     
       }

    public struct HSVColor {
        public int H, S, V;

        public HSVColor(int h, int s, int v)
        {
            H = h;
            S = s;
            V = v;
        }
    }

    public class ColorCalibrator {
        public const int V_SLICE_DEPTH = 25;

        private const string WORK_DIR = "../../resources/vision/";

        private const string DEFAULT_HSV_TO_CC_TABLE = WORK_DIR + "hsv_to_cc_table.dat";
        private const string DEFAULT_RGB_TO_CC_TABLE = WORK_DIR + "rgb_to_cc_table.dat"; // only for quick loading time
        private const string DEFAULT_RGB_TO_HSV_TABLE = WORK_DIR + "rgb_to_hsv_table.dat"; // only for quick loading time


        public Color[, ,] origPixelsHSVspace;

        /* Color Lookup Tables
         * ------------------- */

        // RGB to CC Table is the table used when blobbing
        // This table is generated from HSV to CC table
        private byte[] _RGBtoCCTable;

        // HSV to CC Table is the table is constructed based on what user draws on the HSV space
        // This table is _not_ used when blobbing, this table is used to generate the RGB to CC table
        private byte[, ,] _HSVtoCCTable;

        // RGB to HSV Table is only a "constant" conversion table (its purpose is only to provide fast conversion)
        // It can be generated independently
        private HSVColor[] _RGBtoHSVTable;

        // RGB to CC is out of sync with HSV to CC
        private bool _tableOutOfSync = true;

        private HSVSliceForm[] _slicesForms;

        public bool TableOutOfSync {
            get { return _tableOutOfSync; }
        }

        public ColorCalibrator() {

            _HSVtoCCTable = new byte[361, 101, 100 / V_SLICE_DEPTH + 1];

            // Create forms
            int v, k;

            _slicesForms = new HSVSliceForm[100 / V_SLICE_DEPTH];
            k = 0;
            for (v = 0; v < 100 / V_SLICE_DEPTH; v++) {
                _slicesForms[k] = new HSVSliceForm(v, this);
                k++;
            }
        }

        public void DefaultInitSequence() {

            // this one we can always generate, but loading is quicker
            if (File.Exists(DEFAULT_RGB_TO_HSV_TABLE)) {
                LoadRGBtoHSVTable(DEFAULT_RGB_TO_HSV_TABLE);
            }
            else {
                GenerateRGBtoHSVTable();
                SaveRGBtoHSVTable(DEFAULT_RGB_TO_HSV_TABLE);
            }

            // having HSVtoCC table is not enough to blobb, we need to generate RGB_TO_CC table from it and then blob
            if (File.Exists(DEFAULT_HSV_TO_CC_TABLE))
                LoadHSVtoCCTable(DEFAULT_HSV_TO_CC_TABLE);

            // if this is loaded, we can blob
            if (File.Exists(DEFAULT_RGB_TO_CC_TABLE))
                LoadRGBtoCCTable(DEFAULT_RGB_TO_CC_TABLE);
                
        }

        public void DefaultEndSequence() {
            DialogResult res;
            
            res = MessageBox.Show("Save HSV to CC color table?", "Ending session...", MessageBoxButtons.YesNo);
            if (res == DialogResult.Yes)
                PromptSaveHSVtoCCTable();

            res = MessageBox.Show("Generate and save RGB to CC color table?", "Ending session...", MessageBoxButtons.YesNo);
            if (res == DialogResult.Yes) {
                if (TableOutOfSync)
                    GenerateRGBtoCCTable();
                PromptSaveRGBtoCCTable();
            }
            
        }

        public byte[] RGBtoCCTable {
            get { 
                if (_tableOutOfSync)
                    GenerateRGBtoCCTable();
                return _RGBtoCCTable;
            }
        }
        // not a constant getter!! slice form gets the table from this getter to edit it
        public byte[, ,] HSVtoCCTable {
            get {
                _tableOutOfSync = true;
                return _HSVtoCCTable;
            }
        }
        

        public void LoadPixels(Bitmap bmpImage, Rectangle region) {
            int h, s, v, vi;
            byte r, g, b;

            int i;
            int offset;
            int HSdataLength = 360 * 100 * 4;
            Bitmap[] bmpSlices = new Bitmap[100 / V_SLICE_DEPTH];
            BitmapData[] bmpdSlices = new BitmapData[100 / V_SLICE_DEPTH];
            IntPtr[] ptrSlices = new IntPtr[100 / V_SLICE_DEPTH];
            byte[][] bSlices = new byte[100 / V_SLICE_DEPTH][];

            int dataLength = region.Width * region.Height * 4;
            BitmapData data;
            IntPtr ptr;
            byte[] bImage = new byte[dataLength];

            data = bmpImage.LockBits(region, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            ptr = data.Scan0;
            Marshal.Copy(ptr, bImage, 0, dataLength);
            bmpImage.UnlockBits(data);

            for (i = 0; i < 100 / V_SLICE_DEPTH; i++) {
                bmpSlices[i] = new Bitmap(360, 100, PixelFormat.Format32bppArgb);

                bmpdSlices[i] = bmpSlices[i].LockBits(new Rectangle(0, 0, bmpSlices[i].Width, bmpSlices[i].Height),
                                                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                ptrSlices[i] = bmpdSlices[i].Scan0;

                bSlices[i] = new byte[HSdataLength];
            }


            i = 0;
            do {
                b = bImage[i];
                i++;
                g = bImage[i];
                i++;
                r = bImage[i];
                i += 2;

                RGBtoHSVLookup(r, g, b, out h, out s, out v);
                vi = v / V_SLICE_DEPTH;

                offset = (s * 360 + h) * 4;

                bSlices[vi][offset] = b;
                offset++;
                bSlices[vi][offset] = g;
                offset++;
                bSlices[vi][offset] = r;
                offset++;
                bSlices[vi][offset] = 255;
            } while (i < dataLength);

            for (i = 0; i < 100 / V_SLICE_DEPTH; i++) {
                Marshal.Copy(bSlices[i], 0, ptrSlices[i], HSdataLength);
                bmpSlices[i].UnlockBits(bmpdSlices[i]);

                _slicesForms[i].LoadBmpPixels(bmpSlices[i]);
            }

        }

        public void ShowSlice(int v, int left, int top) {
            _slicesForms[v].Location = new Point(left, top);
            _slicesForms[v].Show();
        }
        public void ShowSlice(int v) {
            _slicesForms[v].Show();
        }
        public void GenerateRGBtoCCTable() {           

            _RGBtoCCTable = new byte[256 * 256 * 256];

            int r, g, b;
            int h, s, v;

            for (r = 0; r <= 255; r++) {
                for (g = 0; g <= 255; g++) {
                    for (b = 0; b <= 255; b++) {
                        RGBtoHSVLookup((byte)r, (byte)g, (byte)b, out h, out s, out v);
                        v = v / V_SLICE_DEPTH;
                        _RGBtoCCTable[r * 65536 + g * 256 + b] = _HSVtoCCTable[h, s, v];
                    }
                }
            }

            _tableOutOfSync = false;
        }

        public void LoadRGBtoCCTable(string fin) {

            _RGBtoCCTable = new byte[256 * 256 * 256];

            int i, j, k;
            FileStream fStream = new FileStream(fin, FileMode.Open, FileAccess.Read);
            BinaryReader binReader = new BinaryReader(fStream);

            for (i = 0; i <= 255; i++)
                for (j = 0; j <= 255; j++)
                    for (k = 0; k <= 255; k++)
                        _RGBtoCCTable[i * 65536 + j * 256 + k] = binReader.ReadByte();

            binReader.Close();
            fStream.Close();

            // !!! if we load this table from file, we assume that both tables we load are in sync
            _tableOutOfSync = false;
        }

        public void SaveRGBtoCCTable(string fout) {
            int i, j, k;
            FileStream fStream = new FileStream(fout, FileMode.Create, FileAccess.Write);
            BinaryWriter binWriter = new BinaryWriter(fStream);

            for (i = 0; i <= 255; i++)
                for (j = 0; j <= 255; j++)
                    for (k = 0; k <= 255; k++)
                        binWriter.Write(_RGBtoCCTable[i * 65536 + j * 256 + k]);

            binWriter.Close();
            fStream.Close();
        }

        // This function both loads the HSV to CC table and populates the slice forms
        public void LoadHSVtoCCTable(string filename) {
            byte b;
            int h, s, v;
            int offset;
            Color colPix;
            BitmapData data;
            IntPtr ptr;

            const int dataLength = 360 * 100 * 4;

            byte[] bSlice = new byte[dataLength];


            FileStream fStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            BinaryReader binReader = new BinaryReader(fStream);

            for (v = 0; v < 100 / ColorCalibrator.V_SLICE_DEPTH; v++) {

                for (h = 0; h < 360; h++) {
                    for (s = 0; s < 100; s++) {
                        b = binReader.ReadByte();
                        // record in table
                        _HSVtoCCTable[h, s, v] = b;
                        // draw on bitmap
                        colPix = ColorClasses.COLOR_CLASSES[b];
                        if (colPix.A != 0) {
                            offset = s * 360 * 4 + h * 4;
                            bSlice[offset] = colPix.B;
                            bSlice[offset + 1] = colPix.G;
                            bSlice[offset + 2] = colPix.R;
                            bSlice[offset + 3] = 255;
                        }
                    }
                }

                // pass the bitmap with color classes to form
                //bmpSlice = new Bitmap(360, 100, PixelFormat.Format32bppArgb);
                Bitmap bmpSlice = new Bitmap(360, 100, PixelFormat.Format32bppArgb);
                //_slicesForms[v].GetBmpCC();
                data = bmpSlice.LockBits(new Rectangle(0, 0, bmpSlice.Width, bmpSlice.Height),
                                             ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                ptr = data.Scan0;
                Marshal.Copy(bSlice, 0, ptr, dataLength);
                bmpSlice.UnlockBits(data);
                _slicesForms[v].LoadBmpCC(bmpSlice);
                
                //slicesForms[v].LoadBmpCC(bmpSlice);
            }

            binReader.Close();
            fStream.Close();

            _tableOutOfSync = true;
        }

        public void SaveHSVtoCCTable(string filename) {
            int h, s, v;

            FileStream fStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
            BinaryWriter binWriter = new BinaryWriter(fStream);

            for (v = 0; v < 100 / V_SLICE_DEPTH; v++)
                for (h = 0; h < 360; h++)
                    for (s = 0; s < 100; s++)
                        binWriter.Write(_HSVtoCCTable[h, s, v]);

            binWriter.Close();
            fStream.Close();
        }

        public void PromptSaveTables() {
            if (TableOutOfSync)
                GenerateRGBtoCCTable();

            PromptSaveRGBtoCCTable();
            PromptSaveHSVtoCCTable();
        }
        private void PromptSaveRGBtoCCTable() {
            SaveFileDialog saveDlg = new SaveFileDialog();
            saveDlg.RestoreDirectory = true;
            saveDlg.Filter = "Color Table (*.dat)|*.dat";
            saveDlg.InitialDirectory = WORK_DIR;

            saveDlg.Title = "Save RGB to CC color table...";
            saveDlg.FileName = Path.GetFileName(DEFAULT_RGB_TO_CC_TABLE);
            saveDlg.FileOk += new System.ComponentModel.CancelEventHandler(delegate {
                SaveRGBtoCCTable(saveDlg.FileName);
            });
            
            saveDlg.ShowDialog();
        }
        private void PromptSaveHSVtoCCTable() {
            SaveFileDialog saveDlg = new SaveFileDialog();
            saveDlg.RestoreDirectory = true;
            saveDlg.Filter = "Color Table (*.dat)|*.dat";
            saveDlg.InitialDirectory = WORK_DIR;

            saveDlg.Title = "Save HSV to CC color table...";
            saveDlg.FileName = Path.GetFileName(DEFAULT_HSV_TO_CC_TABLE);
            saveDlg.FileOk += new System.ComponentModel.CancelEventHandler(delegate {
                 SaveHSVtoCCTable(saveDlg.FileName);
            });
            
            saveDlg.ShowDialog();
        }
        public void PromptLoadTables() {
            // order is important since LoadHSVtoCCTable() sets _tableOutOfSync to true
            // and LoadRGBtoCC sets it to false - this way you can just load HSVtoCC or load
            // both without regenerating (motivation - loading RGBtoCC is faster than generating it)
            PromptLoadHSVtoCCTable();
            PromptLoadRGBtoCCTable();
        }
        private void PromptLoadHSVtoCCTable() {
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.RestoreDirectory = true;
            openDlg.Filter = "Color Table (*.dat)|*.dat";
            openDlg.InitialDirectory = WORK_DIR;

            openDlg.Title = "Load HSV to CC color table...";
            openDlg.FileName = Path.GetFileName(DEFAULT_HSV_TO_CC_TABLE);
            openDlg.FileOk += new System.ComponentModel.CancelEventHandler(delegate {
               LoadHSVtoCCTable(openDlg.FileName);
             });
            
            openDlg.ShowDialog();
        }
        private void PromptLoadRGBtoCCTable() {
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.RestoreDirectory = true;
            openDlg.Filter = "Color Table (*.dat)|*.dat";
            openDlg.InitialDirectory = WORK_DIR;

            openDlg.Title = "Load RGB to CC color table...";
            openDlg.FileName = Path.GetFileName(DEFAULT_RGB_TO_CC_TABLE);
            openDlg.FileOk += new System.ComponentModel.CancelEventHandler(delegate {
                LoadRGBtoCCTable(openDlg.FileName);
            });

            openDlg.ShowDialog();
        }

        public Bitmap mergeBitmaps(Bitmap bmpBack, Bitmap bmpFore) {


            Bitmap bmpMerged;
            int dataLength;
            int i;
            byte[] bFore;
            byte[] bMerged;

            bmpMerged = (Bitmap)bmpBack.Clone();

            BitmapData bmpdMerged;
            bmpdMerged = bmpMerged.LockBits(new Rectangle(0, 0, bmpMerged.Width, bmpMerged.Height),
                                            ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            IntPtr ptrBmpdMerged = bmpdMerged.Scan0;

            BitmapData bmpdFore;
            bmpdFore = bmpFore.LockBits(new Rectangle(0, 0, bmpFore.Width, bmpFore.Height),
                                            ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            IntPtr ptrBmpdFore = bmpdFore.Scan0;

            dataLength = bmpFore.Width * bmpFore.Height * 4;
            bFore = new byte[dataLength];
            bMerged = new byte[dataLength];

            Marshal.Copy(ptrBmpdFore, bFore, 0, dataLength);
            Marshal.Copy(ptrBmpdMerged, bMerged, 0, dataLength);

            i = 0;
            do {
                if (bFore[i + 3] != 0) {
                    bMerged[i] = bFore[i];
                    i++;
                    bMerged[i] = bFore[i];
                    i++;
                    bMerged[i] = bFore[i];
                    i++;
                    bMerged[i] = bFore[i];
                    i++;
                }
                else {
                    i += 4;
                }
            } while (i < dataLength);

            Marshal.Copy(bMerged, 0, ptrBmpdMerged, dataLength);


            bmpMerged.UnlockBits(bmpdMerged);
            bmpFore.UnlockBits(bmpdFore);


            return bmpMerged;
        }

        // HSV to RGB Convertion

        private double minimum(double a, double b, double c) {
            double min;
            min = a;
            if (b < min) {
                min = b;
            }
            if (c < min) {
                min = c;
            }
            return min;
        }
        private double maximum(double a, double b, double c) {
            double max;
            max = a;
            if (b > max) {
                max = b;
            }
            if (c > max) {
                max = c;
            }
            return max;
        }

        /* RGB to HSV Lookup Table
         * Used only to speed up conversion from RGB to HSV space
         */
        public void GenerateRGBtoHSVTable() {
            _RGBtoHSVTable = new HSVColor[256 * 256 * 256];

            int r, g, b;
            HSVColor color;
            Color rgbColor;

            for (r = 0; r <= 255; r++) {
                for (g = 0; g <= 255; g++) {
                    for (b = 0; b <= 255; b++) {
                        rgbColor = Color.FromArgb(r, g, b);
                        color = new HSVColor((int)rgbColor.GetHue(), (int)(rgbColor.GetSaturation() * 99), (int)(rgbColor.GetBrightness() * 99));
                        _RGBtoHSVTable[r * 65536 + g * 256 + b] = color;
                    }
                }
            }
        }
        public void LoadRGBtoHSVTable(string fin) {
            _RGBtoHSVTable = new HSVColor[256 * 256 * 256];

            int[] buff = new int[3];
            int i, j, k;
            FileStream fStream = new FileStream(fin, FileMode.Open, FileAccess.Read);
            BinaryReader binReader = new BinaryReader(fStream);

            for (i = 0; i <= 255; i++) {
                for (j = 0; j <= 255; j++) {
                    for (k = 0; k <= 255; k++) {
                        _RGBtoHSVTable[i * 65536 + j * 256 + k].H = binReader.ReadInt32();
                        _RGBtoHSVTable[i * 65536 + j * 256 + k].S = binReader.ReadInt32();
                        _RGBtoHSVTable[i * 65536 + j * 256 + k].V = binReader.ReadInt32();
                    }
                }
            }

            binReader.Close();
            fStream.Close();
        }
        public void SaveRGBtoHSVTable(string fout) {

            int i, j, k;
            FileStream fStream = new FileStream(fout, FileMode.Create, FileAccess.Write);
            BinaryWriter binWriter = new BinaryWriter(fStream);


            for (i = 0; i <= 255; i++) {
                for (j = 0; j <= 255; j++) {
                    for (k = 0; k <= 255; k++) {
                        binWriter.Write(_RGBtoHSVTable[i * 65536 + j * 256 + k].H);
                        binWriter.Write(_RGBtoHSVTable[i * 65536 + j * 256 + k].S);
                        binWriter.Write(_RGBtoHSVTable[i * 65536 + j * 256 + k].V);
                    }
                }
            }

            binWriter.Close();
            fStream.Close();
        }


        public void RGBtoHSVLookup(byte r, byte g, byte b, out int h, out int s, out int v) {
            if (_RGBtoHSVTable == null) {
                GenerateRGBtoHSVTable();
                SaveRGBtoHSVTable(DEFAULT_RGB_TO_HSV_TABLE);
            }

            HSVColor color;

            color = new HSVColor();

            color = _RGBtoHSVTable[r * 256 * 256 + g * 256 + b];

          h = color.H;
          s = color.S;
          v = color.V;

        }

        public Bitmap ConvertToColorClass(Bitmap original) {
            // change the dataLength if changing size of bytes per pixel (e.g. 3 is for 24bbp)
            System.Drawing.Imaging.PixelFormat format = PixelFormat.Format24bppRgb;
            int dataLength = original.Width * original.Height * 3;
            byte[] rgbValues = new byte[dataLength];

            Bitmap colorClassed = new Bitmap(original.Width, original.Height, format);

            BitmapData data;
            IntPtr ptr;

            data = original.LockBits(new Rectangle(0, 0, original.Width, original.Height),
                                                ImageLockMode.ReadOnly, format);
            ptr = data.Scan0;

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, dataLength);
            original.UnlockBits(data);

            // Convert

            int h, s, v;
            int i = 0;
            int colClass;
            do {
                RGBtoHSVLookup(rgbValues[i + 2], rgbValues[i + 1], rgbValues[i], out h, out s, out v);
                v = v / V_SLICE_DEPTH;
                colClass = _HSVtoCCTable[h, s, v];

                rgbValues[i + 2] = ColorClasses.COLOR_CLASSES[colClass].R;
                rgbValues[i + 1] = ColorClasses.COLOR_CLASSES[colClass].G;
                rgbValues[i] = ColorClasses.COLOR_CLASSES[colClass].B;
                i += 3;
            } while (i < dataLength);

            // Write result
            data = colorClassed.LockBits(new Rectangle(0, 0, colorClassed.Width, colorClassed.Height),
                                         ImageLockMode.WriteOnly, format);

            ptr = data.Scan0;

            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, dataLength);

            colorClassed.UnlockBits(data);

            return colorClassed;

        }

        public void LocatePixel(Color color) {

            int h, s, v;

            RGBtoHSVLookup(color.R, color.G, color.B, out h, out s, out v);

            //Added by rishi 2/17
            int vslice = (int)(v / ColorCalibrator.V_SLICE_DEPTH);

            foreach (HSVSliceForm hsvS in _slicesForms)
                hsvS.Hide();

            HSVSliceForm hsvSlice = _slicesForms[vslice];

            hsvSlice.HighlightPixel(h, s);
            hsvSlice.Show();
        }

     

        public Bitmap CloneBitmap(Bitmap bmp) {
            BitmapData data;
            IntPtr ptr;
            int dataLength = bmp.Width * bmp.Height * 4;
            byte[] bTemp = new byte[dataLength];

            data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                   ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            ptr = data.Scan0;
            Marshal.Copy(ptr, bTemp, 0, dataLength);
            bmp.UnlockBits(data);

            Bitmap cloneBmp = new Bitmap(bmp.Width, bmp.Height);
            data = cloneBmp.LockBits(new Rectangle(0, 0, cloneBmp.Width, cloneBmp.Height),
                               ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            ptr = data.Scan0;
            Marshal.Copy(bTemp, 0, ptr, dataLength);
            cloneBmp.UnlockBits(data);

            return cloneBmp;
        }
    }
    /// <summary>
    /// TODO - Add class summary
    /// </summary>
    /// <remarks>
    /// 	created by - J Dunlap
    /// 	created on - 7/2/2003 11:44:33 PM
    /// </remarks>


    //**********************************************
    // Project: Flood Fill Algorithms in C# & GDI+
    // File Description: Flood Fill Class
    //
    // Copyright: Copyright 2003 by Justin Dunlap.
    //    Any code herein can be used freely in your own 
    //    applications, provided that:
    //     * You agree that I am NOT to be held liable for
    //       any damages caused by this code or its use.
    //     * You give proper credit to me if you re-publish
    //       this code.
    //**********************************************

    /// <summary>
    /// TODO - Add class summary
    /// </summary>
    /// <remarks>
    /// 	created by - J Dunlap
    /// 	created on - 7/2/2003 11:44:33 PM
    /// </remarks>
    public abstract class AbstractFloodFiller : object {

        //private members with public accessors
        protected Color m_fillcolorcolor = Color.Green;
        protected Color m_boundarycolorcolor = Color.Green;
        protected Bitmap m_Bmp = null;
        protected Point m_Pt = new Point();

        //private members
        protected bool[,] PixelsChecked;
        protected Queue CheckQueue = new Queue();


        /// <summary>
        /// Default constructor - initializes all fields to default values
        /// </summary>
        public AbstractFloodFiller() {
        }


        public Color FillColor {
            get {
                return m_fillcolorcolor;
            }
            set {
                m_fillcolorcolor = value;
            }
        }
        public Color BoundaryColor {
            get {
                return m_boundarycolorcolor;
            }
            set {
                m_boundarycolorcolor = value;
            }
        }


        public Bitmap Bmp {
            get {
                return m_Bmp;
            }
            set {
                m_Bmp = value;
            }
        }

        public Point Pt {
            get {
                return m_Pt;
            }
            set {
                m_Pt = value;
            }
        }


        public void FloodFill() {
            Exception ex = null;

            try {
                FloodFill(m_Bmp, m_Pt);
            }
            catch (Exception e) {
                ex = e;
                OnFillFinished(new FillFinishedEventArgs(ex));
                return;

            }
            OnFillFinished(new FillFinishedEventArgs(ex));
        }

        //initializes the FloodFill operation
        public abstract void FloodFill(Bitmap bmp, Point pt);


        //**************
        //COLOR HELPER FUNCTIONS
        //**************

        //public static int BGRA(byte B, byte G, byte R, byte A) {
        public int BGRA(byte B, byte G, byte R, byte A) {
            return (int)(B + (G << 8) + (R << 16) + (A << 24));
        }


        //EVENTS/EVENT RAISERS
        //-------------

        //raised when a fill operation is finished
        public event FillFinishedEventHandler FillFinished;
        protected void OnFillFinished(FillFinishedEventArgs args) {
            if (FillFinished != null)
                FillFinished.BeginInvoke(this, args, null, null);
        }

    }

    public delegate void FillFinishedEventHandler(object sender, FillFinishedEventArgs e);
    public class FillFinishedEventArgs : EventArgs {
        Exception m_exception = null;

        public FillFinishedEventArgs(Exception e) {
            m_exception = e;
        }

        public Exception exception {
            get {
                return m_exception;
            }
        }
    }


    public class FloodFiller : AbstractFloodFiller {

        private int m_fillcolor = 255 << 8;
        private int m_boundarycolor = 255 << 8;

        /// <summary>
        /// Default constructor - initializes all fields to default values
        /// </summary>
        public FloodFiller() {
        }


        ///<summary>initializes the FloodFill operation</summary>
        public override void FloodFill(Bitmap bmp, Point pt) {

            //get the color's int value, and convert it from RGBA to BGRA format (as GDI+ uses BGRA)
            m_fillcolor = BGRA(m_fillcolorcolor.B, m_fillcolorcolor.G, m_fillcolorcolor.R, m_fillcolorcolor.A); m_boundarycolor = BGRA(m_boundarycolorcolor.B, m_boundarycolorcolor.G, m_boundarycolorcolor.R, m_boundarycolorcolor.A);

            //get the bits
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            System.IntPtr Scan0 = bmpData.Scan0;

            unsafe {
                //resolve pointer
                byte* scan0 = (byte*)(void*)Scan0;

                int color = m_fillcolor;
                int boundarycolor = m_boundarycolor;

                //create the array of bools that indicates whether each pixel
                //has been checked.  (Should be bitfield, but C# doesn't support bitfields.)
                PixelsChecked = new bool[bmpData.Width + 1, bmpData.Height + 1];

                //do the first call to the loop
                QueueFloodFill(scan0, pt.X, pt.Y, new Size(bmpData.Width, bmpData.Height),
                    bmpData.Stride, (byte*)&color, (byte*)&boundarycolor);
            }

            bmp.UnlockBits(bmpData);

        }

        //********
        //QUEUE ALGORITHM
        //********

        public unsafe void QueueFloodFill(byte* scan0, int x, int y, Size bmpsize, int stride, byte* fillcolor, byte* boundarycolor) {
            CheckQueue = new Queue();


            //start the loop
            QueueFloodFill4(scan0, x, y, bmpsize, stride, fillcolor, boundarycolor);
            //call next item on queue
            while (CheckQueue.Count > 0) {
                Point pt = (Point)CheckQueue.Dequeue();
                QueueFloodFill4(scan0, pt.X, pt.Y, bmpsize, stride, fillcolor, boundarycolor);
            }

        }

        public unsafe void QueueFloodFill4(byte* scan0, int x, int y, Size bmpsize, int stride, byte* fillcolor, byte* boundarycolor) {
            //don't go over the edge
            if ((x < 0) || (x >= bmpsize.Width)) return;
            if ((y < 0) || (y >= bmpsize.Height)) return;

            //calculate pointer offset
            int* p = (int*)(scan0 + (CoordsToIndex(x, y, stride)));

            //if the pixel is within the color tolerance, fill it and branch out
            if (!(PixelsChecked[x, y]) && CheckPixel((byte*)p, boundarycolor)) {
                p[0] = m_fillcolor; 	 //fill with the color
                PixelsChecked[x, y] = true;

                CheckQueue.Enqueue(new Point(x + 1, y));
                CheckQueue.Enqueue(new Point(x, y + 1));
                CheckQueue.Enqueue(new Point(x - 1, y));
                CheckQueue.Enqueue(new Point(x, y - 1));
            }
        }

        //*********
        //HELPER FUNCTIONS
        //*********

        ///<summary>Sees if a pixel is within the color tolerance range.</summary>
        //px - a pointer to the pixel to check
        unsafe bool CheckPixel(byte* px, byte* boundarycolor) {
            bool ret = true;

            for (byte i = 0; i <= 3; i++)
                ret &= (px[i] == boundarycolor[i]);

            return !ret;
        }

        ///<summary>Given X and Y coordinates and the bitmap's stride, returns a pointer offset</summary>
        int CoordsToIndex(int x, int y, int stride) {
            return (stride * y) + (x * 4);
        }
        /*public Bitmap[] drawSlices(RAWImage rawImage) {
    int h, s, v, vi;
    byte r, g, b;
    //int row, col;
    int i;
    int offset;
    int HSdataLength = 360 * 100 * 4;
    Bitmap[] bmpSlices = new Bitmap[100 / V_SLICE_DEPTH];
    BitmapData[] bmpdSlices = new BitmapData[100 / V_SLICE_DEPTH];
    IntPtr[] ptrSlices = new IntPtr[100 / V_SLICE_DEPTH];
    byte[][] bSlices = new byte[100 / V_SLICE_DEPTH][];


    for (i = 0; i < 100 / V_SLICE_DEPTH; i++) {
        bmpSlices[i] = new Bitmap(360, 100, PixelFormat.Format32bppArgb);

        bmpdSlices[i] = bmpSlices[i].LockBits(new Rectangle(0, 0, bmpSlices[i].Width, bmpSlices[i].Height),
                                        ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        ptrSlices[i] = bmpdSlices[i].Scan0;

        bSlices[i] = new byte[HSdataLength];
    }

    int x, y;
    i = 0;
    do {

        y = i / (rawImage.Width * 3);
        // x = (i - y * (rawImage.Width * 3)) / 3;
        x = (i % (rawImage.Width * 3)) / 3;

        if (x >= rawImage.region_left && x <= rawImage.region_right &&
            y >= rawImage.region_top && y <= rawImage.region_bottom) {

            b = rawImage.RawData[i];
            i++;
            g = rawImage.RawData[i];
            i++;
            r = rawImage.RawData[i];
            i++;


            //if (r > 20 && g > 20 && b > 20) {
            RGBtoHSV(r, g, b, out h, out s, out v);
            vi = v / V_SLICE_DEPTH;

            offset = (s * 360 + h) * 4;

            bSlices[vi][offset] = b;
            offset++;
            bSlices[vi][offset] = g;
            offset++;
            bSlices[vi][offset] = r;
            offset++;
            bSlices[vi][offset] = 255;
            //offset++;
            // }
        }
        else {
            i += 3;
        }
    } while (i < rawImage.RawDataLength);

    for (i = 0; i < 100 / V_SLICE_DEPTH; i++) {
        Marshal.Copy(bSlices[i], 0, ptrSlices[i], HSdataLength);
        bmpSlices[i].UnlockBits(bmpdSlices[i]);
    }

    return bmpSlices;

}*/

        /*public void populateGraphPanels(Bitmap[] bmpSlices) {
            int i;


            for (i = 0; i < 100 / V_SLICE_DEPTH; i++) {

                slicesForms[i]._bmpSlice = new Bitmap(360, 100);
                //   slicesForms[i].bmpOrigPixels = drawSliceWithOrigPixels(slicesForms[i].V);
                slicesForms[i]._bmpOrigPixels = bmpSlices[i];

                slicesForms[i].panSlice.BackgroundImage = slicesForms[i]._bmpOrigPixels;

                slicesForms[i]._colorCalibrator = this;

                //frmColorCalibObj.thumbPanels[i].BackgroundImage = bmpSlice;
            }

            //frmColorCalibObj.updateThumbs();


        }*/
    }
}