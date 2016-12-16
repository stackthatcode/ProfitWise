using System;
using System.Collections.Generic;
using System.Linq;
using Push.Foundation.Utilities.Helpers;

namespace ProfitWise.Data.Model.Profit
{
    public static class ReportSeriesFactory
    {        
        public static ReportSeries GenerateSeries(
                string seriesName, ReportGrouping grouping, DateTime start, DateTime end, DataGranularity level)
        {
            var correctedStart = start.StartOfPeriod(level);
            var correctedEnd = end.EndOfPeriod(level);

            var current = correctedStart;
            var output = new ReportSeries();
            
            output.Name = seriesName;
            output.Data = new List<ReportSeriesElement>();

            while (current <= correctedEnd)
            {
                output.Data.Add(
                    new ReportSeriesElement()
                    {
                        Name = current.DateLabel(level),
                        Start = current.StartOfPeriod(level),
                        End = current.EndOfPeriod(level),
                    });

                current = current.AddTime(level);
            }

            return output;
        }


        public static string DateLabel(this DateTime input, DataGranularity level)
        {
            if (DataGranularity.Year == level)
            {
                return input.Year.ToString();
            }
            if (DataGranularity.Month == level)
            {
                return input.ToShortMonthName() + " " + input.Year.ToString();
            }
            if (DataGranularity.Week == level)
            {
                return "Week of " + input.ToString("MM-dd-yyyy");
            }
            return input.DayOfWeek.ToString() + " " + input.ToString("MM-dd-yyyy");
        }

        // Takes all Order Line Profits...
        public static void Populate(
                    this ReportSeries series,
                    IList<OrderLineProfit> orderLineProfits,
                    Func<OrderLineProfit, bool> orderLineFilter)
        {
            foreach (var element in series.Data)
            {
                element.Value =
                    orderLineProfits
                        .Where(x => x.OrderDate >= element.Start &&
                                    x.OrderDate <= element.End &&
                                    orderLineFilter(x))
                        .Sum(x => x.TotalCogs);
            }
        }
    }
}
