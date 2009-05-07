using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using VisionStatic;
using VisionCamera;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Robocup.Utilities;
using Robocup.Core;

namespace Vision
{
    //public delegate void OnNewStateReady(GameObjects gameObjects);
    public delegate void OnNewStateReady(VisionMessage visionMessage);
    public delegate void VoidDelegate();

    public class Run {

        public int LeftEnd, RightEnd;
        public int Length;
        public int Row;

        public int AvgColorR, AvgColorG, AvgColorB;
        public byte ColorClass;

        public Run Parent;
        public int BlobId;
 
        public Run(int _LeftEnd, int _Length, int _Row, 
                   int _AvgColorR, int _AvgColorB, int _AvgColorG, byte _ColorClass) {
            LeftEnd = _LeftEnd;
            Length = _Length;
            RightEnd = LeftEnd + Length - 1;
            Row = _Row;
            AvgColorR = _AvgColorR;
            AvgColorG = _AvgColorG;
            AvgColorB = _AvgColorB;
            ColorClass = _ColorClass;
            BlobId = -1;
        }
    }

    public class Blob {     

        public int Left, Right, Top, Bottom;

        public int CenterX, CenterY;
        public double CenterWorldX, CenterWorldY;

        public int Area;
        public float AreaScaled;

        public int AvgColorR, AvgColorG, AvgColorB;
        public byte ColorClass;

        public int BlobID;

        private int _avgColorSumR, _avgColorSumG, _avgColorSumB;
        private int _centerXSum, _centerYSum;

        public Blob(byte _colorClass) {
            Left = ImageSettings.IMAGE_RES_X * 3; // just need a maximum value, it's ok for image to be of different dimesions
            Right = 0;
            Top = ImageSettings.IMAGE_RES_Y * 3;
            Bottom = 0;
            Area = 0;
            ColorClass = _colorClass;
            //CenterXSum = 0;
            //CenterYSum = 0;
            BlobID = -1;
        }

        public void AddRun(Run run) {
            Left = (run.LeftEnd < Left) ? run.LeftEnd : Left;
            Right = (run.RightEnd > Right) ? run.RightEnd : Right;

            Top = (run.Row < Top) ? run.Row : Top;
            Bottom = (run.Row > Bottom) ? run.Row : Bottom;

            Area += run.Length;

            _avgColorSumR += run.AvgColorR * run.Length;
            _avgColorSumG += run.AvgColorG * run.Length;
            _avgColorSumB += run.AvgColorB * run.Length;

            _centerXSum += ((run.LeftEnd + run.RightEnd) * run.Length) / 2;
            _centerYSum += run.Row * run.Length;

        }
        public void CalcAvgColor() {
            AvgColorR = _avgColorSumR / Area;
            AvgColorG = _avgColorSumG / Area;
            AvgColorB = _avgColorSumB / Area;
        }
        public void CalcCenter() {
            CenterX = _centerXSum / Area;
            CenterY = _centerYSum / Area;
        }
    }

    class BlobAreaComparer : System.Collections.IComparer {
        int area1, area2;
        public int Compare(object b1, object b2) {
            
            if (b1 == null) {
                return 1;
            }
            if (b2 == null) {
                return -1;
            }

            area1 = ((Blob)b1).Area;
            area2 = ((Blob)b2).Area;
            if (area1 > area2) {
                return 1;
            } else if (area2 > area1) {
                return -1;
            } else {
                return 0;
            }
        }
    }
    class   BlobAreaScaledComparer : System.Collections.Generic.IComparer<Blob>
    {
        float area1, area2;
        public int Compare(Blob b1, Blob b2)
        {

            if (b1 == null)
            {
                return 1;
            }
            if (b2 == null)
            {
                return -1;
            }

            area1 = b1.AreaScaled;
            area2 = b2.AreaScaled;
            if (area1 > area2)
            {
                return 1;
            }
            else if (area2 > area1)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }

    public class Blobber {

        private int MIN_BLOB_AREA;
        private int MIN_BLOB_HEIGHT;
        private int MIN_BLOB_WIDTH;
        private int MAX_BLOB_HEIGHT;
        private int MAX_BLOB_WIDTH;
        private int CAMERA_ID;

