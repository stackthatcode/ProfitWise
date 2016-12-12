﻿using System.Collections.Generic;
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
            var query = @"DELETE FROM profitwisereport            
                        WHERE PwShopId = @PwShopId 
                        AND PwReportId = @reportId;";
            _connection.Execute(query, new { PwShopId, reportId });
        }




    }
}

