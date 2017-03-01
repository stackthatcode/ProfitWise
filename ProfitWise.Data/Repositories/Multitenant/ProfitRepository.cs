﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using ProfitWise.Data.Database;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.Profit;
using ProfitWise.Data.Model.Reports;
using ProfitWise.Data.Model.Shop;

namespace ProfitWise.Data.Repositories.Multitenant
{
    public class ProfitRepository
    {
        private readonly MultitenantFactory _factory;
        public PwShop PwShop { get; set; }
        public long PwShopId => PwShop.PwShopId;
        private readonly ConnectionWrapper _connectionWrapper;


        public ProfitRepository(ConnectionWrapper connectionWrapper, MultitenantFactory factory)
        {
            _connectionWrapper = connectionWrapper;
            _factory = factory;
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.InitiateTransaction();
        }


        // Profit Query output
        public void PopulateQueryStub(long reportId)
        {
            var filterRepository = _factory.MakeReportFilterRepository(this.PwShop);

            var deleteQuery =
                @"DELETE FROM profitquerystub(@PwShopId) WHERE PwReportId = @PwReportId";
            _connectionWrapper.Execute(
                deleteQuery, new { PwShopId, PwReportId = reportId });

            var createQuery =
                @"INSERT INTO profitquerystub(@PwShopId)
                SELECT @PwReportId, @PwShopId, PwMasterVariantId, PwMasterProductId, 
                        Vendor, ProductType, ProductTitle, Sku, VariantTitle
                FROM mtv_masterproductandvariantsearch(@PwShopId) 
                WHERE PwShopId = @PwShopId " +
                filterRepository.ReportFilterClauseGenerator(reportId) +
                @" GROUP BY PwMasterVariantId,  PwMasterProductId, 
                    Vendor, ProductType, ProductTitle, Sku, VariantTitle; ";
            _connectionWrapper.Execute(
                createQuery, new { PwShopId, PwReportId = reportId });
        }

        public List<PwReportSearchStub> RetrieveSearchStubs(long reportId)
        {
            var query =
                @"SELECT t2.*
                FROM profitquerystub(@PwShopId) t1 
	                INNER JOIN mtv_masterproductandvariantsearch(@PwShopId) t2
		                ON t1.PwMasterVariantId = t2.PwMasterVariantId
                WHERE t1.PwReportId = @reportId";

            var results =
                _connectionWrapper.Query<PwReportSearchStub>(query, new { PwShopId, reportId }).ToList();
            return results;
        }


        // Queries for generating Totals  
        public GroupedTotal RetreiveTotalsForAll(TotalQueryContext queryContext)
        {            
            var totalsQuery = @"SELECT " + QueryGutsForTotals();
            var totals = _connectionWrapper.Query<GroupedTotal>(totalsQuery, queryContext).First();

            var numberOfOrdersQuery =
                @"SELECT COUNT(DISTINCT(t3.ShopifyOrderId)) 
                FROM profitquerystub(@PwShopId) t1
	                INNER JOIN variant(@PwShopId) t2
		                ON t1.PwMasterVariantId = t2.PwMasterVariantId 
	                INNER JOIN profitreportentry(@PwShopId) t3
		                ON t2.PwProductId = t3.PwProductId 
			                AND t2.PwVariantId = t3.PwVariantId
			                AND t3.EntryDate >= @StartDate
			                AND t3.EntryDate <= @EndDate 
                            AND t3.EntryType = 1 
                WHERE t1.PwReportId = @PwReportId;";
            
            var orderCount = _connectionWrapper.Query<int>(
                numberOfOrdersQuery, queryContext).First();

            totals.TotalOrders = orderCount;
            return totals;
        }
        
        public List<GroupedTotal> RetrieveTotalsByContext(TotalQueryContext queryContext)
        {
            if (queryContext.Grouping == ReportGrouping.Product)
                return RetreiveTotalsByProduct(queryContext);
            if (queryContext.Grouping == ReportGrouping.Variant)
                return RetreiveTotalsByVariant(queryContext);
            if (queryContext.Grouping == ReportGrouping.ProductType)
                return RetreiveTotalsByProductType(queryContext);
            if (queryContext.Grouping == ReportGrouping.Vendor)
                return RetreiveTotalsByVendor(queryContext);

            throw new ArgumentException("RetrieveTotals does not support that ReportGrouping");
        }

