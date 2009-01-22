using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Robocup.MessageSystem;
using Robocup.Core;
using Robocup.CoreRobotics;
using Robocup.ControlForm;

namespace SimplePathFollower
{
	public partial class FollowerForm : Form
	{
		int MESSAGE_SENDER_PORT = Constants.get<int>("ports", "VisionDataPort");
		private MessageReceiver<VisionMessage> _vision;
		private bool visionConnected;
		private bool controlConnected;
		private PathFollower pf;
		private BasicPredictor predictor;
		
		public FollowerForm()
		{
			InitializeComponent();
			visionConnected = false;
			controlConnected = false;

			pf = new PathFollower();
			pf.Init();
			predictor = (BasicPredictor) pf.Predictor;
		}

		private void BtnVision_Click(object sender, EventArgs e)
		{
			try
			{
				if (!visionConnected)
				{
					_vision = Robocup.MessageSystem.Messages.CreateClientReceiver<Robocup.Core.VisionMessage>(
						VisionHost.Text, MESSAGE_SENDER_PORT);
					_vision.MessageReceived += 
						new Robocup.MessageSystem.ReceiveMessageDelegate<VisionMessage>(
							handleVisionUpdate);

					VisionStatus.BackColor = Color.Green;
					BtnVision.Text = "Disconnect";
					visionConnected = true;
				}
				else
				{
					_vision.Close();
					VisionStatus.BackColor = Color.Red;
					BtnVision.Text = "Connect";
					visionConnected = false;
				}
			}
			catch (Exception except)
			{
				Console.WriteLine("Problem connecting to vision: " + except.ToString());
				Console.WriteLine(except.StackTrace);
			}
		}

		object predictor_lock = new object();
		private void handleVisionUpdate(VisionMessage msg)
		{
			String cameraName = "top_cam";

			List<RobotInfo> ours = new List<RobotInfo>();

			foreach (VisionMessage.RobotData robot in msg.OurRobots)
			{
				ours.Add(new RobotInfo(robot.Position, robot.Orientation, robot.ID));
			}

			List<RobotInfo> theirs = new List<RobotInfo>();
			foreach (VisionMessage.RobotData robot in msg.TheirRobots)
			{
				theirs.Add(new RobotInfo(robot.Position, robot.Orientation, robot.ID));
			}

			lock (predictor_lock)
			{
				predictor.updatePartOurRobotInfo(ours, cameraName);
				predictor.updatePartTheirRobotInfo(theirs, cameraName);
				if (msg.BallPosition != null)
				{
					Vector2 ballposition = new Vector2(2 + 1.01 * (msg.BallPosition.X - 2), msg.BallPosition.Y);
					predictor.updateBallInfo(new BallInfo(ballposition));
				}
			}
		}

		private void BtnControl_Click(object sender, EventArgs e)
		{
			try
			{
				if (!controlConnected)
				{
					if ((pf.Commander as RemoteRobots).start(ControlHost.Text))
					{
						ControlStatus.BackColor = Color.Green;
						BtnControl.Text = "Disconnect";
						controlConnected = true;
					}
				}
				else
				{
					(pf.Commander as RemoteRobots).stop();
					ControlStatus.BackColor = Color.Red;
					BtnControl.Text = "Connect";
					controlConnected = false;
				}
			}
			catch (Exception except)
			{
				Console.WriteLine(except.StackTrace);
			}
		}

		private void BtnStart_Click(object sender, EventArgs e)
		{
			List<Vector2> wpList = new List<Vector2>();
			wpList.Add(new Vector2(-0.5, -0.5));
			wpList.Add(new Vector2(0.5, -0.5));
			wpList.Add(new Vector2(0.5, 0.5));
			wpList.Add(new Vector2(-0.5, 0.5));

			pf.RobotID = 1;
			pf.Waypoints = wpList;
			pf.Follow();
		}
		
		private void BtnStop_Click(object sender, EventArgs e)
		{
			pf.Stop();
		}

	}
}
