using System;
using System.Collections.Generic;
using Push.Foundation.Utilities.Helpers;

namespace ProfitWise.Data.Model
{
    public enum ReportDrillDownLevel
    {
        Year = 1,
        Month = 2,
        Week = 3,
        Day = 4,
    }

    public class ReportSeries
    {
        public ReportDrillDownLevel SeriesGranularity { get; set; }
        public string GroupingKey { get; set; }
        public string Identifier { get; set; }  // 
        public IList<ReportSeriesElement> Data { get; set; }
    }

    public class ReportSeriesElement
    {
        public ReportSeries Parent { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Name { get; set; }    // Which will always be the standarized date
        public decimal Value { get; set; }


        public string DrilldownIdentifier { get; set; } // ...?

        public override string ToString()
        {
            return $"Start: {Start}, End: {End}";
        }
    }

    public static class ReportSeriesExtensions
    {
        public static DateTime AddTime(this DateTime input, ReportDrillDownLevel level)
        {
            if (level == ReportDrillDownLevel.Day)
            {
                return input.AddDays(1);
            }
            if (level == ReportDrillDownLevel.Month)
            {
                return input.AddMonths(1);
            }
            if (level == ReportDrillDownLevel.Week)
            {
                return input.AddDays(7);
            }
            return input.AddYears(1);
        }

        public static DateTime StartOfPeriod(this DateTime input, ReportDrillDownLevel level)
        {
            if (level == ReportDrillDownLevel.Day)
            {
                return input;
            }
            if (level == ReportDrillDownLevel.Month)
            {
                return new DateTime(input.Year, input.Month, 1);
            }
            if (level == ReportDrillDownLevel.Week)
            {
                return input.StartOfWeek(DayOfWeek.Sunday);
            }
            return new DateTime(input.Year, 1, 1);
        }

        public static DateTime EndOfPeriod(this DateTime input, ReportDrillDownLevel level)
        {
            if (level == ReportDrillDownLevel.Day)
            {
                return input;
            }
            if (level == ReportDrillDownLevel.Month)
            {
                var nextMonth = input.AddMonths(1);
                return new DateTime(nextMonth.Year, nextMonth.Month, 1).AddDays(-1);
            }
            if (level == ReportDrillDownLevel.Week)
            {
                return input.StartOfWeek(DayOfWeek.Sunday).AddDays(6);
            }
            return new DateTime(input.Year + 1, 1, 1).AddDays(-1);
        }

        public static ReportDrillDownLevel DefaultTopDrillDownLevel(this TimeSpan lengthOfReportingPeriod)
        {
            if (lengthOfReportingPeriod.Days > 730)
            {
                return ReportDrillDownLevel.Year;
            }
            if (lengthOfReportingPeriod.Days > 90)
            {
                return ReportDrillDownLevel.Month;
            }
            return ReportDrillDownLevel.Week;


            // NOTE - Jeremy, showing 30 days of data seemed a bit infeasible. We can discuss.
        }
    }


    public class ReportSeriesFactory
    {
        public static ReportSeries GenerateSeries(
            string groupingKey, DateTime start, DateTime end)
        {
            var lengthOfReportingPeriod = end - start;
            var level = lengthOfReportingPeriod.DefaultTopDrillDownLevel();
            return GenerateSeries(groupingKey, start, end, level);
        }


        public static ReportSeries GenerateSeries(
                 string groupingKey, DateTime start, DateTime end, ReportDrillDownLevel level)
        {
            var correctedStart = start.StartOfPeriod(level);
            var correctedEnd = end.EndOfPeriod(level);

            var current = correctedStart;
            var output = new ReportSeries();
            output.SeriesGranularity = level;
            output.Data = new List<ReportSeriesElement>();
            output.GroupingKey = groupingKey;

            while (current <= correctedEnd)
            {
                output.Data.Add(new ReportSeriesElement()
                {
                    Start = current.StartOfPeriod(level),
                    End = current.EndOfPeriod(level),
                    

                });

                current = current.AddTime(level);
            }

            return output;
        }
    }
}