        public int RetreiveTotalCounts(TotalQueryContext queryContext)
        {
            var queryGuts =
                @"FROM profitquerystub(@PwShopId) t1
		            INNER JOIN variant(@PwShopId) t2
		                ON t1.PwMasterVariantId = t2.PwMasterVariantId 
	                INNER JOIN profitreportentry(@PwShopId) t3
		                ON t2.PwProductId = t3.PwProductId 
                            AND t2.PwVariantId = t3.PwVariantId  
			                AND t3.EntryDate >= @StartDate 
                            AND t3.EntryDate <= @EndDate             
                WHERE t1.PwReportId = @PwReportId";

            var query = "";
            if (queryContext.Grouping == ReportGrouping.Product)
                query = "SELECT COUNT(DISTINCT(t1.PwMasterProductId)) " + queryGuts;
            if (queryContext.Grouping == ReportGrouping.Variant)
                query = "SELECT COUNT(DISTINCT(t1.PwMasterVariantId)) " + queryGuts;
            if (queryContext.Grouping == ReportGrouping.ProductType)
                query = "SELECT COUNT(DISTINCT(t1.ProductType)) " + queryGuts;
            if (queryContext.Grouping == ReportGrouping.Vendor)
                query = "SELECT COUNT(DISTINCT(t1.Vendor)) " + queryGuts;

            return _connectionWrapper.Query<int>(query, queryContext).First();
        }

        public List<GroupedTotal> RetreiveTotalsByProduct(TotalQueryContext queryContext)
        {
            var query =
                @"SELECT t1.PwMasterProductId AS GroupingKey, t1.ProductTitle AS GroupingName, " +
                QueryGutsForTotals() +
                @"GROUP BY t1.PwMasterProductId, t1.ProductTitle " +
                OrderingAndPagingForTotals(queryContext);

            return _connectionWrapper
                .Query<GroupedTotal>(query, queryContext).ToList()
                .AssignGrouping(ReportGrouping.Product);
        }

        public List<GroupedTotal> RetreiveTotalsByVariant(TotalQueryContext queryContext)
        {
            var query =
                @"SELECT t1.PwMasterVariantId AS GroupingKey, CONCAT(t1.Sku, ' - ', t1.ProductTitle, ' - ', t1.VariantTitle) AS GroupingName, " +
                QueryGutsForTotals() +
                @"GROUP BY t1.PwMasterVariantId, CONCAT(t1.Sku, ' - ', t1.ProductTitle, ' - ', t1.VariantTitle) " +
                OrderingAndPagingForTotals(queryContext);

            return _connectionWrapper
                .Query<GroupedTotal>(query, queryContext).ToList()
                .AssignGrouping(ReportGrouping.Variant);
        }

        public List<GroupedTotal> RetreiveTotalsByProductType(TotalQueryContext queryContext)
        {
            var query =
                @"SELECT t1.ProductType AS GroupingKey, t1.ProductType AS GroupingName, " +
                QueryGutsForTotals() +
                @"GROUP BY t1.ProductType " +
                OrderingAndPagingForTotals(queryContext);

            return _connectionWrapper
                .Query<GroupedTotal>(query, queryContext).ToList()
                .AssignGrouping(ReportGrouping.ProductType);
        }

        public List<GroupedTotal> RetreiveTotalsByVendor(TotalQueryContext queryContext)
        {
            var query =
                @"SELECT t1.Vendor AS GroupingKey, t1.Vendor AS GroupingName, " +
                QueryGutsForTotals() +
                @"GROUP BY t1.Vendor " +
                OrderingAndPagingForTotals(queryContext);

            return _connectionWrapper
                .Query<GroupedTotal>(query, queryContext).ToList()
                .AssignGrouping(ReportGrouping.Vendor);
        }

        private string QueryGutsForTotals()
        {
            return
                @"SUM(t3.NetSales) As TotalRevenue,
                SUM(t3.Quantity) AS TotalQuantitySold,
                COUNT(DISTINCT(t3.ShopifyOrderId)) AS TotalOrders,
		        SUM(t3.CoGS) AS TotalCogs, SUM(t3.NetSales) - SUM(t3.CoGS) AS TotalProfit,
                CASE WHEN SUM(t3.NetSales) = 0 THEN 0 ELSE 100.0 - (100.0 * SUM(t3.CoGS) / SUM(t3.NetSales)) END AS AverageMargin
                FROM profitquerystub(@PwShopId) t1
		            INNER JOIN variant(@PwShopId) t2
		                ON t1.PwMasterVariantId = t2.PwMasterVariantId 
	                INNER JOIN profitreportentry(@PwShopId) t3
		                ON t2.PwProductId = t3.PwProductId 
                            AND t2.PwVariantId = t3.PwVariantId
                            AND t3.EntryDate >= @StartDate
                            AND t3.EntryDate <= @EndDate             
                WHERE t1.PwReportId = @PwReportId ";
        }

