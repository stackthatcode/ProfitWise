using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Model;

namespace ProfitWise.Data.Repositories
{
    [Intercept(typeof(ShopRequired))]
    public class PwReportRepository : IShopFilter
    {
        private readonly MySqlConnection _connection;
        public PwShop PwShop { get; set; }
        public long PwShopId => PwShop.PwShopId;

        public PwReportRepository(MySqlConnection connection)
        {
            _connection = connection;
        }

        public MySqlTransaction InitiateTransaction()
        {
            return _connection.BeginTransaction();
        }

        public long InsertReport(PwReport report)
        {
            var query = @"INSERT INTO profitwisereport (
                            PwShopId, Name, Saved, AllProductTypes, AllVendors, AllProducts, AllSkus, 
                            Grouping, CreatedDate, LastAccessedDate ) 
                        VALUES (
                            @PwShopId, @Name, @Saved, @AllProductTypes, @AllVendors, @AllProducts, @AllSkus, 
                            @Grouping, @CreatedDate, @LastAccessedDate );
                        SELECT LAST_INSERT_ID();";

            report.PwShopId = this.PwShop.PwShopId;
            return _connection.Query<long>(query, report).FirstOrDefault();
        }

        public List<PwReport> RetrieveUserDefinedReports()
        {
            var query = "SELECT * FROM profitwisereport WHERE PwShopId = @PwShopId AND Saved = 1;";
            var results = _connection.Query<PwReport>(query, new {PwShopId}).ToList();
            results.ForEach(x => x.UserDefined = true);
            return results;
        }
        
        public List<PwReport> RetrieveSystemDefinedReports()
        {
            var results = new List<PwReport>()
            {
                PwSystemReportFactory.OverallProfitability(),
            };
            results.ForEach(x => x.PwShopId = this.PwShopId);
            return results;
        }


        public PwReport RetrieveReport(long reportId)
        {
            var systemReports = RetrieveSystemDefinedReports();
            var systemReport = systemReports.FirstOrDefault(x => x.PwReportId == reportId);
            if (systemReport != null)
            {
                return systemReport;
            }

            var query = @"SELECT * FROM profitwisereport 
                        WHERE PwShopId = @PwShopId AND PwReportId = @reportId;";
            return _connection
                    .Query<PwReport>(query, new { PwShopId, reportId })
                    .FirstOrDefault();
        }
        
        public long CopyReport(PwReport report)
        {
            var query = @"INSERT INTO profitwisereport (
                            PwShopId, Name, Saved, Grouping, Ordering, CreatedDate, LastAccessedDate ) 
                        VALUES ( 
                            @PwShopId, @Name, 0, @Grouping, @Ordering, NOW(), NOW() );
                        SELECT LAST_INSERT_ID();";

            return _connection.Query<long>(query, report).First();
        }

        public void UpdateReportSaved(long reportId, bool saved)
        {
            var query = @"UPDATE profitwisereport SET Saved = @saved 
                        WHERE PwShopId = @PwShopId AND PwReportId = @reportId;";
            _connection.Execute(query, new { PwShopId, reportId, saved });
        }

        public void UpdateLastAccessedDate(long reportId, DateTime lastAccessedDate)
        {
            var query = @"UPDATE profitwisereport SET LastAccessedDate = @lastAccessedDate 
                        WHERE PwShopId = @PwShopId AND PwReportId = @reportId;";
            _connection.Execute(query, new { PwShopId, reportId, lastAccessedDate });
        }

        public void UpdateReportName(long reportId, string reportName)
        {
            var query = @"UPDATE profitwisereport SET ReportName = @reportName 
                        WHERE PwShopId = @PwShopId AND PwReportId = @reportId;";
            _connection.Execute(query, new { PwShopId, reportId, reportName });
        }

        public void UpdateReportGrouping(long reportId, string grouping)
        {
            var query = @"UPDATE profitwisereport SET Grouping = @grouping 
                        WHERE PwShopId = @PwShopId AND PwReportId = @reportId;";
            _connection.Execute(query, new { PwShopId, reportId, grouping });
        }



        public List<PwProductTypeSummary> RetrieveProductTypeSummary()
        {
            var query =
                @"SELECT ProductType, COUNT(*) AS Count
                FROM profitwiseproduct
                WHERE PwShopId = @PwShopId AND IsPrimary = 1 
                GROUP BY ProductType;";
            return _connection.Query<PwProductTypeSummary>(query, new {PwShopId}).ToList();
        }

        public IList<PwProductVendorSummary> RetrieveVendorSummary()
        {
            var query =
                @"SELECT Vendor, COUNT(*) AS Count 
                FROM profitwiseproduct 
                WHERE PwShopId = @PwShopId AND IsPrimary = 1
                GROUP BY Vendor;";

            return _connection.Query<PwProductVendorSummary>(query, new { PwShopId, }).ToList();
        }

        public IList<PwProductSummary> RetrieveMasterProductSummary()
        {
            var query =
                @"SELECT t1.PwMasterProductId, t1.Vendor, t1.Title, COUNT(*) AS VariantCount
                FROM profitwiseproduct t1 
	                INNER JOIN profitwisemastervariant t2
		                ON t1.PwMasterProductId = t2.PwMasterProductId AND t1.IsPrimary = 1 
                WHERE t1.PwShopId = @PwShopId
                AND t2.PwShopId = @PwShopId
                GROUP BY t1.PwMasterProductId, t1.Vendor, t1.Title;";

            return _connection.Query<PwProductSummary>(query, new { PwShopId, }).ToList();
        }

        public IList<PwProductSkuSummary> RetrieveSkuSummary()
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
                AND t3.PwShopId = @PwShopId;";
            return _connection.Query<PwProductSkuSummary>(query, new { PwShopId, }).ToList();
        }



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
                    .Query<int?>(query, new {PwShopId, reportId})
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

        public void DeleteFilter(long reportId, string filterType, string key)
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

        public void DeleteFilters(long reportId, string filterType)
        {
            var query = @"DELETE FROM profitwisereportfilter 
                        WHERE PwShopId = @PwShopId 
                        AND PwReportId = @reportId 
                        AND FilterType = @filterType";

            _connection.Execute(query, new { reportId, this.PwShopId, filterType });
        }
    }
}

