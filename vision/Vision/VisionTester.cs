using System;
using System.Collections.Generic;
using System.IO;
using Robocup.Core;
using VisionCamera;

namespace Vision
{
	/// <summary>
	/// Implements a framework to test whether frames actually contain what they should.
	/// Current implementation tests for our robots with given IDs and/or ball.
	/// </summary>
	public class VisionTester
	{
		private const string WORK_DIR = "../../resources/vision/";
		private TextWriter resultWriter;
		private TextWriter badWriter;
        private Team TEST_TEAM;
		
		// Maximum distance that ball can travel between two frames
		// distance greater than this considered error
		private const double MAX_BALL_DIST_SQ = 0.25;
		// Minimal distance from ball to robot center so that it's not found incorrectly inside the robot
		private const double BALL_WITHIN_ROBOT_DIST_SQ = 0.0049;

		private int frameCount;
		private int badFrames;
		private Vector2 oldBallPosition;
		
		public bool TestBall
		{
			get { return testBall; }
			set { testBall = value; }
		}
		private bool testBall;
		
		public List<int> RobotIDs
		{
			get { return robotIDs; }
		}
		private readonly List<int> robotIDs;


		public VisionTester(Team team)
		{
            TEST_TEAM = team;
			robotIDs = new List<int>();
			Reset();
		}

		public VisionTester(Team _team, bool _testBall, List<int> _robotIDs) : 
			this(_team)
		{
			testBall = _testBall;
			robotIDs = _robotIDs;
		}

		public void Reset()
		{
			frameCount = 0;
			badFrames = 0;
			oldBallPosition = null;

			if(resultWriter != null)
				resultWriter.Close();
			resultWriter = new StreamWriter(WORK_DIR + "test_result.txt", false); // rewrite
			
			if(badWriter != null)
				badWriter.Close();
			badWriter = new StreamWriter(WORK_DIR + "test_bad.txt", false); // rewrite
		}

		public void TestFrame(VisionMessage visionMessage, ICamera camera)
		{
			bool good = true;

			//We want to find each needed robot only once
			foreach (int id in robotIDs)
			{
				int occurence = 0;
                foreach (VisionMessage.RobotData robot in visionMessage.Robots) {
                    // Ignore team color that is not being tested
                    if ((robot.Team == VisionMessage.Team.YELLOW ? Team.YELLOW : Team.BLUE) != TEST_TEAM) continue;
                    
                    if (robot.ID == id) occurence++;
                }

				if (occurence != 1)
				{
					good = false;
					break;
				}
			}

			foreach (VisionMessage.RobotData robot in visionMessage.Robots)
			{
                // Ignore team color that is not being tested
                if ((robot.Team == VisionMessage.Team.YELLOW ? Team.YELLOW : Team.BLUE) != TEST_TEAM) continue;

				if (!robotIDs.Contains(robot.ID))
					good = false;

				if (visionMessage.BallPosition != null)
					if (robot.Position.distanceSq(visionMessage.BallPosition) <= BALL_WITHIN_ROBOT_DIST_SQ)
					{
						good = false;
						visionMessage.BallPosition = null;
						break;
					}
			}

			if (testBall)
			{
				if (visionMessage.BallPosition == null) good = false;
				else if (oldBallPosition != null)
					if (oldBallPosition.distanceSq(visionMessage.BallPosition) > MAX_BALL_DIST_SQ)
					{
						good = false;
						visionMessage.BallPosition = null;
					}
			}



			oldBallPosition = visionMessage.BallPosition;
			frameCount++;
			if (!good) badFrames++;

			if (!good)
			{
				//If we're running from a saved sequence, just save the frame number
				SeqCamera seqCamera = camera as SeqCamera;
				if (seqCamera != null)
					badWriter.WriteLine(seqCamera.Frame);
				
				else
				//Otherwise, capture and save_image
				{
					//TODO: implement!!!
				}
			}
			
		}

		public void FinishTest()
		{
			badWriter.Close();
			
			resultWriter.WriteLine("Test finished at: {0}", DateTime.Now);
			resultWriter.WriteLine("All frames: {0}", frameCount);
			resultWriter.WriteLine("Bad: {0}", badFrames);
			resultWriter.Close();
		}
	}
}
