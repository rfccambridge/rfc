using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace RobocupPlays
{
    partial class SelectExpressionForm : Form
    {
        ValueForm.ReturnDesignerExpression returnDelegate;
        IList<DesignerExpression> choices;
        public SelectExpressionForm(IList<DesignerExpression> choices, ValueForm.ReturnDesignerExpression returnDelegate)
        {
            InitializeComponent();

            this.choices = choices;
            this.returnDelegate = returnDelegate;

            listBox1.Items.Clear();
            int maxNameLength = 0;
            foreach (DesignerExpression  exp in choices)
            {
                maxNameLength = Math.Max(maxNameLength, exp.Name.Length);
            }
            foreach (DesignerExpression exp in choices)
            {
                listBox1.Items.Add(exp.ToString().PadRight(5+maxNameLength, ' ') + "\t" + exp.getDefinition());
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            int index = listBox1.SelectedIndex;
            if (index < 0 || index >= choices.Count)
                return;
            this.Close();
            returnDelegate(choices[index]);
        }
    }
}