using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace MachineLearning.ExternalProgramScoring
{
    class ExtProgTester
    {
        static int Main(string[] args)
        {
            /*Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());*/

            SimpleExtScorer scorer = new SimpleExtScorer();
            scorer.setConfigDirectory("../../ExternalProgramScoring/testing/");
            scorer.setExternalProgram("../../ExternalProgramScoring/testing/basic.exe");
            scorer.setConfigFileExtensions(new List<string>(new String[] { "txt" }));
            scorer.showProgramWindow(false);
            scorer.removeTags(false);
            List<ConfigurationFileValues> first = scorer.getFirstArgs();


            Random r = new Random();
            GenerateNextArgs<List<ConfigurationFileValues>> g = delegate(List<ConfigurationFileValues> l, double temp)
            {
                List<ConfigurationFileValues> rtn = new List<ConfigurationFileValues>();
                foreach (ConfigurationFileValues cfv in l)
                {
                    List<double> newvals = new List<double>();
                    foreach(double d in cfv.Values){
                        newvals.Add(d + (r.NextDouble() - .5) * (Math.Pow(temp, .5) + 1E-2));
                    }
                    rtn.Add(new ConfigurationFileValues(cfv.Filename,newvals));
                }
                return rtn;
            };
            SingleTerminationFunction<List<ConfigurationFileValues>> t = SomeTerminationFunctions.repeatedTermClass<List<ConfigurationFileValues>>(100);
            SimulatedAnnealing<List<ConfigurationFileValues>> sa = new SimulatedAnnealing<List<ConfigurationFileValues>>(scorer.score);
            sa.setTemp(5);
            sa.setCoolingFactor(.02);
            sa.setVerbose(true);
            sa.setGenFunction(g);
            sa.setTermFunction(t);
            sa.setCurrent(scorer.getFirstArgs());
            sa.minimize();
            scorer.score(sa.getBest());
            //sa.minimize(scorer.score, first, g, t);
            return 0;
        }
    }
}
