using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace RobocupPlays
{
    partial class ShowExpressionsForm : Form
    {
        List<DesignerExpression> conditions, actions;
        MainForm mainform;
        DesignerExpression prevexpression = null;
        public ShowExpressionsForm(List<DesignerExpression> conditions, List<DesignerExpression> actions, MainForm mainform)
        {
            InitializeComponent();

            this.mainform = mainform;
            this.conditions = conditions;
            this.actions = actions;
        }
        public void update()
        {
            string[] conditionstrings = new string[conditions.Count];
            for (int i = 0; i < conditions.Count; i++)
            {
                conditionstrings[i] = conditions[i].ToString();
            }
            conditionBox.Items.Clear();
            conditionBox.Items.AddRange(conditionstrings);

            string[] actionstrings = new string[actions.Count];
            for (int i = 0; i < actions.Count; i++)
            {
                actionstrings[i] = actions[i].ToString();
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
                exp = conditions[index];
            else if (lb == actionBox)
                exp = actions[index];

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
                exp = conditions[index];
                actionBox.ClearSelected();
                        
            }
            else if (lb == actionBox)
            {
                exp = actions[index];
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
            mainform.repaint();
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