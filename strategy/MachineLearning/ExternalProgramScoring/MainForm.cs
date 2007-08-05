using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;



namespace MachineLearning.ExternalProgramScoring
{
    //to save on the typing and reading:
    using FileArgs = List<ConfigurationFileValues>;
    using Cand = Candidate<List<ConfigurationFileValues>>;

    public partial class MainForm : Form
    {
        SimpleExtScorer scorer;
        SimulatedAnnealing<List<ConfigurationFileValues>> simAnnealing;
        bool showingWindows = false;
        public MainForm()
        {
            InitializeComponent();

            scorer = new SimpleExtScorer();
            simAnnealing = new SimulatedAnnealing<List<ConfigurationFileValues>>(scorer.score);

            Random r = new Random();
            GenerateNextArgs<List<ConfigurationFileValues>> g = delegate(List<ConfigurationFileValues> l, double temp)
            {
                List<ConfigurationFileValues> rtn = new List<ConfigurationFileValues>();
                foreach (ConfigurationFileValues cfv in l)
                {
                    List<double> newvals = new List<double>();
                    foreach (double d in cfv.Values)
                    {
                        newvals.Add(d + (r.NextDouble() - .5) * (Math.Pow(temp, .5) + 1E-2));
                    }
                    rtn.Add(new ConfigurationFileValues(cfv.Filename, newvals));
                }
                return rtn;
            };
            simAnnealing.setGenFunction(g);
            scorer.showProgramWindow(false);
            showingWindows = false;

            simAnnealing.setVerbose(true);

            simAnnealing.IterationFinished += whenIterationFinished;

            //readProperties();
        }

        private void buttonSetProperties_Click(object sender, EventArgs e)
        {
            readProperties();
        }

        private void readProperties()
        {
            scorer.setExternalProgram(textBoxProgram.Text, textBoxArgs.Text);
            scorer.setConfigDirectory(textBoxConfigDir.Text);
            scorer.setConfigFileExtensions(new List<string>(textBoxExtensions.Text.Split(' ')));

            scorer.removeTags(checkBoxRemoveTags.Checked);

            simAnnealing.setCoolingFactor(double.Parse(textBoxCoolingSpeed.Text));
            simAnnealing.setTemp(double.Parse(textBoxTemp.Text));
            //simAnnealing.setTermFunction(SomeTerminationFunctions.repeatedTermClass<List<ConfigurationFileValues>>(0));
            simAnnealing.setTermFunction(SomeTerminationFunctions.repeatedTermClass<List<ConfigurationFileValues>>(int.Parse(textBoxNumIdentical.Text)));

            FileArgs f = scorer.getFirstArgs();
            simAnnealing.setCurrent(f);
            lastBest = f;

            buttonReload.Text = "Reload";
            buttonStart.Enabled = true;
        }

        private void buttonToggleShow_Click(object sender, EventArgs e)
        {
            showingWindows = !showingWindows;
            scorer.showProgramWindow(showingWindows);
            if (showingWindows)
                buttonToggleShow.Text = "Hide External Window";
            else
                buttonToggleShow.Text = "Show External Window";
        }

        int numTildes = 0;
        const int numTildesPerMinute = 60;
        System.Threading.Timer t;
        private void buttonStart_Click(object sender, EventArgs e)
        {
            shouldStop = false;
            new System.Threading.Thread(simAnnealing.minimize).Start();
            //simAnnealing.start();
            buttonReload.Enabled = false;
            buttonStart.Enabled = false;
            t = new System.Threading.Timer(delegate(object o)
            {
                numTildes++;
                if (numTildes % numTildesPerMinute == 0)
                    Console.Write((char)('0' + (numTildes / numTildesPerMinute) % 10));
                else
                    Console.Write('~');
            }, null, 60000 / numTildesPerMinute, 60000 / numTildesPerMinute);
        }

        bool shouldStop = false;
        private void buttonStop_Click(object sender, EventArgs e)
        {
            stopAndSave();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            stopAndSave();
        }

        private void stopAndSave()
        {
            shouldStop = true;
            simAnnealing.stop();
        }
        FileArgs lastBest = null;
        private delegate void EnableButtonDelegate();
        private void enableButtons()
        {
            if (buttonReload.InvokeRequired)
            {
                this.Invoke(new EnableButtonDelegate(enableButtons));
            }
            else
            {
                buttonReload.Enabled = true;
                buttonStart.Enabled = true;
            }
        }
        private void whenIterationFinished(bool done, Cand best, List<Cand> current, List<Cand> rejected)
        {
            numTildes = 0;
            if (!object.ReferenceEquals(lastBest, best.args))
            {
                lastBest = best.args;
                scorer.save(best.args, true);
            }
            if (done)
            {
                if (!shouldStop)
                {
                    Console.WriteLine("the simulated annealing algorithm thinks its finished");
                    Console.WriteLine("but oh boy, does it have another thing coming...");
                    scorer.save(best.args, true);
                    simAnnealing.setTermFunction(SomeTerminationFunctions.repeatedTermClass<FileArgs>(int.Parse(textBoxNumIdentical.Text)));
                    simAnnealing.clearBest();
                    simAnnealing.setCurrent(lastBest);
                    //System.Threading.Thread.Sleep(100);
                    //readProperties();
                    new System.Threading.Thread(simAnnealing.minimize).Start();
                    //simAnnealing.start();
                }
                else
                {
                    enableButtons();
                    if (lastBest != null)
                        scorer.save(lastBest, false);


                    t.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                    t.Dispose();
                    t = null;
                }
            }
        }

        private void checkBoxStochastic_CheckedChanged(object sender, EventArgs e)
        {
            bool check = checkBoxStochastic.Checked;
            textBoxComputationUnit.Enabled = check;
            labelComputationUnit.Enabled = check;
        }
    }
}