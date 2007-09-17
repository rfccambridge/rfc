using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace Robocup.Plays
{
    partial class ShowExpressionsForm : Form
    {
        private List<DesignerExpression> Conditions
        {
            get { return mainform.Play.Conditions; }
        }
        private List<DesignerExpression> Actions
        {
            get { return mainform.Play.Actions; }
        }

        MainForm mainform;
        DesignerExpression prevexpression = null;
        public ShowExpressionsForm(MainForm mainform)
        {
            InitializeComponent();

            this.mainform = mainform;
        }
        public void update()
        {
            string[] conditionstrings = new string[Conditions.Count];
            for (int i = 0; i < Conditions.Count; i++)
            {
                conditionstrings[i] = Conditions[i].ToString();
            }
            conditionBox.Items.Clear();
            conditionBox.Items.AddRange(conditionstrings);

            string[] actionstrings = new string[Actions.Count];
            for (int i = 0; i < Actions.Count; i++)
            {
                actionstrings[i] = Actions[i].ToString();
            }
            actionBox.Items.Clear();
            actionBox.Items.AddRange(actionstrings);
            this.Invalidate();
        }

        private void listboxDoubleClicked(object sender, EventArgs e)
        {
            DesignerExpression exp = null;
            ListBox lb = (ListBox)sender;
            int index=lb.SelectedIndex;
            if (index==-1)//nothing selected
                return;

            if (lb == conditionBox)
                exp = Conditions[index];
            else if (lb == actionBox)
                exp = Actions[index];

            if (exp == null)
                throw new ApplicationException("You're trying to edit a command, but it is somehow set to null");

            mainform.editExpression(exp);
        }

        private void listboxSelectedIndexChanged(object sender, EventArgs e)
        {
            DesignerExpression exp = null;
            ListBox lb = (ListBox)sender;
            int index = lb.SelectedIndex;
            if (index == -1)//nothing selected
                return;

            if (lb == conditionBox)
            {
                exp = Conditions[index];
                actionBox.ClearSelected();
                        
            }
            else if (lb == actionBox)
            {
                exp = Actions[index];
                conditionBox.ClearSelected();

                /*mainform.clearArrows();
                if (prevcommand == null)
                {
                    Arrow[] arrows = ((Action)c).getArrows();
                    foreach (Arrow a in arrows)
                    {
                        mainform.addArrow(a);
                    }
                }*/
            }

            /*if (prevexpression != null)
            {
                prevexpression.unhighlightAll();
            }
            f.highlightAll();*/
            prevexpression = exp;
            mainform.Invalidate();
        }

        private void ShowCommandsForm_Deactivate(object sender, EventArgs e)
        {
            if (prevexpression != null)
            {
                //prevfunction.unhighlightAll();
                //mainform.repaint();
                conditionBox.ClearSelected();
                actionBox.ClearSelected();
            }
        }
    }
}