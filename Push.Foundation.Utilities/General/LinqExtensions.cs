using System;
using System.Collections.Generic;
using System.Linq;

namespace Push.Foundation.Utilities.General
{
    public static class LinqExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> input, Action<T> action)
        {
            foreach (var member in input)
            {
                action(member);
            }
        }

        public static string ToCommaSeparatedList(this IEnumerable<long> input)
        {
            return string.Join(",", input.Select(n => n.ToString()).ToArray());
        }
    }
}