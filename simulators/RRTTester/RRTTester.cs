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

namespace Robocup.RRT
{
    public partial class RRTTester : Form
    {
        IMotionPlanner planner;
        PhysicsEngine engine = new PhysicsEngine(new SimpleReferee());
        Vector2 destination = new Vector2(2, 0);

        public RRTTester()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            InitializeComponent();

            Type[] navigatorTypes = RRTFactory.NavigatorTypes;
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


        #region Navigation Simulation
        bool berunning = false;
        System.Threading.Timer t = null;
        const double distThresh = .02;
        const double moveSpeed = .015;
        const double momentum = .975; //the percentage of the new velocity that comes from the old velocity
        const double friction = .015; //the amount lost to friction
        /*private TestResults test()
        {
            int numRobots = state.Destinations.Length;

            int totalruns = 0;
            DateTime start = DateTime.Now;
            TimeSpan time;
            int totalCalls = 0;
            double minDistanceSq = 100000f;
            double seconds = double.Parse(textBoxTestLength.Text);
            do
            {
                initialize();
                totalruns += numRobots;
                double dist = 0;
                for (int i = 0; i < numRobots; i++)
                {
                    dist = Math.Max(dist, state.OurPositions[i].distanceSq(state.Destinations[i]));
                }
                while (dist > distThresh * distThresh)
                {
                    totalCalls += numRobots;


                    step(1);


                    dist = 0;
                    for (int i = 0; i < numRobots; i++)
                    {
                        dist = Math.Max(dist, state.OurPositions[i].distanceSq(state.Destinations[i]));
                    }
                    for (int i = 0; i < numRobots; i++)
                    {
                        Vector2 curposition = state.OurPositions[0];
                        foreach (Vector2 p in state.TheirPositions)
                        {
                            minDistanceSq = Math.Min(minDistanceSq, p.distanceSq(curposition));
                        }
                        foreach (Vector2 p in state.OurPositions)
                        {
                            double d = p.distanceSq(curposition);
                            if (d > .000001)
                                minDistanceSq = Math.Min(minDistanceSq, d);
                        }
                    }
                }
                time = DateTime.Now - start;
            } while (time.TotalSeconds < seconds && totalCalls < 2000000000);
            TestResults t = new TestResults(totalruns, totalCalls, (double)time.TotalMilliseconds, (double)Math.Sqrt(minDistanceSq));

            initialize();

            //thread-safe:

            //this.restoreFocus();
            //this.Invoke(new FormRestoreFocusDelegate(this.restoreFocus));
            //this.Invalidate();
            //this.Invoke(new FormInvalidateDelegate(this.Invalidate));

            return t;
        }*/

