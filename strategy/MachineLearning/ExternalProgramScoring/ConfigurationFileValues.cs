using System;
using System.Collections.Generic;
using System.Text;

namespace MachineLearning.ExternalProgramScoring
{
    public class ConfigurationFileValues
    {
        private string filename;
        public string Filename
        {
            get { return filename; }
        }
        private List<double> values;
        public List<double> Values
        {
            get { return values; }
        }
        public ConfigurationFileValues(string filename, List<double> values)
        {
            this.filename = filename;
            this.values = values;
        }
    }
}
