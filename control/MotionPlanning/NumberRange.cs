using System;
using System.Collections.Generic;
using System.Text;

namespace Robocup.MotionControl
{
    /// <summary>
    /// Represents a range of numbers that can be added to or eliminated from
    /// </summary>
    public class NumberRange
    {
        List<bool> _lst;
        double _lowLimit;
        double _highLimit;
        double _stepSize;
        int _resolution;

        /// <summary>
        /// Stores a range of possible numbers given a lower bound and a higher bound. 
        /// resolution is the number of steps between them.
        /// </summary>
        /// <param name="lowLimit"></param>
        /// <param name="highLimit"></param>
        /// <param name="resolution"></param>
        public NumberRange(double lowLimit, double highLimit, int resolution)
        {
            _lowLimit = lowLimit;
            _highLimit = highLimit;
            _resolution = resolution;
            _stepSize = (highLimit - lowLimit) / resolution;
            _lst = new List<bool>(resolution);

            //set all elements in lst to true
            for (int i = 0; i < resolution; i++)
            {
                _lst[i] = true;
            }
        }

        /// <summary>
        /// removes the range between "from" and "to" from the _lst 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void remove(double from, double to)
        {
            // find out what bins these correspond to
            int fromstep = (int)((from - _lowLimit) / _stepSize);
            int tostep = (int)((to - _lowLimit) / _stepSize);

            // remove these steps and in between
            for (int i = fromstep; i <= tostep; i++)
            {
                _lst[i] = false;
            }
        }

        public void add(double from, double to)
        {
            // find out what bins these correspond to
            int fromstep = (int)((from - _lowLimit) / _stepSize);
            int tostep = (int)((to - _lowLimit) / _stepSize);

            // add these steps and in between
            for (int i = fromstep; i <= tostep; i++)
            {
                _lst[i] = true;
            }
        }
        // returns the degree
        // note: closestToCenter always find something for now
        public double closestToCenter()
        {
            int centerStep = _resolution / 2;
            for (int i = 0; i < _resolution / 2; i++)
            {
                if (_lst[centerStep + i])                    return (centerStep + i) * _stepSize;                if (_lst[centerStep - i])                    return (centerStep - i) * _stepSize;            }
            return (centerStep) * _stepSize; // this line is BAD!!!1222!!
        }

    }
}
