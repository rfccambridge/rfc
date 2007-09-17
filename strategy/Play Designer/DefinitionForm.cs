using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Robocup.Plays
{
    internal partial class DefinitionForm : Form
    {
        MainForm mainform;
        public DesignerPlay Play
        {
            get { return mainform.Play; }
        }
        internal DefinitionForm(MainForm mainform)
        {
            InitializeComponent();

            this.mainform = mainform;
        }

        private void DefinitionForm_Paint(object sender, PaintEventArgs e)
        {
            if (definitionListBox.Items.Count != Play.Robots.Count)
            {
                updateList();
            }
        }

        internal void updateList()
        {
            definitionListBox.Items.Clear();
            for (int i = 0; i < Play.Robots.Count; i++)
            {
                //DesignerRobot robot = (DesignerRobot)((DesignerExpression)Play.Robots[i]).getValue(-1);
                DesignerRobot robot = (DesignerRobot)(Play.Robots[i]).StoredValue;
                //definitionListBox.Items.Add(((DesignerRobot)Play.Robots[i]).getName());
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
            DesignerExpression temp = Play.Robots[x];
            Play.Robots[x] = Play.Robots[switchwith];
            Play.Robots[switchwith] = temp;
            updateList();
            definitionListBox.SelectedIndex = switchwith;
        }

        private void definitionListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (definitionListBox.SelectedIndex >= 0)
            {
                foreach (DesignerExpression exp in Play.Robots)
                {
                    //DesignerRobot robot = ((DesignerRobot)exp.getValue(-1));
                    DesignerRobot robot = ((DesignerRobot)exp.StoredValue);
                    robot.unhighlight();
                    //if (robot.getName() == ((DesignerRobot)Play.Robots[definitionListBox.SelectedIndex]).getName())
                    if (robot.getName()==(string)definitionListBox.SelectedItem)
                        robot.highlight();
                }
                mainform.repaint();
            }

        }

        private void DefinitionForm_Deactivate(object sender, EventArgs e)
        {
            definitionListBox.ClearSelected();
            foreach (DesignerExpression exp in Play.Robots)
            {
                //DesignerRobot robot = (DesignerRobot)exp.getValue(-1);
                DesignerRobot robot = (DesignerRobot)exp.StoredValue;
                robot.unhighlight();
            }
            mainform.repaint();
        }

    }
}