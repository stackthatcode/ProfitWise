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
                @"SELECT COUNT(DISTINCT(PwMasterProductId)) AS ProductCount, 
                        COUNT(DISTINCT(PwMasterVariantId)) AS VariantCount
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


        // Queries for generating Totals   
        public int FilterCount(long reportId)
        {
            var filterCountQuery =
                @"SELECT COUNT(PwFilterId) FROM profitwisereportfilter
                WHERE PwShopId = @PwShopId AND PwReportId = @reportId";
            var filterCount = _connection.Query<int>(filterCountQuery, new { this.PwShopId, reportId }).First();
            return filterCount;
        }

        public GroupedTotal RetreiveTotalsForAll(TotalQueryContext queryContext)
        {
            var filterCount = FilterCount(queryContext.PwReportId);

            if (filterCount > 0)
            {
                var totalsQuery = @"SELECT " + QueryGutsForTotals();
                return _connection.Query<GroupedTotal>(totalsQuery, queryContext).First();
            }
            else
            {
                var totalsQuery =
                    @"SELECT 
                        SUM(t1.Quantity) AS TotalQuantitySold,
	                    SUM(t1.NetSales) As TotalRevenue, 
	                    SUM(t1.CoGS) AS TotalCogs, 
	                    SUM(t1.NetSales) - SUM(t1.CoGS) AS TotalProfit, 
	                    100.0 - (100.0 * SUM(t1.CoGS) / SUM(t1.NetSales)) AS AverageMargin
                    FROM profitwiseprofitreportentry t1 
                        LEFT OUTER JOIN shopifyorder t2
		                    ON t1.PwShopId = t2.PwShopId AND t1.ShopifyOrderId = t2.ShopifyOrderId AND t2.Cancelled = 0
                    WHERE t1.PwShopId = @PwShopId AND t1.EntryDate >= @StartDate AND t1.EntryDate <= @EndDate";
                var totals = _connection.Query<GroupedTotal>(totalsQuery, queryContext).First();

                var numberOfOrdersQuery =
                    @"SELECT COUNT(*) AS NumberOfOrders FROM shopifyorder
                    WHERE OrderDate >= @StartDate AND OrderDate <= @EndDate
                    AND PwShopId = @PwShopId AND Cancelled = 0";

                var orderCount = _connection.Query<int>(numberOfOrdersQuery, queryContext).First();
                totals.TotalOrders = orderCount;
                return totals;
            }
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
                @"FROM profitwisereportquerystub t1
		            INNER JOIN profitwisevariant t2
		                ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId 
	                INNER JOIN profitwiseprofitreportentry t3
		                ON t2.PwShopId = t3.PwShopId 
                            AND t2.PwProductId = t3.PwProductId 
                            AND t2.PwVariantId = t3.PwVariantId  
			                AND t3.EntryDate >= @StartDate 
                            AND t3.EntryDate <= @EndDate             
                WHERE t1.PwShopId = @PwShopId AND t1.PwReportId = @PwReportId";

            var query = "";
            if (queryContext.Grouping == ReportGrouping.Product)
                query = "SELECT COUNT(DISTINCT(t1.PwMasterProductId)) " + queryGuts;
            if (queryContext.Grouping == ReportGrouping.Variant)
                query = "SELECT COUNT(DISTINCT(t1.PwMasterVariantId)) " + queryGuts;
            if (queryContext.Grouping == ReportGrouping.ProductType)
                query = "SELECT COUNT(DISTINCT(t1.ProductType)) " + queryGuts;
            if (queryContext.Grouping == ReportGrouping.Vendor)
                query = "SELECT COUNT(DISTINCT(t1.Vendor)) " + queryGuts;

            return _connection.Query<int>(query, queryContext).First();
        }

        public List<GroupedTotal> RetreiveTotalsByProduct(TotalQueryContext queryContext)
        {
            var query =
                @"SELECT t1.PwMasterProductId AS GroupingKey, t1.ProductTitle AS GroupingName, " +
                QueryGutsForTotals() +
                @"GROUP BY t1.PwMasterProductId, t1.ProductTitle " +
                OrderingAndPagingForTotals(queryContext);

            return _connection
                .Query<GroupedTotal>(query, queryContext).ToList()
                .AssignGrouping(ReportGrouping.Product);
        }

        public List<GroupedTotal> RetreiveTotalsByVariant(TotalQueryContext queryContext)
        {
            var query =
                @"SELECT t1.PwMasterVariantId AS GroupingKey, CONCAT(t1.Sku, ' - ', t1.VariantTitle) AS GroupingName, " +
                QueryGutsForTotals() +
                @"GROUP BY t1.PwMasterVariantId, GroupingName " +
                OrderingAndPagingForTotals(queryContext);

            return _connection
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

            return _connection
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

            return _connection
                .Query<GroupedTotal>(query, queryContext).ToList()
                .AssignGrouping(ReportGrouping.Vendor);
        }

        public string QueryGutsForTotals()
        {
            return @"SUM(t3.NetSales) As TotalRevenue,
                    SUM(t3.Quantity) AS TotalQuantitySold,
		            SUM(t3.CoGS) AS TotalCogs, SUM(t3.NetSales) - SUM(t3.CoGS) AS TotalProfit,
                    100.0 - (100.0 * SUM(t3.CoGS) / SUM(t3.NetSales)) AS AverageMargin
                    FROM profitwisereportquerystub t1
		                INNER JOIN profitwisevariant t2
		                    ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId 
	                    INNER JOIN profitwiseprofitreportentry t3
		                    ON t1.PwShopId = t3.PwShopId
                                AND t2.PwProductId = t3.PwProductId 
                                AND t2.PwVariantId = t3.PwVariantId
                                AND t3.EntryDate >= @StartDate
                                AND t3.EntryDate <= @EndDate             
                    WHERE t1.PwShopId = @PwShopId AND t1.PwReportId = @PwReportId ";
        }

        public string OrderingAndPagingForTotals(TotalQueryContext queryContext)
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
                orderByClause = "ORDER BY TotalNumberSold DESC ";
            if (queryContext.Ordering == ColumnOrdering.QuantitySoldAscending)
                orderByClause = "ORDER BY TotalNumberSold ASC ";

            return orderByClause + "LIMIT @StartingIndex, @PageSize";
        }



        // Date Period Bucketed Totals
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
                groupingHeader = $@"t1.PwMasterVariantId AS GroupingKey, CONCAT(t1.Sku, ' - ', t1.VariantTitle) AS GroupingName, ";
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
                @"SUM(t3.NetSales) AS TotalRevenue, SUM(t3.CoGS) AS TotalCogs
                FROM profitwisereportquerystub t1
	                INNER JOIN profitwisevariant t2
		                ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId 
	                INNER JOIN profitwiseprofitreportentry t3
		                ON t2.PwShopID = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId
	                INNER JOIN calendar_table t4
		                ON t3.EntryDate = t4.dt
                WHERE t1.PwShopId = @PwShopId AND t1.PwReportID = @PwReportId 
                AND t3.EntryDate >= @StartDate AND t3.EntryDate <= @EndDate ";

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
                @"SELECT t3.EntryDate AS OrderDate, SUM(t3.NetSales) AS TotalRevenue, SUM(t3.CoGS) AS TotalCogs
                FROM profitwisereportquerystub t1
	                INNER JOIN profitwisevariant t2
		                ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId 
	                INNER JOIN profitwiseprofitreportentry t3
		                ON t2.PwShopID = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId
                WHERE t1.PwShopId = @PwShopId AND t1.PwReportID = @PwReportId
                AND t3.EntryDate >= @StartDate AND t3.EntryDate <= @EndDate 
                GROUP BY t3.EntryDate ORDER BY t3.EntryDate";
            
            return _connection.Query<DateTotal>(
                    query, new { PwShopId, PwReportId = reportId, StartDate = startDate, EndDate = endDate })
                .ToList();
        }
    }
}

