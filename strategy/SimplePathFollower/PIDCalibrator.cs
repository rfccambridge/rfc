using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using Robocup.MotionControl;
using Robocup.Utilities;
using Robocup.CoreRobotics;


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

        // used when exploring the PID constants state space
        private double P_X_STEP = 10;
        private double P_Y_STEP = 10;
        private double P_TH_STEP = 10;
        private double D_X_STEP = 10;
        private double D_Y_STEP = 10;
        private double D_TH_STEP = 10;
        //So far, keep the I and the ALPHA terms constant

        public PIDCalibrator(PathFollower _pathFollower) {
            pathFollower = _pathFollower;

            //I know it's bad practice! I don't care!!!
            TangentBugFeedbackMotionPlanner tbugPlanner = (_pathFollower.Planner as TangentBugFeedbackMotionPlanner);
            BugFeedbackMotionPlanner bugPlanner = (_pathFollower.Planner as BugFeedbackMotionPlanner);
            if(bugPlanner != null)
                feedbackPID = bugPlanner.GetFeedbackObj(_pathFollower.RobotID);
            else if (tbugPlanner != null)
                feedbackPID = tbugPlanner.GetFeedbackObj(_pathFollower.RobotID);
            else
                throw new Exception("The sky should fall on your head for calling PIDCalibrator with unsupported motion planner!!!");

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

            StreamWriter writer = File.AppendText(stateFileName);
            writer.WriteLine(state);
            writer.Close();
            //File.AppendAllText(stateFileName, state);
        }

        public void LoadState(out DOF_Constants xConst, out DOF_Constants yConst, out DOF_Constants thetaConst) {

            if (!File.Exists(stateFileName)) {
                Console.WriteLine("Missing PID calibration data. Taking current values from motion planner...");
                feedbackPID.GetConstants(out xConst, out yConst, out thetaConst);
                return;
                //throw new Exception("Missing PID calibration data! Check resources/control/PID.txt ");
            }

            StreamReader reader = File.OpenText(stateFileName);
            string buff = string.Empty;
            //read the last line in the state file
            while (!reader.EndOfStream)
                buff = reader.ReadLine();

            string[] stringConsts = buff.Split('\t');
            if (stringConsts.Length != 14)
                throw new Exception("Corrupted PID calibration data! Check the number of values (should be 14) ");

            xConst = new DOF_Constants(stringConsts[0], stringConsts[1], stringConsts[2], stringConsts[3]);
            yConst = new DOF_Constants(stringConsts[4], stringConsts[5], stringConsts[6], stringConsts[7]);
            thetaConst = new DOF_Constants(stringConsts[8], stringConsts[9], stringConsts[10], stringConsts[11]);
        }

        /// <summary>
        /// Called from the PID feedback each time new values are recalculated.
        /// The error we get is the proportional error (so probably assumes we have a feed forward term)
        /// </summary>
        public void UpdateErrors(double xError, double yError, double thetaError) {
            if (!lapping)
                return;

            //at this stage, ignore orientation; later -> do some normalizing magic to include it
            lapError += xError * xError + yError * yError;
        }

        /// <summary>
        /// Called from PathFollower which runs the lapping code, when the lap is actually finished
        /// </summary>
        public void EndLap(bool success, bool invokeStop) {
            lapTime = HighResTimer.SecondsSinceStart() - lapStartTime;
            if(invokeStop)
                pathFollower.Stop();
            lapping = false;

            if (success)
                DumpState();
        }

        /// <summary>
        /// Called from PathFollower when the lap is actually started (i.e. robot at first point for the first time)
        /// </summary>
        public void StartLap() {
            lapError = 0;
            lapTime = 0;
            lapStartTime = HighResTimer.SecondsSinceStart();

            lapping = true;
        }

        public bool InitLap() {
            return pathFollower.Follow();
        }

        public void ExploreAround() {

            DOF_Constants xConst_init, yConst_init, thetaConst_init;
            LoadState(out xConst_init, out yConst_init, out thetaConst_init);

            feedbackPID.UpdateConstants(xConst_init, yConst_init, thetaConst_init, false);

            if (InitLap()) {
                Console.WriteLine("PID Calibrator can't explore aroung a non-stable point.");
                return; //throw ?
            }


            //Iterate through all points in the state space, surrounding the base point
            //and save state for each of them

                    for (int k = -1; k < 1; k++)
                        for (int l = -1; l < 1; l++)
                            for (int i = -1; i < 1; i++)
                                for (int j = -1; j < 1; j++) {

                            if (i == 0 && j == 0 && k == 0 && l == 0)
                                continue;

                            DOF_Constants xConst = xConst_init;
                            DOF_Constants yConst = yConst_init;
                            DOF_Constants thetaConst = thetaConst_init;

                            //Always change x and y together at this point
                            xConst.P += i * P_X_STEP;
                            yConst.P += i * P_Y_STEP;

                            xConst.D += j * D_X_STEP;
                            yConst.D += j * D_Y_STEP;

                            thetaConst.P += k * P_TH_STEP;
                            thetaConst.D += l * D_TH_STEP;

                            feedbackPID.UpdateConstants(xConst, yConst, thetaConst, false);

                            Console.WriteLine("Exploring at i={0},j={1},k={2},l={3}", i, j, k, l);
                            if (InitLap()) {
                                Console.WriteLine("Exploration failed at i={0},j={1},k={2},l={3}", i, j, k, l);
                                return;
                            }
                        }
            
            //Iterate through all points in the state space, surrounding the base point
            //and save state for each of them

            for (int k = 0; k < 2; k++)
                for (int l = 0; l < 2; l++)
                    for (int i = 0; i < 2; i++)
                        for (int j = 0; j < 2; j++) {

                            if (i == 0 && j == 0 && k == 0 && l == 0)
                                continue;

                            DOF_Constants xConst = xConst_init;
                            DOF_Constants yConst = yConst_init;
                            DOF_Constants thetaConst = thetaConst_init;

                            //Always change x and y together at this point
                            xConst.P += i * P_X_STEP;
                            yConst.P += i * P_Y_STEP;

                            xConst.D += j * D_X_STEP;
                            yConst.D += j * D_Y_STEP;

                            thetaConst.P += k * P_TH_STEP;
                            thetaConst.D += l * D_TH_STEP;

                            feedbackPID.UpdateConstants(xConst, yConst, thetaConst, false);

                            Console.WriteLine("Exploring at i={0},j={1},k={2},l={3}", i, j, k, l);
                            if (InitLap()) {
                                Console.WriteLine("Exploration failed at i={0},j={1},k={2},l={3}", i, j, k, l);
                                return;
                            }
                        }
        }

    }
}