        void step(int num)
        {
            for (int i = 0; i < num; i++)
            {
                MotionPlanningResults results = planner.PlanMotion(0, new RobotInfo(destination, 0, 0), engine, .13);
                engine.setMotorSpeeds(0, results.wheel_speeds);
                engine.step(.01);
                /*int numRobots = state.Destinations.Length;
                for (int r = 0; r < numRobots; r++)
                {
                    {
                        Vector2 curposition = state.OurPositions[r];
                        NavigationResults results;
                        Vector2 waypoint;
                        RobotInfo[] ourinfos = new RobotInfo[state.OurPositions.Length];
                        for (int j = 0; j < state.OurPositions.Length; j++)
                        {
                            ourinfos[j] = new RobotInfo(state.OurPositions[j], 0, j);
                        }
                        RobotInfo[] theirinfos = new RobotInfo[state.TheirPositions.Length];
                        for (int j = 0; j < state.TheirPositions.Length; j++)
                        {
                            theirinfos[j] = new RobotInfo(state.TheirPositions[j], 0, j);
                        }
                        lock (navigator)
                        {
                            results = navigator.navigate(r, curposition, state.Destinations[r], ourinfos, theirinfos, new BallInfo(state.BallPos), .12);
                            waypoint = results.waypoint;
                        }
                        Vector2 newvelocity = (waypoint - curposition);
                        if (newvelocity.magnitudeSq() > moveSpeed * moveSpeed)
                            newvelocity = moveSpeed * (newvelocity.normalize());
                        newvelocity = (1 - momentum) * newvelocity + momentum * state.OurVelocities[r];
                        newvelocity = (1 - friction) * newvelocity;
                        state.OurPositions[r] = curposition + newvelocity;
                        state.OurVelocities[r] = newvelocity;
                    }
                }
                moveObstacles();*/
            }
        }
        /*void moveObstacles()
        {
            for (int i = 0; i < state.OurPositions.Length; i++)
            {
                if (state.OurWaypoints[i].Length > 1)
                {
                    double v = (double)Math.Sqrt(state.OurVelocities[i].magnitudeSq());
                    while (state.OurPositions[i].distanceSq(state.currentPathWaypoint(true, i)) <= .5 * v * v)
                        state.nextPathWaypoint(true, i);
                    Vector2 next = state.currentPathWaypoint(true, i);
                    state.OurPositions[i] += v * (next - state.OurPositions[i]).normalize();
                }
            }
            for (int i = 0; i < state.TheirPositions.Length; i++)
            {
                if (state.TheirWaypoints[i].Length > 1)
                {
                    double v = (double)Math.Sqrt(state.TheirVelocities[i].magnitudeSq());
                    while (state.TheirPositions[i].distanceSq(state.currentPathWaypoint(false, i)) <= .5 * v * v)
                        state.nextPathWaypoint(false, i);
                    Vector2 next = state.currentPathWaypoint(false, i);
                    state.TheirPositions[i] += v * (next - state.TheirPositions[i]).normalize();
                }
            }
        }*/
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
        #endregion
        #region Coordinate Conversions
        static readonly ICoordinateConverter c = new BasicCoordinateConverter(500, 30, 50);
        private Vector2 fieldtopixelPoint(Vector2 p)
        {
            return c.fieldtopixelPoint(p);
        }
        private Vector2 pixeltofieldPoint(Vector2 p)
        {
            return c.pixeltofieldPoint(p);
        }
        #endregion
        #region Drawing
        readonly double robotPixelRadius = c.fieldtopixelDistance(.1);
        readonly double waypointPixelRadius = c.fieldtopixelDistance(.05);
        private object graphicsLock = new object();
        private void RRTTester_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            lock (graphicsLock)
            {
                try
                {
                    Brush b = new SolidBrush(Color.Black);
                    foreach (RobotInfo info in engine.getOurTeamInfo())
                    {
                        Vector2 pp = fieldtopixelPoint(info.Position);
                        g.FillEllipse(b, (float)(pp.X - robotPixelRadius), (float)(pp.Y - robotPixelRadius),
                            (float)(2 * robotPixelRadius), (float)(2 * robotPixelRadius));
                    }
                    g.DrawRectangle(new Pen(b, 3), c.fieldtopixelX(-2.75), c.fieldtopixelY(2),
                        (float)c.fieldtopixelDistance(5.5), (float)c.fieldtopixelDistance(4));
                    b.Dispose();
                    b = new SolidBrush(Color.Red);
                    foreach (RobotInfo info in engine.getTheirTeamInfo())
                    {
                        Vector2 pp = fieldtopixelPoint(info.Position);
                        g.FillEllipse(b, (float)(pp.X - robotPixelRadius), (float)(pp.Y - robotPixelRadius),
                            (float)(2 * robotPixelRadius), (float)(2 * robotPixelRadius));
                    }
                    b.Dispose();

                    b = new SolidBrush(Color.Green);
                    Vector2 dest = fieldtopixelPoint(destination);
                    g.FillEllipse(b, (float)(dest.X - 5), (float)(dest.Y - 5), 10, 10);
                    b.Dispose();
                    b = new SolidBrush(Color.Orange);
                    Vector2 ballpos = fieldtopixelPoint(engine.getBallInfo().Position);
                    g.FillEllipse(b, (float)(ballpos.X - 5), (float)(ballpos.Y - 5), 10, 10);
                    b.Dispose();
                    if (debugDraw() && planner != null)
                    {
                        lock (planner)
                        {
                            try
                            {
                                planner.DrawLast(g, c);
                            }
                            //sometimes it throws this, i think because of some synchronization issue with the Graphics object
                            catch (AccessViolationException) { }
                        }
                    }
                }
                catch (InvalidOperationException) { }
            }
        }
        private bool debugDraw()
        {
            return checkBoxDebugDrawing.Checked;
        }
        #endregion

