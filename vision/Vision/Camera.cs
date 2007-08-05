using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;


public struct RGBQUAD {
    public byte rgbBlue;
    public byte rgbGreen;
    public byte rgbRed;
    public byte rgbReserved;
}

public struct BITMAPINFOHEADER {
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

public struct BITMAPINFO {
    public BITMAPINFOHEADER bmiHeader;
    public RGBQUAD bmiColors;
}


namespace VisionCamera {
    public unsafe class Camera {
        //dll imports

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


        // Bitmap constant
        public const short DIB_RGB_COLORS = 0;

        // The index of the camera to grab from.
        public const int _CAMERA_INDEX = 0;

        // The maximum number of cameras on the bus.
        public const int _MAX_CAMS = 3;

        // The number of images to grab.
        public const int _IMAGES_TO_GRAB = 10;

        public const int COLS = 1024;
        public const int ROWS = 768;
        public const int FORMAT_FACTOR = 3;



        int flycapContext;
        int ret;
        FlyCaptureInfo flycapInfo;
        FlyCaptureImage image;
        FlyCaptureImage flycapRGBImage;
        Boolean started;
        byte[] rawData;
        int nBytes;

        Vision.RAWImage cameraImage;

        public Camera() {

            flycapInfo = new FlyCaptureInfo();
            image = new FlyCaptureImage();
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

            started = false;
        }

        ~Camera() {
            // Destroy the context.
            ret = flycaptureDestroyContext(flycapContext);
            if (ret != 0) {
                reportError(ret, "flycaptureDestroyContext");
                return;
            }

            Console.Write("\nPress Enter");

            Console.Read();
        }

        public void startCapture() {
            /**/
            // Start FlyCapture.
            if (!started) {
                ret = flycaptureStart(flycapContext,
                    FlyCaptureVideoMode.FLYCAPTURE_VIDEOMODE_ANY,
                    //FlyCaptureFrameRate.FLYCAPTURE_FRAMERATE_30);
                    FlyCaptureFrameRate.FLYCAPTURE_FRAMERATE_ANY);
                if (ret != 0) {
                    reportError(ret, "flycaptureStart");
                    return;
                }
                started = true;
            }
        }

        public void stopCapture() {
            if (started) {
                // Stop FlyCapture.
                ret = flycaptureStop(flycapContext);
                if (ret != 0) {
                    reportError(ret, "flycaptureStop");
                    return;
                }
                started = false;
            }
        }

        public void InitBitmapStructure(int nRows, int nCols,
            ref FlyCaptureImage flycapRGBImage) {

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

        public void reportError(int ret, string fname) {
            /*
            Console.Write(fname + " error: " + flycaptureErrorToString(ret) + "\n");
            Console.Write("\nPress Enter");
            Console.Read(); */
            throw new ApplicationException(fname + " error: " + flycaptureErrorToString(ret) + "\n");
            //return;
        }

        public Vision.RAWImage getOneImage() {
            bool startedOld = started;
            
            if (!started)
                startCapture();
            
            Vision.RAWImage image = getFrame();
            
            if (!startedOld)
                stopCapture();

            return image;
        }

        public Vision.RAWImage getFrame()
        {
            // check to make sure initialized
            if (!started) return null;

           // Console.Write("\nGrabbing Images ");
            ret = flycaptureGrabImage2(flycapContext, ref image);
            if (ret != 0) {
                reportError(ret, "flycaptureGrabImage2");
                return null;
            }
           // Console.Write(".");

            // Convert the image.

            ret = flycaptureConvertImage(flycapContext, ref image,
                ref flycapRGBImage);
            if (ret != 0) {
                reportError(ret, "flycaptureConvertImage");
                return null;
            }

            Marshal.Copy((IntPtr)flycapRGBImage.pData, rawData, 0, nBytes);

            //return new RAWImage(rawData, image.iCols, image.iRows, 1);
            return cameraImage;


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
    }
}
