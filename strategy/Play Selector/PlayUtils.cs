using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace Robocup.Plays
{
    public class PlayUtils
    {
        // -- I actually don't think that we'll want to do this ever.  You wouldn't edit the compiled
        // versions of the plays and then resave them.  I commented this out because it doesn't work
        // with my new changes :)   --kevin

        /*public static void savePlays(Dictionary<InterpreterPlay, string> filenames)
        {
            try
            {
                foreach (KeyValuePair<InterpreterPlay, string> pair in filenames)
                {
                    string s = pair.Key.Save();
                    StreamWriter writer = new StreamWriter(pair.Value);
                    writer.WriteLine(s);
                    writer.Close();
                    writer.Dispose();
                }
            }
            catch (IOException)
            {
                throw new ApplicationException("Error saving the files --\n too bad, you've probably lost whichever one was being saved");
            }
        }*/

        public static Dictionary<InterpreterPlay,string> loadPlays(string path)
        {
            Console.WriteLine("Loading plays directory: " + path);
            PlayLoader<InterpreterPlay, InterpreterExpression> loader =
                new PlayLoader<InterpreterPlay, InterpreterExpression>(new InterpreterExpression.Factory());
            string[] files = System.IO.Directory.GetFiles(path);
            
            Dictionary<InterpreterPlay, string> toRet = new Dictionary<InterpreterPlay, string>();

            foreach (string fname in files)
            {
                string extension = fname.Substring(1 + fname.LastIndexOf('.'));
                if (extension != "txt")
                    continue;
                StreamReader reader = new StreamReader(fname);
                string filecontents = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();

                Console.WriteLine("Loaded: " + fname);
                InterpreterPlay p = loader.load(filecontents, Path.GetFileNameWithoutExtension(fname));
                toRet.Add(p, fname);
            }
            return toRet;
        }
    }
}
