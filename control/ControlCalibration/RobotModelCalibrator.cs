using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Geometry;
using Robocup.CoreRobotics;
using Robocup.Utilities;
using Robocup.Core;
using MachineLearning;

namespace Robocup.MotionControl
{
    public class RobotModelCalibrator
    {
        public MovementModeler CalibrateModel(List<List<LogMessage<VisionMessage.RobotData>>> visionData,
            List<List<LogMessage<WheelSpeeds>>> commands)
        {
            System.Diagnostics.Debug.Assert(visionData.Count == commands.Count);

            for (int i = 0; i < visionData.Count; i++)
            {
                visionData[i].Sort();
            }
            for (int i = 0; i < commands.Count; i++)
            {
                commands[i].Sort();
            }

            SimulatedAnnealing<MovementModeler> optimizer =
                new SimulatedAnnealing<MovementModeler>(delegate(MovementModeler m)
            {
                return ScoreModel(m, visionData, commands);
            });
            optimizer.setGenFunction(GenerateNext);
            optimizer.setTermFunction(SomeTerminationFunctions.repeatedTermClass<MovementModeler>(2500));
            optimizer.setCoolingFactor(.001);
            optimizer.NumThreads = Environment.ProcessorCount;
            optimizer.setTemp(1);
            optimizer.minimize();
            return optimizer.getBest();
        }
        /// <summary>
        /// Gets triggered when a new path has been generated and scored.  The second argument is the score.
        /// </summary>
        public event Action<Pair<List<SimulatedPath>, double>> PathScored;
        private double ScoreModel(MovementModeler model, List<List<LogMessage<VisionMessage.RobotData>>> visionData,
            List<List<LogMessage<WheelSpeeds>>> commands)
        {
            List<SimulatedPath> paths = new List<SimulatedPath>();
            double total_score = 0;
            for (int i = 0; i < visionData.Count; i++)
            {
                SimulatedPath path = SimulateCommands(model, visionData[i], commands[i]);
                double score = ScorePath(path, visionData[i]);
                paths.Add(path);
                total_score += score;
            }
            if (PathScored != null)
            {
                PathScored(new Pair<List<SimulatedPath>, double>(paths, total_score));
            }
            return total_score;
        }

        /// <summary>
        /// A node in the (simulated) path of a robot
        /// </summary>
        public class PathNode
        {
            public Vector2 position;
            public Vector2 velocity;
            public double angularVelocity;
            public double orientation;
            public double time;
            public int id;

            public PathNode(Vector2 position, Vector2 velocity, double angularVelocity, double orientation,
                                double time, int id)
            {
                this.position = position;
                this.velocity = velocity;
                this.angularVelocity = angularVelocity;
                this.orientation = orientation;
                this.time = time;
                this.id = id;
            }

            public PathNode()
            {
            }
        }

        //A sequence of nodes representing a (simulated) path of a robot.  The nodes will be in one-to-one correspondance
        //with the vision messages corresponding to this path
        public class SimulatedPath
        {
            public List<PathNode> route = new List<PathNode>();
        }

        // Converts a PathNode to a RobotInfo
        private RobotInfo GetInfoFromPathNode(PathNode node)
        {
            return new RobotInfo(node.position, node.velocity, node.angularVelocity, node.orientation, node.id);
        }

        // Converts a RobotInfo to a PathNode
        private PathNode GetPathNodeFromInfo(RobotInfo info, double time)
        {
            return new PathNode(info.Position, info.Velocity, info.AngularVelocity, info.Orientation, time, info.ID);
        }

