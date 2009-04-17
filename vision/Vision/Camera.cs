using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using VisionStatic;

namespace VisionStatic
{
    public static class ImageSettings
    {
        public const int IMAGE_RES_X = 1024;
        public const int IMAGE_RES_Y = 768;
    }
}

namespace VisionCamera {
    public interface ICamera
    {        
        int FrameCount
        {
            get;
        }

        int startCapture();
        int stopCapture();
        int getOneImage(out Vision.RAWImage image);
        int getFrame(out Vision.RAWImage image);
        void resetFrameCount();
    }

    
    public unsafe class PGRCamera : ICamera {
        public struct RGBQUAD
        {
            public byte rgbBlue;
            public byte rgbGreen;
            public byte rgbRed;
            public byte rgbReserved;
        }

        public struct BITMAPINFOHEADER
        {
            public int biSize;
            public int biWidth;
            public int biHeight;
            public short biPlanes;
            public short biBitCount;
            public int biCompression;
            public int biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public int biClrUsed;
            public int biClrImportant;
        }

        public struct BITMAPINFO
        {
            public BITMAPINFOHEADER bmiHeader;
            public RGBQUAD bmiColors;
        }
      

        #region DLL Imports
        [DllImport("pgrflycapture.dll")]
        public static extern int flycaptureCreateContext(int* flycapcontext);

        [DllImport("pgrflycapture.dll")]
        public static extern int flycaptureStart(int flycapcontext,
            FlyCaptureVideoMode videoMode,
            FlyCaptureFrameRate frameRate);

        [DllImport("pgrflycapture.dll")]
        public static extern string flycaptureErrorToString(int error);

        [DllImport("pgrflycapture.dll")]
        public static extern int flycaptureInitialize(int flycapContext,
            int cameraIndex);

        [DllImport("pgrflycapture.dll")]
        public static extern int flycaptureGetCameraInformation(int flycapContext,
            ref FlyCaptureInfo arInfo);

        [DllImport("pgrflycapture.dll")]
        unsafe public static extern int flycaptureGrabImage2(int flycapContext,
            ref FlyCaptureImage image);

        [DllImport("pgrflycapture.dll")]
        unsafe public static extern int flycaptureSaveImage(int flycapContext,
            ref FlyCaptureImage image, string filename,
            FlyCaptureImageFileFormat fileFormat);

        [DllImport("pgrflycapture.dll")]
        public static extern int flycaptureStop(int flycapContext);

        [DllImport("pgrflycapture.dll")]
        public static extern int flycaptureDestroyContext(int flycapContext);

        [DllImport("pgrflycapture.dll")]
        public static extern int flycaptureConvertImage(int flycapContext,
            ref FlyCaptureImage image, ref FlyCaptureImage imageConvert);

        [DllImport("gdi32.dll")]
        public static extern int CreateDIBSection(int* hDC,
            ref BITMAPINFO pBitmapInfo, int un, ref byte* lplpVoid,
            int handle, int dw);
        #endregion

        #region Constants
        // Bitmap constant
        public const short DIB_RGB_COLORS = 0;

        // The index of the camera to grab from.
        public const int _CAMERA_INDEX = 0;

        // The maximum number of cameras on the bus.
        public const int _MAX_CAMS = 3;

        // The number of images to grab.
        public const int _IMAGES_TO_GRAB = 10;

        public const int COLS = ImageSettings.IMAGE_RES_X;
        public const int ROWS = ImageSettings.IMAGE_RES_Y;
        public const int FORMAT_FACTOR = 3;
        #endregion


        int flycapContext;
        int ret;
        FlyCaptureInfo flycapInfo;
        FlyCaptureImage pgrImage;
        FlyCaptureImage flycapRGBImage;
        Boolean started;
        byte[] rawData;
        int nBytes;
        Object _frameCountLock = new Object();
        int _frameCount;

        Vision.RAWImage cameraImage;

        public int FrameCount
        {
            get
            {
                int frameCount;
                lock (_frameCountLock)
                {
                    frameCount = _frameCount;
                }
                return frameCount;
            }
        }

