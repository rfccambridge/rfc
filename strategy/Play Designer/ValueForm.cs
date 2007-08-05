using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using Robocup.Infrastructure;

namespace RobocupPlays
{
    /*enum EditingCommandStates
    {
        NewCondition, NewAction,Editing
    }*/
    partial class ValueForm : Form
    {
        //private DesignerFunction function = new DesignerFunction(Function.Functions[0]);
        private DesignerExpression expression = null;

        private Label[] labels;

        private Function[] allFunctions;

        //private state_SelectingObject state;
        private MainForm mainform;

        ReturnAValueDelegate returnDelegate;
        Type wantedType;

        IList<DesignerExpression> previous;

        /// <summary>
        /// This will return either a value, or a function to compute a value.
        /// </summary>
        public delegate void ReturnAValueDelegate(DesignerExpression rtn);
        private readonly int[] magicNumbers = new int[8];
        public ValueForm(Type wantedType, ReturnAValueDelegate returnDelegate, MainForm mainform, DesignerExpression startingExpression, IList<DesignerExpression> previous)
        {
            this.wantedType = wantedType;
            this.expression = startingExpression;
            this.mainform = mainform;
            this.returnDelegate = returnDelegate;
            this.previous = previous;
            InitializeComponent();


            #region MAGIC NUMBERS!
            Panel container = splitContainer1.Panel2;
            magicNumbers[0] = container.Size.Width / 2 - label1.Location.X;
            magicNumbers[1] = container.Size.Width / 2 - functionSelector.Location.X;
            magicNumbers[2] = container.Size.Width - contentPanel.Size.Width;
            magicNumbers[3] = container.Size.Height - contentPanel.Size.Height;
            magicNumbers[4] = container.Size.Width / 2 - useFunctionButton.Location.X;
            magicNumbers[5] = container.Size.Height - useFunctionButton.Location.Y;
            magicNumbers[6] = useFunctionButton.Location.X - nameLabel.Location.X;
            magicNumbers[7] = useFunctionButton.Location.X - nameInputBox.Location.X;
            #endregion


            allFunctions = Function.getFunctions(wantedType);

            if (wantedType.GetMethod("Parse", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public, null, new Type[] { typeof(String) }, null) != null)
            //if (wantedType.GetMethod("Parse", System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.InvokeMethod|System.Reflection.BindingFlags.Public) != null)
            {
                label2.Visible = true;
                UseValueButton.Visible = true;
                enterValueBox.Visible = true;
                //object obj = Activator.CreateInstance(wantedType);
                if (expression != null && !expression.IsFunction)
                    //enterValueBox.Text = expression.getValue(0).ToString();
                    enterValueBox.Text = expression.StoredValue.ToString();
                else
                    enterValueBox.Text = ((object)Activator.CreateInstance(wantedType)).ToString();
            }
            else if (canFindOnField(wantedType))
            {
                SelectObjectButton.Visible = true;
                if (allFunctions.Length == 0)
                {
                    //for some reason, if you do this right away, then it sends this form to the back,
                    //but then brings it to the front again when you show it
                    //so wait a little bit, then send it to the back.
                    Timer timer = new Timer();
                    timer.Tick += new EventHandler(delegate(object sender, EventArgs e)
                    {
                        this.SendToBack(); selectClickable(true); timer.Stop();
                    });
                    timer.Interval = 10;
                    timer.Start();
                    //SelectObjectButton_click(null, null);
                    //this.SendToBack();
                    return;
                }
            }
            functionSelector.Items.Clear();
            foreach (Function f in allFunctions)
            {
                functionSelector.Items.Add(f.LongName);
            }
            if (startingExpression != null && startingExpression.IsFunction)
            {
                for (int i = 0; i < functionSelector.Items.Count; i++)
                {
                    if (functionSelector.Items[i].ToString() == startingExpression.theFunction.LongName)
                        functionSelector.SelectedIndex = i;
                    //functionSelector.Select(i, functionSelector.Items[i].ToString().Length);
                }
                expression = startingExpression;
                useFunctionButton.Enabled = true;
            }
            reset();
        }
        public ValueForm(Type wantedType, ReturnAValueDelegate returnDelegate, MainForm mainform, IList<DesignerExpression> intermediates)
            :
            this(wantedType, returnDelegate, mainform, null, intermediates) { }

