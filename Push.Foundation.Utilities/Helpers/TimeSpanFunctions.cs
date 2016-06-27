using System;

namespace Push.Utilities.Helpers
{
    public static class TimeSpanFunctions
    {
        public static string ToFormattedString(this TimeSpan ts)
        {
            return String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds/10);
        }
    }
}
