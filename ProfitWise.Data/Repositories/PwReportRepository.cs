using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Database;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.Preferences;
using ProfitWise.Data.Model.Reports;
using ProfitWise.Data.Model.Shop;
using Push.Foundation.Utilities.Helpers;

namespace ProfitWise.Data.Repositories
{
    [Intercept(typeof(ShopRequired))]
    public class PwReportRepository : IShopFilter
    {
        public PwShop PwShop { get; set; }
        public long PwShopId => PwShop.PwShopId;

        private readonly ConnectionWrapper _connectionWrapper;
        private IDbConnection Connection => _connectionWrapper.DbConn;


        public PwReportRepository(ConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.StartTransactionForScope();
        }
        

        public List<PwReport> RetrieveUserDefinedReports()
        {
            var query = 
                    @"SELECT * FROM profitwisereport 
                    WHERE PwShopId = @PwShopId 
                    AND CopyForEditing = 0;";
            var results = Connection.Query<PwReport>(query, new {PwShopId}).ToList();
            return results.OrderBy(x => x.Name).ToList();
        }

        public int RetrieveUserDefinedReportCount()
        {
            var query =
                    @"SELECT COUNT(*) FROM profitwisereport 
                    WHERE PwShopId = @PwShopId 
                    AND CopyForEditing = 0;";
            var results = Connection.Query<int>(query, new { PwShopId });
            return results.First();
        }

        public List<long> RetrieveSystemDefinedReportsIds()
        {
            return new List<long>
            {
                PwSystemReportFactory.OverallProfitabilityId,
                PwSystemReportFactory.TestReportId,
            };
        }

        public List<PwReport> RetrieveSystemDefinedReports()
        {
            var dateRange = 
                DateRangeDefaults
                    .Factory()
                    .FirstOrDefault(x => x.Id == this.PwShop.DateRangeDefault);
            
            List<PwReport> systemDefinedReports = new List<PwReport>()
            {
                PwSystemReportFactory.OverallProfitability(dateRange),
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
            return Connection
                    .Query<PwReport>(query, new { PwShopId, reportId })
                    .FirstOrDefault();
        }

        public bool ReportExists(long reportId)
        {
            if (RetrieveSystemDefinedReportsIds().Any(x => x == reportId))
            {
                return true;
            }

            var query = @"SELECT PwReportId FROM profitwisereport 
                        WHERE PwShopId = @PwShopId AND PwReportId = @reportId;";
            return Connection
                    .Query<PwReport>(query, new { PwShopId, reportId })
                    .Any();
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
            return Connection
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
                    @GroupingId, @OrderingId, getdate(), getdate() );
                
                SELECT SCOPE_IDENTITY();";

            return Connection.Query<long>(query, report).First();
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
                        LastAccessedDate = getdate()                     
                        WHERE PwShopId = @PwShopId AND PwReportId = @PwReportId;";
            Connection.Execute(query, report );
        }

        public void UpdateReportLastAccessed(long reportId)
        {
            var query = @"UPDATE profitwisereport SET 
                        LastAccessedDate = getdate()                     
                        WHERE PwShopId = @PwShopId AND PwReportId = @reportId;";

            Connection.Execute(query, new { this.PwShopId, reportId });
        }


        public void DeleteReport(long reportId)
        {
            var query = @"DELETE FROM profitwisereport            
                        WHERE PwShopId = @PwShopId 
                        AND PwReportId = @reportId;";
            Connection.Execute(query, new { PwShopId, reportId });
        }
    }
}

