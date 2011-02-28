using System;
using System.Collections.Generic;
using System.Text;

namespace Robocup.MotionControl {
    /// <summary>
    /// Represents a range of numbers that can be added to or eliminated from
    /// </summary>
    public class NumberRange {
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
        public NumberRange(double lowLimit, double highLimit, int resolution) {
            _lowLimit = lowLimit;
            _highLimit = highLimit;
            _resolution = resolution;
            _stepSize = (highLimit - lowLimit) / resolution;
            _lst = new List<bool>();

            //set all elements in lst to true
            for (int i = 0; i < resolution; i++) {
                _lst.Add(true);
            }
        }

        /// <summary>
        /// removes the range between "from" and "to" from the _lst 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void Remove(double from, double to) {
            // find out what bins these correspond to
            int fromstep = (int) Math.Max((int)((from - _lowLimit) / _stepSize), 0);
            int tostep = (int) Math.Min((int)((to - _lowLimit) / _stepSize), _resolution-1);

            // remove these steps and in between
            for (int i = fromstep; i <= tostep; i++) {
                _lst[i] = false;
            }
        }

        public void Add(double from, double to) {
            // find out what bins these correspond to
            int fromstep = valueToStep(from);
            int tostep = valueToStep(to);

            // add these steps and in between
            for (int i = fromstep; i <= tostep; i++) {
                _lst[i] = true;
            }
        }

        /// <summary>
        /// returns the closest values to center on both sides (in a certain range)
        /// if there are no items on a side, returns the respective limit value 
        /// </summary>
        /// <param name="minVal">minimum allowed value</param>
        /// <param name="maxVal">maximum allowed value</param>
        /// <param name="closestLess">closest val in direction of min (if none == minVal)</param>
        /// <param name="closestMore">closest val in direction of max (if none == maxVal)</param>
        public void ClosestToCenter(double minVal, double maxVal, out double closestLess, out double closestMore) {
            closestLess = minVal;
            closestMore = maxVal;
            
            int centerStep = _resolution / 2;
            int minstep = valueToStep(minVal);
            int maxstep = valueToStep(maxVal);
            for (int i = 0; i < _resolution / 2; i++)
            {
                if (closestLess != minVal && closestMore != maxVal)
                    break;

                if ((closestMore == maxVal) && _lst[centerStep + i]) 
                {
                    if (centerStep + i >= minstep && centerStep + i <= maxstep) {
                        closestMore = stepToValue(centerStep + i);
                    }
                }
                if ((closestLess == minVal) && _lst[centerStep - i])
                {
                    if (centerStep - i >= minstep && centerStep - i <= maxstep) {
                        closestLess = stepToValue(centerStep - i);
                    }
                }
            }
        }

        private double stepToValue(int step)
        {
            return _lowLimit + step * _stepSize;
        }
        private int valueToStep(double value)
        {            
            return Math.Min(Math.Max((int)((value - _lowLimit) / _stepSize), 0), _resolution-1);
        }
    }
}
