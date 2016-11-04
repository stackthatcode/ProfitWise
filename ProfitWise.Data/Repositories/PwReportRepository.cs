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

        public void DeleteReport(long reportId)
        {
            var query = @"DELETE FROM profitwisereport WHERE PwShopId = @PwShopId AND PwReportId = @reportId;
                          DELETE FROM profitwisereportproducttype WHERE PwShopId = @PwShopId AND PwReportId = @reportId;
                          DELETE FROM profitwisereportvendor WHERE PwShopId = @PwShopId AND PwReportId = @reportId;
                          DELETE FROM profitwisereportmasterproduct WHERE PwShopId = @PwShopId AND PwReportId = @reportId;
                          DELETE FROM profitwisereportsku WHERE PwShopId = @PwShopId AND PwReportId = @reportId;";
            _connection.Execute(query, new {PwShopId, reportId});
        }

        public long CopyReport(PwReport report)
        {
            var query = @"INSERT INTO profitwisereport (
                            PwShopId, Name, Saved, AllProductTypes, AllVendors, AllProducts, AllSkus, 
                            Grouping, Ordering, CreatedDate, LastAccessedDate ) 
                        VALUES ( 
                            @PwShopId, @Name, 0, @AllProductTypes, @AllVendors, @AllProducts, @AllSkus, 
                            @Grouping, @Ordering, NOW(), NOW() );
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
                WHERE PwShopId = @PwShopId
                AND IsPrimary = 1 
                GROUP BY ProductType;";
            return _connection.Query<PwProductTypeSummary>(query, new {PwShopId}).ToList();
        }

        public void UpdateSelectAllProductTypes(long reportId, bool value)
        {
            var query = @"UPDATE profitwisereport SET AllProductTypes = @value 
                        WHERE PwShopId = @PwShopId AND PwReportId = @reportId;";
            _connection.Execute(query, new { PwShopId, reportId, value });
        }

        public List<string> RetrieveMarkedProductTypes(long reportId)
        {
            var query = @"SELECT ProductType FROM profitwisereportproducttype
                        WHERE PwShopId = @PwShopId AND PwReportId = @reportId";
            return _connection.Query<string>(query, new { PwShopId, reportId }).ToList();
        }

        public void ClearProductTypeMarks(long reportId)
        {
            var query = @"DELETE FROM profitwisereportproducttype 
                        WHERE PwShopId = @PwShopId AND PwReportId = @reportId;";
            _connection.Execute(query, new { PwShopId, reportId });
        }        

        public void MarkProductType(long reportId, string productType)
        {
            var query = @"INSERT profitwisereportproducttype VALUES ( @reportId, @PwShopId , @productType )";
            _connection.Execute(query, new { reportId, PwShopId, productType });
        }

        public void UnmarkProductType(long reportId, string productType)
        {
            var query = @"DELETE FROM profitwisereportproducttype 
                        WHERE PwShopId = @PwShopId 
                        AND PwReportId = @reportId 
                        AND ProductType = @productType";
            _connection.Execute(query, new { PwShopId, reportId, productType });
        }



        public IList<PwProductVendorSummary> RetrieveVendorSummary(long reportId)
        {
            var query =
                @"SELECT Vendor, COUNT(*) AS Count
                FROM profitwiseproduct
                WHERE PwShopId = @PwShopId
                AND IsPrimary = 1 
                GROUP BY Vendor;";
            return _connection.Query<PwProductVendorSummary>(query, new {PwShopId, reportId}).ToList();
        }

        public void UpdateSelectAllVendors(long reportId, bool value)
        {
            var query = @"UPDATE profitwisereport SET AllVendors = @value 
                        WHERE PwShopId = @PwShopId AND PwReportId = @reportId;";
            _connection.Execute(query, new { PwShopId, reportId, value });
        }

        public List<string> RetrieveMarkedVendors(long reportId)
        {
            var query = @"SELECT Vendor FROM profitwisereportvendor
                        WHERE PwShopId = @PwShopId 
                        AND PwReportId = @reportId";
            return _connection.Query<string>(query, new { PwShopId, reportId }).ToList();
        }

        public void ClearVendorMarks(long reportId)
        {
            var query = @"DELETE FROM profitwisereportvendor 
                        WHERE PwShopId = @PwShopId AND PwReportId = @reportId;";
            _connection.Execute(query, new { PwShopId, reportId });
        }
        
        public void MarkVendor(long reportId, string vendor)
        {
            var query = @"INSERT profitwisereportvendor VALUES ( @reportId, @PwShopId, @vendor )";
            _connection.Execute(query, new { reportId, PwShopId, vendor });
        }

        public void UnmarkVendor(long reportId, string vendor)
        {
            var query = @"DELETE FROM profitwisereportvendor 
                        WHERE PwShopId = @PwShopId
                        AND PwReportId = @reportId
                        AND Vendor = @vendor;";
            _connection.Execute(query, new { PwShopId, reportId, vendor });
        }



        public IList<PwProductSummary> RetrieveMasterProductSummary(long reportId)
        {
            var query =
                @"SELECT t1.PwMasterProductId, t1.Title, COUNT(*) AS Count
                FROM profitwiseproduct t1 
	                INNER JOIN profitwisemastervariant t2
		                ON t1.PwMasterProductId = t2.PwMasterProductId AND t1.IsPrimary = 1 
	                INNER JOIN profitwisevariant t3
		                ON t2.PwMasterVariantId = t3.PwMasterVariantId AND t3.IsPrimary = 1
                WHERE t1.PwShopId = @PwShopId
                AND t2.PwShopId = @PwShopId
                AND t3.PwShopId = @PwShopId
                GROUP BY t1.PwMasterProductId, t1.Title;";
            return _connection.Query<PwProductSummary>(query, new { PwShopId, reportId }).ToList();
        }        

        public void UpdateSelectAllMasterProducts(long reportId, bool value)
        {
            var query = @"UPDATE profitwisereport SET AllProducts = @value 
                        WHERE PwShopId = @PwShopId AND PwReportId = @reportId;";
            _connection.Execute(query, new { PwShopId, reportId, value });
        }

        public List<long> RetrieveMarkedMasterProducts(long reportId)
        {
            var query = @"SELECT PwMasterProductId FROM profitwisereportmasterproduct
                        WHERE PwShopId = @PwShopId AND PwReportId = @reportId;";
            return _connection.Query<long>(query, new { PwShopId, reportId }).ToList();
        }

        public void ClearMasterProductMarks(long reportId)
        {
            var query = @"DELETE FROM profitwisereportmasterproduct 
                        WHERE PwShopId = @PwShopId AND PwReportId = @reportId;";
            _connection.Execute(query, new { PwShopId, reportId });
        }

        public void MarkMasterProduct(long reportId, long masterProduct)
        {
            var query = @"INSERT profitwisereportmasterproduct VALUES ( @reportId, @PwShopId, @masterProduct )";
            _connection.Execute(query, new { reportId, PwShopId, masterProduct });
        }

        public void UnmarkMasterProduct(long reportId, long masterProductId)
        {
            var query =
                @"DELETE FROM profitwisereportmasterproduct 
                WHERE PwShopId = @PwShopId 
                AND PwReportId = @reportId 
                AND PwMasterProductId = @masterProductId;";
            _connection.Execute(query, new { PwShopId, reportId, masterProductId });
        }



        public void SelectAllSkus(long reportId)
        {
            var query =
                @"UPDATE profitwisereportsku SET AllSkus = 1 WHERE PwShopId = @PwShopId AND PwReportId = @reportId;
                DELETE profitwisereportsku WHERE PwShopId = @PwShopId AND PwReportId = @reportId;";
            _connection.Execute(query, new { PwShopId, reportId });
        }

        public void DeselectAllSkus(long reportId)
        {
            var query =
                @"UPDATE profitwisereportsku SET AllSkus = 0 WHERE PwShopId = @PwShopId AND PwReportId = @reportId;
                DELETE profitwisereportsku WHERE PwShopId = @PwShopId AND PwReportId = @reportId;";
            _connection.Execute(query, new { PwShopId, reportId });
        }

        public void InsertSku(long reportId, string sku)
        {
            var query = @"INSERT profitwisereportsku VALUES ( @PwShopId, @reportId, @sku )";
            _connection.Execute(query, new { PwShopId, reportId, sku });
        }

        public void DeleteSku(long reportId, string sku)
        {
            var query =
                @"DELETE FROM profitwisereportsku 
                WHERE PwShopId = @PwShopId AND PwReportId = @reportId AND Sku = @sku";
            _connection.Execute(query, new { PwShopId, reportId, sku });
        }

        public List<string> RetrieveSkus(long reportId)
        {
            var query = @"SELECT Sku FROM profitwisereportsku
                        WHERE PwShopId = @PwShopId AND PwReportId = @reportId";
            return _connection.Query<string>(query, new { PwShopId, reportId }).ToList();
        }
    }
}

