using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using Robocup.MotionControl;
using Robocup.Utilities;


namespace SimplePathFollower {
    /// <summary>
    /// This class is designed to help automate the process of PID constants calibrator.
    /// Eventually, it is supposed to do a semi-automatic search through the constants' state space and
    /// choose the optimal values.
    /// So far, it tries to find a metric for the quiality of a set of constants.
    /// </summary>
    public class PIDCalibrator {

        private Feedback feedbackPID;
        private PathFollower pathFollower;
        private const string stateFileName = "../../resources/control/PID.txt";

        private double lapError;
        private double lapTime;
        private double lapStartTime;
        private bool lapping;

        public PIDCalibrator(PathFollower _pathFollower) {
            pathFollower = _pathFollower;

            BugFeedbackMotionPlanner planner = _pathFollower.Planner as BugFeedbackMotionPlanner;
            if (planner == null)
                throw new Exception("The sky should fall on your head for calling PIDCalibrator with unsupported motion planner!!!");

            feedbackPID = planner.GetFeedbackObj(_pathFollower.RobotID);
            feedbackPID.OnUpdateErrors = UpdateErrors;
            
            lapError = 0;
            lapTime = 0;
            lapStartTime = 0;
            lapping = false;
        }

        /// <summary>
        /// Appends the PID state (12 constants, lapError, lapTime) into a resource file
        /// </summary>
        public void DumpState() {
            
            if (!File.Exists(stateFileName)) {
                StreamWriter stateStream = File.CreateText(stateFileName);
                stateStream.WriteLine("X.P\tX.I\tX.D\tX.A\tY.P\tY.I\tY.D\tY.A\tTH.P\tTH.I\tTH.D\tTH.A\tErr\tTime");
                stateStream.Close();
            }
            
            DOF_Constants xConst, yConst, thetaConst;
            feedbackPID.GetConstants(out xConst, out yConst, out thetaConst);

            string state = string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}\t{13}",
                xConst.P, xConst.I, xConst.D, xConst.ALPHA,
                yConst.P, yConst.I, yConst.D, yConst.ALPHA,
                thetaConst.P, thetaConst.I, thetaConst.D, thetaConst.ALPHA,
                Math.Sqrt(lapError).ToString("F2"), lapTime.ToString("F2"));
            
            File.AppendAllText(stateFileName, state);
        }

        /// <summary>
        /// Called from the PID feedback each time new values are recalculated.
        /// The error we get is the proportional error (so probably assumes we have a feed forward term)
        /// </summary>
        public void UpdateErrors(double xError, double yError, double thetaError) 
        {
            if (!lapping)
                return;

            //at this stage, ignore orientation; later -> do some normalizing magic to include it
            lapError += xError * xError + yError * yError;
        }

        /// <summary>
        /// Called from PathFollower which runs the lapping code, when the lap is actually finished
        /// </summary>
        public void EndLap(bool success) 
        {
            lapTime = HighResTimer.SecondsSinceStart() - lapStartTime;
            pathFollower.Stop();
            lapping = false;

            if (success) {
                DumpState();
                //TODO: Do magic to find a new set of constants(explore the state space) and start the next lap 
            }
        }
        
        /// <summary>
        /// Called from PathFollower when the lap is actually started (i.e. robot at first point for the first time)
        /// </summary>
        public void StartLap()
        {
            lapError = 0;
            lapTime = 0;
            lapStartTime = HighResTimer.SecondsSinceStart();

            lapping = true;        
        }

        public bool InitLap() {
            return pathFollower.Follow();
        }

    }
}
