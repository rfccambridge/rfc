using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Core;
using Robocup.CoreRobotics;

using System.IO;

using System.Drawing;

using Robocup.Geometry;

namespace Robocup.MotionControl {
    
    /// <summary>
    /// Represents a set of PID constants for a given degree of freedom
    /// </summary>
    public struct DOF_Constants {
        public DOF_Constants(string _P, string _I, string _D, string _ALPHA){
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
    public class Feedback {
       
        // PID parameters
        private DOF_Numbers xPID;
        private DOF_Numbers yPID;
        private DOF_Numbers thetaPID;
        private string constantsFile;

        private RobotModel model;

        const double wheelR = 0.0782828; //distance from the center of the robot to the wheels in meters

        public delegate void UpdateErrorsDelegate(double xError, double yError, double thetaError);
        public UpdateErrorsDelegate OnUpdateErrors;
        public string ConstantsFile { get { return constantsFile; } }

        public Feedback(int robotID, string constFile, RobotModel _model) {
            constantsFile = constFile;
            xPID = new DOF_Numbers(robotID, DOFType.X, constFile);
            yPID = new DOF_Numbers(robotID, DOFType.Y, constFile);
            thetaPID = new DOF_Numbers(robotID, DOFType.THETA, constFile);
            model = _model;
        }


        /// <summary>
        /// Given a current state and a desired state along a path, returns the desired wheelspeeds to
        /// return to the path
        /// </summary>
        /// <param name="currentPosition">Current position of the robot</param>
        /// <param name="desiredPosition">Nearest waypoint to the robot along the desired path</param>
        /// <returns></returns>
        public WheelSpeeds ComputeWheelSpeeds(RobotInfo currentState, RobotInfo desiredState){
            //first go through the theoretical robot model and compute feed forward commands
            double xForward, yForward, thetaForward;
            model.ComputeCommand(currentState, desiredState, out xForward, out yForward, out thetaForward);
            
            //then simply run PID to correct errors
            double xCommand = xPID.Compute(currentState.Position.X, desiredState.Position.X, currentState.Velocity.X, desiredState.Velocity.X, xForward);
            double yCommand = yPID.Compute(currentState.Position.Y, desiredState.Position.Y, currentState.Velocity.Y, desiredState.Velocity.Y, yForward);

            //to ensure that orientations are between 0 and 2 pi.
            double currentOrientation = UsefulFunctions.angleCheck(currentState.Orientation);
            double desiredOrientation = UsefulFunctions.angleCheck(desiredState.Orientation);
            double angularVCommand = thetaPID.Compute(currentOrientation, desiredOrientation, currentState.AngularVelocity, desiredState.AngularVelocity, thetaForward);

            if (OnUpdateErrors != null) {
                double xError = currentState.Position.X - desiredState.Position.X;
                double yError = currentState.Position.Y - desiredState.Position.Y;
                double thetaError = currentOrientation - desiredOrientation;
                OnUpdateErrors(xError, yError, thetaError);
            }

            //converts from x speed, y speed and angular speed to wheel speeds
            WheelSpeeds ws = convert(xCommand, yCommand, angularVCommand, currentOrientation);
            return ws;
        }
        
        /// <summary>
        /// Convert a desired x velocity, y velocity, angular velocity, and theta arguments to WheelSpeeds object
        /// </summary>
        /// <param name="xCommand"></param>
        /// <param name="yCommand"></param>
        /// <param name="angularV"></param>
        /// <param name="theta"></param>
        /// <returns></returns>
        private WheelSpeeds convert(double xCommand, double yCommand, double angularV, double theta){
            const double ANGLE_AXIS_TO_WHEEL = 41*Math.PI/180;
            
            //I assume the x command is effectively in m/s, so r the radius of the wheels from the center of
            //the robot is in meters

            //change from the x and y of the field to forward and lateral(right is positive) used below
            double forward = Math.Cos(theta)*xCommand+Math.Sin(theta)*yCommand;
            double lateral = Math.Sin(theta)*xCommand-Math.Cos(theta)*yCommand;

            //Console.WriteLine(lateral.ToString() + " lateral|Forward: " + forward.ToString());
        
            //computed here to save typing, since used 4 times
            double sing = Math.Sin(ANGLE_AXIS_TO_WHEEL);
            double cosg = Math.Cos(ANGLE_AXIS_TO_WHEEL);
        
            //wheel one is the front right wheel  wheel 2 is the back right wheel, and so on around the the robot clockwise


            double _lf =   (sing*lateral + cosg * forward - wheelR * angularV);
            double _rf =   -(sing*lateral - cosg*forward - wheelR*angularV);
            double _lb =  (-sing*lateral + cosg*forward - wheelR*angularV);
            double _rb =  -(-sing*lateral - cosg *forward - wheelR * angularV);

            
            /*int scaleUpFactor = 2;
            if (Math.Abs(_lf) < 10 && Math.Abs(_rf) < 10 && Math.Abs(_lb) < 10 && Math.Abs(_rb) < 10) {
                _lf *= scaleUpFactor;
                _rf *= scaleUpFactor;
                _lb *= scaleUpFactor;
                _rb *= scaleUpFactor;
            }*/
            
            int lf, rf, lb, rb;
            lf = (int)_lf;
            rf = (int)_rf;
            lb = (int)_lb;
            rb = (int)_rb;

            
            /*if (lf > 57) 
                lf = 57;
            if (lf < -57)
                lf = -57;
                        
            if (rf > 57) 
                rf = 57;
            if (rf < -57)
                rf = -57;
                        
            if (lb > 57) 
                lb = 57;
            if (lb < -57)
                lb = -57;
                        
            if (rb > 57) 
                rb = 57;
            if (rb < -57)
                rb = -57;
            */

            return new WheelSpeeds(lf, rf, lb, rb);
            //Note somewhere we need to check and ensure that wheel speeds being 
            //sent do not exceed maximum values allowed by the protocol.
        }

        public void ReloadConstants(){
            xPID.ReloadConstants();
            yPID.ReloadConstants();
            thetaPID.ReloadConstants();
        }

        public void UpdateConstants(DOF_Constants x, DOF_Constants y, DOF_Constants theta) {
            xPID.UpdateConstants(x);
            yPID.UpdateConstants(y);
            thetaPID.UpdateConstants(theta);
        }

        public void GetConstants(out DOF_Constants xConst, out DOF_Constants yConst, out DOF_Constants thetaConst) {
            xConst = xPID.PIDConstants;
            yConst = yPID.PIDConstants;
            thetaConst = thetaPID.PIDConstants;
        }
            

        //enum for the three different degrees of freedom the PID loops run for x, y and theta
        enum DOFType { X, Y, THETA };

        private class DOF_Numbers{

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
            public DOF_Numbers(int robot_id, DOFType dof, string _constantsFile){

                //pull out values for constants from a text file
                constantsFile = _constantsFile;
                dofType = dof;
                robotID=robot_id;
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
            public double Compute(double current, double desired, double current_dot, double desired_dot, double feedForward) {
                //Console.WriteLine("P = " + constants.P + " I = " + constants.I + " D = " + constants.D + " " + dofType.ToString());

                double diff = desired - current;
                if (dofType == DOFType.THETA) {//need to pick the smallest angle
                    if (diff > Math.PI)
                        diff = -(2 * Math.PI - diff);
                    else if (diff < -Math.PI)
                        diff = 2 * Math.PI + diff;
                }

                //double diff = UsefulFunctions.angleDifference(desired, current);
                    
                error = constants.ALPHA*(diff)+(1-constants.ALPHA)*(desired_dot-current_dot);
                
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
            public void ReloadConstants() {
                //clear accumulated error
                for (int i = 0; i < oldErrorsWindowSize; i++)
                    oldErrorsWindow[i] = 0;
                Ierror = 0;

                Constants.Load();

                
                constants.P = Constants.get<double>(constantsFile, "P_" + dofType.ToString() + "_" + robotID.ToString());
                constants.I = Constants.get<double>(constantsFile, "I_" + dofType.ToString() + "_" + robotID.ToString());
                constants.D = Constants.get<double>(constantsFile, "D_" + dofType.ToString() + "_" + robotID.ToString());
                constants.ALPHA = Constants.get<double>(constantsFile, "ALPHA_" + dofType.ToString() + "_" + robotID.ToString());
                
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
            public void UpdateConstants(DOF_Constants _constants) {
                //clear accumulated error
                for (int i = 0; i < oldErrorsWindowSize; i++)
                    oldErrorsWindow[i] = 0;
                Ierror = 0;

                constants = _constants;
            }
        }
    }
}
