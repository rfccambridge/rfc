using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;

using DefaultFormatter = System.Runtime.Serialization.Formatters.Binary.BinaryFormatter;

namespace Robocup.Utilities
{
    [Serializable]
    public struct LogMessage<T> : IComparable<LogMessage<T>>
    {
        public double time;
        //public string message;
        public T obj;


        public int CompareTo(LogMessage<T> other)
        {
            return Math.Sign(time - other.time);
        }
    }
    public class LogWriter<T>
    {

        private Stream s;
        private IFormatter f;

        private LogWriter(Stream s) : this(s, new DefaultFormatter()) { }
        private LogWriter(Stream s, IFormatter formatter)
        {
            this.s = s;
            this.f = formatter;
        }
        public void Close()
        {
            lock (write_lock)
            {
                s.Flush();
                s.Close();
            }
            GC.SuppressFinalize(this);
        }
        //the lock object for writing
        object write_lock = new object();
        /// <summary>
        /// Writes the object to the log output.  Is thread safe.
        /// </summary>
        /// <param name="o">The object to be written.  Must be serializable.</param>
        public void LogObject(T o/*, string message*/)
        {
            lock (write_lock)
            {
                LogMessage<T> m;
                m.time = HighResTimer.SecondsSinceStart();
                m.obj = o;
                //m.message = message;
                f.Serialize(s, m);
            }
        }

        ~LogWriter()
        {
            Console.WriteLine("~LogWriter");
            s.Close();
        }

        /// <summary>
        /// This is a list of weak references to LogWriters (keyed by names).
        /// Doing this strategy (interning weak references) allows us to use the same LogWriter for multiple calls of GetLogWriter,
        /// but also allows the memory to be reclaimed if desired.
        /// </summary>
        private static Dictionary<string, WeakReference> writers = new Dictionary<string, WeakReference>();
        public static LogWriter<T> GetLogWriter(string name)
        {
            if (!writers.ContainsKey(name) || !writers[name].IsAlive)
            {
                string fname;
                int tries = 0;
                do
                {
                    fname = name + string.Format("-{0:G3}.log", tries);
                    tries++;
                } while (File.Exists(fname));
                LogWriter<T> writer = new LogWriter<T>(new FileStream(fname, FileMode.Create));
                if (writers.ContainsKey(name))
                    writers.Remove(name);
                writers.Add(name, new WeakReference(writer));
                return writer;
            }
            return (LogWriter<T>)(writers[name].Target);
        }

        /// <summary>
        /// Reads in all the messages in the log, closes the stream, and returns the messages.
        /// </summary>
        static public List<LogMessage<T>> ReadLog(Stream s)
        {
            return ReadLog(s, new DefaultFormatter());
        }
        /// <summary>
        /// Reads in all the messages in the log, closes the stream, and returns the messages.
        /// </summary>
        static public List<LogMessage<T>> ReadLog(Stream s, IFormatter f)
        {
            List<LogMessage<T>> messages = new List<LogMessage<T>>();
            while (s.Position < s.Length)
            {
                messages.Add((LogMessage<T>)f.Deserialize(s));
            }
            s.Close();
            return messages;
        }
    }
}
