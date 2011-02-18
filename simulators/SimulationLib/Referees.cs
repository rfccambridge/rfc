using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using Robocup.CoreRobotics;

namespace Robocup.Simulation
{
    public delegate void GoalScored();
    public delegate void BallOut(Vector2 lastPosition);

    public interface VirtualRef : IReferee
    {
        /// <summary>
        /// Takes as an argument the field state (in the form of a predictor object), and a way to move the ball;
        /// should move the ball and store (for retrieval through IReferee methods) the play type to be run.
        /// </summary>
        /// <param name="predictor">The IPredictor object that provides field state information</param>
        void RunRef(IPredictor predictor);
        event GoalScored GoalScored;
        event BallOut BallOut;
    }
    public class SimpleReferee : VirtualRef
    {
        static double FIELD_WIDTH;
        static double FIELD_HEIGHT;
        static double FIELD_XMIN;
        static double FIELD_XMAX;
        static double FIELD_YMIN;
        static double FIELD_YMAX;
        static double GOAL_WIDTH;
        static double GOAL_HEIGHT;

        MulticastRefBoxSender sender;

        public SimpleReferee()
        {
            sender = new MulticastRefBoxSender();

            LoadConstants();             
        }

        public event GoalScored GoalScored;
        public event BallOut BallOut;

        public void LoadConstants()
        {
            // field drawing
            FIELD_WIDTH = Constants.get<double>("plays", "FIELD_WIDTH");
            FIELD_HEIGHT = Constants.get<double>("plays", "FIELD_HEIGHT");

            FIELD_XMIN = -FIELD_WIDTH / 2;
            FIELD_XMAX = FIELD_WIDTH / 2;
            FIELD_YMIN = -FIELD_HEIGHT / 2;
            FIELD_YMAX = FIELD_HEIGHT / 2;

            GOAL_WIDTH = Constants.get<double>("plays", "GOAL_WIDTH");
            GOAL_HEIGHT = Constants.get<double>("plays", "GOAL_HEIGHT");
        }

        public void RunRef(IPredictor predictor)
        {
            BallInfo ball = predictor.GetBall();
            if (ball == null)
                return;

            // Ball left the field
            if (ball.Position.X >= FIELD_XMAX || ball.Position.X <= FIELD_XMIN ||
                ball.Position.Y >= FIELD_YMAX || ball.Position.Y <= FIELD_YMIN)
            {

                // Check for goal
                if ((ball.Position.X <= FIELD_XMIN && ball.Position.X >= FIELD_XMIN - GOAL_WIDTH &&
                    Math.Abs(ball.Position.Y) <= GOAL_HEIGHT / 2) ||
                    (ball.Position.X >= FIELD_XMAX && ball.Position.X <= FIELD_XMAX + GOAL_WIDTH &&
                    Math.Abs(ball.Position.Y) <= GOAL_HEIGHT / 2)
                    )
                {
                    if (GoalScored != null)
                        GoalScored();
                    return;
                }

                // If no goal, ball is simply out
                if (BallOut != null)
                    BallOut(ball.Position);
            }
        }

        public PlayType GetCurrentPlayType()
        {
            return PlayType.NormalPlay;
        }

        public void Connect(string host, int port)
        {
            sender.Connect(host, port);
        }

        public void Disconnect()
        {
            sender.Disconnect();
        }

        public void SendCommand(char command)
        {
            sender.SendCommand(command);
        }
    }
}
