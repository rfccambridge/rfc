using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Infrastructure;

using System.Drawing;

namespace RobocupPlays
{
    interface DesignerObject
    {
        //string getName();
        //bool isDefined();
        //string getDefinition();
        void rename(string name);
    }

    interface Clickable : DesignerObject
    {
        bool willClick(Vector2 p);
        void highlight();
        void unhighlight();
        //void draw(Graphics g);
    }
    /*interface DesignerGetPointable : GetPointable//,DesignerObject
    {
        bool isDefined();
        string getDefinition();
        //string getPointDefinition();
        //it's now assumed that any GetPointable.getDefinition() gives the definition for the point that you get from it
    }*/
}
