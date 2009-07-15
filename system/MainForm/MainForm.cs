using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Robocup.Utilities;
using Robocup.CoreRobotics;
using WeifenLuo.WinFormsUI.Docking;
using Robocup.Plays;
using Robocup.ControlForm;
using System.IO;

namespace Robocup.Core
{
    public partial class MainForm : Form
    {

        const string DOCKPANEL_LAYOUT_FILE = "../../resources/gui/dockpanel_layout.xml";

        private FieldDrawerForm _fieldDrawerForm;
        public FieldDrawerForm FieldDrawerForm {
            get { return _fieldDrawerForm; }
            set { _fieldDrawerForm = value; }
        }

        private ControlForm _controlForm;
        public ControlForm ControlForm {     
            get { return _controlForm; }
            set { _controlForm = value; }
        }

        private PlaySelectorForm _playSelectorForm;
        public PlaySelectorForm PlaySelectorForm {     
            get { return _playSelectorForm; }
            set { _playSelectorForm = value; }
        }

        private PIDForm _pidForm;
        public PIDForm PIDForm { 
            get { return _pidForm; }
            set { _pidForm = value; }
        }

        public MainForm()
        {
            InitializeComponent();

            this.WindowState = FormWindowState.Maximized;

            dockPanel.Top = 0;
            dockPanel.Left = 0;

                this.FieldDrawerForm = new FieldDrawerForm(this);
                this.PIDForm = new PIDForm(this);
                this.PlaySelectorForm = new PlaySelectorForm(this);
                this.ControlForm = new ControlForm(this);


            if (File.Exists(DOCKPANEL_LAYOUT_FILE))
            {
                dockPanel.LoadFromXml(DOCKPANEL_LAYOUT_FILE, new DeserializeDockContent(GetContentFromPersistString));
            }
            else
            {
                this.FieldDrawerForm.Show(dockPanel, DockState.Document);
                this.PIDForm.Show(dockPanel, DockState.DockRight);
                this.PlaySelectorForm.Show(dockPanel, DockState.DockBottom);
                this.ControlForm.Show(dockPanel, DockState.DockBottom);
            }

            this.FieldDrawerForm.TabText = this.FieldDrawerForm.Text;
        }

        private IDockContent GetContentFromPersistString(string str)
        {
            if (str == typeof(PIDForm).ToString())
                return _pidForm;
            else if (str == typeof(PlaySelectorForm).ToString())
                return _playSelectorForm;
            else if (str == typeof(ControlForm).ToString())
                return _controlForm;
            else
            {
                // DummyDoc overrides Getstr to add extra information into str.
                // Any DockContent may override this value to add any needed information for deserialization              

                return _fieldDrawerForm;
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            dockPanel.Width = this.Width - 5;
            dockPanel.Height = this.Height - statusStrip1.Height - 25;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            dockPanel.SaveAsXml(DOCKPANEL_LAYOUT_FILE);
        }


    }
}