using System;
using Robocup.Core;
using Robocup.Geometry;
using CSML;

namespace Robocup.MotionControl
{
	// Implements model-based feedback
	// Essential job is to calculate command based on current & desired (position, orientation, velocity, angular velocity)
	public class ModelFeedback
	{
        //4x6 GainMatrix that converts error vector to command
		public Matrix GAIN_MATRIX = null;

        // scaling factor applied to the matrix
        static int NUM_ROBOTS = ConstantsRaw.get<int>("default", "NUM_ROBOTS");
        double[] SPEED_SCALING_FACTORS = new double[NUM_ROBOTS]; //Per robot speed scaling
        private double SPEED_SCALING_FACTOR_ALL; //Global speed scaling
        private double WAYPOINT_DIST;

        private double fixedSpeedHackProp;

		public ModelFeedback()
		{
			LoadConstants();
            this.fixedSpeedHackProp = 0;
		}

        public void SetFixedSpeedHackProp(double prop)
        {
            fixedSpeedHackProp = prop;
        }


		public void LoadConstants()
		{
            SPEED_SCALING_FACTOR_ALL = ConstantsRaw.get<double>("control", "SPEED_SCALING_FACTOR_ALL");

            for (int i = 0; i < NUM_ROBOTS; i++)
            {
                SPEED_SCALING_FACTORS[i] = ConstantsRaw.get<double>("control", "SPEED_SCALING_FACTOR_" + i.ToString());
            }

			GAIN_MATRIX = new Matrix(ConstantsRaw.get<string>("control","GAIN_MATRIX"));
            GAIN_MATRIX *= ConstantsRaw.get<double>("control", "GAIN_MATRIX_SCALE");
			
			if(GAIN_MATRIX.ColumnCount != 6 || GAIN_MATRIX.RowCount != 4)
				throw new ApplicationException("Invalid dimensoins of GAIN_MATRIX in control.txt!");

            WAYPOINT_DIST = ConstantsRaw.get<double>("motionplanning", "WAYPOINT_DIST");
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

            //We do a hack here for now. For motion planners who actually like to set their waypoints denser or sparser
            //than tangentbug, they can use fixedSpeedHackProp to artificially extend the desired waypoint out to the
            //same distance that tangentbug would put it.
            //Properly, we should alter this code to not do something like this. Maybe we can convert fixedSpeedHackProp
            //into a "desired precision" variable, where setting the desired precision interpolates between just moving
            //at full speed and actually using the gain matrix for precision placement.
            Vector2 dPos = desiredState.Position - currentState.Position;
            if(fixedSpeedHackProp > 0)
            {
                double magnitude = dPos.magnitude();
                if (desiredState.Velocity != Vector2.ZERO)
                { dPos = dPos.normalizeToLength(fixedSpeedHackProp * WAYPOINT_DIST + (1 - fixedSpeedHackProp) * magnitude); }
            }

			Matrix errorVector = new Matrix(6,1);
            errorVector[1] = new Complex(dPos.X);
            errorVector[2] = new Complex(dPos.Y);
            errorVector[3] = new Complex(dTheta);
			errorVector[4] = new Complex(desiredState.Velocity.X - currentState.Velocity.X);
			errorVector[5] = new Complex(desiredState.Velocity.Y - currentState.Velocity.Y);
			errorVector[6] = new Complex(desiredState.AngularVelocity - currentState.AngularVelocity);

            Matrix localError = globalToLocalMatrix * errorVector;

            //Multiply by the gain matrix, which specifies the wheel speeds that should be given in response to each
            //error component.
            Matrix commandVector = GAIN_MATRIX * localError;

            //Scale the speeds, both globally and per-robot.
            commandVector = SPEED_SCALING_FACTOR_ALL * SPEED_SCALING_FACTORS[currentState.ID] * commandVector;

            //Build and return the command
            return new WheelSpeeds(
                commandVector[1].Re, 
                commandVector[2].Re, 
				commandVector[3].Re, 
                commandVector[4].Re);
		}
	}
}
