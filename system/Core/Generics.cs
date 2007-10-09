using System;
using System.Collections.Generic;
using System.Text;

namespace Robocup.Core
{
    /// <summary>
    /// A pair of two elements.
    /// </summary>
    class Pair<T1, T2>
    {
        public Pair(T1 first, T2 second)
        {
            this.first = first;
            this.second = second;
        }

        private T1 first;
        public T1 First
        {
            get { return first; }
            set { first = value; }
        }

        private T2 second;
        public T2 Second
        {
            get { return second; }
            set { second = value; }
        }
    }

    /// <summary>
    /// A fuction who's purpose is to produce a value; ie it does not take any parameters.
    /// The value does not need to stay constant over time.
    /// </summary>
    public delegate T ValueFunction<T>();
}
