﻿using System;
using System.Collections.Generic;
using System.Linq;
using ProfitWise.Data.Model.Reports;
using Push.Foundation.Utilities.Helpers;

namespace ProfitWise.Data.Model.Profit
{
    public static class GroupedDataExtensions
    {
        public static Summary BuildCompleteGroupedSummary(
                    this IList<OrderLineProfit> orderLineProfits, int currencyId)
        {
            var output = new Summary
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
        public static ExecutiveSummary BuildExecutiveSummary(this IList<OrderLineProfit> profitLines)
        {
            return new ExecutiveSummary()
            {
                TotalNumberSold = profitLines.Select(x => x.ShopifyOrderId).Distinct().Count(),
                TotalCogs = profitLines.Sum(x => x.TotalCogs),
                TotalRevenue = profitLines.Sum(x => x.GrossRevenue),                
            };
        }
        
        // Overload for grouping by Search Stub properties that are long numbers
        public static IList<GroupedTotal> 
                BuildSummaryByGrouping(
                    this IList<OrderLineProfit> profitLines,
                    ReportGrouping reportGrouping,
                    ColumnOrdering ordering = ColumnOrdering.ProfitDescending)
        {
            var output = new List<GroupedTotal>();

            if (reportGrouping == ReportGrouping.Product)
            {
                output =
                    profitLines
                        .GroupBy(x => x.SearchStub.PwMasterProductId)
                        .Select(xg => new GroupedTotal()
                        {
                            ReportGrouping = ReportGrouping.Product,
                            GroupingKey = xg.First().SearchStub.PwMasterProductId.ToString(),
                            GroupingName = xg.First().SearchStub.ProductTitle,

                            TotalRevenue = xg.Sum(line => line.GrossRevenue),
                            TotalCogs = xg.Sum(line => line.TotalCogs),
                            TotalNumberSold = xg.Sum(line => line.NetQuantity),
                        })
                        .ToList();
            }
            if (reportGrouping == ReportGrouping.Vendor)
            {
                output =
                    profitLines
                        .GroupBy(x => x.SearchStub.Vendor)
                        .Select(xg => new GroupedTotal()
                        {
                            ReportGrouping = ReportGrouping.Vendor,
                            GroupingKey = xg.First().SearchStub.Vendor,
                            GroupingName = xg.First().SearchStub.Vendor,

                            TotalRevenue = xg.Sum(line => line.GrossRevenue),
                            TotalCogs = xg.Sum(line => line.TotalCogs),
                            TotalNumberSold = xg.Sum(line => line.NetQuantity),
                        })
                        .ToList();
            }
            if (reportGrouping == ReportGrouping.ProductType)
            {
                output =
                    profitLines
                        .GroupBy(x => x.SearchStub.ProductType)
                        .Select(xg => new GroupedTotal()
                        {
                            ReportGrouping = ReportGrouping.ProductType,
                            GroupingKey = xg.First().SearchStub.ProductType,
                            GroupingName = xg.First().SearchStub.ProductType,

                            TotalRevenue = xg.Sum(line => line.GrossRevenue),
                            TotalCogs = xg.Sum(line => line.TotalCogs),
                            TotalNumberSold = xg.Sum(line => line.NetQuantity),
                        })
                        .ToList();
            }
            if (reportGrouping == ReportGrouping.Variant)
            {
                output =
                    profitLines
                        .GroupBy(x => x.SearchStub.PwMasterVariantId)
                        .Select(xg => new GroupedTotal()
                        {
                            ReportGrouping = ReportGrouping.Variant,
                            GroupingKey = xg.First().SearchStub.PwMasterVariantId.ToString(),
                            GroupingName = xg.First().SearchStub.VariantTitle,

                            TotalRevenue = xg.Sum(line => line.GrossRevenue),
                            TotalCogs = xg.Sum(line => line.TotalCogs),
                            TotalNumberSold = xg.Sum(line => line.NetQuantity),

                        })
                        .ToList();
            }

            return output.TruncateAndAppendRemainingTotal();
        }
        
        public static IList<GroupedTotal> 
                TruncateAndAppendRemainingTotal(
                    this IList<GroupedTotal> input, 
                    ColumnOrdering ordering = ColumnOrdering.ProfitDescending,
                    int maximumRowCount = 10,
                    string leftoversDescription = "All others")
        {
            var orderedInput = input.OrderBy(ordering).ToList();

            var numberOfElements = 
                orderedInput.Count() == maximumRowCount 
                    ? maximumRowCount 
                    : maximumRowCount - 1;

            var topResults =
                orderedInput
                    .Take(numberOfElements)
                    .ToList();

            var remainingResults =
                orderedInput
                    .TakeAfter(numberOfElements)
                    .ToList();

            if (remainingResults.Any())
                topResults.Add(
                    new GroupedTotal()
                    {
                        GroupingName = leftoversDescription,
                        TotalRevenue = remainingResults.Sum(x => x.TotalRevenue),
                        TotalCogs = remainingResults.Sum(x => x.TotalCogs),
                    });

            return topResults;
        }

        public static IEnumerable<GroupedTotal> 
                OrderBy(this IList<GroupedTotal> input, ColumnOrdering ordering)
        {
            if (ordering == ColumnOrdering.ProfitDescending)
            {
                return input.OrderByDescending(x => x.TotalProfit);
            }
            if (ordering == ColumnOrdering.ProfitAscending)
            {
                return input.OrderBy(x => x.TotalProfit);
            }

            if (ordering == ColumnOrdering.CogsDescending)
            {
                return input.OrderByDescending(x => x.TotalCogs);
            }
            if (ordering == ColumnOrdering.CogsAscending)
            {
                return input.OrderBy(x => x.TotalCogs);
            }

            if (ordering == ColumnOrdering.NetSalesDescending)
            {
                return input.OrderByDescending(x => x.TotalRevenue);
            }
            if (ordering == ColumnOrdering.NetSalesAscending)
            {
                return input.OrderBy(x => x.TotalRevenue);
            }

            if (ordering == ColumnOrdering.QuantitySoldDescending)
            {
                return input.OrderByDescending(x => x.TotalRevenue);
            }
            if (ordering == ColumnOrdering.QuantitySoldDescending)
            {
                return input.OrderBy(x => x.TotalRevenue);
            }

            throw new ArgumentException();
        }
    }
}