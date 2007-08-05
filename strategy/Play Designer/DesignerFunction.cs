using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace RobocupPlays
{
    /*class DesignerFunction : Function
    {
        private Type convertType(Type t)
        {
            /*if (t == typeof(PlayCircle))
                return typeof(DesignerCircle);
            else if (t == typeof(Vector2))
                return typeof(DesignerPoint);
            else if (t == typeof(PlayRobot))
                return typeof(DesignerRobot);
            else if (t == typeof(Line))
                return typeof(DesignerLine);
            else*
                return t;
        }
        private string getStringFromType(Type t)
        {
            if (t == typeof(float))
                return "0.0";
            else if (t.IsAssignableFrom(typeof(DesignerPoint)))
                return "<point>";
            else if (t == typeof(GreaterLessThan))
                return "<";
            else if (t.IsAssignableFrom(typeof(DesignerLine)))
                return "<line>";
            else if (t == typeof(TeamCondition))
                return "our_team";
            else if (t.IsAssignableFrom(typeof(DesignerRobot)))
                return "<robot>";
            else if (t.IsAssignableFrom(typeof(PlayCircle)))
                return "<circle>";
            else if (t == typeof(int))
            {
                return "<int>";
            }
            return "<" + t.Name + ">";
        }
        public Label[] getLabels()
        {
            Label[] rtn = new Label[numDescriptionObjects];
            for (int i = 0; i < Description.Length; i++)
            {
                rtn[i * 2] = new Label();
                rtn[i * 2].Text = Description[i];
                rtn[i * 2].Size = rtn[i * 2].GetPreferredSize(new Size());
            }
            for (int i = 0; i < ArgTypes.Length; i++)
            {
                rtn[i * 2 + 1] = new Link(i, convertType(ArgTypes[i]));
                if (objects[i] == null)
                {
                    rtn[i * 2 + 1].Text = getStringFromType(ArgTypes[i]);
                }
                else
                {
                    rtn[i * 2 + 1].Text = objects[i].ToString();
                }
                rtn[i * 2 + 1].Size = rtn[i * 2 + 1].GetPreferredSize(new Size());

            }
            return rtn;
        }
        //private Function function;
        private int numDescriptionObjects;
        private object[] objects;
        public void setObject(int index, object o)
        {
            objects[index] = o;
        }
        public bool objectDefined(int index)
        {
            return objects[index] != null;
        }
        public DesignerFunction(Function function) : base(function)
        {
            //this.function = function;
            numDescriptionObjects = NumArguments + Description.Length;
            objects = new object[NumArguments];
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            sb.Append(Name);
            for (int i = 0; i < objects.Length; i++)
            {
                sb.Append(' ');
                sb.Append(objects[i].ToString());
            }
            sb.Append(")");
            return sb.ToString();
        }
        public void highlightAll()
        {
            foreach (object o in objects)
            {
                if (o is Clickable)
                    ((Clickable)o).highlight();
                if (o is DesignerFunction)
                    ((DesignerFunction)o).highlightAll();
            }
        }
        public void unhighlightAll()
        {
            foreach (object o in objects)
            {
                if (o is Clickable)
                    ((Clickable)o).unhighlight();
                if (o is DesignerFunction)
                    ((DesignerFunction)o).unhighlightAll();
            }
        }
        public bool fullyDefined()
        {
            for (int i = 0; i < objects.Length; i++)
            {
                if (!objectDefined(i))
                    return false;
            }
            return true;
        }
    }*/
}
