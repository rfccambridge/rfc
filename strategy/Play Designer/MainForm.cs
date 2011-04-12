using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using Robocup.Geometry;
using Robocup.Core;
using Robocup.Utilities;

namespace Robocup.Plays
{
    /// <summary>
    /// The main form for the Play Designer
    /// </summary>
    partial class MainForm : Form, ICoordinateConverter
    {
        #region Private Fields
        /// <summary>
        /// represents the "tick" of the play, so that things only get reevaluated when things change (and the tick increases)
        /// </summary>
        int tick = 0;
        /// <summary>
        /// The ArrayList holding all the toolbar buttons in the "Action" group; this is used so that only one of them may be pressed at any time.
        /// </summary>
        private ArrayList actionToolstripButtons = new ArrayList();
        /// <summary>
        /// The form that shows you the conditions and actions you've already created.
        /// </summary>
        ShowExpressionsForm showForm;
        /// <summary>
        /// The form that shows you which order in which the robots are going to be defined.
        /// </summary>
        DefinitionForm definitionForm;
        /// <summary>
        /// The current state; it holds all the information that is specific for a current action.
        /// For instance, when drawing a line, this is the place to store what point was the first one pressed, etc.
        /// </summary>
        State state = new state_AddingRobot();

        /// <summary>
        /// The list of arrows currently being drawn on the field.
        /// </summary>
        ArrayList arrows = new ArrayList();

        /// <summary>
        /// This is the play that houses all the information.  Right now, it's not really necessary,
        /// but maybe eventually if the ability to edit more than one play at once is added (such as
        /// a group of similar plays) this will be nice.
        /// </summary>
        DesignerPlay play = new DesignerPlay();
        public DesignerPlay Play
        {
            get { return play; }
        }

        //Ball ball;

        /// <summary>
        /// This keeps track of how many debug lines have been shown to the user.  This is useful, for
        /// instance, if the same line is shown twice.  This way the user knows that the line is being
        /// shown a second time.
        /// </summary>
        int numdebuglines = 0;
        #endregion

        /// <summary>
        /// Shows a debug line to the user, in the status bar at the bottom of the form.
        /// </summary>
        private void showDebugLine(string s)
        {
            numdebuglines++;
            statusLabel.Text = numdebuglines + ": " + s;
        }

        #region GetClickedOn methods
        private DesignerExpression getClickedOn(Vector2 p)
        {
            return getClickedOn(p, typeof(Vector2), typeof(DesignerRobot), typeof(Line), typeof(Circle));
        }
        /// <summary>
        /// Takes a list of clickable objects and a point, and returns the first one such that the point is on that object
        /// (as defined by that objects willClick() method)
        /// </summary>
        /// <param name="al">The ArrayList holding the objects</param>
        /// <param name="p">The point p that was clicked</param>
        /// <returns>Returns the object that was clicked on, or null if none of them were.</returns>
        /// <param name="types">The types that will be checked for being clicked on</param>
        private DesignerExpression getClickedOn(Vector2 p, params Type[] types)
        {
            foreach (DesignerExpression exp in play.getAllObjects())
            {
                bool ok = false;
                foreach (Type t in types)
                {
                    if (t.IsAssignableFrom(exp.ReturnType))
                    {
                        ok = true;
                        break;
                    }
                }
                if (!ok)
                    continue;
                object c = exp.getValue(tick, null);
                //if (c.willClick(p))
                if (willClick(c, p))
                    return exp;
            }
            return null;
            //return new DesignerExpression(null);
        }
        private bool willClick(object o, Vector2 p)
        {
            if (o is Vector2)
            {
                //DesignerPoint dp = new DesignerPoint((Vector2)o);
                //return dp.willClick(p);
                return UsefulFunctions.distance((Vector2)o, p) <= pixeltofieldDistance(6);
            }
            else if (o is DesignerRobot)
            {
                return UsefulFunctions.distance(((DesignerRobot)o).getPoint(), p) <= pixeltofieldDistance(10);
            }
            else if (o is Line)
            {
                //change these two lines to make it so that you have to click on the segment itself, and not on its extension
                //return ((Line)o).distFromSegment(p) <= PixelDistanceToFieldDistance(3);
                return ((Line)o).distFromLine(p) <= pixeltofieldDistance(3);
            }
            else if (o is Circle)
                return Math.Abs(((Circle)o).distanceFromCenter(p) - ((Circle)o).Radius) <= pixeltofieldDistance(3);
            throw new ApplicationException("Tried to see if you could click on something that's not supported yet");
            //return false;
        }
        #endregion

        /// <summary>
        /// Creates a new form to edit the given play (in text format).  If the play is null, creates 
        /// a new one.
        /// </summary>
        /// <param name="toEdit"></param>
        public MainForm(string toEdit)
        {
            init();
            if (toEdit != null)
            {
                PlayLoader<DesignerPlay, DesignerTactic, DesignerExpression> loader =
                    new PlayLoader<DesignerPlay, DesignerTactic, DesignerExpression>(new DesignerExpression.Factory());
                play = loader.load(toEdit);
            }

            repaint();
        }

        public MainForm()
        {
            init();
            toolstripExitReturn.Visible = false;

            repaint();
        }

