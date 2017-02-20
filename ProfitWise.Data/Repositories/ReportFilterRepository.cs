using System;
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


namespace ProfitWise.Data.Repositories
{
    public class ReportFilterRepository
    {
        private readonly ConnectionWrapper _connectionWrapper;
        private readonly MultitenantFactory _factory;
        public PwShop PwShop { get; set; }


        private IDbConnection Connection => _connectionWrapper.DbConn;
        public long PwShopId => PwShop.PwShopId;



        public ReportFilterRepository(ConnectionWrapper connectionWrapper, MultitenantFactory factory)
        {
            _connectionWrapper = connectionWrapper;
            _factory = factory;
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.StartTransactionForScope();
        }


        // Product-Variant data for creating Filters
        public List<ProductTypeOption> RetrieveProductTypeOptions()
        {
            var query =
                @"SELECT ProductType, COUNT(*) AS Count
                FROM profitwiseproduct
                WHERE PwShopId = @PwShopId AND IsPrimary = 1 
                GROUP BY ProductType;";
            return Connection.Query<ProductTypeOption>(query, new { PwShopId }).ToList();
        }

        public IList<VendorOption> RetrieveVendorOptions(long pwReportId)
        {
            var query =
                @"SELECT Vendor, COUNT(*) AS Count 
                FROM profitwiseproduct 
                WHERE PwShopId = @PwShopId AND IsPrimary = 1 ";

            var filters = RetrieveFilters(pwReportId);
            var productTypeFilter = PwReportFilter.ProductType;
            if (filters.Count(x => x.FilterType == PwReportFilter.ProductType) > 0)
            {
                query += @" AND ProductType IN ( 
                                SELECT StringKey FROM profitwisereportfilter
                                WHERE PwReportId = @pwReportId AND FilterType = @productTypeFilter ) ";
            }

            query += " GROUP BY Vendor;";

            return Connection
                    .Query<VendorOption>(
                            query, new { PwShopId, @pwReportId, productTypeFilter }).ToList();
        }