        private bool canFindOnField(Type t)
        {
            List<Type> types = new List<Type>();
            types.Add(typeof(PlayRobot));
            types.Add(typeof(Circle));
            types.Add(typeof(GetPointable));
            types.Add(typeof(Line));
            types.Add(typeof(Vector2));
            foreach (Type tt in types)
            {
                if (tt.IsAssignableFrom(t))
                    return true;
            }
            return false;
        }

        private void conditionSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            //function = new DesignerFunction(allFunctions[((ComboBox)sender).SelectedIndex]);
            Function f = allFunctions[((ComboBox)sender).SelectedIndex];
            expression = new DesignerExpression(f, f.NumArguments);
            useFunctionButton.Enabled = expression.fullyDefined();
            reset();
        }
        private void reset()
        {
            contentPanel.Controls.Clear();
            if (expression != null && expression.IsFunction)
            {
                labels = expression.getLabels();
                foreach (Label l in labels)
                {
                    if (l is Link)
                    {
                        ((LinkLabel)l).Click += new EventHandler(this.clickHandler);
                    }
                }
                contentPanel.Controls.AddRange(labels);
                arrangeLabels();
            }
            if (expression != null && expression.Name != null)
            {
                this.Text = "Editing expression \"" + expression.Name + "\"";
            }
            else
                this.Text = "Editing expression [unnamed expression]";
        }
        /// <summary>
        /// takes the labels array, and places them nicely in the window.
        /// </summary>
        private void arrangeLabels()
        {
            int minx = 5, maxx = contentPanel.Size.Width - minx, dy = 25;
            int curx = minx, cury = 15;
            for (int i = 0; i < labels.Length; i++)
            {
                if (curx > minx && curx + labels[i].Size.Width > maxx)
                {
                    curx = minx;
                    cury += dy;
                }
                //This is ok to have Point, it has nothing to do with the interpreter
                labels[i].Location = new Point(curx, cury);
                curx += labels[i].Size.Width;
            }
            foreach (Label c in labels)
            {
                if (c is Link)
                {
                    Link l = (Link)c;
                    l.LinkColor = Color.Blue;
                    //if (command.Objects[l.Index] == null)
                    if (!expression.argDefined(l.Index))
                    {
                        l.LinkColor = Color.Red;
                    }
                }
            }
            this.Invalidate();
        }
        private void NewConditionForm_Resize(object sender, EventArgs e)
        {
            Panel container = splitContainer1.Panel2;
            /*magicNumbers[0] = container.Size.Width / 2 - label1.Location.X;
            magicNumbers[1] = container.Size.Width / 2 - conditionSelector.Location.X;
            magicNumbers[2] = container.Size.Width - contentPanel.Size.Height;
            magicNumbers[3] = container.Size.Height - contentPanel.Size.Width;
            magicNumbers[4] = container.Size.Width / 2 - doneButton.Location.X;
            magicNumbers[5] = container.Size.Height - doneButton.Location.Y;*/

            /*label1.Location = new Point(container.Size.Width / 2 - 134, label1.Location.Y);
            conditionSelector.Location = new Point(container.Size.Width / 2 - 33, conditionSelector.Location.Y);
            contentPanel.Size = new Size(container.Size.Width - 28, container.Size.Height - 76);
            doneButton.Location = new Point(container.Size.Width / 2 - 31, container.Size.Height - 33);*/

            label1.Location = new Point(container.Size.Width / 2 - magicNumbers[0], label1.Location.Y);
            functionSelector.Location = new Point(container.Size.Width / 2 - magicNumbers[1], functionSelector.Location.Y);
            contentPanel.Size = new Size(container.Size.Width - magicNumbers[2], container.Size.Height - magicNumbers[3]);
            useFunctionButton.Location = new Point(container.Size.Width / 2 - magicNumbers[4], container.Size.Height - magicNumbers[5]);
            nameLabel.Location = new Point(useFunctionButton.Location.X - magicNumbers[6], useFunctionButton.Location.Y - 14);
            nameInputBox.Location = new Point(useFunctionButton.Location.X - magicNumbers[7], useFunctionButton.Location.Y + 2);

            if (expression != null && expression.IsFunction)
                arrangeLabels();
        }
        int currentIndex = -1;
        private void clickHandler(object sender, EventArgs e)
        {
            this.AcceptButton = useFunctionButton;
#if DEBUG
            if (!(sender is Link))
            {
                throw new ApplicationException("Tried to call clickHandler from something that's not a Link!");
            }
#endif
            Link link = (Link)sender;

            currentIndex = link.Index;
            findObject(((Link)sender).ArgType, expression.getArgument(((Link)sender).Index));
        }

