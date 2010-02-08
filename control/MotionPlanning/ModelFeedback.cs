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
            //get angles between 0 and two pi
            double thetaGoal = desiredState.Orientation;
            double thetaCurr = currentState.Orientation;

            /*Console.Write("\before ncurrent angle: ");
            Console.WriteLine(thetaCurr);
            Console.Write("desired angle: ");
            Console.WriteLine(thetaGoal);
             */

            while (thetaCurr >= 2 * Math.PI) {
                thetaCurr -= 2 * Math.PI;
            }
            while (thetaCurr < 0) {
                thetaCurr += 2 * Math.PI;
            }

            while (thetaGoal >= 2 * Math.PI) {
                thetaGoal -= 2 * Math.PI;
            }
            while (thetaGoal < 0) {
                thetaGoal += 2 * Math.PI;
            }

            if (thetaGoal - thetaCurr >= Math.PI)
                thetaGoal = thetaGoal - 2 * Math.PI;
            else if (thetaGoal - thetaCurr <= -Math.PI)
                thetaGoal = thetaGoal + 2 * Math.PI;


            //Needed to change wheelspeed convention - from {} to {}
            double[,] permutation = new double[4,4]{{0,0,0,1},{1,0,0,0},{0,0,1,0}, {0,0,0,1}};
            Matrix permutationMatrix = new Matrix(permutation);

            double sTheta = Math.Sin(currentState.Orientation);
            double cTheta = Math.Cos(currentState.Orientation);

            double[,] globalToLocal = new double[6, 6]{{cTheta, sTheta, 0,0,0,0},
                                                        {-sTheta, cTheta,0,0,0,0},
                                                        {0,0,1,0,0,0},
                                                        {0,0,0,cTheta,sTheta,0},
                                                        {0,0,0,-sTheta,cTheta,0},
                                                        {0,0,0,0,0,1}};
            Matrix globalToLocalMatrix = new Matrix(globalToLocal);
            
			Matrix errorVector = new Matrix(6,1);
			
			errorVector[1] = new Complex(currentState.Position.X - desiredState.Position.X);
			errorVector[2] = new Complex(currentState.Position.Y - desiredState.Position.Y);
            errorVector[3] = new Complex(thetaCurr - thetaGoal); //currentState.Orientation - desiredState.Orientation);  //Hack to test stuf!!!!!!!!!!!
            
			errorVector[4] = new Complex(currentState.Velocity.X - desiredState.Velocity.X);
			errorVector[5] = new Complex(currentState.Velocity.Y - desiredState.Velocity.Y);
			errorVector[6] = new Complex(currentState.AngularVelocity - desiredState.AngularVelocity);

            Matrix localError = globalToLocalMatrix * errorVector;

            //if (currentState.Velocity.magnitudeSq() > 0.25)
                //GainMatrix = GainMatrix;

            //Matrix commandVector = GainMatrix * localError * 50; // 185
            
            
            Matrix commandVector = GainMatrix * localError;

            //double[] _command = new double[4] { 0, 0, 0, 30 };
            //Matrix commandVector = new Matrix(_command);
            
            //commandVector = permutationMatrix * commandVector; 

			//XXX: Ask if we need to do any scaling here! (Hunter mentions voltages which may be different than current wheelspeeds)
			WheelSpeeds command = new WheelSpeeds(-Convert.ToInt32(commandVector[4].Re), -Convert.ToInt32(commandVector[1].Re), 
				-Convert.ToInt32(commandVector[2].Re), -Convert.ToInt32(commandVector[3].Re));
            //Console.WriteLine(commandVector);

			return command;
		}
	}
}
