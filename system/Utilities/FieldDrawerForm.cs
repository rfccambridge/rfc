using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Robocup.Core;
using System.Drawing;

namespace Robocup.Utilities
{
    partial class FieldDrawerForm : Form
    {
        private delegate void VoidDelegate();

        private FieldDrawer _fieldDrawer;
        bool _glFieldLoaded = false;

        public FieldDrawerForm(FieldDrawer fieldDrawer, double heightToWidth)
        {
            _fieldDrawer = fieldDrawer;
            InitializeComponent();

            this.Width = (int)((double)glField.Height / heightToWidth);
        }

        public void InvalidateGLControl()
        {
            glField.Invalidate();
        }

        public void UpdateTeam(Team team)
        {
            this.Invoke(new VoidDelegate(delegate
            {
                lblTeam.Text = team.ToString();
                lblTeam.ForeColor = team == Team.Yellow ? Color.Yellow : Color.Blue;
            }));
        }

        public void UpdateRefBoxCmd(string refBoxCmd)
        {
            this.Invoke(new VoidDelegate(delegate
            {
                lblRefBoxCmd.Text = refBoxCmd;
            }));
        }

        public void UpdatePlayType(PlayType playType)
        {
            this.Invoke(new VoidDelegate(delegate
            {
                lblPlayType.Text = playType.ToString();
            }));
        }

        public void UpdateInterpretFreq(double freq)
        {
            this.Invoke(new VoidDelegate(delegate
            {
                lblInterpretFreq.Text = String.Format("{0:F2} Hz", freq);
            }));
        }

        public void UpdateInterpretDuration(double duration)
        {
            this.Invoke(new VoidDelegate(delegate
            {
                lblInterpretDuration.Text = String.Format("{0:F2} ms", duration);
            }));
        }

        private void glField_Paint(object sender, PaintEventArgs e)
        {
            if (!_glFieldLoaded)
                return;
            glField.MakeCurrent();
            _fieldDrawer.Paint();            
            glField.SwapBuffers();
        }

        private void glField_Load(object sender, EventArgs e)
        {
            _glFieldLoaded = true;
            _fieldDrawer.Init(glField.Width, glField.Height);
        }

        private void glField_Resize(object sender, EventArgs e)
        {
            _fieldDrawer.Resize(glField.Width, glField.Height);
            glField.Invalidate();
        }

        private void FieldDrawerForm_Resize(object sender, EventArgs e)
        {
            glField.Height = panGameStatus.Top;
        }
    }
}