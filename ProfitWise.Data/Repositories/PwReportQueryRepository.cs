using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.Profit;
using ProfitWise.Data.Model.Reports;

namespace ProfitWise.Data.Repositories
{
    public class PwReportQueryRepository
    {
        private readonly MySqlConnection _connection;
        private readonly MultitenantFactory _factory;
        public PwShop PwShop { get; set; }
        public long PwShopId => PwShop.PwShopId;

        public PwReportQueryRepository(MySqlConnection connection, MultitenantFactory factory)
        {
            _connection = connection;
            _factory = factory;
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
        public List<GroupedTotal> RetreiveTotalsByProduct(long reportId)
        {
            var query =
                @"SELECT t1.PwMasterProductId AS GroupingId,
		                t4.Title AS GroupingName,
		                SUM(t3.GrossRevenue) As TotalRevenue, 
                        SUM(t3.Quantity - t3.TotalRestockedQuantity) AS TotalNumberSold,
		                SUM(t3.UnitCogs * (t3.Quantity - t3.TotalRestockedQuantity)) AS TotalCogs
                FROM profitwisereport t0
	                INNER JOIN profitwisereportquerystub t1
		                ON t0.PwShopId = t1.PwShopId AND t0.PwReportId = t1.PwReportId 
	                INNER JOIN profitwisevariant t2
		                ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId 
	                INNER JOIN shopifyorderlineitem t3
		                ON t1.PwShopId = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId  
			                AND t3.OrderDate >= t0.StartDate AND t3.OrderDate <= t0.EndDate
 	                LEFT JOIN profitwiseproduct t4
		                ON t1.PwShopId = t4.PwShopId AND t4.PwMasterProductId = t1.PwMasterProductId AND t4.IsPrimary = 1               
                WHERE t0.PwShopId = @PwShopId AND t0.PwReportID = @reportId
                GROUP BY t1.PwMasterProductId, t4.Title
                ORDER BY TotalRevenue DESC
                LIMIT 10;";

            return _connection.Query<GroupedTotal>(query, new { PwShopId, reportId }).ToList();
        }

        public List<GroupedTotal> RetreiveTotalsByVariant(long reportId)
        {
            var query =
                @"SELECT t1.PwMasterProductId AS GroupingId,
		                t4.Title AS GroupingName,
		                SUM(t3.GrossRevenue) As TotalRevenue, 
                        SUM(t3.Quantity - t3.TotalRestockedQuantity) AS TotalNumberSold,
		                SUM(t3.UnitCogs * (t3.Quantity - t3.TotalRestockedQuantity)) AS TotalCogs
                FROM profitwisereport t0
	                INNER JOIN profitwisereportquerystub t1
		                ON t0.PwShopId = t1.PwShopId AND t0.PwReportId = t1.PwReportId 
	                INNER JOIN profitwisevariant t2
		                ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId 
	                INNER JOIN shopifyorderlineitem t3
		                ON t1.PwShopId = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId  
			                AND t3.OrderDate >= t0.StartDate AND t3.OrderDate <= t0.EndDate
 	                LEFT JOIN profitwiseproduct t4
		                ON t1.PwShopId = t4.PwShopId AND t4.PwMasterProductId = t1.PwMasterProductId AND t4.IsPrimary = 1               
                WHERE t0.PwShopId = @PwShopId AND t0.PwReportID = @reportId
                GROUP BY t1.PwMasterProductId, t4.Title
                ORDER BY TotalRevenue DESC
                LIMIT 10;";

            return _connection.Query<GroupedTotal>(query, new { PwShopId, reportId }).ToList();
        }




        // Dataset #1 operations 
        [Obsolete]
        public List<OrderLineProfit> 
                    RetrieveOrderLineProfits(long reportId, DateTime startDate, DateTime endDate)
        {
            endDate = endDate.AddDays(1);

            var query =
                @"SELECT t1.PwReportId, t1.PwShopId, t2.PwMasterVariantId, t2.PwProductId, t2.PwVariantId, 
		        t3.OrderDate, t3.ShopifyOrderId, t3.ShopifyOrderLineId, t3.Quantity, t3.TotalRestockedQuantity, t3.UnitPrice, t3.GrossRevenue,
                t4.OrderNumber
                FROM profitwisereportquerystub t1
	                INNER JOIN profitwisevariant t2
		                ON t1.PwMasterVariantId = t2.PwMasterVariantId 
			                AND t1.PwShopId = t2.PwShopId
	                INNER JOIN shopifyorderlineitem t3
		                ON t2.PwProductId = t3.PwProductId 
			                AND t2.PwVariantId = t3.PwVariantId
                            AND t2.PwShopID = t3.PwShopId
	                INNER JOIN shopifyorder t4
		                ON t3.ShopifyOrderId = t4.ShopifyOrderId
                            AND t3.PwShopID = t4.PwShopId
                WHERE t1.PwReportID = @reportId
                AND t1.PwShopId = @PwShopId
                AND t3.OrderDate >= @startDate
                AND t3.OrderDate <= @endDate;";
            
            var results = 
                _connection
                    .Query<OrderLineProfit>(
                        query, new { PwShopId, reportId, startDate, endDate }).ToList();
            return results;
        }

        [Obsolete]
        public List<PwReportMasterVariantCogs> RetrieveCogsData(long reportId)
        {
            var query =
                @"SELECT t2.PwMasterVariantId, t1.CogsCurrencyId, t1.CogsAmount
                FROM profitwisemastervariant t1 
	                INNER JOIN profitwisereportquerystub t2 
                        ON t1.PwMasterVariantId = t2.PwMasterVariantId
                WHERE t1.PwShopId = @PwShopId 
                AND t1.PwShopId = @PwShopId 
                AND t2.PwReportId = @reportId;";

            var results = 
                _connection.Query<PwReportMasterVariantCogs>(
                    query, new { PwShopId, reportId }).ToList();
            return results;
        }

    }
}

