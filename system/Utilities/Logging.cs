using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;

using DefaultFormatter = System.Runtime.Serialization.Formatters.Binary.BinaryFormatter;

namespace Robocup.Utilities
{
    [Serializable]
    public class LogMessage<T> : IComparable<LogMessage<T>>
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

        /// <summary>
        /// Creates a LogWriter from a given stream.
        /// </summary>
        /// <param name="s"></param>
        public LogWriter(Stream s)
        {
            this.s = s;
            this.f = new DefaultFormatter();
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
            LogMessage<T> m = new LogMessage<T>();
            m.time = HighResTimer.SecondsSinceStart();
            m.obj = o;
            //m.message = message;
            lock (write_lock)
            {
                f.Serialize(s, m);
            }
        }
        /// <summary>
        /// Logs the given object with the specified timestamp.  Designed for simulation purposes.
        /// </summary>
        public void SimulateTimedLog(T o, double time)
        {
            LogMessage<T> m = new LogMessage<T>();
            m.time = time;
            m.obj = o;
            //m.message = message;
            lock (write_lock)
            {
                f.Serialize(s, m);
            }
        }
        /// <summary>
        /// Adds a "break" in the logs; if you ask for the logs back later, you can say
        /// to break the log messages up by the breaks that you insert
        /// </summary>
        public void InsertBreak()
        {
            lock (write_lock)
            {
                f.Serialize(s, "break");
            }
        }

        ~LogWriter()
        {
            //Console.WriteLine("~LogWriter");
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
            return GetLogWriter(name, false);
        }
        public static LogWriter<T> GetLogWriter(string name, bool compress)
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
        /// Reads in all the messages in the log, removes any "breaks", closes the stream, and returns the messages.
        /// </summary>
        static public List<LogMessage<T>> ReadLog(Stream s)
        {
            IFormatter f = new DefaultFormatter();
            List<LogMessage<T>> messages = new List<LogMessage<T>>();
            while (s.Position < s.Length)
            {
                object o = f.Deserialize(s);
                if (!"break".Equals(o))
                {
                    LogMessage<T> message = (LogMessage<T>)o;
                    messages.Add(message);
                }
            }
            s.Close();
            return messages;
        }
        /// <summary>
        /// Reads in all the messages in the log, breaking it up by "breaks", closes the stream, and returns the messages.
        /// </summary>
        /// <param name="ReturnEmptySequence">If there are two adjacent "breaks", whether or not to return an empty sequence</param>
        static public List<List<LogMessage<T>>> ReadAndBreakLog(Stream s, bool ReturnEmptySequence)
        {
            IFormatter f = new DefaultFormatter();
            List<List<LogMessage<T>>> rtn = new List<List<LogMessage<T>>>();
            List<LogMessage<T>> messages = new List<LogMessage<T>>();
            //while (s.Position < s.Length)
            while (s.CanRead)
            {
                try
                {
                    object o = f.Deserialize(s);
                    if (!"break".Equals(o))
                    {
                        LogMessage<T> message = (LogMessage<T>)o;
                        messages.Add(message);
                    }
                    else if (ReturnEmptySequence || messages.Count > 0)
                    {
                        rtn.Add(messages);
                        messages = new List<LogMessage<T>>();
                    }
                }
                catch (SerializationException e)
                {
                    if (e.Message.StartsWith("End of Stream"))
                        break;
                    else
                        throw e;
                }
            }
            if (ReturnEmptySequence || messages.Count > 0)
            {
                rtn.Add(messages);
                messages = new List<LogMessage<T>>();
            }
            s.Close();
            return rtn;
        }
    }
}
