using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Collections;
using Robocup.Core;

namespace Robocup.Plays
{
    class InterpreterBall : PlayBall
    {
        public override Vector2 getPoint()
        {
            if (evaluatorstate.ballInfo != null)
                return evaluatorstate.ballInfo.Position;
            return Vector2.ZERO;
        }
        public override Vector2 getVelocity()
        {
            if (evaluatorstate.ballInfo != null)
                return evaluatorstate.ballInfo.Velocity;
            return Vector2.ZERO;
        }
        private EvaluatorState evaluatorstate = null;
        public void setEvaluatorState(EvaluatorState evaluatorstate)
        {
            this.evaluatorstate = evaluatorstate;
        }
    }
}
