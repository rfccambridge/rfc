using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Core;
using Robocup.CoreRobotics;

using System.IO;

using System.Drawing;

namespace Robocup.MotionControl {
    
    /// <summary>
    /// Represents a set of PID constants for a given degree of freedom
    /// </summary>
    public struct DOF_Constants {
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

        const double wheelR = 0.0782828; //distance from the center of the robot to the wheels in meters

        public delegate void UpdateErrorsDelegate(double xError, double yError, double thetaError);
        public UpdateErrorsDelegate OnUpdateErrors;

        public Feedback(int robotID) {
            xPID = new DOF_Numbers(robotID, DOFType.X);
            yPID = new DOF_Numbers(robotID, DOFType.Y);
            thetaPID = new DOF_Numbers(robotID, DOFType.THETA);
        }


        /// <summary>
        /// Given a current state and a desired state along a path, returns the desired wheelspeeds to
        /// return to the path
        /// </summary>
        /// <param name="currentPosition">Current position of the robot</param>
        /// <param name="desiredPosition">Nearest waypoint to the robot along the desired path</param>
        /// <returns></returns>
        public WheelSpeeds ComputeWheelSpeeds(RobotInfo currentState, RobotInfo desiredState){
            //runs the math for the PID
            double xCommand = xPID.Compute(currentState.Position.X, desiredState.Position.X, currentState.Velocity.X, desiredState.Velocity.X);
            double yCommand = yPID.Compute(currentState.Position.Y, desiredState.Position.Y, currentState.Velocity.Y, desiredState.Velocity.Y);
            //to ensure that orientations are between 0 and 2 pi.
            double currentOrientation = angleCheck(currentState.Orientation);
            double desiredOrientation = angleCheck(desiredState.Orientation);
            
            //Console.WriteLine(currentState.Position.X.ToString() + " Current X|Desired: " + desiredState.Position.X.ToString());
            //Console.WriteLine(currentState.Position.Y.ToString() + " Current Y|Desired: " + desiredState.Position.Y.ToString());
            //Console.WriteLine(currentOrientation.ToString() + " Current Theta|Desired: " + desiredOrientation.ToString());
            //Console.WriteLine("current velocity: {0}", currentState.Velocity);
            //Console.WriteLine("position delta: {0}", desiredState.Position-currentState.Position);
            //Console.WriteLine("x/y commands: {0}", new Vector2(xCommand,yCommand));
            
            //Console.WriteLine(xCommand.ToString() + " xCommand|yCommand: " + yCommand.ToString());
            
            
            double angularVCommand = thetaPID.Compute(currentOrientation, desiredOrientation, currentState.AngularVelocity, desiredState.AngularVelocity);

            if (OnUpdateErrors != null) {
                double xError = currentState.Position.X - desiredState.Position.X;
                double yError = currentState.Position.Y - desiredState.Position.Y;
                double thetaError = currentOrientation - desiredOrientation;
                OnUpdateErrors(xError, yError, thetaError);
            }

            //converts from x speed, y speed and angular speed to wheel speeds
            return convert(xCommand, yCommand, angularVCommand, currentOrientation);
        }

        private double angleCheck(double angle) {
            if (angle < 0)
                return angleCheck(angle + 2 * Math.PI);
            else if (angle < 2 * Math.PI)
                return angle;
            else
                return angleCheck(angle - 2 * Math.PI);
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


            int lf = (int)  (sing*lateral + cosg * forward - wheelR * angularV);
            int rf = (int)  -(sing*lateral - cosg*forward - wheelR*angularV);
            int lb = (int) (-sing*lateral + cosg*forward - wheelR*angularV);
            int rb = (int) -(-sing*lateral - cosg *forward - wheelR * angularV);

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

            private DOF_Constants constants;
            public DOF_Constants PIDConstants { get { return constants; } }
                        
            private double error = 0;
            private double oldError = 0;
            private double Ierror = 0;
            private double Derror = 0;

            private DOFType dofType;
            private int robotID;

            /// <summary>
            /// Initialize parameters for a robot from a text file of parameters
            /// </summary>
            /// <param name="robotID">ID of the current robot</param>
            /// <param name="DOF">DOF the degree of freedom this is for either X Y or THETA</param>
            public DOF_Numbers(int robot_id, DOFType dof){

                //pull out values for constants from a text file
                dofType = dof;
                robotID=robot_id;
                ReloadConstants();
              
            }
        
            //computes a command for either the x velocity the y velocity or the angular velocity.
            //assume orientations are between 0 and 2 pi.
            //it usese the PID constants stored in this instance and incorporates both the velocity and current valud i.e. x and dx/dt
            public double Compute(double current, double desired, double current_dot, double desired_dot){

                double diff = desired - current;
                if (dofType == DOFType.THETA) {//need to pick the smallest angle
                    if (diff > Math.PI)
                        diff = -(2 * Math.PI - diff);
                    else if (diff < -Math.PI)
                        diff = 2 * Math.PI + diff;
                }
                    
                error = constants.ALPHA*(diff)+(1-constants.ALPHA)*(desired_dot-current_dot);
                
                
                Ierror = Ierror+error;
                Derror = error - oldError;
                oldError = error;
                
                return desired + constants.P*error + constants.I*Ierror + constants.D*Derror;
            }

            //loads/reloads all constants from the file and sets I error to 0 to aid debugging
            public void ReloadConstants() {
                Ierror = 0;
                Constants.Load();
                constants.P = Constants.get<double>("control", "P_" + dofType.ToString() + "_" + robotID.ToString());
                constants.I = Constants.get<double>("control", "I_" + dofType.ToString() + "_" + robotID.ToString());
                constants.D = Constants.get<double>("control", "D_" + dofType.ToString() + "_" + robotID.ToString());
                constants.ALPHA = Constants.get<double>("control", "ALPHA_" + dofType.ToString() + "_" + robotID.ToString());
            }

            /// <summary>
            /// Can be used to programatically update the set of constants.
            /// </summary>
            public void UpdateConstants(DOF_Constants _constants) {
                Ierror = 0;

                constants = _constants;
            }
        }
    }
}
