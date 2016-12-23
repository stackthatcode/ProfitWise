using System;
using System.Collections.Generic;
using Push.Foundation.Utilities.Helpers;

namespace ProfitWise.Data.Model.Profit
{
    public static class ReportSeriesFactory
    {        
        public static ReportSeries GenerateSeries(
                string groupKey, string groupingName, DateTime start, DateTime end, PeriodType periodType)
        {
            var series = new ReportSeries();
            series.GroupingKey = groupKey;
            series.GroupingName = groupingName;
            series.PeriodType = periodType;
            series.Elements = new List<ReportSeriesElement>();

            var correctedStart = start.StartOfPeriod(periodType);
            var correctedEnd = end.EndOfPeriod(periodType);
            var current = correctedStart;

            while (current <= correctedEnd)
            {
                var element = GenerateElement(periodType, current);
                element.Parent = series;
                series.Elements.Add(element);
                
                current = current.AddTime(periodType);
            }

            return series;
        }

        public static ReportSeriesElement GenerateElement(PeriodType periodType, DateTime periodStart)
        {
            if (periodType == PeriodType.Year)
            {
                return new ReportSeriesElement
                {
                    Year = periodStart.Year
                };
            }
            if (periodType == PeriodType.Quarter)
            {
                return new ReportSeriesElement
                {
                    Year = periodStart.Year,
                    Quarter = periodStart.Quarter()
                };
            }
            if (periodType == PeriodType.Month)
            {
                return new ReportSeriesElement
                {
                    Year = periodStart.Year,
                    Quarter = periodStart.Quarter(),
                    Month = periodStart.Month,
                };
            }
            if (periodType == PeriodType.Week)
            {
                return new ReportSeriesElement
                {
                    Year = periodStart.Year,
                    Quarter = periodStart.Quarter(),
                    Month = periodStart.Month,
                    Week = periodStart.WeekOfYearIso8601(),
                };
            }
            return new ReportSeriesElement
            {
                Year = periodStart.Year,
                Quarter = periodStart.Quarter(),
                Month = periodStart.Month,
                Week = periodStart.WeekOfYearIso8601(),
                Day = periodStart.Day,
            };
        }


        public static string DateLabel(this DateTime input, PeriodType level)
        {
            if (PeriodType.Year == level)
            {
                return input.Year.ToString();
            }
            if (PeriodType.Quarter == level)
            {
                return "Q" + input.Month.Quarter() +", " + input.Year;
            }
            if (PeriodType.Month == level)
            {
                return input.ToShortMonthName() + " " + input.Year;
            }
            if (PeriodType.Week == level)
            {
                return "Week of " + input.ToString("MM-dd-yyyy");
            }
            return input.DayOfWeek + " " + input.ToString("MM-dd-yyyy");
        }

        public static string CanonicalDateIdentifier(this DateTime input, PeriodType level)
        {
            if (PeriodType.Year == level)
            {
                return input.Year.ToString();
            }
            if (PeriodType.Quarter == level)
            {
                return input.Year + ":Q" + input.Month.Quarter();
            }
            if (PeriodType.Month == level)
            {
                return input.Year + ":M" + input.Month;
            }
            if (PeriodType.Week == level)
            {
                return input.Year + ":W" + input.WeekOfYearIso8601();
            }
            // DataGranularity.Day
            return input.Year + ":" + input.Month + ":" + input.Day;
        }

    }
}
