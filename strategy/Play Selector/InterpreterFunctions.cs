using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Infrastructure;

namespace RobocupPlays
{
    static class InterpreterFunctions
    {


        static bool added = false;
        static public bool Added
        {
            get { return added; }
        }
        static public void addFunctions()
        {
            if (added)
                return;
            added = true;
            /*Function.addFunction("ball", "ball", typeof(InterpreterPoint), new Type[] { }, "the ball", delegate(object[] objects)
            {
                return new InterpreterPoint(new InterpreterBall());
            });*/
            Function.addFunction("closest", "closest", typeof(InterpreterRobotDefinition), new Type[] { typeof(TeamCondition), typeof(Vector2), typeof(RobotAssignmentType) }, "the robot on ~ closest to ~", delegate(EvaluatorState state, object[] objects)
            {
                return new InterpreterClosestDefinition(((TeamCondition)objects[0]).maybeOurs(), (Vector2)objects[1], (RobotAssignmentType)objects[2]);
            });
            Function.addFunction("closest-with-tags", "closest-with-tags", typeof(InterpreterRobotDefinition), new Type[] { typeof(TeamCondition), typeof(Vector2), typeof(RobotAssignmentType), typeof(string) }, "the robot on ~ closest to ~, with comma-separated tags ~", delegate(EvaluatorState state, object[] objects)
            {
                List<string> tags_list = new List<string>();
                tags_list.AddRange(((string)objects[3]).Split(','));
                return new InterpreterClosestDefinition(
                    ((TeamCondition)objects[0]).maybeOurs(), (Vector2)objects[1], (RobotAssignmentType)objects[2], tags_list);
            });
            //Function.addFunction("orderforcer","orderforcer",typeof(orderForcer
        }
    }
}
