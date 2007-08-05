using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using VisionStatic;

namespace Vision
{
    public partial class HSVSliceForm : Form
    {
        public int V {
            get { return _v; }
        }
        
        private enum Tool { PENCIL, ERASER, PAINT_BUCKET };

        private const int UNDO_CAPACITY = 10;
        private const int STUPID_MOUSE_OFFSET_X = 5;
        private const int STUPID_MOUSE_OFFSET_Y = 22;
        
        private ColorCalibrator _colorCalibrator;

        private readonly int _v;

        private Bitmap _bmpSlice;
        private Bitmap _bmpOrigPixels;

        // used to redraw only when necessary
        private bool _viewOutOfSync;

        // bitmap used for drawing (almost always is a clone of _bmpSlice)
        private Bitmap _bmpSliceTemp;
        
        // we're drawing on two graphics objects at the same time
        private Graphics _bmpGfx; // one for actual data
        private Graphics _panGfx; // the other for display

        // NOT IMPLEMENTED YET
        private int zoomFactor;

        // undo functionality
        LinkedList<Bitmap> _undoBitmap;
        LinkedList<byte[,]> _undoHStoCCTable;

        private Point _lastMousePos;
        private Rectangle _mouseUnclipped;
        private Pen _currPen;
        private Pen _eraserPen;
        private Tool _currTool;
        private FloodFiller _floodFiller;
        private ToolStripButton _currColorBtnChecked;
        private ToolStripButton _currToolBtnChecked;     
        private ToolStripButton[] _toolStripBtns;
        
        public HSVSliceForm(int v, ColorCalibrator colorCalibrator)
        {
            InitializeComponent();

            _v = v;
            _colorCalibrator = colorCalibrator;

            _bmpSlice = new Bitmap(360, 100, PixelFormat.Format32bppArgb);
            _bmpSliceTemp = new Bitmap(360, 100, PixelFormat.Format32bppArgb);
            _bmpGfx = Graphics.FromImage(_bmpSliceTemp);
            _bmpGfx.CompositingMode = CompositingMode.SourceCopy;
            _panGfx = panSlice.CreateGraphics();

            _undoBitmap = new LinkedList<Bitmap>();
            _undoHStoCCTable = new LinkedList<byte[,]>();

            _floodFiller = new FloodFiller();

            // transparent would be ideal, but this also works
            _eraserPen = new Pen(Color.FromArgb(0, 0, 0));
            _eraserPen.Width = 5;

            zoomFactor = 1;

            // controls
            Text = "HSV Slice for v = " + (_v * ColorCalibrator.V_SLICE_DEPTH).ToString() + " to " + ((_v + 1) * ColorCalibrator.V_SLICE_DEPTH).ToString();
            txtV.Text = (_v * ColorCalibrator.V_SLICE_DEPTH).ToString();
            ToolStripButton tsBtn;
            _toolStripBtns = new ToolStripButton[ColorClasses.COLOR_CLASSES.Length];
            
            int i, k;
            k  = 0;
            for (i = 1; i < ColorClasses.COLOR_CLASSES.Length; i++) {
                tsBtn = new ToolStripButton();

                tsBtn.BackColor = SystemColors.Control;
                tsBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                tsBtn.Image = GenerateToolStripBtnImage(ColorClasses.COLOR_CLASSES[i]);
                tsBtn.Tag = ColorClasses.COLOR_CLASSES[i];
                tsBtn.Size = new System.Drawing.Size(15, 15);

                tsBtn.Click += new System.EventHandler(this.toolStripBtn_Click);

                _toolStripBtns[k++] = tsBtn;
                toolStrip1.Items.Add(tsBtn);
            }

            _currColorBtnChecked = _toolStripBtns[0];
            toolStripBtn_Click(_toolStripBtns[0], new EventArgs());

            _currToolBtnChecked = tsbPencil;
            tsbPencil_Click(tsbPencil, new EventArgs());

            tmrDraw.Stop();
            _mouseUnclipped = Cursor.Clip;
            
            
            if (_v == 0)
                btnPrevSlice.Enabled = false;
            if (_v == 100 / ColorCalibrator.V_SLICE_DEPTH - 1)
                btnNextSlice.Enabled = false;          
        }