        //ValueInputForm vif;

        private void returnValueHere(DesignerExpression o)
        {
            setObject(o);
            this.Focus();
        }
        public delegate void ReturnDesignerExpression(DesignerExpression c);

        /// <summary>
        /// Return DesignerExpressions here.  This form will close afterwards, no matter what
        /// (useful if there is no other choice but to select something from the field)
        /// </summary>
        private void returnExpressionHere_Quit(DesignerExpression exp)
        {
            this.Enabled = true;
            this.Close();
            returnDelegate(exp);
        }
        /// <summary>
        /// Return DesignerExprssions here.  If the expression is null, though, then this form
        /// will not quit afterwards.
        /// </summary>
        private void returnExpressionHere_NoQuit(DesignerExpression exp)
        {
            this.Enabled = true;
            if (exp != null)
            {
                this.Close();
                returnDelegate(exp);
            }
            else
                this.Focus();
        }
        /// <summary>
        /// This gets called when this ValueForm wants another to produce an object
        /// </summary>
        private void findObject(Type t, DesignerExpression prevExpression)
        {
            ValueForm vf = new ValueForm(t, returnValueHere, mainform, prevExpression, previous);
            vf.FormClosed += delegate(object sender, FormClosedEventArgs e) { this.Enabled = true; };
            this.Enabled = false;
            vf.Show();
        }

        /// <summary>
        /// This is called when the user has finished defining on of the argument objects.
        /// </summary>
        public void setObject(DesignerExpression obj)
        {
            expression.setArgument(currentIndex, obj);
            this.Enabled = true;
            //this.Focus();

            /*if (vif != null)
            {
                vif.Dispose();
                vif = null;
            }*/

            useFunctionButton.Enabled = expression.fullyDefined();

            reset();
        }

        private void doneButton_Click(object sender, EventArgs e)
        {
            if (sender == useFunctionButton)
            {
#if DEBUG
                if (!expression.fullyDefined())
                    throw new ApplicationException("For some reason the condition is not fully defined, but it is letting you return it.");
#endif
                string name = nameInputBox.Text;
                if (name != "")
                {
                    expression.Name = nameInputBox.Text;
                }
                returnDelegate(expression);
                this.Close();
            }
            else if (sender == UseValueButton)
            {
                try
                {
                    //use reflection to call the Class.Parse() method on the string entered
                    object o = wantedType.InvokeMember("Parse", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Static, null, null, new object[] { enterValueBox.Text });
                    returnDelegate(new DesignerExpression(o));
                    this.Close();
                }
                catch (System.Reflection.TargetInvocationException) { }
            }
        }

        private void selectClickable(bool quitIfFail)
        {
            this.Enabled = false;
            mainform.BringToFront();
            //mainform.findObject(wantedType, returnExpressionHere);
            if (quitIfFail)
                mainform.findObject(wantedType, returnExpressionHere_Quit);
            else
                mainform.findObject(wantedType, returnExpressionHere_NoQuit);
        }
        private void SelectObjectButton_click(object sender, EventArgs e)
        {
            selectClickable(false);
        }

        private void buttonSelectPrevious_Click(object sender, EventArgs e)
        {
            List<DesignerExpression> possibilities = new List<DesignerExpression>();
            foreach (DesignerExpression exp in previous)
            {
                if (wantedType.IsAssignableFrom(exp.ReturnType))
                    possibilities.Add(exp);
            }
            if (possibilities.Count == 0)
            {
                MessageBox.Show("there are no saved objects of that type");
                return;
            }
                


            this.Enabled = false;
            SelectExpressionForm form = new SelectExpressionForm(possibilities, returnExpressionHere_NoQuit);
            form.FormClosed += delegate(object s, FormClosedEventArgs ee) { this.Enabled = true; };
            form.Show();
        }

        private void enterValueBox_Click(object sender, EventArgs e)
        {
            this.AcceptButton = UseValueButton;
        }

        /*private void NewConditionForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            mainform.editFormClosed();
        }*/
    }
}