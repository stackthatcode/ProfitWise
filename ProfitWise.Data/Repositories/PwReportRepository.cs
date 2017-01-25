using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.Reports;
using ProfitWise.Data.Model.Shop;

namespace ProfitWise.Data.Repositories
{
    [Intercept(typeof(ShopRequired))]
    public class PwReportRepository : IShopFilter
    {
        private readonly IDbConnection _connection;
        public PwShop PwShop { get; set; }
        public long PwShopId => PwShop.PwShopId;

        public PwReportRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connection.BeginTransaction();
        }
        

        public List<PwReport> RetrieveUserDefinedReports()
        {
            var query = 
                    @"SELECT * FROM profitwisereport 
                    WHERE PwShopId = @PwShopId 
                    AND CopyForEditing = 0;";
            var results = _connection.Query<PwReport>(query, new {PwShopId}).ToList();
            return results.OrderBy(x => x.Name).ToList();
        }

        public int RetrieveUserDefinedReportCount()
        {
            var query =
                    @"SELECT COUNT(*) FROM profitwisereport 
                    WHERE PwShopId = @PwShopId 
                    AND CopyForEditing = 0;";
            var results = _connection.Query<int>(query, new { PwShopId });
            return results.First();
        }


        public List<PwReport> RetrieveSystemDefinedReports()
        {
            List<PwReport> systemDefinedReports = new List<PwReport>()
            {
                PwSystemReportFactory.OverallProfitability(),
                PwSystemReportFactory.TestReport(),
            };

            systemDefinedReports.ForEach(x => x.PwShopId = this.PwShopId);
            return systemDefinedReports.OrderBy(x => x.Name).ToList();
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
                    PwShopId, Name, IsSystemReport, CopyForEditing, OriginalReportId, StartDate, EndDate, 
                    GroupingId, OrderingId, CreatedDate, LastAccessedDate ) 
                VALUES ( 
                    @PwShopId, @Name, @IsSystemReport, @CopyForEditing, @OriginalReportId, @StartDate, @EndDate,
                    @GroupingId, @OrderingId, NOW(), NOW() );
                
                SELECT LAST_INSERT_ID();";

            return _connection.Query<long>(query, report).First();
        }

        public void UpdateReport(PwReport report)
        {
            report.PwShopId = PwShopId;

            var query = @"UPDATE profitwisereport SET 
                        Name = @Name,
                        IsSystemReport = @IsSystemReport,
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
            var query = @"DELETE FROM profitwisereport            
                        WHERE PwShopId = @PwShopId 
                        AND PwReportId = @reportId;";
            _connection.Execute(query, new { PwShopId, reportId });
        }

    }
}

