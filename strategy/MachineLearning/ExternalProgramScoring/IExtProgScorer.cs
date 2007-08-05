using System;
using System.Collections.Generic;
using System.Text;

namespace MachineLearning.ExternalProgramScoring
{
    /// <summary>
    /// A interface that provides a scoring function, based on an external program.
    /// 
    /// This is how it works [with likely additions like this]:
    /// 
    /// You give it the program to run, as well as a directory that contains a set of
    /// configuration files.  Each configuration file will have a set of tags that say
    /// which should be changed around, and how.  For instance, if the tag is "#ml#",
    /// then the phrase "#ml#1.5#ml#" means that the value is 1.5, and should be changed
    /// by the algorithm.  (The configuration files can be specified.)  The scorer only
    /// looks at config files in the directory its given [but can be specified to search
    /// multiple directories / recurse].
    /// 
    /// Currently, there is no support for having the tag itself appear in the file [but
    /// if useful there eventually will be a way, such as two tags that have nothing inbetween].
    /// 
    /// Currently, all values must be unconstrained floating point numbers [eventually there
    /// may be extra information that you can include between the tags, or inside a tag,
    /// specifying what type it it, or what constraints].
    /// 
    /// The scorer will first copy all the files to a backup directory [likely "ml_backup"].
    /// The scorer will then change some or all of the right values, and remove the tags.
    /// Then the external program will be run.  To return a value, this program should
    /// write the result to a specified value (such as "ml.results" -- the first argument to the program),
    /// where the file is just the score in text form, and a newline afterwards.
    /// 
    /// Then the scorer will add back the tags, and possibly change values again.
    /// 
    /// The external program must the files exactly as they are when the program is called;
    /// there is no guarantee that the files will remain constant.  The configuration files
    /// must not be modified externally however.
    /// [Eventually, it will be nice to have multiple scorers running in parallel, on the
    /// same set of configuration files.  This may be very difficult to acheive though,
    /// and it might be better to have them running on separate configuration files.]
    /// 
    /// </summary>
    public interface IExtProgScorer<T>
    {
        /// <summary>
        /// Sets the file extensions that represent configuration files to be
        /// inspected and changed.  If no extensions are given, all extensions
        /// are used.
        /// </summary>
        void setConfigFileExtensions(List<string> extensions);
        /// <summary>
        /// Sets the working directory of the external program.  If no working directory is specified, the
        /// configuration directory is used.
        /// </summary>
        void setWorkingDirectory(string directory);
        /// <summary>
        /// Sets the directory in which all the configuration files are stored.
        /// </summary>
        void setConfigDirectory(string directory);
        /// <summary>
        /// Sets the command of the external program to be run.
        /// </summary>
        void setExternalProgram(string program);
        /// <summary>
        /// Sets the command of the external program to be run, along with all
        /// the command-line arguments.
        /// </summary>
        /// <param name="args">One string that is all the command-line arguments</param>
        void setExternalProgram(string program, string args);
        /// <summary>
        /// Sets the command of the external program to be run, along with all
        /// the command-line arguments.
        /// </summary>
        void setExternalProgram(string program, params string[] args);
        /// <summary>
        /// Sets the command line arguments (after the initial one that says
        /// which file to write to).
        /// </summary>
        void setArguments(params string[] args);
        /// <summary>
        /// Reads from the files the values that are already there (so that
        /// any optimization algorithms know where to start).
        /// </summary>
        List<ConfigurationFileValues> getFirstArgs();
        /// <summary>
        /// The main method that does that actual scoring.
        /// </summary>
        T score(List<ConfigurationFileValues> args);
        /// <summary>
        /// Sets whether or not to show the window for the
        /// external program.
        /// </summary>
        void showProgramWindow(bool showWindow);
        /// <summary>
        /// Sets whether or not you can leave the tags in the files.
        /// If set to true, then the tags are guaranteed to be removed.
        /// If set to false, then the tags can (but do not have to be) removed.
        /// </summary>
        /// <param name="leaveTags">false only if the external program works even with the tags in there.</param>
        void removeTags(bool removeTags);
        /// <summary>
        /// Sets the strings used to denote tags.  Default is "#ml".
        /// </summary>
        void setTag(string tag);
    }
}
