using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace RobocupPlays
{
    public class Link : LinkLabel
    {
        int index;
        public int Index
        {
            get { return index; }
        }
        private Type argType;
        public Type ArgType
        {
            get { return argType; }
        }
        public Link(int index, Type type)
            : base()
        {
            this.index = index;
            this.argType = type;
        }
    }
}
