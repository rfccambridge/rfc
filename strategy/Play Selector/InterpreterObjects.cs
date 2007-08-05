using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Collections;
using Robocup.Infrastructure;

namespace RobocupPlays
{
    class InterpreterRobot : PlayRobot, InterpreterGetPointable
    {
        public InterpreterRobot(InterpreterRobotDefinition definition)
            : base(definition)
        {        }
        public override Vector2 getPoint()
        {
            return ((InterpreterRobotDefinition)definition).getPoint();
        }
        public void setEvaluatorState(EvaluatorState evaluatorstate)
        {
            ((InterpreterRobotDefinition)definition).setEvaluatorState(evaluatorstate);
        }
        public bool define()
        {
            return ((InterpreterRobotDefinition)definition).define();
        }
        public bool canForceDefine(int id)
        {
            return ((InterpreterRobotDefinition)definition).canForceDefine(id);
        }
        public void forceDefine(int id)
        {
            ((InterpreterRobotDefinition)definition).forceDefine(id);
        }
        public InterpreterRobot Save()
        {
            return new InterpreterRobot(new InterpreterSavedDefinition(this.Ours, this.getID()));
        }
    }
    class InterpreterBall : PlayBall, InterpreterGetPointable
    {
        public override Vector2 getPoint()
        {
            return evaluatorstate.ballInfo.Position;
        }
        private EvaluatorState evaluatorstate = null;
        public void setEvaluatorState(EvaluatorState evaluatorstate)
        {
            this.evaluatorstate = evaluatorstate;
        }
    }
}
