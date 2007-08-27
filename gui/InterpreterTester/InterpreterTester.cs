#define DIFFERENTPLAYS
//#define MULTITHREAD
//#define TIMING


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

using RobocupPlays;
using System.Collections;

using Robocup.Infrastructure;

//using Navigator = Navigation.Examples.LookAheadPotential;
using Navigator = Navigation.Examples.DumbNavigator;
//using Navigator = Navigation.Current.CurrentNavigator;

using RobotInfo = Robocup.Infrastructure.RobotInfo;

namespace InterpreterTester
{
    public partial class InterpreterTester : Form, IController, IPredictor
    {
        Interpreter interpreter, defensiveinterpreter;
        const float ballspeed = .06f, balldecay = .98f;
        private const float ballbounce = .01f, collisionradius = .12f, speed = 0.02f;

        const int testIterations = 1000;

        Random r = new Random();

        private PlayTypes playType = PlayTypes.NormalPlay;

        //private List<InterpreterPlay> plays;

        private Dictionary<InterpreterPlay, string> filenames;

        /*private void savePlays()
        {
            try
            {
                foreach (KeyValuePair<InterpreterPlay, string> pair in filenames)
                {
                    string s = pair.Key.Save();
                    StreamWriter writer = new StreamWriter(pair.Value);
                    writer.WriteLine(s);
                    writer.Close();
                    writer.Dispose();
                }
            }
            catch (IOException)
            {
                MessageBox.Show("Error saving the files --\n too bad, you've probably lost whichever one was being saved");
            }
        }*/

        private void loadPlays()
        {
            PlayLoader<InterpreterPlay, InterpreterExpression> loader =
                new PlayLoader<InterpreterPlay, InterpreterExpression>(new InterpreterExpression.Factory());
            string[] files = System.IO.Directory.GetFiles("../../Plays");
            List<InterpreterPlay> plays = new List<InterpreterPlay>();
            filenames = new Dictionary<InterpreterPlay, string>();
            foreach (string fname in files)
            {
                string extension = fname.Substring(1 + fname.LastIndexOf('.'));
                if (extension != "txt")
                    continue;
                StreamReader reader = new StreamReader(fname);
                string filecontents = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();

                try
                {
                    InterpreterPlay p = loader.load(filecontents);
                    plays.Add(p);
                    filenames.Add(p, fname);
                }
                catch (Exception ex)
                {
                    DialogResult d = MessageBox.Show("Error loading plays: " + ex.Message + "\n\nContinue? \"Cancel\" will throw this error", "Error #" + ((ex.Message.GetHashCode() % 9000 + 2000000000) % 9000 + 1000),
                        MessageBoxButtons.YesNoCancel, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2);
                    if (d == DialogResult.Cancel)
                    {
                        //I wanted to say "throw ex;", but then all you see is that the line "throw ex;" threw an error.
                        //this way, if you debug, you can actually go to the line where it occurs (assuming that an error happens again)
                        plays.Add(loader.load(filecontents));
                        MessageBox.Show("Wow...it threw an error the first time but not the second time.  Debugging this will be hard.");
                        Application.Exit();
                    }
                    else if (d == DialogResult.No)
                    {
                        Application.Exit();
                        Environment.Exit(-1);
                    }
                }
            }
            interpreter = new Interpreter(false, plays.ToArray(), this, this);


#if DIFFERENTPLAYS
            files = System.IO.Directory.GetFiles("../../Plays/Opponent Plays");
            List<InterpreterPlay> defensiveplays = new List<InterpreterPlay>();
            foreach (string fname in files)
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(fname);
                string filecontents = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();

                try
                {
                    defensiveplays.Add(loader.load(filecontents));
                }
                catch (Exception ex)
                {
                    DialogResult d = MessageBox.Show("Error loading plays: " + ex.Message + "\n\nContinue? \"Cancel\" will throw this error", "Error #" + ((ex.Message.GetHashCode() % 9000 + 2000000000) % 9000 + 1000),
                        MessageBoxButtons.YesNoCancel, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2);
                    if (d == DialogResult.Cancel)
                    {
                        //I wanted to say "throw ex;", but then all you see is that the line "throw ex;" threw an error.
                        //this way, if you debug, you can actually go to the line where it occurs (assuming that an error happens again)
                        defensiveplays.Add(loader.load(filecontents));
                        MessageBox.Show("Wow...it threw an error the first time but not the second time.  Debugging this will be hard.");
                        Application.Exit();
                    }
                    else if (d == DialogResult.No)
                        Application.Exit();
                }
            }
#else
            List<InterpreterPlay> defensiveplays = plays;
#endif
            defensiveinterpreter = new Interpreter(true, defensiveplays.ToArray(),
                new TeamFlipperPredictor(this), this);
        }
        private void InterpreterTester_Load(object sender, EventArgs e)
        {
        }
#if MULTITHREAD
        System.Threading.Thread workerThread;
#endif
        public InterpreterTester()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            InitializeComponent();
            loadPlays();