        private void init()
        {
            InitializeComponent();

            //I don't know why, but if you set a combo box to be uneditable (the user can't add
            //their own values), then you can't specify at compile time what the initial selection is.
            //toolstripPlayType.SelectedIndex = 0;
            toolstripPlayType.Items.Clear();
            toolstripPlayType.Items.AddRange(Enum.GetNames(typeof(PlayType)));
            toolstripPlayType.SelectedIndex = 0;

            showForm = new ShowExpressionsForm(this);
            showForm.Show();
            definitionForm = new DefinitionForm(this);
            definitionForm.Show();

            //for some reason, this toolstrip keeps on changing its position.  so I set it here to make sure it stays where I want it
            definitionsToolStrip.Location = new Point(0, 0);

            //We add here all the buttons and their associated states, that signify which action the user is doing.
            //Since the user can only be doing one action at once, only one button can be pressed at once.
            actionToolstripButtons.Add(new ToolstripButtonAndState(toolstripAddRobot, new state_AddingRobot()));
            actionToolstripButtons.Add(new ToolstripButtonAndState(toolstripMoveObjects, new state_MovingObjects()));
            actionToolstripButtons.Add(new ToolstripButtonAndState(toolstripDrawLine, new state_DrawingLine()));
            actionToolstripButtons.Add(new ToolstripButtonAndState(toolstripPlaceIntersection, new state_PlacingIntersection()));
            actionToolstripButtons.Add(new ToolstripButtonAndState(toolstriponsAddClosestDefinition, new state_AddingClosestCondition()));
            actionToolstripButtons.Add(new ToolstripButtonAndState(toolstripAddBall, new state_AddingBall()));
            actionToolstripButtons.Add(new ToolstripButtonAndState(toolstripAddPoint, new state_AddingPoint()));
            actionToolstripButtons.Add(new ToolstripButtonAndState(toolstripAddCircle, new state_AddingCircle()));
            actionToolstripButtons.Add(new ToolstripButtonAndState(toolstripEditObject, new state_EditingObject()));

            double minx = pixeltofieldX(30), maxx = pixeltofieldX(30 + 610);
            double miny = pixeltofieldY(56), maxy = pixeltofieldY(56 + 420);
            this.Size = new Size(30 + 610 + 37, 56 + 420 + 91);
            double goalback = pixeltofieldDistance(10);// 12;
            play.AddPlayObject(new DesignerExpression(Function.getFunction("point"), minx, miny), "topLeftCorner");
            play.AddPlayObject(new DesignerExpression(Function.getFunction("point"), minx, maxy), "bottomLeftCorner");
            play.AddPlayObject(new DesignerExpression(Function.getFunction("point"), maxx, maxy), "bottomRightCorner");
            play.AddPlayObject(new DesignerExpression(Function.getFunction("point"), maxx, miny), "topRightCorner");

            Version v = System.Reflection.Assembly.GetAssembly(this.GetType()).GetName(false).Version;
            showDebugLine("This is Play Designer version " + v.Major + "." + v.Minor + ", build " + v.Build);
        }

        #region "Action Button" stuff
        /// <summary>
        /// This struct just pairs buttons and the state they represent together.
        /// </summary>
        struct ToolstripButtonAndState
        {
            public ToolStripButton button;
            public State state;
            public ToolstripButtonAndState(ToolStripButton button, State state)
            {
                this.button = button;
                this.state = state;
            }
        }
        /// <summary>
        /// When any of the toolstrip "action buttons" are pressed, they call this function.
        /// We can tell which button was pressed by the sender parameter.
        /// </summary>
        private void actionToolstripButton_Click(object sender, EventArgs e)
        {
            if (state is state_SelectingObject && ((state_SelectingObject)state).editForm != null)
            {
                showDebugLine("You can't switch out of selecting an object!");
                return;
            }

            ToolStripButton b = (ToolStripButton)sender;
            foreach (ToolstripButtonAndState t in actionToolstripButtons)
            {
                if (t.button == b)
                {
                    state = (State)Activator.CreateInstance(t.state.GetType());
                }
                t.button.Checked = false;
                t.button.CheckState = CheckState.Unchecked;
            }
            b.Checked = true;
            b.CheckState = CheckState.Checked;
        }
        /// <summary>
        /// Self explanatory: it clears all the checked action buttons.
        /// </summary>
        private void clearCheckedButtons()
        {
            foreach (ToolstripButtonAndState t in actionToolstripButtons)
            {
                t.button.Checked = false;
            }
        }
        #endregion

