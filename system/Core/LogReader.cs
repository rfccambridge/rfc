using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Robocup.Core {
  
    public class LogReader {
        List<Object> _loggedItems = new List<object>();
        TextReader _textReader = null;
        List<Type> _lineFormat = null;                       // types of objects in a line of a log file

        public bool LogOpen
        {
            get { return (_textReader != null); }
        }

        public void OpenLogFile(string path, List<Type> lineFormat)
        {
            if (_textReader != null)
                throw new ApplicationException("A log file is already open.");

            _textReader = new StreamReader(path);
            _lineFormat = lineFormat;
        }

        public void CloseLogFile()
        {
            if (_textReader != null)
                _textReader.Close();

            _textReader = null;
        }

        public List<Object> GetLoggedItems() {
            return _loggedItems;
        }

        public void Next()
        {
            if (_textReader == null)
                throw new ApplicationException("Log file not open.");

            string line = _textReader.ReadLine();
            if (line != null) // if not eof
            {
                parseLogLine(line);
            }                     
        }
        
        public void Prev() {
            throw new ApplicationException("not implemented");
        }

        private void parseLogLine(string line) {
            string[] items = line.Split('|');

            if (items.Length != _lineFormat.Count) {
                throw new ApplicationException("Data in log file does not match line format specified.");
            }

            _loggedItems.Clear();

            for (int i = 0; i < _lineFormat.Count; i++)
            {                                
                Object itemObj = parseItem(items[i], _lineFormat[i]);
                _loggedItems.Add(itemObj);              
            }
        }

        private Object parseItem(string str, Type type)
        {
            Object obj = null;
            string[] items;

            // Apparently it is not legal to switch on Type.

            switch (type.Name)
            {
                case "DateTime":  // NOTE: this assumes a particular custom string representation             
                    items = str.Split(':');
                    obj = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                                             int.Parse(items[0]), int.Parse(items[1]), int.Parse(items[2]),
                                             int.Parse(items[3]));
                    break;

                case "RobotInfo":
                    items = str.Split('#');

                    Vector2 position = (Vector2)parseItem(items[0], typeof(Vector2));
                    Vector2 velocity = (Vector2)parseItem(items[1], typeof(Vector2));
                    double rotVelocity = double.Parse(items[2]);
                    double orientation = double.Parse(items[3]);
                    int id = int.Parse(items[4]);

                    obj = new RobotInfo(position, velocity, rotVelocity, orientation, id);
                    break;

                case "Vector2":
                    items = (str.Substring(1, str.Length - 2)).Split(','); // strip the "<" and ">"
                    obj = new Vector2(double.Parse(items[0]), double.Parse(items[1]));
                    break;

                case "WheelSpeeds":
                    items = (str.Substring(1, str.Length - 2)).Split(','); // strip the "{" and "}"
                    obj = new WheelSpeeds(int.Parse(items[0]), int.Parse(items[1]),
                                          int.Parse(items[2]), int.Parse(items[3]));
                    break;

                case "RobotPath":
                    items = str.Split('&');
                    List<RobotInfo> path = new List<RobotInfo>();
                    foreach (string item in items)
                        path.Add((RobotInfo)parseItem(item, typeof(RobotInfo)));
                    obj = new RobotPath(path);
                    break;                    

                default:
                    throw new ApplicationException("Parsing type " + type.ToString() + " is not implemented.");
            }

            return obj;
        }
    }
}
