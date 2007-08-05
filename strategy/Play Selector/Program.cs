using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Drawing;

namespace RobocupPlays
{
    static class Program
    {
        //A Commander class for testing purposes:
        private class ConsoleCommander : Commander
        {

            public void move(int robotID, float x, float y)
            {
                Console.WriteLine("Moving robot "+robotID+" to point "+x+", "+y);
            }

            public void move(int robotID, float x, float y, float orientation)
            {
                Console.WriteLine("Moving robot " + robotID + " to point " + x + ", " + y + ", facing " + orientation);
            }

            public void kick(int robotID)
            {
                Console.WriteLine("Telling robot "+robotID+" to kick");
            }
            public void stop(int robotID)
            {
                Console.WriteLine("Telling robot " + robotID + " to stop moving");
            }
        }
        static void Main()
        {

            //This will load all the plays in the Plays folder:
            PlayLoader loader = new PlayLoader();

            string[] files = System.IO.Directory.GetFiles("C:/Microsoft Robotics Studio (1.0)/Samples/Simulator/Plays");
            ArrayList plays = new ArrayList();
            foreach (string fname in files)
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(fname);
                string filecontents = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();

                plays.Add(loader.load(filecontents));
            }

            //Now that we have all the plays loaded, we create a new commander (which will just output the commands to the console),
            //and create a new interpreter, feeding it the list of plays, and the commander we just created
            Commander commander=new ConsoleCommander();
            Interpreter interpreter = new Interpreter((InterpreterPlay[])plays.ToArray(typeof(InterpreterPlay)), commander);

            Random r=new Random();
            for (int i = 0; i < 10; i++)
            {

                //a RobotInfo is a class to hold all the information about a robot's state.  right now, it's just
                //position, orientation, and a unique ID number

                //so we create an array of these, representing our team
                RobotInfo[] ourteaminfo = new RobotInfo[]{
                     new RobotInfo(new PlayPoint((float)(10*r.NextDouble()-5),(float)(10*r.NextDouble()-5)),0,0),
                     new RobotInfo(new PlayPoint((float)(10*r.NextDouble()-5),(float)(10*r.NextDouble()-5)),0,1),
                     new RobotInfo(new PlayPoint((float)(10*r.NextDouble()-5),(float)(10*r.NextDouble()-5)),0,2),
                     new RobotInfo(new PlayPoint((float)(10*r.NextDouble()-5),(float)(10*r.NextDouble()-5)),0,3),
                     new RobotInfo(new PlayPoint((float)(10*r.NextDouble()-5),(float)(10*r.NextDouble()-5)),0,4)
                };
                //and the enemy team
                RobotInfo[] theirteaminfo = new RobotInfo[]{
                };
                //BallInfo is analogous to RobotInfo, except it just holds the position
                BallInfo ballinfo = new BallInfo(new PlayPoint(-0.0f, -0.0f),0,0);

                //then we tell the interpreter to take these data and interpret them.
                //it goes through the plays, selects some of them, takes the actions from those plays,
                //interprets those actions, and then outputs the results to the commander,
                //which right now is just displaying the results to the screen
                /*interpreter.setState(ourteaminfo, theirteaminfo, ballinfo);
                interpreter.selectActions();
                interpreter.runActions();*/
                interpreter.interpret(ourteaminfo, theirteaminfo, ballinfo,PlayTypes.NormalPlay);

                Console.WriteLine("~~~~~~~~~~~~~" + i + "~~~~~~~~~~~~~");
            }
        }
    }
}
