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
using RobocupPlays;
using Robocup.Infrastructure;
using Navigator = Navigation.Examples.DumbNavigator;

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

        FieldState _state;
        FieldView _fieldView;
        SimSystem _player1;
        SimSystem _player2;
        SimEngine _engine;

        private void init()
        {
            // init score
            _state = new FieldState();
            _fieldView = new FieldView(_state);
            // TODO make configurable how many to load

            RefBoxListener refbox = new RefBoxListener(10001);

            _player1 = new SimSystem(_fieldView,_state,refbox,true);
            _player2 = new SimSystem(_fieldView,_state,refbox,false);
            _engine = new SimEngine(_state, this);
        }

        


        bool drawArrows = false;

        private void SoccerSim_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            _fieldView.paintField(g);
            if (drawArrows)
            {
                _fieldView.paintArrows(g);
            }
        }

        #region User Input
        bool berunning = false;
        System.Threading.Timer t = null;
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
                    _engine.step();
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
                sb.AppendLine("R  \t same, but skips frames");
                sb.AppendLine("a  \t toggles arrow drawing");
                sb.AppendLine("p  \t reloads all the plays");
                sb.AppendLine("s  \t saves all the plays");
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
                int atATime = 1;
                if (char.IsUpper(e.KeyChar))
                    atATime = 10;
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

        

        
    }
}