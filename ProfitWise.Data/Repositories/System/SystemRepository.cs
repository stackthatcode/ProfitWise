using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using ProfitWise.Data.Database;
using ProfitWise.Data.Model.Cogs.UploadObjects;
using ProfitWise.Data.Model.System;

namespace ProfitWise.Data.Repositories.System
{
    public class SystemRepository
    {
        private readonly ConnectionWrapper _connectionWrapper;
        private IDbConnection Connection => _connectionWrapper.DbConn;

        private static bool _maintenanceActiveCache = false;
        private static DateTime? _lastMaintenanceActiveCheck;
        private DateTime NextMaintenanceCheck => _lastMaintenanceActiveCheck ?? DateTime.UtcNow;
        private readonly object maintenanceConcurrencyLock = new object();

        public SystemRepository(ConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.InitiateTransaction();
        }


        public bool RetrieveMaintenanceActive()
        {
            if (DateTime.UtcNow >= NextMaintenanceCheck)
            {
                lock (maintenanceConcurrencyLock)
                {
                    var query = "SELECT [MaintenanceActive] FROM systemstate;";
                    _maintenanceActiveCache = Connection.Query<bool>(query, new {}).First();

                    // Think ... this could go way farther out to the future
                    _lastMaintenanceActiveCheck = DateTime.UtcNow.AddMinutes(1);
                }
            }
            return _maintenanceActiveCache;
        }

        public SystemState Retrieve()
        {
            var query = "SELECT * FROM systemstate;";
            return Connection.Query<SystemState>(query, new { }).First();
        }

        public DateTime? RetrieveLastExchangeRateDate()
        {
            var query = "SELECT ExchangeRateLastDate FROM systemstate;";
            return Connection.Query<DateTime?>(query, new {  }).First();
        }

        public void UpdateLastExchangeRateDate(DateTime lastDate)
        {
            var query = "UPDATE systemstate SET ExchangeRateLastDate = @lastDate;";
            Connection.Execute(query, new { lastDate });
        }
        
        public void UpdateMaintenance(bool active, string reason)
        {
            _maintenanceActiveCache = active;
            var query =
                @"UPDATE systemstate SET 
                    [MaintenanceActive] = @active,
                    [MaintenanceReason] = @reason";
            Connection.Execute(query, new { active, reason });
        }




        // TODO - this is where the deletion logic appears
        public long DeletePickListByDate(DateTime cutoffDate)
        {
            var query =
                @"DELETE FROM profitwisepicklistmasterproduct 
                WHERE PwShopId = PwShopId AND PwPickListId IN 
	                (   SELECT PwPickListId FROM profitwisepicklist 
                        WHERE LastAccessed <= @cutoffDate );
                
                DELETE FROM profitwisepicklist WHERE LastAccessed <= @cutoffDate;";

            return Connection.Query<long>(
                query, new { cutoffDate }).FirstOrDefault();
        }

        public void CleanupReportData(DateTime cutoffDate)
        {
            var query =
                @"  DELETE FROM profitwiseprofitquerystub WHERE PwReportId IN 
                    ( SELECT PwReportId FROM profitwisereport 
                    WHERE LastAccessedDate < @cutoffDate AND CopyForEditing = 1)

                    DELETE FROM profitwisereportfilter WHERE PwReportId IN 
                    ( SELECT PwReportId FROM profitwisereport 
                    WHERE LastAccessedDate < @cutoffDate AND CopyForEditing = 1)

                    DELETE FROM profitwisereport WHERE PwReportId IN 
                    ( SELECT PwReportId FROM profitwisereport 
                    WHERE LastAccessedDate < @cutoffDate AND CopyForEditing = 1)";

            Connection.Execute(query, new { cutoffDate });
        }

        public int NaughtySystemQuery()
        {
            var query = @"SELECT * [NonExistentTable]";
            return Connection.Query<int>(query, new { }).FirstOrDefault();
        }


        public void InsertCalendarEntry(
                DateTime date, int y, int q, int m, int d, int dw, string monthName, string dayName, int w, bool isWeekday)
        {
            var query = @"INSERT INTO calendar_table VALUES (@date, @y,  @q, @m, @d, @dw, @monthName, @dayName, @w, @isWeekday, 0, '', 0)";
            Connection.Execute(query, new { date, y, q, m, d, dw, monthName, dayName, w, isWeekday });
        }


        // Clean-up Queries
        public void DeleteChildlessMasterVariants()
        {
            var query1 =
                @"DELETE FROM profitwisemastervariantcogsdetail
                WHERE PwMasterVariantId NOT IN ( SELECT PwMasterVariantId FROM profitwisevariant )";

            _connectionWrapper.Execute(query1);

            var query2 =
                @"DELETE FROM profitwisemastervariantcogscalc
                WHERE PwMasterVariantId NOT IN ( SELECT PwMasterVariantId FROM profitwisevariant )";

            _connectionWrapper.Execute(query2);

            var query3 =
                @"DELETE FROM profitwisemastervariant
                WHERE PwMasterVariantId NOT IN ( SELECT PwMasterVariantId FROM profitwisevariant )";

            _connectionWrapper.Execute(query3);
        }

        public void DeleteChildlessMasterProducts()
        {
            var query1 = 
                    @"DELETE t1 FROM profitwisemastervariantcogscalc t1
	                    INNER JOIN profitwisemastervariant t2 
		                    ON t1.PwMasterVariantId = t2.PwMasterVariantId
                    WHERE PwMasterProductId NOT IN (SELECT PwMasterProductId FROM profitwiseproduct);";
            _connectionWrapper.Execute(query1);

            var query2 = 
                    @"DELETE t1 FROM profitwisemastervariantcogsdetail t1
	                    INNER JOIN profitwisemastervariant t2 
		                        ON t1.PwMasterVariantId = t2.PwMasterVariantId
                    WHERE PwMasterProductId NOT IN (SELECT PwMasterProductId FROM profitwiseproduct);";
            _connectionWrapper.Execute(query2);

            var query3 = 
                    @"DELETE FROM profitwisemastervariant
                    WHERE PwMasterProductId NOT IN (SELECT PwMasterProductId FROM profitwiseproduct);";
            _connectionWrapper.Execute(query3);

            var query4 =
                    @"DELETE FROM profitwisemasterproduct
                    WHERE PwMasterProductId NOT IN (SELECT PwMasterProductId FROM profitwiseproduct);";
            _connectionWrapper.Execute(query4);
        }

        public List<Upload> RetrieveOldUploads(int maximumAgeDays)
        {
            var query =
                @"SELECT * FROM profitwiseuploads
                WHERE UploadStatus <> 1 
                AND LastUpdated <= DATEADD(day, -@maximumAgeDays, GETUTCDATE())
                ORDER BY [LastUpdated] DESC;";

            return _connectionWrapper
                .Query<Upload>(query, new {maximumAgeDays})
                .ToList();
        }

        public void DeleteFileUpload(long fileUploadId)
        {
            var query = @"DELETE FROM profitwiseuploads WHERE FileUploadId = @fileUploadId";
            _connectionWrapper.Execute(query, new { fileUploadId });
        }
    }
}

