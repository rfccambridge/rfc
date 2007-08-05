using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MachineLearning.ExternalProgramScoring
{
    abstract class ExtProgScorerBase<T> : IExtProgScorer<T>
    {
        private string tag = "#ml";
        private bool shouldRemoveTags = true;

        #region IO
        private List<string> validExtensions;
        private bool validFilename(string fname)
        {
            if (validExtensions == null || validExtensions.Count == 0)
                return true;
            string extension = fname.Substring(fname.LastIndexOf('.') + 1);
            return validExtensions.Contains(extension);
        }
        private List<string> getValidFilenames(string dirName)
        {
            string[] allfilenames = Directory.GetFiles(dirName);
            if (validExtensions == null || validExtensions.Count == 0)
                return new List<string>(allfilenames);
            List<string> rtn = new List<string>();
            foreach (string s in allfilenames)
            {
                if (validFilename(s))
                    rtn.Add(s);
            }
            return rtn;
        }
        private List<string> getValidFilenames()
        {
            return getValidFilenames(configDirectory);
        }
        #endregion

        #region Setting Properties
        private string configDirectory;
        private string externalProgram;
        private string workingDirectory;
        private string arguments = "ml.results";
        volatile private bool showWindow = false;

        private void checkValidValues()
        {
            Debug.Assert(configDirectory != null, "The directory for configuration files has not been set!");
            Debug.Assert(externalProgram != null, "The external program to run has not been set!");
            Debug.Assert(arguments.StartsWith("ml.results"), "Does not pass in the name of the output file to the external program!");
        }

        public void setConfigFileExtensions(List<string> extensions)
        {
            validExtensions = extensions;
        }

        public void setConfigDirectory(string directory)
        {
            configDirectory = directory;
            //Directory.SetCurrentDirectory(directory);
        }

        public void setExternalProgram(string program)
        {
            setExternalProgram(program, "");
        }

        public void setExternalProgram(string program, string args)
        {
            externalProgram = program;
            arguments = "ml.results " + args;
        }

        public void setExternalProgram(string program, params string[] args)
        {
            setExternalProgram(program, string.Join(" ", args));
        }

        public void setArguments(params string[] args)
        {
            setExternalProgram(externalProgram, args);
        }

        public void setWorkingDirectory(string directory)
        {
            workingDirectory = directory;
        }

        public void showProgramWindow(bool showWindow)
        {
            //only do stuff if the value is being changed
            if (showWindow != this.showWindow)
            {
                this.showWindow = showWindow;
            }
        }
        #endregion

        public List<ConfigurationFileValues> getFirstArgs()
        {
            checkValidValues();
            cleanUp();
            List<string> fnames = getValidFilenames();
            List<ConfigurationFileValues> rtn = new List<ConfigurationFileValues>();
            foreach (string s in fnames)
            {
                StreamReader sr = new StreamReader(s);
                List<double> doubles = FileIOHandler.getValues(sr.ReadToEnd(), tag);
                rtn.Add(new ConfigurationFileValues(s, doubles));
                sr.Close();
            }
            return rtn;
        }
        private void cleanUp()
        {
            string resultsFile = configDirectory + "ml.results";
            if (File.Exists(resultsFile))
                File.Delete(resultsFile);
            string backupDir = configDirectory + "\\ml_backup\\";
            if (!Directory.Exists(backupDir))
                return;
            string[] files = Directory.GetFiles(backupDir);
            if (files.Length == 0)
                return;
            Console.Write("untidy shutdown -- performing cleanup...");
            FileIOHandler.moveAll(backupDir, configDirectory,validExtensions);
            Console.WriteLine("done");
            Directory.Delete(backupDir);
        }
        abstract protected T parseOutputFile(string wholeFile);
        public T score(List<ConfigurationFileValues> args)
        {
            //we need to store this in case it gets set in the middle
            bool backingUp = shouldRemoveTags;
            checkValidValues();
            string backupDir = configDirectory + "\\ml_backup\\";
            if (backingUp)
            {
                FileIOHandler.moveAll(configDirectory, backupDir, validExtensions);
                FileIOHandler.updateAllValues(backupDir, configDirectory, validExtensions, args, tag, false, false);
            }
            else
            {
                FileIOHandler.updateAllValues(configDirectory, configDirectory, validExtensions, args, tag, true, false);
            }
            Process p = new Process();
            if (workingDirectory != null)
                p.StartInfo.WorkingDirectory = workingDirectory;
            else
                p.StartInfo.WorkingDirectory = configDirectory;
            p.StartInfo.Arguments = arguments;
            p.StartInfo.FileName = externalProgram;
            if (!showWindow)
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.Start();
            p.PriorityClass = ProcessPriorityClass.Idle;
            p.WaitForExit();
            if (!File.Exists(configDirectory + "ml.results"))
            {
                System.Threading.Thread.Sleep(1000);
                if (!File.Exists(configDirectory + "ml.results"))
                {
                    Console.WriteLine("The file \"ml.results\" has not been created.  Aborting");
                    //System.Threading.Thread.Sleep(1000);
                    //throw new ApplicationException("The file \"ml.results\" has not been created");
                    System.Environment.Exit(-1);
                }
            }
            string wholeFile = File.ReadAllText(configDirectory + "/ml.results");
            T rtn = parseOutputFile(wholeFile);
            if (backingUp)
            {
                FileIOHandler.moveAll(backupDir, configDirectory, validExtensions);
            }
            File.Delete(configDirectory + "ml.results");
            return rtn;
        }
        public void save(List<ConfigurationFileValues> toSave, bool backup)
        {
            checkValidValues();
            cleanUp();
            string saveDir = configDirectory;
            if (backup)
            {
                saveDir = configDirectory + "\\ml_save\\";
                if (!Directory.Exists(saveDir))
                    Directory.CreateDirectory(saveDir);
            }
            FileIOHandler.updateAllValues(configDirectory, saveDir, validExtensions, toSave, tag, true, false);
        }

        public void removeTags(bool removeTags)
        {
            this.shouldRemoveTags = removeTags;
        }

        public void setTag(string tag)
        {
            this.tag = tag;
        }
    }
}
