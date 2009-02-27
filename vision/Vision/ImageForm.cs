using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using VisionStatic;
using Robocup.Utilities;
using System.IO;
using System.Drawing.Drawing2D;
using Robocup.Core;
using System.Text.RegularExpressions;
using VisionCamera;
using System.Diagnostics;

namespace Vision
{
    public partial class ImageForm : Form
    {
        private const string WORK_DIR = "../../resources/vision/";
        private const string DEFAULT_REGION_FILE = WORK_DIR + "region.txt";
        private const string DEFAULT_TRAIN_FILE = WORK_DIR + "training.txt";
        private const string IDLE_STATUS = "Ready. Press F1 for key functions.";

        private readonly int MESSAGE_SENDER_PORT = Constants.get<int>("ports", "VisionDataPort");

        private static HelpForm HelpForm = new HelpForm();
        private static Form SeqCameraForm = null; // setup upon first request
		private static TestForm TestForm = new TestForm();

        /* PRIVATE MEMBERS */

        private enum ViewMode { NORMAL, COLOR_CLASS };
        private enum Position { FRONT, REAR, LEFT, RIGHT, FRONT_LEFT, FRONT_RIGHT, REAR_LEFT, REAR_RIGHT };

        // avoid creating camera objects every time
        private static PGRCamera _PGRCamera = new PGRCamera();
        //private static PGRCamera _PGRCamera = null;
        private static SeqCamera _seqCamera = new SeqCamera();

        private VisionCamera.ICamera _camera;
        private ColorCalibrator _colorCalibrator;
        private TsaiCalibrator _tsaiCalibrator;
        private Blobber _blobber;

        private Robocup.MessageSystem.MessageSender<Robocup.Core.VisionMessage> _messageSender;

        private FieldState _fieldState;

        private Bitmap _bitmap;
        private RAWImage _rawImage;
        private float _zoomFactor = 1;
        private ViewMode _viewMode = ViewMode.NORMAL;
        private List<SelectionBox.SelectionBox> _highlights;

        private Bitmap _normalBitmap; //used to save the orig, when displaying color-class

        // Recording
        private bool _recording;
        private string _recPath = WORK_DIR;

    	private bool _testing;

        /* CONSTRUCTORS */

        public ImageForm()
        {
            InitializeComponent();

            Size = new Size(ImageSettings.IMAGE_RES_X + 20, ImageSettings.IMAGE_RES_Y + 40 + 20);
            imagePanel.Size = new Size(ImageSettings.IMAGE_RES_X + 4, ImageSettings.IMAGE_RES_Y + 4);
            imagePicBox.Size = new Size(ImageSettings.IMAGE_RES_X, ImageSettings.IMAGE_RES_Y);
            regionSelBox.RegionLocation = new Point(0, 0);
            regionSelBox.RegionSize = new Size(ImageSettings.IMAGE_RES_X, ImageSettings.IMAGE_RES_Y);
            btnPGRCamera.Checked = true;

            _highlights = new List<SelectionBox.SelectionBox>(100);

            // camera ID depends on which computer this is running on
            // get computer name
            //string compName = SystemInformation.ComputerName.ToUpper();
            //int CAMERA_ID = Constants.get<int>("vision", "CAMERA_ID_" + compName);

            _camera = _PGRCamera;
            //_camera = _seqCamera;
            _tsaiCalibrator = new TsaiCalibrator();
            _colorCalibrator = new ColorCalibrator();
            _blobber = new Blobber(_colorCalibrator, _tsaiCalibrator, this,
                delegate()
                {
                    ChangeStatus("Blobber stopped.");
                });
            _blobber.SetCamera(_camera);
            _blobber.ReloadParameters();
            _fieldState = new FieldState();

            DefaultInitSequence();

            ChangeStatus(IDLE_STATUS);
        }

        private void ImageForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DefaultEndSequence();
        }

        private void DefaultInitSequence()
        {

            if (File.Exists(DEFAULT_REGION_FILE))
                LoadRegion(DEFAULT_REGION_FILE);

            // UNCOMMENT THIS -- doing for faster load
            _colorCalibrator.DefaultInitSequence();
            _tsaiCalibrator.DefaultInitSequence(imagePicBox);           

            _messageSender = Robocup.MessageSystem.Messages.CreateServerSender<VisionMessage>(MESSAGE_SENDER_PORT);


            FieldState.Form.Show();
        }

        private void DefaultEndSequence()
        {
            SaveRegion(DEFAULT_REGION_FILE);
            _tsaiCalibrator.DefaultEndSequence();
            _colorCalibrator.DefaultEndSequence();
        }

        /* PUBLIC METHODS */

        public void LoadImage(Bitmap bitmap)
        {
            ClearHighlights();
            _bitmap = bitmap;
            _viewMode = ViewMode.NORMAL;
            RedrawImage();
        }

