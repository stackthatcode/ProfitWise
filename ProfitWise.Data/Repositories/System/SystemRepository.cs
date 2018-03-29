using System;
using System.Data;
using System.Linq;
using Dapper;
using ProfitWise.Data.Database;
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
    }
}
