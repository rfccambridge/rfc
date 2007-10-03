using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace InterpreterTester
{
    class PlayScorer
    {
        static int Main(string[] args)
        {
            string fname;
            if (args.Length == 0)
            {
                System.Windows.Forms.MessageBox.Show("You didn't pass in the name of the output file -- assuming \"ml.results\"");
                fname = "ml.results";
            }
            else
                fname = args[0];
            int numIterations = 100000;
            if (args.Length >= 2)
                numIterations = int.Parse(args[1]);
            InterpreterTester it = new InterpreterTester();
            System.Threading.Thread t = new System.Threading.Thread(delegate() { Application.Run(it); });
            t.Priority = System.Threading.ThreadPriority.BelowNormal;
            t.Start();
            //have to negate the value, since we're minimizing
            //double value = -it.score(25000, 10);
            double stddev;
            double value = -it.score(numIterations, 1000, out stddev);
            //double value = -it.score(20);
            File.WriteAllText(fname, value + "\n" + stddev + "\n");
            Console.WriteLine(value + "\n" + stddev + "\n");
            t.Abort();
            Application.Exit();
            return 0;
        }
    }
    class TimeTester
    {
        static int Main(string[] args)
        {
            InterpreterTester it = new InterpreterTester();

            int current = 0;
            int diff = 61813;
            //int diff = 6181;
            while (true)
            {
                current = (current + diff) % 100000;
                double stddev;
                double value = it.score(current, 100,out stddev);
                Console.WriteLine(current + "\t" + value+"\t"+stddev);
            }

            //return 0;
        }
    }
}
