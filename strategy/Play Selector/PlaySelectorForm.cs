using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Robocup.Plays
{
    public partial class PlaySelectorForm : Form
    {
        public PlaySelectorForm()
        {
            InitializeComponent();           
        }

        public void LoadPlays(List<InterpreterPlay> plays) {
            this.checkedListBox1.Items.Clear();
            foreach (InterpreterPlay play in plays)
                this.checkedListBox1.Items.Add(play);

            for (int i = 0; i < this.checkedListBox1.Items.Count; ++i)
                this.checkedListBox1.SetItemChecked(i, true);
        }

        // Activates the move button if there are checked items.
        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            InterpreterPlay play = (InterpreterPlay)(this.checkedListBox1.Items[e.Index]);
            play.isEnabled = (e.NewValue == CheckState.Checked);
            Console.WriteLine("Play is " + (play.isEnabled ? "ENABLED " : "DISABLED: ") + play.Name);
        }

        private void PlaySelectorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        private void click_Button1(object sender, System.EventArgs e) {
            for (int i = 0; i < this.checkedListBox1.Items.Count; ++i)
                this.checkedListBox1.SetItemChecked(i, true);
        }


        private void click_Button2(object sender, System.EventArgs e) {
            for (int i = 0; i < this.checkedListBox1.Items.Count; ++i)
                this.checkedListBox1.SetItemChecked(i, false);

        }
    }

}