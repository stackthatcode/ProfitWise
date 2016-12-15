using System;
using System.Collections.Generic;
using System.Linq;
using Push.Foundation.Utilities.Helpers;

namespace ProfitWise.Data.Model
{
    public enum ReportColumnOrdering
    {
        ProfitDescending = 1,
        ProfitAscending = 2,
        NetSalesDescending = 3,
        NetSalesAscending = 4,
        CogsDescending = 5,
        CogsAscending = 6,
        QuantitySoldDescending = 7,
        QuantitySoldAscending = 8,
    };


    public static class ReportExtensions
    {
        public static ReportSeries BuildDrillDownSeries(
                this IList<PwReportOrderLineProfit> orderLineProfits,
                    Func<PwReportOrderLineProfit, bool> searchLambda)
        {
            throw new NotImplementedException();
        }

        public static PwReportSummaryOutput BuildSummaryReportOutput(
                    this IList<PwReportOrderLineProfit> orderLineProfits, int currencyId)
        { 
            var output = new PwReportSummaryOutput();

            output.CurrencyId = currencyId;

            output.ExecutiveSummary = BuildExecutiveSummary(orderLineProfits);

            output.ProductsByMostProfitable
                    = BuildKeyedSummary(
                        orderLineProfits,
                        x => x.SearchStub.PwMasterProductId,
                        x => x.SearchStub.ProductTitle);
            
            output.VendorsByMostProfitable
                    = BuildKeyedSummary(
                        orderLineProfits,
                        x => x.SearchStub.Vendor,
                        x => x.SearchStub.Vendor);
            
            output.VariantByMostProfitable
                    = BuildKeyedSummary(
                        orderLineProfits,
                        x => x.SearchStub.PwMasterVariantId,
                        x => x.SearchStub.Sku);
            
            output.ProductTypeByMostProfitable
                    = BuildKeyedSummary(
                        orderLineProfits,
                        x => x.SearchStub.ProductType,
                        x => x.SearchStub.ProductType);


            return output;
        }

        // Temporary holding place for helper functions
        public static PwReportExecutiveSummary 
                BuildExecutiveSummary(this IList<PwReportOrderLineProfit> profitLines)
        {
            return new PwReportExecutiveSummary()
            {
                NumberOfOrders = profitLines.Select(x => x.ShopifyOrderId).Distinct().Count(),
                CostOfGoodsSold = profitLines.Sum(x => x.TotalCogs),
                GrossRevenue = profitLines.Sum(x => x.GrossRevenue),                
                Profit = profitLines.Sum(x => x.Profit)
            };
        }

        public static IList<PwReportKeyedSummaryTotal<T1>> 
                BuildKeyedSummary<T1>(
                    this IList<PwReportOrderLineProfit> profitLines,
                    Func<PwReportOrderLineProfit, T1> keySelector,
                    Func<PwReportOrderLineProfit, string> titleSelector,
                    ReportColumnOrdering ordering = ReportColumnOrdering.ProfitDescending)
        {
            var output =
                profitLines
                    .GroupBy(keySelector)
                    .Select(xg => new PwReportKeyedSummaryTotal<T1>()
                    {
                        TotalRevenue = xg.Sum(line => line.GrossRevenue),
                        TotalCogs = xg.Sum(line => line.TotalCogs),
                        TotalNumberSold = xg.Sum(line => line.NetQuantity),
                        GroupingKey = xg.Key,
                        GroupingName = titleSelector(xg.First())
                    })
                    .ToList();

            return BuildOrdereredSummary(output);
        }

        public static IList<PwReportKeyedSummaryTotal<T1>> 
                    BuildOrdereredSummary<T1>(
                        this IList<PwReportKeyedSummaryTotal<T1>> input, 
                        int numberOfElements = 10,
                        ReportColumnOrdering ordering = ReportColumnOrdering.ProfitDescending)
        {
            // For now we order by Total Profit...
            var topResults =
                input
                    .OrderByDescending(x => x.TotalProfit)
                    .Take(numberOfElements - 1)
                    .ToList();

            var remainingResults =
                input
                    .OrderByDescending(x => x.TotalProfit)
                    .TakeAfter(numberOfElements - 1)
                    .ToList();

            topResults.Add(new PwReportKeyedSummaryTotal<T1>()
            {
                GroupingName = "All others",
                TotalRevenue = remainingResults.Sum(x => x.TotalRevenue),
                TotalCogs = remainingResults.Sum(x => x.TotalCogs),
            });

            return topResults;
        }

        public static IEnumerable<PwReportKeyedSummaryTotal<T>> OrderBy<T>(
                    this IList<PwReportKeyedSummaryTotal<T>> input,
                    ReportColumnOrdering ordering)
        {
            if (ordering == ReportColumnOrdering.ProfitDescending)
            {
                return input.OrderByDescending(x => x.TotalProfit);
            }
            if (ordering == ReportColumnOrdering.ProfitAscending)
            {
                return input.OrderBy(x => x.TotalProfit);
            }

            if (ordering == ReportColumnOrdering.CogsDescending)
            {
                return input.OrderByDescending(x => x.TotalCogs);
            }
            if (ordering == ReportColumnOrdering.CogsAscending)
            {
                return input.OrderBy(x => x.TotalCogs);
            }

            if (ordering == ReportColumnOrdering.NetSalesDescending)
            {
                return input.OrderByDescending(x => x.TotalRevenue);
            }
            if (ordering == ReportColumnOrdering.NetSalesAscending)
            {
                return input.OrderBy(x => x.TotalRevenue);
            }

            if (ordering == ReportColumnOrdering.QuantitySoldDescending)
            {
                return input.OrderByDescending(x => x.TotalRevenue);
            }
            if (ordering == ReportColumnOrdering.QuantitySoldDescending)
            {
                return input.OrderBy(x => x.TotalRevenue);
            }

            throw new ArgumentException();
        }
    }
}
