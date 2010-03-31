using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using System.ComponentModel;

namespace Robocup.SerialControl.WheelSpeedFunctions
{
    public interface IWheelSpeedFunction
    {
        WheelSpeeds Eval(double t);
        void ClearState();
    }

    class ChangingSine : IWheelSpeedFunction
    {
        public ChangingSine()
        {
            period = startPeriod;
        }        

        private double amplitude = 40;
        public double Amplitude
        {
            get { return amplitude; }
            set { amplitude = value; }
        }
        private double startPeriod = 0.02;
        [Description("The initial period of the sine wave, in seconds")]
        public double StartPeriod
        {
            get { return startPeriod; }
            set { startPeriod = value; }
        }

        private double period;

        private double endPeriod = 10.0;
        [Description("The final period of the sine vawe, in seconds")]
        public double EndPeriod
        {
            get { return endPeriod; }
            set { endPeriod = value; }
        }

        private double periodStep = 2;
        [Description("Increment step of period (multipled by this)")]
        public double PeriodStep
        {
            get { return periodStep; }
            set { periodStep = value; }
        }

        private int numIter = 5;
        [Description("Number of periods using the same frequency")]
        public int NumIter
        {
            get { return numIter; }
            set { numIter = value; }
        }

        private int currentIter = 0;
        private double lastPeriodTime = 0;        

        public WheelSpeeds Eval(double t)
        {
            int speed;
            if (period > endPeriod)
                speed = 0;
            else
                speed = (int)(amplitude * Math.Sin(t / period * 2 * Math.PI));
            currentIter = Convert.ToInt32(Math.Floor((t - lastPeriodTime) / period));
            if (currentIter > numIter)
            {
                currentIter = 0;
                period *= periodStep;
                lastPeriodTime = t;
            }
            return new WheelSpeeds(speed, speed, speed, speed);
        }

        public void ClearState()
        {
            period = startPeriod;
            currentIter = 0;
        }
    }

    class Sine : IWheelSpeedFunction
    {        
        private double amplitude = 40;
        public double Amplitude
        {
            get { return amplitude; }
            set { amplitude = value; }
        }
        private double period = 2.0;
        [Description("The period of the sine wave, in seconds")]
        public double Period
        {
            get { return period; }
            set { period = value; }
        }

        public WheelSpeeds Eval(double t)
        {
            int speed = (int)(amplitude * Math.Sin(t / period * 2 * Math.PI));
            return new WheelSpeeds(speed, speed, speed, speed);
        }

        public void ClearState() { }
    }
    
    class Step : IWheelSpeedFunction
    {       
        private double initialpower = 0;
        public double InitialPower
        {
            get { return initialpower; }
            set { initialpower = value; }
        }

        private double finalpower = 40;
        public double Finalpower
        {
            get { return finalpower; }
            set { finalpower = value; }
        }

        private double transition = 2;
        [Description("The time at which it transitions from the initial value to the final value")]
        public double TransitionTime
        {
            get { return transition; }
            set { transition = value; }
        }

        public WheelSpeeds Eval(double t)
        {
            int speed = (int)(t > transition ? finalpower : initialpower);
            return new WheelSpeeds(speed, speed, speed, speed);
        }

        public void ClearState() { }
    }

    public class Ramp : IWheelSpeedFunction
    {
        private double waittime = .1;

        public double WaitTime
        {
            get { return waittime; }
            set { waittime = value; }
        }
        private int start = 0;

        public int Start
        {
            get { return start; }
            set { start = value; }
        }
        private int change = 0;

        public int Change
        {
            get { return change; }
            set { change = value; }
        }

        public WheelSpeeds Eval(double t)
        {
            int num = (int)(t / waittime);
            int speed = start + num * change;
            return new WheelSpeeds(speed, speed, speed, speed);
        }

        public void ClearState() { }
    }

    /// <summary>
    /// A square wave- goes full speed one direction to other, alternating in given period
    /// </summary>
    public class Square : IWheelSpeedFunction
    {        
        private double amplitude = 20;
        [Description("The amplitude of the square wave- max wheel command it gives")]

        public double Amplitude
        {
            get { return amplitude; }
            set { amplitude = value; }
        }
        private double period = 2.0;
        [Description("The period of the square wave (single peak, one direction), in seconds")]
        public double Period
        {
            get { return period; }
            set { period = value; }
        }

        public WheelSpeeds Eval(double t)
        {
            // Compute whether this is in an "odd" or an "even" phase
            int mode = (int)(t / period) % 2;
            int speed = 0;

            if (mode == 0)
            {
                speed = (int)amplitude;
            }
            else
            {
                speed = -(int)amplitude;
            }
            return new WheelSpeeds(speed, speed, speed, speed);
        }

        public void ClearState() { }
    }
}
