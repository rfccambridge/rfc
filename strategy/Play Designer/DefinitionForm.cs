using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace RobocupPlays
{
    internal partial class DefinitionForm : Form
    {
        DesignerPlay play;
        MainForm mainform;
        internal DefinitionForm(DesignerPlay play, MainForm mainform)
        {
            InitializeComponent();

            this.play = play;
            this.mainform = mainform;
        }

        private void DefinitionForm_Paint(object sender, PaintEventArgs e)
        {
            if (definitionListBox.Items.Count != play.Robots.Count)
            {
                updateList();
            }
        }

        internal void updateList()
        {
            definitionListBox.Items.Clear();
            for (int i = 0; i < play.Robots.Count; i++)
            {
                //DesignerRobot robot = (DesignerRobot)((DesignerExpression)play.Robots[i]).getValue(-1);
                DesignerRobot robot = (DesignerRobot)(play.Robots[i]).StoredValue;
                //definitionListBox.Items.Add(((DesignerRobot)play.Robots[i]).getName());
                definitionListBox.Items.Add(robot.getName());
            }
            this.Invalidate();
        }

        private void movePressed(object sender, EventArgs e)
        {
            int x = definitionListBox.SelectedIndex;
            if (x == -1)
            {
                //nothing is selected
                return;
            }
            int switchwith = -1;
            if (sender == buttonMoveDown)
            {
                switchwith = x + 1;
                if (switchwith >= definitionListBox.Items.Count)
                    return;
            }
            else if (sender == buttonMoveUp)
            {
                switchwith = x - 1;
                if (switchwith < 0)
                    return;
            }
            DesignerExpression temp = play.Robots[x];
            play.Robots[x] = play.Robots[switchwith];
            play.Robots[switchwith] = temp;
            updateList();
            definitionListBox.SelectedIndex = switchwith;
        }

        private void definitionListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (definitionListBox.SelectedIndex >= 0)
            {
                foreach (DesignerExpression exp in play.Robots)
                {
                    //DesignerRobot robot = ((DesignerRobot)exp.getValue(-1));
                    DesignerRobot robot = ((DesignerRobot)exp.StoredValue);
                    robot.unhighlight();
                    //if (robot.getName() == ((DesignerRobot)play.Robots[definitionListBox.SelectedIndex]).getName())
                    if (robot.getName()==(string)definitionListBox.SelectedItem)
                        robot.highlight();
                }
                mainform.repaint();
            }

        }

        private void DefinitionForm_Deactivate(object sender, EventArgs e)
        {
            definitionListBox.ClearSelected();
            foreach (DesignerExpression exp in play.Robots)
            {
                //DesignerRobot robot = (DesignerRobot)exp.getValue(-1);
                DesignerRobot robot = (DesignerRobot)exp.StoredValue;
                robot.unhighlight();
            }
            mainform.repaint();
        }

    }
}