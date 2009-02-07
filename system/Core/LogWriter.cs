using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;

namespace Robocup.Core
{
    public class LogWriter
    {
        TextWriter _textWriter = null;

        public void OpenLogFile(string path){
            if (_textWriter != null)
                throw new ApplicationException("Log file already open");

            _textWriter = new StreamWriter(path);
        }
        public void CloseLogFile() {
            if (_textWriter == null)
                return;

            _textWriter.Close();
            _textWriter = null;
        }
        public void LogItems(List<Object> items)
        {
            for (int i = 0; i < items.Count - 1; i++)
                _textWriter.Write(objToString(items[i]) + "|");
            _textWriter.Write(objToString(items[items.Count - 1]));

            _textWriter.WriteLine();
            
        }

        public string objToString(Object obj)
        {
            // Get the type name from the full namespace: e.g. System.DateTime => DateTime
          /*  string fullType = obj.GetType().ToString();
            string[] typeItems = fullType.Split('.');
            string type = typeItems[typeItems.Length - 1];*/


            // Apparently, we cannot switch on Type

            // Specify any custom to string conversion for storage in the log here
            switch (obj.GetType().Name)
            {
                case "DateTime":
                    DateTime dtObj = (DateTime)obj;
                    return dtObj.Hour.ToString() + ":" + dtObj.Minute.ToString() + ":" +
                           dtObj.Second.ToString() + ":" + dtObj.Millisecond.ToString();                    

                case "RobotInfo":
                    RobotInfo info = (RobotInfo)obj;                    
                    return info.Position.ToString() + "#" + info.Velocity.ToString() + "#" +
                           info.AngularVelocity.ToString() + "#" + info.Orientation.ToString() + "#" +
                           info.ID.ToString();

                case "RobotPath":
                    string str = "";
                    RobotPath path = (RobotPath)obj;                    
                    foreach (RobotInfo waypoint in path.Waypoints)
                    {
                        str += objToString(waypoint) + "&";
                    }
                    str = str.Substring(0, str.Length - 1); // remove the last connector char
                    return str;

                default:
                    return obj.ToString();

            }
        }
    }
}
