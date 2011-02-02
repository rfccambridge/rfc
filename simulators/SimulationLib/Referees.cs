using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using Robocup.CoreRobotics;

namespace Robocup.Simulation
{
    public delegate void GoalScored();

    public interface VirtualRef : IReferee
    {
        /// <summary>
        /// Takes as an argument the field state (in the form of a predictor object), and a way to move the ball;
        /// should move the ball and store (for retrieval through IReferee methods) the play type to be run.
        /// </summary>
        /// <param name="predictor">The IPredictor object that provides field state information</param>
        void RunRef(IPredictor predictor);
        event GoalScored GoalScored;
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

#if FALSE
        /// <summary>
        /// The number of consecutive rounds during which play should have been restarted;
        /// </summary>
        int should_restart = 0;
#endif
        public void RunRef(IPredictor predictor)
        {
            BallInfo ball = predictor.GetBall();
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
//TODO: Abstract away or remove completely (not working anyway)
#if FALSE
            bool immobile = false;
            if (ball.Velocity.magnitudeSq() < .003 * .003)
                immobile = true;

            // find robot-ball collisions
            {
                const double threshsq = .35 * .35;
                int numTooClose = 0;
                foreach (RobotInfo info in predictor.GetRobots())
                {
                    double dist = ball.Position.distanceSq(info.Position);
                    if (dist < threshsq)
                        numTooClose++;
                }

                if (numTooClose >= 4)
                    immobile = true;
            }

            // increment immobile count
            if (immobile)
            {
                // reset if stuck too long
                should_restart++;
                /*if (should_restart > 200)
                {
                    Console.WriteLine("STUCK !!!!!!!!!!!!!!!!!!!!!!!!!");
                    string x = Console.ReadLine();
                    Console.WriteLine("BS: [ " + x + "]");
                    move_ball(new BallInfo(new Vector2(0, 0)));
                    should_restart = 0;
                }*/
            }
            else
                should_restart = 0;
#endif
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
