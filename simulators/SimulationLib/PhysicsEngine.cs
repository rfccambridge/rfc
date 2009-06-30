using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using MovementModeler = Robocup.CoreRobotics.MovementModeler;

namespace Robocup.Simulation
{
    public class PhysicsEngine : IPredictor, IRobots
    {
    	private const int YELLOW = 0;
    	private const int BLUE = 1;
    	private const int OUR_TEAM = YELLOW;

		private bool _marking = false;
		private Vector2 _markedPosition;
		private double BALL_MOVED_DIST;

		private BallInfo ball = null;
		private List<RobotInfo>[] robots = new List<RobotInfo>[] { new List<RobotInfo>(), 
                                                                   new List<RobotInfo>() };

		VirtualRef referee;
        Dictionary<int, MovementModeler> movement_modelers = new Dictionary<int, MovementModeler>();
		Dictionary<int, WheelSpeeds> speeds = new Dictionary<int, WheelSpeeds>();

		public PhysicsEngine(VirtualRef referee)
		{
			ResetPositions();
			this.referee = referee;
			foreach (RobotInfo info in GetRobots())
				movement_modelers.Add(info.ID, new MovementModeler());

			BALL_MOVED_DIST = Constants.get<double>("plays", "BALL_MOVED_DIST");

		}

    	public void MoveRobot(int team, int robotID, RobotInfo new_info)
        {
			if (team != new_info.Team)
				throw new ApplicationException("old robot team and new robot team dont match!");
			if (robotID != new_info.ID)
				throw new ApplicationException("old robot id and new robot ids dont match!");

            foreach (RobotInfo info in GetRobots(team))
                if (info.ID == robotID)
                    UpdateRobot(info, new_info);
        }
        public void MoveBall(Vector2 newPosition)
        {
            UpdateBall(new BallInfo(newPosition));
        }

		public void ResetPositions()
        {
            robots = new List<RobotInfo>[] { 
			    // YELLOW
                new List<RobotInfo>() {
				new RobotInfo(new Vector2(-1.0, -1), 0, YELLOW, 0),
                new RobotInfo(new Vector2(-1.0, 0),  0, YELLOW, 1),
                new RobotInfo(new Vector2(-1.0, 1),  0, YELLOW, 2),
                new RobotInfo(new Vector2(-2f, -1),  0, YELLOW, 3),
                new RobotInfo(new Vector2(-2f, 1),  0, YELLOW, 4) },

				// BLUE
				new List<RobotInfo>() {
                new RobotInfo(new Vector2(1.0, -1),  0, BLUE, 5),
				new RobotInfo(new Vector2(1.0, 0),  0, BLUE, 6),
                new RobotInfo(new Vector2(1.0, 1),  0, BLUE, 7),
                new RobotInfo(new Vector2(2f, -1),  0, BLUE, 8),
                new RobotInfo(new Vector2(2f, 1),  0, BLUE, 9) }
            };
            ball = new BallInfo(Vector2.ZERO);
        }

