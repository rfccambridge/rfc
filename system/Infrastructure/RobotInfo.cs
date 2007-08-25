using System;
using System.Collections.Generic;
using System.Text;

namespace Robocup.Infrastructure
{
    public class BallInfo
    {
        private Vector2 position;
        public Vector2 Position
        {
            get { return position; }
        }
        private float dx;
        public float dX
        {
            get { return dx; }
        }
        private float dy;
        public float dY
        {
            get { return dy; }
        }

        public BallInfo(Vector2 position, float dx, float dy)
        {
            this.position = position;
            this.dx = dx;
            this.dy = dy;
        }
        public BallInfo(BallInfo copy)
        {
            this.position = copy.position;
            this.dx = copy.dx;
            this.dy = copy.dy;
        }

        public override string ToString()
        {
            return "BallInfo: " + position;
        }
    }
    public class RobotInfo
    {
        private readonly List<string> tags = new List<string>();
        public List<string> Tags
        {
            get { return tags; }
        }

        private Vector2 position;
        public Vector2 Position
        {
            get { return position; }
        }
        private float orientation;
        public float Orientation
        {
            get { return orientation; }
        }
        private RobotStates state;
        public RobotStates State
        {
            get { return state; }
            set { state = value; }
        }
        private bool assigned = false;
        public bool Assigned
        {
            get { return assigned; }
            set { assigned = value; }
        }
        private int idnum;

        public int ID
        {
            get { return idnum; }
            //set { idnum = value; }
        }

        public RobotInfo(Vector2 position, float orientation, int id) : this(position, orientation, id, RobotStates.Free) { }
        internal RobotInfo(Vector2 position, float orientation, int id, RobotStates state)
        {
            this.position = position;
            this.orientation = orientation;
            this.state = state;
            this.assigned = false;
            this.idnum = id;
        }

        public void setFree()
        {
            this.State = RobotStates.Free;
            this.Assigned = false;
        }
        public RobotInfo copy()
        {
            return new RobotInfo(position, orientation, idnum, state);
        }
        public override string ToString()
        {
            return idnum + ": " + position;
        }
    }

    public enum RobotStates
    {
        Free = 0,
        Busy = 10
    }
}
