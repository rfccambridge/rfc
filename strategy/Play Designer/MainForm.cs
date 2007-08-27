using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using Robocup.Infrastructure;

namespace RobocupPlays
{
    /// <summary>
    /// The main form for the Play Designer
    /// </summary>
    partial class MainForm : Form
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
        /// <summary>
        /// Takes a list of clickable objects and a point, and returns the first one such that the point is on that object
        /// (as defined by that objects willClick() method)
        /// </summary>
        /// <param name="al">The ArrayList holding the objects</param>
        /// <param name="p">The point p that was clicked</param>
        /// <returns>Returns the object that was clicked on, or null if none of them were.</returns>
        private DesignerExpression getClickedOn(IList<DesignerExpression> list, Vector2 p)
        {
            foreach (DesignerExpression exp in list)
            {
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
                return UsefulFunctions.distance((Vector2)o, p) <= PixelDistanceToFieldDistance(6);
            }
            else if (o is PlayRobot)
            {
                return UsefulFunctions.distance(((PlayRobot)o).getPoint(), p) <= PixelDistanceToFieldDistance(10);
            }
            else if (o is Line)
            {
                //change these two lines to make it so that you have to click on the segment itself, and not on its extension
                //return ((Line)o).distFromSegment(p) <= PixelDistanceToFieldDistance(3);
                return ((Line)o).distFromLine(p) <= PixelDistanceToFieldDistance(3);
            }
            else if (o is Circle)
                return Math.Abs(((Circle)o).distanceFromCenter(p) - ((Circle)o).Radius) <= PixelDistanceToFieldDistance(3);
            throw new ApplicationException("Tried to see if you could click on something that's not supported yet");
            //return false;
        }
        #endregion


