using System;
using System.Collections.Generic;
using System.Text;
using Vision;

namespace Robocup.Core {
    public class Path {
        public List<RobotInfo> Waypoints;        
        public Vector2 Destination;
    }
    public class GameState {
        public List<RobotInfo> OurRobots;
        public List<RobotInfo> TheirRobots;
        public BallInfo BallInfo;
        public Dictionary<int, Path> Paths; // RobotID -> path
        public Dictionary<int, Vector2> NextWaypoints; // RobotID -> next waypoint
    }
    public class LogReader {
        public GameState GetGameState();
        public void Next();
        public void Prev();

        private void parseLogLine(string line, out DateTime timestamp,
                                             out RobotInfo robotInfo, out RobotInfo desiredInfo,
                                             out Vector2 waypoint, out WheelSpeeds wheelSpeeds) {
            string[] items = line.Split(' ');

            // Timestamp
            string[] timeItems = items[0].Split(':');
            timestamp = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, int.Parse(timeItems[0]), int.Parse(timeItems[1]),
                                              int.Parse(timeItems[2]));

            // Robotinfo
            string[] positionItems = (items[2].Substring(1, items[2].Length - 2)).Split(','); // strip the "<" and ">"
            string[] velocityItems = (items[4].Substring(1, items[4].Length - 2)).Split(',');
            robotInfo = new RobotInfo(new Vector2(double.Parse(positionItems[0]), double.Parse(positionItems[1])),
                                      new Vector2(double.Parse(velocityItems[0]), double.Parse(velocityItems[1])),
                                      0, double.Parse(items[3]), int.Parse(items[1]));

            // DesiredInfo
            positionItems = (items[6].Substring(1, items[6].Length - 2)).Split(','); // strip the "<" and ">"
            velocityItems = (items[8].Substring(1, items[8].Length - 2)).Split(',');
            desiredInfo = new RobotInfo(new Vector2(double.Parse(positionItems[0]), double.Parse(positionItems[1])),
                                      new Vector2(double.Parse(velocityItems[0]), double.Parse(velocityItems[1])),
                                      0, double.Parse(items[7]), int.Parse(items[5]));

            // Waypoint
            string[] waypointItems = (items[9].Substring(1, items[9].Length - 2)).Split(','); // strip the "<" and ">"
            waypoint = new Vector2(double.Parse(waypointItems[0]), double.Parse(waypointItems[1]));

            // WheelSpeeds
            string[] wheelsItems = (items[10].Substring(1, items[10].Length - 2)).Split(','); // strip the "{" and "}"
            wheelSpeeds = new WheelSpeeds(int.Parse(wheelsItems[0]), int.Parse(wheelsItems[1]),
                                          int.Parse(wheelsItems[2]), int.Parse(wheelsItems[3]));
        }
    }
}
