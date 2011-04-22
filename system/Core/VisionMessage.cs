using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Geometry;

namespace Robocup.Core
{
    public class VisionMessage
    {        
        public class RobotData
        {
            private Team team;
            private Vector2 position;
            private int id;
            private double orientation;

            public RobotData(int id, Team team, Vector2 position, double orientation)
            {
                this.id = id;
                this.team = team;
                this.position = position;
                this.orientation = orientation;
            }            
            /// <summary>
            /// Robot's team, in terms of the Team enum type
            /// </summary>
            public Team Team
            {
                get { return team; }
            }
            
            /// <summary>
            /// The ID of the robot -- can be any value
            /// </summary>
            public int ID
            {
                get { return id; }
            }
	    
            /// <summary>
            /// The position of the robot, in our standard coordinate system
            /// ([0,0] is center of field, [1,1] is up and to the right, the unit is meters)
            /// </summary>
            public Vector2 Position
            {
                get { return position; }
            }            
            public double Orientation
            {
                get { return orientation; }
            }
        }

        private List<RobotData> robots = new List<RobotData>();
        
        /// <summary>
        /// Creates a VisionMessage which contains no ball or robots
        /// </summary>
        public VisionMessage(int cameraID)
        {
            this.cameraID = cameraID;
        }
        /// <summary>
        /// Creates a new VisionMessage with the specified ball position.
        /// </summary>
        /// <param name="ballPosition">The position of the ball on the field, where
        /// ([0,0] is center of field, [1,1] is up and to the right, and the unit is meters.
        /// 
        /// If no ball was found, this parameter should be null.
        /// </param>
        public VisionMessage(int cameraID, BallInfo ball)
        {
            this.ball = ball;
            this.cameraID = cameraID;
        }

        private readonly int cameraID;
        public int CameraID
        {
            get { return cameraID; }            
        }

        private BallInfo ball;
        /// <summary>
        /// The position of the ball, in our standard coordinate system
        /// ([0,0] is center of field, [1,1] is up and to the right, the unit is meters).
        /// 
        /// If no ball was found on the field, the value will be null.
        /// </summary>
        public BallInfo Ball
        {
            get { return ball; }
            set { ball = value; }
        }        
        public List<RobotData> Robots
        {
            get { return robots; }
        }       
    }
}