        public void LoadImage(RAWImage rawImage)
        {
            ClearHighlights();
            _rawImage = rawImage;
            _bitmap = rawImage.toBitmap();
            _normalBitmap = rawImage.toBitmap();
            _viewMode = ViewMode.NORMAL;
            RedrawImage();
        }

        public RAWImage CaptureImage()
        {
            RAWImage image;
            _camera.getOneImage(out image);
            if (image == null)
                throw new System.ApplicationException("CaptureImage: got invalid image from camera!");
            return image;
        }

        public void RedrawImage()
        {
            imagePicBox.Width = _bitmap.Width;
            imagePicBox.Height = _bitmap.Height;
            imagePicBox.BackgroundImage = _bitmap;

            // move the selection box if it ended up outside of bounds
            Rectangle region = GetRegion();
            if (region.Right >= imagePicBox.Width - 1 || region.Bottom >= imagePicBox.Height -1 )
                SetRegion(new Rectangle(new Point(0, 0), new Size(imagePicBox.Width, imagePicBox.Height)));

        }

        public Rectangle GetRegion()
        {
            return new Rectangle(regionSelBox.RegionLocation, regionSelBox.RegionSize);
        }
        public void SetRegion(Rectangle rect)
        {
            regionSelBox.RegionLocation = rect.Location;
            regionSelBox.RegionSize = rect.Size;
        }
        private void LoadRegion(string filename)
        {
            TextReader tr = new StreamReader(filename);

            string line = tr.ReadLine();
            tr.Close();

            string[] dims = line.Split(new char[] { ' ' }, 4);
            SetRegion(new Rectangle(Convert.ToInt32(dims[0]), Convert.ToInt32(dims[1]),
                                    Convert.ToInt32(dims[2]), Convert.ToInt32(dims[3])));
        }
        private void SaveRegion(string filename)
        {
            TextWriter tw = new StreamWriter(filename);
            Rectangle region = GetRegion();
            tw.WriteLine("{0} {1} {2} {3}", region.Left, region.Top, region.Width, region.Height);
            tw.Close();
        }

        public SelectionBox.SelectionBox HighlightBlob(Blob blob)
        {
            Color color = Color.FromArgb(255, 0, 255); // pinkish
            Color hoverColor = Color.FromArgb(255, 150, 255); // lighter pinkish

            Pen pen = new Pen(color, 1);
            pen.DashStyle = DashStyle.Solid;
            SelectionBox.SelectionBox selBox = new SelectionBox.SelectionBox(pen);
            selBox.RegionLocation = new Point(blob.Left, blob.Top);
            selBox.RegionSize = new Size(blob.Right - blob.Left, blob.Bottom - blob.Top);
            selBox.Resizable = false;

            Size blobSize = new Size(blob.Right - blob.Left, blob.Bottom - blob.Top);
            string caption = "Blob #" + blob.BlobID.ToString() +
                "\nColorClass: " + ColorClasses.GetName(blob.ColorClass) +
                "\nArea: " + blob.Area.ToString() +
                "\nScaled Area: " + String.Format("{0:0.00}", blob.AreaScaled) +
                "\nCenter: (" + blob.CenterX + ", " + blob.CenterY + ")" +
                "\nCenterW: " + String.Format("({0:0.00}, {1:0.00})", blob.CenterWorldX, blob.CenterWorldY) +
                "\nDimensions: " + blobSize.Width.ToString() + " x " + blobSize.Height.ToString();

            ToolTip blobInfo = new ToolTip();
            blobInfo.SetToolTip(selBox, caption);
            blobInfo.InitialDelay = 0;
            blobInfo.AutoPopDelay = int.MaxValue;

            selBox.MouseEnter += new EventHandler(delegate
            {
                selBox.Color = hoverColor;
                selBox.Invalidate();
            });
            selBox.MouseLeave += new EventHandler(delegate
            {
                selBox.Color = color;
                selBox.Invalidate();
            });

            // right click = hides it (forever, but we won't dispose() of the object)
            selBox.MouseClick += new MouseEventHandler(delegate(object sender, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Right)
                    return;

                selBox.Visible = false;
            });

            //selBox.Tag = blobInfo;

            //imagePicBox.Controls.Add(blobInfo);
            imagePicBox.Controls.Add(selBox);
            selBox.Show();