		/// <summary>
		/// Steps forward the given number of seconds
		/// </summary>
		public void Step(double dt)
		{
			foreach (RobotInfo info in GetRobots(OUR_TEAM))
			{
				int id = info.ID;
				if (!speeds.ContainsKey(id))
					speeds[id] = new WheelSpeeds();
				UpdateRobot(info, movement_modelers[id].ModelWheelSpeeds(info, speeds[id], dt));
			}

			//the speed at which the ball will bounce when it hits another robot
			double ballbounce = .005;
			const double collisionradius = .10;
			//the fraction of the ball velocity that it loses every second
			//well, roughly, because compounding counts, so it's off by about a factor of 2.5
			const double balldecay = 6;

			BallInfo ballInfo = GetBall();

			// run one step of tester

			// update ball location
			Vector2 newballlocation = ballInfo.Position + dt * ballInfo.Velocity;
			Vector2 newballvelocity = (1 - dt * balldecay) * ballInfo.Velocity;

			// check for collisions ball-robot, update ball position
			bool collided = false;
			foreach (RobotInfo r in GetRobots())
			{
				Vector2 location = r.Position;
				if (newballlocation.distanceSq(location) <= collisionradius * collisionradius)
				{
					collided = true;
					ballbounce = Math.Sqrt((ballInfo.Velocity - r.Velocity).magnitudeSq()) * .02;
					newballvelocity = ballbounce * ((ballInfo.Position - location).normalize());
					newballlocation = location + (collisionradius + .005) * (ballInfo.Position - location).normalize();
					break;
				}
			}

			// do wall boundaries
			if (!collided)
			{
				Vector2 ballPos = ballInfo.Position;
				double ballVx = newballvelocity.X;
				double ballVy = newballvelocity.Y;
				if (ballPos.X < -2.45)
					ballVx = Math.Abs(ballVx);
				else if (ballPos.X > 2.45)
					ballVx = -Math.Abs(ballVx);
				if (ballPos.Y < -1.7)
					ballVy = Math.Abs(ballVy);
				else if (ballPos.Y > 1.7)
					ballVy = -Math.Abs(ballVy);
				newballvelocity = new Vector2(ballVx, ballVy);
				newballlocation = new Vector2(ballInfo.Position.X + ballVx, ballInfo.Position.Y + ballVy);
			}

			var allRobots = GetRobots();

			// fix robot-robot collisions
			for (int i = 0; i < allRobots.Count; i++)
			{
				for (int j = 0; j < allRobots.Count; j++)
				{
					if (i == j)
						continue;
					Vector2 p1 = allRobots[i].Position;
					Vector2 p2 = allRobots[j].Position;
					if (p1.distanceSq(p2) <= .2 * .2)
					{
						Vector2 t1 = p1 + .01 * (p1 - p2).normalize();
						Vector2 t2 = p2 + .01 * (p2 - p1).normalize();
						UpdateRobot(allRobots[i], new RobotInfo(t1, allRobots[i].Orientation, 
							allRobots[i].Team, allRobots[i].ID));
						UpdateRobot(allRobots[j], new RobotInfo(t2, allRobots[j].Orientation, 
							allRobots[j].Team, allRobots[j].ID));
					}
				}
			}

			// friction
			UpdateBall(new BallInfo(newballlocation, newballvelocity));
			referee.RunRef(this, UpdateBall);
		}

        private void UpdateRobot(RobotInfo old_info, RobotInfo new_info)
        {
			if (old_info.Team != new_info.Team)
				throw new ApplicationException("old robot team and new robot team dont match!");
			if (old_info.ID != new_info.ID)
				throw new ApplicationException("old robot id and new robot ids dont match!");

            int index = robots[old_info.Team].IndexOf(old_info);
			if (index >= 0)
			{
				robots[old_info.Team][index].Position = new_info.Position;
				robots[old_info.Team][index].Velocity = new_info.Velocity;
				robots[old_info.Team][index].Orientation = new_info.Orientation;
			}
			else
			{
				throw new ApplicationException("no robot found with id " + old_info.ID + " on team " + old_info.Team);
			}
        }
        private void UpdateBall(BallInfo new_info)
        {
        	ball.Position = new_info.Position;
        	ball.Velocity = new_info.Velocity;
        	ball.LastSeen = new_info.LastSeen;
        }

        #region IRobot members
        const double initial_ball_speed = 0.1f;
       
