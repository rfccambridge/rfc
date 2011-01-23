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

        public static Dictionary<string, InterpreterTactic> loadTactics(string path)
        {
            Console.WriteLine("Loading tactics directory: " + path);
            TacticLoader<InterpreterTactic, InterpreterExpression> loader =
                new TacticLoader<InterpreterTactic, InterpreterExpression>(new InterpreterExpression.Factory());
            string[] files = Directory.GetFiles(path);

            Dictionary<string, InterpreterTactic> tacticBook = new Dictionary<string, InterpreterTactic>();

            foreach (string fname in files)
            {
                if (Path.GetExtension(fname) != ".txt")
                    continue;

                StreamReader reader = new StreamReader(fname);
                string filecontents = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();

                Console.WriteLine("Loaded: " + fname);
                InterpreterTactic t = loader.load(filecontents, Path.GetFileNameWithoutExtension(fname));

                if (tacticBook.ContainsKey(t.Name))
                    throw new ApplicationException("Duplicate tactic with name: " + t.Name);

                tacticBook.Add(t.Name, t);
            }

            return tacticBook;
        }

        public static Dictionary<InterpreterPlay,string> loadPlays(string path, Dictionary<string, InterpreterTactic> tacticBook)
        {
            Console.WriteLine("Loading plays directory: " + path);
            PlayLoader<InterpreterPlay, InterpreterTactic, InterpreterExpression> loader =
                new PlayLoader<InterpreterPlay, InterpreterTactic, InterpreterExpression>(new InterpreterExpression.Factory(), tacticBook);
            string[] files = Directory.GetFiles(path);
            
            Dictionary<InterpreterPlay, string> toRet = new Dictionary<InterpreterPlay, string>();

            foreach (string fname in files)
            {
                if (Path.GetExtension(fname) != ".txt")
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
