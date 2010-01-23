using System;
using System.Collections.Generic;
using System.Text;
using Robocup.CoreRobotics;
using Robocup.Plays;
using Robocup.Core;
using Robocup.Utilities;
using Robocup.Geometry;

namespace Robocup.ControlForm
{
    public class PathFollowerPlayer : Player
    {
        protected int _robotID = 0;
        protected bool _firstLoop = true;
        protected List<RobotInfo> _waypoints = new List<RobotInfo>();
        protected int _waypointIndex = 0;
        protected HighResTimer _lapTimer = new HighResTimer();

        protected double MIN_GOAL_DIST;
        protected double MIN_GOAL_DIFF_ORIENTATION;

        public int RobotID
        {
            get { return _robotID; }
            set { _robotID = value; }
        }

        public PathFollowerPlayer(Team team, FieldHalf fieldHalf, IPredictor predictor,
                          IRobots commander, FieldDrawer fieldDrawer) :
            base(team, fieldHalf, predictor, commander, fieldDrawer)
        {
            LoadConstants();
        }

        public override void LoadConstants()
        {
            base.LoadConstants();
            MIN_GOAL_DIST = Constants.get<double>("motionplanning", "MIN_GOAL_DIST");
            MIN_GOAL_DIFF_ORIENTATION = Constants.get<double>("motionplanning", "MIN_GOAL_DIFF_ORIENTATION");
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

        protected override void doAction()
        {
            //because this class just gets one point from the gui,
            //generating a static path is taken care of in feedbackbackMotionPlanner

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

            // TODO: Add full ID support for all robots
            _controller.move(_robotID, false, _waypoints[_waypointIndex].Position, _waypoints[_waypointIndex].Orientation);

            // if lapping stops midway...
            //if (waypointIndex > waypoints.Count - 1) {
            //    waypointIndex = 0;
            //}            

            if (curinfo == null)
            {
                Console.WriteLine("Robot #" + _robotID + " not found on team " + _team.ToString());
                return;
            }
            /*double wpDistanceSq = curinfo.Position.distanceSq(waypoints[waypointIndex]);
            if (wpDistanceSq > MIN_SQ_DIST_TO_WP) {
                waypointIndex = (waypointIndex + 1);//Stop at end for now % waypoints.Count;
                controller.move(robotID, false, waypoints[waypointIndex]);
            }*/

            // Lap around

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
                        _fieldDrawer.UpdateLapDuration(_lapTimer.Duration);                        
                    }

                    Console.WriteLine("Starting lap...");
                    _lapTimer.Start();
                    _firstLoop = false;
                }
                _waypointIndex = (_waypointIndex + 1) % _waypoints.Count;
            }

        }
    }

    public class KickPlayer : Player
    {
        protected Vector2 _target = new Vector2(0, 0);
        protected int _robotID = 0;
        protected ActionInterpreter _actionInterpreter;

        public Vector2 Target
        {
            get { return _target; }
            set { _target = value; }
        }

        public int RobotID
        {
            get { return _robotID; }
            set { _robotID = value; }
        }

        public KickPlayer(Team team, FieldHalf fieldHalf, IPredictor predictor,
                          IRobots commander, FieldDrawer fieldDrawer) :
            base(team, fieldHalf, predictor, commander, fieldDrawer)
        {
            _actionInterpreter = new ActionInterpreter(team, _controller, predictor);
        }

        protected override void doAction()
        {
            _actionInterpreter.Kick(_robotID, _target);            
        }
    }

    public class BeamKickPlayer : KickPlayer
    {
        public BeamKickPlayer(Team team, FieldHalf fieldHalf, IPredictor predictor,
                  IRobots commander, FieldDrawer fieldDrawer)
            : base(team, fieldHalf, predictor, commander, fieldDrawer)
        {
        }

        protected override void doAction()
        {
            _actionInterpreter.BeamKick(_robotID, _target);
        }
    }
}
