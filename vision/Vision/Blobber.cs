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

namespace VisionStatic {

   public static class Field {
        public const int WIDTH = 1024;
        public const int HEIGHT = 768;
    }

}
namespace Vision
{
    public delegate void OnNewStateReady(GameObjects gameObjects);

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
            Left = VisionStatic.Field.WIDTH * 3;
            Right = 0;
            Top = VisionStatic.Field.HEIGHT;
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

    class BlobComparer : System.Collections.IComparer {
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

    public class Blobber {

        private int MIN_BLOB_AREA;
        private int MIN_BLOB_HEIGHT;
        private int MIN_BLOB_WIDTH;
        private int MAX_BLOB_HEIGHT;
        private int MAX_BLOB_WIDTH;
        

        public Run[] runs;

        public int totalRuns;
        public int[] numRunsInRow;
        public Blob[] blobs;
        public int totalBlobs;

        private TsaiCalibrator _tsaiCalibrator;
        private ColorCalibrator _colorCalibrator;
        private Camera _camera;

        private ImageForm _imageForm;
        

        private BlobComparer blobComparer;

        private bool _blobbing;

        private OnNewStateReady _onNewStateReady;

        private delegate void VisionLoopDelegate();
        private delegate void ProcessFrameDelegate(RAWImage frame, Rectangle region, out GameObjects gameObjects);

        private VisionLoopDelegate _visionLoopDelegate;
        private IAsyncResult _visionLoopHandle;
        

        public bool Blobbing {
            get { return _blobbing; }
        }

        public Blobber(ColorCalibrator colorCalibrator, TsaiCalibrator tsaiCalibrator, 
            Camera camera, ImageForm imageForm) {
            
            _tsaiCalibrator = tsaiCalibrator;
            _colorCalibrator = colorCalibrator;
            _camera = camera;

            _imageForm = imageForm;

            blobComparer = new BlobComparer();

            _visionLoopDelegate = new VisionLoopDelegate(VisionLoop);

            runs = new Run[VisionStatic.Field.HEIGHT * VisionStatic.Field.WIDTH];
            blobs = new Blob[VisionStatic.Field.HEIGHT * VisionStatic.Field.WIDTH];
            numRunsInRow = new int[VisionStatic.Field.HEIGHT]; 
        }

        public void ReloadParameters()
        {
                    MIN_BLOB_AREA = Constants.get<int>("MIN_BLOB_AREA");
                    MIN_BLOB_HEIGHT = Constants.get<int>("MIN_BLOB_WIDTH"); ;
                    MIN_BLOB_WIDTH = Constants.get<int>("MIN_BLOB_WIDTH"); ;
                    MAX_BLOB_HEIGHT = Constants.get<int>("MAX_BLOB_HEIGHT"); ;
                    MAX_BLOB_WIDTH = Constants.get<int>("MAX_BLOB_WIDTH"); ;
        }

        public void Start(OnNewStateReady onNewStateReadyDelegate) {
            _onNewStateReady = onNewStateReadyDelegate;

            _blobbing = true;
            
            _camera.startCapture();

            // go into the blobbing loop on a separate thread
            _visionLoopHandle = _visionLoopDelegate.BeginInvoke(null, null);
        }
        public void Stop() {
            _blobbing = false;

            while (!_visionLoopHandle.IsCompleted) {
                Thread.Sleep(10);
            }

            _camera.stopCapture();

            _visionLoopDelegate.EndInvoke(_visionLoopHandle);

            //if (startVisionDel != null) {
            //    startVisionDel.EndInvoke(arStartVision);
            //    startVisionDel = null;
           // }

        }
        public void VisionLoop() {
            GameObjects irrelevant = null;
            RAWImage rawImage;
            IAsyncResult processFrameHandle = null;

            // these only need to be created once
            
            ProcessFrameDelegate processFrameDelegate = new ProcessFrameDelegate(ProcessFrame);
            AsyncCallback frameProcessedCallback = new AsyncCallback(FrameProcessedCallback);

            Rectangle region = _imageForm.GetRegion();

            while (_blobbing) {
                //Console.WriteLine("Getting image...");
                rawImage = _camera.getFrame();
                //Console.WriteLine("Got image...");

                //make sure blobbing finished
                while (processFrameHandle != null && !processFrameHandle.IsCompleted) {
                    Thread.Sleep(5);
                }
                processFrameHandle = processFrameDelegate.BeginInvoke(rawImage, region, out irrelevant, frameProcessedCallback, null);
            }
        }
        
        private void ProcessFrame(RAWImage frame, Rectangle region, out GameObjects gameObjects) {

            //Console.WriteLine("Processing frame...");
            
            doBlob(frame, region);
            gameObjects = VisionStatic.RobotFinder.findGameObjects(blobs, totalBlobs, _tsaiCalibrator);

            //Console.WriteLine("Frame processed. Blobs found: " + totalBlobs.ToString());

        }

        private void FrameProcessedCallback(IAsyncResult processFrameHandle) {
            GameObjects gameObjects = new GameObjects();

            // Extract the delegate from the AsyncResult.  
            ProcessFrameDelegate processFrameDelegate = (ProcessFrameDelegate)((AsyncResult)processFrameHandle).AsyncDelegate;
            
            // Obtain the result
            processFrameDelegate.EndInvoke(out gameObjects, processFrameHandle);

            try
            {
                _onNewStateReady.DynamicInvoke(new object[] { gameObjects });
            } 
            catch (Exception)
            {
            }
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
            } while (i < VisionStatic.Field.HEIGHT);
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

                //for (row = 0; row < VisionStatic.Field.HEIGHT; row++) {


                //row = rawImage.region_top;
                row = region.Top;

                do {

                    numRunsInRow[row] = 0;

                    //for (col = 0; col < VisionStatic.Field.WIDTH - 1; col++) {
                    //col = rawImage.region_left;
                    col = region.Left;

                    i = row * VisionStatic.Field.WIDTH * 3 + col * 3;

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
                    
                        centerWorld = _tsaiCalibrator.imgToWorldLookup[blobs[i].CenterY * VisionStatic.Field.WIDTH + blobs[i].CenterX];
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
