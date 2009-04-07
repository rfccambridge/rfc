using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

using Robocup.Utilities;
using Robocup.Plays;
using Robocup.CoreRobotics;
using Robocup.Core;
using Robocup.Simulation;

namespace SoccerSim
{
    // make this load/instantiate from a text file
    class SimVision
    {
        const int TEAMSIZE = 5;

        PhysicsEngine physics_engine;

        volatile bool running = false;
        int _sleepTime;
        System.Timers.Timer t;
        int counter = 0;

        SoccerSim _parent;

        private Robocup.MessageSystem.MessageSender<Robocup.Core.VisionMessage> _messageSender;
        private int MESSAGE_SENDER_PORT = Robocup.Core.Constants.get<int>("ports", "VisionDataPort");

        public SimVision(PhysicsEngine physics_engine, SoccerSim parent)
        {
            this._parent = parent;

            this.physics_engine = physics_engine;
            _messageSender = Robocup.MessageSystem.Messages.CreateServerSender<VisionMessage>(MESSAGE_SENDER_PORT);
        }

        #region Simulation


        # region Start/Stop
        public void start()
        {
            if (!running)
            {
                //if (!initialized)
                //    initialize();
                _sleepTime = Constants.get<int>("default", "UPDATE_SLEEP_TIME");
                t = new System.Timers.Timer(_sleepTime);
                t.AutoReset = true;
                t.Elapsed += delegate(object sender, System.Timers.ElapsedEventArgs e)
                {
                    run();
                };
                t.Start();
                counter = 0;
                running = true;
            }
        }

        public void stop()
        {
            if (running)
            {
                t.Enabled = false;
                running = false;
                Console.WriteLine("--------------DONE RUNNING: -----------------");
            }
        }
        /// <summary>
        /// Steps forward the given number of seconds.
        /// </summary>
        public void step(double dt)
        {
            physics_engine.step(dt);
        }

        # endregion

        VisionMessage vm;
        List<RobotInfo> bots;
        public void run()
        {
            if (counter % 100 == 0)
                Console.WriteLine("--------------RUNNING ROUND: " + counter + "-----------------");

            vm = new VisionMessage(new Vector2(-physics_engine.getBallInfo().Position.Y,physics_engine.getBallInfo().Position.X));
            bots = physics_engine.getOurTeamInfo();
            foreach (RobotInfo ourRobot in bots)
            {
                vm.Robots.Add(new VisionMessage.RobotData(ourRobot.ID, VisionMessage.Team.YELLOW, new Vector2(-ourRobot.Position.Y, ourRobot.Position.X), ourRobot.Orientation));
            }
            bots = physics_engine.getTheirTeamInfo();
            foreach (RobotInfo theirRobot in bots)
            {
                vm.Robots.Add(new VisionMessage.RobotData(theirRobot.ID, VisionMessage.Team.BLUE, new Vector2(-theirRobot.Position.Y, theirRobot.Position.X), theirRobot.Orientation));
            }
            _messageSender.Post(vm);
            _parent.Invalidate();

            counter++;
        }

        #endregion

    }
}
