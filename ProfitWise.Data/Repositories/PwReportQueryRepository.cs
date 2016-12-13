﻿using System.Collections.Generic;
using System.Linq;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;

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

        // Product-Variant reference data for creating Filters

        // Product & Variant selections
        public PwReportRecordCount RetrieveReportRecordCount(long reportId)
        {
            var query =
                @"SELECT COUNT(DISTINCT(t1.PwProductId)) AS ProductCount, COUNT(t3.PwVariantId) AS VariantCount
                FROM profitwiseproduct t1 
	                INNER JOIN profitwisemastervariant t2 ON t1.PwMasterProductId = t2.PwMasterProductId
                    INNER JOIN profitwisevariant t3 ON t2.PwMasterVariantId = t3.PwMasterVariantId    
                WHERE t1.PwShopId = @PwShopId AND t2.PwShopId = @PwShopId AND t3.PwShopId = @PwShopId
                AND t1.IsPrimary = 1 AND t3.IsPrimary = 1 ";

            query += ReportFilterClauseGenerator(reportId);

            return _connection
                .Query<PwReportRecordCount>(query, new { PwShopId, PwReportId = reportId })
                .FirstOrDefault();
        }

        public List<PwReportMasterProductSelection> RetrieveProductSelections(long reportId, int pageNumber, int pageSize)
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
                .Query<PwReportMasterProductSelection>(query, new { PwShopId, PwReportId = reportId, startRecord, pageSize, })
                .ToList();
        }

        public List<PwReportMasterVariantSelection> RetrieveVariantSelections(long reportId, int pageNumber, int pageSize)
        {
            var query =
                @"SELECT PwShopId, PwMasterProductId, ProductTitle, PwMasterVariantId, VariantTitle, Sku, Vendor
                FROM vw_MasterProductAndVariantSearch   
                WHERE PwShopId = @PwShopId ";
            query += ReportFilterClauseGenerator(reportId);
            query += @" ORDER BY ProductTitle, VariantTitle LIMIT @startRecord, @pageSize";

            var startRecord = (pageNumber - 1) * pageSize;

            return _connection
                .Query<PwReportMasterVariantSelection>(query, new { PwShopId, PwReportId = reportId, startRecord, pageSize })
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


        public void GenerateQueryStub(long reportId)
        {
            var deleteQuery = 
                @"DELETE FROM profitwisereportquerystub
                WHERE PwShopId = @PwShopId AND PwReportId = @reportId";
            _connection.Execute(deleteQuery, new { PwShopId, reportId });

            var createQuery =
                @"INSERT INTO profitwisereportquerystub
                SELECT @reportId, @PwShopId, PwMasterVariantId
                FROM vw_MasterProductAndVariantSearch 
                WHERE PwShopId = @PwShopId AND PwReportId = @reportId " +
                ReportFilterClauseGenerator(reportId) + 
                " GROUP BY PwMasterVariantId;";
            _connection.Execute(createQuery, new { PwShopId, reportId });
        }

        public IList<PwReportOrderLineProfit> RetrieveQueryOrders(long reportId)
        {
            var query =
                @"SELECT * FROM vw_ReportOrderset 
                WHERE PwShopId = @PwShopId AND PWReportId = @reportId";

            var results = _connection.Query<PwReportOrderLineProfit>(query, new { PwShopId }).ToList();
            return results;
        }

        public IList<PwReportOrderLineProfit> RetrieveCogsData(long reportId)
        {
            var query =
                @"SELECT * FROM vw_ReportOrderset 
                WHERE PwShopId = @PwShopId AND PWReportId = @reportId";

            var results = _connection.Query<PwReportOrderLineProfit>(query, new { PwShopId }).ToList();
            return results;
        }

    }
}