            init();
#if MULTITHREAD
            workerThread = new System.Threading.Thread(delegate()
            {
                while (true)
                {
                    while (ourcopy3 == null || theircopy3 == null) { System.Threading.Thread.Sleep(0); }
                    interpreter.interpret(ourcopy3, theircopy3, ballinfo, playType);
                    ourcopy3 = null;
                    theircopy3 = null;
                }
            });
            workerThread.Start();
#endif
        }
        private void init()
        {
            ourgoals = theirgoals = 0;
            ballImmobile = 0;
            ourinfo = new RobotInfo[] {
                new RobotInfo(new Vector2(-1.0f, -1), 3, 0),
                new RobotInfo(new Vector2(-1.0f, 0), 3, 1),
                new RobotInfo(new Vector2(-1.0f, 1), 3, 2),
                new RobotInfo(new Vector2(-2f, -1), 3, 3),
                new RobotInfo(new Vector2(-2f, 1), 3, 4)
            };
            theirinfo = new RobotInfo[] {
                new RobotInfo(new Vector2(1.0f, -1), 3, TEAMSIZE+0),
                new RobotInfo(new Vector2(1.0f, 0), 3, TEAMSIZE+1),
                new RobotInfo(new Vector2(1.0f, 1), 3, TEAMSIZE+2),
                new RobotInfo(new Vector2(2f, -1), 3, TEAMSIZE+3),
                new RobotInfo(new Vector2(2f, 1), 3, TEAMSIZE+4)
            };
            ballinfo = new BallInfo(new Vector2(0, 0), 0, 0);
            ballvx = ballvy = 0;
        }

        int ourgoals = 0, theirgoals = 0;
        const int TEAMSIZE = 5;

        RobotInfo[] ourinfo;

        RobotInfo[] theirinfo;

        BallInfo ballinfo = new BallInfo(new Vector2(0, 0), 0, 0);
        float ballvx = 0, ballvy = 0;

        int ballImmobile = 0;

        #region Coordinate Conversions
        private int fieldtopixelX(double x)
        {
            return (int)(300 + 100 * x);
        }
        private int fieldtopixelY(double y)
        {
            return (int)(200 - 100 * y);
        }
        private Vector2 fieldtopixelPoint(Vector2 p)
        {
            return new Vector2(fieldtopixelX(p.X), fieldtopixelY(p.Y));
        }
        private float pixeltofieldX(float x)
        {
            return (float)((x - 300f) / 100f);
        }
        private float pixeltofieldY(float y)
        {
            return (float)((y - 200f) / -100f);
        }
        private Vector2 pixeltofieldPoint(Vector2 p)
        {
            return new Vector2(pixeltofieldX(p.X), pixeltofieldY(p.Y));
        }
        #endregion

        #region Drawing Commands
        private void drawRobot(RobotInfo r, Graphics g, Color c)
        {
            Brush b = new SolidBrush(c);
            Vector2 center = fieldtopixelPoint(r.Position);
            g.FillEllipse(b, center.X - 10, center.Y - 10, 20, 20);
            PointF[] corners = new PointF[4];
            float angle = -r.Orientation;
            float outerangle = .6f;
            float innerangle = 1.0f;
            float innerradius = 7f;
            float outerradius = 11f;
            corners[0] = (PointF)(center + (new Vector2((float)(innerradius * Math.Cos(angle + innerangle)), (float)(innerradius * Math.Sin(angle + innerangle)))));
            corners[1] = (PointF)(center + (new Vector2((float)(innerradius * Math.Cos(angle - innerangle)), (float)(innerradius * Math.Sin(angle - innerangle)))));
            corners[2] = (PointF)(center + (new Vector2((float)(outerradius * Math.Cos(angle - outerangle)), (float)(outerradius * Math.Sin(angle - outerangle)))));
            corners[3] = (PointF)(center + (new Vector2((float)(outerradius * Math.Cos(angle + outerangle)), (float)(outerradius * Math.Sin(angle + outerangle)))));
            Brush b2 = new SolidBrush(Color.Gray);
            g.FillPolygon(b2, corners);
            b2.Dispose();
            b.Dispose();
        }
        bool drawArrows = false;
        private void InterpreterTester_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Brush b0 = new SolidBrush(Color.YellowGreen);
            g.FillEllipse(b0, fieldtopixelX(-2.53) - 5, fieldtopixelY(0) - 5, 10, 10);
            g.FillEllipse(b0, fieldtopixelX(+2.53) - 5, fieldtopixelY(0) - 5, 10, 10);
            b0.Dispose();
            Pen p = new Pen(Color.Black, 3);
            g.DrawRectangle(p, fieldtopixelX(2.45), fieldtopixelY(.35), fieldtopixelX(2.63) - fieldtopixelX(2.45), fieldtopixelY(-.35) - fieldtopixelY(.35));
            g.DrawRectangle(p, fieldtopixelX(-2.63), fieldtopixelY(.35), fieldtopixelX(-2.45) - fieldtopixelX(-2.63), fieldtopixelY(-.35) - fieldtopixelY(.35));
            g.DrawRectangle(p, fieldtopixelX(-2.45), fieldtopixelY(1.7), fieldtopixelX(2.45) - fieldtopixelX(-2.45), fieldtopixelY(-1.7) - fieldtopixelY(1.7));
            p.Dispose();
            Brush b = new SolidBrush(Color.Black);
            foreach (RobotInfo r in ourinfo)
            {
                drawRobot(r, g, Color.Black);
                //g.FillEllipse(b, fieldtopixelX(r.Position.X) - 10, fieldtopixelY(r.Position.Y) - 10, 20, 20);
            }
            b.Dispose();
            b = new SolidBrush(Color.Red);
            foreach (RobotInfo r in theirinfo)
            {
                drawRobot(r, g, Color.Red);
                //g.FillEllipse(b, fieldtopixelX(r.Position.X) - 10, fieldtopixelY(r.Position.Y) - 10, 20, 20);
            }
            if (drawArrows)
            {
                lock (arrows)
                {
                    foreach (Arrow a in arrows)
                    {
                        a.draw(g);
                    }
                }
            }
            b.Dispose();
            b = new SolidBrush(Color.Orange);
            g.FillEllipse(b, fieldtopixelX(ballinfo.Position.X) - 3, fieldtopixelY(ballinfo.Position.Y) - 3, 6, 6);

        }
        List<Arrow> arrows = new List<Arrow>();
        void addArrow(Arrow a)
        {
            lock (arrows)
            {
                arrows.Add(a);
            }
        }
        #endregion
        #region Interpreting

