using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Nebula.Debugger.Debugger.Data
{
    public static class Extensions
    {
        public static bool TryFirstOrDefault<T>(this IEnumerable<T> source, Predicate<T> predicate, [NotNullWhen(true)] out T? value)
        {
            value = default;
            using (IEnumerator<T> iterator = source.GetEnumerator())
            {
                if (iterator.MoveNext())
                {
                    T t = iterator.Current!;

                    if (t != null && predicate(t))
                    {
                        value = t;
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
