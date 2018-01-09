using System;
using System.Collections.Generic;
using Push.Foundation.Utilities.Helpers;

namespace ProfitWise.Data.Model.Profit
{
    public static class ReportSeriesFactory
    {
        public static List<ReportSeries> GenerateSeriesMultiple(
                DateTime start, DateTime end,
                List<GroupingKeyAndName> groupingKeysAndNames,
                PeriodType topPeriodType,
                PeriodType maximumDepth)
        {
            var seriesDataset = new List<ReportSeries>();
            foreach (var groupingPair in groupingKeysAndNames)
            {
                var series = 
                    GenerateSeriesRecursive(
                        groupingPair.GroupingKey, groupingPair.GroupingName, start, end, 
                        topPeriodType, maximumDepth);

                seriesDataset.Add(series);
            }
            return seriesDataset;
        }

        public static ReportSeries GenerateSeriesRecursive(
                    string groupingKey, string groupingName, DateTime start, DateTime end,
                    PeriodType periodType, PeriodType maximumDepth)
        {
            var series = GenerateSeries(groupingKey, groupingName, start, end, periodType);

            if (periodType < maximumDepth && periodType != PeriodType.Day)
            {
                foreach (var element in series.Elements)
                {
                    var nextDepth = periodType.NextDrilldownLevel();
                    var childSeries = GenerateSeriesRecursive(
                        groupingKey, groupingName, element.Start, element.End, nextDepth, maximumDepth);

                    if (childSeries != null)
                    {
                        element.ChildSeries = childSeries;
                        childSeries.Parent = element;
                    }
                }
            }
            return series;
        }

        public static ReportSeries GenerateSeries(
                string groupKey, string groupingName, DateTime start, DateTime end, PeriodType periodType)
        {
            var series = new ReportSeries();
            series.GroupingKey = groupKey;
            series.GroupingName = groupingName;
            series.Elements = new List<ReportSeriesElement>();

            var correctedStart = start.StartOfPeriod(periodType);
            var correctedEnd = end.EndOfPeriod(periodType);

            var current = correctedStart;
            while (current <= correctedEnd)
            {
                var element = GenerateElement(periodType, current, current.EndOfPeriod(periodType));
                element.Parent = series;
                series.Elements.Add(element);
                
                current = current.AddTime(periodType);
            }

            return series;
        }

        public static ReportSeriesElement GenerateElement(
                    PeriodType periodType, DateTime periodStart, DateTime periodEnd)
        {
            var output =
                new ReportSeriesElement
                {
                    PeriodType = periodType,
                    Start = periodStart,
                    End = periodEnd
                };

            if (periodType >= PeriodType.Year)
            {
                output.Year = periodStart.Year;
            }
            if (periodType >= PeriodType.Quarter)
            {
                output.Quarter = periodStart.Quarter();
            }
            if (periodType >= PeriodType.Month)
            {
                output.Month = periodStart.Month;
            }
            if (periodType >= PeriodType.Week)
            {
                var firstDayOfWeek = periodStart.StartOfWeek(DayOfWeek.Sunday);
                var weekNumber = firstDayOfWeek.WeekOfYearIso8601();
                var canonizedWeek = periodStart.Year * 100 + weekNumber;
                output.Week = canonizedWeek;
            }
            if (periodType >= PeriodType.Day)
            {
                output.Day = periodStart.Day;
            }
            return output;
        }

    }
}