        public MainForm()
        {
            InitializeComponent();

            intermediates = play.Intermediates;

            //I don't know why, but if you set a combo box to be uneditable (the user can't add
            //their own values), then you can't specify at compile time what the initial selection is.
            //toolstripPlayType.SelectedIndex = 0;
            toolstripPlayType.Items.Clear();
            toolstripPlayType.Items.AddRange(Enum.GetNames(typeof(PlayTypes)));
            toolstripPlayType.SelectedIndex = 0;

            showForm = new ShowExpressionsForm(play.Conditions, play.Actions, this);
            showForm.Show();
            definitionForm = new DefinitionForm(play, this);
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

            float minx = PixelXToFieldX(30), maxx = PixelXToFieldX(520);
            float miny = PixelYToFieldY(56), maxy = PixelYToFieldY(396);
            float goalback = PixelDistanceToFieldDistance(8);// 12;
            /*play.Points.Add(new DesignerPoint_const(new Vector2(minx, miny)));
            play.Points.Add(new DesignerPoint_const(new Vector2(minx, maxy)));
            play.Points.Add(new DesignerPoint_const(new Vector2(maxx, maxy)));
            play.Points.Add(new DesignerPoint_const(new Vector2(maxx, miny)));
            play.Points.Add(new DesignerPoint_const(new Vector2(minx - goalback, (miny + maxy) / 2)));
            play.Points.Add(new DesignerPoint_const(new Vector2(maxx + goalback, (miny + maxy) / 2)));*/
            play.AddPlayObject(new DesignerExpression(Function.getFunction("point"), minx, miny), "topLeftCorner");
            play.AddPlayObject(new DesignerExpression(Function.getFunction("point"), minx, maxy), "bottomLeftCorner");
            play.AddPlayObject(new DesignerExpression(Function.getFunction("point"), maxx, maxy), "bottomRightCorner");
            play.AddPlayObject(new DesignerExpression(Function.getFunction("point"), maxx, miny), "topRightCorner");
            play.AddPlayObject(new DesignerExpression(Function.getFunction("point"), minx - goalback, (miny + maxy) / 2), "ourgoal");
            play.AddPlayObject(new DesignerExpression(Function.getFunction("point"), maxx + goalback, (miny + maxy) / 2), "theirgoal");

            //toolStripComboBox1.Text = "asdf";

            Version v = System.Reflection.Assembly.GetAssembly(this.GetType()).GetName(false).Version;
            showDebugLine("This is Play Designer version " + v.Major + "." + v.Minor + ", build " + v.Build);

            repaint();
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
                //if (((state_SelectingObject)state).editForm.CanFocus)
                //{
                showDebugLine("You can't switch out of selecting an object!");
                return;
                /*}
                else
                {
                    editFormClosed();
                }*/
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
        #region MouseDown
        //The great big handler for when the mouse is pressed anywhere on the field.
        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            Vector2 clickPoint = PixelPointToFieldPoint((Vector2)e.Location);
            state.mousedown = true;

            if (state is state_AddingRobot)
            {
                state_AddingRobot s = (state_AddingRobot)state;
                bool ours = true;
                if (e.Button == MouseButtons.Right)
                    ours = false;
                //if (play.Robots.Count < 5)
                //{
                /*DesignerRobot r = new DesignerRobot(clickPoint, ours);
                play.Robots.Add(r);
                play.Points.Add(new DesignerPoint(r));
                repaint();*/
                //play.Robots.Add(new DesignerExpression(Function.getFunction("robot"), 1));
                DesignerRobot r = new DesignerRobot(clickPoint, ours);
                DesignerExpression exp = new DesignerExpression(r);
                play.AddPlayObject(exp);
                play.AddPlayObject(new DesignerExpression(Function.getFunction("pointof"), exp));
                //}
                repaint();
            }
            else if (state is state_EditingObject)
            {
                DesignerExpression exp = getClickedOn(play.getClickables(), clickPoint);
                if (e.Button == MouseButtons.Left)
                {
                    if (exp != null && !(typeof(PlayRobot).IsAssignableFrom(exp.ReturnType)))
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
            else if (state is state_MovingObjects)
            {
                state_MovingObjects s = (state_MovingObjects)state;
                //s.robot = (DesignerRobot)getClickedOn(play.Robots, clickPoint);
                s.robot = getClickedOn(play.Robots, clickPoint);
                //DesignerPoint p = (DesignerPoint)getClickedOn(play.Points, clickPoint);
                //Vector2 p = (Vector2)getClickedOn(play.Points, clickPoint).getValue(tick);
                DesignerExpression point = getClickedOn(play.Points, clickPoint);
                if (point == null || point.theFunction.Name != "point")
                    point = null;
                /*if (!(p is DesignerPoint_const))
                    p = null;*/
                //s.point = (DesignerPoint_const)p;
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
                if (numclickedon > 1)
                {
                    //showDebugLine("You clicked on both a robot and a point, which is not allowed (yet).");
                    showDebugLine("You clicked on " + numclickedon + " movable objects.");
                    s.robot = null;
                    s.point = null;
                    s.ball = null;
                }
                s.setMouse(clickPoint);
            }
            else if (state is state_DrawingLine)
            {
                state_DrawingLine s = (state_DrawingLine)state;
                //s.firstpoint = (DesignerPoint)getClickedOn(play.Points, clickPoint);
                //s.firstpoint = (DesignerPoint)getClickedOn(play.Points, clickPoint).getValue(tick);
                s.firstpoint = getClickedOn(play.Points, clickPoint);
                if (s.firstpoint != null)
                {
                    s.line = new DesignerExpression(Function.getFunction("line"), s.firstpoint, s.firstpoint);
                    play.AddPlayObject(s.line);
                }
            }
            else if (state is state_AddingClosestCondition)
            {
                state_AddingClosestCondition s = (state_AddingClosestCondition)state;
                //s.firstpoint = (DesignerPoint)getClickedOn(play.Points, clickPoint);
                //s.firstrobot = (DesignerRobot)getClickedOn(play.Robots, clickPoint);
                //s.firstpoint = (DesignerPoint)getClickedOn(play.Points, clickPoint).getValue(tick);
                //s.firstrobot = (DesignerRobot)getClickedOn(play.Robots, clickPoint).getValue(tick);
                s.firstpoint = getClickedOn(play.Points, clickPoint);
                s.firstrobot = getClickedOn(play.Robots, clickPoint);
                /*if (s.firstpoint != null)// && !s.firstpoint.isDefined())
                {
                    s.firstpoint = null;
                    if (s.firstrobot == null)
                        showDebugLine("You can't define a robot by attaching it to an undefined point!");
                }
                if (s.firstrobot != null)// && s.firstrobot.isDefined())
                {
                    s.firstrobot = null;
                    showDebugLine("You can't multiply-define something!");
                }*/
                if (s.firstpoint != null && s.firstrobot != null)
                {
                    s.firstpoint = null;
                    s.firstrobot = null;
                    showDebugLine("You clicked on both a point and a robot, which is not allowed.");
                }
            }
            else if (state is state_AddingBall)
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
            else if (state is state_SelectingObject)
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
                    //s.editForm.setObject(null);
                }
                DesignerExpression exp = null;
                if (s.t.IsAssignableFrom(typeof(Vector2)))
                {
                    exp = getClickedOn(play.Points, clickPoint);
                }
                else if (s.t.IsAssignableFrom(typeof(DesignerRobot)))
                {
                    exp = getClickedOn(play.Robots, clickPoint);
                }
                else if (s.t.IsAssignableFrom(typeof(Circle)))
                {
                    exp = getClickedOn(play.Circles, clickPoint);
                }
                else if (s.t.IsAssignableFrom(typeof(Line)))
                {
                    exp = getClickedOn(play.Lines, clickPoint);
                }
                else
                    throw new ApplicationException("Unable to find something of type " + s.t.Name);
                //DesignerExpression exp = getClickedOn(play.getClickables(), clickPoint);
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
                //repaint();
                //}
            }
            else if (state is state_AddingPoint)
            {
                state_AddingPoint s = (state_AddingPoint)state;
                //s.point = new DesignerPoint_const(clickPoint);
                //play.Points.Add(s.point);
                //play.AddPlayObject(new DesignerExpression(s.point));
                s.point = new DesignerExpression(Function.getFunction("point"), (float)clickPoint.X, (float)clickPoint.Y);
                play.AddPlayObject(s.point);
                //s.setMouse(clickPoint);
                repaint();
            }
            else if (state is state_AddingCircle)
            {
                state_AddingCircle s = (state_AddingCircle)state;
                //DesignerPoint dp = (DesignerPoint)getClickedOn(play.Points, clickPoint);
                //DesignerExpression firstpoint =
                s.firstpoint = getClickedOn(play.Points, clickPoint);
                if (s.firstpoint != null)
                {
                    s.circle = new DesignerExpression(Function.getFunction("circle"), s.firstpoint, 0);
                    play.AddPlayObject(s.circle);
                }
                /*if (exp != null)
                {
                    s.circle = new DesignerCircle(dp);
                    //play.Circles.Add(s.circle);
                    play.Circles.Add(new DesignerExpression(s.circle));
                }*/
            }
            else if (state is state_PlacingIntersection)
            {
                DesignerExpression[] clickedLines = new DesignerExpression[2];
                int numlinesclicked = 0;
                foreach (DesignerExpression exp in play.Lines)
                {
                    Line l = (Line)exp.getValue(tick, null);
                    //if (l.willClick(clickPoint))
                    if (willClick(l, clickPoint))
                    {
                        if (numlinesclicked < 2)
                            clickedLines[numlinesclicked] = exp;
                        numlinesclicked++;
                    }
                }
                DesignerExpression[] clickedCircles = new DesignerExpression[2];
                int numcirclesclicked = 0;
                foreach (DesignerExpression exp in play.Circles)
                {
                    Circle c = (Circle)exp.getValue(tick, null);
                    //if (c.willClick(clickPoint))
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
                        //play.Points.Add(new DesignerPoint(new DesignerLineLineIntersection(clickedLines[0], clickedLines[1])));
                        play.AddPlayObject(new DesignerExpression(Function.getFunction("linelineintersection"), clickedLines));
                    }
                    else if (numcirclesclicked == 2)
                    {
                        //play.Points.Add(new DesignerPoint(new DesignerCircleCircleIntersection(clickedCircles[0], clickedCircles[1], clickPoint)));
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
        }
        #endregion
        #region MouseMove
        //The handler for when the mouse is moved
        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            Vector2 clickPoint = PixelPointToFieldPoint((Vector2)e.Location);
            if (state is state_MovingObjects)
            {
                state_MovingObjects s = (state_MovingObjects)state;
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
                    s.point.setArgument(0, (float)clickPoint.X);
                    s.point.setArgument(1, (float)clickPoint.Y);
                    needtorepaint = true;
                }
                else if (s.ball != null)
                {
                    s.ball.translate(s.diff(clickPoint));
                    needtorepaint = true;
                }
                s.setMouse(clickPoint);
                if (needtorepaint)
                    repaint();
            }
            else if (state is state_PlacingIntersection)
            {
                foreach (DesignerExpression exp in play.Lines)
                {
                    Line l = (Line)exp.getValue(tick, null);
                    exp.Highlighted = willClick(l, clickPoint);
                }
                foreach (DesignerExpression exp in play.Circles)
                {
                    Circle c = (Circle)exp.getValue(tick, null);
                    exp.Highlighted = willClick(c, clickPoint);
                }
                repaint();
            }
            else if (state is state_SelectingObject)
            {
                state_SelectingObject s = (state_SelectingObject)state;
                if (s.t != null)
                {
                    List<DesignerExpression> clickables = play.getClickables();
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
                            //Console.WriteLine(exp.Highlighted);
                        }
                    }
                    repaint();
                }
            }
            else if (state is state_AddingPoint)
            {
                state_AddingPoint s = (state_AddingPoint)state;
                if (s.mousedown)
                {
                    //s.point.translate(s.diff(clickPoint));
                    s.point.setArgument(0, clickPoint.X);
                    s.point.setArgument(1, clickPoint.Y);
                    //s.setMouse(clickPoint);
                    repaint();
                }
            }
            else if (state is state_AddingCircle)
            {
                if (state.mousedown && ((state_AddingCircle)state).circle != null)
                {
                    state_AddingCircle s = (state_AddingCircle)state;
                    //DesignerPoint dp = (DesignerPoint)getClickedOn(play.Points, clickPoint);
                    //DesignerPoint dp = (DesignerPoint)getClickedOn(play.Points, clickPoint).getValue(tick);

                    /*if (dp == null)
                        s.circle.setRadius(clickPoint);
                    else
                        s.circle.setRadius(dp);*/
                    //s.circle.set
                    DesignerExpression point2 = getClickedOn(play.Points, clickPoint);
                    if (point2 != null)
                    {
                        s.circle.setArgument(1, new DesignerExpression(Function.getFunction("pointpointdistance"), s.firstpoint, point2));
                    }
                    else
                    {
                        //point2 = new DesignerExpression(new Vector2(clickPoint));
                        s.circle.setArgument(1, UsefulFunctions.distance((Vector2)s.firstpoint.getValue(tick, null), clickPoint));
                    }
                    //s.circle.setArgument(1, new DesignerExpression(Function.getFunction("pointpointdistance"),s.firstpoint,clickPoint));
                    repaint();
                }
            }
            else if (state is state_DrawingLine)
            {
                if (state.mousedown && ((state_DrawingLine)state).line != null)
                {
                    state_DrawingLine s = (state_DrawingLine)state;
                    DesignerExpression point2 = getClickedOn(play.Points, clickPoint);
                    if (point2 != null)
                    {
                        s.line.setArgument(1, point2);
                    }
                    else
                        s.line.setArgument(1, clickPoint);
                    //s.line.setArgument(1, new DesignerExpression(Function.getFunction("pointpointdistance"), s.firstpoint, point2));
                    //s.circle.setArgument(1, new DesignerExpression(Function.getFunction("pointpointdistance"),s.firstpoint,clickPoint));
                    repaint();
                }
            }
        }
        #endregion
        #region MouseUp
        //And the handler for when the mouse is released.
        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            Vector2 clickPoint = PixelPointToFieldPoint((Vector2)e.Location);
            state.mousedown = false;
            if (state is state_MovingObjects)
            {
                state = new state_MovingObjects();//nulls them all out
            }
            /*else if (state is state_DrawingLine)
            {
                state_DrawingLine s = (state_DrawingLine)state;
                //DesignerPoint secondpoint = (DesignerPoint)getClickedOn(play.Points, clickPoint);
                DesignerExpression secondpoint = getClickedOn(play.Points, clickPoint);
                if (s.firstpoint != null && secondpoint != null)
                {
                    //play.Lines.Add(new DesignerLine(s.firstpoint, secondpoint));
                    play.Lines.Add(new DesignerExpression(Function.getFunction("line"), s.firstpoint, secondpoint));
                    repaint();
                }
            }*/
            else if (state is state_AddingClosestCondition)
            {
                state_AddingClosestCondition s = (state_AddingClosestCondition)state;
                //DesignerPoint secondpoint = (DesignerPoint)getClickedOn(play.Points, clickPoint);
                //DesignerRobot secondrobot = (DesignerRobot)getClickedOn(play.Robots, clickPoint);
                DesignerExpression secondpoint = getClickedOn(play.Points, clickPoint);
                DesignerExpression secondrobot = getClickedOn(play.Robots, clickPoint);
                if (s.firstrobot != null && secondpoint != null)
                {
                    /*if (!secondpoint.isDefined())
                        showDebugLine("You can't define a robot by a point that's not defined!");
                    else
                    {*/
                    DesignerRobotDefinition df = new ClosestDefinition(s.firstrobot, secondpoint);
                    play.Definitions.Add(df);
                    ((DesignerRobot)s.firstrobot.getValue(tick, null)).setDefinition(df);
                    definitionForm.updateList();
                    //}
                }
                else if (s.firstpoint != null && secondrobot != null)
                {
                    /*if (secondrobot.isDefined())
                        showDebugLine("You can't multiply-define something!");
                    else
                    {*/
                    DesignerRobotDefinition df = new ClosestDefinition(secondrobot, s.firstpoint);
                    play.Definitions.Add(df);
                    ((DesignerRobot)secondrobot.getValue(tick, null)).setDefinition(df);
                    definitionForm.updateList();
                    //}
                }
                repaint();

            }
        }
        #endregion
        #endregion

        #region Coordinate Transformations
        /* These next four functions are from converting from screen coordinates to field coordinates.
         * These should only really be used at the very beginning, when you need to know how many pixels 50 cm is
         * (for drawing the right size circles) and at the very end, when you save the play and convert all
         * the coordinates into field coordinates.
         */
        static public float PixelXToFieldX(float x)
        {
            return (x - 275f) / 100f;
        }
        static public float PixelYToFieldY(float y)
        {
            return -(y - 26f - 200f) / 100f;
        }
        static public Vector2 PixelPointToFieldPoint(Vector2 p)
        {
            //return new Vector2((p.X - 275f) / 100f, (p.Y - 26f - 200f) / 100f);
            return new Vector2(PixelXToFieldX(p.X), PixelYToFieldY(p.Y));
        }
        static public float FieldXToPixelX(float x)
        {
            return x * 100f + 275f;
        }
        static public float FieldYToPixelY(float y)
        {
            return -y * 100f + 26f + 200f;
        }
        static public Vector2 FieldPointToPixelPoint(Vector2 p)
        {
            //return new Vector2(p.X * 100f + 275f, p.Y * 100f + 26f + 200f);
            return new Vector2(FieldXToPixelX(p.X), FieldYToPixelY(p.Y));
        }
        static public float PixelDistanceToFieldDistance(float d)
        {
            return d / 100;
        }
        static public float FieldDistanceToPixelDistance(float d)
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
        }

        /* The painting handler.  I chose to custom-paint everything insted of using custom controls,
         * because this gives me more control, and I think it's a slightly cleaner approach.
         */
        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            foreach (DesignerExpression r in play.Robots)
            {
                try
                {
                    ((DesignerRobot)r.getValue(tick, null)).draw(g);
                }
                catch (NoIntersectionException) { }
            }
            if (drawDefinitionsButton.Checked)
            {
                foreach (DesignerRobotDefinition rd in play.Definitions)
                {
                    try
                    {
                        if (rd is ClosestDefinition)
                            ((ClosestDefinition)rd).draw(g, tick);
                    }
                    catch (NoIntersectionException) { }
                }
            }
            if (drawLinesButton.Checked)
            {
                foreach (DesignerExpression exp in play.Lines)
                {
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
                foreach (DesignerExpression exp in play.Circles)
                {
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
                Vector2 ballloc = FieldPointToPixelPoint(play.Ball.getPoint());
                float radius = FieldDistanceToPixelDistance(DesignerBall.Radius);
                g.FillEllipse(b, ballloc.X - radius, ballloc.Y - radius, 2 * radius, 2 * radius);
                b.Dispose();
            }
            if (drawPointsButton.Checked)
            {
                foreach (DesignerExpression exp in play.Points)
                {
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
                drawpoints[0] = FieldPointToPixelPoint(points[0]);
                drawpoints[1] = FieldPointToPixelPoint(points[1]);
                Pen myPen = new Pen(c, 2);
                g.DrawLine(myPen, drawpoints[0], drawpoints[1]);
                myPen.Dispose();
            }
            else if (o is Circle)
            {
                Color c = Color.Yellow;
                if (highlighted)
                    c = Color.Blue;
                Pen myPen = new Pen(c);

                float Radius = ((Circle)o).Radius;
                Radius = FieldDistanceToPixelDistance(Radius);
                Vector2 p = ((Circle)o).getCenter();
                p = FieldPointToPixelPoint(p);

                g.DrawEllipse(myPen, p.X - Radius, p.Y - Radius, 2 * Radius, 2 * Radius);
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
                p = FieldPointToPixelPoint(p);

                float radius = 3;
                g.FillEllipse(myBrush, p.X - radius, p.Y - radius, 2 * radius, 2 * radius);
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
            string fname = saveFileDialog.FileName;
            string extension = fname.Substring(fname.LastIndexOf('.') + 1);

            //...save the play to that location.
            Stream stream = saveFileDialog.OpenFile();
            StreamWriter writer = new StreamWriter(stream);

            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter f = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            //Stream bStream = new FileStream(fname.Substring(0, fname.LastIndexOf('.')) + ".play", FileMode.Create, FileAccess.Write, FileShare.None);

            try
            {
                if (extension == "txt")
                    writer.WriteLine(play.Save());
                else if (extension == "play")
                    f.Serialize(stream, play);
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
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter f = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

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
            }
            stream.Close();
            this.Invalidate();
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
            s.editForm = new ValueForm(wantedType, returnDelegate, this, intermediates);
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
        private void addExpressionsIntermediates(DesignerExpression exp)
        {
            if (exp.Name != null)
                play.addIntermediate(exp);
            if (exp.IsFunction)
            {
                for (int i = 0; i < exp.theFunction.NumArguments; i++)
                {
                    addExpressionsIntermediates(exp.getArgument(i));
                }
            }
        }
        /// <summary>
        ///This is a function so that other objects can add Conditions to the play.
        /// </summary>
        /// <param name="o"></param>
        public void addCondition(DesignerExpression exp)
        {
            play.Conditions.Add(exp);
            addExpressionsIntermediates(exp);
#if DEBUG
            if (!(state is state_SelectingObject))
                throw new ApplicationException("The state was somehow changed from state_SelectingObject !");
            if (((state_SelectingObject)state).editForm == null)
                throw new ApplicationException("The selection form reference has been set to null");
#endif
            showForm.update();   //updates the show form with any new conditions/actions
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
        public void addAction(DesignerExpression exp)
        {
            play.Actions.Add(exp);
            if (exp.Name != null)
                play.addIntermediate(exp);
#if DEBUG
            if (!(state is state_SelectingObject))
                throw new ApplicationException("The state was somehow changed from state_SelectingObject !");
            if (((state_SelectingObject)state).editForm == null)
                throw new ApplicationException("The selection form reference has been set to null");
#endif
            ((state_SelectingObject)state).editForm.Close();
            showForm.update();   //updates the show form with any new conditions/actions

            //editFormClosed();
        }
        private IList<DesignerExpression> intermediates;
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
            }, this, exp, intermediates);
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
            play.PlayType = (PlayTypes)Enum.Parse(typeof(PlayTypes), ((ToolStripComboBox)sender).Text);
        }

    }
}