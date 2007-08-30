using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

using Robocup.Constants;
using Robocup.Infrastructure;
using RobocupPlays;
using Robocup.CoreRobotics;


namespace SoccerSim
{
    // make this load/instantiate from a text file
    public class SimEngine
    {
        const float balldecay = .98f;
        private const float ballbounce = .01f, collisionradius = .12f;
        const int TEAMSIZE = 5;

        private float ballVx, ballVy;
        int _ourGoals = 0, _theirGoals = 0;
        int _ballImmobile = 0;

        FieldState _state;

        volatile bool running = false;
        int _sleepTime;
        Thread worker;
        int counter = 0;

        SoccerSim _parent;

        public SimEngine(FieldState fieldState, SoccerSim parent)
        {
            ballVx = 0.0f;
            ballVy = 0.0f;
            _ourGoals = 0;
            _theirGoals = 0;
            _state = fieldState;

            _parent = parent;
        }
        
        #region Simulation

        private void goalScored(bool scoredByLeftTeam)
        {
            if (scoredByLeftTeam)
                _ourGoals++;
            else
                _theirGoals++;
        }

        public void step()
        {
            BallInfo ball = _state.getBallInfo();
            // run one step of tester

            // update ball location
            Vector2 newballlocation = new Vector2(ball.Position.X + ballVx, ball.Position.Y + ballVy);

            // check for collisions ball-robot, update ball position
            bool collided = false;
            foreach (RobotInfo r in _state.getTheirTeamInfo())
            {
                Vector2 location = r.Position;
                if (newballlocation.distanceSq(location) <= collisionradius * collisionradius)
                {
                    collided = true;
                    ballVx = ballbounce * ((ball.Position - location).normalize().X);
                    ballVy = ballbounce * ((ball.Position - location).normalize().Y);
                    newballlocation = location + .13f * (ball.Position - location).normalize();
                    break;
                }
            }

            // check for collisions ball-robot, in state.getOurTeamInfo(), update ball position
            if (!collided)
            {
                foreach (RobotInfo r in _state.getOurTeamInfo())
                {
                    Vector2 location = r.Position;
                    if (newballlocation.distanceSq(location) <= collisionradius * collisionradius)
                    {
                        collided = true;
                        ballVx = ballbounce * ((ball.Position - location).normalize().X);
                        ballVy = ballbounce * ((ball.Position - location).normalize().Y);
                        newballlocation = location + .13f * (ball.Position - location).normalize();
                        break;
                    }
                }
            }

            // do wall boundaries
            if (!collided)
            {
                Vector2 ballPos = ball.Position;
                if (ballPos.X < -2.45)
                    ballVx = Math.Abs(ballVx);
                else if (ballPos.X > 2.45)
                    ballVx = -Math.Abs(ballVx);
                if (ballPos.Y < -1.7)
                    ballVy = Math.Abs(ballVy);
                else if (ballPos.Y > 1.7)
                    ballVy = -Math.Abs(ballVy);
                newballlocation = new Vector2(ball.Position.X + ballVx, ball.Position.Y + ballVy);
            }

            // Check for goal
            if (Math.Abs(ball.Position.Y) <= .35 && Math.Abs(ball.Position.X) >= 2.4)
            {
                goalScored(ball.Position.X > 0);
                _state.updateBallInfo(new BallInfo(new Vector2(0, 0), 0, 0));
                ballVx = ballVy = 0;
                return;
            }

            List<RobotInfo> allinfos = new List<RobotInfo>(10);
            allinfos.AddRange(_state.getOurTeamInfo());
            allinfos.AddRange(_state.getTheirTeamInfo());

            // fix robot-robot collisions
            for (int i = 0; i < allinfos.Count; i++)
            {
                for (int j = 0; j < allinfos.Count; j++)
                {
                    if (i == j)
                        continue;
                    Vector2 p1 = allinfos[i].Position;
                    Vector2 p2 = allinfos[j].Position;
                    if (p1.distanceSq(p2) <= .2 * .2)
                    {
                        Vector2 t1 = p1 + .01f * (p1 - p2).normalize();
                        Vector2 t2 = p2 + .01f * (p2 - p1).normalize();
                        if (i < _state.getOurTeamInfo().Count)
                            _state.getOurTeamInfo()[i] = new RobotInfo(t1, allinfos[i].Orientation, allinfos[i].ID);
                        else
                            _state.getTheirTeamInfo()[i - _state.getOurTeamInfo().Count] = new RobotInfo(t1, allinfos[i].Orientation, allinfos[i].ID);

                        if (j < _state.getOurTeamInfo().Count)
                            _state.getOurTeamInfo()[j] = new RobotInfo(t2, allinfos[j].Orientation, allinfos[j].ID);
                        else
                            _state.getTheirTeamInfo()[j - _state.getOurTeamInfo().Count] = new RobotInfo(t2, allinfos[j].Orientation, allinfos[j].ID);
                    }
                }
            }

            // friction
            _state.updateBallInfo(new BallInfo(newballlocation, ballVx, ballVy));
            ballVx *= balldecay;
            ballVy *= balldecay;

            bool immobile = false;
            if (ballVx * ballVx + ballVy * ballVy < .01 * .01)
                immobile = true;

            // find robot-ball collisions
            {
                const double threshsq = .35 * .35;
                int numTooClose = 0;
                foreach (RobotInfo info in _state.getOurTeamInfo())
                {
                    double dist = ball.Position.distanceSq(info.Position);
                    if (dist < threshsq)
                        numTooClose++;
                }
                foreach (RobotInfo info in _state.getTheirTeamInfo())
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
                _ballImmobile++;
                if (_ballImmobile > 200)
                {
                    _state.updateBallInfo(new BallInfo(new Vector2(0, 0), 0, 0));
                    ballVx = ballVy = 0;
                    _ballImmobile = 0;
                }
            }
            else
                _ballImmobile = 0;
        }

        # region Start/Stop
        public void start()
        {
            if (!running)
            {
                //if (!initialized)
                //    initialize();
                _sleepTime = Constants.get<int>("UPDATE_SLEEP_TIME");
                worker = new Thread(run);
                worker.Start();
                counter = 0;
                running = true;
            }
        }

        public void stop()
        {
            if (running)
            {
                running = false;
            }
        }

        # endregion

        public void run()
        {
            while (running)
            {
                if (counter % 100 == 0)
                    Console.WriteLine("--------------RUNNING ROUND: " + counter + "-----------------");

                step();
                _parent.Invalidate();

                counter++;
                Thread.Sleep(_sleepTime);

            }
            Console.WriteLine("--------------DONE RUNNING: -----------------");
        }

        #endregion

    }
}
