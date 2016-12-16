using System;
using Push.Foundation.Utilities.Helpers;

namespace ProfitWise.Data.Model.Profit
{
    public enum DataGranularity
    {
        Year = 1,
        Month = 2,
        Week = 3,
        Day = 4,
    }

    public static class DataGranularityExtensions
    {
        public static DateTime AddTime(this DateTime input, DataGranularity level)
        {
            if (level == DataGranularity.Day)
            {
                return input.AddDays(1);
            }
            if (level == DataGranularity.Month)
            {
                return input.AddMonths(1);
            }
            if (level == DataGranularity.Week)
            {
                return input.AddDays(7);
            }

            // DataGranularity.Year
            return input.AddYears(1);
        }

        public static DateTime StartOfPeriod(this DateTime input, DataGranularity level)
        {
            if (level == DataGranularity.Day)
            {
                return input;
            }
            if (level == DataGranularity.Month)
            {
                return new DateTime(input.Year, input.Month, 1);
            }
            if (level == DataGranularity.Week)
            {
                return input.StartOfWeek(DayOfWeek.Sunday);
            }
            return new DateTime(input.Year, 1, 1);
        }

        public static DateTime EndOfPeriod(this DateTime input, DataGranularity level)
        {
            if (level == DataGranularity.Day)
            {
                return input;
            }
            if (level == DataGranularity.Month)
            {
                var nextMonth = input.AddMonths(1);
                return new DateTime(nextMonth.Year, nextMonth.Month, 1).AddDays(-1);
            }
            if (level == DataGranularity.Week)
            {
                return input.StartOfWeek(DayOfWeek.Sunday).AddDays(6);
            }
            return new DateTime(input.Year + 1, 1, 1).AddDays(-1);
        }

        public static DataGranularity SuggestedDataGranularity(this TimeSpan lengthOfReportingPeriod)
        {
            if (lengthOfReportingPeriod.Days > 730)
            {
                return DataGranularity.Year;
            }
            if (lengthOfReportingPeriod.Days > 90)
            {
                return DataGranularity.Month;
            }
            if (lengthOfReportingPeriod.Days > 7)
            {
                return DataGranularity.Week;
            }

            return DataGranularity.Day;
        }
    }

}
