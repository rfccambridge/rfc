using System;
using System.Collections.Generic;
using System.Text;

namespace MachineLearning
{

    public static class SomeTerminationFunctions
    {
        static public SingleTerminationFunction<T> repeatedTermClass<T>(int numtimes) where T : class
        {
            int numsame = 0;
            T last = default(T);
            return delegate(T t, double score)
            {
                if (object.ReferenceEquals(t, last))
                    numsame++;
                else
                {
                    last = t;
                    numsame = 0;
                }
                return numsame >= numtimes;
            };
        }
        static public SingleTerminationFunction<T> repeatedTermStruct<T>(int numtimes) where T : struct
        {
            int numsame = 0;
            T last = default(T);
            return delegate(T t, double score)
            {
                if (ValueType.Equals(t, last))
                    numsame++;
                else
                {
                    last = t;
                    numsame = 0;
                }
                return numsame >= numtimes;
            };
        }
    }
}
