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
            return _connectionWrapper.StartTransactionForScope();
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
    }
}
