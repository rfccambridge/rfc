using System;
using Robocup.Core;
using CSML;

namespace Robocup.MotionControl
{
	// Implements model-based feedback
	// Essential job is to calculate command based on current & desired (position, orientation, velocity, angular velocity)
	public class ModelFeedback
	{
        // scaling factor applied to the matrix
        static int NUM_ROBOTS = Constants.get<int>("default", "NUM_ROBOTS");
        double [] SPEED_SCALING_FACTORS = new double[NUM_ROBOTS];

        private double SPEED_SCALING_FACTOR_ALL;

		public ModelFeedback()
		{
			LoadConstants();
		}

		//4x6 GainMatrix that converts error vector to command
		public Matrix GainMatrix = null;

		public void LoadConstants()
		{
            SPEED_SCALING_FACTOR_ALL = Constants.get<double>("control", "SPEED_SCALING_FACTOR_ALL");

            for (int i = 0; i < NUM_ROBOTS; i++)
            {
                SPEED_SCALING_FACTORS[i] = Constants.get<double>("control", "SPEED_SCALING_FACTOR_" + i.ToString());
            }

			GainMatrix = new Matrix(Constants.get<string>("control","GAIN_MATRIX"));
            GainMatrix *= Constants.get<double>("control", "GAIN_MATRIX_SCALE");
			
			if(GainMatrix.ColumnCount != 6 || GainMatrix.RowCount != 4)
				throw new ApplicationException("Invalid dimensoins of GAIN_MATRIX in control.txt!");
		}

		/// <summary>
		/// Simply multiplies the error vector by the gain matrix; Real work done in designing that matrix
		/// </summary>
		/// <param name="currentState"></param>
		/// <param name="desiredState"></param>
		/// <returns></returns>
		public WheelSpeeds ComputeWheelSpeeds(RobotInfo currentState, RobotInfo desiredState)
		{
            //Smallest turn algorithm
            double dTheta = desiredState.Orientation - currentState.Orientation;

            //Map dTheta to the equivalent angle in [-PI,PI]
            dTheta = dTheta % (2 * Math.PI);
            if (dTheta > Math.PI) dTheta -= 2 * Math.PI;
            if (dTheta < -Math.PI) dTheta += 2 * Math.PI;

            double sTheta = Math.Sin(currentState.Orientation);
            double cTheta = Math.Cos(currentState.Orientation);

            //Convert to a reference frame centered at the robot, where wheels are diagonally oriented.
            //When the robot is facing RIGHT, the wheels are numbered 1,2,3,4 COUNTERCLOCKWISE, beginning with the
            //lower right wheel (the front-right wheel from the robot's perspective).
            //Sending POSITIVE speeds to all wheels causes the robot to go COUNTERCLOCKWISE
            double[,] globalToLocal = new double[6, 6]{{ cTheta, sTheta,   0,      0,      0,   0},
                                                       {-sTheta, cTheta,   0,      0,      0,   0},
                                                       {      0,      0,   1,      0,      0,   0},
                                                       {      0,      0,   0, cTheta, sTheta,   0},
                                                       {      0,      0,   0,-sTheta, cTheta,   0},
                                                       {      0,      0,   0,      0,      0,   1}};
            Matrix globalToLocalMatrix = new Matrix(globalToLocal);
            
			Matrix errorVector = new Matrix(6,1);
			errorVector[1] = new Complex(desiredState.Position.X - currentState.Position.X);
			errorVector[2] = new Complex(desiredState.Position.Y - currentState.Position.Y);
            errorVector[3] = new Complex(dTheta);
			errorVector[4] = new Complex(desiredState.Velocity.X - currentState.Velocity.X);
			errorVector[5] = new Complex(desiredState.Velocity.Y - currentState.Velocity.Y);
			errorVector[6] = new Complex(desiredState.AngularVelocity - currentState.AngularVelocity);

            Matrix localError = globalToLocalMatrix * errorVector;

            //Multiply by the gain matrix, which specifies the wheel speeds that should be given in response to each
            //error component.
            Matrix commandVector = GainMatrix * localError;

            //Scale the speeds, both globally and per-robot.
            commandVector = SPEED_SCALING_FACTOR_ALL * SPEED_SCALING_FACTORS[currentState.ID] * commandVector;

            //Build and return the command
            WheelSpeeds command = new WheelSpeeds(
                Convert.ToInt32(commandVector[1].Re), 
                Convert.ToInt32(commandVector[2].Re), 
				Convert.ToInt32(commandVector[3].Re), 
                Convert.ToInt32(commandVector[4].Re));

			return command;
		}
	}
}
