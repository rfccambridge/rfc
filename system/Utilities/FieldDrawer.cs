using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using Robocup.Core;

namespace Robocup.Utilities
{
    public enum ArrowType
    {
        Destination,
        Waypoint
    }

    public class WaypointAddedEventArgs : EventArgs
    {
        public Object Object;
        public Color Color;
        public WaypointAddedEventArgs(Object obj, Color color)
        {
            Object = obj;
            Color = color;
        }
    }

    public class WaypointRemovedEventArgs : EventArgs
    {
        public Object Object;
        public WaypointRemovedEventArgs(Object obj)
        {
            Object = obj;
        }
    }

    public class WaypointMovedEventArgs : EventArgs
    {
        public Object Object;
        public Vector2 NewLocation;
        public WaypointMovedEventArgs(Object obj, Vector2 newLocation)
        {
            Object = obj;
            NewLocation = newLocation;
        }
    }

    public class FieldDrawer
    {
        public event EventHandler<WaypointAddedEventArgs> WaypointAdded;
        public event EventHandler<WaypointRemovedEventArgs> WaypointRemoved;
        public event EventHandler<WaypointMovedEventArgs> WaypointMoved;

        private class RobotDrawingInfo
        {
            public RobotInfo RobotInfo;
            public Dictionary<ArrowType, Arrow> Arrows = new Dictionary<ArrowType, Arrow>();
            public string PlayName;
            public RobotPath Path;
        
            public RobotDrawingInfo(RobotInfo robotInfo)
            {
                RobotInfo = robotInfo;
            }
        }

        private class Marker
        {
            public Vector2 Location;
            public Color Color;
            public object Object;

            public Marker(Vector2 loc, Color col, Object obj)
            {
                Location = loc;
                Color = col;
                Object = obj;
            }
        }

        private class Arrow
        {
            public Vector2 ToPoint;
            public Color Color;

            public Arrow(Vector2 toPoint, Color color)
            {         
                ToPoint = toPoint;
                Color = color;
            }
        }

        private class State
        {
            public Dictionary<Team, Dictionary<int, RobotDrawingInfo>> Robots = new Dictionary<Team, Dictionary<int, RobotDrawingInfo>>();
            public BallInfo Ball;
            public Dictionary<int, Marker> Markers = new Dictionary<int, Marker>();

            public int NextRobotHandle = 0;            
            public int NextMarkerHandle = 0;

            public State()
            {
                Robots.Add(Team.Yellow, new Dictionary<int, RobotDrawingInfo>());
                Robots.Add(Team.Blue, new Dictionary<int, RobotDrawingInfo>());
            }

            public void Clear()
            {                
                Robots[Team.Yellow].Clear();
                Robots[Team.Blue].Clear();
                Ball = null;
                //Markers.Clear();

                NextRobotHandle = 0;
                //NextMarkerHandle = 0;
          }
        }

        const double MARKER_SIZE = 0.025;

        double FIELD_WIDTH;
        double FIELD_HEIGHT;
        double REFEREE_ZONE_WIDTH;
        double CENTER_CIRCLE_RADIUS;

        FieldDrawerForm _fieldDrawerForm; 
        State _bufferedState = new State();
        State _state = new State();
        object _stateLock = new object();
        bool _collectingState = false;
        object _collectingStateLock = new object();
        bool _robotsAndBallUpdated = false;
        Marker _draggedMarker;
        double _glControlWidth;
        double _glControlHeight;

        IntPtr _ballQuadric, _centerCircleQuadric, _robotQuadric;
        OpenTK.Graphics.TextPrinter _printer = new OpenTK.Graphics.TextPrinter();
        double[] _modelViewMatrix = new double[16];
        double[] _projectionMatrix = new double[16];
        int[] _viewport = new int[4];

        public bool Visible
        {
            get { return _fieldDrawerForm.Visible; }
        }

        public FieldDrawer()
        {
            FIELD_HEIGHT = Constants.get<double>("plays", "FIELD_HEIGHT");
            FIELD_WIDTH = Constants.get<double>("plays", "FIELD_WIDTH");

            REFEREE_ZONE_WIDTH = Constants.get<double>("plays", "REFEREE_ZONE_WIDTH");
            CENTER_CIRCLE_RADIUS = Constants.get<double>("plays", "CENTER_CIRCLE_RADIUS");

            double ratio = FIELD_HEIGHT / FIELD_WIDTH;
            _fieldDrawerForm = new FieldDrawerForm(this, ratio);
        }

