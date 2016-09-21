using System.Collections.Generic;

namespace Push.Foundation.Utilities.Helpers
{
    public static class StringExtensions
    {
        public static string StringJoin(this IEnumerable<string> input, string separator)
        {
            return string.Join(separator, input);
        }
    }
}
