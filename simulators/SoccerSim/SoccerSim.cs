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
        SimSystem _player1;
        SimSystem _player2;
        SimEngine _engine;
        VirtualRef referee;
        ICoordinateConverter converter = new Robocup.Utilities.BasicCoordinateConverter(650, 30, 50);

        private void init()
        {
            referee = new SimpleReferee();
            PhysicsEngine physics_engine = new PhysicsEngine(referee);
            _fieldView = new FieldDrawer(physics_engine, converter);
            // TODO make configurable how many to load

            //RefBoxListener refbox = new RefBoxListener(10001);

            _player1 = new SimSystem(_fieldView, physics_engine, referee, true);
            _player2 = new SimSystem(_fieldView, physics_engine, referee, false);
            _engine = new SimEngine(physics_engine, this);
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
        bool berunning = false;

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
                MessageBox.Show(sb.ToString());
            }
            else if (c == 's')
            {
                //savePlays();
            }
            else if (c == 'a')
            {
                drawArrows = !drawArrows;
                this.Invalidate();
            }
            else if (c == 'r')
            {

                berunning = !berunning;
                if (berunning)
                {
                    _player1.start();
                    _player2.start();
                    _engine.start();
                }
                else
                {
                    _engine.stop();
                    _player2.stop();
                    _player1.stop();
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
        private void SoccerSim_MouseClick(object sender, MouseEventArgs e)
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
            /*if (e.Button == MouseButtons.Middle)
            {
                state.BallVx = ballvy = 0;
                ballinfo = new BallInfo(pixeltofieldPoint((Vector2)e.Location), 0, 0);
                this.Invalidate();
            }*/
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