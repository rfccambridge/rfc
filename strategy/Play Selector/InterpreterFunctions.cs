using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Infrastructure;

namespace RobocupPlays
{
    static class InterpreterFunctions
    {
        static public List<Function> Functions()
        {
            List<Function> rtn = new List<Function>();
            /*Function.addFunction("ball", "ball", typeof(InterpreterPoint), new Type[] { }, "the ball", delegate(object[] objects)
            {
                return new InterpreterPoint(new InterpreterBall());
            });*/
            rtn.Add(new Function("closest", "closest", typeof(InterpreterRobotDefinition), new Type[] { typeof(TeamCondition), typeof(Vector2), typeof(RobotAssignmentType) }, "the robot on ~ closest to ~", delegate(EvaluatorState state, object[] objects)
            {
                return new InterpreterClosestDefinition(((TeamCondition)objects[0]).maybeOurs(), (Vector2)objects[1], (RobotAssignmentType)objects[2]);
            }));
            rtn.Add(new Function("closest-with-tags", "closest-with-tags", typeof(InterpreterRobotDefinition), new Type[] { typeof(TeamCondition), typeof(Vector2), typeof(RobotAssignmentType), typeof(string) }, "the robot on ~ closest to ~, with comma-separated tags ~", delegate(EvaluatorState state, object[] objects)
            {
                List<string> tags_list = new List<string>();
                tags_list.AddRange(((string)objects[3]).Split(','));
                return new InterpreterClosestDefinition(
                    ((TeamCondition)objects[0]).maybeOurs(), (Vector2)objects[1], (RobotAssignmentType)objects[2], tags_list);
            }));
            //Function.addFunction("orderforcer","orderforcer",typeof(orderForcer
            return rtn;
        }
    }
}
