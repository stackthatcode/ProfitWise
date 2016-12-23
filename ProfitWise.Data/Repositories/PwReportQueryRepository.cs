﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.Profit;
using ProfitWise.Data.Model.Reports;
using Push.Foundation.Utilities.Logging;

namespace ProfitWise.Data.Repositories
{
    public class PwReportQueryRepository
    {
        private readonly MySqlConnection _connection;
        private readonly MultitenantFactory _factory;
        private readonly IPushLogger _logger;
        public PwShop PwShop { get; set; }
        public long PwShopId => PwShop.PwShopId;

        public PwReportQueryRepository(MySqlConnection connection, MultitenantFactory factory, IPushLogger logger)
        {
            _connection = connection;
            _factory = factory;
            _logger = logger;
        }


        // Product & Variant counts and selection details
        public ProductAndVariantCount RetrieveReportRecordCount(long reportId)
        {
            var query =
                @"SELECT COUNT(PwMasterProductId) AS ProductCount, 
                        COUNT(PwMasterVariantId) AS VariantCount
                FROM vw_MasterProductAndVariantSearch 
                WHERE PwShopId = @PwShopId ";

            query += ReportFilterClauseGenerator(reportId);

            return _connection
                .Query<ProductAndVariantCount>(query, new { PwShopId, PwReportId = reportId })
                .FirstOrDefault();
        }

        public List<PwReportSelectionMasterProduct> RetrieveProductSelections(long reportId, int pageNumber, int pageSize)
        {
            var query =
                @"SELECT PwShopId, PwMasterProductId, ProductTitle AS Title, Vendor, ProductType
                FROM vw_MasterProductAndVariantSearch   
                WHERE PwShopId = @PwShopId ";
            query += ReportFilterClauseGenerator(reportId);
            query += @" GROUP BY PwMasterProductId, Title, Vendor, ProductType 
                        ORDER BY Title LIMIT @startRecord, @pageSize";

            var startRecord = (pageNumber - 1) * pageSize;

            return _connection
                .Query<PwReportSelectionMasterProduct>(query, new { PwShopId, PwReportId = reportId, startRecord, pageSize, })
                .ToList();
        }

        public List<PwReportSelectionMasterVariant> RetrieveVariantSelections(long reportId, int pageNumber, int pageSize)
        {
            var query =
                @"SELECT PwShopId, PwMasterProductId, ProductTitle, PwMasterVariantId, VariantTitle, Sku, Vendor
                FROM vw_MasterProductAndVariantSearch   
                WHERE PwShopId = @PwShopId ";
            query += ReportFilterClauseGenerator(reportId);
            query += @" ORDER BY ProductTitle, VariantTitle LIMIT @startRecord, @pageSize";

            var startRecord = (pageNumber - 1) * pageSize;

            return _connection
                .Query<PwReportSelectionMasterVariant>(query, new { PwShopId, PwReportId = reportId, startRecord, pageSize })
                .ToList();
        }

