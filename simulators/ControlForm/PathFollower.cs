using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Robocup.CoreRobotics;
using Robocup.Plays;
using Robocup.Core;
using Robocup.Utilities;
using Robocup.Geometry;

namespace Robocup.ControlForm
{
    public class WaypointPlayer : Player
    {
        protected List<RobotInfo> _waypoints = new List<RobotInfo>();
        protected int _waypointIndex = 0;
        protected int _robotID = 0;

        public int RobotID
        {
            get { return _robotID; }
            set { _robotID = value; }
        }

        public WaypointPlayer(Team team, FieldHalf fieldHalf, FieldDrawer fieldDrawer, IPredictor predictor) :
            base("", team, fieldHalf, fieldDrawer, predictor) {
        }

        public override string ToString()
        {
            return base.ToString() + ", Robot " + _robotID.ToString();
        }

        public int AddWaypoint(RobotInfo waypoint)
        {
            if (_running)
                throw new ApplicationException("Can't add/remove endpoints while player is running.");
            _waypoints.Add(waypoint);
            return _waypoints.Count - 1;
        }

        public void RemoveWaypoint(int index)
        {
            if (_running)
                throw new ApplicationException("Can't add/remove endpoints while player is running.");
            _waypoints.RemoveAt(index);
        }

        public void ClearWaypoints()
        {
            if (_running)
                throw new ApplicationException("Can't add/remove endpoints while player is running.");
            _waypoints.Clear();
            _waypointIndex = 0;
        }
    }

    public class PathFollowerPlayer : WaypointPlayer
    {
        protected bool _firstLoop = true;
		protected bool _lapping = false;
        protected HighResTimer _lapTimer = new HighResTimer();

        protected double MIN_GOAL_DIST;
        protected double MIN_GOAL_DIFF_ORIENTATION;

		protected delegate void LapStart();
		protected delegate void LapEnd();
		protected delegate void UpdateState(RobotInfo currInfo);

    	protected LapStart OnLapStart;
		protected LapEnd OnLapEnd;
		protected UpdateState BeforeMoving;

        public PathFollowerPlayer(Team team, FieldHalf fieldHalf, FieldDrawer fieldDrawer, IPredictor predictor) :
            base(team, fieldHalf, fieldDrawer, predictor)
        {
            LoadConstants();
        }

        public override void LoadConstants()
        {
            base.LoadConstants();
            MIN_GOAL_DIST = Constants.get<double>("motionplanning", "MIN_GOAL_DIST");
            MIN_GOAL_DIFF_ORIENTATION = Constants.get<double>("motionplanning", "MIN_GOAL_DIFF_ORIENTATION");
        }      

        protected override void doAction()
        {
            if (_waypoints.Count == 0)
                return;

            // TODO: figure out how to handle missing robots
            RobotInfo curinfo;
            try
            {
                 curinfo = _predictor.GetRobot(_team, _robotID);
            }
            catch
            {
                Console.WriteLine("Predictor did not find Robot " + _robotID.ToString());
                return;
            }

        	if(BeforeMoving != null)
				BeforeMoving(curinfo);
            
			_controller.Move(_robotID, false, _waypoints[_waypointIndex].Position, _waypoints[_waypointIndex].Orientation);

            if (curinfo == null)
            {
                Console.WriteLine("Robot #" + _robotID + " not found on team " + _team.ToString());
                return;
            }
            
            
            double sqDistToGoal = curinfo.Position.distanceSq(_waypoints[_waypointIndex].Position);
            double diffOrientation = Math.Abs(UsefulFunctions.angleDifference(curinfo.Orientation, _waypoints[_waypointIndex].Orientation));

            if (sqDistToGoal < MIN_GOAL_DIST * MIN_GOAL_DIST && diffOrientation < MIN_GOAL_DIFF_ORIENTATION)
            {
                if (_waypointIndex == 0)
                {
                    if (!_firstLoop)
                    {
                        Console.WriteLine("Ending lap...");
                        _lapTimer.Stop();

                    	_lapping = false;
						if(OnLapEnd != null)
							OnLapEnd();
                        _fieldDrawer.UpdateLapDuration(_lapTimer.Duration);                        
                    }

                    Console.WriteLine("Starting lap...");
					
					if (OnLapStart != null)
						OnLapStart();

                	_lapping = true;					
					_lapTimer.Start();
                    _firstLoop = false;
                }
                _waypointIndex = (_waypointIndex + 1) % _waypoints.Count;
            }

        }

