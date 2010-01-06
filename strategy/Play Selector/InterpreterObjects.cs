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
            {
                return evaluatorstate.ballInfo.Position;
            }
            // otherwise, arbitrarily return <100, 100>
            return new Vector2(0, 0);
        }
        private EvaluatorState evaluatorstate = null;
        public void setEvaluatorState(EvaluatorState evaluatorstate)
        {
            this.evaluatorstate = evaluatorstate;
        }
    }
}