            return selBox;
        }


        public void ClearHighlights()
        {
            if (_highlights.Count <= 0)
                return;

            foreach (SelectionBox.SelectionBox selBox in _highlights)
            {
                //imagePicBox.Controls.Remove(((Label)selBox.Tag));
                imagePicBox.Controls.Remove(selBox);
            }

            _highlights.Clear();
        }

        public void AdvanceFrame(int increment)
        {
            RAWImage image;
            int rc = _seqCamera.getFrame(out image, increment);
            switch (rc)
            {
                case 0:
                    LoadImage(image);
                    ChangeStatus("Frame " + _seqCamera.Frame.ToString() +
                        " loaded from sequence " + Path.GetFileName(_seqCamera.Sequence));
                    break;
                case 1: ChangeStatus("Beginning of sequence reached."); break;
                case 2: ChangeStatus("End of sequence reached."); break;
                default: ChangeStatus("Error when advancing frame."); break;
            }
        }


        /* PRIVATE METHODS */

        private Bitmap ZoomBitmap(Bitmap original, float factor)
        {
            Bitmap zoomed = new Bitmap((int)(original.Width * factor), (int)(original.Height * factor));
            Graphics gfx = Graphics.FromImage(zoomed);
            Rectangle srcRect = new Rectangle(0, 0, original.Width, original.Height);
            Rectangle dstRect = new Rectangle(0, 0, (int)(original.Width * factor), (int)(original.Height * factor));
            gfx.DrawImage(original, dstRect, srcRect, GraphicsUnit.Pixel);

            return zoomed;
        }
        private void ChangeStatus(string status)
        {
            lblStatus.Text = status;
        }

        private void BuildSeqCameraForm()
        {
            SeqCameraForm = new Form();
            SeqCameraForm.Text = "BMP Sequence Camera";
            SeqCameraForm.Size = new Size(200, 130);
            SeqCameraForm.MaximizeBox = false;
            SeqCameraForm.StartPosition = FormStartPosition.CenterParent;
            SeqCameraForm.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            SeqCameraForm.DialogResult = DialogResult.Cancel;            

            Label lblSleepTime = new Label();
            lblSleepTime.Text = "Sleep time (ms): ";
            lblSleepTime.Location = new Point(10, 20);

            TextBox txtSleepTime = new TextBox();
            txtSleepTime.Size = new Size(40, 10);
            txtSleepTime.Location = new Point(lblSleepTime.Right + 5, lblSleepTime.Top);
            txtSleepTime.Text = _seqCamera.SleepTime.ToString();

            CheckBox chkRepeat = new CheckBox();
            chkRepeat.Text = "Repeat";
            chkRepeat.Location = new Point(lblSleepTime.Left, lblSleepTime.Bottom + 5);
            chkRepeat.Checked = _seqCamera.Repeat;

            Button btnSet = new Button();
            btnSet.Text = "Apply";
            btnSet.Location = new Point(chkRepeat.Left, chkRepeat.Bottom + 5);
            btnSet.Click += new EventHandler(delegate(object s, EventArgs ea)
            {
                try
                {
                    _seqCamera.SleepTime = int.Parse(txtSleepTime.Text);
                }
                catch (Exception)
                {
                    MessageBox.Show("Failed to parse Sleep Time (integer, milliseconds)!");
                    return;
                }
                _seqCamera.Repeat = chkRepeat.Checked;
                _camera = _seqCamera;
                _blobber.SetCamera(_camera);
                SeqCameraForm.DialogResult = DialogResult.OK;
            });

            SeqCameraForm.Controls.Add(lblSleepTime);
            SeqCameraForm.Controls.Add(txtSleepTime);
            SeqCameraForm.Controls.Add(chkRepeat);
            SeqCameraForm.Controls.Add(btnSet);
        }

        /* EVENT HANDLERS */
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.F1)
            {
                if (HelpForm.Visible)
                    HelpForm.BringToFront();
                else
                    HelpForm.Show();
            }

            return base.ProcessDialogKey(keyData);
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // save Tsai points
            if (keyData == (Keys.Control | Keys.S))
                _tsaiCalibrator.SaveTsaiPointsDlg();
            
                
            if (keyData == (Keys.Alt | Keys.F4))
                this.Close();

            return false;
        }
        private void ImageForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.Handled) return;

            RAWImage image = null;
            switch (Char.ToLower(e.KeyChar))
            {
                case 'c': //capture image
                    if (_blobber != null && _blobber.Blobbing)
                        return;
                    ClearHighlights();
                    try
                    {
                        image = CaptureImage();
                    }
                    catch
                    {
                        MessageBox.Show("Capture failed. Try again.");
                        return;
                    }
                    LoadImage(image);
                    ChangeStatus("Frame captured.");
                    break;
                case 'l': //load image from bitmap file
                    if (_blobber != null && _blobber.Blobbing)
                        return;
                    ClearHighlights();

                    OpenFileDialog openDlg = new OpenFileDialog();
                    openDlg.Filter = "Bitmap images (*.bmp)|*.bmp";
                    openDlg.CheckFileExists = openDlg.CheckPathExists = true;
                    openDlg.InitialDirectory = WORK_DIR;
                    openDlg.RestoreDirectory = true;
                    openDlg.FileOk += new CancelEventHandler(delegate(Object s, CancelEventArgs e1)
                    {
                        if (!e1.Cancel)
                        {
                            image = new RAWImage(openDlg.FileName);
                            LoadImage(image);
                            // see if file is part of a sequence
                            Regex pattern = new Regex(@"([0-9]+)\.bmp$");
                            Match match = pattern.Match(Path.GetFileName(openDlg.FileName));
                            if (match.Success)
                            {
                                _seqCamera.Sequence = openDlg.FileName.Substring(0, openDlg.FileName.Length - match.Groups[1].Length - 4);
                                _seqCamera.StartFrame = _seqCamera.Frame = int.Parse(match.Groups[1].Value);
                                ChangeStatus("Frame " + _seqCamera.Frame.ToString() +
                                    " loaded from sequence " + Path.GetFileName(_seqCamera.Sequence));
                            }
                            else
                            {
                                ChangeStatus("Image loaded from " + Path.GetFileName(openDlg.FileName));
                            }

                        }
                    });
                    openDlg.ShowDialog();

                    break;
                case 's': //save image to bitmap file
                     
                    if (_rawImage == null)
                    {
                        MessageBox.Show("No image to save!");
                        return;
                    }
                    if (_blobber != null && _blobber.Blobbing)
                        return;

                    SaveFileDialog saveDlg = new SaveFileDialog();
                    saveDlg.Filter = "Bitmap image (*.bmp)|*.bmp";
                    saveDlg.InitialDirectory = WORK_DIR;
                    saveDlg.RestoreDirectory = true;
                    saveDlg.FileOk += new CancelEventHandler(delegate(Object s, CancelEventArgs e1)
                    {
                        if (!e1.Cancel)
                        {
                            _normalBitmap.Save(saveDlg.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                            ChangeStatus("Image saved to " + Path.GetFileName(saveDlg.FileName));
                        }
                    });
                    saveDlg.ShowDialog();

                    break;
                case '=': //zoom in
                    _zoomFactor *= 2;
                    _bitmap = ZoomBitmap(_bitmap, 2);
                    RedrawImage();
                    break;
                case '-': //zoom out
                    if (_zoomFactor > 1)
                    {
                        _zoomFactor *= 0.5f;
                        _bitmap = ZoomBitmap(_bitmap, 0.5f);
                        RedrawImage();
                    }
                    break;
                case 'v': // change view mode
                    if (_colorCalibrator == null)
                    {
                        MessageBox.Show("ColorCalibrator not loaded!");
                        return;
                    }
                    if (_bitmap == null)
                        return;

                    if (_viewMode == ViewMode.COLOR_CLASS)
                    {
                        LoadImage(_normalBitmap);
                        _normalBitmap = null;
                        _viewMode = ViewMode.NORMAL;
                    }
                    else
                    {
                        _normalBitmap = _bitmap;
                        LoadImage(_colorCalibrator.ConvertToColorClass(_bitmap));
                        _viewMode = ViewMode.COLOR_CLASS;
                    }
                    break;
                case 't': // show/hide tsai points
                    if (_tsaiCalibrator == null)
                    {
                        MessageBox.Show("TsaiCalibrator not loaded!");
                        return;
                    }
                    _tsaiCalibrator.ToggleTsaiPoints();
                    break;
                case 'y': // generate tsai image to world lookup table
                    if (_tsaiCalibrator == null)
                    {
                        MessageBox.Show("TsaiCalibrator not loaded!");
                        return;
                    }
                    try
                    {
                        _tsaiCalibrator.GenerateImageToWorldTable();
                    }
                    catch
                    {
                        MessageBox.Show("Failed to generate Image to World Lookup table!");
                        return;
                    }
                    ChangeStatus("Tsai image->world coord lookup table generated.");
                    break;
                case 'p': // load pixels into the color calibrator
                    if (_colorCalibrator == null)
                    {
                        MessageBox.Show("ColorCalibrator not loaded!");
                        return;
                    }
                    if (_bitmap == null)
                        return;



                    // need to clone, because two things try to lock this same bitmap object
                    _colorCalibrator.LoadPixels((Bitmap)_bitmap.Clone(), GetRegion());
                    ChangeStatus("Pixels plotted.");
                    break;
                case 'b': // blob and resolve objects for current image
                    if (_blobber == null)
                    {
                        MessageBox.Show("Blobber not loaded!");
                        return;
                    }
                    if (_blobber.Blobbing)
                        return;

                    if (_colorCalibrator == null)
                    {
                        MessageBox.Show("Color Calibrator not loaded!");
                        return;
                    }
                    if (_colorCalibrator.RGBtoCCTable == null)
                    {
                        MessageBox.Show("RGB to CC Table not generated!");
                        return;
                    }
                    if (_tsaiCalibrator == null)
                    {
                        MessageBox.Show("Tsai Calibrator not loaded!");
                        return;
                    }
                    if (_tsaiCalibrator.imgToWorldLookup == null)
                    {
                        MessageBox.Show("Tsai Image to World lookup table not generated!");
                        return;
                    }

                    if (_rawImage == null)
                    {
                        image = CaptureImage();
                        ClearHighlights();
                        LoadImage(image);
                    }
                    try
                    {
                        _blobber.doBlob(_rawImage, GetRegion());
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine("Exception occured in doBlob(): " + exc.Message);
                        return;
                    }
                    //GameObjects gameObjs = VisionStatic.RobotFinder.findGameObjects(_blobber.blobs, _blobber.totalBlobs, _tsaiCalibrator);
                    VisionMessage visionMessageLocal = VisionStatic.RobotFinder.findGameObjects2(_blobber.blobs, _blobber.totalBlobs, _tsaiCalibrator);

                    if (visionMessageLocal.OurRobots.Count <= 0)
                    {
                        Console.WriteLine("NO ROBOT!!!!");
                        TextWriter twr = new StreamWriter(WORK_DIR + "bad_frames.txt", true); // append
                        twr.WriteLine(_seqCamera.Frame);
                        twr.Close();
                    }

                    _fieldState.Update(visionMessageLocal);

                    ClearHighlights();
                    for (int i = 0; i < _blobber.totalBlobs; i++)
                    {
                        _highlights.Add(HighlightBlob(_blobber.blobs[i]));
                    }

                    int totalObjects = ((visionMessageLocal.BallPosition == null) ? 0 : 1) + visionMessageLocal.OurRobots.Count + visionMessageLocal.TheirRobots.Count; // 1 for ball
                    ChangeStatus("Frame processed. Found " + _blobber.totalBlobs.ToString() + " blobs and " +
                                  totalObjects.ToString() + " objects.");

                    break;
                case 'd': // detect tsai points
                    if (_blobber == null)
                    {
                        MessageBox.Show("Blobber not loaded!");
                        return;
                    }
                    if (_blobber.Blobbing)
                        return;
                    if (_blobber.totalBlobs <= 0)
                    {
                        MessageBox.Show("Must blob first!");
                        return;
                    }

                    TsaiPtFinder.LoadImage(_rawImage);
                    List<Pair<Point, DPoint>> pairs;

                    string compName = SystemInformation.ComputerName.ToUpper();
                    DPoint offset = new DPoint(Constants.get<double>("vision", "TSAI_OFFSET_X_" + compName),
                                             Constants.get<double>("vision", "TSAI_OFFSET_Y_" + compName));
                    
                    pairs = TsaiPtFinder.FindTsaiPts(_blobber.blobs, offset);

                    

                    // Clear and load before appending, so that this method can be re-run
                    _tsaiCalibrator.ClearTsaiPoints();
                    _tsaiCalibrator.LoadTsaiPoints();
                    _tsaiCalibrator.AppendTsaiPoints(pairs);
                    
                    _tsaiCalibrator.CreateLabels(imagePicBox);
                    _tsaiCalibrator.showTsaiPoints();



                   // Graphics gfx = imagePicBox.CreateGraphics();
                   /* Pen[] pens = new Pen[] { Pens.Aqua, Pens.Red, Pens.Green, Pens.Gold };
                    for (int i = 0; i < 4; i++)
                    {
                      

                            if (models[i] == null) continue;
                            Point x1, x2;
                            if (models[i][1] != 0)
                            {
                               // x1 = new Point(pos.X + 0, pos.Y + (int)(-models[i][2] / models[i][1]));
                               // x2 = new Point(pos.X + 30, pos.Y + (int)((-models[i][0] * 30 - models[i][2]) / models[i][1]));
                                x1 = new Point(pos.X + (int)(-models[i][2] / models[i][1]), pos.Y + 0);
                                x2 = new Point(pos.X + (int)((-models[i][0] * 30 - models[i][2]) / models[i][1]), pos.Y + 30);
                            }
                            else
                            {
                                //x1 = new Point(pos.X + (int)(-models[i][2]), pos.Y);
                                //x2 = new Point(pos.X + (int)(-models[i][2]), pos.Y + 30);
                                x1 = new Point(pos.X, pos.Y + (int)(-models[i][2]));
                                x2 = new Point(pos.X +30 , pos.Y + (int)(-models[i][2]));
                            }
                            gfx.DrawLine(pens[i], x1, x2);
                        

                    }*/

            
                   /* foreach (Pair<Point, DPoint> pair in pairs)
                    {
                        gfx.FillRectangle(Brushes.Aqua, new Rectangle((int)pair.First.X,(int)pair.First.Y, 5, 5));
                    }*/
                    
                   // gfx.Dispose();

                    ChangeStatus("Tsai point detection finished.");
                    break;
                case 'k':
                    _tsaiCalibrator.ClearTsaiPoints();
                    ChangeStatus("Tsai points cleared.");
                    break;
                case 'h':
                    ClearHighlights();
                    ChangeStatus("Highlights removed.");
                    break;
				case 'x': //start testing
					if (TestForm.ShowDialog() == DialogResult.Cancel)
						return;

            		TestForm.Tester.Reset();
					_testing = true;
					//Intentional fall-through, who said goto was bad practice?
					goto case 'a';
                case 'a': // start live blobbing
                    if (_blobber == null)
                    {
                        MessageBox.Show("Blobber not loaded!");
                        return;
                    }
                    if (_colorCalibrator == null)
                    {
                        MessageBox.Show("Color Calibrator not loaded!");
                        return;
                    }
                    if (_colorCalibrator.RGBtoCCTable == null)
                    {
                        MessageBox.Show("RGB to CC Table not generated!");
                        return;
                    }
                    if (_tsaiCalibrator == null)
                    {
                        MessageBox.Show("Tsai Calibrator not loaded!");
                        return;
                    }
                    if (_tsaiCalibrator.imgToWorldLookup == null)
                    {
                        MessageBox.Show("Tsai Image to World lookup table not generated!");
                        return;
                    }



                    if (!_blobber.Blobbing)


                        //_blobber.Start(new OnNewStateReady(delegate(GameObjects gameObjects) {
                        try
                        {
                            Console.WriteLine("Starting vision stream...");
                            Stopwatch stopWatch = new Stopwatch();
                            stopWatch.Reset();
                            stopWatch.Start();
                            _blobber.Start(new OnNewStateReady(delegate(VisionMessage visionMessage)
                            {
                                //gameObjects.Source = SystemInformation.ComputerName;
                                
                                // Compute the frame rate
                                long dt = stopWatch.ElapsedMilliseconds / 1000;
                                if (dt > 5)
                                {
                                    double rate = _camera.FrameCount / dt;
                                    ChangeStatus("Running at " + rate.ToString() + " fps...");
                                    stopWatch.Stop();
                                    stopWatch.Reset();                                    
                                    _camera.resetFrameCount();
                                    stopWatch.Start();
                                }
								
								if(_testing)
									TestForm.Tester.TestFrame(visionMessage, _camera);

                                _fieldState.Update(visionMessage);

                                //_messageSender.Post(gameObjects);
                                //Console.WriteLine("Posting vision message.");
                                _messageSender.Post(visionMessage);

                            }));
                        }
                        catch (Exception exc)
                        {
                            MessageBox.Show("Failed while blobbing: " + exc.Message);
                            return;
                        }

                    ChangeStatus("Running...");
                    break;
                case 'z': // stop live blobbing
                    if (_blobber == null)
                        return;

                    if (_blobber.Blobbing)
                    {
                        _blobber.Stop();
                        ChangeStatus(IDLE_STATUS);
                    }

					if(_testing)
					{
						TestForm.Tester.FinishTest();
						_testing = false;
					}
                    break;
                case '.': // go to next (saved) frame
                    AdvanceFrame(1);
                    break;
                case ',': // go to previous (saved) frame
                    AdvanceFrame(-1);
                    break;
                case 'f': // show Field State form
                    if (FieldState.Form.Visible == true)
                        FieldState.Form.BringToFront();
                    else
                        FieldState.Form.Show();
                    break;
                case 'r': // reload constants from file
                    Constants.Load();
                    RobotFinder.LoadParameters();
                    ColorClasses.LoadParameters();
                    TsaiPtFinder.LoadParameters();
                    if (_blobber != null)
                        _blobber.ReloadParameters();
                    ChangeStatus("Constants reloaded.");
                    break;

                #region NeuralNet_NOT_IMPLEMENTED
                    /*
                case '2': // construct training data
                    int robotID = 1;

                    Blob[] blobs = _blobber.blobs;
                    LinkedList<Blob> dots = new LinkedList<Blob>();   
                    LinkedList<double> distsSq = new LinkedList<double>();                
                    Blob centerDot = null;                    
                    int maxDots = -1;
                    

                    // Generate training data: locations and areas of dots
                    
                    for (int i = 0; i < _blobber.totalBlobs; i++)
                    {
                        if (blobs[i].ColorClass == ColorClasses.OUR_CENTER_DOT)
                        {
                            // Center dot is probably useful to have separate for neural network
                            Blob tempCenterDot = blobs[i];
                            LinkedList<Blob> tempDots = new LinkedList<Blob>();
                            LinkedList<double> tempDistsSq = new LinkedList<double>();
                            double distSq;
                            
                            for (int j = 0; j < _blobber.totalBlobs; j++)
                            {
                               
                                byte c = blobs[j].ColorClass;
                                if (c == ColorClasses.COLOR_DOT_CYAN || c == ColorClasses.COLOR_DOT_GREEN ||
                                    c == ColorClasses.COLOR_DOT_PINK)
                                {
                                    distSq = RobotFinder.distanceSq(tempCenterDot.CenterX, tempCenterDot.CenterY, blobs[j].CenterX, blobs[j].CenterY);                                    
                                    if (distSq < RobotFinder.DIST_SQ_TO_CENTER_PIX)
                                    {
                                        tempDistsSq.AddLast(distSq);
                                        tempDots.AddLast(blobs[j]);
                                    }
                                }
                            }

                            // if too few dots around the center were found, then we're probably not looking at a center dot
                            if (tempDots.Count > maxDots)
                            {
                                centerDot = tempCenterDot;                              
                                dots = tempDots;
                                distsSq = tempDistsSq;
                                maxDots = tempDots.Count;
                            }                           
                        }
                    }


                    Console.WriteLine("Center dot: " + ((centerDot == null) ? "! not found !" : centerDot.BlobID.ToString()));
                    Console.Write("Dots found (" + dots.Count + "): ");
                    foreach (Blob dot in dots)
                    {
                        Console.Write(dot.BlobID + " " + ColorClasses.GetName(dot.ColorClass) + "; ");
                    }
                    Console.WriteLine();

                    if (centerDot == null)
                        return;                   
                
                    // to do: maybe insert some out-lier detector here to filter out the false dots

                    // for now just take the closest 4 if there are more
                    Blob[] fourDots = new Blob[4];
                    // I only know how to sort using Array.Sort()
                    double[] distsSqAr = new double[distsSq.Count];
                    Blob[] dotsAr = new Blob[dots.Count];
                    distsSq.CopyTo(distsSqAr, 0);
                    dots.CopyTo(dotsAr, 0);
                    Array.Sort(distsSqAr, dotsAr);                    
                    for (int i = 0; i < 4; i++)
                        fourDots[i] = dotsAr[i];
                    
                    Console.Write("Four dots chosen: ");
                    foreach (Blob dot in fourDots)
                    {
                        Console.Write(dot.BlobID + " " + ColorClasses.GetName(dot.ColorClass) + "; ");
                    }
                    Console.WriteLine();
                   
                    // Find the center vectors
                    System.Windows.Vector[] centerVectors = new System.Windows.Vector[4];
                    for (int i = 0; i < 4; i++)
                    {
                        centerVectors[i] = new System.Windows.Vector(fourDots[i].CenterX - centerDot.CenterX, fourDots[i].CenterY - centerDot.CenterY);
                    }

                    // Find the three vectors for each dot, and based on their length determine whether the dot a front one or a rear one
                    System.Windows.Vector[,] vectors = new System.Windows.Vector[4, 4];
                    double[,] lengths = new double[4, 4];
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            vectors[i, j] = new System.Windows.Vector(fourDots[j].CenterX - fourDots[i].CenterX, fourDots[j].CenterY - fourDots[i].CenterY);
                            lengths[i, j] = vectors[i, j].LengthSquared;
                        }
                    }

                    // 
                    int frontLeft, frontRight, rearLeft, rearRight;
                    frontLeft = frontRight = rearLeft = rearRight = -1;

                    for (int i = 0; i < 4; i++)
                    {
                        double[] ls = new double[4];
                        for (int j = 0; j < 4; j++)
                            ls[j] = lengths[i, j];
                        Array.Sort<double>(ls);

                        if (Math.Abs(ls[2] - ls[1]) < Math.Abs(ls[3] - ls[2]))
                        {
                            // rear
                            if (rearLeft < 0)
                                rearLeft = i;
                            else if (rearRight < 0)
                                rearRight = i;
                            else
                            {
                                Console.WriteLine("Too many rear dots!");
                                return;
                            }
                        }
                        else
                        {
                            // front
                            if (frontLeft < 0)
                                frontLeft = i;
                            else if (frontRight < 0)
                                frontRight = i;
                            else
                            {
                                Console.WriteLine("Too many front dots!");
                                return;
                            }
                        }
                    }

                    //determine left/right
                    int t;
                    if (System.Windows.Vector.CrossProduct(centerVectors[frontLeft], centerVectors[frontRight]) < 0)
                    {
                        t = frontLeft;
                        frontLeft = frontRight;
                        frontRight = t;
                    }

                    if (System.Windows.Vector.CrossProduct(centerVectors[rearLeft], centerVectors[rearRight]) > 0)
                    {
                        t = rearLeft;
                        rearLeft = rearRight;
                        rearRight = t;
                    }

                    Console.WriteLine("Positions identified: ");
                    Console.WriteLine(fourDots[frontLeft].BlobID + "(" + ColorClasses.GetName(fourDots[frontLeft].ColorClass) + ")        " + 
                                      fourDots[frontRight].BlobID + "(" + ColorClasses.GetName(fourDots[frontRight].ColorClass) + ")");
                    Console.WriteLine("  " + fourDots[rearLeft].BlobID + "(" + ColorClasses.GetName(fourDots[rearLeft].ColorClass) + ")    " + 
                                      fourDots[rearRight].BlobID + "(" + ColorClasses.GetName(fourDots[rearRight].ColorClass) + ")");
                    Console.WriteLine();

                    break;


                    // Save training data to file (append)
                    TextWriter tw = new StreamWriter(DEFAULT_TRAIN_FILE, true);

                    // First thing is the robot id and the count of dots
                    tw.WriteLine("{0:G} {1:G} {2:G}", robotID, dots.Count);

                    if (centerDot != null)
                    {
                        tw.WriteLine("{0:G} {1:E} {2:E} {3:E}", centerDot.ColorClass, 
                                     centerDot.CenterWorldX, 
                                     centerDot.CenterWorldY,
                                     centerDot.AreaScaled);
                    }
                    foreach (Blob dot in dots)                    
                        tw.WriteLine("{0:G} {1:E} {2:E} {3:E}", dot.ColorClass, 
                        dot.CenterWorldX - centerDot.CenterWorldX, 
                        dot.CenterWorldY - centerDot.CenterWorldY, 
                        dot.AreaScaled);                    

                    tw.Close();

                    break;
                     */
                #endregion
                
                case 'g':
                    SaveRegion(DEFAULT_REGION_FILE);
                    ChangeStatus("Region saved.");
                    break;
                case ';':
                    if (_recording)
                        return;

                    // Get the folder
                    FolderBrowserDialog dlg = new FolderBrowserDialog();
                    dlg.Description = "Select folder to where the sequence of frames will be written.";
                    dlg.SelectedPath = Path.GetFullPath(_recPath);
                    dlg.ShowNewFolderButton = true;
                    DialogResult dlgRes = dlg.ShowDialog();
                    if (dlgRes != DialogResult.OK)
                        return;
                    _recPath = dlg.SelectedPath + "\\";

                    // start a new thread that gets frames and writes them to files                    
                    _recording = true;
                    VoidDelegate recLoopDelegate = new VoidDelegate(RecLoop);
                    AsyncCallback recErrorHandler = new AsyncCallback(RecErrorHandler);
                    IAsyncResult recLoopHandle = recLoopDelegate.BeginInvoke(RecErrorHandler, null);

                    ChangeStatus("Recording to " + Path.GetDirectoryName(_recPath) + "...");
                    break;
                case '\'':
                    _recording = false;
                    ChangeStatus(IDLE_STATUS);
                    break;

                case '6':
                    TextReader tr = new StreamReader(WORK_DIR + "tsai_points.txt");
                    TextWriter tw = new StreamWriter(WORK_DIR + "tsai_points_fixed.txt");

                    string line;
                    while ((line = tr.ReadLine()) != null) {                    
                        string[] items = line.Split(' ');
                        int fixedY = int.Parse(items[3]) - 1500;
                        tw.WriteLine(items[0] + " " + items[1] + " " +  items[2] + " " + fixedY.ToString() + " " + items[4]);
                    }

                    tw.Close();
                    tr.Close();
                    break;
            }

        }

        // Need to move these to a better place in the code
        private void RecLoop()
        {
            RAWImage img;
            Bitmap bmp;
            string fname; 
            int i = 0;

            _camera.startCapture();

            while (_recording)
            {
                if (_camera.getFrame(out img) != 0)
                {
                    Console.WriteLine("Error getting frame from camera!");
                    return;
                }
                bmp = img.toBitmap();

                fname = _recPath + "frame" + i.ToString() +".bmp";
                bmp.Save(fname, System.Drawing.Imaging.ImageFormat.Bmp);
                i++;
            }

            _camera.stopCapture();
        }
        private void RecErrorHandler(IAsyncResult res)
        {
            Console.WriteLine("RecLoop exited and error handler was called.");
            _camera.stopCapture();
            _recording = false;
        }

        private void CheckMenuItem(ToolStripItemCollection items, ToolStripMenuItem itemToCheck)
        {
            foreach (ToolStripMenuItem item in items)
                item.Checked = false;
            itemToCheck.Checked = true;
        }

        private void imagePicBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (_colorCalibrator == null || _bitmap == null)
                return;

            Color pixColor;
            if (_viewMode != ViewMode.NORMAL)
                pixColor = _normalBitmap.GetPixel(e.X, e.Y);
            else
                pixColor = _bitmap.GetPixel(e.X, e.Y);

            _colorCalibrator.LocatePixel(pixColor);


        }

        private void btnPGRCamera_Click(object sender, EventArgs e)
        {
            CheckMenuItem(btnCamera.DropDownItems, (ToolStripMenuItem)sender);

            _camera = _PGRCamera;
            _blobber.SetCamera(_camera);
        }

        private void btnBMPSeqCamera_Click(object sender, EventArgs e)
        {
            if (SeqCameraForm == null)
                BuildSeqCameraForm();
            DialogResult dlgRes = SeqCameraForm.ShowDialog();

            if (dlgRes == DialogResult.OK)
                CheckMenuItem(btnCamera.DropDownItems, (ToolStripMenuItem)sender);

        }

    }
}
    