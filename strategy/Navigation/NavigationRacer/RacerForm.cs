using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Navigation;

using Robocup.Infrastructure;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace NavigationRacer
{
    public partial class RacerForm : Form
    {
        INavigator navigator;
        FieldState state;

        List<Arrow> arrows = new List<Arrow>();

        FieldState savedState = null;
        void initialize()
        {
            if (savedState == null)
                state = FieldState.Default;
            else
                state = savedState.Clone();
        }

        public RacerForm()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            InitializeComponent();

            savedState = null;
            initialize();

            Type[] navigatorTypes = NavigatorFactory.NavigatorTypes;
            foreach (Type t in navigatorTypes)
            {
                navigatorChooseBox.Items.Add(t.Name);
            }

            navigatorChooseBox.SelectedIndex = navigatorChooseBox.Items.Count - 1;
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
        const float distThresh = .02f;
        const float moveSpeed = .015f;
        const float momentum = .975f; //the percentage of the new velocity that comes from the old velocity
        const float friction = .015f; //the amount lost to friction
        private TestResults test()
        {
            int numRobots = state.Destinations.Length;

            int totalruns = 0;
            DateTime start = DateTime.Now;
            TimeSpan time;
            int totalCalls = 0;
            float minDistanceSq = 100000f;
            float seconds = float.Parse(textBoxTestLength.Text);
            do
            {
                initialize();
                totalruns += numRobots;
                float dist = 0;
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
                            float d = p.distanceSq(curposition);
                            if (d > .000001)
                                minDistanceSq = Math.Min(minDistanceSq, d);
                        }
                    }
                }
                time = DateTime.Now - start;
            } while (time.TotalSeconds < seconds && totalCalls < 2000000000);
            TestResults t = new TestResults(totalruns, totalCalls, (float)time.TotalMilliseconds, (float)Math.Sqrt(minDistanceSq));

            initialize();

            //thread-safe:

            //this.restoreFocus();
            //this.Invoke(new FormRestoreFocusDelegate(this.restoreFocus));
            //this.Invalidate();
            //this.Invoke(new FormInvalidateDelegate(this.Invalidate));

            return t;
        }

        void step(int num)
        {
            int numRobots = state.Destinations.Length;
            for (int r = 0; r < numRobots; r++)
            {
                for (int i = 0; i < num; i++)
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
                        results = navigator.navigate(r, curposition, state.Destinations[r], ourinfos, theirinfos, new BallInfo(state.BallPos), .12f);
                        waypoint = results.waypoint;
                    }
                    Vector2 newvelocity = (waypoint - curposition);
                    if (newvelocity.magnitudeSq() > moveSpeed * moveSpeed)
                        newvelocity = moveSpeed * (newvelocity.normalize());
                    newvelocity = (1 - momentum) * newvelocity + momentum * state.OurVelocities[r];
                    newvelocity = (1 - friction) * newvelocity;
                    state.OurPositions[r] = curposition + newvelocity;
                    state.OurVelocities[r] = newvelocity;
                    if (waypoint.distanceSq(curposition) > .01 * .01)
                    {
                        lock (arrows)
                        {
                            arrows.Clear();
                            arrows.Add(new Arrow(fieldtopixelPoint(curposition), fieldtopixelPoint(waypoint), Color.Red, 2));
                        }
                    }
                }
            }
            moveObstacles();
        }
        void moveObstacles()
        {
            for (int i = 0; i < state.OurPositions.Length; i++)
            {
                if (state.OurWaypoints[i].Length > 1)
                {
                    float v = (float)Math.Sqrt(state.OurVelocities[i].magnitudeSq());
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
                    float v = (float)Math.Sqrt(state.TheirVelocities[i].magnitudeSq());
                    while (state.TheirPositions[i].distanceSq(state.currentPathWaypoint(false, i)) <= .5 * v * v)
                        state.nextPathWaypoint(false, i);
                    Vector2 next = state.currentPathWaypoint(false, i);
                    state.TheirPositions[i] += v * (next - state.TheirPositions[i]).normalize();
                }
            }
        }
        int numrunning = 0;
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
        readonly ICoordinateConverter c = new CoordinateConversions();
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
        readonly float robotPixelRadius = new CoordinateConversions().fieldtopixelDistance(.1f);
        readonly float waypointPixelRadius = new CoordinateConversions().fieldtopixelDistance(.05f);
        private object GraphicsLock = new object();
        private void RacerForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            lock (GraphicsLock)
            {
                try
                {
                    Brush purpleBrush = new SolidBrush(Color.Purple);
                    Pen purplePen = new Pen(Color.Purple, 2);
                    foreach (Vector2[] points in state.OurWaypoints)
                    {
                        if (points.Length > 1)
                        {
                            PointF[] waypoints = new PointF[points.Length];
                            for (int i = 0; i < waypoints.Length; i++)
                            {
                                waypoints[i] = c.fieldtopixelPoint(points[i]);
                                g.FillEllipse(purpleBrush, waypoints[i].X - waypointPixelRadius, waypoints[i].Y - waypointPixelRadius,
                                    2 * waypointPixelRadius, 2 * waypointPixelRadius);
                            }
                            g.DrawPolygon(purplePen, waypoints);
                        }
                    }
                    foreach (Vector2[] points in state.TheirWaypoints)
                    {
                        if (points.Length > 1)
                        {
                            PointF[] waypoints = new PointF[points.Length];
                            for (int i = 0; i < waypoints.Length; i++)
                            {
                                waypoints[i] = c.fieldtopixelPoint(points[i]);
                                g.FillEllipse(purpleBrush, waypoints[i].X - waypointPixelRadius, waypoints[i].Y - waypointPixelRadius,
                                    2 * waypointPixelRadius, 2 * waypointPixelRadius);
                            }
                            g.DrawPolygon(purplePen, waypoints);
                        }
                    }
                    purpleBrush.Dispose();
                    purplePen.Dispose();
                    Brush b = new SolidBrush(Color.Black);
                    foreach (Vector2 p in state.OurPositions)
                    {
                        Vector2 pp = fieldtopixelPoint(p);
                        g.FillEllipse(b, pp.X - robotPixelRadius, pp.Y - robotPixelRadius, 2 * robotPixelRadius, 2 * robotPixelRadius);
                    }
                    b.Dispose();
                    b = new SolidBrush(Color.Red);
                    foreach (Vector2 p in state.TheirPositions)
                    {
                        Vector2 pp = fieldtopixelPoint(p);
                        g.FillEllipse(b, pp.X - robotPixelRadius, pp.Y - robotPixelRadius, 2 * robotPixelRadius, 2 * robotPixelRadius);
                    }
                    b.Dispose();

                    b = new SolidBrush(Color.Green);
                    foreach (Vector2 p in state.Destinations)
                    {
                        Vector2 dest = fieldtopixelPoint(p);
                        g.FillEllipse(b, dest.X - 5, dest.Y - 5, 10, 10);
                    }
                    b.Dispose();
                    b = new SolidBrush(Color.Orange);
                    Vector2 ballpos = fieldtopixelPoint(state.BallPos);
                    g.FillEllipse(b, ballpos.X - 5, ballpos.Y - 5, 10, 10);
                    b.Dispose();
                    lock (arrows)
                    {
                        foreach (Arrow a in arrows)
                        {
                            a.draw(g);
                        }
                    }
                    if (debugDraw() && navigator != null)
                    {
                        lock (navigator)
                        {
                            try
                            {
                                navigator.drawLast(g, c);
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
        Vector2[] clickedArray = null;
        int clickedIndex = -1;
        bool movingBall = false;
        private void RacerForm_MouseDown(object sender, MouseEventArgs e)
        {
            clickedArray = null;
            clickedIndex = -1;
            movingBall = false;
            Vector2 clickPoint = pixeltofieldPoint((Vector2)e.Location);
            for (int i = 0; i < state.OurPositions.Length; i++)
            {
                Vector2 p = state.OurPositions[i];
                if (p.distanceSq(clickPoint) <= .1 * .1)
                {
                    clickedArray = state.OurPositions;
                    clickedIndex = i;
                    return;
                }
                for (int j = 0; j < state.OurWaypoints[i].Length; j++)
                {
                    Vector2 p2 = state.OurWaypoints[i][j];
                    if (p2.distanceSq(clickPoint) <= .05 * .05)
                    {
                        clickedArray = state.OurWaypoints[i];
                        clickedIndex = j;
                    }
                }
            }
            for (int i = 0; i < state.TheirPositions.Length; i++)
            {
                Vector2 p = state.TheirPositions[i];
                if (p.distanceSq(clickPoint) <= .1 * .1)
                {
                    clickedArray = state.TheirPositions;
                    clickedIndex = i;
                    return;
                }
                for (int j = 0; j < state.TheirWaypoints[i].Length; j++)
                {
                    Vector2 p2 = state.TheirWaypoints[i][j];
                    if (p2.distanceSq(clickPoint) <= .05 * .05)
                    {
                        clickedArray = state.TheirWaypoints[i];
                        clickedIndex = j;
                    }
                }
            }
            for (int i = 0; i < state.Destinations.Length; i++)
            {
                Vector2 p = state.Destinations[i];
                if (clickPoint.distanceSq(p) <= .05 * .05)
                {
                    clickedArray = state.Destinations;
                    clickedIndex = i;
                }

            }
            if (clickPoint.distanceSq(state.BallPos) <= .05 * .05)
                movingBall = true;
        }

        private void RacerForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (clickedArray != null)
            {
                clickedArray[clickedIndex] = pixeltofieldPoint((Vector2)e.Location);
                this.Invalidate();
            }
            else if (movingBall)
            {
                state.BallPos = pixeltofieldPoint((Vector2)e.Location);
                this.Invalidate();
            }
        }

        private void RacerForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
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
            clickedIndex = -1;
            movingBall = false;
            savedState = state.Clone();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog.ShowDialog();
        }

        private void saveFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            System.IO.Stream stream = saveFileDialog.OpenFile();
            System.IO.StreamWriter writer = new System.IO.StreamWriter(stream);

            try
            {
                writer.WriteLine(state.save());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

            writer.Close();
            stream.Close();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog();
        }

        private void openFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            System.IO.Stream stream = openFileDialog.OpenFile();
            System.IO.StreamReader reader = new System.IO.StreamReader(stream);

            //try
            //{
            savedState = FieldState.load(reader);
            initialize();
            this.Invalidate();
            /*}
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                throw new Exception("error opening file", ex);
            }*/

            reader.Close();
            stream.Close();
        }

        private void navigatorChooseBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int i = navigatorChooseBox.SelectedIndex;
            navigator = NavigatorFactory.createNavigator(NavigatorFactory.NavigatorTypes[i]);
            //navigatorChooseBox.Visible = false;

            restoreFocus();
        }

        private void calculateReferenceToolStripMenuItem_Click(object sender, EventArgs e)
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
        }

        private void textBoxTestLength_TextChanged(object sender, EventArgs e)
        {
            restoreFocus();
        }

        private void RacerForm_Activated(object sender, EventArgs e)
        {

            restoreFocus();
        }


        private void checkBoxDebugDrawing_CheckedChanged(object sender, EventArgs e)
        {
            restoreFocus();
        }
        private void RacerForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            if (c == 't')
            {
                TestResults t = test();
                MessageBox.Show(t.compileSingleResult(state.ReferenceResults));
                this.restoreFocus();
            }
            else if (c == 's')
            {
                berunning = !berunning;
                if (berunning)
                {
                    t = new System.Threading.Timer(new System.Threading.TimerCallback(show), null, 0, 10);

                }
                else
                    t.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            }
            else if (c == 'r')
            {
                if (savedState != null)
                {
                    state = savedState.Clone();
                    this.Invalidate();
                }
            }
            else if (c == ' ')
            {
                step(1);
                this.Invalidate();
            }
        }

        private void testAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //open dialog box, get list of files
            OpenFileDialog dlgTestAll = new OpenFileDialog();
            dlgTestAll.Title = "Run multiple tests";
            dlgTestAll.CheckFileExists = true;
            dlgTestAll.CheckPathExists = true;
            dlgTestAll.Filter = "Test state (*.txt) | *.txt";
            dlgTestAll.Multiselect = true;
            dlgTestAll.FileOk += new System.ComponentModel.CancelEventHandler(this.dlgTestAll_FileOK);

            dlgTestAll.ShowDialog();

        }

        private void dlgTestAll_FileOK(object sender, CancelEventArgs e)
        {

            if (e.Cancel == true)
            {
                resetAll();
            }
            else
            {
                startTesting(((OpenFileDialog)sender).FileNames);

            }
        }

        private void dlgSaveToCSV_FileOK(object sender, CancelEventArgs e)
        {
            if (e.Cancel == true)
            {
                resetAll();
            }
            else
            {
                saveToCSV((TestResults[,])(((SaveFileDialog)sender).Tag), ((SaveFileDialog)sender).FileName);
            }
        }
        #endregion

        #region Batch Testing
        const bool sortByTest = true;

        //necessary for thread-safety
        private delegate void TestMultipleDelegate(string[] filenames, out TestResults[,] testResults);

        private delegate void FormRestoreFocusDelegate();
        private delegate void FormInvalidateDelegate();
        private delegate void ResetAllDelegate();
        private delegate void ShowTestResultsDialogDelegate(TestResults[,] testResults);
        private delegate void IncrProgressDelegate();

        private FrmBatchTesting frmBatchTesting;
        private bool testingStopped;
        private IncrProgressDelegate incrProgressDel;


        /***
         *   Two Main THREADING methods
        ***/

        private void startTesting(string[] testFilenames)
        {
            IAsyncResult asyncResult = null;
            TestMultipleDelegate testMultipleDelegate = new TestMultipleDelegate(this.testMultiple);
            AsyncCallback doneTestingCB = new AsyncCallback(this.doneTesting);

            //have to be there - but serve no purpose
            TestResults[,] irrelevant = null;

            //create the progress indicator form
            this.Enabled = false;
            frmBatchTesting = new FrmBatchTesting(testFilenames.Length, testFilenames.Length * (NavigatorFactory.NavigatorTypes.Length + 1), this);
            frmBatchTesting.Show();
            frmBatchTesting.Focus();
            incrProgressDel = new IncrProgressDelegate(frmBatchTesting.incrProgress);

            //spawn a thread with the big testing process
            asyncResult = testMultipleDelegate.BeginInvoke(testFilenames, out irrelevant, doneTestingCB, null);

        }

        private void doneTesting(IAsyncResult asyncResult)
        {
            TestMultipleDelegate testMultipleDelegate;
            TestResults[,] testResults;

            // Extract the delegate from the AsyncResult.  
            testMultipleDelegate = (TestMultipleDelegate)((AsyncResult)asyncResult).AsyncDelegate;
            // Obtain the results.
            testMultipleDelegate.EndInvoke(out testResults, asyncResult);

            this.Invoke(new ResetAllDelegate(this.resetAll));
            this.Invoke(new ShowTestResultsDialogDelegate(this.showSaveTestResultsDialog), new object[] { testResults });

        }

        private void incrProgress()
        {
            if (frmBatchTesting != null)
            {
                frmBatchTesting.incrProgress();
            }
        }

        private void resetAll()
        {
            frmBatchTesting.Dispose();
            frmBatchTesting = null;
            testingStopped = false;
            this.Enabled = true;
            this.Focus();
            restoreFocus();
        }

        public void stopTesting()
        {
            testingStopped = true;
        }

        /*
         * testMultiple: array of test files (->) results as matrix [File x Navigator]
         * NOTE: fetches list of navigators by itself
         * 
         */
        private void testMultiple(string[] testFilenames, out TestResults[,] testResults)
        {

            int i, j;

            StreamReader inStream;
            INavigator[] navigators;

            INavigator refNavigator;


            //create all navigators
            navigators = new INavigator[NavigatorFactory.NavigatorTypes.Length];
            for (i = 0; i < navigators.Length; i++)
            {
                navigators[i] = NavigatorFactory.createNavigator(NavigatorFactory.NavigatorTypes[i]);
            }

            testResults = new TestResults[testFilenames.Length, navigators.Length + 1]; //one for reference results

            //for computing reference results
            refNavigator = NavigatorFactory.createReferenceNavigator();

            i = 0;
            while (i < testFilenames.Length && !testingStopped)
            {

                inStream = new StreamReader(testFilenames[i]);
                state = FieldState.load(inStream);

                //compute ref results
                navigator = refNavigator;
                testResults[i, 0] = test(); //this has to come first cause it creates the object
                this.Invoke(incrProgressDel);
                testResults[i, 0].TestFileName = Path.GetFileName(testFilenames[i]);
                testResults[i, 0].NavigatorName = "Reference Navigator";

                //run the test using all navigators
                for (j = 1; j < navigators.Length + 1 && !testingStopped; j++)
                {
                    inStream.DiscardBufferedData();
                    inStream.BaseStream.Seek(0, SeekOrigin.Begin);
                    navigator = navigators[j - 1]; //navigators is zero-indexed
                    testResults[i, j] = test();
                    testResults[i, j].NavigatorName = navigators[j - 1].Name; //"Navigator #" + j.ToString();
                    this.Invoke(incrProgressDel);

                }

                inStream.Close();

                //update progress indicator

                i++;
            }

        }
        private void showSaveTestResultsDialog(TestResults[,] testResults)
        {
            SaveFileDialog dlgSaveToCSV = new SaveFileDialog();
            dlgSaveToCSV.Title = "Save test results to CSV";
            dlgSaveToCSV.DefaultExt = ".txt";
            dlgSaveToCSV.OverwritePrompt = true;
            dlgSaveToCSV.Filter = "Test result (*.csv) | *.csv";
            dlgSaveToCSV.Tag = testResults;
            dlgSaveToCSV.FileOk += new System.ComponentModel.CancelEventHandler(this.dlgSaveToCSV_FileOK);

            dlgSaveToCSV.ShowDialog();
        }

        void saveToCSV(TestResults[,] testResults, string filename)
        {
            int i, j;
            StringBuilder sb = new StringBuilder();

            if (sortByTest)
            {
                for (i = 0; i < testResults.GetLength(0); i++)
                {
                    if (testResults[i, 0] == null)
                    {
                        continue;
                    }
                    sb.AppendLine("Test File:");
                    sb.AppendLine(testResults[i, 0].TestFileName + ",,,,,,,,");
                    sb.Append(",");

                    //header
                    sb.Append("ClosestDistanceToObstacle"); sb.Append(",");
                    sb.Append("AverageMillisecondsPerRun"); sb.Append(",");
                    sb.Append("AverageIterationsPerRun"); sb.Append(",");
                    sb.Append("AverageMillisecondsPerIteration"); sb.Append(",");
                    sb.Append("Estimated time"); sb.Append(",");
                    sb.Append("TotalTimeScore"); sb.Append(",");
                    sb.Append("IterationSpeedScore"); sb.Append(",");
                    sb.Append("IterationsScore");

                    sb.AppendLine();


                    for (j = 0; j < testResults.GetLength(1); j++)
                    {
                        if (testResults[i, j] == null)
                        {
                            continue;
                        }
                        sb.Append(testResults[0, j].NavigatorName + ",");
                        sb.Append(testResults[i, j].ClosestDistanceToObstacle); sb.Append(",");
                        sb.Append(testResults[i, j].AverageMillisecondsPerRun); sb.Append(",");
                        sb.Append(testResults[i, j].AverageIterationsPerRun); sb.Append(",");
                        sb.Append(testResults[i, j].AverageMillisecondsPerIteration); sb.Append(",");
                        sb.Append((testResults[i, j].AverageIterationsPerRun / 200 +
                                   5 * testResults[i, j].AverageMillisecondsPerRun / 1000)); sb.Append(",");
                        sb.Append(testResults[i, j].TotalTimeScore(testResults[i, 0])); sb.Append(",");
                        sb.Append(testResults[i, j].IterationSpeedScore(testResults[i, 0])); sb.Append(",");
                        sb.Append(testResults[i, j].IterationsScore(testResults[i, 0]));

                        sb.AppendLine();

                    }
                }
            }
#pragma warning disable 0162
            else
            {
                for (j = 0; j < testResults.GetLength(1); j++)
                {
                    if (testResults[0, j] == null)
                    {
                        continue;
                    }
                    sb.AppendLine(testResults[0, j].NavigatorName + ",,,,,,,,");

                    //header
                    sb.Append("Test File"); sb.Append(",");
                    sb.Append("ClosestDistanceToObstacle"); sb.Append(",");
                    sb.Append("AverageMillisecondsPerRun"); sb.Append(",");
                    sb.Append("AverageIterationsPerRun"); sb.Append(",");
                    sb.Append("AverageMillisecondsPerIteration"); sb.Append(",");
                    sb.Append("Estimated time"); sb.Append(",");
                    sb.Append("TotalTimeScore"); sb.Append(",");
                    sb.Append("IterationSpeedScore"); sb.Append(",");
                    sb.Append("IterationsScore");

                    sb.Append("\n");


                    for (i = 0; i < testResults.GetLength(0); i++)
                    {
                        if (testResults[i, j] == null)
                        {
                            continue;
                        }
                        sb.Append(testResults[i, 0].TestFileName); sb.Append(",");
                        sb.Append(testResults[i, j].ClosestDistanceToObstacle); sb.Append(",");
                        sb.Append(testResults[i, j].AverageMillisecondsPerRun); sb.Append(",");
                        sb.Append(testResults[i, j].AverageIterationsPerRun); sb.Append(",");
                        sb.Append(testResults[i, j].AverageMillisecondsPerIteration); sb.Append(",");
                        sb.Append((testResults[i, j].AverageIterationsPerRun / 200 +
                                   5 * testResults[i, 0].AverageMillisecondsPerRun / 1000)); sb.Append(",");
                        sb.Append(testResults[i, j].TotalTimeScore(testResults[i, 0])); sb.Append(",");
                        sb.Append(testResults[i, j].IterationSpeedScore(testResults[i, 0])); sb.Append(",");
                        sb.Append(testResults[i, j].IterationsScore(testResults[i, 0]));

                        sb.Append("\n");

                    }
                }
            }
#pragma warning restore 0162

            StreamWriter writer = new StreamWriter(filename);
            writer.Write(sb.ToString());
            writer.Close();
        }

        #endregion

        private void RacerForm_Load(object sender, EventArgs e)
        {

        }

        private void RacerForm_MouseClick(object sender, MouseEventArgs e)
        {
            restoreFocus();
        }
    }
}
