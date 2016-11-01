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
            return new List<PwReport>()
            {
                PwSystemReportFactory.OverallProfitability(),
            };
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

        public long CopyReport(long reportId)
        {
            var query = @"INSERT INTO profitwisereport (
                            PwShopId, Name, Saved, AllProductTypes, AllVendors, AllProducts, AllSkus, 
                            Grouping, CreatedDate, LastAccessedDate ) 
                        SELECT PwShopId, Name, Saved, AllProductTypes, AllVendors, AllProducts, AllSkus, 
                            Grouping, CreatedDate, LastAccessedDate
                        FROM profitwisereport
                        WHERE PwShopId = @PwShopId AND PwReportId = @reportId;
                        SELECT LAST_INSERT_ID();";

            return _connection.Query(query, new {PwShopId, reportId}).First();
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



        public void SelectAllProductTypes(long reportId)
        {
            var query = 
                @"UPDATE profitwisereport SET AllProductTypes = 1 WHERE PwShopId = @PwShopId AND PwReportId = @reportId;
                DELETE profitwisereportproducttype WHERE PwShopId = @PwShopId AND PwReportId = @reportId;";
            _connection.Execute(query, new { PwShopId, reportId });
        }

        public void DeselectAllProductTypes(long reportId)
        {
            var query =
                @"UPDATE profitwisereport SET AllProductTypes = 0 WHERE PwShopId = @PwShopId AND PwReportId = @reportId;
                DELETE profitwisereportproducttype WHERE PwShopId = @PwShopId AND PwReportId = @reportId;";
            _connection.Execute(query, new { PwShopId, reportId });
        }

        public void InsertProductType(long reportId, string productType)
        {
            var query = @"INSERT profitwisereportproducttype VALUES ( @PwShopId, @reportId, @productType )";
            _connection.Execute(query, new { PwShopId, reportId, productType });
        }

        public void DeleteProductType(long reportId, string productType)
        {
            var query = @"DELETE FROM profitwisereportproducttype 
                        WHERE PwShopId = @PwShopId AND PwReportId = @reportId AND ProductType = @productType";
            _connection.Execute(query, new { PwShopId, reportId, productType });
        }

        public List<string> RetrieveProductTypes(long reportId)
        {
            var query = @"SELECT ProductType FROM profitwisereportproducttype
                        WHERE PwShopId = @PwShopId AND PwReportId = @reportId";
            return _connection.Query<string>(query, new { PwShopId, reportId }).ToList();
        }



        public void SelectAllVendors(long reportId)
        {
            var query =
                @"UPDATE profitwisereport SET AllVendors = 1 WHERE PwShopId = @PwShopId AND PwReportId = @reportId;
                DELETE profitwisereportvendor WHERE PwShopId = @PwShopId AND PwReportId = @reportId;";
            _connection.Execute(query, new { PwShopId, reportId });
        }

        public void DeselectAllVendors(long reportId)
        {
            var query =
                @"UPDATE profitwisereport SET AllVendors = 0 WHERE PwShopId = @PwShopId AND PwReportId = @reportId;
                DELETE profitwisereportvendor WHERE PwShopId = @PwShopId AND PwReportId = @reportId;";
            _connection.Execute(query, new { PwShopId, reportId });
        }

        public void InsertVendor(long reportId, string vendor)
        {
            var query = @"INSERT profitwisereportvendor VALUES ( @PwShopId, @reportId, @vendor )";
            _connection.Execute(query, new { PwShopId, reportId, vendor });
        }

        public void DeleteVendor(long reportId, string vendor)
        {
            var query = @"DELETE FROM profitwisereportvendor 
                        WHERE PwShopId = @PwShopId AND PwReportId = @reportId AND Vendor = @vendor";
            _connection.Execute(query, new { PwShopId, reportId, vendor });
        }

        public List<string> RetrieveVendors(long reportId)
        {
            var query = @"SELECT Vendor FROM profitwisereportvendor
                        WHERE PwShopId = @PwShopId AND PwReportId = @reportId";
            return _connection.Query<string>(query, new { PwShopId, reportId }).ToList();
        }



        public void SelectAllMasterProducts(long reportId)
        {
            var query =
                @"UPDATE profitwisereport SET AllProducts = 1 WHERE PwShopId = @PwShopId AND PwReportId = @reportId;
                DELETE profitwisereportmasterproduct WHERE PwShopId = @PwShopId AND PwReportId = @reportId;";
            _connection.Execute(query, new { PwShopId, reportId });
        }

        public void DeselectAllMasterProducts(long reportId)
        {
            var query =
                @"UPDATE profitwisereport SET AllProducts = 0 WHERE PwShopId = @PwShopId AND PwReportId = @reportId;
                DELETE profitwisereportmasterproduct WHERE PwShopId = @PwShopId AND PwReportId = @reportId;";
            _connection.Execute(query, new { PwShopId, reportId });
        }

        public void InsertMasterProduct(long reportId, long masterProduct)
        {
            var query = @"INSERT profitwisereportmasterproduct VALUES ( @PwShopId, @reportId, @masterProduct )";
            _connection.Execute(query, new { PwShopId, reportId, masterProduct });
        }

        public void DeleteMasterProduct(long reportId, long masterProductId)
        {
            var query =
                @"DELETE FROM profitwisereportmasterproduct 
                WHERE PwShopId = @PwShopId AND PwReportId = @reportId AND PwMasterProductId = @masterProductId";
            _connection.Execute(query, new { PwShopId, reportId, masterProductId });
        }

        public List<long> RetrieveMasterProducts(long reportId)
        {
            var query = @"SELECT PwMasterProductId FROM profitwisereportmasterproduct
                        WHERE PwShopId = @PwShopId AND PwReportId = @reportId";
            return _connection.Query<long>(query, new { PwShopId, reportId }).ToList();
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

