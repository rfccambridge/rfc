using System;
using System.Collections.Generic;
using System.Text;

namespace Robocup.Core
{
    /// <summary>
    /// A pair of two elements.
    /// </summary>
    public class Pair<T1, T2>
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

    /// <summary>
    /// EventArguments class with arbitrary data item. Useful for custom events.
    /// </summary>
    /// <typeparam name="T">Type of the data item</typeparam>
    public class EventArgs<T> : EventArgs
    {
        T _data;
        public T Data
        {
            get { return _data; }
        }
        public EventArgs(T data)
        {
            _data = data;
        }
    }


    /// <summary>
    /// Threadsafe random number generator.
    /// Use this function to get your random numbers, rather than creating your own Random() instance!
    /// This is because a lot of the code in this program runs multiple times per second, meaning that 
    /// repeated instantiations of Random() might actually be seeded exactly the same. Moreover, Random() isn't
    /// threadsafe by itself, and there's a lot of threading going in certain parts of the code.
    /// </summary>
    /// <typeparam name="T">Type of the data item</typeparam>

    public static class RandGen
    {
        private static Random _global = new Random();
        private static int _gs = (int)0x0CAFEDAD;
        [ThreadStatic]
        private static Random _local;
        private static bool _hasNextGaussian = false;
        private static double _nextGaussian;

        public static void InitLocalIfNeeded()
        {
            if (_local == null)
            {
                int seed;
                lock (_global)
                {
                    seed = _global.Next();
                    seed += (_gs = (_gs = (_gs = _gs ^ (_gs << 17)) ^ (_gs >> 13)) ^ (_gs << 5));
                }
                _local = new Random(seed);
            }
        }

        //Returns a nonnegative random number.
        public static int Next()
        {
            InitLocalIfNeeded();
            return _local.Next();
        }

        //Returns a nonnegative random number less than the specified maximum.
        public static int Next(int maxValue)
        {
            InitLocalIfNeeded();
            return _local.Next(maxValue);
        }

        //Returns a random number within a specified range. [inclusive,exclusive)
        public static int Next(int minValue, int maxValue)
        {
            InitLocalIfNeeded();
            return _local.Next(minValue,maxValue);
        }

        //Returns a random number within a specified range. [inclusive,exclusive)
        public static double NextDouble()
        {
            InitLocalIfNeeded();
            return _local.NextDouble();
        }

        //Returns a normally distributed value with mean 0 and standard deviation 1
        public static double NextGaussian()
        {
            if (_hasNextGaussian)
            {
                _hasNextGaussian = false;
                return _nextGaussian;
            }
            else
            {
                double x, y, rsquared;
                do
                {
                    x = 2.0 * NextDouble() - 1.0;
                    y = 2.0 * NextDouble() - 1.0;
                    rsquared = x * x + y * y;
                } while (rsquared >= 1.0 || rsquared == 0.0);

                double polar = Math.Sqrt((-2.0) * Math.Log(rsquared) / rsquared);
                _nextGaussian = x * polar;
                _hasNextGaussian = true;
                return y * polar;
            }
        }
    }

}
