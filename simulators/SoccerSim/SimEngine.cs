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
    class SimEngine
    {
        const int TEAMSIZE = 5;

        PhysicsEngine physics_engine;

        volatile bool running = false;
        int _sleepTime;
        System.Timers.Timer t;
        int counter = 0;

        SoccerSim _parent;

        public SimEngine(PhysicsEngine physics_engine, SoccerSim parent)
        {
            this._parent = parent;

            this.physics_engine = physics_engine;
        }

        #region Simulation


        # region Start/Stop
        public void start()
        {
            if (!running)
            {
                //if (!initialized)
                //    initialize();
                _sleepTime = Constants.get<int>("default", "UPDATE_SLEEP_TIME") / 2;
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

        public void run()
        {
            if (counter % 100 == 0)
                Console.WriteLine("--------------RUNNING ROUND: " + counter + "-----------------");

            step(_sleepTime / 1000.0);
            _parent.Invalidate();

            counter++;
        }

        #endregion

    }
}
