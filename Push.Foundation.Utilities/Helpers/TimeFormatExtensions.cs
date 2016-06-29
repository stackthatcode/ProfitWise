using System;
using System.Globalization;

namespace Push.Foundation.Utilities.Helpers
{
    public static class TimeFormatExtensions
    {
        public static string ToFormattedString(this TimeSpan ts)
        {
            return $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds/10:00}";
        }

        public static string ToStringWithMillis(this DateTime input)
        {
            return input.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
        }
    }
}
