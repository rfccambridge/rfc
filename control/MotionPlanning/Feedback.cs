using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Core;
using Robocup.CoreRobotics;

using System.IO;

using System.Drawing;

namespace Robocup.MotionControl {
    /// <summary>
    /// Represents a PID loop to stabilize motion planning along a path
    /// There needs to be one of these for each robot
    /// </summary>
    class Feedback {
        // PID parameters
        DOF_Numbers xPID;
        DOF_Numbers yPID;
        DOF_Numbers thetaPID;

        const double wheelR = 0.0782828; //distance from the center of the robot to the wheels in meters


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
        public WheelSpeeds computeWheelSpeeds(RobotInfo currentState, RobotInfo desiredState){
            //runs the math for the PID
            double xCommand = xPID.compute(currentState.Position.X, desiredState.Position.X, currentState.Velocity.X, desiredState.Velocity.X);
            double yCommand = yPID.compute(currentState.Position.Y, desiredState.Position.Y, currentState.Velocity.Y, desiredState.Velocity.Y);
            //to ensure that orientations are between 0 and 2 pi.
            double currentOrientation = angleCheck(currentState.Orientation);
            double desiredOrientation = angleCheck(desiredState.Orientation);
            
            //Console.WriteLine(currentState.Position.X.ToString() + " Current X|Desired: " + desiredState.Position.X.ToString());
            //Console.WriteLine(currentState.Position.Y.ToString() + " Current Y|Desired: " + desiredState.Position.Y.ToString());
            //Console.WriteLine(currentOrientation.ToString() + " Current Theta|Desired: " + desiredOrientation.ToString());
            
            //Console.WriteLine(xCommand.ToString() + " xCommand|yCommand: " + yCommand.ToString());
            
            
            double angularVCommand = thetaPID.compute(currentOrientation, desiredOrientation, currentState.AngularVelocity, desiredState.AngularVelocity);
            
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
            xPID.reloadConstants();
            yPID.reloadConstants();
            thetaPID.reloadConstants();
        }
            

        //enum for the three different degrees of freedom the PID loops run for x, y and theta
        enum DOFType { X, Y, THETA };

        private class DOF_Numbers{

            //these should all be initialized to their tuned value, or auto tuned
            //they should also all be positive values except maybe D            

            private double P;
            private double I;
            private double D;
            private double ALPHA; // must be between zero and 1
            
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
                reloadConstants();
              
            }
        
            //computes a command for either the x velocity the y velocity or the angular velocity.
            //assume orientations are between 0 and 2 pi.
            //it usese the PID constants stored in this instance and incorporates both the velocity and current valud i.e. x and dx/dt
            public double compute(double current, double desired, double current_dot, double desired_dot){

                double diff = desired - current;
                if (dofType == DOFType.THETA) {//need to pick the smallest angle
                    if (diff > Math.PI)
                        diff = -(2 * Math.PI - diff);
                    else if (diff < -Math.PI)
                        diff = 2 * Math.PI + diff;
                }
                    
                error = ALPHA*(diff)+(1-ALPHA)*(desired_dot-current_dot);
                
                
                Ierror = Ierror+error;
                Derror = error - oldError;
                oldError = error;
                
                return P*error+I*Ierror + D*Derror;
            }

            //loads/reloads all constants from the file and sets I error to 0 to aid debugging
            public void reloadConstants() {
                Ierror = 0;
                Constants.Load();
                P = Constants.get<double>("control", "P_" + dofType.ToString() + "_" + robotID.ToString());
                
                I = Constants.get<double>("control", "I_" + dofType.ToString() + "_" + robotID.ToString());
                D = Constants.get<double>("control", "D_" + dofType.ToString() + "_" + robotID.ToString());
                ALPHA = Constants.get<double>("control", "ALPHA_" + dofType.ToString() + "_" + robotID.ToString());
            }
        }
    }
}
