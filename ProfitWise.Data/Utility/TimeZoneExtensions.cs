using System;

namespace ProfitWise.Data.Utility
{
    public static class TimeZoneParsingExtensions
    {
        public static int ParseTimeZoneHours(this string timeZoneAsString)
        {
            return Int32.Parse(timeZoneAsString.Substring(4, 3));
        }
        public static int ParseTimeZoneMinutes(this string timeZoneAsString)
        {
            return Int32.Parse(timeZoneAsString.Substring(8, 2));
        }

    }
}
