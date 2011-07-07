#define DIFFERENTPLAYS
//#define TIMING


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

using Robocup.Plays;
using Robocup.Utilities;

using Robocup.Core;
using Robocup.Geometry;

//using Navigator = Navigation.Examples.LookAheadPotential;
//using Navigator = Navigation.Examples.DumbNavigator;
using Navigator = Navigation.Current.CurrentNavigator;

namespace InterpreterTester
{
    public partial class InterpreterTester : Form, IController, IPredictor, ICoordinateConverter
    {


        Interpreter interpreter, defensiveinterpreter;
        const double ballspeed = .08, balldecay = .98;
        private const double ballbounce = .01, collisionradius = .12, speed = 0.02;
        // This is an approximation; assuming that the ball travels at max 10m/s, here it travels at
        // ballspeed m/tick, so ms/tick = 1000 ms/s * (ballspeed m/tick) / (10m/s)
        private const double ms_per_tick = 1000 * ballspeed / 10;

        int ourgoals = 0, theirgoals = 0;
        const int TEAMSIZE = 5;

        RobotInfo[] ourinfo;

        RobotInfo[] theirinfo;

        Team OUR_TEAM = Team.Yellow;
        Team THEIR_TEAM = Team.Blue;

        FieldHalf OUR_FIELD_HALF = FieldHalf.Left;
        FieldHalf THEIR_FIELD_HALF = FieldHalf.Right;

        Dictionary<Team, List<RobotInfo>> _robots = new Dictionary<Team, List<RobotInfo>>();

        BallInfo ballinfo = new BallInfo(new Vector2(0, 0));
        double ballvx = 0, ballvy = 0;

        int ballImmobile = 0;

        private const double chop = .001;

        const int testIterations = 1000;

        string OFFENSE_PLAY_DIR;
        string DEFENSE_PLAY_DIR;

        Random r = new Random();

        private PlayType playType = PlayType.NormalPlay;        

        PlayManager play_manager;

        public InterpreterTester()
        {
            foreach (Team team in Enum.GetValues(typeof(Team)))
                _robots.Add(team, new List<RobotInfo>());            

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            InitializeComponent();            

            init();
        }
        private void init()
        {
            ourgoals = theirgoals = 0;
            ballImmobile = 0;
            /*ourinfo = new RobotInfo[] {
                new RobotInfo(new Vector2(-1.0, -1), 3, 0),
                new RobotInfo(new Vector2(-1.0, 0), 3, 1),
                new RobotInfo(new Vector2(-1.0, 1), 3, 2),
                new RobotInfo(new Vector2(-2, -1), 3, 3),
                new RobotInfo(new Vector2(-2, 1), 3, 4)
            };
            theirinfo = new RobotInfo[] {
                new RobotInfo(new Vector2(1.0, -1), 3, TEAMSIZE+0),
                new RobotInfo(new Vector2(1.0, 0), 3, TEAMSIZE+1),
                new RobotInfo(new Vector2(1.0, 1), 3, TEAMSIZE+2),
                new RobotInfo(new Vector2(2, -1), 3, TEAMSIZE+3),
                new RobotInfo(new Vector2(2, 1), 3, TEAMSIZE+4)
            };*/
            ourinfo = new RobotInfo[] {
                new RobotInfo(new Vector2(-2, 0), 1, 0),
                new RobotInfo(new Vector2(-1, 0), 1, 1)
            };
            theirinfo = new RobotInfo[] {
                new RobotInfo(new Vector2(1, 0), 3, TEAMSIZE+0),
                new RobotInfo(new Vector2(2, 0), 3, TEAMSIZE+1),
            };
            ballinfo = new BallInfo(new Vector2(0, 0));
            ballvx = ballvy = 0;

            LoadConstants();
            loadPlays();
        }

        public void LoadConstants()
        {
            OFFENSE_PLAY_DIR = ConstantsRaw.get<string>("default", "INTERPRETER_TESTER_OFFENSE_PLAY_DIR");
            DEFENSE_PLAY_DIR = ConstantsRaw.get<string>("default", "INTERPRETER_TESTER_DEFENSE_PLAY_DIR");
        }

