using System;
using System.Collections.Generic;
using System.Globalization;
using Push.Foundation.Utilities.Helpers;

namespace ProfitWise.Data.Model.Profit
{
    public static class ReportSeriesFactory
    {        
        public static ReportSeries GenerateSeries(
                string seriesName, DateTime start, DateTime end, DataGranularity granularity)
        {
            var correctedStart = start.StartOfPeriod(granularity);
            var correctedEnd = end.EndOfPeriod(granularity);

            var current = correctedStart;
            var output = new ReportSeries();

            output.name = seriesName;
            output.Granularity = granularity;
            output.data = new List<ReportSeriesElement>();
            
            while (current <= correctedEnd)
            {
                output.data.Add(
                    new ReportSeriesElement()
                    {
                        name = current.DateLabel(granularity),
                        DateIdentifier = current.CanonicalDateIdentifier(granularity),
                        y = 0,

                        StartDate = current.StartOfPeriod(granularity),
                        EndDate = current.EndOfPeriod(granularity),
                        Parent = output,
                    });

                current = current.AddTime(granularity);
            }

            return output;
        }
        public static int MonthToQuarter(this int month)
        {
            if (month >= 1 && month <= 3)
                return 1;
            if (month >= 4 && month <= 6)
                return 2;
            if (month >= 7 && month <= 9)
                return 3;
            if (month >= 10 && month <= 12)
                return 4;
            throw new ArgumentException();
        }

        public static string DateLabel(this DateTime input, DataGranularity level)
        {
            if (DataGranularity.Year == level)
            {
                return input.Year.ToString();
            }
            if (DataGranularity.Quarter == level)
            {
                return "Q" + input.Month.MonthToQuarter() +", " + input.Year;
            }
            if (DataGranularity.Month == level)
            {
                return input.ToShortMonthName() + " " + input.Year;
            }
            if (DataGranularity.Week == level)
            {
                return "Week of " + input.ToString("MM-dd-yyyy");
            }
            return input.DayOfWeek + " " + input.ToString("MM-dd-yyyy");
        }

        public static string CanonicalDateIdentifier(this DateTime input, DataGranularity level)
        {
            if (DataGranularity.Year == level)
            {
                return input.Year.ToString();
            }
            if (DataGranularity.Quarter == level)
            {
                return input.Year + ":Q" + input.Month.MonthToQuarter();
            }
            if (DataGranularity.Month == level)
            {
                return input.Year + ":M" + input.Month;
            }
            if (DataGranularity.Week == level)
            {
                return input.Year + ":W" + input.WeekOfYearIso8601();
            }
            // DataGranularity.Day
            return input.Year + ":" + input.Month + ":" + input.Day;
        }

        public static int WeekOfYearIso8601(this DateTime date)
        {
            var day = (int)CultureInfo.CurrentCulture.Calendar.GetDayOfWeek(date);
            return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date.AddDays(4 - (day == 0 ? 7 : day)), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
    }
}