        public void Init(int w, int h)
        {
            _glControlWidth = w;
            _glControlHeight = h;

            GL.ClearColor(Color.Green);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(-REFEREE_ZONE_WIDTH - FIELD_WIDTH / 2, FIELD_WIDTH / 2 + REFEREE_ZONE_WIDTH,
                     -REFEREE_ZONE_WIDTH - FIELD_HEIGHT / 2, FIELD_HEIGHT / 2 + REFEREE_ZONE_WIDTH, -1, 1);
            GL.Viewport(0, 0, w, h); // Use all of the glControl painting area

            _ballQuadric = OpenTK.Graphics.Glu.NewQuadric();
            OpenTK.Graphics.Glu.QuadricDrawStyle(_ballQuadric, OpenTK.Graphics.QuadricDrawStyle.Fill);

            _centerCircleQuadric = OpenTK.Graphics.Glu.NewQuadric();
            OpenTK.Graphics.Glu.QuadricDrawStyle(_centerCircleQuadric, OpenTK.Graphics.QuadricDrawStyle.Silhouette);

            _robotQuadric = OpenTK.Graphics.Glu.NewQuadric();
            OpenTK.Graphics.Glu.QuadricDrawStyle(_robotQuadric, OpenTK.Graphics.QuadricDrawStyle.Line);

            // For debugging
            //BuildTestScene();
        }

        public void Resize(int w, int h)
        {
            _glControlWidth = w;
            _glControlHeight = h;            
            GL.Viewport(0, 0, w, h);
        }       

        public void Show()
        {
            _fieldDrawerForm.Show();
        }

        public void Hide()
        {
            _fieldDrawerForm.Hide();
        }

        public void MouseDown(Point loc)
        {
            Vector2 pt = controlToFieldCoords(loc);
            _draggedMarker = null;
            lock (_stateLock) {
                foreach (Marker marker in _state.Markers.Values)
                    if (ptInsideMarker(marker, pt))
                        _draggedMarker = marker;
            }
        }

        public void MouseUp(Point loc)
        {
            if (_draggedMarker != null)
            {
                if (loc.X < 0 || loc.X > _glControlWidth || loc.Y < 0 || loc.Y > _glControlHeight)
                {
                    if (WaypointRemoved != null)
                        WaypointRemoved(this, new WaypointRemovedEventArgs(_draggedMarker.Object));
                }
                _draggedMarker = null;
            }
        }

        public void MouseMove(Point loc)
        {
            if (_draggedMarker != null)
            {
                Vector2 pt = controlToFieldCoords(loc);
                lock (_collectingStateLock)
                {
                    _draggedMarker.Location = pt;
                    if (WaypointMoved != null)
                        WaypointMoved(this, new WaypointMovedEventArgs(_draggedMarker.Object, pt));
                }
            }
            _fieldDrawerForm.InvalidateGLControl();
        }

        public void DragDrop(object obj, Point loc)
        {
            if (obj.GetType() == typeof(WaypointAddedEventArgs))
            {
                WaypointAddedEventArgs eventArgs = obj as WaypointAddedEventArgs;
                RobotInfo waypoint = eventArgs.Object as RobotInfo;
                waypoint.Position = controlToFieldCoords(loc);
                if (WaypointAdded != null)
                    WaypointAdded(this, eventArgs);
            }
        }

        public void BeginCollectState()
        {
            lock (_collectingStateLock)
            {
                if (!_collectingState)
                    _collectingState = true;
                else
                    throw new ApplicationException("Already collecting state!");
            }
            _bufferedState.Clear();
            _robotsAndBallUpdated = false;
        }
        public void EndCollectState()
        {
            lock (_collectingStateLock)
            {
                if (_collectingState)
                    _collectingState = false;
                else
                    throw new ApplicationException("Never began collecting state!");
            }

            lock (_stateLock)
            {
                // Apply the modifications stored in the buffer

                if (_robotsAndBallUpdated)
                {
                    _state.Ball = _bufferedState.Ball;                    
                    foreach (Team team in Enum.GetValues(typeof(Team)))
                    {
                        _state.Robots[team].Clear();
                        foreach (KeyValuePair<int, RobotDrawingInfo> pair in _bufferedState.Robots[team])
                            _state.Robots[team].Add(pair.Key, pair.Value);
                    }                    
                }

                foreach (KeyValuePair<int, Marker> pair in _bufferedState.Markers)
                {
                    if (pair.Value != null)
                    {
                        if (_state.Markers.ContainsKey(pair.Key))
                            _state.Markers[pair.Key] = pair.Value;
                        else
                            _state.Markers.Add(pair.Key, pair.Value);
                    }
                    else
                    {
                        if (_state.Markers.ContainsKey(pair.Key))
                            _state.Markers.Remove(pair.Key);
                    }
                }
            }

            _fieldDrawerForm.InvalidateGLControl();
        }

