#define DIFFERENTPLAYS

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

using Robocup.CoreRobotics;
using Robocup.Plays;
using Robocup.Core;
using Navigator = Navigation.Examples.DumbNavigator;
using Robocup.Simulation;

namespace SoccerSim
{
    public partial class SoccerSim : Form
    {

        public SoccerSim()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            InitializeComponent();

            init();
        }

        FieldDrawer _fieldView;
        PhysicsEngine _physics_engine;
        SimSystem _player1;
        SimSystem _player2;
        SimEngine _engine;
        SimVision _vision;
        VirtualRef referee;
        ICoordinateConverter converter = new Robocup.Utilities.BasicCoordinateConverter(650, 30, 50);

        

        private void init()
        {
            referee = new SimpleReferee();
            _physics_engine = new PhysicsEngine(referee);
            _fieldView = new FieldDrawer(_physics_engine, converter);
            // TODO make configurable how many to load

            //RefBoxListener refbox = new RefBoxListener(10001);

            _player1 = new SimSystem(_fieldView, _physics_engine, referee, true);
            _player2 = new SimSystem(_fieldView, _physics_engine, referee, false);
            _engine = new SimEngine(_physics_engine, this);

            _vision = new SimVision(_physics_engine, this);

            InitDragAndDrop();
        }




        bool drawArrows = false;

        private void SoccerSim_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            _fieldView.paintField(g);
            if (drawArrows)
            {
                _player1.Controller.drawCurrent(g, converter);
                _player2.Controller.drawCurrent(g, converter);
                //_fieldView.paintArrows(g);
            }
        }

        #region User Input

        private DragAndDropper draganddrop = new DragAndDropper();
        void InitDragAndDrop()
        {
            draganddrop.AddDragandDrop(delegate() { return _physics_engine.getBallInfo().Position; }, .05, delegate(Vector2 v) { _physics_engine.MoveBall(v); });
            foreach (RobotInfo info in _physics_engine.getAllInfos())
            {
                int id = info.ID;
                draganddrop.AddDragandDrop(delegate() { return _physics_engine.getCurrentInformation(id).Position; }, .1,
                    delegate(Vector2 v)
                    {
                        RobotInfo inf = _physics_engine.getCurrentInformation(id);
                        _physics_engine.MoveRobot(id, new RobotInfo(v, inf.Orientation, id));
                    }
                );
            }
        }
        
        

        bool berunning = false;
        bool visionRunning = false;

        private void SoccerSim_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = char.ToLower(e.KeyChar);
            if (c >= '1' && c <= '9')
            {
                int numSteps = c - '1' + 1;
                Console.WriteLine("running " + numSteps + " rounds");
                for (int i = 0; i < numSteps; i++)
                {
                    _player1.runRound();
                    _player2.runRound();
                    _engine.step(1.0/30);
                }
                this.Invalidate();
            }
            else if (c == 'h')
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("KEYBOARD COMMANDS");
                sb.AppendLine("h  \t show this help box");
                sb.AppendLine("1-9\t advance that many frames");
                sb.AppendLine("r  \t sets it to run continuously");
                sb.AppendLine("a  \t toggles arrow drawing");
                sb.AppendLine("p  \t reloads all the plays");
                sb.AppendLine("v  \t starts vision service running on localhost");
                MessageBox.Show(sb.ToString());
            }
            else if (c == 's')
            {
                //savePlays();
            }
            else if (c == 'a')
            {
                drawArrows = !drawArrows;
                if (drawArrows)
                {
                    arrowStatus.BackColor = Color.Green;
                    arrowStatus.Text = "Arrows";
                }
                else
                {
                    arrowStatus.BackColor = Color.Red;
                    arrowStatus.Text = "No Arrows";
                }
                this.Invalidate();
            }
            else if (c == 'r')
            {

                berunning = !berunning;
                if (berunning)
                {
                    runningStatus.BackColor = Color.Green;
                    runningStatus.Text = "Running";
                    _player1.start();
                    _player2.start();
                    _engine.start();
                }
                else
                {
                    _engine.stop();
                    _player2.stop();
                    _player1.stop();
                    runningStatus.BackColor = Color.Red;
                    runningStatus.Text = "Not Running";
                }

            }
            else if (c == 'q')
            {
                _physics_engine.resetPositions();
            }
            else if (c == 'v')
            {

                visionRunning = !visionRunning;
                if (visionRunning)
                {
                    visionStatus.BackColor = Color.Green;
                    visionStatus.Text = "Vision";
                    _vision.start();
                }
                else
                {
                    visionStatus.BackColor = Color.Red;
                    visionStatus.Text = "No Vision";
                    _vision.stop();
                }

            }
            else if (c == 'p')
            {
                //loadPlays();
            }
        }

        /*int numrunning = 0;
        private void synchronousRun(int speed)
        {
            numrunning++;
            if (numrunning > 1)
            {
                numrunning--;
                return;
            }
            _engine.run(speed);
            this.Invalidate();
            numrunning--;
        }*/

        private void SoccerSim_MouseDown(object sender, MouseEventArgs e)
        {
            draganddrop.MouseDown(converter.pixeltofieldPoint((Vector2)e.Location));
        }
        private void SoccerSim_MouseMove(object sender, MouseEventArgs e)
        {
            bool moved = draganddrop.MouseMove(converter.pixeltofieldPoint((Vector2)e.Location));
            if (moved) this.Invalidate();
        }

        private void SoccerSim_MouseUp(object sender, MouseEventArgs e)
        {
            draganddrop.MouseUp();
        }
        
        #endregion

        private void SoccerSim_FormClosing(object sender, FormClosingEventArgs e)
        {
            _player1.stop();
            _player2.stop();
            _engine.stop();
        }




    }
}