        private bool _paramsLoaded = false;

        public Run[] runs;

        public int totalRuns;
        public int[] numRunsInRow;
        public Blob[] blobs;
        public int totalBlobs;

        private TsaiCalibrator _tsaiCalibrator;
        private ColorCalibrator _colorCalibrator;
        private ICamera _camera;

        private ImageForm _imageForm;
        

        private BlobAreaComparer blobComparer;

        private bool _blobbing;

        private OnNewStateReady _onNewStateReady;

        
        //private delegate void ProcessFrameDelegate(RAWImage frame, Rectangle region, out GameObjects gameObjects);
        private delegate void ProcessFrameDelegate(RAWImage frame, Rectangle region, out VisionMessage visionMessage);
        
        private VoidDelegate _visionLoopDelegate;
        private AsyncCallback _errorHandler;
        private VoidDelegate _userErrorHandler;
        private IAsyncResult _visionLoopHandle;


        public bool ParamsLoaded {
            get { return _paramsLoaded; }
        }

        public bool Blobbing {
            get { return _blobbing; }
        }

        public Blobber(ColorCalibrator colorCalibrator, TsaiCalibrator tsaiCalibrator, 
            ImageForm imageForm, VoidDelegate userErrorHandler) {
            
            _tsaiCalibrator = tsaiCalibrator;
            _colorCalibrator = colorCalibrator;
            _userErrorHandler = userErrorHandler;

            _imageForm = imageForm;

            blobComparer = new BlobAreaComparer();

            _visionLoopDelegate = new VoidDelegate(VisionLoop);
            _errorHandler = new AsyncCallback(ErrorHandler);
          
            // this is not size of current image, this is the MAXIMUM size of the images
            // that the application would ever process, just making sure to allocate enough memory
            runs = new Run[ImageSettings.IMAGE_RES_X * ImageSettings.IMAGE_RES_Y];
            blobs = new Blob[ImageSettings.IMAGE_RES_X * ImageSettings.IMAGE_RES_Y];
            numRunsInRow = new int[ImageSettings.IMAGE_RES_Y]; 
        }

        public void SetCamera(ICamera camera)
        {
            _camera = camera;
        }

        public void ReloadParameters()
        {
            _paramsLoaded = false;

            MIN_BLOB_AREA = Constants.get<int>("vision", "MIN_BLOB_AREA");
            MIN_BLOB_HEIGHT = Constants.get<int>("vision", "MIN_BLOB_WIDTH");
            MIN_BLOB_WIDTH = Constants.get<int>("vision", "MIN_BLOB_WIDTH");
            MAX_BLOB_HEIGHT = Constants.get<int>("vision", "MAX_BLOB_HEIGHT");
            MAX_BLOB_WIDTH = Constants.get<int>("vision", "MAX_BLOB_WIDTH");

            CAMERA_ID = Constants.get<int>("vision", "CAMERA_ID");

            _paramsLoaded = true;
        }

        public void Start(OnNewStateReady onNewStateReadyDelegate) {
            _onNewStateReady = onNewStateReadyDelegate;

            _blobbing = true;
            
            int rc;
            if ((rc = _camera.startCapture()) > 0)
            {
                _blobbing = false;
                throw new Exception("Failed to prepare camera for capturing: " +
                                    "startCapture() returned " + rc.ToString());
            }
            

            // go into the blobbing loop on a separate thread 
            // the visionloop thread should not finish by itself, so if it does
            _visionLoopHandle = _visionLoopDelegate.BeginInvoke(_errorHandler, null);
        }
        
