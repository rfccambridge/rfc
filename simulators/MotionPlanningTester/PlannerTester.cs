using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Robocup.Core;
using Robocup.Utilities;
using Robocup.Simulation;
using Robocup.CoreRobotics;

namespace Robocup.MotionControl
{
    public partial class PlannerTester : Form
    {
        IMotionPlanner planner;
        readonly FieldDrawer drawer;
        readonly PhysicsEngine engine;
        Vector2 destination = new Vector2(2, 0);
        readonly ICoordinateConverter converter;

        System.Threading.Timer t;

        public PlannerTester()
        {
            engine = new PhysicsEngine(new SimpleReferee());
            converter = new BasicCoordinateConverter(600, 30, 50);
            
            // TODO: Bring drawing back to PlannerTester!
            //drawer = new FieldDrawer(engine, converter);
            drawer = new FieldDrawer();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            InitializeComponent();

            InitDragAndDrop();

            Type[] navigatorTypes = PlannerFactory.NavigatorTypes;
            foreach (Type t in navigatorTypes)
            {
                plannerChooseBox.Items.Add(t.Name);
            }
            plannerChooseBox.SelectedIndex = 0;
        }

        //would be nice to find a work-around without having to do this
        void restoreFocus()
        {
            foreach (Control c in this.Controls)
            {
                c.Enabled = false;
            }
            this.Focus();
            foreach (Control c in this.Controls)
            {
                c.Enabled = true;
            }
        }

        void step(int num)
        {
            for (int i = 0; i < num; i++)
            {
                RobotPath path = planner.PlanMotion(Team.Yellow, 0, new RobotInfo(destination, 0, 0), engine, .13);
            	MotionPlanningResults results = planner.FollowPath(path, engine);

                engine.setMotorSpeeds(0, results.wheel_speeds);
                if (!checkBoxDisableMovement.Checked)
                    engine.Step(.025);
                //moveObstacles();
            }
        }
        volatile int numrunning = 0;
        void show(object state)
        {
            numrunning++;
            if (numrunning > 1)
            {
                numrunning--;
                return;
            }
            step(1);
            this.Invalidate();
            numrunning--;
        }
        private object graphicsLock = new object();
        private void RRTTester_Paint(object sender, PaintEventArgs e)
        {
            // TODO: Bring drawing back to PlannerTester
            /*lock (graphicsLock)
            {
                Graphics g = e.Graphics;
                drawer.paintField(g);
                if (debugDraw() && planner != null)
                {
                    lock (planner)
                    {
                        //try
                        //{
                            planner.DrawLast(g, converter);
                        //}
                        //sometimes it throws this, i think because of some synchronization issue with the Graphics object
                        //catch (AccessViolationException) { }
                    }
                }
                Brush b = new SolidBrush(Color.Green);
                Vector2 dest = converter.fieldtopixelPoint(destination);
                g.FillEllipse(b, (float)(dest.X - 5), (float)(dest.Y - 5), 10, 10);
                b.Dispose();
            }*/
        }
        private bool debugDraw()
        {
            return checkBoxDebugDrawing.Checked;
        }

        #region User Input

        private DragAndDropper draganddrop = new DragAndDropper();
        void InitDragAndDrop()
        {
            draganddrop.AddDragandDrop(delegate() { return destination; }, .05, delegate(Vector2 v) { destination = v; });
            draganddrop.AddDragandDrop(delegate() { return engine.GetBall().Position; }, .05, delegate(Vector2 v) { engine.MoveBall(v); });
            foreach (RobotInfo info in engine.GetRobots())
            {
                int id = info.ID;
            	Team team = info.Team;
                draganddrop.AddDragandDrop(delegate() { return engine.GetRobot(team, id).Position; }, .1,
                    delegate(Vector2 v)
                    {
                        RobotInfo inf = engine.GetRobot(team, id);
                        engine.MoveRobot(team, id, new RobotInfo(v, inf.Orientation, id));
                    }
                );
            }
        }
        private void RRTTester_MouseDown(object sender, MouseEventArgs e)
        {
            draganddrop.MouseDown(converter.pixeltofieldPoint((Vector2)e.Location));
        }
        private void RRTTester_MouseMove(object sender, MouseEventArgs e)
        {
            bool moved = draganddrop.MouseMove(converter.pixeltofieldPoint((Vector2)e.Location));
            if (moved) this.Invalidate();
        }
        private void RRTTester_MouseUp(object sender, MouseEventArgs e)
        {
            draganddrop.MouseUp();
        }

        private void navigatorChooseBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int i = plannerChooseBox.SelectedIndex;
            planner = PlannerFactory.createPlanner(PlannerFactory.NavigatorTypes[i]);

            propertyGrid1.SelectedObject = planner;
            //navigatorChooseBox.Visible = false;

            restoreFocus();
        }

        private void RRTTester_Activated(object sender, EventArgs e)
        {

            restoreFocus();
        }


        private void checkBoxDebugDrawing_CheckedChanged(object sender, EventArgs e)
        {
            restoreFocus();
        }
        private void RRTTester_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            if (c == 't')
            {
                /*TestResults t = test();
                MessageBox.Show(t.compileSingleResult(state.ReferenceResults));
                this.restoreFocus();*/
                HighResTimer timer = new HighResTimer();
                timer.Start();
                step(100);
                timer.Stop();
                MessageBox.Show((timer.Duration*10).ToString());
                restoreFocus();
                this.Invalidate();
            }
            else if (c == 'r')
            {
                if (t == null)
                {
                    t = new System.Threading.Timer(new System.Threading.TimerCallback(show), null, 0, 10);
                }
                else
                {
                    t.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                    t = null;
                }
            }
            /*else if (c == 'c')
            {
                if (savedState != null)
                {
                    state = savedState.Clone();
                    this.Invalidate();
                }
            }*/
            else if (c == ' ')
            {
                step(1);
                this.Invalidate();
            }
        }
        #endregion

        private void RRTTester_MouseClick(object sender, MouseEventArgs e)
        {
            restoreFocus();
        }
    }
}