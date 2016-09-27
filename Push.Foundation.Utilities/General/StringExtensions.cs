using System;
using System.Collections.Generic;

namespace Push.Foundation.Utilities.General
{
    public static class StringExtensions
    {
        public static string TruncateAfter(this string input, int length)
        {
            return input.Length <= length ? input : input.Substring(0, length);
        }

        public static string JoinByNewline(this IEnumerable<string> input)
        {
            return string.Join(Environment.NewLine, input);
        }
    }
}