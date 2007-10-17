using System;
using System.Collections.Generic;
using System.Text;
using Robocup.CoreRobotics;
using Robocup.Utilities;
using Robocup.Core;
using MachineLearning;

namespace Robocup.MotionControl
{
    public class RobotModelCalibrator
    {
        public MovementModeler CalibrateModel(List<List<LogMessage<VisionDataMessage.RobotData>>> visionData,
            List<List<LogMessage<WheelSpeeds>>> commands)
        {
            System.Diagnostics.Debug.Assert(visionData.Count == commands.Count);

            visionData.Sort();
            commands.Sort();

            SimulatedAnnealing<MovementModeler> optimizer =
                new SimulatedAnnealing<MovementModeler>(delegate(MovementModeler m)
            {
                return ScoreModel(m, visionData, commands);
            });
            optimizer.setGenFunction(GenerateNext);
            optimizer.setTermFunction(SomeTerminationFunctions.repeatedTermClass<MovementModeler>(100));
            optimizer.minimize();
            return optimizer.getBest();
        }
        /// <summary>
        /// Gets triggered when a new path has been generated and scored.  The second argument is the score.
        /// </summary>
        public event Action<Pair<List<SimulatedPath>, double>> PathScored;
        private double ScoreModel(MovementModeler model, List<List<LogMessage<VisionDataMessage.RobotData>>> visionData,
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



        //Things that Jieyun needs to write:
        private SimulatedPath SimulateCommands(MovementModeler model, List<LogMessage<VisionDataMessage.RobotData>> visionData,
            List<LogMessage<WheelSpeeds>> commands)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        private double ScorePath(SimulatedPath path, List<LogMessage<VisionDataMessage.RobotData>> visionData)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        private MovementModeler GenerateNext(MovementModeler current, double temperature)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public class SimulatedPath
        {
        }
    }
}
