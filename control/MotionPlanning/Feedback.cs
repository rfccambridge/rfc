using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Core;
using Robocup.CoreRobotics;

using System.IO;

using System.Drawing;

using Robocup.Geometry;

namespace Robocup.MotionControl
{

    /// <summary>
    /// Represents a set of PID constants for a given degree of freedom
    /// </summary>
    public struct DOF_Constants
    {
        public DOF_Constants(string _P, string _I, string _D, string _ALPHA)
        {
            P = double.Parse(_P);
            I = double.Parse(_I);
            D = double.Parse(_D);
            ALPHA = double.Parse(_ALPHA);
        }

        public double P;
        public double I;
        public double D;
        public double ALPHA; //must be between 0 and 1
    }


    /// <summary>
    /// Represents a PID loop to stabilize motion planning along a path
    /// There needs to be one of these for each robot
    /// </summary>
    public class Feedback
    {

        // PID parameters
        private DOF_Numbers xPID;
        private DOF_Numbers yPID;
        private DOF_Numbers thetaPID;
        private string constantsFile;


        private bool useFwdLat;
        private DOF_Numbers forwardPID;
        private DOF_Numbers lateralPID;

        private RobotModel model;
        private double DESIRED_SPEED;

        const double wheelR = 0.0782828; //distance from the center of the robot to the wheels in meters

        public delegate void UpdateErrorsDelegate(double xError, double yError, double thetaError);
        public UpdateErrorsDelegate OnUpdateErrors;
        public string ConstantsFile { get { return constantsFile; } }

        public Feedback(int robotID, string constFile, RobotModel _model, bool _useFwdLat)
        {
            constantsFile = constFile;
            xPID = new DOF_Numbers(robotID, DOFType.X, constFile);
            yPID = new DOF_Numbers(robotID, DOFType.Y, constFile);
            thetaPID = new DOF_Numbers(robotID, DOFType.THETA, constFile);
            model = _model;

            forwardPID = new DOF_Numbers(robotID, DOFType.FWDV, constFile);
            lateralPID = new DOF_Numbers(robotID, DOFType.LATV, constFile);

            useFwdLat = _useFwdLat;

            ReloadConstants();
        }


        /// <summary>
        /// Given a current state and a desired state along a path, returns the desired wheelspeeds to
        /// return to the path
        /// </summary>
        /// <param name="currentPosition">Current position of the robot</param>
        /// <param name="desiredPosition">Nearest waypoint to the robot along the desired path</param>
        /// <returns></returns>
        public WheelSpeeds ComputeWheelSpeeds(RobotInfo currentState, RobotInfo desiredState)
        {
            //first go through the theoretical robot model and compute feed forward commands
            double xForward, yForward, thetaForward;
            model.ComputeCommand(currentState, desiredState, out xForward, out yForward, out thetaForward);

            Vector2 directionVector = desiredState.Position - currentState.Position;

            double direction = UsefulFunctions.angleCheck(directionVector.cartesianAngle());

            double currVelocity = Math.Sqrt(currentState.Velocity.magnitudeSq());
            double angleVelocityDirection = UsefulFunctions.angleDifference(directionVector.cartesianAngle(),
                                                                            currentState.Velocity.cartesianAngle());

            double currVelocityForward = currVelocity * Math.Cos(angleVelocityDirection);
            double currVelocityLateral = currVelocity * Math.Sin(angleVelocityDirection);

            double forwardCommand = forwardPID.Compute(currVelocityForward, DESIRED_SPEED, 0, 0, DESIRED_SPEED);
            double lateralCommand = lateralPID.Compute(currVelocityLateral, 0, 0, 0, 0);

            //then simply run PID to correct errors
            double xCommand = xPID.Compute(currentState.Position.X, desiredState.Position.X, currentState.Velocity.X, desiredState.Velocity.X, xForward);
            double yCommand = yPID.Compute(currentState.Position.Y, desiredState.Position.Y, currentState.Velocity.Y, desiredState.Velocity.Y, yForward);

            //to ensure that orientations are between 0 and 2 pi.
            double currentOrientation = UsefulFunctions.angleCheck(currentState.Orientation);
            double desiredOrientation = UsefulFunctions.angleCheck(desiredState.Orientation);
            double angularVCommand = thetaPID.Compute(currentOrientation, desiredOrientation, currentState.AngularVelocity, desiredState.AngularVelocity, thetaForward);

            if (OnUpdateErrors != null)
            {
                double xError = currentState.Position.X - desiredState.Position.X;
                double yError = currentState.Position.Y - desiredState.Position.Y;
                double thetaError = currentOrientation - desiredOrientation;
                OnUpdateErrors(xError, yError, thetaError);
            }

            //change from the x and y of the field to forward and lateral(right is positive) used below
            double robotForward = -(Math.Cos(currentOrientation) * xCommand + Math.Sin(currentOrientation) * yCommand);
            double robotLateral = -(-Math.Sin(currentOrientation) * xCommand + Math.Cos(currentOrientation) * yCommand);

            #region From Motion frame to Robot frame
            double frameAngleDiff = UsefulFunctions.angleDifference(direction, currentOrientation);

            //change from the forward and lateral frame to the robot frame
            double robotFwd = -(Math.Cos(frameAngleDiff) * forwardCommand + Math.Sin(frameAngleDiff) * lateralCommand);
            double robotLtr = -(-Math.Sin(frameAngleDiff) * forwardCommand + Math.Cos(frameAngleDiff) * lateralCommand);
             
            #endregion

            WheelSpeeds tst = model.Convert(robotFwd, robotLtr, angularVCommand);

            //converts from x speed, y speed and angular speed to wheel speeds
            WheelSpeeds ws = model.Convert(robotForward, robotLateral, angularVCommand);
            //return ws;

            return useFwdLat ? tst : ws;
        }

