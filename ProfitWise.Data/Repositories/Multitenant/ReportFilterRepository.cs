using System.Collections.Generic;
using System.Data;
using System.Linq;
using ProfitWise.Data.Database;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model.Profit;
using ProfitWise.Data.Model.Reports;
using ProfitWise.Data.Model.Shop;

namespace ProfitWise.Data.Repositories.Multitenant
{
    public class ReportFilterRepository
    {
        private readonly ConnectionWrapper _connectionWrapper;
        private readonly MultitenantFactory _factory;
        public PwShop PwShop { get; set; }        
        public long PwShopId => PwShop.PwShopId;


        public ReportFilterRepository(ConnectionWrapper connectionWrapper, MultitenantFactory factory)
        {
            _connectionWrapper = connectionWrapper;
            _factory = factory;
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.InitiateTransaction();
        }


        // Product-Variant data for creating Filters
        public List<ProductTypeOption> RetrieveProductTypeOptions()
        {
            var query =
                @"SELECT ProductType, COUNT(*) AS Count
                FROM product(@PwShopId)
                WHERE IsPrimary = 1 
                GROUP BY ProductType;";
            return _connectionWrapper.Query<ProductTypeOption>(query, new { PwShopId }).ToList();
        }

        public IList<VendorOption> RetrieveVendorOptions(long pwReportId)
        {
            var query = @"SELECT Vendor, COUNT(*) AS Count FROM product(@PwShopId) 
                        WHERE IsPrimary = 1 ";

            var filters = RetrieveFilters(pwReportId);
            var productTypeFilter = PwReportFilter.ProductType;
            if (filters.Count(x => x.FilterType == PwReportFilter.ProductType) > 0)
            {
                query += @" AND ProductType IN ( 
                                SELECT StringKey FROM reportfilter(@PwShopId)
                                WHERE PwReportId = @pwReportId AND FilterType = @productTypeFilter ) ";
            }

            query += " GROUP BY Vendor;";

            return _connectionWrapper
                    .Query<VendorOption>(
                            query, new { PwShopId, @pwReportId, productTypeFilter }).ToList();
        }