        public void Paint()
        {
            lock (_stateLock)
            {                
                drawField();
                foreach (Marker marker in _state.Markers.Values)
                    drawMarker(marker);
                foreach (Team team in Enum.GetValues(typeof(Team)))
                    foreach (RobotDrawingInfo robot in _state.Robots[team].Values)                
                        drawRobot(robot);             

                if (_state.Ball != null)
                    drawBall(_state.Ball);
            }
        }

        public void UpdateTeam(Team team)
        {
            _fieldDrawerForm.UpdateTeam(team);
        }

        public void UpdateRefBoxCmd(string refBoxCmd)
        {
            _fieldDrawerForm.UpdateRefBoxCmd(refBoxCmd);
        }

        public void UpdatePlayType(PlayType playType)
        {
            _fieldDrawerForm.UpdatePlayType(playType);
        }

        public void UpdateInterpretFreq(double freq)
        {
            _fieldDrawerForm.UpdateInterpretFreq(freq);
        }

        public void UpdateInterpretDuration(double duration)
        {
            _fieldDrawerForm.UpdateInterpretDuration(duration);
        }

        public void UpdateLapDuration(double duration)
        {
            _fieldDrawerForm.UpdateLapDuration(duration);
        }

        public void UpdatePlayName(Team team, int robotID, string name)
        {
            lock (_collectingStateLock)
            {
                if (!_collectingState)
                    return;
                if (_bufferedState.Robots[team].ContainsKey(robotID))
                    _bufferedState.Robots[team][robotID].PlayName = name;                
            }
        }

        public void UpdateRobotsAndBall(List<RobotInfo> robots, BallInfo ball)
        {
            lock (_collectingStateLock)
            {
                if (!_collectingState)
                    return;

                _robotsAndBallUpdated = true;
                _bufferedState.Ball = ball;

                foreach (Team team in Enum.GetValues(typeof(Team)))
                    _bufferedState.Robots[team].Clear();
                foreach (RobotInfo robot in robots)
                    // Note: robots with duplicate ID's are ignored
                    if (!_bufferedState.Robots[robot.Team].ContainsKey(robot.ID))
                        _bufferedState.Robots[robot.Team].Add(robot.ID, new RobotDrawingInfo(robot));
            }
        }

        public int AddMarker(Vector2 location, Color color)
        {
            return AddMarker(location, color, null);
        }

        public int AddMarker(Vector2 location, Color color, Object obj)
        {
            lock (_collectingStateLock)
            {
                if (!_collectingState)
                    throw new ApplicationException("Not collecting state!");
                int handle = _bufferedState.NextMarkerHandle;
                _bufferedState.Markers.Add(handle, new Marker(location, color, obj));
                unchecked { _bufferedState.NextMarkerHandle++; }
                return handle;
            }
        }
        public void RemoveMarker(int handle)
        {
            lock (_collectingStateLock)
            {
                if (!_collectingState)
                    throw new ApplicationException("Not collecting state!");
                if (!_state.Markers.ContainsKey(handle))
                    throw new ApplicationException("Trying to remove an object that doesn't exist!");
                if (_bufferedState.Markers.ContainsKey(handle))
                    _bufferedState.Markers[handle] = null;
                else
                    _bufferedState.Markers.Add(handle, null);
            }
        }
        public void UpdateMarker(int handle, Vector2 location, Color color)
        {
            throw new NotImplementedException();
        }
        public Color GetMarkerColor(int handle)
        {
            lock (_stateLock) {
                return _state.Markers[handle].Color;
            }
        }

        public void DrawArrow(Team team, int robotID, ArrowType type, Vector2 toPoint)
        {
            Color color = Color.Black;

            switch (type) {
                case ArrowType.Destination: color = Color.Red; break;
                case ArrowType.Waypoint: color = Color.LightPink; break;
            }

            lock (_collectingStateLock)
            {
                if (_bufferedState.Robots[team].ContainsKey(robotID))
                {
                    if (_bufferedState.Robots[team][robotID].Arrows.ContainsKey(type))
                        _bufferedState.Robots[team][robotID].Arrows[type].ToPoint = toPoint;
                    else
                        _bufferedState.Robots[team][robotID].Arrows.Add(type, new Arrow(toPoint, color));
                }
            }
        }