        private void loadPlays()
        {
            if (play_manager != null)
                play_manager.Close();

            Dictionary<string, InterpreterTactic> tacticBook = PlayUtils.loadTactics(Constants.PlayFiles.TACTIC_DIR);

            // Player 1 plays
            Dictionary<InterpreterPlay, string> offensePlays = PlayUtils.loadPlays(OFFENSE_PLAY_DIR, tacticBook);
            interpreter = new Interpreter(OUR_TEAM, this, this, null);
            interpreter.LoadPlays(new List<InterpreterPlay>(offensePlays.Keys));

            // Player 2 plays
            Dictionary<InterpreterPlay, string> defensePlays = PlayUtils.loadPlays(DEFENSE_PLAY_DIR, tacticBook);
            // TODO: Part of fixing InterpreterTester: Flipping should not be necessary, although that depends on the implementation fo this  predictor
            defensiveinterpreter = new Interpreter(THEIR_TEAM, this, this, null);
            defensiveinterpreter.LoadPlays(new List<InterpreterPlay>(defensePlays.Keys));

            // Play manager stuff
            play_manager = new PlayManager(interpreter, defensiveinterpreter, offensePlays, defensePlays);
            play_manager.Show();
        }

  
        #region Coordinate Conversions
        public int fieldtopixelX(double x)
        {
            return (int)(300 + 100 * x);
        }
        public int fieldtopixelY(double y)
        {
            return (int)(200 - 100 * y);
        }
        public Vector2 fieldtopixelPoint(Vector2 p)
        {
            return new Vector2(fieldtopixelX(p.X), fieldtopixelY(p.Y));
        }
        public double pixeltofieldX(double x)
        {
            return ((x - 300) / 100);
        }
        public double pixeltofieldY(double y)
        {
            return ((y - 200) / -100);
        }
        public Vector2 pixeltofieldPoint(Vector2 p)
        {
            return new Vector2(pixeltofieldX(p.X), pixeltofieldY(p.Y));
        }
        public double fieldtopixelDistance(double d)
        {
            return d * 100;
        }
        public double pixeltofieldDistance(double d)
        {
            return d * .01;
        }
        #endregion

        #region Drawing Commands
        private void drawRobot(RobotInfo r, Graphics g, Color c)
        {
            Brush b = new SolidBrush(c);
            Vector2 center = fieldtopixelPoint(r.Position);
            g.FillEllipse(b, (float)center.X - 10, (float)center.Y - 10, 20, 20);
            PointF[] corners = new PointF[4];
            double angle = -r.Orientation;
            double outerangle = .6;
            double innerangle = 1.0;
            double innerradius = 7;
            double outerradius = 11;
            corners[0] = (center + (new Vector2(innerradius * Math.Cos(angle + innerangle), innerradius * Math.Sin(angle + innerangle)))).ToPointF();
            corners[1] = (center + (new Vector2(innerradius * Math.Cos(angle - innerangle), innerradius * Math.Sin(angle - innerangle)))).ToPointF();
            corners[2] = (center + (new Vector2(outerradius * Math.Cos(angle - outerangle), outerradius * Math.Sin(angle - outerangle)))).ToPointF();
            corners[3] = (center + (new Vector2(outerradius * Math.Cos(angle + outerangle), outerradius * Math.Sin(angle + outerangle)))).ToPointF();
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
            foreach (RobotInfo r in GetRobots(OUR_TEAM))
            {
                drawRobot(r, g, Color.Black);
                //g.FillEllipse(b, fieldtopixelX(r.Position.X) - 10, fieldtopixelY(r.Position.Y) - 10, 20, 20);
            }
            b.Dispose();
            b = new SolidBrush(Color.Red);
            foreach (RobotInfo r in GetRobots(THEIR_TEAM))
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
                TagSystem.GetTags(r.ID).Clear();
                if (r.ID == 0)
                    TagSystem.AddTag(r.ID, "goalie");
            }
#if TIMING
            timer.Stop();
            Console.WriteLine(timer.Duration * 1000 + " ms for getting ready to interpreter.interpret()");
            timer.Start();
#endif
            Score score = new Score();
            score.SetScore(OUR_TEAM, ourgoals);
            score.SetScore(THEIR_TEAM, theirgoals);
            interpreter.interpret(playType, score);
            defensiveinterpreter.interpret(playType, score);
#if TIMING
            timer.Stop();
            Console.WriteLine(timer.Duration * 1000 + " ms for calling interpreter.interpret()");
#endif
        }
        private void goalScored(bool scoredByLeftTeam)
        {
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
                    newballlocation = location + .13 * (ballinfo.Position - location).normalize();
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
                        newballlocation = location + .13 * (ballinfo.Position - location).normalize();
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
                ballinfo = new BallInfo(new Vector2(0, 0));
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
                        Vector2 t1 = p1 + .01 * (p1 - p2).normalize();
                        Vector2 t2 = p2 + .01 * (p2 - p1).normalize();
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

            ballinfo = new BallInfo(newballlocation, (1 / (ms_per_tick * 1000)) * (new Vector2(ballvx, ballvy)));
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
                if (ballImmobile > 1000)
                {
                    ballinfo = new BallInfo(new Vector2(0, 0));
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
                HighResTimer timer = new HighResTimer();
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
                ballinfo = new BallInfo(pixeltofieldPoint((Vector2)e.Location));
                this.Invalidate();
            }
        }
        #endregion        