        public void LoadBmpCC(Bitmap bmp) {
            _bmpSlice = bmp;
            _bmpSliceTemp = _colorCalibrator.CloneBitmap(_bmpSlice);
            _bmpGfx = Graphics.FromImage(_bmpSliceTemp);
            _bmpGfx.CompositingMode = CompositingMode.SourceCopy;
            _viewOutOfSync = true;
        }
        //public Bitmap GetBmpCC() {
       //     return _bmpSlice;
       // }
        public void LoadBmpPixels(Bitmap bmp) {
            _bmpOrigPixels = bmp;
            _viewOutOfSync = true;
            // just added
            UpdatePanelImage();
        }
        public void UpdatePanelImage() {
            if (!_viewOutOfSync)
                return;

            if (_bmpOrigPixels != null){
                panSlice.BackgroundImage = _colorCalibrator.mergeBitmaps(_bmpSlice, _bmpOrigPixels);
            }else{
                panSlice.BackgroundImage = _bmpSlice;
            }

            _viewOutOfSync = false;

            //panSlice.Refresh();
        }
        public void HighlightPixel(int h, int s) {
            lblHighlight.Show();
            lblHighlight.SetBounds(h - 3, s - 3, 6, 6);
            txtH.Text = h.ToString();
            txtS.Text = s.ToString();
        }
        public void Undo() {
            if (_undoBitmap.Count <= 0)
                return;

            // restore state
            _bmpSlice = _undoBitmap.Last.Value;
            _undoBitmap.RemoveLast();
            byte[,] table = _undoHStoCCTable.Last.Value;
            _undoHStoCCTable.RemoveLast();
            int h, s;
            for (h = 0; h < 360; h++)
                for (s = 0; s < 100; s++)
                    _colorCalibrator.HSVtoCCTable[h, s, _v] = table[h, s];

            _bmpSliceTemp = _colorCalibrator.CloneBitmap(_bmpSlice);
            _bmpGfx = Graphics.FromImage(_bmpSliceTemp);

            _viewOutOfSync = true;

            UpdatePanelImage();
            panSlice.Refresh();

            if (_undoBitmap.Count <= 0)
                tsbUndoLastFill.Enabled = false;
        }

        /* Private Methods */

