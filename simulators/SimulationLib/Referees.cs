using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Robocup.Core;
using Robocup.CoreRobotics;

namespace Robocup.Simulation
{
    public delegate void GoalScored();
    public delegate void BallOut(Vector2 lastPosition);

    public interface IVirtualReferee
    {
        /// <summary>
        /// Takes as an argument the field state (in the form of a predictor object), and a way to move the ball;
        /// should move the ball and store (for retrieval through IReferee methods) the play type to be run.
        /// </summary>
        /// <param name="predictor">The IPredictor object that provides field state information</param>
        void RunRef(IPredictor predictor);
        void SetCurrentCommand(char commandToRun);
        /// <summary>
        /// Allows the automated referee to emit command sequences (f.e. stop->free_kick_blue)
        /// </summary>
        /// <param name="dealy">delay in wall clock (not simulated) time, in milliseconds</param>
        void EnqueueCommand(char command, int dealy);
        char GetLastCommand();

        event GoalScored GoalScored;
        event BallOut BallOut;
    }

    public class SimpleReferee : IVirtualReferee
    {
        private char command;
        private Object commandLock = new Object();
        private Queue<Pair<char, int>> commandQueue = new Queue<Pair<char, int>>();
        private System.Threading.Timer commandQueueTimer;

        public SimpleReferee()
        {
            command = MulticastRefBoxSender.HALT;
            commandQueueTimer = new System.Threading.Timer(CommandQueueTimer_Elapsed);
            commandQueueTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);        
        }

        public event GoalScored GoalScored;
        public event BallOut BallOut;

        public void RunRef(IPredictor predictor)
        {
            BallInfo ball = predictor.GetBall();
            if (ball == null)
                return;

            // Ball left the field
            if (ball.Position.X >= Constants.Field.XMAX || ball.Position.X <= Constants.Field.XMIN ||
                ball.Position.Y >= Constants.Field.YMAX || ball.Position.Y <= Constants.Field.YMIN)
            {

                // Check for goal
                if(ball.Position.Y <= Constants.Field.GOAL_YMAX && ball.Position.Y >= Constants.Field.GOAL_YMIN)
                {
                    if(ball.Position.X <= Constants.Field.XMIN && ball.Position.X >= Constants.Field.GOAL_XMIN)
                    {
                        if (GoalScored != null)
                            GoalScored();
                        return;
                    }

                    if(ball.Position.X >= Constants.Field.XMAX && ball.Position.X <= Constants.Field.GOAL_XMAX)
                    {
                        if (GoalScored != null)
                            GoalScored();
                        return;
                    }
                }

                // If no goal, ball is simply out
                if (BallOut != null)
                    BallOut(ball.Position);
            }
        }

        public void SetCurrentCommand(char commandToRun)
        {
            lock (commandLock)
            {
                command = commandToRun;
                commandQueue.Clear();
            }
        }

        public void EnqueueCommand(char command, int delay)
        {
            lock (commandLock)
            {
                commandQueue.Enqueue(new Pair<char, int>(command, delay));

                // If adding to head, enable queue timier
                if (commandQueue.Count == 1)
                    commandQueueTimer.Change(delay, System.Threading.Timeout.Infinite);
            }
        }

        private void CommandQueueTimer_Elapsed(Object stateInfo)
        {
            lock (commandLock)
            {
                if (commandQueue.Count > 0)
                    command = commandQueue.Dequeue().First;

                if (commandQueue.Count > 0)
                    commandQueueTimer.Change(commandQueue.Peek().Second, System.Threading.Timeout.Infinite);
                else
                    commandQueueTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            }
        }

        public char GetLastCommand()
        {
            lock (commandLock)
            {
                return command;
            }
        }
    }
}
