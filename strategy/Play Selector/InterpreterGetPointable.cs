using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Collections;

namespace RobocupPlays
{
    interface InterpreterGetPointable : GetPointable
    {
        //void setEvaluatorState(EvaluatorState evaluatorstate);
    }
    /*class InterpreterLineLineIntersection : PlayLineLineIntersection, InterpreterGetPointable
    {
        public InterpreterLineLineIntersection(InterpreterLine l1, InterpreterLine l2)
            :
            base(l1, l2) { }

        private EvaluatorState evaluatorstate = null;
        public void setEvaluatorState(EvaluatorState evaluatorstate)
        {
            this.evaluatorstate = evaluatorstate;
        }
    }
    class InterpreterLineCircleIntersection : PlayLineCircleIntersection, InterpreterGetPointable
    {
        public InterpreterLineCircleIntersection(InterpreterLine l, InterpreterCircle c, int whichlineintersection)
            :
            base(l, c, whichlineintersection) { }

        private EvaluatorState evaluatorstate = null;
        public void setEvaluatorState(EvaluatorState evaluatorstate)
        {
            this.evaluatorstate = evaluatorstate;
        }
    }
    class InterpreterCircleCircleIntersection : PlayCircleCircleIntersection, InterpreterGetPointable
    {
        public InterpreterCircleCircleIntersection(InterpreterCircle c1, InterpreterCircle c2, int whichlineintersection)
            :
            base(c1, c2, whichlineintersection) { }

        private EvaluatorState evaluatorstate = null;
        public void setEvaluatorState(EvaluatorState evaluatorstate)
        {
            this.evaluatorstate = evaluatorstate;
        }
    }*/
}
