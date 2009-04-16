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
            ((InterpreterPlay)(this.checkedListBox1.Items[e.Index])).isEnabled = (e.NewValue == CheckState.Checked);
        }

        private void PlaySelectorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }
    }

}