        public void DrawPath(RobotPath path)
        {
            Team team = path.Team;
            int robotID = path.ID;

            lock (_collectingStateLock)
            {
                _bufferedState.Robots[team][robotID].Path = path;
            }
        }
        

        private void drawField()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.Color3(Color.White);

            // Field border
            GL.Begin(BeginMode.LineLoop);
            GL.Vertex2(-FIELD_WIDTH / 2, -FIELD_HEIGHT / 2);
            GL.Vertex2(FIELD_WIDTH / 2, -FIELD_HEIGHT / 2);
            GL.Vertex2(FIELD_WIDTH / 2, FIELD_HEIGHT / 2);
            GL.Vertex2(-FIELD_WIDTH / 2, FIELD_HEIGHT / 2);
            GL.End();

            // Center line
            GL.Begin(BeginMode.Lines);
            GL.Vertex2(0, FIELD_HEIGHT / 2);
            GL.Vertex2(0, -FIELD_HEIGHT / 2);
            GL.End();

            // Center circle
            const int SLICES = 20;
            GL.LoadIdentity();
            //GL.Translate(0, 0, 0);            
            GL.Begin(BeginMode.LineLoop);
            OpenTK.Graphics.Glu.Disk(_centerCircleQuadric, 0, CENTER_CIRCLE_RADIUS, SLICES, 1);
            GL.End();
        }

        private void drawRobot(RobotDrawingInfo drawingInfo)
        {
            const double ROBOT_RADIUS = 0.08;
            const double ROBOT_ARC_SWEEP = 270; // degrees
            const int SLICES = 10;

            RobotInfo robot = drawingInfo.RobotInfo;
            double angle = (robot.Orientation + Math.PI) * 180 / Math.PI;

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Translate(robot.Position.X, robot.Position.Y, 0);
            GL.Rotate(angle, 0, 0, 1);
            GL.Color3(robot.Team == Team.Yellow ? Color.Yellow : Color.Blue);            
            GL.Begin(BeginMode.Polygon);
            OpenTK.Graphics.Glu.PartialDisk(_robotQuadric, 0, ROBOT_RADIUS, SLICES, 1,
                                            -(360 - ROBOT_ARC_SWEEP) / 2, ROBOT_ARC_SWEEP);
            GL.End();

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            // TODO: Figure out the real way to render text!
            renderString(robot.ID.ToString(), robot.Position + new Vector2(-0.03, 0.045), Color.Red, 8);
            renderString(drawingInfo.PlayName, robot.Position + new Vector2(-0.05, -0.05), Color.Cyan, 8);

            foreach (Arrow arrow in drawingInfo.Arrows.Values)                
                drawArrow(robot.Position, arrow.ToPoint, arrow.Color);

            if (drawingInfo.Path != null)
                drawPath(drawingInfo.Path);
        }

        private void drawBall(BallInfo ball)
        {
            const double BALL_RADIUS = 0.02;
            const int SLICES = 8;
            
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Translate(ball.Position.X, ball.Position.Y, 0);
            GL.Color3(Color.Orange);
            GL.Begin(BeginMode.Polygon);
            OpenTK.Graphics.Glu.Disk(_ballQuadric, 0, BALL_RADIUS, SLICES, 1);
            GL.End();
        }

        private void drawMarker(Marker marker)
        {            
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Translate(marker.Location.X, marker.Location.Y, 0);
            GL.Color3(marker.Color);
            GL.Begin(BeginMode.Quads);
            GL.Vertex2(-MARKER_SIZE, MARKER_SIZE);
            GL.Vertex2(MARKER_SIZE, MARKER_SIZE);
            GL.Vertex2(MARKER_SIZE, -MARKER_SIZE);
            GL.Vertex2(-MARKER_SIZE, -MARKER_SIZE);
            GL.End();
        }