        public void Stop() {
            _blobbing = false;

            const int TIMEOUT = 10;
            int time = 0;
            while (!_visionLoopHandle.IsCompleted && time < TIMEOUT) {
                Thread.Sleep(10);
                time++;
            }

            _camera.stopCapture();

            _visionLoopDelegate.EndInvoke(_visionLoopHandle);

            //if (startVisionDel != null) {
            //    startVisionDel.EndInvoke(arStartVision);
            //    startVisionDel = null;
           // }

        }
        public void VisionLoop() {
            //GameObjects irrelevant = null;
            VisionMessage irrelevant = null;
            RAWImage rawImage;
            IAsyncResult processFrameHandle = null;

            // these only need to be created once
            
            ProcessFrameDelegate processFrameDelegate = new ProcessFrameDelegate(ProcessFrame);
            AsyncCallback frameProcessedCallback = new AsyncCallback(FrameProcessedCallback);

            Rectangle region = _imageForm.GetRegion();

            while (_blobbing) {
                //make sure blobbing finished
                while (processFrameHandle != null && !processFrameHandle.IsCompleted) {
                    Thread.Sleep(5);
                }

                //Console.WriteLine("Getting image...");
                int rc = _camera.getFrame(out rawImage);
                if (rc > 0) {
                    Console.WriteLine("VisionLoop: getFrame() failed with return code: " + rc.ToString());
                    continue; // try and get another thread
                }
                //Console.WriteLine("Got image...");

               processFrameHandle = processFrameDelegate.BeginInvoke(rawImage, region, out irrelevant, frameProcessedCallback, null);
            }
        }
        
        //private void ProcessFrame(RAWImage frame, Rectangle region, out GameObjects gameObjects) {
        private void ProcessFrame(RAWImage frame, Rectangle region, out VisionMessage visionMessage) {

            //Console.WriteLine("Processing frame...");
            
            doBlob(frame, region);
            visionMessage = VisionStatic.RobotFinder.findGameObjects2(blobs, totalBlobs, _tsaiCalibrator);

          /*  if (visionMessage.OurRobots.Count <= 0)
            {
                Console.WriteLine("NO ROBOT!!!!");
                string WORK_DIR = "../../resources/vision/";
                TextWriter twr = new StreamWriter(WORK_DIR + "bad_frames.txt", true); // append
                twr.WriteLine(((SeqCamera)_camera).Frame);
                twr.Close();
            }*/

            //Console.WriteLine("Frame processed. Blobs found: " + totalBlobs.ToString());

        }

        private void FrameProcessedCallback(IAsyncResult processFrameHandle) {
            //GameObjects gameObjects = new GameObjects();
            VisionMessage visionMessage = new VisionMessage(CAMERA_ID);

            // Extract the delegate from the AsyncResult.  
            ProcessFrameDelegate processFrameDelegate = (ProcessFrameDelegate)((AsyncResult)processFrameHandle).AsyncDelegate;
            
            // Obtain the resultF
            processFrameDelegate.EndInvoke(out visionMessage, processFrameHandle);

            //try
            //{
                _onNewStateReady.DynamicInvoke(new object[] { visionMessage });
            //} 
            //catch (Exception)
            //{
            //}
        }

        private void ErrorHandler(IAsyncResult res)
        {
            _blobbing = false;
            _camera.stopCapture();
            _userErrorHandler.BeginInvoke(null, null);
        }


        private void CleanContainers() {
            int i = 0;

            do {
                runs[i] = null;
                i++;
            } while (i < totalRuns);
            
            i = 0;
            do {
                blobs[i] = null;
                i++;
            } while (i < totalBlobs);
            i = 0;
            do {
                numRunsInRow[i] = 0;
                i++;
            } while (i < ImageSettings.IMAGE_RES_Y); // see constructor for explication of ImageSettings.IMAGE_RES_Y
        }

