using System;
using System.Collections.Generic;
using System.Text;

namespace Robocup.Core
{
    public class BallInfo
    {
        private readonly Vector2 position;
        public Vector2 Position
        {
            get { return position; }
        }
        private readonly Vector2 velocity;
        public Vector2 Velocity
        {
            get { return velocity; }
        }

        /// <summary>
        /// Creates a BallInfo with zero velocity.
        /// </summary>
        public BallInfo(Vector2 position) : this(position, Vector2.ZERO) { }
        public BallInfo(Vector2 position, Vector2 velocity)
        {
            this.position = position;
            this.velocity = velocity;
        }
        public BallInfo(BallInfo copy)
        {
            this.position = copy.position;
            this.velocity = copy.velocity;
        }

        public override string ToString()
        {
            return "BallInfo: " + position;
        }
    }
    public class RobotInfo
    {
        /// <summary>
        /// Creates a RobotInfo with zero velocity.
        /// </summary>
        public RobotInfo(Vector2 position, float orientation, int id)
            : this(position, Vector2.ZERO, orientation, id)
        { }
        public RobotInfo(Vector2 position, Vector2 velocity, float orientation, int id)
        {
            this.position = position;
            this.velocity = velocity;
            this.orientation = orientation;
            this.idnum = id;
        }

        private readonly List<string> tags = new List<string>();
        /// <summary>
        /// The list of strings that this particular robot has been tagged with.  Ex: "goalie" if this is the goalie bot.
        /// You can add and remove tags from this.
        /// </summary>
        public List<string> Tags
        {
            get { return tags; }
        }

        private readonly Vector2 position;
        /// <summary>
        /// The position of the robot.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
        }
        private readonly Vector2 velocity;
        /// <summary>
        /// The velocity of the robot.
        /// </summary>
        public Vector2 Velocity
        {
            get { return velocity; }
        }

        private readonly float orientation;
        public float Orientation
        {
            get { return orientation; }
        }

        private readonly int idnum;
        public int ID
        {
            get { return idnum; }
        }

        public override string ToString()
        {
            return idnum + ": " + position;
        }
    }
    /// <summary>
    /// This class is for use by the interpreter only.  The interpreter copies the data from RobotInfos into these objects,
    /// adding some extra data.
    /// </summary>
    public class InterpreterRobotInfo : RobotInfo
    {
        private RobotStates state;
        /// <summary>
        /// Whether this robot has an action assigned or not.
        /// </summary>
        public RobotStates State
        {
            get { return state; }
            set { state = value; }
        }
        private bool assigned = false;
        /// <summary>
        /// Whether this robot has a definition assigned or not, for this particular play.
        /// </summary>
        public bool Assigned
        {
            get { return assigned; }
            set { assigned = value; }
        }
        /// <summary>
        /// </summary>
        public void setFree()
        {
            this.State = RobotStates.Free;
            this.Assigned = false;
        }

        public InterpreterRobotInfo(Vector2 position, Vector2 velocity, float orientation, int id)
            : base(position, velocity, orientation, id)
        {
            this.state = RobotStates.Free;
            this.assigned = false;
        }
    }

    public enum RobotStates
    {
        Free,
        Busy
    }
}
