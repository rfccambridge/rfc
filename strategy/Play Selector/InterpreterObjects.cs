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
            return evaluatorstate.ballInfo.Position;
        }
        private EvaluatorState evaluatorstate = null;
        public void setEvaluatorState(EvaluatorState evaluatorstate)
        {
            this.evaluatorstate = evaluatorstate;
        }
    }
}
