using System;
using System.Collections.Generic;
using System.Linq;
using Push.Foundation.Utilities.Helpers;

namespace ProfitWise.Data.Model
{
    public static class ReportGroupedDataExtensions
    {
        public static 
                PwReportSummary BuildReportSummary(
                    this IList<PwReportOrderLineProfit> orderLineProfits, int currencyId)
        {
            var output = new PwReportSummary
            {
                CurrencyId = currencyId,
                ExecutiveSummary = orderLineProfits.BuildExecutiveSummary(),

                ProductsByMostProfitable = orderLineProfits.BuildSummaryByGrouping(ReportGrouping.Product),
                VendorsByMostProfitable = orderLineProfits.BuildSummaryByGrouping(ReportGrouping.Vendor),
                ProductTypeByMostProfitable = orderLineProfits.BuildSummaryByGrouping(ReportGrouping.ProductType),
                VariantByMostProfitable = orderLineProfits.BuildSummaryByGrouping(ReportGrouping.Variant),
            };
            
            return output;
        }

        // Sums everything without an grouping
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

        // Overload for grouping by Search Stub properties that are long numbers
        public static IList<ReportGroupedTotal> 
                BuildSummaryByGrouping(
                    this IList<PwReportOrderLineProfit> profitLines,
                    ReportGrouping reportGrouping,
                    ReportColumnOrdering ordering = ReportColumnOrdering.ProfitDescending)
        {
            var output = new List<ReportGroupedTotal>();

            if (reportGrouping == ReportGrouping.Product)
            {
                output =
                    profitLines
                        .GroupBy(x => x.SearchStub.PwMasterProductId)
                        .Select(xg => new ReportGroupedTotal()
                        {
                            TotalRevenue = xg.Sum(line => line.GrossRevenue),
                            TotalCogs = xg.Sum(line => line.TotalCogs),
                            TotalNumberSold = xg.Sum(line => line.NetQuantity),
                            GroupingKeyNumeric = xg.Key,
                            GroupingName = xg.First().SearchStub.ProductTitle
                        })
                        .ToList();
            }

            if (reportGrouping == ReportGrouping.Vendor)
            {
                output =
                    profitLines
                        .GroupBy(x => x.SearchStub.Vendor)
                        .Select(xg => new ReportGroupedTotal()
                        {
                            TotalRevenue = xg.Sum(line => line.GrossRevenue),
                            TotalCogs = xg.Sum(line => line.TotalCogs),
                            TotalNumberSold = xg.Sum(line => line.NetQuantity),
                            GroupingKeyString = xg.Key, // ... which will be Vendor
                            GroupingName = xg.First().SearchStub.Vendor
                        })
                        .ToList();
            }

            if (reportGrouping == ReportGrouping.ProductType)
            {
                output =
                    profitLines
                        .GroupBy(x => x.SearchStub.ProductType)
                        .Select(xg => new ReportGroupedTotal()
                        {
                            TotalRevenue = xg.Sum(line => line.GrossRevenue),
                            TotalCogs = xg.Sum(line => line.TotalCogs),
                            TotalNumberSold = xg.Sum(line => line.NetQuantity),
                            GroupingKeyString = xg.Key, // ... which will be Vendor
                            GroupingName = xg.First().SearchStub.ProductType,
                        })
                        .ToList();
            }

            if (reportGrouping == ReportGrouping.Variant)
            {
                output =
                    profitLines
                        .GroupBy(x => x.SearchStub.PwMasterVariantId)
                        .Select(xg => new ReportGroupedTotal()
                        {
                            TotalRevenue = xg.Sum(line => line.GrossRevenue),
                            TotalCogs = xg.Sum(line => line.TotalCogs),
                            TotalNumberSold = xg.Sum(line => line.NetQuantity),
                            GroupingKeyNumeric = xg.Key, // ... which will be Vendor
                            GroupingName = xg.First().SearchStub.Vendor
                        })
                        .ToList();
            }

            return output.BuildOrdereredWithLeftovers();
        }
        
        public static IList<ReportGroupedTotal> 
                    BuildOrdereredWithLeftovers(
                        this IList<ReportGroupedTotal> input, 
                        ReportColumnOrdering ordering = ReportColumnOrdering.ProfitDescending,
                        int numberOfElements = 10,
                        string leftoversDescription = "All others")
        {
            var orderedInput = input.OrderBy(ordering).ToList();

            var topResults =
                orderedInput
                    .Take(numberOfElements - 1)
                    .ToList();

            var remainingResults =
                orderedInput
                    .TakeAfter(numberOfElements - 1)
                    .ToList();

            if (topResults.Count() <= numberOfElements - 1)
            topResults.Add(
                new ReportGroupedTotal()
                {
                    GroupingName = leftoversDescription,
                    TotalRevenue = remainingResults.Sum(x => x.TotalRevenue),
                    TotalCogs = remainingResults.Sum(x => x.TotalCogs),
                });

            return topResults;
        }

        public static 
                IEnumerable<ReportGroupedTotal> OrderBy(
                    this IList<ReportGroupedTotal> input,
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
