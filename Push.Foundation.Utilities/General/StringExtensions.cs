using System;
using System.Collections.Generic;
using Castle.Core.Internal;

namespace Push.Foundation.Utilities.General
{
    public static class StringExtensions
    {
        public static bool IsEmptyOrWhitespaces(this string input)
        {
            return (input ?? "").Trim().IsNullOrEmpty();
        }

        public static string TruncateAfter(this string input, int length)
        {
            return input.Length <= length ? input : input.Substring(0, length);
        }

        public static string JoinByNewline(this IEnumerable<string> input)
        {
            return string.Join(Environment.NewLine, input);
        }

        public static string ToCommaDelimited(this IEnumerable<string> input)
        {
            return input.ToDelimited(",");
        }

        public static string ToDelimited(this IEnumerable<string> input, string delimiter)
        {
            return string.Join(delimiter, input);
        }

        public static bool CaselessEquals(this string input, string other)
        {
            return string.Equals(input, other, StringComparison.OrdinalIgnoreCase);
        }
    }
}