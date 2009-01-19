using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Core;
using Robocup.CoreRobotics;

using System.Drawing;

namespace Robocup.MotionControl {
    /// <summary>
    /// Represents a PID to stabilize motion planning along a path
    /// </summary>
    /// 
    
    class Feedback {
        /// <summary>
        /// Given a robot and a desired position along a path, returns the desired wheelspeeds to
        /// return to the path
        /// </summary>
        /// <param name="currentPosition">Current position of the robot</param>
        /// <param name="desiredPosition">Nearest waypoint to the robot along the desired path</param>
        /// <returns></returns>

        DOF_Numbers xPID = new DOF_Numbers();
        DOF_Numbers yPID = new DOF_Numbers();
        DOF_Numbers thetaPID = new DOF_Numbers();

        const double wheelR = 0.0782828; //distance from the center of the robot to the wheels in meters
        
        public WheelSpeeds computeWheelSpeeds(RobotInfo currentState, RobotInfo desiredState){
            double xCommand = xPID.compute(currentState.Position.X, desiredState.Position.X, currentState.Velocity.X, desiredState.Velocity.X);
            double yCommand = yPID.compute(currentState.Position.X, desiredState.Position.X, currentState.Velocity.X, desiredState.Velocity.X);
            double angularVCommand = thetaPID.compute(currentState.Orientation, desiredState.Orientation, currentState.AngularVelocity, desiredState.AngularVelocity);
            return convert(xCommand, yCommand, angularVCommand, currentState.Orientation);
        }

        //I assume the x command is effectively in m/s, so r the radius of the wheels from the center of
        //the robot is in meters
        private WheelSpeeds convert(double xCommand, double yCommand, double angularV, double theta){
            //change from the x and y of the field to forward and lateral(right is positive) used below
            double forward = Math.Cos(theta)*xCommand+Math.Sin(theta)*yCommand;
            double lateral = Math.Sin(theta)*xCommand-Math.Cos(theta)*yCommand;
        
            //computed here to save typing, since used 4 times
            double sing = Math.Sin(Math.PI/180*41);
            double cosg = Math.Cos(Math.PI/180*41);
        
            //wheel one is the front right wheel  wheel 2 is the back right wheel, and so on around the the robot clockwise
            int rf = (int) (-sing*forward + cosg*lateral + wheelR*angularV);
            int rb = (int) (sing*forward + cosg*lateral + wheelR*angularV);
            int lb = (int) (-sing*forward - cosg*lateral + wheelR*angularV);
            int lf = (int) (sing*forward - cosg*lateral + wheelR*angularV);
            return new WheelSpeeds(lf, rf, lb, rb);
            //Note somewhere we need to check and ensure that wheel speeds being 
            //sent do not exceed maximum values allowed by the protocol.
        }

        private class DOF_Numbers{

            //these should all be initialized to their tuned value, or auto tuned
            //they should also all be positive values except maybe D
            private double P = 30;
            private double I = 5;
            private double D = 2;
            private double alpha = .3; // must be between zero and 1
            
            private double error = 0;
            private double oldError = 0;
            private double Ierror = 0;
            private double Derror = 0;
            
            //DOF is whether it's x or y or theta, which may all have seperate constants,
            //although x and y will probably end up being the same
            public DOF_Numbers() { }

            public DOF_Numbers(double robotID, String DOF){
                //pull out values for constants from a text file
            }
        
            //computes a command for either the x velocity the y velocity or the angular velocity.
            //it usese the PID constants stored in this instance and incorporates both the velocity and current valud i.e. x and dx/dt
            public double compute(double current, double desired, double current_dot, double desired_dot){
                error = alpha*(desired-current)+(1-alpha)*(desired_dot-current_dot);
                Ierror = Ierror+error;
                Derror = error - oldError;
                oldError = error;
                
                return P*error+I*Ierror + D*Derror;
            }
        }
    }
}
