using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;

namespace Robocup.RRT
{
    public class RRTPlanner : IMotionPlanner
    {
        private class ExtendResults<T>
        {
            public T extension;
            public enum ResultType
            {
                Success, Blocked, Destination
            }
            public ResultType resultType;
            public ExtendResults(T extension, ResultType type)
            {
                this.extension = extension;
                this.resultType = type;
            }
        }

        private Dictionary<int, RRTIndvPlanner> planners = new Dictionary<int, RRTIndvPlanner>();
        public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius)
        {
            if (!planners.ContainsKey(id))
                planners.Add(id, new RRTIndvPlanner());
            return planners[id].PlanMotion(id, desiredState, predictor, avoidBallRadius);
        }

        const int MAX_ITERATIONS = 2000;

        private class RRTIndvPlanner : IMotionPlanner
        {
            ExtendResults<Vector2> Extend(Vector2 original, Vector2 destination)
            {
                throw new Exception("The method or operation is not implemented.");
            }
            ExtendResults<Vector2> Extend(RobotInfo original, RobotInfo destination)
            {
                throw new Exception("The method or operation is not implemented.");
            }
            ExtendResults<Vector2> Extend(RobotInfo original, Vector2 destination)
            {
                throw new Exception("The method or operation is not implemented.");
            }
            RobotInfo GenRandInfo()
            {
                throw new Exception("The method or operation is not implemented.");
            }
            Vector2 GenRandPoint()
            {
                throw new Exception("The method or operation is not implemented.");
            }
            public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius)
            {
                RobotInfoNNFinder riFinder = new RobotInfoNNFinder();
                riFinder.AddInfo(predictor.getCurrentInformation(id));
                Vector2NNFinder v2Finder = new Vector2NNFinder();
                v2Finder.AddPoint(desiredState.Position);
                for (int i = 0; i < MAX_ITERATIONS; i++)
                {
                }
                throw new Exception("The method or operation is not implemented.");
            }
        }
    }
}