        private void drawPath(RobotPath path)
        {
            const double WAYPOINT_RADIUS = 0.02;
            const int SLICES = 8;

            if (path.isEmpty())
                return;

            foreach (RobotInfo waypoint in path.Waypoints)
            {
                //GL.Vertex2(waypoint.Position.X, waypoint.Position.Y);
                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadIdentity();
                GL.Color3(path.Team == Team.Yellow ? Color.Yellow : Color.Blue);
                GL.Translate(waypoint.Position.X, waypoint.Position.Y, 0);
                GL.Begin(BeginMode.Polygon);
                OpenTK.Graphics.Glu.Disk(_ballQuadric, 0, WAYPOINT_RADIUS, SLICES, 1);
                GL.End();
            }
        }

        private bool ptInsideMarker(Marker marker, Vector2 pt)
        {
            if (pt.X < marker.Location.X - MARKER_SIZE || pt.X > marker.Location.X + MARKER_SIZE
                || pt.Y < marker.Location.Y - MARKER_SIZE || pt.Y > marker.Location.Y + MARKER_SIZE)
                return false;
            return true;
        }

        private void drawArrow(Vector2 fromPoint, Vector2 toPoint, Color color)
        {
            const double TIP_HEIGHT = 0.15;
            const double TIP_BASE = 0.1;
            double angle = Math.Atan2(toPoint.Y - fromPoint.Y, toPoint.X - fromPoint.X) * 180 / Math.PI;
            double length = Math.Sqrt(fromPoint.distanceSq(toPoint));
            GL.Color3(color);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Translate(fromPoint.X, fromPoint.Y, 0);
            GL.Rotate(angle, 0, 0, 1);
            GL.Begin(BeginMode.Triangles);
            GL.Vertex2(0, 0);
            GL.Vertex2(length - TIP_HEIGHT, TIP_BASE / 4);
            GL.Vertex2(length - TIP_HEIGHT, -TIP_BASE / 4);
            GL.Vertex2(length, 0);
            GL.Vertex2(length - TIP_HEIGHT, TIP_BASE / 2);
            GL.Vertex2(length - TIP_HEIGHT, -TIP_BASE / 2);
            GL.End();
        }

        private OpenTK.Vector3 worldToScreen(OpenTK.Vector3 world)
        {
            OpenTK.Vector3 screen;          

            GL.GetInteger(GetPName.Viewport, _viewport);
            GL.GetDouble(GetPName.ModelviewMatrix, _modelViewMatrix);
            GL.GetDouble(GetPName.ProjectionMatrix, _projectionMatrix);


            OpenTK.Graphics.Glu.Project(world, _modelViewMatrix, _projectionMatrix, _viewport,
                                        out screen);

            screen.Y = _viewport[3] - screen.Y;
            return screen;
        }

        private void renderString(string s, Vector2 location, Color color, float size)
        {
            OpenTK.Vector3 screen = worldToScreen(new OpenTK.Vector3((float)location.X, (float)location.Y, 0.0f));            

            _printer.Begin();
            GL.Translate(screen);
            _printer.Print(s, new Font(FontFamily.GenericSansSerif, size), color);
            _printer.End();
        }

        private Vector2 controlToFieldCoords(Point loc)
        {
            double viewWidth = 2 * REFEREE_ZONE_WIDTH + FIELD_WIDTH;
            double viewHeight = 2 * REFEREE_ZONE_WIDTH + FIELD_HEIGHT;
            double translateX = -FIELD_WIDTH / 2 - REFEREE_ZONE_WIDTH;
            double translateY = -FIELD_HEIGHT / 2 - REFEREE_ZONE_WIDTH;
            return new Vector2((double)loc.X / _glControlWidth * viewWidth + translateX, 
                               (1 - (double)loc.Y / _glControlHeight) * viewHeight + translateY);
        }

        private void BuildTestScene()
        {
            List<RobotInfo> robots = new List<RobotInfo>();
            robots.Add(new RobotInfo(new Vector2(0, 0), Math.PI / 2, 2));
            robots.Add(new RobotInfo(new Vector2(2, 1.2), Math.PI, 3));
            BallInfo ball = new BallInfo(new Vector2(1, 2));

            Vector2 marker1loc = new Vector2(-0.5, 0.5);
            Vector2 marker2loc = new Vector2(-1.5, 1);

            BeginCollectState();

            UpdateRobotsAndBall(robots, ball);
            AddMarker(marker1loc, Color.Red);
            AddMarker(marker2loc, Color.Cyan);

            DrawArrow(Team.Yellow, 2, ArrowType.Destination, marker1loc);
            DrawArrow(Team.Blue, 2, ArrowType.Waypoint, marker2loc);

            EndCollectState();
        }

    }
}
