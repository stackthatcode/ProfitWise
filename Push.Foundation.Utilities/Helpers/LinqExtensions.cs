using System;
using System.Collections.Generic;
using System.Linq;

namespace Push.Foundation.Utilities.Helpers
{
    public static class LinqExtensions
    {

        // Ex: collection.TakeLast(5);
        public static IEnumerable<T> TakeAfter<T>(this IEnumerable<T> source, int N)
        {
            var remainingCount = source.Count() - N;
            return source.TakeLast(remainingCount);
        }

        // Ex: collection.TakeLast(5);
        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int N)
        {
            return source.Skip(Math.Max(0, source.Count() - N));
        }
    }
}
