using System;
using System.Collections.Generic;
using System.Text;

namespace RobocupPlays
{
    public class InterpreterExpression : Expression
    {

        public bool IsRobotOnOurTeam()
        {
            if (ReturnType != typeof(InterpreterRobotDefinition))
                throw new ApplicationException("You can't get the team of a robot, from an expression that's not a robot!");
            return ((TeamCondition)((InterpreterExpression)Arguments[0]).getValue(-1, null)).maybeOurs();
            //return ((InterpreterRobot)e.getValue(-1)).Ours;
        }

        public InterpreterExpression(object value) : base(value) { }
        public InterpreterExpression(Function f, params object[] args) : base(f, args) { }

        //private InterpreterExpression savedExpression = null;
        /*internal void clearSaved()
        {
            savedExpression = null;
        }*/
        /*internal InterpreterExpression Save(bool saveours, bool savetheirs)
        {
            if (!saveours && !savetheirs)
                return this;
            throw new ApplicationException("Not working yet!  Don't choose to save the actions");
            if (savedExpression == null)
            {
                if (IsFunction)
                {
                    object[] savedArgs = new object[Arguments.Length];
                    for (int i = 0; i < Arguments.Length; i++)
                    {
                        InterpreterExpression e = Arguments[i] as InterpreterExpression;
                        if (e == null)
                        {
                            InterpreterRobot r = Arguments[i] as InterpreterRobot;
                            if (r == null)
                                savedArgs[i] = Arguments[i];
                            else if (r.Ours && saveours)
                                savedArgs[i] = r.Save();
                            else if (!r.Ours && savetheirs)
                                savedArgs[i] = r.Save();
                            else
                                savedArgs[i] = Arguments[i];
                        }
                        else
                            savedArgs[i] = e.Save(saveours,savetheirs);
                    }
                    savedExpression = new InterpreterExpression(theFunction, savedArgs);
                }
                else //if (!IsFunction)
                {
                    InterpreterRobot r = this.StoredValue as InterpreterRobot;
                    if (r == null)
                        savedExpression = this;
                    else
                        savedExpression = new InterpreterExpression(r.Save());
                }
            }
            return savedExpression;
        }*/

    }
}
