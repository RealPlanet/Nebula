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
            using (var iterator = source.GetEnumerator())
            {
                if (iterator.MoveNext())
                {
                    var t = iterator.Current!;

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