		public override void Stop()
		{
			base.Stop();

			_lapping = false;
		}
    }

	public class MeasuringFollowerPlayer : PathFollowerPlayer
	{
		protected double _lapDistance;
		protected RobotInfo _lastPosition;
		protected string stateFileName = "../../resources/laps.txt";


		public MeasuringFollowerPlayer(Team team, FieldHalf fieldHalf, FieldDrawer fieldDrawer, IPredictor predictor) :
			base(team, fieldHalf, fieldDrawer, predictor)
		{
			OnLapStart = LapStarted;
			OnLapEnd = LapEnded;
			BeforeMoving = StateUpdating;
		}
		
		void StateUpdating(RobotInfo currInfo)
		{
			if (currInfo == null)
				throw new Exception("MeasuringFollowerPlayer.StateUpdating called without a robot.");

			if (_lastPosition == null)
			{
				_lastPosition = currInfo;
				return;
			}

			if (!_lapping)
				return;

			_lapDistance += Math.Sqrt(_lastPosition.Position.distanceSq(currInfo.Position));
			_lastPosition = currInfo;
		}
		
		void LapStarted()
		{
			_lapDistance = 0;
		}

		void LapEnded()
		{
			double pathLength = 0;
			//Compute theoretical path length
			for (int i = 0; i < _waypoints.Count; i++)
			{
				pathLength += Math.Sqrt(_waypoints[i].Position.distanceSq(
					_waypoints[(i+1)%_waypoints.Count].Position));
			}

			double excessDistance = (_lapDistance - pathLength)/pathLength;

			StreamWriter outFile;
			if (!File.Exists(stateFileName))
			{
				outFile = File.CreateText(stateFileName);
				outFile.WriteLine("Lap(th)\tLap(r)\tExcess\tTime(s)");
			}
			else
			{
				outFile = File.AppendText(stateFileName);
			}

			string state = string.Format("{0}\t{1}\t{2}\t{3}",
				pathLength.ToString("F2"), _lapDistance.ToString("F2"),
				excessDistance.ToString("F2"), _lapTimer.Duration.ToString("F2"));
			outFile.WriteLine(state);
			outFile.Close();		
		}
	}

    public class KickPlayer : WaypointPlayer
    {
        protected Vector2 _target = new Vector2(0, 0);
        protected ActionInterpreter _actionInterpreter;

        public Vector2 Target
        {
            get { return _target; }
            set { _target = value; }
        }

        public KickPlayer(Team team, FieldHalf fieldHalf, FieldDrawer fieldDrawer, IPredictor predictor)
            :
            base(team, fieldHalf, fieldDrawer, predictor)
        {
            _actionInterpreter = new ActionInterpreter(team, _controller, _predictor);
        }

        public override void LoadConstants()
        {
            base.LoadConstants();

            if (_actionInterpreter != null)
                _actionInterpreter.LoadConstants();
        }

        protected override void doAction()
        {
            if (_waypoints.Count > 0)
                _actionInterpreter.Kick(_robotID, _waypoints[0].Position);
            else
                Console.WriteLine("KickPlayer: no target waypoint to kick to.");
        }
    }

    public class BeamKickPlayer : KickPlayer
    {
        public BeamKickPlayer(Team team, FieldHalf fieldHalf, FieldDrawer fieldDrawer, IPredictor predictor)
            : base(team, fieldHalf, fieldDrawer, predictor)
        {
            
        }

        protected override void doAction()
        {
            if (_predictor.GetBall() != null)
            {
                //StreamWriter sw = new StreamWriter("speeds.txt", true);
               // sw.WriteLine(_predictor.GetBall().Velocity.magnitude());
                //sw.Close();
            }
                if (_waypoints.Count > 0)
                _actionInterpreter.BeamKick(_robotID, _waypoints[0].Position, 1);
            else
                Console.WriteLine("BeamKickPlayer: no target waypoint to kick to.");
        }
    }
}
