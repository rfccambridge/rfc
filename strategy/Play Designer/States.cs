using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using Robocup.Core;
using Robocup.Geometry;

/* This file holds all the code for the separate variables that we want to keep track of,
 * based on what the user is currently doing (rather than cluttering up the main form, and
 * trying to remember what each is, and trying to resolve naming conflicts).
 */
namespace Robocup.Plays
{
    /// <summary>
    /// The base State class.  This is for polymorphism, and so that all states keep track of
    /// whether the mouse is pressed down or not.
    /// </summary>
    abstract class State
    {
        public bool mousedown = false;
    }
    /// <summary>
    /// This class is for the states that involve dragging the mouse.  It keeps track of where the mouse was last seen,
    /// and will give you the difference in mouse position each time it moves (well, as long as you ask for it).
    /// </summary>
    abstract class State_Moving : State
    {
        //private Vector2 prevMouse = null;
        private Vector2 prevMouse = null;
        public Vector2 diff(Vector2 newpoint)
        {
            return new Vector2(newpoint.X - prevMouse.X, newpoint.Y - prevMouse.Y);
        }
        public void setMouse(Vector2 newmouse)
        {
            prevMouse = newmouse;
        }
    }


    class state_AddingRobot : State { }
    class state_MovingObjects : State_Moving
    {
        public DesignerExpression robot = null;
        public DesignerExpression point = null;
        public DesignerBall ball = null;
    }
    class state_DrawingLine : State
    {
        public DesignerExpression line = null;
        public DesignerExpression firstpoint = null;
    }
    class state_PlacingIntersection : State { }
    class state_AddingClosestCondition : State
    {
        public DesignerExpression firstpoint = null;
        public DesignerExpression firstrobot = null;
    }
    class state_AddingBall : State { }
    class state_SelectingObject : State
    {
        public Type t = null;
        public ValueForm editForm = null;
        public ValueForm.ReturnDesignerExpression returnDelegate = null;
        public State previousState = null;
    }
    class state_AddingPoint : State {
        public DesignerExpression point = null;
    }
    class state_AddingCircle : State
    {
        public DesignerExpression firstpoint = null;
        public DesignerExpression circle = null;
        //public DesignerExpression secondpoint = new DesignerExpression(Function.getFunction("point"), 0, 0);
    }
    class state_EditingObject : State
    {
    }
}