        private void SaveChanges() {
            BitmapData data;
            IntPtr ptr;
            int dataLength = _bmpSlice.Width * _bmpSlice.Height * 4;
            byte[] bSliceTemp = new byte[dataLength];

            // save previous state for undo
            if (_undoBitmap.Count > UNDO_CAPACITY) {
                _undoBitmap.RemoveFirst();
                _undoHStoCCTable.RemoveFirst();
            }

            _undoBitmap.AddLast(_colorCalibrator.CloneBitmap(_bmpSlice));

            // clone the HS slice from the main table
            byte[,] table = new byte[360, 100];
            int h, s;
            for (h = 0; h < 360; h++)
                for (s = 0; s < 100; s++)
                    table[h, s] = _colorCalibrator.HSVtoCCTable[h, s, _v];
            _undoHStoCCTable.AddLast(table);

            // save changes 
            data = _bmpSliceTemp.LockBits(new Rectangle(0, 0, _bmpSliceTemp.Width, _bmpSliceTemp.Height),
                                                 ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            ptr = data.Scan0;

            Marshal.Copy(ptr, bSliceTemp, 0, dataLength);

            _bmpSliceTemp.UnlockBits(data);


            // _bmpSliceTemp => table
            int colClass;
            int i = 0;
            do {
                if (bSliceTemp[i + 3] != 0) {
                    colClass = isInArray(ColorClasses.COLOR_CLASSES, bSliceTemp[i + 2], bSliceTemp[i + 1], bSliceTemp[i]);
                    if (colClass >= 0) {
                        _colorCalibrator.HSVtoCCTable[(i % (360 * 4)) / 4, i / (360 * 4), _v] = (byte)colClass;
                    }
                }
                i += 4;
            } while (i < dataLength);

            // _bmpSliceTemp => _bmpSlice
            data = _bmpSlice.LockBits(new Rectangle(0, 0, _bmpSlice.Width, _bmpSlice.Height),
                               ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            ptr = data.Scan0;
            Marshal.Copy(bSliceTemp, 0, ptr, dataLength);
            _bmpSlice.UnlockBits(data);

            _viewOutOfSync = true;

            UpdatePanelImage();
            panSlice.Refresh();

            if (_undoBitmap.Count > 0)
                tsbUndoLastFill.Enabled = true;

        }

        private void ToggleHighlightLabel() {
            if (lblHighlight.Visible)
                lblHighlight.Hide();
            else
                lblHighlight.Show();

            txtH.Text = (lblHighlight.Left + 3).ToString();
            txtS.Text = (lblHighlight.Top + 3).ToString();
        }

        private int isInArray(Color[] array, byte r, byte g, byte b) {
            int i;
            i = 0;
            do {
                if (array[i].R == r && array[i].G == g && array[i].B == b) {
                    return i;
                }
                i++;
            } while (i < array.Length);

            return -1;
        }

        private Bitmap GenerateToolStripBtnImage(Color color) {
            Bitmap bmpImage;
            Graphics gfxBmpImage;

            bmpImage = new Bitmap(10, 10);
            gfxBmpImage = Graphics.FromImage(bmpImage);
            gfxBmpImage.FillRectangle(new SolidBrush(color), 0, 0, bmpImage.Width, bmpImage.Height);
            gfxBmpImage.Dispose();

            return bmpImage;
        }
        private void ToNextSlice() {
            if (_v == 100 / ColorCalibrator.V_SLICE_DEPTH - 1)
                return;
            _colorCalibrator.ShowSlice(_v + 1, Left, Top);
            Hide();
        }
        private void ToPrevSlice() {
            if (_v == 0)
                return;
            _colorCalibrator.ShowSlice(_v - 1, Left, Top);
            Hide();
        }

        /* Event Handlers for controls */

        private void toolStripBtn_Click(object sender, EventArgs e) {
            _currColorBtnChecked.Checked = false;
            if (_currPen == null) {
                _currPen = new Pen(((Color)((ToolStripButton)sender).Tag));
            } else {
                _currPen.Color = (Color)(((ToolStripButton)sender).Tag);
            }
            _currPen.Width = 1;


            ((ToolStripButton)sender).Checked = true;
            _currColorBtnChecked = (ToolStripButton)sender;          
        }
        private void tsbPencil_Click(object sender, EventArgs e) {
            _currToolBtnChecked.Checked = false;

            _currTool = Tool.PENCIL;

            ((ToolStripButton)sender).Checked = true;
            _currToolBtnChecked = (ToolStripButton)sender;
        }
        private void tsbEraser_Click(object sender, EventArgs e) {
            _currToolBtnChecked.Checked = false;

            _currTool = Tool.ERASER;
           
            ((ToolStripButton)sender).Checked = true;
            _currToolBtnChecked = (ToolStripButton)sender;
        }

        private void tsbPaintBucket_Click(object sender, EventArgs e) {
            _currToolBtnChecked.Checked = false;

            _currTool = Tool.PAINT_BUCKET;


            ((ToolStripButton)sender).Checked = true;
            _currToolBtnChecked = (ToolStripButton)sender;
        }
        private void panSlice_MouseDown(object sender, MouseEventArgs e) {

            switch (_currTool) {
                case Tool.PAINT_BUCKET:

                    _floodFiller.Pt = new Point(e.X / zoomFactor, e.Y / zoomFactor);
                    _floodFiller.Bmp = _bmpSliceTemp;
                    _floodFiller.FillColor = _currPen.Color;
                    _floodFiller.BoundaryColor = _currPen.Color;

                    _floodFiller.FloodFill();

                    SaveChanges();

                    break;
                case Tool.PENCIL: 
                case Tool.ERASER:
                    
                    Cursor.Clip = new Rectangle(new Point(panSlice.Location.X + this.Location.X + STUPID_MOUSE_OFFSET_X, 
                        panSlice.Location.Y + this.Location.Y + STUPID_MOUSE_OFFSET_Y), 
                        new Size(360, 100)); 
                    tmrDraw.Start();
                    _lastMousePos = new Point();
                    _lastMousePos.X = Cursor.Position.X - this.Left - STUPID_MOUSE_OFFSET_X - panSlice.Left;
                    _lastMousePos.Y = Cursor.Position.Y - this.Top - STUPID_MOUSE_OFFSET_Y - panSlice.Top;
                    break;
            }

        }

        private void tsbUndoLastFill_Click(object sender, EventArgs e) {
            Undo();
        }

        private void tmrDraw_Tick(object sender, EventArgs e) {

                Point currMousePos = new Point();

                currMousePos.X = Cursor.Position.X - this.Left - STUPID_MOUSE_OFFSET_X - panSlice.Left;
                currMousePos.Y = Cursor.Position.Y - this.Top - STUPID_MOUSE_OFFSET_Y - panSlice.Top;

                if (currMousePos.X == _lastMousePos.X && currMousePos.Y == _lastMousePos.Y) {
                    return;
                }
            
            switch (_currTool) {
                case Tool.ERASER:
                    _bmpGfx.DrawLine(_eraserPen, _lastMousePos.X / zoomFactor, _lastMousePos.Y / zoomFactor,
                        currMousePos.X / zoomFactor, currMousePos.Y / zoomFactor);
                    _panGfx.DrawLine(_eraserPen, _lastMousePos, currMousePos);
                    break;
                case Tool.PENCIL:
                    _bmpGfx.DrawLine(_currPen, _lastMousePos.X / zoomFactor, _lastMousePos.Y / zoomFactor,
                        currMousePos.X / zoomFactor, currMousePos.Y / zoomFactor);
                    _panGfx.DrawLine(_currPen, _lastMousePos, currMousePos);        
                    break;
              }


            _lastMousePos = currMousePos;
        }

        private void panSlice_MouseUp(object sender, MouseEventArgs e) {
            tmrDraw.Stop();
            Cursor.Clip = _mouseUnclipped;
            if (_currTool == Tool.PENCIL || _currTool == Tool.ERASER)
                SaveChanges();
        }

        private void panSlice_Paint(object sender, PaintEventArgs e) {
            UpdatePanelImage();
        }

        private void panSlice_MouseMove(object sender, MouseEventArgs e) {
            txtH.Text = e.X.ToString();
            txtS.Text = e.Y.ToString();
        }
        
        private void HSVSliceForm_FormClosing(object sender, FormClosingEventArgs e) {
            Hide();
            e.Cancel = true;
        }
        private void btnNextSlice_Click(object sender, EventArgs e) {
            ToNextSlice();
        }

        private void btnPrevSlice_Click(object sender, EventArgs e) {
            ToPrevSlice();
        }

        private void tsbSaveTables_Click(object sender, EventArgs e) {
            _colorCalibrator.PromptSaveTables();
        }

        private void tsbLoadTables_Click(object sender, EventArgs e) {
            _colorCalibrator.PromptLoadTables();
        }

        protected override bool ProcessDialogKey(Keys keyData) {
            switch (keyData) {
                case Keys.Control | Keys.S:
                    _colorCalibrator.PromptSaveTables();
                    break;
                case Keys.Control | Keys.L:
                    _colorCalibrator.PromptLoadTables();
                    break;
                case Keys.Control | Keys.G:
                    // but if it is out of sync with HSVtoCC, then it is regenerated 
                    // automatically before any blobbing - so this functionality is here just in case
                    _colorCalibrator.GenerateRGBtoCCTable();
                    break;
                case Keys.Control | Keys.Z:
                    Undo();
                    break;
                case Keys.OemPeriod:
                    ToNextSlice();
                    break;
                case Keys.Oemcomma:
                    ToPrevSlice();
                    break;
                case Keys.H:
                    ToggleHighlightLabel();
                    break;
                case Keys.W:
                    tsbPencil_Click(tsbPencil, null);
                    break;
                case Keys.E:
                    tsbEraser_Click(tsbEraser, null);
                    break;
                case Keys.R:
                    tsbPaintBucket_Click(tsbPaintBucket, null);
                    break;
            }
            
            return base.ProcessDialogKey(keyData);
        }




    }
}
