using System.Collections.Generic;

namespace Nebula.Core.Utility
{
    public static class LinqEx
    {
        public static IEnumerable<T> Except<T>(this IEnumerable<T> collection, T except)
        {
            foreach(var i in collection)
            {
                if(Compare(i, except))
                {
                    continue;
                }

                yield return i;
            }
        }

        public static bool Compare<T>(T x, T y)
        {
            return EqualityComparer<T>.Default.Equals(x, y);
        }
    }
}
