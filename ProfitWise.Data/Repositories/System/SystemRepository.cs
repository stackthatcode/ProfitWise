using System;
using System.Data;
using System.Linq;
using Dapper;
using ProfitWise.Data.Database;
using ProfitWise.Data.Model.System;

namespace ProfitWise.Data.Repositories
{
    public class SystemRepository
    {
        private readonly ConnectionWrapper _connectionWrapper;
        private IDbConnection Connection => _connectionWrapper.DbConn;


        public SystemRepository(ConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.InitiateTransaction();
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
    }
}
