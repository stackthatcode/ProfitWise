using System;
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
                        Vendor, ProductType, ProductTitle, VariantTitle
                FROM vw_MasterProductAndVariantSearch 
                WHERE PwShopId = @PwShopId " +
                ReportFilterClauseGenerator(reportId) + 
                " GROUP BY PwMasterVariantId;";
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
                @"SELECT t1.PwMasterProductId AS GroupingId, t1.ProductTitle AS GroupingName, " +
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
                @"SELECT t1.PwMasterVariantId AS GroupingId, t1.VariantTitle AS GroupingName, " +
                QueryGutsForTotals() +
                @"GROUP BY t1.PwMasterVariantId, t1.VariantTitle " +
                QueryTailForTotals(10);

            return _connection.Query<GroupedTotal>(
                    query, new { PwShopId, PwReportId = reportId, StartDate = startDate, EndDate = endDate })
                .ToList();
        }

        public List<GroupedTotal> RetreiveTotalsByProductType(long reportId, DateTime startDate, DateTime endDate)
        {
            var query =
                @"SELECT t1.ProductType AS GroupingId, t1.ProductType AS GroupingName, " +
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
                @"SELECT t1.Vendor AS GroupingId, t1.Vendor AS GroupingName, " +
                QueryGutsForTotals() +
                @"GROUP BY t1.Vendor " +
                QueryTailForTotals(10);

            return _connection.Query<GroupedTotal>(
                    query, new { PwShopId, PwReportId = reportId, StartDate = startDate, EndDate = endDate })
                .ToList();
        }

        // Matches the following Date Label convention: 2014, 2014 Q2, January 2014, Week 13 of 2014, 4/11/2014
        public List<CanonizedDateTotal> 
                RetrieveCanonizedDateTotals(
                    long reportId, 
                    DateTime startDate, 
                    DateTime endDate, 
                    ReportGrouping grouping, 
                    DataGranularity granularity)
        {
            string dateField;
            if (granularity == DataGranularity.Year)
                dateField = "t4.y AS DateLabel, ";
            else if (granularity == DataGranularity.Quarter) 
                dateField = "t4.y, t4.q, CONCAT('Q', t4.q, ', ', t4.y) AS DateLabel, ";
            else if (granularity == DataGranularity.Month)
                dateField = "t4.y, t4.m, CONCAT(t4.monthName, ' ',  t4.y) AS DateLabel, ";
            else if (granularity == DataGranularity.Week)
                dateField = "t4.y, t4.w, CONCAT('Week ', t4.w, ' of ', t4.y) AS DateLabel, ";
            else // DataGranularity.Day
                dateField = "t4.dt, CONCAT(t4.m, '/', t4.d, '/', t4.y) AS DateLabel, ";

            // IMPORTANT *** canonicalized date identifier format
            string dateIdField;
            if (granularity == DataGranularity.Year)
                dateIdField = "t4.y AS DateIdentifier, ";
            else if (granularity == DataGranularity.Quarter)
                dateIdField = "t4.y, t4.q, CONCAT(t4.y, ':Q',  t4.q) AS DateIdentifier, ";
            else if (granularity == DataGranularity.Month)
                dateIdField = "t4.y, t4.m, CONCAT(t4.y, ':M', t4.m) AS DateIdentifier, ";
            else if (granularity == DataGranularity.Week)
                dateIdField = "t4.y, t4.w, CONCAT(t4.y, ':W', t4.w) AS DateIdentifier, ";
            else // DataGranularity.Day
                dateIdField = "t4.dt, CONCAT(t4.y, ':',  t4.m, ':', t4.d) AS DateIdentifier, ";

            string groupingField;
            if (grouping == ReportGrouping.Product)
                groupingField = "t1.ProductTitle AS GroupingName, ";
            else if (grouping == ReportGrouping.Variant)
                groupingField = "t1.VariantTitle AS GroupingName, ";
            else if (grouping == ReportGrouping.ProductType)
                groupingField = "t1.ProductType AS GroupingName, ";
            else if (grouping == ReportGrouping.Vendor)
                groupingField = "t1.Vendor AS GroupingName, ";
            else // Overall
                groupingField = "'Overall' AS GroupingName, ";

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

            string queryTail;
            if (granularity == DataGranularity.Year)
                queryTail = "GROUP BY t4.y, GroupingName ORDER BY t4.y;";
            else if (granularity == DataGranularity.Quarter)
                queryTail = "GROUP BY t4.y, t4.q, GroupingName ORDER BY t4.y, t4.q;";
            else if (granularity == DataGranularity.Month)
                queryTail = "GROUP BY t4.y, t4.m, GroupingName ORDER BY t4.y, t4.m;";
            else if (granularity == DataGranularity.Week)
                queryTail = "GROUP BY t4.y, t4.w, GroupingName ORDER BY t4.y, t4.w;";
            else // DataGranularity.Day
                queryTail = "GROUP BY t4.dt, GroupingName ORDER BY t4.dt;";

            var query = @"SELECT " + dateField + dateIdField + groupingField + queryGuts + queryTail;

            return _connection.Query<CanonizedDateTotal>(
                    query, new { PwShopId, PwReportId = reportId, StartDate = startDate, EndDate = endDate })
                .ToList();
        }


        public List<DateTotal> RetrieveOverallDateTotals(
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

