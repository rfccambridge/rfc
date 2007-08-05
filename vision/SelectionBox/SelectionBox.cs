using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace SelectionBox {
    /// <summary>
    /// Summary description for SelectionBox.
    /// </summary>

    public class SelectionBox : Label {
        private enum ResizeMode { NONE, BOTTOM_RIGHT, BOTTOM, RIGHT, MOVE };

        #region Implementtation Member Fields

        // DRAWING
        private const int MARGIN = 2;
        private const int RESIZE_HANDLE_WIDTH = 5;
        
        private bool _resizable = true;
        private ResizeMode _resizeMode = ResizeMode.NONE;
        private Color _color = Color.Red;
        private Pen _pen;
        
        #endregion

 
        [        
        System.ComponentModel.Description("Color of the selection box"),
        System.ComponentModel.Category("Appearance"),
        ]
        public Color Color {
            get {
                return _pen.Color;
            }
            set {
                _pen.Color = value;
                Invalidate();
            }
        }
        [
        System.ComponentModel.Description("Location of top-left corner of selected region. " +
                                          "Different from location of control."),
        ]
        public Point RegionLocation {
            get {
                return new Point(Left + MARGIN / 2, Top + MARGIN / 2);
            }
            set {
                Left = value.X - MARGIN / 2;
                Top = value.Y - MARGIN / 2;
                Invalidate();
            }
        }
        [
        System.ComponentModel.Description("Width and Height of of selected region. " +
                                          "Different from location of control."),
        ]
        public Size RegionSize {
            get {
                //return new Size(Width - RESIZE_HANDLES_SIZE / 2, Height - RESIZE_HANDLES_SIZE / 2);
                return new Size(Width - MARGIN, Height - MARGIN);
            }
            set {
                //Width = value.Width + RESIZE_HANDLES_SIZE / 2;
                //Height = value.Height + RESIZE_HANDLES_SIZE / 2;
                Width = value.Width + MARGIN;
                Height = value.Height + MARGIN;
                Invalidate();
            }
        }
        [
        System.ComponentModel.Description("Specifies whether the box is static or resizable."),
        ]
        public bool Resizable {
            get {
                return _resizable;
            }
            set {
                _resizable = value;
            }
        }
        
    
        #region Removed Properties
        [
        Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override bool AutoSize {
            get {
                return false;
            }
            set { ;}
        }
        [
        Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override Color BackColor {
            get {
                return new Color();
            }
            set { ;}
        }
        [
        Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override string Text {
            get {
                return "";
            }
            set { ;}
        }

        #endregion

        public SelectionBox() : this(Color.Red) {
        }
        public SelectionBox(Color color) {
            Pen pen = new Pen(color, 1);
            pen.DashStyle = DashStyle.Dash;
            _pen = pen;
        }
        public SelectionBox(Pen pen) {
            base.AutoSize = false;
            base.Text = "";
            _pen = pen;
        }

        protected override void OnPaintBackground(System.Windows.Forms.PaintEventArgs pevent) {

            base.OnPaintBackground(pevent);

            Graphics gfx = pevent.Graphics;

            UpdateRegion();
            Rectangle rect = new Rectangle(MARGIN / 2, MARGIN / 2, Width - MARGIN, Height - MARGIN);

            gfx.DrawRectangle(_pen, rect);

            if (_resizable) {
                //draw the resize handles
                Brush brush = new SolidBrush(_pen.Color);

                // bottom-right
                Rectangle bottomRightHandle = new Rectangle(MARGIN / 2 + RegionSize.Width - RESIZE_HANDLE_WIDTH,
                                                            MARGIN / 2 + RegionSize.Height - RESIZE_HANDLE_WIDTH,
                                                            RESIZE_HANDLE_WIDTH, RESIZE_HANDLE_WIDTH);
                                                    
                gfx.FillRectangle(brush, bottomRightHandle);
                
                // right
                Rectangle rightHandle = new Rectangle(MARGIN / 2 + RegionSize.Width - RESIZE_HANDLE_WIDTH,
                                                            MARGIN / 2 + RegionSize.Height / 2 - RESIZE_HANDLE_WIDTH,
                                                            RESIZE_HANDLE_WIDTH, RESIZE_HANDLE_WIDTH);
                
                gfx.FillRectangle(brush, rightHandle);

                // bottom
                Rectangle bottomHandle = new Rectangle(MARGIN / 2 + RegionSize.Width / 2 - RESIZE_HANDLE_WIDTH,
                                                            MARGIN / 2 + RegionSize.Height - RESIZE_HANDLE_WIDTH,
                                                            RESIZE_HANDLE_WIDTH, RESIZE_HANDLE_WIDTH);
                gfx.FillRectangle(brush, bottomHandle);

                // draw the move handle
                Rectangle moveHandle = new Rectangle(0, 0, RESIZE_HANDLE_WIDTH, RESIZE_HANDLE_WIDTH);
                gfx.FillRectangle(brush, moveHandle);
            }
            

        }

        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);

            if (!_resizable)
                return;

            _resizeMode = ResizeMode.NONE;

            /*if (e.Y >= this.Height - RESIZE_HANDLES_SIZE) {
                if (e.X >= this.Width / 2 - RESIZE_HANDLES_SIZE / 2&& e.X <= this.Width / 2 + RESIZE_HANDLES_SIZE / 2) {
                    _resizeMode = ResizeMode.BOTTOM;
                } else if (e.X >= this.Width - RESIZE_HANDLES_SIZE) {
                    _resizeMode = ResizeMode.BOTTOM_RIGHT;
                }
            }*/

            if (e.Y >= (MARGIN / 2 + RegionSize.Height) - RESIZE_HANDLE_WIDTH) {
                if (e.X >= MARGIN / 2 + RegionSize.Width / 2 - RESIZE_HANDLE_WIDTH && 
                    e.X <= MARGIN / 2 + RegionSize.Width / 2 + RESIZE_HANDLE_WIDTH) {
                    _resizeMode = ResizeMode.BOTTOM;
                }
                else if (e.X >= MARGIN / 2 + RegionSize.Width - RESIZE_HANDLE_WIDTH) {
                    _resizeMode = ResizeMode.BOTTOM_RIGHT;
                }
            }

            /*if (e.Y >= this.Height / 2 - RESIZE_HANDLES_SIZE / 2 && e.Y <= this.Height / 2 + RESIZE_HANDLES_SIZE / 2) {
                if (e.X >= this.Width - RESIZE_HANDLES_SIZE) {
                    _resizeMode = ResizeMode.RIGHT;
                }
            }*/
            if (e.Y >= MARGIN / 2 + RegionSize.Height / 2 - RESIZE_HANDLE_WIDTH && 
                e.Y <= MARGIN / 2 + RegionSize.Height / 2 + RESIZE_HANDLE_WIDTH) {
                if (e.X >= MARGIN / 2 + RegionSize.Width - RESIZE_HANDLE_WIDTH) {
                    _resizeMode = ResizeMode.RIGHT;
                }
            }

            /*if (e.X <= RESIZE_HANDLES_SIZE && e.Y <= RESIZE_HANDLES_SIZE) {
                _resizeMode = ResizeMode.MOVE;
            }*/

            if (e.X <= RESIZE_HANDLE_WIDTH && e.Y <= RESIZE_HANDLE_WIDTH) {
                _resizeMode = ResizeMode.MOVE;
            }

        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);

            if (_resizeMode == ResizeMode.NONE)
                return;

            //if (Math.Abs(e.X - _prevX) <= REDRAW_WHEN_MOVED && Math.Abs(e.Y - _prevY) <= REDRAW_WHEN_MOVED)
            //    return;

                switch (_resizeMode) {
                    case ResizeMode.BOTTOM_RIGHT:
                        if (e.X < 2 * MARGIN) {
                            Width = 2 * MARGIN;
                        }
                        else {
                            Width = e.X + MARGIN / 2;
                        }
                        if (e.Y < 2 * MARGIN) {
                            Height = 2 * MARGIN;
                        }
                        else {
                            Height = e.Y + MARGIN / 2;
                        }
                        
                        break;
                    case ResizeMode.BOTTOM:
                        if (e.Y < 2 * MARGIN) {
                            Height = 2 * MARGIN;
                        }
                        else {
                            Height = e.Y + MARGIN / 2;
                        }

                        break;
                    case ResizeMode.RIGHT:
                        if (e.X < 2 * MARGIN) {
                            Width = 2 * MARGIN;
                        }
                        else {
                            Width = e.X + MARGIN / 2;
                        }
                        break;
                    case ResizeMode.MOVE:
                        
                        //"boundary check" in mouseup
                        
                            Left += e.X - MARGIN / 2;
                            Top += e.Y - MARGIN / 2;
                       
                        break;
                }
              //  _prevX = e.X;
              //  _prevY = e.Y;

                
          
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            base.OnMouseUp(e);

            if (_resizeMode == ResizeMode.NONE)
                return;

            if (Left < -1 * MARGIN / 2)
                Left = -1 * MARGIN / 2;
            if (Left > Parent.Width - 2 * MARGIN)
                Left = Parent.Width - 2 * MARGIN;

            if (Top < -1 * MARGIN / 2)
                Top = -1 * MARGIN / 2;
            if (Top > Parent.Height - 2 * MARGIN)
                Top = Parent.Height - 2 * MARGIN;

            //if (Left + Width > this.Parent.Width + MARGIN / 2)
            if (Left + Width > this.Parent.Width + MARGIN)
                //Width = Parent.Width - Left;
                RegionSize = new Size(Parent.Width - Left, RegionSize.Height);

            //if (Top + Height > this.Parent.Height + MARGIN / 2)
                if (Top + Height > this.Parent.Height + MARGIN)
                //Height = Parent.Height - Top;
                    RegionSize = new Size(RegionSize.Width, Parent.Height - Top);

            _resizeMode = ResizeMode.NONE;

            //MessageBox.Show("Location: " + RegionLocation.ToString() + "\nSize: " + RegionSize.ToString());
        }

        private void UpdateRegion() {
            //set control region, so that it is empty in the middle
            GraphicsPath gPath = new GraphicsPath();

            // include the dashed line in the region;
            //rect = new Rectangle(RESIZE_HANDLES_SIZE / 2 - 1, RESIZE_HANDLES_SIZE / 2 - 1,
            //                                Width - RESIZE_HANDLES_SIZE / 2,
            //                                Height - RESIZE_HANDLES_SIZE / 2);
            Rectangle rect = new Rectangle(0, 0, base.Width, base.Height);
            gPath.AddRectangle(rect);
            //rect.Inflate(-2, -2);
            Rectangle inner = new Rectangle(0, 0, base.Width, base.Height);
            rect.Inflate(-1 * (MARGIN / 2 + RESIZE_HANDLE_WIDTH), -1 * (MARGIN / 2 + RESIZE_HANDLE_WIDTH));
            gPath.AddRectangle(rect);

            //include the handles in the region

            //gPath.AddRectangles(new Rectangle[] {rightHandle, bottomHandle, bottomRightHandle, moveHandle });

            Region = new Region(gPath);
        }

    }
}