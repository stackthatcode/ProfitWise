using System.Collections.Generic;
using System.Linq;

namespace Push.Foundation.Utilities.Helpers
{
    public static class StringExtensions
    {
        public static string StringJoin(this IEnumerable<string> input, string separator)
        {
            return string.Join(separator, input);
        }

        public static List<string> SplitBy(this string input, char separator)
        {
            return input.Split(separator)
                        .Select(x => x.Trim())
                        .Where(x => x != "")
                        .ToList();
        }
    }
}
