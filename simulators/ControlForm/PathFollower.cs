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
    abstract public class WaypointPlayer : Player
    {
        protected List<RobotInfo> _waypoints = new List<RobotInfo>();
        protected int _waypointIndex = 0;
        protected int _robotID = 0;

        abstract public int RobotID { get; set; }

        public WaypointPlayer(Team team, FieldHalf fieldHalf, FieldDrawer fieldDrawer, IPredictor predictor) :
            base("", team, fieldHalf, fieldDrawer, predictor) {
        }

        public override string ToString()
        {
            return base.ToString() + ", Robot " + _robotID.ToString();
        }

        public int AddWaypoint(RobotInfo waypoint)
        {
            if (Running)
                throw new ApplicationException("Can't add/remove endpoints while player is running.");
            _waypoints.Add(waypoint);
            return _waypoints.Count - 1;
        }

        public void RemoveWaypoint(int index)
        {
            if (Running)
                throw new ApplicationException("Can't add/remove endpoints while player is running.");
            _waypoints.RemoveAt(index);
        }

        public void ClearWaypoints()
        {
            if (Running)
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

		protected delegate void LapStart(int id);
		protected delegate void LapEnd(int id);
		protected delegate void UpdateState(RobotInfo currInfo);

    	protected LapStart OnLapStart;
		protected LapEnd OnLapEnd;
		protected UpdateState BeforeMoving;

        public PathFollowerPlayer(Team team, FieldHalf fieldHalf, FieldDrawer fieldDrawer, IPredictor predictor) :
            base(team, fieldHalf, fieldDrawer, predictor)
        {
            LoadConstants();
        }

        public override int RobotID
        {
            get { return _robotID; }
            set { _robotID = value; }
        }

        public override void LoadConstants()
        {
            base.LoadConstants();
            MIN_GOAL_DIST = ConstantsRaw.get<double>("motionplanning", "PFP_MIN_GOAL_DIST");
            MIN_GOAL_DIFF_ORIENTATION = ConstantsRaw.get<double>("motionplanning", "PFP_MIN_GOAL_DIFF_ORIENTATION");
        }

        protected virtual void doRobotAction(int robotID)
        {
            if (_waypoints.Count == 0)
                return;

            // TODO: figure out how to handle missing robots
            RobotInfo curinfo = null;
            try
            {
                curinfo = _predictor.GetRobot(_team, robotID);
            }
            catch
            {
                Console.WriteLine("Predictor did not find Robot " + robotID.ToString());
                return;
            }

            if (BeforeMoving != null)
                BeforeMoving(curinfo);

            _controller.Move(robotID, false, _waypoints[_waypointIndex].Position, _waypoints[_waypointIndex].Orientation);

            if (curinfo == null)
            {
                Console.WriteLine("Robot #" + robotID + " not found on team " + _team.ToString());
                return;
            }


            double sqDistToGoal = curinfo.Position.distanceSq(_waypoints[_waypointIndex].Position);
            double diffOrientation = Math.Abs(Angle.AngleDifference(curinfo.Orientation, _waypoints[_waypointIndex].Orientation));

            if (sqDistToGoal < MIN_GOAL_DIST * MIN_GOAL_DIST && diffOrientation < MIN_GOAL_DIFF_ORIENTATION)
            {
                if (_waypointIndex == 0)
                {
                    if (!_firstLoop)
                    {
                        Console.WriteLine("Ending lap...");
                        _lapTimer.Stop();

                        _lapping = false;
                        if (OnLapEnd != null)
                            OnLapEnd(robotID);
                        _fieldDrawer.UpdateLapDuration(_lapTimer.Duration);
                    }

                    Console.WriteLine("Starting lap...");

                    if (OnLapStart != null)
                        OnLapStart(robotID);

                    _lapping = true;
                    _lapTimer.Start();
                    _firstLoop = false;
                }
                _waypointIndex = (_waypointIndex + 1) % _waypoints.Count;
            }
        }

        public override void doAction()
        {
            doRobotAction(_robotID);
        }

		public override void Stop()
		{
			base.Stop();

			_lapping = false;
		}
    }

    public class MultiFollowerPlayer : PathFollowerPlayer
    {
        private const int NUM_FOLLOWERS = 4;
        private int _startID;
        private int[] _waypointsIndex = new int[NUM_FOLLOWERS];

        private double[] _lapDistance = new double[NUM_FOLLOWERS];
        private HighResTimer[] _lapTimers = new HighResTimer[NUM_FOLLOWERS];
        private RobotInfo[] _lastPosition = new RobotInfo[NUM_FOLLOWERS];

        private String stateFileName = "../../resources/laps.txt";

        public MultiFollowerPlayer(Team team, FieldHalf fieldHalf, FieldDrawer fieldDrawer, IPredictor predictor)
            : base (team, fieldHalf, fieldDrawer, predictor)
        {
            for (int i = 0; i < NUM_FOLLOWERS; i++) {
                _waypointsIndex[i] = 0;
                _lapTimers[i] = new HighResTimer();
            }

            OnLapEnd = LapEnded;
            OnLapStart = LapStarted;
            BeforeMoving = StateUpdating;
        }

        public override int RobotID
        {
            get { return _startID; }
            set { _startID = value; }
        }

        public override void doAction()
        {
            for (int i = 0; i < NUM_FOLLOWERS; i++)
            {
                // stupid hack to have the gui working with waypoints and to let bots keep track of waypoints separately
                _waypointIndex = _waypointsIndex[i];    
                doRobotAction(_startID + i);
                _waypointsIndex[i] = _waypointIndex;
            }
            Auction.FinishRound(_team, _predictor);
            
            // This simulates the delays from the rest of the
            // system when we are not running a simple motion tester.
            // Most of that time is spent in Interpreter.Interpret()
            System.Threading.Thread.Sleep(10);
        }

        void StateUpdating(RobotInfo currInfo)
        {
            if (currInfo == null)
                throw new Exception("MeasuringFollowerPlayer.StateUpdating called without a robot.");

            int id = currInfo.ID;

            if (_lastPosition[id] == null)
            {
                _lastPosition[id] = currInfo;
                return;
            }

            if (!_lapping)
                return;

            _lapDistance[id] += Math.Sqrt(_lastPosition[id].Position.distanceSq(currInfo.Position));
            _lastPosition[id] = currInfo;
        }

        void LapStarted(int id)
        {
            _lapDistance[id] = 0;
            _lapTimers[id].Start();
        }


        void LapEnded(int id)
        {
            _lapTimers[id].Stop();
            double pathLength = 0;
            //Compute theoretical path length
            for (int i = 0; i < _waypoints.Count; i++)
            {
                pathLength += Math.Sqrt(_waypoints[i].Position.distanceSq(
                    _waypoints[(i + 1) % _waypoints.Count].Position));
            }

            double excessDistance = (_lapDistance[id] - pathLength) / pathLength;

            StreamWriter outFile;
            if (!File.Exists(stateFileName))
            {
                outFile = File.CreateText(stateFileName);
                outFile.WriteLine("ID\tLap(th)\tLap(r)\tExcess\tTime(s)");
            }
            else
            {
                outFile = File.AppendText(stateFileName);
            }

            string state = string.Format("{0}\t{1}\t{2}\t{3}\t{4}",
                id, pathLength.ToString("F2"), _lapDistance[id].ToString("F2"),
                excessDistance.ToString("F2"), _lapTimers[id].Duration.ToString("F2"));
            outFile.WriteLine(state);
            outFile.Close();
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
		
		void LapStarted(int id)
		{
			_lapDistance = 0;
		}

		void LapEnded(int id)
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

        public override int RobotID
        {
            get { return _robotID; }
            set { _robotID = value; }
        }

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

        public override void doAction()
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

        public override void doAction()
        {
            if (_waypoints.Count > 0)
                _actionInterpreter.BeamKick(_robotID, _waypoints[0].Position, 15);
            else
                Console.WriteLine("BeamKickPlayer: no target waypoint to kick to.");
        }
    }
}
