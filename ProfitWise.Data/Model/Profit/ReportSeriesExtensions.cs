using System;
using System.Collections.Generic;
using System.Linq;

namespace ProfitWise.Data.Model.Profit
{
    public static class ReportSeriesFactory
    {        
        public static ReportSeries GenerateSeries(DateTime start, DateTime end, DataGranularity level)
        {
            var correctedStart = start.StartOfPeriod(level);
            var correctedEnd = end.EndOfPeriod(level);

            var current = correctedStart;
            var output = new ReportSeries();

            output.SeriesGranularity = level;
            output.Data = new List<ReportSeriesElement>();

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

        // Takes all Order Line Profits...
        public static void PopulateWithTotals(
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