        #region MouseHandlers
        #region state_AddingRobot
        private void state_AddingRobot_MouseDown(Vector2 clickPoint, MouseEventArgs e)
        {
            state_AddingRobot s = (state_AddingRobot)state;
            bool ours = true;
            if (e.Button == MouseButtons.Right)
                ours = false;
            DesignerRobot r = new DesignerRobot(clickPoint, ours);
            DesignerExpression exp = new DesignerExpression(r);
            play.AddPlayObject(exp, r.getName());
            play.addRobot(exp);
            play.AddPlayObject(new DesignerExpression(Function.getFunction("pointof"), exp));
            repaint();
        }
        private void state_AddingRobot_MouseMove(Vector2 clickPoint, MouseEventArgs e) { }
        private void state_AddingRobot_MouseUp(Vector2 clickPoint, MouseEventArgs e) { }
        #endregion
        #region state_EditingObject
        private void state_EditingObject_MouseDown(Vector2 clickPoint, MouseEventArgs e)
        {
            DesignerExpression exp = getClickedOn(clickPoint, typeof(Vector2));
            if (exp == null)
                exp = getClickedOn(clickPoint);
            if (e.Button == MouseButtons.Left)
            {
                if (exp != null && !(typeof(PlayRobotDefinition).IsAssignableFrom(exp.ReturnType)))
                {
                    editExpression(exp);
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (exp != null)
                {
                    play.delete(exp);
                    repaint();
                }
            }
            else if (e.Button == MouseButtons.Middle)
            {
                createExpression(typeof(object));
            }
        }
        private void state_EditingObject_MouseMove(Vector2 clickPoint, MouseEventArgs e) { }
        private void state_EditingObject_MouseUp(Vector2 clickPoint, MouseEventArgs e) { }
        #endregion
        #region state_MovingObjects
        private int findHover(state_MovingObjects s, Vector2 clickPoint)
        {
            s.robot = getClickedOn(clickPoint, typeof(DesignerRobot));
            DesignerExpression point = getClickedOn(clickPoint, typeof(Vector2));
            if (point == null || !point.IsFunction ||
                (point.theFunction.Name == "point" &&
                  (point.getArgument(0).IsFunction || point.getArgument(1).IsFunction)))
                point = null;
            s.point = point;
            if (play.Ball != null && play.Ball.willClick(clickPoint))
                s.ball = play.Ball;
            int numclickedon = 0;
            if (s.robot != null)
                numclickedon++;
            if (s.point != null)
                numclickedon++;
            if (s.ball != null)
                numclickedon++;
            if (numclickedon == 2 && s.point != null && s.point.UsesFunction("pointof"))
            {
                numclickedon--;
                s.point = null;
            }
            return numclickedon;
        }
        private void state_MovingObjects_MouseDown(Vector2 clickPoint, MouseEventArgs e)
        {
            state_MovingObjects s = (state_MovingObjects)state;
            int numclickedon = findHover(s, clickPoint);
            if (numclickedon != 1)
            {
                showDebugLine("You clicked on " + numclickedon + " movable objects.");
                s.robot = null;
                s.point = null;
                s.ball = null;
            }
            s.setMouse(clickPoint);
        }
        private void state_MovingObjects_MouseMove(Vector2 clickPoint, MouseEventArgs e)
        {
            state_MovingObjects s = (state_MovingObjects)state;
            if (s.mousedown)
            {
                bool needtorepaint = false;
                if (s.robot != null)
                {
                    //s.robot.translate(s.diff(clickPoint));
                    ((DesignerRobot)s.robot.getValue(tick, null)).translate(s.diff(clickPoint));
                    needtorepaint = true;
                }
                else if (s.point != null)
                {
                    //s.point.translate(s.diff(clickPoint));
                    s.point.setArgument(0, clickPoint.X);
                    s.point.setArgument(1, clickPoint.Y);
                    needtorepaint = true;
                }
                else if (s.ball != null)
                {
                    s.ball.setPosition(s.ball.getPoint() + s.diff(clickPoint));
                    needtorepaint = true;
                }
                s.setMouse(clickPoint);
                if (needtorepaint)
                    repaint();
            }
            else
            {
                if (s.robot != null)
                    ((DesignerRobot)s.robot.getValue(0, null)).unhighlight();
                if (s.point != null)
                    s.point.Highlighted = false;
                findHover(s, clickPoint);
                if (s.robot != null && s.point != null)
                {
                    s.point = null;
                    s.robot = null;
                }
                if (s.point != null)
                    s.point.Highlighted = true;
                if (s.robot != null)
                    ((DesignerRobot)s.robot.getValue(0, null)).highlight();
                repaint();
            }
        }
        private void state_MovingObjects_MouseUp(Vector2 clickPoint, MouseEventArgs e)
        {
            state_MovingObjects s = (state_MovingObjects)state;
            if (s.robot != null)
                s.robot.Highlighted = false;
            if (s.point != null)
                s.point.Highlighted = false;
            state = new state_MovingObjects();
        }
        #endregion
        #region state_DrawingLine
        private void state_DrawingLine_MouseDown(Vector2 clickPoint, MouseEventArgs e)
        {
            state_DrawingLine s = (state_DrawingLine)state;
            s.firstpoint = getClickedOn(clickPoint, typeof(Vector2));
            if (s.firstpoint != null)
            {
                s.line = new DesignerExpression(Function.getFunction("line"), s.firstpoint, s.firstpoint);
                play.AddPlayObject(s.line);
            }
        }
        private void state_DrawingLine_MouseMove(Vector2 clickPoint, MouseEventArgs e)
        {
            if (state.mousedown && ((state_DrawingLine)state).line != null)
            {
                state_DrawingLine s = (state_DrawingLine)state;
                DesignerExpression point2 = getClickedOn(clickPoint, typeof(Vector2));
                if (point2 == null)
                {
                    point2 = new DesignerExpression(clickPoint);
                }
                s.line.setArgument(1, point2);
                repaint();
            }
        }
        private void state_DrawingLine_MouseUp(Vector2 clickPoint, MouseEventArgs e)
        {
            state_DrawingLine s = (state_DrawingLine)state;
            if (s.line != null)
            {
                DesignerExpression point2 = getClickedOn(clickPoint, typeof(Vector2));
                if (point2 == null)
                {
                    point2 = new DesignerExpression(Function.getFunction("point"), clickPoint.X, clickPoint.Y);
                    play.AddPlayObject(point2);
                }
                s.line.setArgument(1, point2);
                s.line = null;
                repaint();
            }
        }
        #endregion
        #region state_AddingClosestCondition
        private void findHover(state_AddingClosestCondition s, Vector2 clickPoint)
        {
            s.firstpoint = getClickedOn(clickPoint, typeof(Vector2));
            s.firstrobot = getClickedOn(clickPoint, typeof(DesignerRobot));
            if (s.firstpoint != null && s.firstrobot != null)
            {
                if (s.firstpoint.UsesFunction("pointof"))
                    s.firstpoint = null;
            }
            if (s.firstpoint != null && s.firstrobot != null)
            {
                s.firstpoint = null;
                s.firstrobot = null;
            }
        }
        private void state_AddingClosestCondition_MouseDown(Vector2 clickPoint, MouseEventArgs e) { }
        private void state_AddingClosestCondition_MouseMove(Vector2 clickPoint, MouseEventArgs e)
        {
            if (!state.mousedown)
            {
                state_AddingClosestCondition s = (state_AddingClosestCondition)state;
                if (s.firstrobot != null)
                    ((DesignerRobot)s.firstrobot.getValue(0, null)).unhighlight();
                if (s.firstpoint != null)
                    s.firstpoint.Highlighted = false;
                findHover(s, clickPoint);
                if (s.firstpoint != null)
                    s.firstpoint.Highlighted = true;
                if (s.firstrobot != null)
                    ((DesignerRobot)s.firstrobot.getValue(0, null)).highlight();
                repaint();
            }
        }
        private void state_AddingClosestCondition_MouseUp(Vector2 clickPoint, MouseEventArgs e)
        {
            state_AddingClosestCondition s = (state_AddingClosestCondition)state;
            DesignerExpression secondpoint = getClickedOn(clickPoint, typeof(Vector2));
            DesignerExpression secondrobot = getClickedOn(clickPoint, typeof(DesignerRobot));
            if (s.firstrobot != null && secondpoint != null)
            {
                DesignerRobotDefinition df = new ClosestDefinition(s.firstrobot, secondpoint);
                ((DesignerRobot)s.firstrobot.getValue(tick, null)).setDefinition(df);
                definitionForm.updateList();
            }
            else if (s.firstpoint != null && secondrobot != null)
            {
                DesignerRobotDefinition df = new ClosestDefinition(secondrobot, s.firstpoint);
                ((DesignerRobot)secondrobot.getValue(tick, null)).setDefinition(df);
                definitionForm.updateList();
            }
            repaint();
        }
        #endregion
        #region state_AddingBall
        private void state_AddingBall_MouseDown(Vector2 clickPoint, MouseEventArgs e)
        {
            if (play.Ball == null)
            {
                play.Ball = new DesignerBall(clickPoint);
                //play.Points.Add(new DesignerPoint(play.Ball));
                play.AddPlayObject(new DesignerExpression(Function.getFunction("pointof"), play.Ball));
            }
            else
                play.Ball.setPosition(clickPoint);
            repaint();
        }
        private void state_AddingBall_MouseMove(Vector2 clickPoint, MouseEventArgs e) { }
        private void state_AddingBall_MouseUp(Vector2 clickPoint, MouseEventArgs e) { }
        #endregion
        #region state_SelectingObject
        private void state_SelectingObject_MouseDown(Vector2 clickPoint, MouseEventArgs e)
        {
            state_SelectingObject s = (state_SelectingObject)state;
            if (s.editForm == null || s.t == null)
                return;
            if (e.Button == MouseButtons.Right)
            {
                //just in case you click the right mouse button when you're not selecting anything:
                if (s.returnDelegate != null)
                {
                    s.returnDelegate(null);
                    s.t = null;
                    return;
                }
            }
            DesignerExpression exp = getClickedOn(clickPoint, s.t);
            if (exp != null)
            {
                s.t = null;
                exp.Highlighted = false;
                if (exp.ReturnType.IsAssignableFrom(typeof(DesignerRobot)))
                {
                    ((DesignerRobot)exp.getValue(tick, null)).unhighlight();
                }
                s.returnDelegate(exp);
            }
        }
        private void state_SelectingObject_MouseMove(Vector2 clickPoint, MouseEventArgs e)
        {
            state_SelectingObject s = (state_SelectingObject)state;
            if (s.t != null)
            {
                List<DesignerExpression> clickables = play.getAllObjects();
                foreach (DesignerExpression exp in clickables)
                {
                    object c = exp.getValue(tick, null);
                    if (!s.t.IsAssignableFrom(c.GetType()))
                        continue;

                    if (c is DesignerRobot)
                    {
                        if (willClick(c, clickPoint))
                            ((DesignerRobot)c).highlight();
                        else

                            ((DesignerRobot)c).unhighlight();
                    }
                    else
                    {
                        exp.Highlighted = willClick(c, clickPoint);
                    }
                }
                repaint();
            }
        }
        private void state_SelectingObject_MouseUp(Vector2 clickPoint, MouseEventArgs e) { }
        #endregion
        #region state_AddingPoint
        private void state_AddingPoint_MouseDown(Vector2 clickPoint, MouseEventArgs e)
        {
            state_AddingPoint s = (state_AddingPoint)state;
            s.point = new DesignerExpression(Function.getFunction("point"), clickPoint.X, clickPoint.Y);
            play.AddPlayObject(s.point);
            repaint();
        }
        private void state_AddingPoint_MouseMove(Vector2 clickPoint, MouseEventArgs e)
        {
            state_AddingPoint s = (state_AddingPoint)state;
            if (s.mousedown)
            {
                s.point.setArgument(0, clickPoint.X);
                s.point.setArgument(1, clickPoint.Y);
                repaint();
            }
        }
        private void state_AddingPoint_MouseUp(Vector2 clickPoint, MouseEventArgs e) { }
        #endregion
        #region state_AddingCircle
        private void state_AddingCircle_MouseDown(Vector2 clickPoint, MouseEventArgs e)
        {
            state_AddingCircle s = (state_AddingCircle)state;
            s.firstpoint = getClickedOn(clickPoint, typeof(Vector2));
            if (s.firstpoint != null)
            {
                s.circle = new DesignerExpression(Function.getFunction("circle"), s.firstpoint, 0);
                play.AddPlayObject(s.circle);
            }
        }
        private void state_AddingCircle_MouseMove(Vector2 clickPoint, MouseEventArgs e)
        {
            if (state.mousedown && ((state_AddingCircle)state).circle != null)
            {
                state_AddingCircle s = (state_AddingCircle)state;

                DesignerExpression point2 = getClickedOn(clickPoint, typeof(Vector2));
                if (point2 != null)
                {
                    s.circle.setArgument(1, new DesignerExpression(Function.getFunction("pointpointdistance"), s.firstpoint, point2));
                }
                else
                {
                    s.circle.setArgument(1, UsefulFunctions.distance((Vector2)s.firstpoint.getValue(tick, null), clickPoint));
                }
                repaint();
            }
        }
        private void state_AddingCircle_MouseUp(Vector2 clickPoint, MouseEventArgs e) { }
        #endregion
        #region state_PlacingIntersection
        private void state_PlacingIntersection_MouseDown(Vector2 clickPoint, MouseEventArgs e)
        {
            DesignerExpression[] clickedLines = new DesignerExpression[2];
            int numlinesclicked = 0;
            foreach (DesignerExpression exp in play.getAllObjects())
            {
                if (exp.ReturnType != typeof(Line))
                    continue;
                Line l = (Line)exp.getValue(tick, null);
                if (willClick(l, clickPoint))
                {
                    if (numlinesclicked < 2)
                        clickedLines[numlinesclicked] = exp;
                    numlinesclicked++;
                }
            }
            DesignerExpression[] clickedCircles = new DesignerExpression[2];
            int numcirclesclicked = 0;
            foreach (DesignerExpression exp in play.getAllObjects())
            {
                if (exp.ReturnType != typeof(Circle))
                    continue;
                Circle c = (Circle)exp.getValue(tick, null);
                if (willClick(c, clickPoint))
                {
                    if (numcirclesclicked < 2)
                        clickedCircles[numcirclesclicked] = exp;
                    numcirclesclicked++;
                }
            }

            if (numcirclesclicked + numlinesclicked != 2)
            {
                showDebugLine("You clicked " + numlinesclicked + " lines and " + numcirclesclicked + " circles");
            }
            else
            {
                if (numlinesclicked == 2)
                {
                    play.AddPlayObject(new DesignerExpression(Function.getFunction("linelineintersection"), clickedLines));
                }
                else if (numcirclesclicked == 2)
                {
                    Circle c1 = (Circle)clickedCircles[0].getValue(tick, null);
                    Circle c2 = (Circle)clickedCircles[1].getValue(tick, null);
                    play.AddPlayObject(new DesignerExpression(Function.getFunction("circlecircleintersection"),
                        clickedCircles[0], clickedCircles[1],
                        PlayCircleCircleIntersection.WhichIntersection(c1, c2, clickPoint)));
                }
                else if (numcirclesclicked == 1 && numlinesclicked == 1)
                {
                    Line line = (Line)clickedLines[0].getValue(tick, null);
                    Circle circle = (Circle)clickedCircles[0].getValue(tick, null);

                    play.AddPlayObject(new DesignerExpression(Function.getFunction("linecircleintersection"),
                        clickedLines[0], clickedCircles[0],
                        LineCircleIntersection.WhichIntersection(line, circle, clickPoint)));
                }
                repaint();
            }
        }
        private void state_PlacingIntersection_MouseMove(Vector2 clickPoint, MouseEventArgs e)
        {
            foreach (DesignerExpression exp in play.getAllObjects())
            {
                if (exp.ReturnType != typeof(Line))
                    continue;
                Line l = (Line)exp.getValue(tick, null);
                exp.Highlighted = willClick(l, clickPoint);
            }
            foreach (DesignerExpression exp in play.getAllObjects())
            {
                if (exp.ReturnType != typeof(Circle))
                    continue;
                Circle c = (Circle)exp.getValue(tick, null);
                exp.Highlighted = willClick(c, clickPoint);
            }
            repaint();
        }
        private void state_PlacingIntersection_MouseUp(Vector2 clickPoint, MouseEventArgs e) { }
        #endregion
        #region FormMouseHandlers
        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            Vector2 clickPoint = pixeltofieldPoint((Vector2)e.Location);
            state.mousedown = true;

            this.GetType().InvokeMember(state.GetType().Name + "_MouseDown",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Instance,
                null, this, new object[] { clickPoint, e });
        }
        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            Vector2 clickPoint = pixeltofieldPoint((Vector2)e.Location);