        private string OrderingAndPagingForTotals(TotalQueryContext queryContext)
        {
            string orderByClause = "";
            if (queryContext.Ordering == ColumnOrdering.AverageMarginDescending)
                orderByClause = "ORDER BY AverageMargin DESC ";
            if (queryContext.Ordering == ColumnOrdering.AverageMarginAscending)
                orderByClause = "ORDER BY AverageMargin ASC ";
            if (queryContext.Ordering == ColumnOrdering.ProfitDescending)
                orderByClause = "ORDER BY TotalProfit DESC ";
            if (queryContext.Ordering == ColumnOrdering.ProfitAscending)
                orderByClause = "ORDER BY TotalProfit ASC ";
            if (queryContext.Ordering == ColumnOrdering.CogsDescending)
                orderByClause = "ORDER BY TotalCogs DESC ";
            if (queryContext.Ordering == ColumnOrdering.CogsAscending)
                orderByClause = "ORDER BY TotalCogs ASC ";
            if (queryContext.Ordering == ColumnOrdering.NetSalesDescending)
                orderByClause = "ORDER BY TotalRevenue DESC ";
            if (queryContext.Ordering == ColumnOrdering.NetSalesAscending)
                orderByClause = "ORDER BY TotalRevenue ASC ";
            if (queryContext.Ordering == ColumnOrdering.QuantitySoldDescending)
                orderByClause = "ORDER BY TotalQuantitySold DESC ";
            if (queryContext.Ordering == ColumnOrdering.QuantitySoldAscending)
                orderByClause = "ORDER BY TotalQuantitySold ASC ";

            return orderByClause + "OFFSET @StartingIndex ROWS FETCH NEXT @PageSize ROWS ONLY;";
        }
        
        // Date Period Bucketed Totals
        public List<DatePeriodTotal> RetrieveDatePeriodTotals(
                long reportId, DateTime startDate, DateTime endDate, List<string> filterKeys,
                ReportGrouping grouping, PeriodType periodType)
        {
            var innerQuery = DatePeriodTotalsInnerQuery(filterKeys, grouping, periodType);

            // NOTE: for MySQL go back prior to 1/26/2017 for structuring of this query without CTE
            var cteQueryStart =
                @"WITH InnerQuery ( Year, Quarter, Month, Week, Day, GroupingKey, GroupingName, NetSales, CoGS ) AS 
               (" + innerQuery + ")";

            var cteBody = 
                @" GroupingKey, GroupingName, SUM(NetSales) AS TotalRevenue, SUM(CoGS) AS TotalCogs 
                FROM InnerQuery ";

            string query = "";
            if (periodType == PeriodType.Year)
            {
                query = cteQueryStart + @"SELECT Year, " + cteBody +
                        @"GROUP BY Year, GroupingKey, GroupingName ORDER BY Year;";
            }
            else if (periodType == PeriodType.Quarter)
            {
                query = cteQueryStart + @"SELECT Year, Quarter, " + cteBody + 
                        @"GROUP BY Year, Quarter, GroupingKey, GroupingName
                        ORDER BY Year, Quarter;";
            }
            else if (periodType == PeriodType.Month)
            {
                query = cteQueryStart + @"SELECT Year, Quarter, Month, " + cteBody + 
                        @"GROUP BY Year, Quarter, Month, GroupingKey, GroupingName
                        ORDER BY Year, Quarter, Month;";
            }
            else if (periodType == PeriodType.Week)
            {
                query = cteQueryStart + @"SELECT Year, Quarter, Month, Week, " + cteBody +
                    @"GROUP BY Year, Quarter, Month, Week, GroupingKey, GroupingName
                    ORDER BY Year, Quarter, Month, Week;";
            }
            else // DataGranularity.Day
            {
                query = cteQueryStart + @"SELECT Year, Quarter, Month, Week, Day, " + cteBody +
                     @"GROUP BY Year, Quarter, Month, Week, Day, GroupingKey, GroupingName
                    ORDER BY Year, Quarter, Month, Week, Day;";
            }
            
            var output = _connectionWrapper
                .Query<DatePeriodTotal>(
                    query,
                    new { PwShopId, PwReportId = reportId, StartDate = startDate, EndDate = endDate, FilterKeys = filterKeys, })
                .ToList();

            // ... and finally decorate these values
            output.ForEach(x =>
            {
                x.GroupingType = grouping;
                x.PeriodType = periodType;
            });
            return output;
        }

