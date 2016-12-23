using System;
using Push.Foundation.Utilities.Helpers;

namespace ProfitWise.Data.Model.Profit
{
    public enum PeriodType
    {
        Year = 1,
        Quarter = 2,
        Month = 3,
        Week = 4,
        Day = 5,
    }

    public static class DataGranularityExtensions
    {
        public static DateTime AddTime(this DateTime input, PeriodType level)
        {
            if (level == PeriodType.Quarter)
            {
                return input.AddMonths(3);
            }
            if (level == PeriodType.Month)
            {
                return input.AddMonths(1);
            }
            if (level == PeriodType.Week)
            {
                return input.AddDays(7);
            }
            if (level == PeriodType.Day)
            {
                return input.AddDays(1);
            }

            // DataGranularity.Year
            return input.AddYears(1);
        }

        public static DateTime StartOfPeriod(this DateTime input, PeriodType level)
        {
            if (level == PeriodType.Quarter)
            {
                int month;
                if (input.Month < 4)
                {
                    month = 1;
                }
                else if (input.Month < 7)
                {
                    month = 4;
                }
                else if (input.Month < 10)
                {
                    month = 7;
                }
                else month = 10;

                return new DateTime(input.Year, month, 1);
            }
            if (level == PeriodType.Month)
            {
                return new DateTime(input.Year, input.Month, 1);
            }
            if (level == PeriodType.Day)
            {
                return input;
            }
            if (level == PeriodType.Week)
            {
                return input.StartOfWeek(DayOfWeek.Sunday);
            }
            return new DateTime(input.Year, 1, 1);
        }

        public static DateTime EndOfPeriod(this DateTime input, PeriodType level)
        {
            return input.StartOfPeriod(level).AddTime(level).AddDays(-1);
        }

        public static PeriodType ToDefaultGranularity(this TimeSpan lengthOfReportingPeriod)
        {
            if (lengthOfReportingPeriod.Days > 730)
            {
                return PeriodType.Year;
            }
            if (lengthOfReportingPeriod.Days > 365)
            {
                return PeriodType.Quarter;
            }
            if (lengthOfReportingPeriod.Days > 90)
            {
                return PeriodType.Month;
            }
            if (lengthOfReportingPeriod.Days > 7)
            {
                return PeriodType.Week;
            }

            return PeriodType.Day;
        }

        public static PeriodType NextDrilldownLevel(this PeriodType input)
        {
            return input == PeriodType.Day ? PeriodType.Day : (PeriodType) ((int) input + 1);
        }
    }

}