            this.GetType().InvokeMember(state.GetType().Name + "_MouseMove",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Instance,
                null, this, new object[] { clickPoint, e });
        }
        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            Vector2 clickPoint = pixeltofieldPoint((Vector2)e.Location);
            state.mousedown = false;

            this.GetType().InvokeMember(state.GetType().Name + "_MouseUp",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Instance,
                null, this, new object[] { clickPoint, e });
        }
        #endregion
        #endregion

        #region Coordinate Transformations
        /* These next four functions are from converting from screen coordinates to field coordinates.
         * These should only really be used at the very beginning, when you need to know how many pixels 50 cm is
         * (for drawing the right size circles) and at the very end, when you save the play and convert all
         * the coordinates into field coordinates.
         */
        public double pixeltofieldX(double x)
        {
            return (x - 30 - 305) / 100;
        }
        public double pixeltofieldY(double y)
        {
            return -(y - 56 - 210) / 100;
        }
        public Vector2 pixeltofieldPoint(Vector2 p)
        {
            //return new Vector2((p.X - 275) / 100, (p.Y - 26 - 200) / 100);
            return new Vector2(pixeltofieldX(p.X), pixeltofieldY(p.Y));
        }
        public int fieldtopixelX(double x)
        {
            return (int)(x * 100 + 305 + 30);
        }
        public int fieldtopixelY(double y)
        {
            return (int)(-y * 100 + 56 + 210);
        }
        public Vector2 fieldtopixelPoint(Vector2 p)
        {
            //return new Vector2(p.X * 100 + 275, p.Y * 100 + 26 + 200);
            return new Vector2(fieldtopixelX(p.X), fieldtopixelY(p.Y));
        }
        public double pixeltofieldDistance(double d)
        {
            return d / 100;
        }
        public double fieldtopixelDistance(double d)
        {
            return d * 100;
        }
        #endregion

        #region Drawing Commands
        //This is called when the user selects one of the things that he does/does not want to be drawn.
        private void selectdraw_click(object sender, EventArgs e)
        {
            repaint();
            toolStripDropDownButton1.ShowDropDown();
        }
        internal void repaint()
        {
            this.Invalidate();
            showForm.update();
            definitionForm.updateList();
        }

        /* The painting handler.  I chose to custom-paint everything insted of using custom controls,
         * because this gives me more control, and I think it's a slightly cleaner approach.
         */
        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            foreach (DesignerExpression exp in play.getAllObjects())
            {
                if (exp.ReturnType != typeof(DesignerRobot))
                    continue;
                //l.draw(g);
                try
                {
                    ((DesignerRobot)exp.getValue(tick, null)).draw(g, this);
                }
                catch (NoIntersectionException) { }
            }
            foreach (DesignerExpression r in play.Robots)
            {
                try
                {
                    ((DesignerRobot)r.getValue(tick, null)).draw(g, this);
                }
                catch (NoIntersectionException) { }
            }
            /*if (drawActionsButton.Checked)
            {
                foreach (DesignerExpression exp in play.Actions)
                {
                    if (exp.ReturnType != typeof(ActionDefinition))
                        continue;
                    //l.draw(g);
                    if (exp.UsesFunction("robotpointmove"))
                    {
                        new Arrow(((DesignerRobot)exp.getArgument(0).getValue(tick, null)).getPoint(), (Vector2)exp.getArgument(1).getValue(tick, null)
                            , Color.Blue, .02).drawConvertToPixels(g, this);
                        //draw((ActionDefinition)exp.getValue(tick, null), g, exp.Highlighted);
                    }
                }
            }*/
            if (drawDefinitionsButton.Checked)
            {
                foreach (DesignerRobotDefinition rd in play.Definitions)
                {
                    try
                    {
                        if (rd is ClosestDefinition)
                            ((ClosestDefinition)rd).draw(g, this, tick);
                    }
                    catch (NoIntersectionException) { }
                }
            }
            if (drawLinesButton.Checked)
            {
                foreach (DesignerExpression exp in play.getAllObjects())
                {
                    if (exp.ReturnType != typeof(Line))
                        continue;
                    //l.draw(g);
                    try
                    {
                        draw((Line)exp.getValue(tick, null), g, exp.Highlighted);
                    }
                    catch (NoIntersectionException) { }
                }
            }
            if (drawCirclesButton.Checked)
            {
                foreach (DesignerExpression exp in play.getAllObjects())
                {
                    if (exp.ReturnType != typeof(Circle))
                        continue;
                    try
                    {
                        Circle c = (Circle)exp.getValue(tick, null);
                        //c.draw(g);
                        draw(c, g, exp.Highlighted);
                    }
                    catch (NoIntersectionException) { }
                }
            }
            if (play.Ball != null)
            {
                Brush b = new SolidBrush(play.Ball.color);
                Vector2 ballloc = fieldtopixelPoint(play.Ball.getPoint());
                double radius = fieldtopixelDistance(DesignerBall.Radius);
                g.FillEllipse(b, (float)(ballloc.X - radius), (float)(ballloc.Y - radius), 2 * (float)radius, 2 * (float)radius);
                b.Dispose();
            }
            if (drawPointsButton.Checked)
            {
                foreach (DesignerExpression exp in play.getAllObjects())
                {
                    if (exp.ReturnType != typeof(Vector2))
                        continue;
                    try
                    {
                        draw((Vector2)exp.getValue(tick, null), g, exp.Highlighted);
                    }
                    catch (NoIntersectionException) { }
                }
            }
            /*if (drawActionsButton.Checked)
            {
                foreach (Action a in play.Actions)
                {
                    foreach (Arrow arrow in a.getArrows())
                    {
                        arrow.draw(g);
                    }
                }
            }*/
            else
            {
                foreach (Arrow a in arrows)
                {
                    a.draw(g);
                }
            }
            tick++;
        }
        private void draw(object o, Graphics g, bool highlighted)
        {
            if (o is Line)
            {
                Color c = Color.Yellow;
                if (highlighted)
                    c = Color.Blue;

                Line l = (Line)o;
                //Vector2[] points = (Vector2[])(l.getPoints()).Clone();
                Vector2[] points = l.getPoints();
                Vector2[] drawpoints = new Vector2[2];
                drawpoints[0] = fieldtopixelPoint(points[0]);
                drawpoints[1] = fieldtopixelPoint(points[1]);
                Pen myPen = new Pen(c, 2);
                g.DrawLine(myPen, drawpoints[0].ToPointF(), drawpoints[1].ToPointF());
                myPen.Dispose();
            }
            else if (o is Circle)
            {
                Color c = Color.Yellow;
                if (highlighted)
                    c = Color.Blue;
                Pen myPen = new Pen(c);

                double Radius = ((Circle)o).Radius;
                Radius = fieldtopixelDistance(Radius);
                Vector2 p = ((Circle)o).Center;
                p = fieldtopixelPoint(p);

                g.DrawEllipse(myPen, (float)(p.X - Radius), (float)(p.Y - Radius), (float)(2 * Radius), (float)(2 * Radius));
                myPen.Dispose();
            }
            else if (o is Vector2)
            {
                /*try
                {*/
                Color c = Color.Purple;
                if (highlighted)
                    c = Color.Blue;
                Brush myBrush = new SolidBrush(c);

                Vector2 p = (Vector2)o;
                p = fieldtopixelPoint(p);

                double radius = 3;
                g.FillEllipse(myBrush, (float)(p.X - radius), (float)(p.Y - radius), (float)(2 * radius), (float)(2 * radius));
                myBrush.Dispose();
                /*}
                catch (NoIntersectionException)
                {
                    Console.WriteLine("No intersection! cant draw...");
                }*/
            }
        }
        //Procedures for adding/clearing the arrows.  Used when you select an action from the other window
        /*public void addArrow(Arrow a)
        {
            arrows.Add(a);
            repaint();
        }
        public void clearArrows()
        {
            arrows.Clear();
            repaint();
        }*/
        #endregion

        #region Saving and Loading
        //When the save button is clicked...
        private void toolstripSave_Click(object sender, EventArgs e)
        {
            //...pop up a dialog asking where the user wants to save the play...
            saveFileDialog.ShowDialog();
        }

        //...and when the user says ok on that dialog...
        private void saveFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            string saved = play.Save();
            if (saved == null)
                return;

            string fname = saveFileDialog.FileName;

            //...save the play to that location.
            Stream stream = saveFileDialog.OpenFile();
            StreamWriter writer = new StreamWriter(stream);

            try
            {
                writer.WriteLine(saved);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

            writer.Close();
            stream.Close();
            //bStream.Close();
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog();
        }

        private void openFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            Stream stream = openFileDialog.OpenFile();
            StreamReader sr = new StreamReader(stream);
            string s = sr.ReadToEnd();
            PlayLoader<DesignerPlay, DesignerTactic, DesignerExpression> loader =
                new PlayLoader<DesignerPlay, DesignerTactic, DesignerExpression>(new DesignerExpression.Factory());
            play = loader.load(s);
            /*System.Runtime.Serialization.Formatters.Binary.BinaryFormatter f = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            try
            {
                play = (DesignerPlay)f.Deserialize(stream);
            }
            catch (FileLoadException e2)
            {
                //if (e2.FileName.Contains("PublicKeyToken"))
                MessageBox.Show("Error loading this file.  Looks like it was saved with an old version of the assembly.  Message:\n\"" + e2.Message + "\"");
                //else
                //    MessageBox.Show("Error loading this file.  Some sort of IO problem.\nCould not load \"" + e2.FileName + "\"");
            }
            catch (System.Runtime.Serialization.SerializationException e3)
            {
                MessageBox.Show("There was an error parsing the input file.  The message was:\n" + e3.Message);
            }*/
            stream.Close();
            repaint();
        }
        #endregion

        #region Commands for dealing with the edit forms (adding actions/conditions, selecting objects)
        //When the "Add Condition" or "Add Action" actions are clicked
        private void toolstripAddCommand_Click(object sender, EventArgs e)
        {
            clearCheckedButtons();
            if (state is state_SelectingObject && ((state_SelectingObject)state).editForm != null)
            {
                showDebugLine("You can't be editing two commands at once!");
                return;
            }
            ((ToolStripButton)sender).Checked = true;
            if (sender == toolstripAddAction)
            {
                createExpression(typeof(ActionDefinition));
            }
            else if (sender == toolstripAddCondition)
            {
                createExpression(typeof(bool));
            }
        }
        private void createExpression(Type wantedType)
        {
            state = new state_SelectingObject();
            state_SelectingObject s = (state_SelectingObject)state;
            ValueForm.ReturnAValueDelegate returnDelegate;
            if (wantedType == typeof(bool))
                returnDelegate = this.addCondition;
            else if (wantedType == typeof(ActionDefinition))
                returnDelegate = this.addAction;
            else
                returnDelegate = this.addExpressionsIntermediates;
            s.editForm = new ValueForm(wantedType, returnDelegate, this, play.NonDisplayables);
            s.editForm.FormClosed += delegate(object obj, FormClosedEventArgs ee) { editFormClosed(); };
            s.editForm.Show();
        }
        internal void findObject(Type t, ValueForm.ReturnDesignerExpression returnDelegate)
        {
            ((state_SelectingObject)state).t = t;
            ((state_SelectingObject)state).returnDelegate = returnDelegate;
            this.Focus();
        }
        /// <summary>
        /// If you create an expression, and label one of the child expressions, then only the name will appear.
        /// You need to add the definition to the play so it will show up in the Objects: category.
        /// This also adds the argument, exp, if it is named.
        /// </summary>
        /// <param name="exp"></param>
        private bool addExpressionsIntermediates(DesignerExpression exp)
        {
            if (exp.Name == null)
            {
                MessageBox.Show("Please enter a name");
                return false;
            }
            else if (play.PlayObjects.ContainsKey(exp.Name))
            {
                MessageBox.Show("Sorry, that name is already in use");
                return false;
            }
            if (exp.Name != null && !play.PlayObjects.ContainsValue(exp))
                play.PlayObjects.Add(exp.Name, exp);
            if (exp.IsFunction)
            {
                for (int i = 0; i < exp.theFunction.NumArguments; i++)
                {
                    internalAddExpressionsIntermediates(exp.getArgument(i));
                }
            }
            return true;
        }
        private void internalAddExpressionsIntermediates(DesignerExpression exp)
        {
            if (exp.Name != null && !play.PlayObjects.ContainsValue(exp))
                play.PlayObjects.Add(exp.Name, exp);
            if (exp.IsFunction)
            {
                for (int i = 0; i < exp.theFunction.NumArguments; i++)
                {
                    internalAddExpressionsIntermediates(exp.getArgument(i));
                }
            }
        }
        /// <summary>
        ///This is a function so that other objects can add Conditions to the play.
        /// </summary>
        /// <param name="o"></param>
        public bool addCondition(DesignerExpression exp)
        {
            play.Conditions.Add(exp);
            internalAddExpressionsIntermediates(exp);
#if DEBUG
            if (!(state is state_SelectingObject))
                throw new ApplicationException("The state was somehow changed from state_SelectingObject !");
            if (((state_SelectingObject)state).editForm == null)
                throw new ApplicationException("The selection form reference has been set to null");
#endif
            showForm.update();   //updates the show form with any new conditions/actions
            return true;
        }
        //This happens when the form to edit conditions/actions is closed, whether the "done" button was clicked,
        //or the user clicked the x in the top-right corner.  It cleans it up and nulls out the reference.
        public void editFormClosed()
        {
            ((state_SelectingObject)state).editForm.Dispose();
            toolstripAddCondition.Checked = false;
            toolstripAddAction.Checked = false;
            ((state_SelectingObject)state).editForm = null;
            if (((state_SelectingObject)state).previousState != null)
                state = ((state_SelectingObject)state).previousState;
            repaint();
            this.Focus();
        }
        //public void addAction(Action a)
        public bool addAction(DesignerExpression exp)
        {
            play.Actions.Add(exp);
            internalAddExpressionsIntermediates(exp);
            if (exp.Name != null)
                play.PlayObjects.Add(exp.Name, exp);
#if DEBUG
            if (!(state is state_SelectingObject))
                throw new ApplicationException("The state was somehow changed from state_SelectingObject !");
            if (((state_SelectingObject)state).editForm == null)
                throw new ApplicationException("The selection form reference has been set to null");
#endif
            ((state_SelectingObject)state).editForm.Close();
            showForm.update();   //updates the show form with any new conditions/actions

            //editFormClosed();
            return true;
        }
        /// <summary>
        /// This is called when the ShowCommandForm tells the main form that the user wants to edit this command.
        /// </summary>
        public void editExpression(DesignerExpression exp)
        {
            if (state is state_SelectingObject && ((state_SelectingObject)state).editForm != null)
            {
                showDebugLine("You can't be editing two commands at once!");
                return;
            }
            State prevstate = state;
            //((state_SelectingObject)state).editForm = new EditCommandForm((state_SelectingObject)state, this, f);
            state = new state_SelectingObject();
            ((state_SelectingObject)state).previousState = prevstate;
            ((state_SelectingObject)state).editForm = new ValueForm(exp.ReturnType, delegate(DesignerExpression expr)
            {
                //we have to replace the previous version thats still in the play:
                int index;
                index = play.Conditions.IndexOf(exp);
                if (index != -1)
                {
                    play.Conditions.RemoveAt(index);
                    play.Conditions.Insert(index, expr);
                }
                index = play.Actions.IndexOf(exp);
                if (index != -1)
                {
                    play.Actions.RemoveAt(index);
                    play.Actions.Insert(index, expr);
                }
                showForm.update();
                return true;
            }, this, exp, play.NonDisplayables);
            ((state_SelectingObject)state).editForm.FormClosed += delegate(object obj, FormClosedEventArgs ee)
            {
                editFormClosed();
            };
            ((state_SelectingObject)state).editForm.Show();
        }
        #endregion

        private void toolstripPlayType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //This is a workaround for the fact that when you change the selection in a combo box on a toolbar,
            //it's then the selected part of the toolbar.  When you mouse over other buttons, nothing really happens
            //because that part of the toolbar doesn't have the focus.
            toolstripPlayType.Visible = false;
            toolstripPlayType.Visible = true;
            play.PlayType = (PlayType)Enum.Parse(typeof(PlayType), ((ToolStripComboBox)sender).Text, true);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            showForm.Close();
            definitionForm.Close();
        }

        private bool returningPlay = false;
        /// <summary>
        /// Whether or not the play should be looked at when the form closes.
        /// For instance, if the user messes up and wants to just exit out, this will be false and the
        /// play should be discarded.
        /// </summary>
        public bool ReturningPlay
        {
            get { return returningPlay; }
        }

        private void toolstripExitReturn_Click(object sender, EventArgs e)
        {
            returningPlay = true;
            Close();
        }

    }
}