        private static string DatePeriodTotalsInnerQuery(List<string> filterKeys, ReportGrouping grouping, PeriodType periodType)
        {
            string dateHeader;
            if (periodType == PeriodType.Year)
            {
                dateHeader = $@"t4.y AS Year, NULL AS Quarter, NULL AS Month, NULL AS Week, NULL AS Day, ";
            }
            else if (periodType == PeriodType.Quarter)
            {
                dateHeader =
                    $@"t4.y AS Year, t4.q AS Quarter, NULL AS Month, NULL AS Week, NULL AS Day, ";
            }
            else if (periodType == PeriodType.Month)
            {
                dateHeader =
                    $@"t4.y AS Year, t4.q AS Quarter, t4.m AS Month, NULL AS Week, NULL AS Day, ";
            }
            else if (periodType == PeriodType.Week)
            {
                dateHeader =
                    $@"t4.y AS Year, t4.q AS Quarter, t4.m AS Month, t4.w AS Week, NULL AS Day, ";
            }
            else // DataGranularity.Day
            {
                dateHeader =
                    $@"t4.y AS Year, t4.q AS Quarter, t4.m AS Month, t4.w AS Week, t4.d AS Day, ";
            }

            string groupingHeader;
            if (grouping == ReportGrouping.Product)
            {
                groupingHeader = $@"t1.PwMasterProductId AS GroupingKey, t1.ProductTitle AS GroupingName, ";
            }
            else if (grouping == ReportGrouping.Variant)
            {
                groupingHeader =
                    $@"t1.PwMasterVariantId AS GroupingKey, CONCAT(t1.Sku, ' - ', t1.VariantTitle) AS GroupingName, ";
            }
            else if (grouping == ReportGrouping.ProductType)
            {
                groupingHeader = $@"t1.ProductType AS GroupingKey, t1.ProductType AS GroupingName, ";
            }
            else if (grouping == ReportGrouping.Vendor)
            {
                groupingHeader = $@"t1.Vendor AS GroupingKey, t1.Vendor AS GroupingName, ";
            }
            else // Overall
            {
                groupingHeader = $@"NULL AS GroupingKey, 'Overall' AS GroupingName, ";
            }

            var queryGuts =
                @"t3.NetSales AS TotalRevenue, t3.CoGS AS TotalCogs
                FROM profitquerystub(@PwShopId) t1
	                INNER JOIN variant(@PwShopId) t2
		                ON t1.PwMasterVariantId = t2.PwMasterVariantId 
	                INNER JOIN profitreportentry(@PwShopId) t3
		                ON t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId
	                INNER JOIN calendar_table t4
		                ON t3.EntryDate = t4.dt
                WHERE t1.PwReportID = @PwReportId 
                AND t3.EntryDate >= @StartDate
                AND t3.EntryDate <= @EndDate ";

            var filterClause = "";
            if (filterKeys != null && filterKeys.Count > 0)
            {
                if (grouping == ReportGrouping.Product)
                {
                    filterClause = "AND t1.PwMasterProductId in @FilterKeys ";
                }
                if (grouping == ReportGrouping.Variant)
                {
                    filterClause = "AND t1.PwMasterVariantId in @FilterKeys ";
                }
                if (grouping == ReportGrouping.ProductType)
                {
                    filterClause = "AND t1.ProductType in @FilterKeys ";
                }
                if (grouping == ReportGrouping.Vendor)
                {
                    filterClause = "AND t1.Vendor in @FilterKeys ";
                }
            }
            var innerQuery = @"SELECT " + dateHeader + groupingHeader + queryGuts + filterClause;
            return innerQuery;
        }

        public List<DateTotal> RetrieveDateTotals(long reportId, DateTime startDate, DateTime endDate)
        {
            var query =
                @"SELECT t3.EntryDate AS OrderDate, SUM(t3.NetSales) AS TotalRevenue, SUM(t3.CoGS) AS TotalCogs
                FROM profitquerystub(@PwShopId) t1
	                INNER JOIN variant(@PwShopId) t2
		                ON t1.PwMasterVariantId = t2.PwMasterVariantId 
	                INNER JOIN profitreportentry(@PwShopId) t3
		                ON t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId
                WHERE t1.PwReportID = @PwReportId AND t3.EntryDate >= @StartDate AND t3.EntryDate <= @EndDate 
                GROUP BY t3.EntryDate ORDER BY t3.EntryDate";
            
            return _connectionWrapper.Query<DateTotal>(
                    query, new { PwShopId, PwReportId = reportId, StartDate = startDate, EndDate = endDate })
                .ToList();
        }
    }
}