        public IList<MasterProductOption> RetrieveMasterProductOptions(long pwReportId)
        {
            var query =
                @"SELECT t1.PwMasterProductId, t1.Vendor, t1.Title, COUNT(*) AS VariantCount
                FROM product(@PwShopId) t1 
	                INNER JOIN mastervariant(@PwShopId) t2
		                ON t1.PwMasterProductId = t2.PwMasterProductId AND t1.IsPrimary = 1 
                WHERE t2.PwShopId = @PwShopId ";

            var filters = RetrieveFilters(pwReportId);
            var productTypeFilter = PwReportFilter.ProductType;
            var vendorTypeFilter = PwReportFilter.Vendor;

            if (filters.Count(x => x.FilterType == PwReportFilter.ProductType) > 0)
            {
                query += @" AND ProductType IN ( 
                                SELECT StringKey FROM reportfilter(@PwShopId)
                                WHERE PwReportId = @pwReportId AND FilterType = @productTypeFilter ) ";
            }
            if (filters.Count(x => x.FilterType == PwReportFilter.Vendor) > 0)
            {
                query += @" AND Vendor IN ( 
                                SELECT StringKey FROM reportfilter(@PwShopId)
                                WHERE PwReportId = @pwReportId AND FilterType = @vendorTypeFilter ) ";
            }

            query += " GROUP BY t1.PwMasterProductId, t1.Vendor, t1.Title;";

            return _connectionWrapper.Query<MasterProductOption>(
                            query, new { PwShopId, @pwReportId, productTypeFilter, vendorTypeFilter }).ToList();
        }

        public IList<MasterVariantOption> RetrieveMasterVariantOptions(long pwReportId)
        {
            var query =
                @"SELECT t1.PwMasterProductId, t1.Vendor, t1.Title AS ProductTitle, 
                            t2.PwMasterVariantId, t3.Title AS VariantTitle, t3.Sku
                FROM product(@PwShopId) t1 
	                INNER JOIN mastervariant(@PwShopId) t2
		                ON t1.PwMasterProductId = t2.PwMasterProductId AND t1.IsPrimary = 1 
	                INNER JOIN variant(@PwShopId) t3
		                ON t2.PwMasterVariantId = t3.PwMasterVariantId AND t3.IsPrimary = 1
                WHERE t3.PwShopId = @PwShopId ";

            var filters = RetrieveFilters(pwReportId);
            var productTypeFilter = PwReportFilter.ProductType;
            var vendorTypeFilter = PwReportFilter.Vendor;
            var productFilter = PwReportFilter.MasterProduct;

            if (filters.Count(x => x.FilterType == PwReportFilter.ProductType) > 0)
            {
                query += @" AND ProductType IN ( 
                                SELECT StringKey FROM reportfilter(@PwShopId)
                                WHERE PwReportId = @pwReportId AND FilterType = @productTypeFilter ) ";
            }
            if (filters.Count(x => x.FilterType == PwReportFilter.Vendor) > 0)
            {
                query += @" AND Vendor IN ( 
                                SELECT StringKey FROM reportfilter(@PwShopId)
                                WHERE PwReportId = @pwReportId AND FilterType = @vendorTypeFilter ) ";
            }
            if (filters.Count(x => x.FilterType == PwReportFilter.MasterProduct) > 0)
            {
                query += @" AND t1.PwMasterProductId IN ( 
                                SELECT NumberKey FROM reportfilter(@PwShopId)
                                WHERE PwReportId = @pwReportId AND FilterType = @productFilter ) ";
            }

            query += " ORDER BY t1.Title, t3.Title, t3.Sku";
            return _connectionWrapper
                .Query<MasterVariantOption>(query,
                            new { PwShopId, @pwReportId, productTypeFilter, vendorTypeFilter, productFilter })
                .ToList();
        }

        public IList<MasterProductOption> RetrieveProductOptions(long pwReportId)
        {
            var query =
                @"SELECT t1.PwProductId, t1.Vendor, t1.Title, COUNT(*) AS VariantCount
                FROM product(@PwShopId) t1 
	                INNER JOIN variant(@PwShopId) t2 ON t1.PwProductId = t2.PwProductId
                    INNER JOIN mastervariant(@PwShopId) t3 ON t2.PwMasterVariantId = t3.PwMasterVariantId
                WHERE t1.IsActive = 1
                AND t2.IsActive = 1 
                AND t2.Inventory IS NOT NULL
                AND t3.StockedDirectly = 1";

            var filters = RetrieveFilters(pwReportId);
            var productTypeFilter = PwReportFilter.ProductType;
            var vendorTypeFilter = PwReportFilter.Vendor;

            if (filters.Count(x => x.FilterType == PwReportFilter.ProductType) > 0)
            {
                query += @" AND ProductType IN ( 
                                SELECT StringKey FROM reportfilter(@PwShopId)
                                WHERE PwReportId = @pwReportId AND FilterType = @productTypeFilter ) ";
            }
            if (filters.Count(x => x.FilterType == PwReportFilter.Vendor) > 0)
            {
                query += @" AND Vendor IN ( 
                                SELECT StringKey FROM reportfilter(@PwShopId)
                                WHERE PwReportId = @pwReportId AND FilterType = @vendorTypeFilter ) ";
            }

            query += " GROUP BY t1.PwProductId, t1.Vendor, t1.Title;";

            return _connectionWrapper.Query<MasterProductOption>(
                            query, new { PwShopId, @pwReportId, productTypeFilter, vendorTypeFilter }).ToList();
        }

        public IList<MasterVariantOption> RetrieveVariantOptions(long pwReportId)
        {
            var query =
                @"SELECT t1.PwProductId, t2.PwVariantId, t1.Vendor, t1.Title AS ProductTitle, 
                        t2.Title AS VariantTitle, t2.Sku
                FROM product(@PwShopId) t1 
	                INNER JOIN variant(@PwShopId) t2 ON t1.PwProductId = t2.PwProductId
                    INNER JOIN mastervariant(@PwShopId) t3 ON t2.PwMasterVariantId = t3.PwMasterVariantId
                WHERE t1.IsActive = 1
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
                                SELECT StringKey FROM reportfilter(@PwShopId)
                                WHERE PwReportId = @pwReportId AND FilterType = @productTypeFilter ) ";
            }
            if (filters.Count(x => x.FilterType == PwReportFilter.Vendor) > 0)
            {
                query += @" AND Vendor IN ( 
                                SELECT StringKey FROM reportfilter(@PwShopId)
                                WHERE PwReportId = @pwReportId AND FilterType = @vendorTypeFilter ) ";
            }
            if (filters.Count(x => x.FilterType == PwReportFilter.Product) > 0)
            {
                query += @" AND t1.PwProductId IN ( 
                                SELECT NumberKey FROM reportfilter(@PwShopId)
                                WHERE PwReportId = @pwReportId AND FilterType = @productFilter ) ";
            }

            query += " ORDER BY t1.Title, t2.Title, t2.Sku";
            return _connectionWrapper
                .Query<MasterVariantOption>(query,
                            new { PwShopId, @pwReportId, productTypeFilter, vendorTypeFilter, productFilter })
                .ToList();
        }



        // Report Filters
        public IList<PwReportFilter> RetrieveFilters(long reportId)
        {
            var query = @"SELECT * FROM reportfilter(@PwShopId) 
                        WHERE PwReportId = @reportId ORDER BY DisplayOrder;";
            return _connectionWrapper.Query<PwReportFilter>(query, new { PwShopId, reportId }).ToList();
        }

        public PwReportFilter RetrieveFilter(long reportId, long filterId)
        {
            var query =
                @"SELECT * FROM reportfilter(@PwShopId) 
                WHERE PwReportId = @reportId AND PwFilterId = @filterId";
            return _connectionWrapper
                    .Query<PwReportFilter>(query, new { PwShopId, reportId, filterId })
                    .FirstOrDefault();
        }

        public int? RetrieveMaxFilterOrder(long reportId)
        {
            var query =
                @"SELECT MAX(DisplayOrder) FROM reportfilter(@PwShopId) WHERE PwReportId = @reportId";
            return _connectionWrapper
                    .Query<int?>(query, new { PwShopId, reportId })
                    .FirstOrDefault();
        }

        public int? RetrieveMaxFilterId(long reportId)
        {
            var query =
                @"SELECT MAX(PwFilterId) FROM reportfilter(@PwShopId) WHERE PwReportId = @reportId";

            return _connectionWrapper
                    .Query<int?>(query, new { PwShopId, reportId })
                    .FirstOrDefault();
        }

        public PwReportFilter InsertFilter(PwReportFilter filter)
        {
            var query =
                @"INSERT INTO reportfilter(@PwShopId) VALUES 
                ( @PwReportId, @PwShopId, @PwFilterId, @FilterType, @NumberKey, @StringKey, @Title, @Description, @DisplayOrder )";

            filter.PwFilterId = (RetrieveMaxFilterId(filter.PwReportId) ?? 0) + 1;
            filter.DisplayOrder = (RetrieveMaxFilterOrder(filter.PwReportId) ?? 0) + 1;

            _connectionWrapper.Execute(query, filter);

            return RetrieveFilter(filter.PwReportId, filter.PwFilterId);
        }

        public void CloneFilters(long sourceReportId, long destinationReportId)
        {
            this.DeleteFilters(destinationReportId);
            var query =
                @"INSERT INTO reportfilter(@PwShopId) 
                SELECT @destinationReportId, @PwShopId, PwFilterId, FilterType, NumberKey, StringKey, Title, Description, DisplayOrder
                FROM reportfilter(@PwShopId) WHERE PwReportId = @sourceReportId";

            _connectionWrapper.Execute(query, new { PwShopId, sourceReportId, destinationReportId });
        }

        public void DeleteFilter(long reportId, int filterType, string key)
        {
            var query = @"DELETE FROM reportfilter(@PwShopId) 
                        WHERE PwReportId = @reportId AND FilterType = @filterType";

            if (PwReportFilter.FilterTypeUsesNumberKey(filterType))
            {
                query += " AND NumberKey = @key";
            }
            else
            {
                query += " AND StringKey = @key";
            }

            _connectionWrapper.Execute(query,
                 new { reportId, this.PwShopId, filterType, key });
        }

        public void DeleteFilterById(long reportId, long filterId)
        {
            var query = @"DELETE FROM reportfilter(@PwShopId) 
                        WHERE PwReportId = @reportId AND PwFilterId = @filterId";
            _connectionWrapper.Execute(query, new { reportId, this.PwShopId, filterId });
        }

        public void DeleteFilters(long reportId, int filterType)
        {
            var query = @"DELETE FROM reportfilter(@PwShopId) 
                        WHERE PwReportId = @reportId AND FilterType = @filterType";

            _connectionWrapper.Execute(query, new { reportId, this.PwShopId, filterType });
        }

        public void DeleteFilters(long reportId)
        {
            var query = @"DELETE FROM reportfilter(@PwShopId) WHERE PwReportId = @reportId";
            _connectionWrapper.Execute(query, new { reportId, this.PwShopId });
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
                        FROM mtv_masterproductandvariantsearch(@PwShopId) 
                        WHERE PwShopId = @PwShopId ";
                query += ReportFilterClauseGenerator(reportId);
            }
            if (report.ReportTypeId == ReportType.GoodsOnHand)
            {
                query = @"SELECT COUNT(DISTINCT(PwProductId)) AS ProductCount, 
                        COUNT(DISTINCT(PwVariantId)) AS VariantCount
                        FROM mtv_goodsonhand(@PwShopId) 
                        WHERE PwShopId = @PwShopId ";
                query += ReportFilterClauseGenerator(reportId);
            }

            return _connectionWrapper
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
                        FROM mtv_masterproductandvariantsearch(@PwShopId) WHERE PwShopId = @PwShopId ";
                query += ReportFilterClauseGenerator(reportId);
                query += @" GROUP BY PwMasterProductId, ProductTitle, Vendor, ProductType 
                        ORDER BY ProductTitle OFFSET @startRecord ROWS FETCH NEXT @pageSize ROWS ONLY;";
            }
            if (report.ReportTypeId == ReportType.GoodsOnHand)
            {
                query = @"SELECT PwProductId, ProductTitle AS Title, Vendor, ProductType
                        FROM mtv_goodsonhand(@PwShopId) WHERE PwShopId = @PwShopId ";
                query += ReportFilterClauseGenerator(reportId);
                query += @" GROUP BY PwProductId, ProductTitle, Vendor, ProductType 
                        ORDER BY ProductTitle OFFSET @startRecord ROWS FETCH NEXT @pageSize ROWS ONLY;";
            }

            var startRecord = (pageNumber - 1) * pageSize;

            return _connectionWrapper
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
                        FROM mtv_masterproductandvariantsearch(@PwShopId) WHERE PwShopId = @PwShopId ";
                query += ReportFilterClauseGenerator(reportId);
                query += @" ORDER BY ProductTitle, VariantTitle OFFSET @startRecord ROWS FETCH NEXT @pageSize ROWS ONLY;";
            }

            if (report.ReportTypeId == ReportType.GoodsOnHand)
            {
                query = @"SELECT PwProductId, ProductTitle, PwVariantId, VariantTitle, Sku, Vendor
                        FROM mtv_goodsonhand(@PwShopId) WHERE PwShopId = @PwShopId ";
                query += ReportFilterClauseGenerator(reportId);
                query += @" ORDER BY ProductTitle, VariantTitle OFFSET @startRecord ROWS FETCH NEXT @pageSize ROWS ONLY;";
            }

            var startRecord = (pageNumber - 1) * pageSize;
            return _connectionWrapper
                .Query<ReportSelectionMasterVariant>(query, new { PwShopId, PwReportId = reportId, startRecord, pageSize })
                .ToList();
        }


        // TODO - refactor this to stop repeating the same clauses?
        public string ReportFilterClauseGenerator(long reportId)
        {
            var filterRepository = _factory.MakeReportFilterRepository(this.PwShop);
            var filters = filterRepository.RetrieveFilters(reportId);
            return ReportFilterClauseGenerator(filters);
        }

        public string ReportFilterClauseGenerator(IList<PwReportFilter> filters)
        {
            var query = "";
            if (filters.Count(x => x.FilterType == PwReportFilter.ProductType) > 0)
            {
                query += $@"AND ProductType IN ( SELECT StringKey FROM reportfilter(@PwShopId) 
                            WHERE PwReportId = @PwReportId AND FilterType = {PwReportFilter.ProductType} ) ";
            }
            if (filters.Count(x => x.FilterType == PwReportFilter.Vendor) > 0)
            {
                query += $@"AND Vendor IN ( SELECT StringKey FROM reportfilter(@PwShopId) 
                            WHERE PwReportId = @PwReportId AND FilterType = {PwReportFilter.Vendor} ) ";
            }
            if (filters.Count(x => x.FilterType == PwReportFilter.MasterProduct) > 0)
            {
                query += $@"AND PwMasterProductId IN ( SELECT NumberKey FROM reportfilter(@PwShopId) 
                            WHERE PwReportId = @PwReportId AND FilterType = {PwReportFilter.MasterProduct} ) ";
            }
            if (filters.Count(x => x.FilterType == PwReportFilter.MasterVariant) > 0)
            {
                query += $@"AND PwMasterVariantId IN ( SELECT NumberKey FROM reportfilter(@PwShopId) 
                            WHERE PwReportId = @PwReportId AND FilterType = {PwReportFilter.MasterVariant} ) ";
            }
            if (filters.Count(x => x.FilterType == PwReportFilter.Product) > 0)
            {
                query += $@"AND PwProductId IN ( SELECT NumberKey FROM reportfilter(@PwShopId) 
                            WHERE PwReportId = @PwReportId AND FilterType = {PwReportFilter.Product} ) ";
            }
            if (filters.Count(x => x.FilterType == PwReportFilter.Variant) > 0)
            {
                query += $@"AND PwVariantId IN ( SELECT NumberKey FROM reportfilter(@PwShopId) 
                            WHERE PwReportId = @PwReportId AND FilterType = {PwReportFilter.Variant} ) ";
            }
            return query;
        }

    }
}
