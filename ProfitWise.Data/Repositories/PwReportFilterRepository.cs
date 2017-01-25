using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.Shop;

namespace ProfitWise.Data.Repositories
{
    public class PwReportFilterRepository
    {
        private readonly IDbConnection _connection;
        public PwShop PwShop { get; set; }
        public long PwShopId => PwShop.PwShopId;

        public PwReportFilterRepository(IDbConnection connection)
        {
            _connection = connection;
        }



        // Product-Variant reference data for creating Filters
        public List<PwProductTypeSummary> RetrieveProductTypeSummary()
        {
            var query =
                @"SELECT ProductType, COUNT(*) AS Count
                FROM profitwiseproduct
                WHERE PwShopId = @PwShopId AND IsPrimary = 1 
                GROUP BY ProductType;";
            return _connection.Query<PwProductTypeSummary>(query, new { PwShopId }).ToList();
        }

        public IList<PwProductVendorSummary> RetrieveVendorSummary(long pwReportId)
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

            return _connection
                    .Query<PwProductVendorSummary>(
                            query, new { PwShopId, @pwReportId, productTypeFilter }).ToList();
        }

        public IList<PwProductSummary> RetrieveMasterProductSummary(long pwReportId)
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

            return _connection.Query<PwProductSummary>(
                            query, new { PwShopId, @pwReportId, productTypeFilter, vendorTypeFilter }).ToList();
        }

        public IList<PwProductSkuSummary> RetrieveSkuSummary(long pwReportId)
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
            var productFilter = PwReportFilter.Product;

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
                query += @" AND t1.PwMasterProductId IN ( 
                                SELECT NumberKey FROM profitwisereportfilter
                                WHERE PwReportId = @pwReportId AND FilterType = @productFilter ) ";
            }

            query += " ORDER BY t1.Title, t3.Title, t3.Sku";
            return _connection
                .Query<PwProductSkuSummary>(query,
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
            return _connection.Query<PwReportFilter>(query, new { PwShopId, reportId }).ToList();
        }

        public PwReportFilter RetrieveFilter(long reportId, long filterId)
        {
            var query =
                @"SELECT * FROM profitwisereportfilter 
                WHERE PwShopId = @PwShopId AND PwReportId = @reportId AND PwFilterId = @filterId";
            return _connection
                    .Query<PwReportFilter>(query, new { PwShopId, reportId, filterId })
                    .FirstOrDefault();
        }

        public int? RetrieveMaxFilterOrder(long reportId)
        {
            var query =
                @"SELECT MAX(DisplayOrder) FROM profitwisereportfilter 
                WHERE PwShopId = @PwShopId AND PwReportId = @reportId";
            return _connection
                    .Query<int?>(query, new { PwShopId, reportId })
                    .FirstOrDefault();
        }

        public int? RetrieveMaxFilterId(long reportId)
        {
            var query =
                @"SELECT MAX(PwFilterId) FROM profitwisereportfilter 
                WHERE PwShopId = @PwShopId AND PwReportId = @reportId";

            return _connection
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

            _connection.Execute(query, filter);

            return RetrieveFilter(filter.PwReportId, filter.PwFilterId);
        }

        public void CloneFilters(long sourceReportId, long destinationReportId)
        {
            this.DeleteFilters(destinationReportId);
            var query =
                @"INSERT INTO profitwisereportfilter 
                SELECT @destinationReportId, @PwShopId, PwFilterId, FilterType, NumberKey, StringKey, Title, Description, DisplayOrder
                FROM profitwisereportfilter WHERE PwShopId = @PwShopId AND PwReportId = @sourceReportId";

            _connection.Execute(query, new { PwShopId, sourceReportId, destinationReportId });
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

            _connection.Execute(query,
                 new { reportId, this.PwShopId, filterType, key });
        }

        public void DeleteFilterById(long reportId, long filterId)
        {
            var query = @"DELETE FROM profitwisereportfilter 
                        WHERE PwShopId = @PwShopId 
                        AND PwReportId = @reportId 
                        AND PwFilterId = @filterId";

            _connection.Execute(query, new { reportId, this.PwShopId, filterId });
        }

        public void DeleteFilters(long reportId, int filterType)
        {
            var query = @"DELETE FROM profitwisereportfilter 
                        WHERE PwShopId = @PwShopId 
                        AND PwReportId = @reportId 
                        AND FilterType = @filterType";

            _connection.Execute(query, new { reportId, this.PwShopId, filterType });
        }

        public void DeleteFilters(long reportId)
        {
            var query = @"DELETE FROM profitwisereportfilter 
                        WHERE PwShopId = @PwShopId 
                        AND PwReportId = @reportId";

            _connection.Execute(query, new { reportId, this.PwShopId });
        }

    }
}
