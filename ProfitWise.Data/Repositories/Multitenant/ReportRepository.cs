using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Dapper;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Database;
using ProfitWise.Data.Model.Preferences;
using ProfitWise.Data.Model.Reports;
using ProfitWise.Data.Model.Shop;
using ProfitWise.Data.Services;

namespace ProfitWise.Data.Repositories.Multitenant
{
    [Intercept(typeof(ShopRequired))]
    public class ReportRepository : IShopFilter
    {
        public PwShop PwShop { get; set; }
        public long PwShopId => PwShop.PwShopId;
        private readonly ConnectionWrapper _connectionWrapper;
        private readonly TimeZoneTranslator _timeZoneTranslator;


        public ReportRepository(
                ConnectionWrapper connectionWrapper, 
                TimeZoneTranslator timeZoneTranslator)
        {
            _connectionWrapper = connectionWrapper;
            _timeZoneTranslator = timeZoneTranslator;
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.InitiateTransaction();
        }
        

        public List<PwReport> RetrieveUserDefinedReports(int? reportTypeId = null)
        {
            var query = @"SELECT * FROM report(@PwShopId) WHERE CopyForEditing = 0 ";

            if (reportTypeId.HasValue)
            {
                query += "AND ReportTypeId = @reportTypeId;";
            }

            var results = _connectionWrapper.Query<PwReport>(query, new { reportTypeId, PwShopId }).ToList();
            return results.OrderBy(x => x.Name).ToList();
        }

        public int RetrieveUserDefinedReportCount()
        {
            var query = @"SELECT COUNT(*) FROM report(@PwShopId) WHERE CopyForEditing = 0;";
            var results = _connectionWrapper.Query<int>(query, new { PwShopId });
            return results.First();
        }

        public List<long> RetrieveSystemDefinedReportsIds()
        {
            return new List<long>
            {
                SystemReportFactory.OverallProfitabilityId,
                SystemReportFactory.GoodsOnHandId,
            };
        }

        public List<PwReport> RetrieveSystemDefinedReports(int? reportTypeId = null)
        {
            var today = _timeZoneTranslator.Today(PwShop.TimeZone);
            var dateRange = DateRangeDefaults
                            .Factory(today)
                            .FirstOrDefault(x => x.Id == this.PwShop.DateRangeDefault);

            var systemDefinedReports = new List<PwReport>()
            {
                SystemReportFactory.OverallProfitability(dateRange), 
                SystemReportFactory.GoodsOnHandReport(today),
            };

            systemDefinedReports.ForEach(x => x.PwShopId = this.PwShopId);
            return systemDefinedReports
                        .Where(x => reportTypeId == null || x.ReportTypeId == reportTypeId)
                        .OrderBy(x => x.Name).ToList();
        }

        public PwReport RetrieveReport(long reportId)
        {
            var systemReports = RetrieveSystemDefinedReports();
            var systemReport = systemReports.FirstOrDefault(x => x.PwReportId == reportId);
            if (systemReport != null)
            {
                return systemReport;
            }

            var query = @"SELECT * FROM report(@PwShopId) WHERE PwReportId = @reportId;";
            return _connectionWrapper
                    .Query<PwReport>(query, new { PwShopId, reportId })
                    .FirstOrDefault();
        }

        public bool HasFilters(long reportId)
        {
            var systemReports = RetrieveSystemDefinedReports();
            var systemReport = systemReports.FirstOrDefault(x => x.PwReportId == reportId);
            if (systemReport != null)
            {
                return false;
            }

            var query = 
                @"SELECT COUNT(PwFilterId) 
                FROM reportfilter(@PwShopId) 
                WHERE PwReportId = @reportId;";

            var count =  _connectionWrapper.Query<int>(query, new { PwShopId, reportId }).FirstOrDefault();
            return count > 0;
        }

        public bool ReportExists(long reportId)
        {
            if (RetrieveSystemDefinedReportsIds().Any(x => x == reportId))
            {
                return true;
            }

            var query = @"SELECT PwReportId FROM report(@PwShopId) WHERE PwReportId = @reportId;";
            return _connectionWrapper
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
                reportName = SystemReportFactory.CustomDefaultNameBuilder(reportNumber);
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

            var query = @"SELECT * FROM report(@PwShopId) WHERE CopyForEditing = 0 AND Name = @name;";
            return _connectionWrapper
                    .Query<PwReport>(query, new { PwShopId, reportId, name })
                    .Any();
        }

        public long InsertReport(PwReport report)
        {
            var query =
                @"INSERT INTO report(@PwShopId) (
                    PwShopId, ReportTypeId, Name, IsSystemReport, CopyForEditing, OriginalReportId, 
                    StartDate, EndDate, GroupingId, OrderingId, CreatedDate, LastAccessedDate ) 
                VALUES ( 
                    @PwShopId, @ReportTypeId, @Name, @IsSystemReport, @CopyForEditing, @OriginalReportId, 
                    @StartDate, @EndDate, @GroupingId, @OrderingId, getdate(), getdate() );
                
                SELECT SCOPE_IDENTITY();";

            return _connectionWrapper.Query<long>(query, report).First();
        }

        public void UpdateReport(PwReport report)
        {
            report.PwShopId = PwShopId;

            var query = @"UPDATE report(@PwShopId) SET 
                            Name = @Name,
                            IsSystemReport = @IsSystemReport,
                            CopyForEditing = @CopyForEditing,
                            OriginalReportId = @OriginalReportId,
                            StartDate = @StartDate,
                            EndDate = @EndDate,
                            GroupingId = @GroupingId,
                            OrderingId = @OrderingId,
                            LastAccessedDate = getdate()                     
                        WHERE PwReportId = @PwReportId;";
            _connectionWrapper.Execute(query, report );
        }

        public void UpdateReportLastAccessed(long reportId)
        {
            var query = @"UPDATE report(@PwShopId) SET LastAccessedDate = getdate() WHERE PwReportId = @reportId;";
            _connectionWrapper.Execute(query, new { this.PwShopId, reportId });
        }

        public void DeleteReport(long reportId)
        {
            var query = @"DELETE FROM report(@PwShopId) WHERE PwReportId = @reportId;";
            _connectionWrapper.Execute(query, new { PwShopId, reportId });
        }
    }
}

