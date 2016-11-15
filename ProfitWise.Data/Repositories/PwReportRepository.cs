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
                WHERE PwShopId = @PwShopId AND IsPrimary = 1 
                GROUP BY ProductType;";
            return _connection.Query<PwProductTypeSummary>(query, new {PwShopId}).ToList();
        }

        public IList<PwProductVendorSummary> RetrieveVendorSummary(long reportId)
        {
            var queryStart =
                @"SELECT Vendor, COUNT(*) AS Count 
                FROM profitwiseproduct 
                WHERE PwShopId = @PwShopId AND IsPrimary = 1";

            var queryMiddle = "";

            if (AreThereProductTypesFilters(reportId))
            {
                queryMiddle =
                    @"AND PwProductId IN ( 
                    SELECT PwProductId FROM vw_reportproducttypetoproduct 
                    WHERE PwShopId = @PwShopId AND PwReportId = @reportId ) ";
            }

            var queryEnd = "";

            var query = queryStart + queryMiddle + queryEnd;

            return _connection.Query<PwProductVendorSummary>(query, new { PwShopId, reportId }).ToList();
        }

        public IList<PwProductSummary> RetrieveMasterProductSummary(long reportId)
        {
            var queryHead =
                @"SELECT t1.PwMasterProductId, t1.Vendor, t1.Title, COUNT(*) AS Count
                FROM profitwiseproduct t1 
	                INNER JOIN profitwisemastervariant t2
		                ON t1.PwMasterProductId = t2.PwMasterProductId AND t1.IsPrimary = 1 
                WHERE t1.PwShopId = @PwShopId
                AND t2.PwShopId = @PwShopId ";

            var queryMiddle = "";

            if (AreThereProductTypesFilters(reportId))
            {
                queryMiddle +=
                    @"AND t1.PwProductId IN ( 
                    SELECT PwProductId FROM vw_reportproducttypetoproduct 
                    WHERE PwShopId = @PwShopId AND PwReportId = @reportId ) ";
            }

            if (AreThereVendorsFilters(reportId))
            {
                queryMiddle +=
                    @"AND t1.PwProductId IN ( 
                    SELECT PwProductId FROM vw_reportvendortoproduct
                    WHERE PwShopId = @PwShopId AND PwReportId = @reportId ) ";
            }

            var queryTails =
                @"GROUP BY t1.PwMasterProductId, t1.Vendor, t1.Title;";

            var query = queryHead + queryMiddle + queryTails;
            return _connection.Query<PwProductSummary>(query, new { PwShopId, reportId }).ToList();
        }
        public IList<PwProductSkuSummary> RetrieveSkuSummary(long reportId)
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
            return _connection.Query<PwProductSkuSummary>(query, new { PwShopId, reportId }).ToList();
        }

        public void UpdateSelectAllProductTypes(long reportId, bool value)
        {
            var query = @"UPDATE profitwisereport SET AllProductTypes = @value 
                        WHERE PwShopId = @PwShopId AND PwReportId = @reportId;";
            _connection.Execute(query, new { PwShopId, reportId, value });
        }

        public bool AreThereProductTypesFilters(long reportId)
        {
            var query = @"SELECT COUNT(ProductType) FROM profitwisereportproducttype
                        WHERE PwShopId = @PwShopId AND PwReportId = @reportId";
            return _connection.Query<int>(query, new {PwShopId, reportId}).First() > 0;
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



        public void UpdateSelectAllVendors(long reportId, bool value)
        {
            var query = @"UPDATE profitwisereport SET AllVendors = @value 
                        WHERE PwShopId = @PwShopId AND PwReportId = @reportId;";
            _connection.Execute(query, new { PwShopId, reportId, value });
        }

        public bool AreThereVendorsFilters(long reportId)
        {
            var query = @"SELECT COUNT(Vendor) FROM profitwisereportvendor
                        WHERE PwShopId = @PwShopId AND PwReportId = @reportId";
            return _connection.Query<int>(query, new {PwShopId, reportId}).First() > 0;
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

        public void ClearUnassociatedVendorMarks(long reportId)
        {
            if (!AreThereProductTypesFilters(reportId))
            {
                return;
            }

            var query =
                @"DELETE FROM profitwisereportvendor 
                WHERE PwShopId = @PwShopId 
                AND PwReportId = @reportId
                AND Vendor NOT IN (
	                SELECT DISTINCT Vendor FROM vw_reportproducttypetoproduct 
	                WHERE PwShopId = @PwShopId AND PwReportId = @reportId
                );";
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

        public void ClearUnassociatedMasterProductMarks(long reportId)
        {
            if (AreThereProductTypesFilters(reportId))
            {
                var query1 =
                    @"DELETE FROM profitwisereportmasterproduct 
                WHERE PwShopId = @PwShopId 
                AND PwReportId = @reportId
                AND PwMasterProductId NOT IN (
	                SELECT DISTINCT PwMasterProductId FROM vw_reportproducttypetoproduct 
	                WHERE PwShopId = @PwShopId AND PwReportId = @reportId
                );";
                _connection.Execute(query1, new {PwShopId, reportId});
            }

            if (AreThereVendorsFilters(reportId))
            {
                var query2 =
                    @"DELETE FROM profitwisereportmasterproduct 
                WHERE PwShopId = @PwShopId 
                AND PwReportId = @reportId
                AND PwMasterProductId NOT IN (
	                SELECT DISTINCT PwMasterProductId FROM vw_reportvendortoproduct 
	                WHERE PwShopId = @PwShopId AND PwReportId = @reportId
                );";
                _connection.Execute(query2, new {PwShopId, reportId});
            }
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



        
        public void UpdateSelectAllSkus(long reportId, bool value)
        {
            var query = @"UPDATE profitwisereport SET AllSkus = @value 
                        WHERE PwShopId = @PwShopId AND PwReportId = @reportId;";
            _connection.Execute(query, new { PwShopId, reportId, value });
        }

        public void ClearSkuMarks(long reportId)
        {
            var query = @"DELETE FROM profitwisereportsku 
                        WHERE PwShopId = @PwShopId AND PwReportId = @reportId;";
            _connection.Execute(query, new { PwShopId, reportId });
        }

        public void MarkSku(long reportId, long pwMasterVariantId)
        {
            var query = @"INSERT profitwisereportsku 
                        VALUES ( @reportId, @PwShopId, @pwMasterVariantId )";
            _connection.Execute(query, new { reportId, PwShopId, pwMasterVariantId });
        }

        public void UnmarkSku(long reportId, long pwMasterVariantId)
        {
            var query =
                @"DELETE FROM profitwisereportsku 
                WHERE PwShopId = @PwShopId 
                AND PwReportId = @reportId 
                AND PwMasterVariantId = @pwMasterVariantId";
            _connection.Execute(query, new { PwShopId, reportId, pwMasterVariantId });
        }

        public List<long> RetrieveMarkedSkus(long reportId)
        {
            var query = @"SELECT PwMasterVariantId FROM profitwisereportsku
                        WHERE PwShopId = @PwShopId AND PwReportId = @reportId";
            return _connection.Query<long>(query, new { PwShopId, reportId }).ToList();
        }


    }
}

