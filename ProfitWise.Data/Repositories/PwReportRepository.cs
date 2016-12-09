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
        

        public List<PwReport> RetrieveUserDefinedReports()
        {
            var query = 
                    @"SELECT * FROM profitwisereport 
                    WHERE PwShopId = @PwShopId 
                    AND CopyOfSystemReport = 0
                    AND CopyForEditing = 0;";
            var results = _connection.Query<PwReport>(query, new {PwShopId}).ToList();
            return results;
        }


        
        public List<PwReport> RetrieveSystemDefinedReports()
        {
            List<PwReport>
            systemDefinedReports = new List<PwReport>()
            {
                PwSystemReportFactory.OverallProfitability(),
                PwSystemReportFactory.TestReport(),
            };
            systemDefinedReports.ForEach(x => x.PwShopId = this.PwShopId);
            return systemDefinedReports;
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

        public string RetrieveAvailableDefaultName()
        {
            var reports = RetrieveUserDefinedReports();
            var reportNumber = 1;
            var reportName = "";
            while (true)
            {
                reportName = PwSystemReportFactory.CustomDefaultNameBuilder(reportNumber);
                if (reports.Any(x => x.Name == reportName))
                {
                    reportNumber++;
                }
                else
                {
                    break;
                }
            }
            return reportName;
        }

        public bool ReportNameCollision(long reportId, string name)
        {
            if (RetrieveSystemDefinedReports().Any(x => x.Name == name))
            {
                return true;
            }

            var query = @"SELECT * FROM profitwisereport 
                        WHERE PwShopId = @PwShopId 
                        AND CopyForEditing = 0
                        AND Name = @name;";
            return _connection
                    .Query<PwReport>(query, new { PwShopId, reportId, name })
                    .Any();
        }


        public long InsertReport(PwReport report)
        {
            var query =
                @"INSERT INTO profitwisereport (
                    PwShopId, Name, CopyForEditing, CopyOfSystemReport, OriginalReportId, StartDate, EndDate, 
                    GroupingId, OrderingId, CreatedDate, LastAccessedDate ) 
                VALUES ( 
                    @PwShopId, @Name, @CopyForEditing, @CopyOfSystemReport, @OriginalReportId, @StartDate, @EndDate,
                    @GroupingId, @OrderingId, NOW(), NOW() );
                
                SELECT LAST_INSERT_ID();";

            return _connection.Query<long>(query, report).First();
        }

        public void UpdateReport(PwReport report)
        {
            report.PwShopId = PwShopId;

            var query = @"UPDATE profitwisereport 
                        SET Name = @Name,
                        CopyOfSystemReport = @CopyOfSystemReport,
                        CopyForEditing = @CopyForEditing,
                        OriginalReportId = @OriginalReportId,
                        StartDate = @StartDate,
                        EndDate = @EndDate,
                        GroupingId = @GroupingId,
                        OrderingId = @OrderingId,
                        LastAccessedDate = @LastAccessedDate                     
                        WHERE PwShopId = @PwShopId AND PwReportId = @PwReportId;";
            _connection.Execute(query, report );
        }

        public void DeleteReport(long reportId)
        {
            this.DeleteFilters(reportId);
            var query = @"DELETE FROM profitwisereport            
                        WHERE PwShopId = @PwShopId 
                        AND PwReportId = @reportId;";
            _connection.Execute(query, new { PwShopId, reportId });
        }


        // Store data for Report Filter selections
        public List<PwProductTypeSummary> RetrieveProductTypeSummary()
        {
            var query =
                @"SELECT ProductType, COUNT(*) AS Count
                FROM profitwiseproduct
                WHERE PwShopId = @PwShopId AND IsPrimary = 1 
                GROUP BY ProductType;";
            return _connection.Query<PwProductTypeSummary>(query, new {PwShopId}).ToList();
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
                .Query<PwReportRecordCount>(query, new {PwShopId, PwReportId = reportId})
                .FirstOrDefault();
        }

        public List<PwReportMasterProductSelection> RetrieveProductSelections(long reportId, int pageNumber, int pageSize)
        {
            var query =
                @"SELECT t1.PwShopId, t1.PwMasterProductId, t1.Title, t1.Vendor, t1.ProductType
                FROM profitwiseproduct t1 
	                INNER JOIN profitwisemastervariant t2 ON t1.PwMasterProductId = t2.PwMasterProductId
                    INNER JOIN profitwisevariant t3 ON t2.PwMasterVariantId = t3.PwMasterVariantId    
                WHERE t1.PwShopId = @PwShopId AND t2.PwShopId = @PwShopId AND t3.PwShopId = @PwShopId
                AND t1.IsPrimary = 1 AND t3.IsPrimary = 1 ";
            query += ReportFilterClauseGenerator(reportId);
            query += @" GROUP BY t1.PwMasterProductId, t1.Title, t1.Vendor, t1.ProductType 
                        ORDER BY t1.Title LIMIT @startRecord, @pageSize";

            var startRecord = (pageNumber - 1) * pageSize;

            return _connection
                .Query<PwReportMasterProductSelection>(query, new { PwShopId, PwReportId = reportId, startRecord, pageSize, })
                .ToList();
        }

        public List<PwReportMasterVariantSelection> RetrieveVariantSelections(long reportId, int pageNumber, int pageSize)
        {
            var query = 
                @"SELECT t1.PwShopId, t1.PwMasterProductId, t1.Title AS ProductTitle, 
                        t3.PwMasterVariantId, t3.Title AS VariantTitle, t3.Sku, t1.Vendor
                FROM profitwiseproduct t1 
	                INNER JOIN profitwisemastervariant t2 ON t1.PwMasterProductId = t2.PwMasterProductId
                    INNER JOIN profitwisevariant t3 ON t2.PwMasterVariantId = t3.PwMasterVariantId    
                WHERE t1.PwShopId = @PwShopId AND t2.PwShopId = @PwShopId AND t3.PwShopId = @PwShopId
                AND t1.IsPrimary = 1 AND t3.IsPrimary = 1 ";
            query += ReportFilterClauseGenerator(reportId);
            query += @" ORDER BY t1.Title, t3.Title LIMIT @startRecord, @pageSize";

            var startRecord = (pageNumber - 1) * pageSize;

            return _connection
                .Query<PwReportMasterVariantSelection>(query, new { PwShopId, PwReportId = reportId, startRecord, pageSize })
                .ToList();
        }

        public string ReportFilterClauseGenerator(long reportId)
        {
            var query = "";
            var filters = RetrieveFilters(reportId);
            if (filters.Count(x => x.FilterType == PwReportFilter.ProductType) > 0)
            {
                query += $@"AND t1.ProductType IN ( SELECT StringKey FROM profitwisereportfilter 
                            WHERE PwReportId = @PwReportId AND FilterType = {PwReportFilter.ProductType} ) ";
            }
            if (filters.Count(x => x.FilterType == PwReportFilter.Vendor) > 0)
            {
                query += $@"AND t1.Vendor IN ( SELECT StringKey FROM profitwisereportfilter 
                            WHERE PwReportId = @PwReportId AND FilterType = {PwReportFilter.Vendor} ) ";
            }
            if (filters.Count(x => x.FilterType == PwReportFilter.Product) > 0)
            {
                query += $@"AND t1.PwMasterProductId IN ( SELECT NumberKey FROM profitwisereportfilter 
                            WHERE PwReportId = @PwReportId AND FilterType = {PwReportFilter.Product} ) ";
            }
            if (filters.Count(x => x.FilterType == PwReportFilter.Sku) > 0)
            {
                query += $@"AND t3.PwMasterVariantId IN ( SELECT NumberKey FROM profitwisereportfilter 
                            WHERE PwReportId = @PwReportId AND FilterType = {PwReportFilter.Sku} ) ";
            }

            return query;
        }
    }
}