#if MULTITHREAD
        volatile RobotInfo[] ourcopy3 = null;
        volatile RobotInfo[] theircopy3 = null;
#endif
        private void interpret()
        {
#if TIMING
            PAB.HiPerfTimer timer = new global::InterpreterTester.PAB.HiPerfTimer();
            timer.Start();
#endif
            lock (arrows)
            {
                arrows.Clear();
            }
            foreach (RobotInfo r in ourinfo)
            {
                r.Tags.Clear();
                if (r.ID == 0)
                    r.Tags.Add("goalie");
                r.setFree();
            }
            foreach (RobotInfo r in theirinfo)
                r.setFree();
#if MULTITHREAD
            int ol = ourinfo.Length, tl = theirinfo.Length;
            RobotInfo[] ourcopy1 = new RobotInfo[ol];
            RobotInfo[] theircopy1 = new RobotInfo[tl];
            for (int i = 0; i < ol; i++)
            {
                ourcopy1[i] = ourinfo[i].copy();
            }
            for (int i = 0; i < tl; i++)
            {
                theircopy1[i] = theirinfo[i].copy();
            }
#endif
#if TIMING
            timer.Stop();
            Console.WriteLine(timer.Duration * 1000 + " ms for getting ready to interpreter.interpret()");
            timer.Start();
#endif
#if MULTITHREAD
            ourcopy3 = ourcopy1;
            theircopy3 = theircopy1;
#else
            interpreter.interpret(playType);
            foreach (RobotInfo r in ourinfo)
                r.setFree();
            foreach (RobotInfo r in theirinfo)
                r.setFree();
#endif
            defensiveinterpreter.interpret(playType);
#if MULTITHREAD
            while (ourcopy3!=null && theircopy3!=null)
            {
            }
#endif
#if TIMING
            timer.Stop();
            Console.WriteLine(timer.Duration * 1000 + " ms for calling interpreter.interpret()");
#endif
        }
        private void goalScored(bool scoredByLeftTeam)
        {
            /*if (interpreter is LearningInterpreter)
            {
                LearningInterpreter linterpreter = (LearningInterpreter)interpreter;
                linterpreter.goalScored(scoredByLeftTeam);
            }
            if (defensiveinterpreter is LearningInterpreter)
            {
                LearningInterpreter linterpreter = (LearningInterpreter)defensiveinterpreter;
                linterpreter.goalScored(!scoredByLeftTeam);
            }*/
            if (scoredByLeftTeam)
                ourgoals++;
            else
                theirgoals++;
        }
        private void step()
        {
#if TIMING
            PAB.HiPerfTimer timer = new global::InterpreterTester.PAB.HiPerfTimer();
            timer.Start();
#endif

            Vector2 newballlocation = new Vector2(ballinfo.Position.X + ballvx, ballinfo.Position.Y + ballvy);

            bool collided = false;
            foreach (RobotInfo r in theirinfo)
            {
                Vector2 location = r.Position;
                if (newballlocation.distanceSq(location) <= collisionradius * collisionradius)
                {
                    collided = true;
                    ballvx = ballbounce * ((ballinfo.Position - location).normalize().X);
                    ballvy = ballbounce * ((ballinfo.Position - location).normalize().Y);
                    newballlocation = location + .13f * (ballinfo.Position - location).normalize();
                    break;
                }
            }
            if (!collided)
            {
                foreach (RobotInfo r in ourinfo)
                {
                    Vector2 location = r.Position;
                    if (newballlocation.distanceSq(location) <= collisionradius * collisionradius)
                    {
                        collided = true;
                        ballvx = ballbounce * ((ballinfo.Position - location).normalize().X);
                        ballvy = ballbounce * ((ballinfo.Position - location).normalize().Y);
                        newballlocation = location + .13f * (ballinfo.Position - location).normalize();
                        break;
                    }
                }
            }
            if (!collided)
            {
                Vector2 ball = ballinfo.Position;
                if (ball.X < -2.45)
                    ballvx = Math.Abs(ballvx);
                else if (ball.X > 2.45)
                    ballvx = -Math.Abs(ballvx);
                if (ball.Y < -1.7)
                    ballvy = Math.Abs(ballvy);
                else if (ball.Y > 1.7)
                    ballvy = -Math.Abs(ballvy);
                newballlocation = new Vector2(ballinfo.Position.X + ballvx, ballinfo.Position.Y + ballvy);
            }
            if (Math.Abs(ballinfo.Position.Y) <= .35 && Math.Abs(ballinfo.Position.X) >= 2.4)
            {
                goalScored(ballinfo.Position.X > 0);
                ballinfo = new BallInfo(new Vector2(0, 0), 0, 0);
                ballvx = ballvy = 0;
                return;
            }
            List<RobotInfo> allinfos = new List<RobotInfo>(10);
            allinfos.AddRange(ourinfo);
            allinfos.AddRange(theirinfo);
            for (int i = 0; i < allinfos.Count; i++)
            {
                for (int j = 0; j < allinfos.Count; j++)
                {
                    if (i == j)
                        continue;
                    Vector2 p1 = allinfos[i].Position;
                    Vector2 p2 = allinfos[j].Position;
                    if (p1.distanceSq(p2) <= .2 * .2)
                    {
                        Vector2 t1 = p1 + .01f * (p1 - p2).normalize();
                        Vector2 t2 = p2 + .01f * (p2 - p1).normalize();
                        if (i < ourinfo.Length)
                            ourinfo[i] = new RobotInfo(t1, allinfos[i].Orientation, allinfos[i].ID);
                        else
                            theirinfo[i - ourinfo.Length] = new RobotInfo(t1, allinfos[i].Orientation, allinfos[i].ID);

                        if (j < ourinfo.Length)
                            ourinfo[j] = new RobotInfo(t2, allinfos[j].Orientation, allinfos[j].ID);
                        else
                            theirinfo[j - ourinfo.Length] = new RobotInfo(t2, allinfos[j].Orientation, allinfos[j].ID);
                    }
                }
            }

            ballinfo = new BallInfo(newballlocation, ballvx, ballvy);
            ballvx *= balldecay;
            ballvy *= balldecay;

            bool immobile = false;
            if (ballvx * ballvx + ballvy * ballvy < .01 * .01)
                immobile = true;

            {
                const double threshsq = .35 * .35;
                int numTooClose = 0;
                foreach (RobotInfo info in ourinfo)
                {
                    double dist = ballinfo.Position.distanceSq(info.Position);
                    if (dist < threshsq)
                        numTooClose++;
                }
                foreach (RobotInfo info in theirinfo)
                {
                    double dist = ballinfo.Position.distanceSq(info.Position);
                    if (dist < threshsq)
                        numTooClose++;
                }
                if (numTooClose >= 4)
                    immobile = true;
            }

            if (immobile)
            {
                ballImmobile++;
                if (ballImmobile > 200)
                {
                    ballinfo = new BallInfo(new Vector2(0, 0), 0, 0);
                    ballvx = ballvy = 0;
                    ballImmobile = 0;
                }
            }
            else
                ballImmobile = 0;

#if TIMING
            timer.Stop();
            Console.WriteLine(timer.Duration * 1000 + " ms for infrastructure");
            timer.Start();
#endif
            interpret();
#if TIMING
            timer.Stop();
            Console.WriteLine(timer.Duration * 1000 + " ms for the strategy loop");
#endif
        }

        private void run(int times)
        {
            for (int i = 0; i < times; i++)
            {
                step();
            }
        }
        #endregion

        #region User Input
        bool berunning = false;
        System.Threading.Timer t = null;
        private void InterpreterTester_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = char.ToLower(e.KeyChar);
            if (c >= '1' && c <= '9')
            {
                run(c - '1' + 1);
                this.Invalidate();
            }
            else if (c == 'h')
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("KEYBOARD COMMANDS");
                sb.AppendLine("h  \t show this help box");
                sb.AppendLine("1-9\t advance that many frames");
                sb.AppendLine("r  \t sets it to run continuously");
                sb.AppendLine("R  \t same, but skips frames");
                sb.AppendLine("a  \t toggles arrow drawing");
                sb.AppendLine("p  \t reloads all the plays");
                sb.AppendLine("s  \t saves all the plays");
                sb.AppendLine("t  \t runs " + testIterations + " frames and shows you the elapsed time");
                //sb.AppendLine("o  \t shows current results");
                MessageBox.Show(sb.ToString());
            }
            /*else if (c == 's')
            {
                savePlays();
                //KeyCollection.Enumerator e = filenames.Keys.GetEnumerator();
                //MessageBox.Show(filenames.Keys.GetEnumerator()..Current.Save());
            }*/
            else if (c == 'a')
            {
                drawArrows = !drawArrows;
                this.Invalidate();
            }
            else if (c == 'r')
            {
                int atATime = 1;
                if (char.IsUpper(e.KeyChar))
                    atATime = 10;
                berunning = !berunning;
                if (berunning)
                {
                    t = new System.Threading.Timer(new System.Threading.TimerCallback(delegate(object o) { synchronousRun(atATime); }), null, 0, 10);

                }
                else
                    t.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            }
            else if (c == 'p')
            {
                loadPlays();
            }
            /*else if (c == 'o')
            {
                LearningInterpreter li = interpreter as LearningInterpreter;
                if (li != null)
                {
                    MessageBox.Show(li.getResults());
                }
            }*/
            else if (c == 't')
            {
                PAB.HiPerfTimer timer = new global::InterpreterTester.PAB.HiPerfTimer();
                timer.Start();
                run(testIterations);
                timer.Stop();
                this.Invalidate();
                MessageBox.Show("an average of " + timer.Duration * 1000 / testIterations + " ms per iteration");
            }
        }

        int numrunning = 0;
        private void synchronousRun(int speed)
        {
            numrunning++;
            if (numrunning > 1)
            {
                numrunning--;
                return;
            }
            run(speed);
            this.Invalidate();
            numrunning--;
        }
        private void InterpreterTester_MouseClick(object sender, MouseEventArgs e)
        {
            /*if (e.Button == MouseButtons.Left)
            {
                RobotInfo prev = ourinfo[0];
                ourinfo[0] = new RobotInfo(pixeltofieldPoint((Vector2)e.Location), prev.Orientation, 0);
                this.Invalidate();
            }
            else if (e.Button == MouseButtons.Right)
            {
                RobotInfo prev = theirinfo[0];
                theirinfo[0] = new RobotInfo(pixeltofieldPoint((Vector2)e.Location), prev.Orientation, TEAMSIZE);
                this.Invalidate();
            }
            else */
            if (e.Button == MouseButtons.Middle)
            {
                ballvx = ballvy = 0;
                ballinfo = new BallInfo(pixeltofieldPoint((Vector2)e.Location), 0, 0);
                this.Invalidate();
            }
        }
        #endregion

        private const float chop = .001f;

        #region Commander Members
        public void move(int robotID, bool avoidBall, Vector2 dest)
        {
            RobotInfo[] infos = ourinfo;
            int newid = robotID;
            int change = 1;
            if (robotID >= TEAMSIZE)
            {
                change = -1;
                infos = theirinfo;
                newid -= TEAMSIZE;
            }
            RobotInfo r = infos[newid];
            move(robotID, avoidBall, dest, (float)Math.Atan2(ballinfo.Position.Y - r.Position.Y, change * (ballinfo.Position.X - r.Position.X)));
        }
        const float distThreshold = .005f;
        //Navigation.Current.CurrentNavigator navigator = new Navigation.Current.CurrentNavigator(),
        //    othernavigator = new Navigation.Current.CurrentNavigator();
        Navigator navigator = new Navigator(),
            othernavigator = new Navigator();
        public void move(int robotID, bool avoidBall, Vector2 destination, float orientation)
        {
            //Graphics g = this.CreateGraphics();

            //g.FillRectangle(new SolidBrush(Color.Black), 0, 0, 300, 300);

            RobotInfo[] infos = ourinfo;
            RobotInfo[] otherinfo = theirinfo;
            Navigator n = navigator;
            int navigatorId = robotID;
            //float mult=1f;
            if (robotID >= TEAMSIZE)
            {
                infos = theirinfo;
                otherinfo = ourinfo;
                robotID -= TEAMSIZE;
                n = othernavigator;
                //mult=-1;
                //orientation = (float)(Math.PI - orientation);
            }
            //Vector2 destination = new Vector2(x, y); // changed 6/9/07 to use Vector2 (jie)
            Vector2 ballposition = ballinfo.Position;
            Vector2 position = infos[robotID].Position;
            if (position.distanceSq(destination) <= distThreshold * distThreshold)
                return;
            double ballavoidance = 0;
            if (avoidBall)
                ballavoidance = (float)Math.Max(1, Math.Min(1.7, (1 + 10 * Math.Sqrt(ballinfo.dX * ballinfo.dX + ballinfo.dY * ballinfo.dY)) * (2.40 - 1.5 * ((destination - ballposition).normalize() * (position - ballposition).normalize()))));
            //PAB.HiPerfTimer timer = new global::InterpreterTester.PAB.HiPerfTimer();
            //timer.Start();
            /*Vector2 result = n.navigate(robotID, position, destination,
                ourteam, theirteam, ballposition,
                ballavoidance
                , 2.1f,
             (float)Math.Max(.00006, Math.Min(.06, Math.Sqrt(position.distanceSq(destination) * .25f)))
            );*/
            Vector2 result = n.navigate(navigatorId, position, destination, infos, otherinfo, ballinfo, .12f);
            //timer.Stop();
            //Console.WriteLine(timer.Duration*1000+" ms for navigation");
            //Console.WriteLine(navigator.treecount + " nodes, and " + navigator.obstacleStuckCount + " stuck");

            //if (infos.Length == 2 && robotID == 0)
            /* if (navigator.obstacleStuckCount>=2000)
             {
                 Brush b = new SolidBrush(Color.Black);
                 Graphics g = this.CreateGraphics();
                 for (int i = 0; i < navigator.treecount; i++)
                 {
                     g.FillRectangle(b, fieldtopixelX( navigator.rrttree[i].x) - 2,fieldtopixelY( navigator.rrttree[i].y) - 2, 4, 4);
                 }
                 position = position;
             }*/


            RobotInfo prev = infos[robotID];
            //addArrow(new Arrow(fieldtopixelPoint(infos[robotID].Position), fieldtopixelPoint(new Vector2(x, y)), Color.Blue, 4.0f));
            //infos[robotID] = new RobotInfo(translate(prev.Position, normalize((new Vector2(x, y)) - prev.Position), .01f), (prev.Orientation * .9f + orientation * .1f), prev.ID);
            if (position.distanceSq(destination) > chop * chop)
            {
                addArrow(new Arrow(fieldtopixelPoint(position), fieldtopixelPoint(result), Color.Green, 3.0f));
                addArrow(new Arrow(fieldtopixelPoint(position), fieldtopixelPoint(destination), Color.Red, 3.0f));
                if (prev.Position != result)
                {
                    infos[robotID] = new RobotInfo(prev.Position + speed * (result - prev.Position).normalize(), (prev.Orientation * .85f + orientation * .15f), prev.ID);
                }
            }
            //Console.WriteLine("going from " + prev.Position + " to " + result);
            //Console.WriteLine(prev.Position + .01f * (result - prev.Position).normalize());
            //infos[robotID] = new RobotInfo(translate(prev.Position, normalize(result - prev.Position), .01f), (prev.Orientation * .9f + orientation * .1f), prev.ID);
        }

        public void kick(int robotID)
        {
            RobotInfo[] infos = ourinfo;
            if (robotID >= TEAMSIZE)
            {
                infos = theirinfo;
                robotID -= TEAMSIZE;
            }
            const float randomComponent = ballspeed / 3;
            ballvx = (float)(ballspeed * Math.Cos(infos[robotID].Orientation));
            ballvy = (float)(ballspeed * Math.Sin(infos[robotID].Orientation));
            ballvx += (float)(r.NextDouble() * 2 - 1) * randomComponent;
            ballvy += (float)(r.NextDouble() * 2 - 1) * randomComponent;
            RobotInfo prev = infos[robotID];
            float recoil = 1.5f;
            infos[robotID] = new RobotInfo(prev.Position + (new Vector2(-ballvx * recoil, -ballvy * recoil)), prev.Orientation, prev.ID);
            //throw new Exception("The method or operation is not implemented.");
        }

        public void stop(int robotID)
        {
            //throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        private void InterpreterTester_FormClosing(object sender, FormClosingEventArgs e)
        {
#if MULTITHREAD
            workerThread.Abort();
#endif
        }

        /// <summary>
        /// Runs the game for a set number of steps, then returns how well we did
        /// (higher number = better).  The result it returns will be scaled for the number of steps;
        /// ie if you double the number of steps we won't immediately do twice as good/bad.
        /// </summary>
        /// <param name="steps">The number of steps to take.</param>
        /// <param name="stddev">Returns here a measure of confidence in the answer; the probability of
        /// any score being the "actual" score roughly follows a normal distribution with mean the return
        /// value, and stddev this value.</param>
        public double score(int steps, int speed, out double stddev)
        {
            init();
            int stepsLeft = steps;
            while (stepsLeft > 0)
            {
                synchronousRun(Math.Min(stepsLeft, speed));
                stepsLeft -= speed;
            }
            stddev = Math.Sqrt(ourgoals + theirgoals + 1) * 10000d / steps;
            return (double)(ourgoals - theirgoals) * 10000d / steps;
        }


        #region IPredictor Members

        public RobotInfo getCurrentInformation(int robotID)
        {
            foreach (RobotInfo info in ourinfo)
            {
                if (info.ID == robotID)
                    return info;
            }
            foreach (RobotInfo info in theirinfo)
            {
                if (info.ID == robotID)
                    return info;
            }
            throw new ApplicationException("could not find robot id " + robotID);
        }

        public List<RobotInfo> getOurTeamInfo()
        {
            return new List<RobotInfo>(ourinfo);
        }

        public List<RobotInfo> getTheirTeamInfo()
        {
            return new List<RobotInfo>(theirinfo);
        }

        public BallInfo getBallInfo()
        {
            return ballinfo;
        }

        #endregion
    }
}