        public string ReportFilterClauseGenerator(long reportId)
        {
            var query = "";
            var filterRepository = _factory.MakeReportFilterRepository(this.PwShop);
            var filters = filterRepository.RetrieveFilters(reportId);

            if (filters.Count(x => x.FilterType == PwReportFilter.ProductType) > 0)
            {
                query += $@"AND ProductType IN ( SELECT StringKey FROM profitwisereportfilter 
                            WHERE PwReportId = @PwReportId AND FilterType = {PwReportFilter.ProductType} ) ";
            }
            if (filters.Count(x => x.FilterType == PwReportFilter.Vendor) > 0)
            {
                query += $@"AND Vendor IN ( SELECT StringKey FROM profitwisereportfilter 
                            WHERE PwReportId = @PwReportId AND FilterType = {PwReportFilter.Vendor} ) ";
            }
            if (filters.Count(x => x.FilterType == PwReportFilter.Product) > 0)
            {
                query += $@"AND PwMasterProductId IN ( SELECT NumberKey FROM profitwisereportfilter 
                            WHERE PwReportId = @PwReportId AND FilterType = {PwReportFilter.Product} ) ";
            }
            if (filters.Count(x => x.FilterType == PwReportFilter.Sku) > 0)
            {
                query += $@"AND PwMasterVariantId IN ( SELECT NumberKey FROM profitwisereportfilter 
                            WHERE PwReportId = @PwReportId AND FilterType = {PwReportFilter.Sku} ) ";
            }

            return query;
        }


        // Profit Query output
        public void PopulateQueryStub(long reportId)
        {
            var deleteQuery =
                @"DELETE FROM profitwisereportquerystub
                WHERE PwShopId = @PwShopId AND PwReportId = @PwReportId";
            _connection.Execute(deleteQuery, new { PwShopId, PwReportId = reportId });

            var createQuery =
                @"INSERT INTO profitwisereportquerystub
                SELECT @PwReportId, @PwShopId, PwMasterVariantId, PwMasterProductId, 
                        Vendor, ProductType, ProductTitle, Sku, VariantTitle
                FROM vw_MasterProductAndVariantSearch 
                WHERE PwShopId = @PwShopId " +
                ReportFilterClauseGenerator(reportId) +
                @" GROUP BY PwMasterVariantId,  PwMasterProductId, 
                    Vendor, ProductType, ProductTitle, Sku, VariantTitle; ";
            _connection.Execute(createQuery, new { PwShopId, PwReportId = reportId });
        }

        public List<PwReportSearchStub> RetrieveSearchStubs(long reportId)
        {
            var query =
                @"SELECT t2.*
                FROM profitwisereportquerystub t1 
	                INNER JOIN vw_MasterProductAndVariantSearch t2
		                ON t1.PwMasterVariantId = t2.PwMasterVariantId
                WHERE t1.PwShopId = @PwShopId
                AND t1.PwReportId = @reportId
                AND t2.PwShopId = @PwShopId";

            var results =
                _connection
                    .Query<PwReportSearchStub>(
                        query, new { PwShopId, reportId }).ToList();
            return results;
        }

        // Dataset #2 operations
        public string QueryGutsForTotals()
        {
            return @"SUM(t3.GrossRevenue) As TotalRevenue, 
                    SUM(t3.Quantity - t3.TotalRestockedQuantity) AS TotalNumberSold,
		            SUM(t3.UnitCogs * (t3.Quantity - t3.TotalRestockedQuantity)) AS TotalCogs
            FROM profitwisereportquerystub t1
		        INNER JOIN profitwisevariant t2
		            ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId 
	            INNER JOIN shopifyorderlineitem t3
		            ON t1.PwShopId = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId  
			            AND t3.OrderDate >= @StartDate AND t3.OrderDate <= @EndDate             
            WHERE t1.PwShopId = @PwShopId AND t1.PwReportId = @PwReportId ";
        }

        public string QueryTailForTotals(int limit)
        {
            return $"ORDER BY TotalRevenue DESC LIMIT {limit};";
        }

        public ExecutiveSummary RetreiveTotalsForAll(long reportId, DateTime startDate, DateTime endDate)
        {
            var query = @"SELECT " + QueryGutsForTotals();
            return _connection
                    .Query<ExecutiveSummary>(
                        query, new {PwShopId, PwReportId = reportId, StartDate = startDate , EndDate = endDate})
                    .First();
        }

        public List<GroupedTotal> RetreiveTotalsByProduct(long reportId, DateTime startDate, DateTime endDate)
        {
            var query =
                @"SELECT t1.PwMasterProductId AS GroupingKey, t1.ProductTitle AS GroupingName, " +
                QueryGutsForTotals() +
                @"GROUP BY t1.PwMasterProductId, t1.ProductTitle " +
                QueryTailForTotals(10);

            return _connection.Query<GroupedTotal>(
                    query, new { PwShopId, PwReportId = reportId, StartDate = startDate, EndDate = endDate })
                .ToList();
        }

        public List<GroupedTotal> RetreiveTotalsByVariant(long reportId, DateTime startDate, DateTime endDate)
        {
            var query =
                @"SELECT t1.PwMasterVariantId AS GroupingKey, CONCAT(t1.Sku, ' - ', t1.VariantTitle) AS GroupingName, " +
                QueryGutsForTotals() +
                @"GROUP BY t1.PwMasterVariantId, GroupingName " +
                QueryTailForTotals(10);

            return _connection.Query<GroupedTotal>(
                    query, new { PwShopId, PwReportId = reportId, StartDate = startDate, EndDate = endDate })
                .ToList();
        }

        public List<GroupedTotal> RetreiveTotalsByProductType(long reportId, DateTime startDate, DateTime endDate)
        {
            var query =
                @"SELECT t1.ProductType AS GroupingKey, t1.ProductType AS GroupingName, " +
                QueryGutsForTotals() +
                @"GROUP BY t1.ProductType " +
                QueryTailForTotals(10);

            return _connection.Query<GroupedTotal>(
                    query, new { PwShopId, PwReportId = reportId, StartDate = startDate, EndDate = endDate })
                .ToList();
        }

        public List<GroupedTotal> RetreiveTotalsByVendor(long reportId, DateTime startDate, DateTime endDate)
        {
            var query =
                @"SELECT t1.Vendor AS GroupingKey, t1.Vendor AS GroupingName, " +
                QueryGutsForTotals() +
                @"GROUP BY t1.Vendor " +
                QueryTailForTotals(10);

            return _connection.Query<GroupedTotal>(
                    query, new { PwShopId, PwReportId = reportId, StartDate = startDate, EndDate = endDate })
                .ToList();
        }


        public List<DatePeriodTotal> RetrieveDatePeriodTotals(
                long reportId, DateTime startDate, DateTime endDate, List<string> filterKeys,
                ReportGrouping grouping, PeriodType periodType)
        {
            string dateHeader;
            if (periodType == PeriodType.Year)
            {
                dateHeader = $@"t4.y AS Year, ";
            }
            else if (periodType == PeriodType.Quarter)
            {
                dateHeader =
                    $@"t4.y AS Year, t4.q AS Quarter, ";
            }
            else if (periodType == PeriodType.Month)
            {
                dateHeader =
                    $@"t4.y AS Year, t4.q AS Quarter, t4.m AS Month, ";
            }
            else if (periodType == PeriodType.Week)
            {
                dateHeader =
                    $@"t4.y AS Year, t4.q AS Quarter, t4.m AS Month, t4.w AS Week, ";
            }
            else   // DataGranularity.Day
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
                groupingHeader = $@"t1.PwMasterProductId AS GroupingKey, t1.ProductTitle AS GroupingName, ";
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
                @"SUM(t3.GrossRevenue) AS TotalRevenue, 
		        SUM(t3.UnitCogs * (t3.Quantity - t3.TotalRestockedQuantity)) AS TotalCogs
                FROM profitwisereportquerystub t1
	                INNER JOIN profitwisevariant t2
		                ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId 
	                INNER JOIN shopifyorderlineitem t3
		                ON t2.PwShopID = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId
	                INNER JOIN calendar_table t4
		                ON t3.OrderDate = t4.dt
                WHERE t1.PwShopId = @PwShopId AND t1.PwReportID = @PwReportId 
                AND t3.OrderDate >= @StartDate AND t3.OrderDate <= @EndDate ";

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

            string groupAndOrderClause;
            if (periodType == PeriodType.Year)
            {
                groupAndOrderClause = 
                    @"GROUP BY Year, GroupingKey, GroupingName
                    ORDER BY Year;";
            }
            else if (periodType == PeriodType.Quarter)
            {
                groupAndOrderClause =
                    @"GROUP BY Year, Quarter, GroupingKey, GroupingName
                    ORDER BY Year, Quarter;";
            }
            else if (periodType == PeriodType.Month)
            {
                groupAndOrderClause =
                    @"GROUP BY Year, Quarter, Month, GroupingKey, GroupingName
                    ORDER BY Year, Quarter, Month;";
            }
            else if (periodType == PeriodType.Week)
            {
                groupAndOrderClause =
                    @"GROUP BY Year, Quarter, Month, Week, GroupingKey, GroupingName
                    ORDER BY Year, Quarter, Month, Week;";
            }
            else // DataGranularity.Day
            {
                groupAndOrderClause =
                     @"GROUP BY Year, Quarter, Month, Week, Day, GroupingKey, GroupingName
                    ORDER BY Year, Quarter, Month, Week, Day;";
            }

            var query = @"SELECT " + dateHeader + dateHeader + groupingHeader + queryGuts + filterClause + groupAndOrderClause;

            var output = _connection
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


        public List<DateTotal> RetrieveDateTotals(
                long reportId, DateTime startDate, DateTime endDate)
        {
            var query = 
                @"SELECT t3.OrderDate,
		                SUM(t3.GrossRevenue) AS TotalRevenue, 
		                SUM(t3.UnitCogs * (t3.Quantity - t3.TotalRestockedQuantity)) AS TotalCogs
                FROM profitwisereportquerystub t1
	                INNER JOIN profitwisevariant t2
		                ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId 
	                INNER JOIN shopifyorderlineitem t3
		                ON t2.PwShopID = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId
                WHERE t1.PwShopId = @PwShopId AND t1.PwReportID = @PwReportId
                AND t3.OrderDate >= @StartDate AND t3.OrderDate <= @EndDate 
                GROUP BY t3.OrderDate
                ORDER BY t3.OrderDate";

            return _connection.Query<DateTotal>(
                    query, new { PwShopId, PwReportId = reportId, StartDate = startDate, EndDate = endDate })
                .ToList();
        }
    }
}

