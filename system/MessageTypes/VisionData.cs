using System;
using System.Collections.Generic;
using System.Text;

namespace Robocup.Core
{
    [Serializable]
    public class VisionDataMessage
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
            public int ID
            {
                get { return id; }
            }
	
            private Vector2 position;
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
        public VisionDataMessage(Vector2 ballPosition)
        {
            this.ballPosition = ballPosition;
        }
        private Vector2 ballPosition;
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