        public IList<MasterProductOption> RetrieveMasterProductOptions(long pwReportId)
        {
            var query =
                @"SELECT t1.PwMasterProductId, t1.Vendor, t1.Title, COUNT(*) AS VariantCount
                FROM profitwiseproduct t1 
	                INNER JOIN profitwisemastervariant t2
		                ON t1.PwMasterProductId = t2.PwMasterProductId AND t1.IsPrimary = 1 
                WHERE t1.PwShopId = @PwShopId
                AND t2.PwShopId = @PwShopId ";

            var filters = RetrieveFilters(pwReportId);
            var productTypeFilter = PwReportFilter.ProductType;
            var vendorTypeFilter = PwReportFilter.Vendor;

            if (filters.Count(x => x.FilterType == PwReportFilter.ProductType) > 0)
            {
                query += @" AND ProductType IN ( 
                                SELECT StringKey FROM profitwisereportfilter
                                WHERE PwReportId = @pwReportId AND FilterType = @productTypeFilter ) ";
            }
            if (filters.Count(x => x.FilterType == PwReportFilter.Vendor) > 0)
            {
                query += @" AND Vendor IN ( 
                                SELECT StringKey FROM profitwisereportfilter
                                WHERE PwReportId = @pwReportId AND FilterType = @vendorTypeFilter ) ";
            }

            query += " GROUP BY t1.PwMasterProductId, t1.Vendor, t1.Title;";

            return Connection.Query<MasterProductOption>(
                            query, new { PwShopId, @pwReportId, productTypeFilter, vendorTypeFilter }).ToList();
        }

        public IList<MasterVariantOption> RetrieveMasterVariantOptions(long pwReportId)
        {
            var query =
                @"SELECT t1.PwMasterProductId, t1.Vendor, t1.Title AS ProductTitle, 
                            t2.PwMasterVariantId, t3.Title AS VariantTitle, t3.Sku
                FROM profitwiseproduct t1 
	                INNER JOIN profitwisemastervariant t2
		                ON t1.PwMasterProductId = t2.PwMasterProductId AND t1.IsPrimary = 1 
	                INNER JOIN profitwisevariant t3
		                ON t2.PwMasterVariantId = t3.PwMasterVariantId AND t3.IsPrimary = 1
                WHERE t1.PwShopId = @PwShopId
                AND t2.PwShopId = @PwShopId
                AND t3.PwShopId = @PwShopId ";

            var filters = RetrieveFilters(pwReportId);
            var productTypeFilter = PwReportFilter.ProductType;
            var vendorTypeFilter = PwReportFilter.Vendor;
            var productFilter = PwReportFilter.MasterProduct;

            if (filters.Count(x => x.FilterType == PwReportFilter.ProductType) > 0)
            {
                query += @" AND ProductType IN ( 
                                SELECT StringKey FROM profitwisereportfilter
                                WHERE PwReportId = @pwReportId AND FilterType = @productTypeFilter ) ";
            }
            if (filters.Count(x => x.FilterType == PwReportFilter.Vendor) > 0)
            {
                query += @" AND Vendor IN ( 
                                SELECT StringKey FROM profitwisereportfilter
                                WHERE PwReportId = @pwReportId AND FilterType = @vendorTypeFilter ) ";
            }
            if (filters.Count(x => x.FilterType == PwReportFilter.MasterProduct) > 0)
            {
                query += @" AND t1.PwMasterProductId IN ( 
                                SELECT NumberKey FROM profitwisereportfilter
                                WHERE PwReportId = @pwReportId AND FilterType = @productFilter ) ";
            }

            query += " ORDER BY t1.Title, t3.Title, t3.Sku";
            return Connection
                .Query<MasterVariantOption>(query,
                            new { PwShopId, @pwReportId, productTypeFilter, vendorTypeFilter, productFilter })
                .ToList();
        }

        public IList<MasterProductOption> RetrieveProductOptions(long pwReportId)
        {
            var query =
                @"SELECT t1.PwProductId, t1.Vendor, t1.Title, COUNT(*) AS VariantCount
                FROM profitwiseproduct t1 
	                INNER JOIN profitwisevariant t2 ON t1.PwProductId = t2.PwProductId
                    INNER JOIN profitwisemastervariant t3 
                        ON t2.PwMasterVariantId = t3.PwMasterVariantId
                WHERE t1.PwShopId = @PwShopId
                AND t2.PwShopId = @PwShopId 
                AND t3.PwShopID = @PwShopId
                AND t1.IsActive = 1
                AND t2.IsActive = 1 
                AND t2.Inventory IS NOT NULL
                AND t3.StockedDirectly = 1";

            var filters = RetrieveFilters(pwReportId);
            var productTypeFilter = PwReportFilter.ProductType;
            var vendorTypeFilter = PwReportFilter.Vendor;

            if (filters.Count(x => x.FilterType == PwReportFilter.ProductType) > 0)
            {
                query += @" AND ProductType IN ( 
                                SELECT StringKey FROM profitwisereportfilter
                                WHERE PwReportId = @pwReportId AND FilterType = @productTypeFilter ) ";
            }
            if (filters.Count(x => x.FilterType == PwReportFilter.Vendor) > 0)
            {
                query += @" AND Vendor IN ( 
                                SELECT StringKey FROM profitwisereportfilter
                                WHERE PwReportId = @pwReportId AND FilterType = @vendorTypeFilter ) ";
            }

            query += " GROUP BY t1.PwProductId, t1.Vendor, t1.Title;";

            return Connection.Query<MasterProductOption>(
                            query, new { PwShopId, @pwReportId, productTypeFilter, vendorTypeFilter }).ToList();
        }

        public IList<MasterVariantOption> RetrieveVariantOptions(long pwReportId)
        {
            var query =
                @"SELECT t1.PwProductId, t2.PwVariantId, t1.Vendor, t1.Title AS ProductTitle, 
                        t2.Title AS VariantTitle, t2.Sku
                FROM profitwiseproduct t1 
	                INNER JOIN profitwisevariant t2 ON t1.PwProductId = t2.PwProductId
                    INNER JOIN profitwisemastervariant t3 
                        ON t2.PwMasterVariantId = t3.PwMasterVariantId
                WHERE t1.PwShopId = @PwShopId
                AND t2.PwShopId = @PwShopId 
                AND t3.PwShopID = @PwShopId
                AND t1.IsActive = 1
                AND t2.IsActive = 1 
                AND t2.Inventory IS NOT NULL
                AND t3.StockedDirectly = 1 ";
            
            var filters = RetrieveFilters(pwReportId);
            var productTypeFilter = PwReportFilter.ProductType;
            var vendorTypeFilter = PwReportFilter.Vendor;
            var productFilter = PwReportFilter.MasterProduct;

            if (filters.Count(x => x.FilterType == PwReportFilter.ProductType) > 0)
            {
                query += @" AND ProductType IN ( 
                                SELECT StringKey FROM profitwisereportfilter
                                WHERE PwReportId = @pwReportId AND FilterType = @productTypeFilter ) ";
            }
            if (filters.Count(x => x.FilterType == PwReportFilter.Vendor) > 0)
            {
                query += @" AND Vendor IN ( 
                                SELECT StringKey FROM profitwisereportfilter
                                WHERE PwReportId = @pwReportId AND FilterType = @vendorTypeFilter ) ";
            }
            if (filters.Count(x => x.FilterType == PwReportFilter.Product) > 0)
            {
                query += @" AND t1.PwProductId IN ( 
                                SELECT NumberKey FROM profitwisereportfilter
                                WHERE PwReportId = @pwReportId AND FilterType = @productFilter ) ";
            }

            query += " ORDER BY t1.Title, t2.Title, t2.Sku";
            return Connection
                .Query<MasterVariantOption>(query,
                            new { PwShopId, @pwReportId, productTypeFilter, vendorTypeFilter, productFilter })
                .ToList();
        }



        // Report Filters
        public IList<PwReportFilter> RetrieveFilters(long reportId)
        {
            var query =
                @"SELECT * FROM profitwisereportfilter 
                WHERE PwShopId = @PwShopId AND PwReportId = @reportId
                ORDER BY DisplayOrder;";
            return Connection.Query<PwReportFilter>(query, new { PwShopId, reportId }, _connectionWrapper.Transaction).ToList();
        }

        public PwReportFilter RetrieveFilter(long reportId, long filterId)
        {
            var query =
                @"SELECT * FROM profitwisereportfilter 
                WHERE PwShopId = @PwShopId AND PwReportId = @reportId AND PwFilterId = @filterId";
            return Connection
                    .Query<PwReportFilter>(query, new { PwShopId, reportId, filterId })
                    .FirstOrDefault();
        }

        public int? RetrieveMaxFilterOrder(long reportId)
        {
            var query =
                @"SELECT MAX(DisplayOrder) FROM profitwisereportfilter 
                WHERE PwShopId = @PwShopId AND PwReportId = @reportId";
            return Connection
                    .Query<int?>(query, new { PwShopId, reportId })
                    .FirstOrDefault();
        }

        public int? RetrieveMaxFilterId(long reportId)
        {
            var query =
                @"SELECT MAX(PwFilterId) FROM profitwisereportfilter 
                WHERE PwShopId = @PwShopId AND PwReportId = @reportId";

            return Connection
                    .Query<int?>(query, new { PwShopId, reportId })
                    .FirstOrDefault();
        }

        public PwReportFilter InsertFilter(PwReportFilter filter)
        {
            var query =
                @"INSERT INTO profitwisereportfilter VALUES 
                ( @PwReportId, @PwShopId, @PwFilterId, @FilterType, @NumberKey, @StringKey, @Title, @Description, @DisplayOrder )";

            filter.PwFilterId = (RetrieveMaxFilterId(filter.PwReportId) ?? 0) + 1;
            filter.DisplayOrder = (RetrieveMaxFilterOrder(filter.PwReportId) ?? 0) + 1;

            Connection.Execute(query, filter);

            return RetrieveFilter(filter.PwReportId, filter.PwFilterId);
        }

        public void CloneFilters(long sourceReportId, long destinationReportId)
        {
            this.DeleteFilters(destinationReportId);
            var query =
                @"INSERT INTO profitwisereportfilter 
                SELECT @destinationReportId, @PwShopId, PwFilterId, FilterType, NumberKey, StringKey, Title, Description, DisplayOrder
                FROM profitwisereportfilter WHERE PwShopId = @PwShopId AND PwReportId = @sourceReportId";

            Connection.Execute(query, new { PwShopId, sourceReportId, destinationReportId });
        }

        public void DeleteFilter(long reportId, int filterType, string key)
        {
            var query = @"DELETE FROM profitwisereportfilter 
                        WHERE PwShopId = @PwShopId 
                        AND PwReportId = @reportId 
                        AND FilterType = @filterType";

            if (PwReportFilter.FilterTypeUsesNumberKey(filterType))
            {
                query += " AND NumberKey = @key";
            }
            else
            {
                query += " AND StringKey = @key";
            }

            Connection.Execute(query,
                 new { reportId, this.PwShopId, filterType, key });
        }

        public void DeleteFilterById(long reportId, long filterId)
        {
            var query = @"DELETE FROM profitwisereportfilter 
                        WHERE PwShopId = @PwShopId 
                        AND PwReportId = @reportId 
                        AND PwFilterId = @filterId";

            Connection.Execute(query, new { reportId, this.PwShopId, filterId });
        }

        public void DeleteFilters(long reportId, int filterType)
        {
            var query = @"DELETE FROM profitwisereportfilter 
                        WHERE PwShopId = @PwShopId 
                        AND PwReportId = @reportId 
                        AND FilterType = @filterType";

            Connection.Execute(query, new { reportId, this.PwShopId, filterType });
        }

        public void DeleteFilters(long reportId)
        {
            var query = @"DELETE FROM profitwisereportfilter 
                        WHERE PwShopId = @PwShopId 
                        AND PwReportId = @reportId";

            Connection.Execute(query, new { reportId, this.PwShopId });
        }





        // Product & Variant counts and selection details
        public ProductAndVariantCount RetrieveReportRecordCount(long reportId)
        {
            var repository = _factory.MakeReportRepository(this.PwShop);
            var report = repository.RetrieveReport(reportId);
            var query = "";

            if (report.ReportTypeId == ReportType.Profitability)
            {
                query = @"SELECT COUNT(DISTINCT(PwMasterProductId)) AS ProductCount, 
                        COUNT(DISTINCT(PwMasterVariantId)) AS VariantCount
                        FROM vw_masterproductandvariantsearch 
                        WHERE PwShopId = @PwShopId ";
                query += ReportFilterClauseGenerator(reportId);
            }
            if (report.ReportTypeId == ReportType.GoodsOnHand)
            {
                query = @"SELECT COUNT(DISTINCT(PwProductId)) AS ProductCount, 
                        COUNT(DISTINCT(PwVariantId)) AS VariantCount
                        FROM [vw_goodsonhand] 
                        WHERE PwShopId = @PwShopId ";
                query += ReportFilterClauseGenerator(reportId);
            }

            return Connection
                .Query<ProductAndVariantCount>(query, new { PwShopId, PwReportId = reportId })
                .FirstOrDefault();
        }
        
        public List<ReportSelectionMasterProduct> RetrieveProductSelections(long reportId, int pageNumber, int pageSize)
        {
            var repository = _factory.MakeReportRepository(this.PwShop);
            var report = repository.RetrieveReport(reportId);
            var query = "";

            if (report.ReportTypeId == ReportType.Profitability)
            {
                query = @"SELECT PwMasterProductId, ProductTitle AS Title, Vendor, ProductType
                        FROM vw_masterproductandvariantsearch WHERE PwShopId = @PwShopId ";
                query += ReportFilterClauseGenerator(reportId);
                query += @" GROUP BY PwMasterProductId, ProductTitle, Vendor, ProductType 
                        ORDER BY ProductTitle OFFSET @startRecord ROWS FETCH NEXT @pageSize ROWS ONLY;";
            }
            if (report.ReportTypeId == ReportType.GoodsOnHand)
            {
                query = @"SELECT PwProductId, ProductTitle AS Title, Vendor, ProductType
                        FROM [vw_goodsonhand] WHERE PwShopId = @PwShopId ";
                query += ReportFilterClauseGenerator(reportId);
                query += @" GROUP BY PwProductId, ProductTitle, Vendor, ProductType 
                        ORDER BY ProductTitle OFFSET @startRecord ROWS FETCH NEXT @pageSize ROWS ONLY;";
            }

            var startRecord = (pageNumber - 1) * pageSize;

            return Connection
                .Query<ReportSelectionMasterProduct>(query, new { PwShopId, PwReportId = reportId, startRecord, pageSize, })
                .ToList();
        }
        
        public List<ReportSelectionMasterVariant> RetrieveVariantSelections(long reportId, int pageNumber, int pageSize)
        {
            var repository = _factory.MakeReportRepository(this.PwShop);
            var report = repository.RetrieveReport(reportId);
            var query = "";

            if (report.ReportTypeId == ReportType.Profitability)
            {
                query = @"SELECT PwMasterProductId, ProductTitle, PwMasterVariantId, VariantTitle, Sku, Vendor
                        FROM vw_masterproductandvariantsearch WHERE PwShopId = @PwShopId ";
                query += ReportFilterClauseGenerator(reportId);
                query += @" ORDER BY ProductTitle, VariantTitle OFFSET @startRecord ROWS FETCH NEXT @pageSize ROWS ONLY;";
            }

            if (report.ReportTypeId == ReportType.GoodsOnHand)
            {
                query = @"SELECT PwProductId, ProductTitle, PwVariantId, VariantTitle, Sku, Vendor
                        FROM [vw_goodsonhand] WHERE PwShopId = @PwShopId ";
                query += ReportFilterClauseGenerator(reportId);
                query += @" ORDER BY ProductTitle, VariantTitle OFFSET @startRecord ROWS FETCH NEXT @pageSize ROWS ONLY;";
            }

            var startRecord = (pageNumber - 1) * pageSize;
            return Connection
                .Query<ReportSelectionMasterVariant>(query, new { PwShopId, PwReportId = reportId, startRecord, pageSize })
                .ToList();
        }


        // TODO - refactor this to stop repeating the same clauses?
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
            if (filters.Count(x => x.FilterType == PwReportFilter.MasterProduct) > 0)
            {
                query += $@"AND PwMasterProductId IN ( SELECT NumberKey FROM profitwisereportfilter 
                            WHERE PwReportId = @PwReportId AND FilterType = {PwReportFilter.MasterProduct} ) ";
            }
            if (filters.Count(x => x.FilterType == PwReportFilter.MasterVariant) > 0)
            {
                query += $@"AND PwMasterVariantId IN ( SELECT NumberKey FROM profitwisereportfilter 
                            WHERE PwReportId = @PwReportId AND FilterType = {PwReportFilter.MasterVariant} ) ";
            }
            if (filters.Count(x => x.FilterType == PwReportFilter.Product) > 0)
            {
                query += $@"AND PwProductId IN ( SELECT NumberKey FROM profitwisereportfilter 
                            WHERE PwReportId = @PwReportId AND FilterType = {PwReportFilter.Product} ) ";
            }
            if (filters.Count(x => x.FilterType == PwReportFilter.Variant) > 0)
            {
                query += $@"AND PwVariantId IN ( SELECT NumberKey FROM profitwisereportfilter 
                            WHERE PwReportId = @PwReportId AND FilterType = {PwReportFilter.Variant} ) ";
            }
            return query;
        }

    }
}
