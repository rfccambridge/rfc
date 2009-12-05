using System;
using Robocup.Core;
using CSML;

namespace Robocup.MotionControl
{
	
	// Implements model-based feedback (TODO: cite doc with actual model)
	// Essential job is to calculate command based on current & desired (position, orientation, velocity, angular velocity)
	public class ModelFeedback
	{
		public ModelFeedback()
		{
			LoadConstants();
		}

		//4x6 GainMatrix that converts error vector to command
		public Matrix GainMatrix = null;

		public void LoadConstants()
		{
			GainMatrix = new Matrix(Constants.get<string>("control","GAIN_MATRIX"));
			
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
			Matrix errorVector = new Matrix(6,1);
			
			errorVector[0] = new Complex(currentState.Position.X - desiredState.Position.X);
			errorVector[1] = new Complex(currentState.Position.Y - desiredState.Position.Y);
			errorVector[2] = new Complex(currentState.Orientation - desiredState.Orientation);

			errorVector[3] = new Complex(currentState.Velocity.X - desiredState.Velocity.X);
			errorVector[4] = new Complex(currentState.Velocity.Y - desiredState.Velocity.Y);
			errorVector[5] = new Complex(currentState.AngularVelocity - desiredState.AngularVelocity);

			Matrix commandVector = errorVector * GainMatrix;
			//XXX: Ask if we need to do any scaling here! (Hunter mentions voltages which may be different than current wheelspeeds)
			WheelSpeeds command = new WheelSpeeds(Convert.ToInt32(commandVector[0].Re), Convert.ToInt32(commandVector[0].Re), 
				Convert.ToInt32(commandVector[0].Re), Convert.ToInt32(commandVector[0].Re));

			return command;
		}
	}
}