        #region User Input
        RobotInfo clickedRobot = null;
        bool movingBall = false;
        private void RRTTester_MouseDown(object sender, MouseEventArgs e)
        {
            clickedRobot = null;
            movingBall = false;
            Vector2 clickPoint = pixeltofieldPoint((Vector2)e.Location);
            foreach (RobotInfo info in engine.getOurTeamInfo())
            {
                Vector2 p = info.Position;
                if (p.distanceSq(clickPoint) <= .1 * .1)
                {
                    clickedRobot = info;
                    return;
                }
            }
            foreach (RobotInfo info in engine.getTheirTeamInfo())
            {
                Vector2 p = info.Position;
                if (p.distanceSq(clickPoint) <= .1 * .1)
                {
                    clickedRobot = info;
                    return;
                }
            }
            /*for (int i = 0; i < 1; i++)
            {
                Vector2 p = destination;
                if (clickPoint.distanceSq(p) <= .05 * .05)
                {
                    clickedArray = state.Destinations;
                    clickedIndex = i;
                }
            }*/
            if (clickPoint.distanceSq(engine.getBallInfo().Position) <= .05 * .05)
                movingBall = true;
        }

        private void RRTTester_MouseMove(object sender, MouseEventArgs e)
        {
            if (clickedRobot != null)
            {
                RobotInfo new_info = new RobotInfo(pixeltofieldPoint((Vector2)e.Location), clickedRobot.Orientation, clickedRobot.ID);
                engine.MoveRobot(clickedRobot.ID, new_info);
                clickedRobot = new_info;
                this.Invalidate();
            }
            else if (movingBall)
            {
                engine.MoveBall(pixeltofieldPoint((Vector2)e.Location));
                this.Invalidate();
            }
        }

        private void RRTTester_MouseUp(object sender, MouseEventArgs e)
        {
            /*if (e.Button == MouseButtons.Right)
            {
                if (clickedArray == state.OurPositions && clickedIndex >= state.Destinations.Length)
                {
                    Vector2[] oldArray = state.OurWaypoints[clickedIndex];
                    state.OurWaypoints[clickedIndex] = new Vector2[oldArray.Length + 1];
                    oldArray.CopyTo(state.OurWaypoints[clickedIndex], 0);
                    state.OurWaypoints[clickedIndex][oldArray.Length] = c.pixeltofieldPoint((Vector2)e.Location);
                }
                else if (clickedArray == state.TheirPositions)
                {
                    Vector2[] oldArray = state.TheirWaypoints[clickedIndex];
                    state.TheirWaypoints[clickedIndex] = new Vector2[oldArray.Length + 1];
                    oldArray.CopyTo(state.TheirWaypoints[clickedIndex], 0);
                    state.TheirWaypoints[clickedIndex][oldArray.Length] = c.pixeltofieldPoint((Vector2)e.Location);
                }
                this.Invalidate();
            }
            clickedArray = null;
            clickedIndex = -1;*/
            clickedRobot = null;
            movingBall = false;
        }

        private void navigatorChooseBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int i = plannerChooseBox.SelectedIndex;
            planner = RRTFactory.createPlanner(RRTFactory.NavigatorTypes[i]);
            //navigatorChooseBox.Visible = false;

            restoreFocus();
        }

        /*private void calculateReferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            INavigator prev = navigator;
            navigator = NavigatorFactory.createReferenceNavigator();
            TestResults t = test();
            state.ReferenceResults = t;
            if (savedState == null)
                savedState = state.Clone();
            savedState.ReferenceResults = state.ReferenceResults;
            MessageBox.Show(t.compileSingleResult(state.ReferenceResults));
            navigator = prev;
            restoreFocus();
        }*/

        /*private void textBoxTestLength_TextChanged(object sender, EventArgs e)
        {
            restoreFocus();
        }*/

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
                step(1000);
                timer.Stop();
                MessageBox.Show(timer.Duration.ToString());
            }
            else if (c == 'r')
            {
                berunning = !berunning;
                if (berunning)
                {
                    t = new System.Threading.Timer(new System.Threading.TimerCallback(show), null, 0, 10);

                }
                else
                    t.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
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