        public void kick(int robotID)
        {
            RobotInfo robot = GetRobot(OUR_TEAM, robotID);
        	BallInfo ballInfo = GetBall();
            // add randomness to actual robot location / direction
            //const double randomComponent = initial_ball_speed / 3;            

            double ballVx = (double)(initial_ball_speed) * Math.Cos(robot.Orientation);
            double ballVy = (double)(initial_ball_speed) * Math.Sin(robot.Orientation);

            Console.WriteLine("ORIENTATION: " + robot.Orientation + " X: " + ballVx + " Y: " + ballVy);
            //ballVx += (double)(r.NextDouble() * 2 - 1) * randomComponent;
            //ballVy += (double)(r.NextDouble() * 2 - 1) * randomComponent;
            //RobotInfo prev = robot;
            //const double recoil = .02 / initial_ball_speed; ;
            UpdateBall(new BallInfo(ballInfo.Position, new Vector2(ballVx, ballVy)));
            //UpdateRobot(robot, new RobotInfo(prev.Position + (new Vector2(-ballVx * recoil, -ballVy * recoil)), prev.Orientation, prev.ID));
        }
        public void beamKick(int robotID) 
        {
            // for now, just kick!
            kick(robotID);
        }
		public void beamKick(int robotID, bool something)
		{
			throw new NotImplementedException("PhysicsEnginge: beamKick not implemented");
		}
        public void setMotorSpeeds(int robotID, WheelSpeeds speeds)
        {
            this.speeds[robotID] = speeds;
        }
        public void charge(int robotID) {
            throw new NotImplementedException("PhysicsEngine: not implemented");
        }
		public void startDribbler(int id)
		{
		}
		public void stopDribbler(int id)
		{
		}
        #endregion

		#region IPredictor Members

		public RobotInfo getCurrentInformation(int robotID)
		{
			/* foreach (RobotInfo info in getOurTeamInfo())
			 {
				 if (info.ID == robotID)
				 {
					 return info;
				 }
			 }
			 foreach (RobotInfo info in getTheirTeamInfo())
			 {
				 if (info.ID == robotID)
				 {
					 return info;
				 }
			 }
			 return null; */
			//throw new ApplicationException("PhysicsEngine: not implemented");
			return GetRobot(OUR_TEAM, robotID);
		}
		public List<RobotInfo> getOurTeamInfo()
		{
			//return new List<RobotInfo>(ourinfo);
			throw new ApplicationException("PhysicsEngine: not implemented");
		}
		public List<RobotInfo> getTheirTeamInfo()
		{
			//return new List<RobotInfo>(theirinfo);
			throw new ApplicationException("PhysicsEngine: not implemented");
		}
		public List<RobotInfo> getAllInfos()
		{
			return GetRobots();
		}
		public BallInfo getBallInfo()
		{
			//return ball_info;
			return GetBall();
		}

		// TO REPLACE THE ABOVE
		public List<RobotInfo> GetRobots(int team)
		{
			return robots[team];
		}
		public List<RobotInfo> GetRobots()
		{
			var allRobots = new List<RobotInfo>();
			allRobots.AddRange(robots[YELLOW]);
			allRobots.AddRange(robots[BLUE]);
			return allRobots;
		}
		public RobotInfo GetRobot(int team, int id)
		{
			List<RobotInfo> teamRobots = GetRobots(team);
			RobotInfo robot = teamRobots.Find(new Predicate<RobotInfo>(delegate(RobotInfo r)
			{
				return r.ID == id;
			}));
			if (robot == null)
			{
				throw new ApplicationException("PhysicsEngine.GetRobot: no robot with id=" +
					id.ToString() + " found on team " + team.ToString());
			}
			return robot;
		}
		public BallInfo GetBall()
		{
			return ball;
		}

		public void SetBallMark()
		{
			if (ball == null)
			{
				//throw new ApplicationException("Cannot mark ball position because no ball is seen.");
				return;
			}
			_markedPosition = ball != null ? new Vector2(ball.Position) : null;
			_marking = true;
		}

		public void ClearBallMark()
		{
			_marking = false;
			_markedPosition = null;
		}
		public bool HasBallMoved()
		{
			if (!_marking) return false;
			bool ret = (ball != null && _markedPosition == null) || (ball != null &&
						_markedPosition.distanceSq(ball.Position) > BALL_MOVED_DIST * BALL_MOVED_DIST);
			return ret;
		}
		public void SetPlayType(PlayTypes newPlayType)
		{
			// Do nothing: this method is for assumed ball: returning clever values for the ball
			// based on game state -- i.e. center of field during kick-off
		}
		#endregion
    }
}
