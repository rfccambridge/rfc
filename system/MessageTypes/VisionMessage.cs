using System;
using System.Collections.Generic;
using System.Text;

namespace Robocup.Core
{
    [Serializable]
    public class VisionMessage
    {
        public struct RobotData
        {
            public RobotData(int id, bool ourTeam, Vector2 position, double orientation)
            {
                this.id = id;
                this.ourTeam = ourTeam;
                this.position = position;
                this.orientation = orientation;
            }
            private bool ourTeam;
            /// <summary>
            /// Whether or not this robot is on our team.
            /// </summary>
            public bool OurTeam
            {
                get { return ourTeam; }
            }

            private int id;
            /// <summary>
            /// The ID of the robot -- can be any value
            /// </summary>
            public int ID
            {
                get { return id; }
            }
	
            private Vector2 position;
            /// <summary>
            /// The position of the robot, in our standard coordinate system
            /// ([0,0] is center of field, [1,1] is up and to the right, the unit is meters)
            /// </summary>
            public Vector2 Position
            {
                get { return position; }
            }
            private double orientation;
            public double Orientation
            {
                get { return orientation; }
            }
        }
        public VisionMessage(Vector2 ballPosition)
        {
            this.ballPosition = ballPosition;
        }
        private Vector2 ballPosition;
        /// <summary>
        /// The position of the ball, in our standard coordinate system
        /// ([0,0] is center of field, [1,1] is up and to the right, the unit is meters)
        /// </summary>
        public Vector2 BallPosition
        {
            get { return ballPosition; }
        }
        private List<RobotData> ourRobots = new List<RobotData>();
        public List<RobotData> OurRobots
        {
            get { return ourRobots; }
        }
        private List<RobotData> theirRobots = new List<RobotData>();
        public List<RobotData> TheirRobots
        {
            get { return theirRobots; }
        }
    }
}