        public void ReloadConstants()
        {
            xPID.ReloadConstants();
            yPID.ReloadConstants();
            thetaPID.ReloadConstants();

            DESIRED_SPEED = ConstantsRaw.get<double>(constantsFile, "DESIRED_SPEED");
        }

        public void UpdateConstants(DOF_Constants x, DOF_Constants y, DOF_Constants theta, bool save)
        {
            xPID.UpdateConstants(x, save);
            yPID.UpdateConstants(y, save);
            thetaPID.UpdateConstants(theta, save);
        }

        public void GetConstants(out DOF_Constants xConst, out DOF_Constants yConst, out DOF_Constants thetaConst)
        {
            xConst = xPID.PIDConstants;
            yConst = yPID.PIDConstants;
            thetaConst = thetaPID.PIDConstants;
        }


        //enum for the three different degrees of freedom the PID loops run for x, y and theta
        enum DOFType { X, Y, THETA, FWDV, LATV };

        private class DOF_Numbers
        {

            //these should all be initialized to their tuned value, or auto tuned
            //they should also all be positive values except maybe D            

            private const int oldErrorsWindowSize = 300;

            private DOF_Constants constants;
            public DOF_Constants PIDConstants { get { return constants; } }

            private double error = 0;
            private double oldError = 0;
            private List<double> oldErrorsWindow;
            private double Ierror = 0;
            private double Derror = 0;

            private DOFType dofType;
            private int robotID;

            private string constantsFile;

            /// <summary>
            /// Initialize parameters for a robot from a text file of parameters
            /// </summary>
            /// <param name="robotID">ID of the current robot</param>
            /// <param name="DOF">DOF the degree of freedom this is for either X Y or THETA</param>
            public DOF_Numbers(int robot_id, DOFType dof, string _constantsFile)
            {

                //pull out values for constants from a text file
                constantsFile = _constantsFile;
                dofType = dof;
                robotID = robot_id;
                //init error window with zeros
                oldErrorsWindow = new List<double>(oldErrorsWindowSize);
                for (int i = 0; i < oldErrorsWindowSize; i++)
                    oldErrorsWindow.Add(0);
                Ierror = 0;

                ReloadConstants();

            }