        public void doBlob(RAWImage rawImage, Rectangle region) {
            int row, col;
            int i, j;
            int runLeftEnd;
            int runLength;

            int avgColorR, avgColorG, avgColorB;

            Run root1, root2;
            Run temp_root1, temp_root2;
            Run temp_root1_par, temp_root2_par;

            int maxInRowI, maxInRowJ;

            Run topParent;
            int topParentsBlobId;
            Blob newBlob;

            int totalBlobs_all;
            DPoint centerWorld;
            int width, height;

            byte[] data = rawImage.rawData;

            CleanContainers();

            //Console.WriteLine(timer.Measure("Blob() all three passes:", delegate {
           // Console.WriteLine(timer.Measure("Blob() 1st pass:", delegate {

                

            byte[] RGBtoCCTable = _colorCalibrator.RGBtoCCTable;

                byte prevColorClass, currColorClass;


                totalRuns = 0;
                i = 0;

                //for (row = 0; row < rawImage.Height; row++) {


                //row = rawImage.region_top;
                row = region.Top;

                do {

                    numRunsInRow[row] = 0;

                    //for (col = 0; col < rawImage.Width - 1; col++) {
                    //col = rawImage.region_left;
                    col = region.Left;

                    i = row * rawImage.Width * 3 + col * 3;

                    avgColorR = data[i + 2];
                    avgColorG = data[i + 1];
                    avgColorB = data[i];

                    //runLeftEnd = rawImage.region_left;
                    runLeftEnd = region.Left;
                    runLength = 0;

                    //old: prevColorClass = colorLookup[data[i] * 65536 + data[i + 1] * 256 + data[i + 2]];
                    //prevColorClass = colorLookup[data[i + 2] * 65536 + data[i + 1] * 256 + data[i]];
                    //prevColorClass = _colorCalibrator.RGBtoCCTable[data[i + 2] * 65536 + data[i + 1] * 256 + data[i]];
                    prevColorClass = RGBtoCCTable[data[i + 2] * 65536 + data[i + 1] * 256 + data[i]];

                    do {

                        //old: currColorClass = colorLookup[data[i + 3] * 65536 + data[i + 4] * 256 + data[i + 5]];
                        //currColorClass = colorLookup[data[i + 5] * 65536 + data[i + 4] * 256 + data[i + 3]];
                        //currColorClass = _colorCalibrator.RGBtoCCTable[data[i + 5] * 65536 + data[i + 4] * 256 + data[i + 3]];
                        currColorClass = RGBtoCCTable[data[i + 5] * 65536 + data[i + 4] * 256 + data[i + 3]];
                        if (prevColorClass == currColorClass) {
                            runLength += 1;

                            avgColorR += data[i + 5];
                            avgColorG += data[i + 4];
                            avgColorB += data[i + 3];
                        } else {

                            runLength += 1;
                            avgColorR = (avgColorR / runLength);
                            avgColorG = (avgColorG / runLength);
                            avgColorB = (avgColorB / runLength);

                            runs[totalRuns] = new Run(runLeftEnd, runLength, row, avgColorR, avgColorB, avgColorG, prevColorClass);
                            numRunsInRow[row]++;
                          
                            totalRuns++;
                            avgColorR = data[i + 5];
                            avgColorG = data[i + 4];
                            avgColorB = data[i + 3];


                            runLeftEnd = col + 1;
                            runLength = 0;

                        }

                        i += 3;
                        prevColorClass = currColorClass;

                        col += 1;
                   // } while (col < rawImage.region_right - 1);
                    } while (col < region.Right - 1);

                    runLength += 1;

                    avgColorR = (avgColorR / runLength);
                    avgColorG = (avgColorG / runLength);
                    avgColorB = (avgColorB / runLength);

                    runs[totalRuns] = new Run(runLeftEnd, runLength, row, avgColorR, avgColorB, avgColorG, prevColorClass);
                    numRunsInRow[row]++;
                    totalRuns++;

                    i += 3;
                    row++;
                //} while (row < rawImage.region_bottom);
                } while (row < region.Bottom);


         //   }));

        //    Console.WriteLine(timer.Measure("Blob() 2nd pass:", delegate {

                //second pass - build tree of runs

                

                i = 0;
                j = numRunsInRow[runs[0].Row];
                do {

                    maxInRowI = i + numRunsInRow[runs[i].Row];
                    maxInRowJ = j + numRunsInRow[runs[j].Row];

                    do {

                        if (runs[i].ColorClass > 1 && runs[j].ColorClass > 1) {
                            if (runs[i].ColorClass == runs[j].ColorClass) {
                                if (runs[j].Parent == null) {
                                    if (i != j) {  //??
                                        runs[j].Parent = runs[i];
                                    }
                                } else {
                                    //get possible two root-parents
                                    root1 = runs[j];
                                    while (root1.Parent != null) {
                                        root1 = root1.Parent;
                                    }

                                    temp_root1 = runs[j];

                                    while (temp_root1.Parent != null) {
                                        temp_root1_par = temp_root1.Parent;
                                        temp_root1.Parent = root1;
                                        temp_root1 = temp_root1_par;
                                    }

                                    root2 = runs[i];
                                    while (root2.Parent != null) {
                                        root2 = root2.Parent;
                                    }


                                    temp_root2 = runs[i];
                                    while (temp_root2.Parent != null) {
                                        temp_root2_par = temp_root2.Parent;
                                        temp_root2.Parent = root2;
                                        temp_root2 = temp_root2_par;
                                    }

                                    if (root1 != root2) {
                                        //set parent of lower of the two roots be the higher of the two roots
                                        if (root1.Row < root2.Row) {
                                            root2.Parent = root1;
                                        } else {
                                            if (root2.Row < root1.Row) {
                                                root1.Parent = root2;
                                            } else {
                                                //root1.Row == root2.Row
                                                if (root1.LeftEnd < root2.LeftEnd) {
                                                    root2.Parent = root1;
                                                } else {
                                                    // root2.LeftEnd < root1.LeftEnd
                                                    root1.Parent = root2;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (runs[i].RightEnd > runs[j].RightEnd) {
                            j++;
                        } else {
                            if (runs[j].RightEnd > runs[i].RightEnd) {
                                i++;
                            } else {
                                i++;
                                j++;
                            }
                        }
                    } while (i < maxInRowI && j < maxInRowJ);

                } while (j < totalRuns);

           // }));

           // Console.WriteLine(timer.Measure("Blob() 3rd pass:", delegate {

                //third pass: create blobs


                totalBlobs = 0;
                i = 0;
                do {
                    if (runs[i].ColorClass > 1) {
                        if (runs[i].Parent == null) {
                            runs[i].BlobId = totalBlobs;
                            newBlob = new Blob(runs[i].ColorClass);
                            newBlob.BlobID = totalBlobs;
                            newBlob.AddRun(runs[i]);
                            blobs[totalBlobs] = newBlob;
                            totalBlobs++;

                        } else {
                            //lookup topmost parent
                            topParent = runs[i];
                            while (topParent.Parent != null) {
                                topParent = topParent.Parent;
                            }
                            topParentsBlobId = topParent.BlobId;
                            runs[i].BlobId = topParentsBlobId;
                            blobs[topParentsBlobId].AddRun(runs[i]);
                        }
                    }
                    i++;
                } while (i < totalRuns);


                
                totalBlobs_all = totalBlobs;
                                
                i = 0;

                while (i < totalBlobs_all) {
                    height = blobs[i].Bottom - blobs[i].Top;
                    width = blobs[i].Right - blobs[i].Left;
                    if (blobs[i].Area >= MIN_BLOB_AREA && 
                        height >= MIN_BLOB_HEIGHT && height <= MAX_BLOB_HEIGHT &&
                        width >= MIN_BLOB_WIDTH && width <= MAX_BLOB_WIDTH) {
                       

                        blobs[i].CalcAvgColor();
                        blobs[i].CalcCenter();

                        blobs[i].AreaScaled = _tsaiCalibrator.GetAreaScalingCoeff(blobs[i].CenterX, blobs[i].CenterY) * blobs[i].Area;
                    
                        centerWorld = _tsaiCalibrator.imgToWorldLookup[blobs[i].CenterY * rawImage.Width + blobs[i].CenterX];
                        blobs[i].CenterWorldX = centerWorld.wx;
                        blobs[i].CenterWorldY = centerWorld.wy;

                    } else {
                        blobs[i] = null;
                        totalBlobs--;
                    }
                    i++;
                };

            //}));

            //sort blob array by Area
            
            Array.Sort(blobs, 0, totalBlobs_all, blobComparer);
            
            
        }
    }
}
