using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;

namespace SoccerSim
{
    class SimpleReferee : VirtualRef
    {
        int _ourGoals = 0, _theirGoals = 0;

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
        public void RunRef(PhysicsEngine physics_engine, Action<BallInfo> move_ball)
        {
            BallInfo ball = physics_engine.getBallInfo();
            // Check for goal
            if (Math.Abs(ball.Position.Y) <= .35 && Math.Abs(ball.Position.X) >= 2.4)
            {
                goalScored(ball.Position.X > 0);
                move_ball(new BallInfo(new Vector2(0, 0)));
                return;
            }

            bool immobile = false;
            if (ball.Velocity.magnitudeSq() < .01 * .01)
                immobile = true;

            // find robot-ball collisions
            {
                const double threshsq = .35 * .35;
                int numTooClose = 0;
                foreach (RobotInfo info in physics_engine.getOurTeamInfo())
                {
                    double dist = ball.Position.distanceSq(info.Position);
                    if (dist < threshsq)
                        numTooClose++;
                }
                foreach (RobotInfo info in physics_engine.getTheirTeamInfo())
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
                if (should_restart > 200)
                {
                    move_ball(new BallInfo(new Vector2(0, 0)));
                    should_restart = 0;
                }
            }
            else
                should_restart = 0;
        }
    }
}