            //computes a command for either the x velocity the y velocity or the angular velocity.
            //assume orientations are between 0 and 2 pi.
            //it usese the PID constants stored in this instance and incorporates both the velocity and current valud i.e. x and dx/dt
            public double Compute(double current, double desired, double current_dot, double desired_dot, double feedForward)
            {
                //Console.WriteLine("P = " + constants.P + " I = " + constants.I + " D = " + constants.D + " " + dofType.ToString());

                double diff = desired - current;
                if (dofType == DOFType.THETA)
                {//need to pick the smallest angle
                    if (diff > Math.PI)
                        diff = -(2 * Math.PI - diff);
                    else if (diff < -Math.PI)
                        diff = 2 * Math.PI + diff;
                }

                //double diff = UsefulFunctions.angleDifference(desired, current);

                error = constants.ALPHA * (diff) + (1 - constants.ALPHA) * (desired_dot - current_dot);

                Derror = error - oldError;
                oldError = error;

                //this ensures that the Ierror is calculated for a limited window with a certain fixed size
                Ierror = Ierror + error - oldErrorsWindow[0];
                oldErrorsWindow.RemoveAt(0);
                oldErrorsWindow.Add(error);

                //double command = desired + constants.P * error + constants.I * Ierror + constants.D * Derror;
                double command = feedForward + constants.P * error + constants.I * Ierror + constants.D * Derror;
                return command;
            }

            //loads/reloads all constants from the file and sets I error to 0 to aid debugging
            public void ReloadConstants()
            {
                //clear accumulated error
                for (int i = 0; i < oldErrorsWindowSize; i++)
                    oldErrorsWindow[i] = 0;
                Ierror = 0;

                ConstantsRaw.Load();

                constants.P = ConstantsRaw.get<double>(constantsFile, "P_" + dofType.ToString() + "_" + robotID.ToString());
                constants.I = ConstantsRaw.get<double>(constantsFile, "I_" + dofType.ToString() + "_" + robotID.ToString());
                constants.D = ConstantsRaw.get<double>(constantsFile, "D_" + dofType.ToString() + "_" + robotID.ToString());
                constants.ALPHA = ConstantsRaw.get<double>(constantsFile, "ALPHA_" + dofType.ToString() + "_" + robotID.ToString());

                //if (!Constants.nondestructiveGet<double>(constantsFile, "P_" + dofType.ToString() + "_" + robotID.ToString(), out constants.P)) {
                //    Console.WriteLine("DOF_Numbers: constant not found " + "P_" + dofType.ToString() + "_" + robotID.ToString());
                //    constants.P = 0;
                //}
                //if (!Constants.nondestructiveGet<double>(constantsFile, "I_" + dofType.ToString() + "_" + robotID.ToString(), out constants.I)) {
                //Console.WriteLine("DOF_Numbers: constant not found " + "I_" + dofType.ToString() + "_" + robotID.ToString());
                //    constants.I = 0;
                //}
                //if (!Constants.nondestructiveGet<double>(constantsFile, "D_" + dofType.ToString() + "_" + robotID.ToString(), out constants.D)) {
                //Console.WriteLine("DOF_Numbers: constant not found " + "D_" + dofType.ToString() + "_" + robotID.ToString());
                //    constants.D = 0;
                //}
                //if (!Constants.nondestructiveGet<double>(constantsFile, "ALPHA_" + dofType.ToString() + "_" + robotID.ToString(), out constants.ALPHA)) {
                //    Console.WriteLine("DOF_Numbers: constant not found " + "ALPHA_" + dofType.ToString() + "_" + robotID.ToString());
                //    constants.ALPHA = 0;
                //}



            }

            /// <summary>
            /// Can be used to programatically update the set of constants.
            /// </summary>
            public void UpdateConstants(DOF_Constants _constants, bool save)
            {
                //clear accumulated error
                for (int i = 0; i < oldErrorsWindowSize; i++)
                    oldErrorsWindow[i] = 0;
                Ierror = 0;

                constants = _constants;
            }
        }
    }
}