        #region Commander Members
        public void Move(RobotInfo dest, bool avoidBall)
        {
            Move(dest.ID, avoidBall, dest.Position);
        }
        
        public void Move(int robotID, bool avoidBall, Vector2 dest)
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
            Move(robotID, avoidBall, dest, (double)Math.Atan2(ballinfo.Position.Y - r.Position.Y, change * (ballinfo.Position.X - r.Position.X)));
        }

        public void Move(RobotInfo info, bool avoidBall, double speedScale)
        {
            throw new NotImplementedException();
        }


        public void Move(int robotID, bool avoidBall, Vector2 destination, double orientation, double speedScale)
        {
            throw new NotImplementedException();
        }

        const double distThreshold = .005;
        //Navigation.Current.CurrentNavigator navigator = new Navigation.Current.CurrentNavigator(),
        //    othernavigator = new Navigation.Current.CurrentNavigator();
        Navigator navigator = new Navigator(),
            othernavigator = new Navigator();
        public void Move(int robotID, bool avoidBall, Vector2 destination, double orientation)
        {
            //Graphics g = this.CreateGraphics();

            //g.FillRectangle(new SolidBrush(Color.Black), 0, 0, 300, 300);

            RobotInfo[] infos = ourinfo;
            RobotInfo[] otherinfo = theirinfo;
            Navigator n = navigator;
            int navigatorId = robotID;
            //double mult=1;
            if (robotID >= TEAMSIZE)
            {
                infos = theirinfo;
                otherinfo = ourinfo;
                robotID -= TEAMSIZE;
                n = othernavigator;
                //mult=-1;
                //orientation = Math.PI - orientation;
            }
            //Vector2 destination = new Vector2(x, y); // changed 6/9/07 to use Vector2 (jie)
            Vector2 ballposition = ballinfo.Position;
            Vector2 position = infos[robotID].Position;
            if (position.distanceSq(destination) <= distThreshold * distThreshold)
                return;
            double ballavoidance = 0;
            if (avoidBall)
                ballavoidance = (double)Math.Max(1, Math.Min(1.7, (1 + Math.Sqrt(ballinfo.Velocity.magnitudeSq())) * (2.40 - 1.5 * ((destination - ballposition).normalize() * (position - ballposition).normalize()))));
            //PAB.HiPerfTimer timer = new global::InterpreterTester.PAB.HiPerfTimer();
            //timer.Start();
            /*Vector2 result = n.navigate(robotID, position, destination,
                ourteam, theirteam, ballposition,
                ballavoidance
                , 2.1,
             (double)Math.Max(.00006, Math.Min(.06, Math.Sqrt(position.distanceSq(destination) * .25)))
            );*/
            NavigationResults result = n.navigate(navigatorId, position, destination, infos, otherinfo, ballinfo, .12);
            Vector2 waypoint = result.waypoint;
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
            //addArrow(new Arrow(fieldtopixelPoint(infos[robotID].Position), fieldtopixelPoint(new Vector2(x, y)), Color.Blue, 4.0));
            //infos[robotID] = new RobotInfo(translate(prev.Position, normalize((new Vector2(x, y)) - prev.Position), .01), (prev.Orientation * .9 + orientation * .1), prev.ID);
            if (position.distanceSq(destination) > chop * chop)
            {
                addArrow(new Arrow(fieldtopixelPoint(position), fieldtopixelPoint(waypoint), Color.Green, 3.0));
                addArrow(new Arrow(fieldtopixelPoint(position), fieldtopixelPoint(destination), Color.Red, 3.0));
                //double neworientation = prev.Orientation * .85 + orientation * .15;
                double neworientation = orientation;
                if (prev.Position != waypoint)
                {
                    infos[robotID] = new RobotInfo(prev.Position + speed * (waypoint - prev.Position).normalize(), neworientation, prev.ID);
                }
            }
            //Console.WriteLine("going from " + prev.Position + " to " + result);
            //Console.WriteLine(prev.Position + .01 * (result - prev.Position).normalize());
            //infos[robotID] = new RobotInfo(translate(prev.Position, normalize(result - prev.Position), .01), (prev.Orientation * .9 + orientation * .1), prev.ID);
        }

        public void Kick(int robotID)
        {
            RobotInfo[] infos = ourinfo;
            if (robotID >= TEAMSIZE)
            {
                infos = theirinfo;
                robotID -= TEAMSIZE;
            }
            const double randomComponent = ballspeed / 6000;
            ballvx = ballspeed * Math.Cos(infos[robotID].Orientation);
            ballvy = ballspeed * Math.Sin(infos[robotID].Orientation);
            ballvx += (r.NextDouble() * 2 - 1) * randomComponent;
            ballvy += (r.NextDouble() * 2 - 1) * randomComponent;
            RobotInfo prev = infos[robotID];
            double recoil = 1.5;
            infos[robotID] = new RobotInfo(prev.Position + (new Vector2(-ballvx * recoil, -ballvy * recoil)), prev.Orientation, prev.ID);
            //throw new Exception("The method or operation is not implemented.");
        }
        public void Charge(int robotID) {
            throw new NotImplementedException("InterpereterTester: charge() not implemented.");
        }
        public void Charge(int robotID, int strength)
        {
            throw new NotImplementedException("InterpereterTester: charge() not implemented.");
        }
        public void beamKick(int robotID, bool somethign) {
            throw new NotImplementedException("InterpereterTester: beamKick() not implemented.");
        }
        public void BreakBeam(int robotId, int strength)
        {
            throw new NotImplementedException("InterpereterTester: BreakBream() not implemented.");
        }
        public void StartDribbling(int robotID)
        {
            throw new NotImplementedException("InterpreterTester: StartDribbling() not implemented.");
        }
        public void StopDribbling(int robotID)
        {
            throw new NotImplementedException("InterpreterTester: StopDribbling() not implemented.");
        }
        public void Stop(int robotID)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }
        public void moveKick(int i, Vector2 pt)
        {
            throw new NotImplementedException("not implemented");
        }
        public void StartControlling()
        {
        }
        
        public void StopControlling()
		{
			
		}
        public void Kick(int robotID, Vector2 target)
        {
            throw new NotImplementedException("Not implemented!");
        }
        public void Connect(string host, int port)
        {
            throw new NotImplementedException("Not implemented!");
        }
        public void Disconnect()
        {
            throw new NotImplementedException("Not implemented!");
        }

        #endregion

        private void InterpreterTester_FormClosing(object sender, FormClosingEventArgs e)
        {
            play_manager.Close();
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
            return (ourgoals - theirgoals) * 10000d / steps;
        }


        #region IPredictor Members

        public List<RobotInfo> GetRobots(Team team)
        {
            return _robots[team];
        }
        public List<RobotInfo> GetRobots() {
            List<RobotInfo> combined = new List<RobotInfo>();
            foreach (Team team in Enum.GetValues(typeof(Team)))
                combined.AddRange(_robots[team]);
            return combined;
        }
        public RobotInfo GetRobot(Team team, int id)
        {
            foreach (RobotInfo robot in _robots[team])
                if (robot.ID == id)
                    return robot;
            return null;
        }
        public BallInfo GetBall()
        {
            return ballinfo;
        }

        public void SetBallMark()
        {
            throw new ApplicationException("InterpreterTester.setBallMark: not implemented");
        }
        public void ClearBallMark()
        {
            throw new ApplicationException("InterpreterTester.clearBallMark: not implemented");
        }
        public bool HasBallMoved()
        {
            throw new ApplicationException("InterpreterTester.hasBallMoved: not implemented");
        }
        public void SetPlayType(PlayType newPlayType)
        {
            throw new ApplicationException("InterpreterTester.setPlayType: not implemented");
        }
  

        #endregion     
    }
}
 