        public PGRCamera() {

            flycapInfo = new FlyCaptureInfo();
            pgrImage = new FlyCaptureImage();
            flycapRGBImage = new FlyCaptureImage();
            nBytes = ROWS * COLS * FORMAT_FACTOR;	// for R,G,B
            rawData = new byte[nBytes];

            cameraImage = new Vision.RAWImage(rawData, COLS, ROWS, 1);

            // Create the context.
            fixed (int* fixedContext = &flycapContext) {
                ret = flycaptureCreateContext(fixedContext);
                if (ret != 0) {
                    reportError(ret, "flycaptureCreateContext");
                    return;
                }
            }

            // Initialize the camera.
            ret = flycaptureInitialize(flycapContext, _CAMERA_INDEX);
            if (ret != 0) {
                reportError(ret, "flycaptureInitialize");
                return;
            }

            // Get the info for this camera.
            ret = flycaptureGetCameraInformation(flycapContext, ref flycapInfo);
            if (ret != 0) {
                reportError(ret, "flycaptureGetCameraInformation");
                return;
            }

            if (flycapInfo.CameraType ==
                FlyCaptureCameraType.FLYCAPTURE_COLOR) {
                Console.Write("Model: Colour " + flycapInfo.pszModelString + "\n"
                    + "Serial #: " + flycapInfo.SerialNumber + "\n");
            }

            InitBitmapStructure(ROWS, COLS, ref flycapRGBImage);
            flycapRGBImage.pixelFormat = FlyCapturePixelFormat.FLYCAPTURE_BGR;

            resetFrameCount();

            started = false;
        }

        ~PGRCamera() {
            // Destroy the context.
            ret = flycaptureDestroyContext(flycapContext);
            if (ret != 0) {
                reportError(ret, "flycaptureDestroyContext");
                return;
            }

            Console.Write("\nPress Enter");

            Console.Read();
        }

        public int startCapture() {
            /**/
            // Start FlyCapture.
            if (!started) {
                ret = flycaptureStart(flycapContext,
                    FlyCaptureVideoMode.FLYCAPTURE_VIDEOMODE_ANY,
                    //FlyCaptureFrameRate.FLYCAPTURE_FRAMERATE_30);
                    FlyCaptureFrameRate.FLYCAPTURE_FRAMERATE_ANY);
                if (ret != 0) {
                    reportError(ret, "flycaptureStart");
                    return 1;
                }
                resetFrameCount();
                started = true;
            }
            return 0;
        }

        public int stopCapture() {
            if (started) {
                // Stop FlyCapture.
                ret = flycaptureStop(flycapContext);
                if (ret != 0) {
                    reportError(ret, "flycaptureStop");
                    return 1;
                }
                started = false;
            }
            return 0;
        }

        public int getOneImage(out Vision.RAWImage image) {
            bool startedOld = started;

            int rc;
            if (!started)
                if ((rc = startCapture()) > 0)
                {
                    image = null;
                    return rc;
                }

            if ((rc = getFrame(out image)) > 0)
            {
                image = null;
                return rc;
            }

            if (!startedOld)
                if ((rc = stopCapture()) > 0)
                {
                    image = null;
                    return rc;
                }

            return 0;
        }

        public int getFrame(out Vision.RAWImage image)
        {
            // check to make sure initialized
            if (!started)
            {
                image = null;
                return 1;
            }

           // Console.Write("\nGrabbing Images ");
            ret = flycaptureGrabImage2(flycapContext, ref pgrImage);
            if (ret != 0) {
                reportError(ret, "flycaptureGrabImage2");
                image = null;
                return 1;
            }
           // Console.Write(".");

            // Convert the image.
            // Sometimes the line below throws:
            // "Attempted to read or write protected memory. This is often an indication that other memory is corrupt."
           // try {
                ret = flycaptureConvertImage(flycapContext, ref pgrImage,
                    ref flycapRGBImage);
            //}
           // catch (AccessViolationException e) {
            //    Console.WriteLine("getFrame: AccessViolationException occured called flycaptureConvertImage(). Skipping frame.");
             //   image = cameraImage;
            //    return 0;
           // }
            if (ret != 0) {
                reportError(ret, "flycaptureConvertImage");
                image = null;
                return 1;
            }

            Marshal.Copy((IntPtr)flycapRGBImage.pData, rawData, 0, nBytes);

            //return new RAWImage(rawData, image.iCols, image.iRows, 1);
            image = cameraImage;

            lock (_frameCountLock)
            {
                _frameCount++;
            }

            return 0;


            /*// Save the image.
            Console.Write("\nSaving Last Image ");
            ret = flycaptureSaveImage(flycapContext, ref flycapRGBImage, "raw.bmp",
                FlyCaptureImageFileFormat.FLYCAPTURE_FILEFORMAT_BMP);
            if (ret != 0)
            {
                reportError(ret, "flycaptureSaveImage");
                return;
            }
            else
                System.Diagnostics.Process.Start("mspaint.exe", "raw.bmp");*/

        }

