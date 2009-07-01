using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;

namespace Robocup.Simulation
{
    public interface VirtualRef : IReferee
    {
        /// <summary>
        /// Takes as an argument the field state (in the form of a predictor object), and a way to move the ball;
        /// should move the ball and store (for retrieval through IReferee methods) the play type to be run.
        /// </summary>
        /// <param name="predictor">The IPredictor object that provides field state information</param>
        /// <param name="move_ball">A delegate that lets the ref move the ball around</param>
        void RunRef(IPredictor predictor, Action<BallInfo> move_ball);
    }
    public class SimpleReferee : VirtualRef
    {
        int _ourGoals = 0, _theirGoals = 0;

        static double FIELD_WIDTH;
        static double FIELD_HEIGHT;
        static double FIELD_XMIN;
        static double FIELD_XMAX;
        static double FIELD_YMIN;
        static double FIELD_YMAX;
        static double GOAL_WIDTH;
        static double GOAL_HEIGHT;

        public SimpleReferee()
        {
             // field drawing
            FIELD_WIDTH = Constants.get<double>("plays", "FIELD_WIDTH");
            FIELD_HEIGHT = Constants.get<double>("plays", "FIELD_HEIGHT");
            
            FIELD_XMIN = -FIELD_WIDTH / 2;
            FIELD_XMAX = FIELD_WIDTH / 2;
            FIELD_YMIN = -FIELD_HEIGHT / 2;
            FIELD_YMAX = FIELD_HEIGHT/2;

            GOAL_WIDTH = Constants.get<double>("plays","GOAL_WIDTH");
            GOAL_HEIGHT = Constants.get<double>("plays", "GOAL_HEIGHT");
        }

        private void goalScored(bool scoredByLeftTeam)
        {
            if (scoredByLeftTeam)
                _ourGoals++;
            else
                _theirGoals++;
        }

        /// <summary>
        /// The number of consecutive rounds during which play should have been restarted;
        /// </summary>
        int should_restart = 0;
        public void RunRef(IPredictor predictor, Action<BallInfo> move_ball)
        {
            BallInfo ball = predictor.GetBall();
            // Check for goal

            if ((ball.Position.X <= FIELD_XMIN && ball.Position.X >= FIELD_XMIN - GOAL_WIDTH &&
                Math.Abs(ball.Position.Y) <= GOAL_HEIGHT / 2) ||
                (ball.Position.X >= FIELD_XMAX && ball.Position.X <= FIELD_XMAX + GOAL_WIDTH &&
                Math.Abs(ball.Position.Y) <= GOAL_HEIGHT / 2)
                )
            {
                Console.WriteLine("Goal Ball reset!");
                goalScored(ball.Position.X < 0);
                move_ball(new BallInfo(new Vector2(0, 0)));
                return;
            }

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
        }

        public PlayTypes GetCurrentPlayType()
        {
            return PlayTypes.NormalPlay;
        }
    }
}