        // Simulates a path given a model, commands, and vision data (which just gives the initial state and times for modeling).
        // Returns a path with nodes at the vision message times
        private SimulatedPath SimulateCommands(MovementModeler model, List<LogMessage<VisionMessage.RobotData>> visionData,
            List<LogMessage<WheelSpeeds>> commands)
        {
            SimulatedPath path = new SimulatedPath();

            PathNode firstNode = new PathNode();

            // set up initial state
            firstNode.id = visionData[0].obj.ID;
            firstNode.orientation = visionData[0].obj.Orientation;
            firstNode.position = visionData[0].obj.Position;
            firstNode.time = visionData[0].time;
            firstNode.velocity = new Vector2();
            firstNode.angularVelocity = 0;
            path.route.Add(firstNode);

            //if there is no command before the first vision message, 
            //add a sentinal command to keep it going at its initial velocity
            //(assume that it was in a steady-state)
            //(this is probably a bad assumption, but the effects should be small)
            if (commands[0].time > visionData[0].time)
            {
                LogMessage<WheelSpeeds> sentinal = new LogMessage<WheelSpeeds>();
                sentinal.obj = (WheelSpeeds)model.GetWheelSpeedsFromInfo(GetInfoFromPathNode(firstNode));
                sentinal.time = visionData[0].time - .01;
                commands.Insert(0, sentinal);
            }

            //this is the next vision command that has not been modeled until;
            //ie we've modeled the robot state at the times visionData[i].time for all i<j
            //and added nodes with those times to the path
            int j = 1;

            //We go through the lists of vision and command data, and build up a path.
            //We do this by holding a "current state", which is valid at a given time.
            //Looking at a strictly increasing set of time values, we can then determine
            //the state at any of those times by modeling the state from the "current time"
            //to the desired time, and updating the state/time appropriately
            RobotInfo currentState = GetInfoFromPathNode(firstNode);
            double currenttime = firstNode.time;

            //first, break the simulation into time ranges based on commands, and for each range:
            //  -look at each vision update in the range, and model (as above) until that point (and add to path)
            //  -model until the end of the range
            for (int i = 0; i < commands.Count; i++)
            {
                WheelSpeeds currentCommand = commands[i].obj;

                //iterate over all vision messages that are under the effect of this command,
                //ie all messages until the next command is sent (or all remaining messages if there is no next command)
                //and for each vision message, continue modeling until that point, and add the node to the path
                while ((j < visionData.Count) && ((i + 1 == commands.Count) || (visionData[j].time < commands[i + 1].time)))
                {
                    currentState = model.ModelWheelSpeeds(currentState, currentCommand, visionData[j].time - currenttime);
                    currenttime = visionData[j].time;
                    PathNode NewNode = GetPathNodeFromInfo(currentState, currenttime);
                    path.route.Add(NewNode);
                    j++;
                }

                //model until the end of the range (if there is an end); though there is a case that the current time is after the range
                //(if the first vision message is after the first couple command messages)
                if (i + 1 < commands.Count && commands[i + 1].time > currenttime)
                {
                    currentState = model.ModelWheelSpeeds(currentState, currentCommand, commands[i + 1].time - currenttime);
                    currenttime = commands[i + 1].time;
                }
            }

            return path;
        }

        // Scores the simulated path against the vision data, assuming that nodes i in the vision and path correspond to each other
        private double ScorePath(SimulatedPath path, List<LogMessage<VisionMessage.RobotData>> visionData)
        {
            double answer = 0;
            for (int i = 0; i < path.route.Count; i++)
            {
                Vector2 temp = (path.route[i].position - visionData[i].obj.Position);
                answer += temp.magnitudeSq();// / (i + 3);
            }
            return answer;
        }

        Random ran = new Random();
        // Changes the given value by up to maxchange in either the positive or negative direction
        private void NextValue(ref double original, double maxchange)
        {
            double delta = (ran.NextDouble() - .5) * 2 * maxchange;
            original = original + delta;
        }

        private MovementModeler GenerateNext(MovementModeler current, double temperature)
        {
            MovementModeler rtn = new MovementModeler();
            if (current == null)
            {
                return rtn;
            }
            rtn.changeConstlb = current.changeConstlb;
            rtn.changeConstlf = current.changeConstlf;
            rtn.changeConstrb = current.changeConstrb;
            rtn.changeConstrf = current.changeConstrf;
            NextValue(ref rtn.changeConstlb, temperature / 2 + .01);
            NextValue(ref rtn.changeConstlf, temperature / 2 + .01);
            NextValue(ref rtn.changeConstrb, temperature / 2 + .01);
            NextValue(ref rtn.changeConstrf, temperature / 2 + .01);

            return rtn;
        }
    }
}
