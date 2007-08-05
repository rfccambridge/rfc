using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MachineLearning.ExternalProgramScoring
{
    static class FileIOHandler
    {
        #region IO
        private static bool validFilename(string fname, List<string> validExtensions)
        {
            if (validExtensions == null || validExtensions.Count == 0)
                return true;
            string extension = fname.Substring(fname.LastIndexOf('.') + 1);
            return validExtensions.Contains(extension);
        }
        private static List<string> getValidFilenames(string dirName, List<string> validExtensions)
        {
            string[] allfilenames = Directory.GetFiles(dirName);
            if (validExtensions == null || validExtensions.Count == 0)
                return new List<string>(allfilenames);
            List<string> rtn = new List<string>();
            foreach (string s in allfilenames)
            {
                if (validFilename(s, validExtensions))
                    rtn.Add(s);
            }
            return rtn;
        }
        //private delegate void IOWork();
        /// <summary>
        /// Calls "work" until it does not throw an IOException or UnauthorizedAccessException
        /// </summary>
        /*private static void doIOWork(IOWork work)
        {
            bool completed = false;
            while (!completed)
            {
                try
                {
                    work();
                    completed = true;
                }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }*/
        #endregion

        /// <summary>
        /// Reads the specified string for double values.  The values must be
        /// sandwiched between two tags; two tags in a row is invalid.
        /// </summary>
        /// <returns>A list of the values, in the order that they appear in the file</returns>
        public static List<double> getValues(string wholeFile, string tag)
        {
            string[] splitStrings = wholeFile.Split(new string[] { tag }, StringSplitOptions.None);
            System.Diagnostics.Debug.Assert(splitStrings.Length % 2 == 1,
                "Expected an even number of tags, but found an odd number("
                + (splitStrings.Length - 1) + ")");
            List<double> l = new List<double>();
            for (int i = 1; i < splitStrings.Length; i += 2)
            {
                l.Add(double.Parse(splitStrings[i]));
            }
            return l;
        }
        /// <summary>
        /// Takes the original file as input, and writes a new file, with or without tags.
        /// </summary>
        /// <param name="includeTags">Whether or not to include the tags in the output</param>
        public static void updateValues(string wholeFile, StreamWriter outFile, List<double> newValues, string tag, bool includeTags)
        {
            string[] splitStrings = wholeFile.Split(new string[] { tag }, StringSplitOptions.None);
            System.Diagnostics.Debug.Assert(newValues.Count * 2 + 1 == splitStrings.Length,
                "The number of new values given (" + newValues.Count +
                ")does not match up with the number of tags found (" +
                (splitStrings.Length - 1) + ")");
            for (int i = 0; i < newValues.Count; i++)
                splitStrings[2 * i + 1] = "" + newValues[i];
            for (int i = 0; i < splitStrings.Length; i++)
            {
                if (includeTags && i != 0)
                {
                    outFile.Write(tag);
                }
                outFile.Write(splitStrings[i]);
            }
        }
        /// <summary>
        /// Takes the original file as input, and writes a new file, with or without tags.
        /// </summary>
        /// <param name="includeTags">Whether or not to include the tags in the output</param>
        public static void updateValues(string inFilename, string outFilename, List<double> newValues, string tag, bool includeTags)
        {
            if (inFilename == outFilename && newValues.Count == 0)
                return;

            bool completed = false;
            while (!completed)
            {
                StreamReader sr = null;
                StreamWriter sw = null;
                try
                {
                    sr = new StreamReader(inFilename);
                    string wholeFile = sr.ReadToEnd();
                    sr.Close();
                    sw = new StreamWriter(outFilename);
                    updateValues(wholeFile, sw, newValues, tag, includeTags);
                    sw.Close();
                    completed = true;
                }
                catch (Exception) { completed = false; }
                finally { sr.Close(); if (sw != null)sw.Close(); }
            }
        }

        /// <summary>
        /// Copies all the files from one directory to another (can be the same directory), and updates the values.
        /// </summary>
        /// <param name="fromDir">The directory to read the files from</param>
        /// <param name="toDir">The directory to write the files to</param>
        /// <param name="validExtensions">The etensions to use.  If it is empty/null, all extensions are used.</param>
        /// <param name="newValues">The list of new values to apply.  must be exactly one entry for each configurable file</param>
        /// <param name="tag">The file tag to use</param>
        /// <param name="includeTags">Whether or not to include the tag in the new files.</param>
        /// <param name="deleteOld">Whether or not to delete the old files.  (Has no meaning if fromDir=oldDir)</param>
        static public void updateAllValues(string fromDir, string toDir, List<string> validExtensions, List<ConfigurationFileValues> newValues, string tag, bool includeTags, bool deleteOld)
        {
            List<string> fnames = getValidFilenames(fromDir, validExtensions);
            //assert that the filenames are the same
            {
                if (fnames.Count > newValues.Count)
                    throw new ApplicationException("There are more configurable files than values!");
                else if (fnames.Count < newValues.Count)
                    throw new ApplicationException("There are more values than files!");
                /*foreach (ConfigurationFileValues cfv in newValues)
                {
                    if (!fnames.Contains(cfv.Filename))
                        throw new ApplicationException("The filename " + cfv.Filename + " does not exist on disk (or has been eliminated through file extensions");
                }*/
            }
            if (! Directory.Exists(toDir))
                Directory.CreateDirectory(toDir);
            foreach (ConfigurationFileValues cfv in newValues)
            {
                string fname = cfv.Filename;
                string onlyName = new FileInfo(fname).Name;
                string newName = toDir + "/" + onlyName;
                string oldName = fromDir + "/" + onlyName;
                updateValues(oldName, newName, cfv.Values, tag, includeTags);
                if (deleteOld && (!new DirectoryInfo(fromDir).FullName.Equals(new DirectoryInfo(toDir).FullName)))
                {
                    File.Delete(fname);
                }
                //updateValues(fname,
            }
        }
        /// <summary>
        /// Moves all the matching files from one directory to another.  Overwrites destination files.
        /// </summary>
        static public void moveAll(string fromDir, string toDir, List<string> validExtensions)
        {
            if (!Directory.Exists(toDir))
                Directory.CreateDirectory(toDir);
            List<string> fnames = getValidFilenames(fromDir,validExtensions);
            foreach (string fname in fnames)
            {
                string actualName = new FileInfo(fname).Name;
                string newName=toDir + actualName;
                if (File.Exists(newName))
                    File.Delete(newName);
                File.Move(fname, newName);
            }
        }
    }
}
