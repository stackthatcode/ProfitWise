using System;
using System.Collections.Generic;
using System.Linq;

namespace ProfitWise.Data.Model.Profit
{
    public static class ReportSeriesFactory
    {
        public static IList<ReportSeries>
                BuildSeriesByProductTypes(
                    this IList<OrderLineProfit> orderLineProfits,
                    IList<string> productTypes,
                    DateTime startDate, DateTime endDate, DataGranularity granularity)
        {
            var output = new List<ReportSeries>();
            foreach (var productType in productTypes)
            {
                var series = GenerateSeries(startDate, endDate, granularity);
                series.PopulateWithTotals(orderLineProfits, x => x.SearchStub.ProductType == productType);
                output.Add(series);
            }
            return output;
        }

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
        private static void PopulateWithTotals(
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
