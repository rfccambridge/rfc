using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace RobocupPlays
{
    public class PlayUtils
    {
        public static void savePlays(Dictionary<InterpreterPlay, string> filenames)
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
        }

        public static Dictionary<InterpreterPlay,string> loadPlays(string path)
        {
            PlayLoader loader = new PlayLoader();
            string[] files = System.IO.Directory.GetFiles("../../plays/");
            
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

                try
                {
                    InterpreterPlay p = loader.load(filecontents);
                    toRet.Add(p, fname);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return toRet;
        }
    }
}
