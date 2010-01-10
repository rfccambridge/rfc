using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

using Robocup.Utilities;
using Robocup.CoreRobotics;
using Robocup.Core;
using Robocup.Simulation;

namespace Robocup.Simulation
{
    // make this load/instantiate from a text file
    public class SimEngine
    {

        const int TEAMSIZE = 5;

        PhysicsEngine physics_engine;

        volatile bool running = false;
        int _sleepTime;
        System.Timers.Timer t;
        int counter = 0;        

        public SimEngine(PhysicsEngine physics_engine)
        {            
            this.physics_engine = physics_engine;
        }

        #region Simulation


        # region Start/Stop
        public void start()
        {
            if (!running)
            {
                double freq = Constants.get<double>("default", "SIM_ENGINE_FREQUENCY");
                double period = 1.0 / freq * 1000; // in ms

                t = new System.Timers.Timer(period);
                t.AutoReset = true;
                t.Elapsed += delegate(object sender, System.Timers.ElapsedEventArgs e)
                {
                    run(period);
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
            physics_engine.Step(dt);
        }

        # endregion

        public void run(double dt)
        {
            if (counter % 100 == 0)
                Console.WriteLine("--------------RUNNING ROUND: " + counter + "-----------------");

            step(dt / 1000.0); // convert to sec
            counter++;
        }

        #endregion

    }
}
