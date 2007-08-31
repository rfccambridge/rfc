using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using RobocupPlays;
using System.IO;

namespace InterpreterTester
{
    public partial class PlayManager : Form
    {
        public struct PlayRecord
        {
            public PlayRecord(string pathname, InterpreterPlay play)
            {
                this.play = play;
                this.pathname = pathname;
            }
            public InterpreterPlay play;
            public string pathname;
            public override string ToString()
            {
                return play.Name + "  --  " + pathname;
            }
        }
        Interpreter leftInterpreter;
        Interpreter rightInterpreter;
        List<PlayRecord> leftPlays;
        List<PlayRecord> rightPlays;

        // We keep copies of the text versions of the plays, and try to avoid nagging save dialogs.
        //maps filenames -> text
        Dictionary<string, string> changed_files = new Dictionary<string, string>();
        //maps plays -> text
        Dictionary<InterpreterPlay, string> originals = new Dictionary<InterpreterPlay, string>();

        public PlayManager(Interpreter leftInterpreter, Interpreter rightInterpreter,
            List<PlayRecord> leftPlays, List<PlayRecord> rightPlays)
        {
            InitializeComponent();

            this.leftInterpreter = leftInterpreter;
            this.rightInterpreter = rightInterpreter;
            this.leftPlays = leftPlays;
            this.rightPlays = rightPlays;

            UpdateList();
        }

        public void UpdateList()
        {
            listBoxLeft.Items.Clear();
            listBoxRight.Items.Clear();
            foreach (PlayRecord record in leftPlays)
            {
                listBoxLeft.Items.Add(record);
            }
            foreach (PlayRecord record in rightPlays)
            {
                listBoxRight.Items.Add(record);
            }
        }

        private void listBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListBox lb = (ListBox)sender;
            int index = lb.SelectedIndex;
            PlayRecord record = (PlayRecord)lb.Items[index];
            // If this play was created during this session
            bool newly_created = (record.pathname == "<unsaved>");
            string original;
            if (newly_created)
                original = originals[record.play];
            else if (changed_files.ContainsKey(record.pathname))
                original = changed_files[record.pathname];
            else
                original = System.IO.File.ReadAllText(record.pathname);
            string txt = PlayDesignerInterface.CreateAndRunDesigner(original);
            if (txt != null)
            {
                InterpreterPlay play = new PlayLoader<InterpreterPlay, InterpreterExpression>(
                    new InterpreterExpression.Factory()).load(txt);
                string fname = record.pathname;
                if (sender == listBoxLeft)
                {
                    leftPlays.Remove(record);
                    leftPlays.Insert(index, new PlayRecord(fname, play));
                    leftInterpreter.ReplacePlay(record.play, play);
                }
                else if (sender == listBoxRight)
                {
                    rightPlays.Remove(record);
                    rightPlays.Insert(index, new PlayRecord(fname, play));
                    rightInterpreter.ReplacePlay(record.play, play);
                }
                if (newly_created)
                {
                    originals.Remove(record.play);
                    originals.Add(play, txt);
                }
                else
                {
                    if (changed_files.ContainsKey(fname))
                        changed_files.Remove(fname);
                    changed_files.Add(fname, txt);
                }
                UpdateList();
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            string txt = PlayDesignerInterface.CreateAndRunDesigner();
            if (txt != null)
            {
                InterpreterPlay play = new PlayLoader<InterpreterPlay, InterpreterExpression>(
                    new InterpreterExpression.Factory()).load(txt);

                string fname = "<unsaved>";
                CancelEventHandler handler = new CancelEventHandler(delegate(object o, CancelEventArgs ce)
                {
                    Stream s = saveFileDialog1.OpenFile();
                    StreamWriter sw = new StreamWriter(s);
                    sw.Write(txt);
                    sw.Close();
                    s.Close();
                    fname = saveFileDialog1.FileName;
                });
                saveFileDialog1.FileOk += handler;
                saveFileDialog1.ShowDialog();
                saveFileDialog1.FileOk -= handler;

                if (sender == buttonAddLeft)
                {
                    leftPlays.Add(new PlayRecord(fname, play));
                    leftInterpreter.AddPlay(play);
                }
                else if (sender == buttonAddRight)
                {
                    rightPlays.Add(new PlayRecord(fname, play));
                    rightInterpreter.AddPlay(play);
                }
                originals.Add(play, txt);
                UpdateList();
            }
        }

        private void SaveChanged()
        {
            if (changed_files.Count == 0)
                //no changes made
                return;
            DialogResult result =
                MessageBox.Show("Would you like to save your changes?", "Save changes", MessageBoxButtons.YesNo);
            if (result == DialogResult.No)
                //user doesn't want to save
                return;
            List<string> saved = new List<string>();
            foreach (KeyValuePair<string, string> pair in changed_files)
            {
                string path = pair.Key;
                saveFileDialog1.FileName = new FileInfo(path).Name;
                saveFileDialog1.InitialDirectory = new FileInfo(path).DirectoryName;
                CancelEventHandler handler = new CancelEventHandler(delegate(object o, CancelEventArgs e)
                {
                    Stream s = saveFileDialog1.OpenFile();
                    StreamWriter sw = new StreamWriter(s);
                    sw.Write(pair.Value);
                    saved.Add(pair.Key);
                    sw.Close();
                    s.Close();
                });
                saveFileDialog1.FileOk += handler;
                saveFileDialog1.ShowDialog();
                saveFileDialog1.FileOk -= handler;
            }
            foreach (string s in saved)
            {
                changed_files.Remove(s);
            }
        }

        private void PlayManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveChanged();
        }

        private void listBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (sender == listBoxLeft)
                {
                    int index = listBoxLeft.SelectedIndex;
                    if (index == -1)
                        return;
                    leftInterpreter.RemovePlay(leftPlays[index].play);
                    leftPlays.RemoveAt(index);
                }
                else if (sender == listBoxRight)
                {
                    int index = listBoxRight.SelectedIndex;
                    if (index == -1)
                        return;
                    rightInterpreter.RemovePlay(rightPlays[index].play);
                    rightPlays.RemoveAt(index);
                }
                UpdateList();
            }
        }

        private void listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender == listBoxLeft)
                listBoxRight.ClearSelected();
            if (sender == listBoxRight)
                listBoxLeft.ClearSelected();
        }
    }
}