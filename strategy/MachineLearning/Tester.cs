using System;
using System.Collections.Generic;
using System.Text;

namespace MachineLearning
{
    class DoubleDoubles
    {
        public double x = 3, y = 3;
        public override string ToString()
        {
            return x + "\t" + y;
        }
    }
    class Tester
    {
        static int Main(string[] args)
        {
            /*Random r = new Random();
            ScoreFunction<double> s = delegate(double d) { return d * d * d * d - 10 * d * d + 10 * d; };
            GenerateNextArgs<double> g = delegate(double d, double temp) { Console.WriteLine(d); return d + (r.NextDouble() - .5) / 10; };
            TerminationFunction<double> t = SomeTerminationFunctions.repeatedTermStruct<double>(1000);
            SimulatedAnnealing<double> sa = new SimulatedAnnealing<double>();
            sa.setStartingTemp(10);
            sa.setCoolingFactor(.001);
            sa.learn(s, 2, g,t);*/
            Random r = new Random();
            ScoreFunction<DoubleDoubles> s = delegate(DoubleDoubles d)
            {
                return (1 - d.x) * (1 - d.x) + 100 * (d.y - d.x * d.x) * (d.y - d.x * d.x);
            };
            GenerateNextArgs<DoubleDoubles> g = delegate(DoubleDoubles d, double temp)
            {
                Console.WriteLine(d);
                DoubleDoubles d2=new DoubleDoubles();
                d2.x = d.x + (r.NextDouble() - .5) * (Math.Pow(temp,.5) + 1E-2);
                d2.y = d.y + (r.NextDouble() - .5) * (Math.Pow(temp,.5) + 1E-2);
                return d2;
            };
            SingleTerminationFunction<DoubleDoubles> t = SomeTerminationFunctions.repeatedTermClass<DoubleDoubles>(100000);
            SimulatedAnnealing<DoubleDoubles> sa = new SimulatedAnnealing<DoubleDoubles>(s);
            sa.setTemp(2);
            sa.setCoolingFactor(.02);
            sa.setVerbose(false);
            sa.setGenFunction(g);
            sa.setTermFunction(t);
            sa.setCurrent(new DoubleDoubles());
            sa.minimize();
            //sa.minimize(s, new DoubleDoubles(), g, t);
            return 0;
        }
    }
}