        public void resetFrameCount() {
            lock(_frameCountLock) {
                _frameCount = 0;
            }
        }

        private void InitBitmapStructure(int nRows, int nCols,
            ref FlyCaptureImage flycapRGBImage)
        {

            BITMAPINFO bmi = new BITMAPINFO();	// bitmap header
            byte* pvBits = null; //	pointer	to DIB section


            bmi.bmiHeader.biSize = 40; // sizeof(BITMAPINFOHEADER) = 40
            bmi.bmiHeader.biWidth = nCols;
            bmi.bmiHeader.biHeight = nRows;
            bmi.bmiHeader.biPlanes = 1;
            bmi.bmiHeader.biBitCount = 24; // three	8-bit components
            bmi.bmiHeader.biCompression = 0; //	BI_RGB = 0
            bmi.bmiHeader.biSizeImage = nBytes;
            CreateDIBSection(null, ref bmi, DIB_RGB_COLORS,
                ref pvBits, 0, 0);

            // This	is where we	set	the	"to	be converted" image	data pointer 
            // to our newly created bitmap data pointer
            flycapRGBImage.pData = pvBits;
        }

        private void reportError(int ret, string fname)
        {
            
            Console.Write(fname + " error: " + flycaptureErrorToString(ret) + "\n");
            //Console.Write("\nPress Enter");
            //Console.Read(); 
            //throw new ApplicationException(fname + " error: " + flycaptureErrorToString(ret) + "\n");
            
        }
    }

    public class SeqCamera : ICamera
    {       

        private string _sequence = "";
        private int _frame = -1;
        private int _startFrame = -1;
        private bool _repeat = false;
        private int _sleepTime = 0; // in ms;
        private int _frameCount = 0;
        private Object _frameCountLock = new Object();

        // Properties
        public bool Repeat
        {
            get { return _repeat; }
            set { _repeat = value; }
        }
        public int Frame
        {
            get { return _frame; }
            set { _frame = value; }
        }
        public int StartFrame
        {
            get { return _startFrame; }
            set { _startFrame = value; }
        }
        public string Sequence
        {
            get { return _sequence; }
            set { _sequence = value; }
        }
        public int SleepTime
        {
            get { return _sleepTime; }
            set { _sleepTime = value; }
        }
        public int FrameCount
        {
            get
            {
                int frameCount;
                lock (_frameCountLock)
                {
                    frameCount = _frameCount;
                }
                return frameCount;
            }
        }

        // Methods
        public int startCapture()
        {
            if (_frame < 0)
                return 1;
            return 0;
        }
        public int stopCapture()
        {
            // nothing to be done
            return 0;
        }
        public int getOneImage(out Vision.RAWImage image)
        {
            return getFrame(out image);
        }
        public int getFrame(out Vision.RAWImage image)
        {
            return getFrame(out image, 1);
        }
        public int getFrame(out Vision.RAWImage image, int increment)
        {
            if (_frame < 0)
            {
                image = null;
                return 1;
            }
            Thread.Sleep(_sleepTime);

            _frame += increment;
            string nextFrameFile = _sequence + _frame.ToString() + ".bmp";
            if (!File.Exists(nextFrameFile))
            {
                if (_repeat) {
                    _frame = _startFrame;
                    nextFrameFile = _sequence + _frame.ToString() + ".bmp";
                } else {
                    _frame -= increment;
                    image = null;
                    return 2;
                }
            }
            image = new Vision.RAWImage(nextFrameFile);

            lock (_frameCountLock)
            {
                _frameCount++;
            }

            return 0;
        }

        public void resetFrameCount()
        {
            lock (_frameCountLock)
            {
                _frameCount = 0;
            }
        }

    }
}
