using System;
using System.Data;
using System.Linq;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Database;
using ProfitWise.Data.Model.System;

namespace ProfitWise.Data.Repositories
{
    public class SystemStateRepository
    {
        private readonly ConnectionWrapper _connectionWrapper;
        private IDbConnection Connection => _connectionWrapper.DbConn;


        public SystemStateRepository(ConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.StartTransactionForScope();
        }



        public SystemState Retrieve()
        {
            var query = @"SELECT * FROM systemstate";
            return Connection.Query<SystemState>(query).First();
        }

        public void UpdateExchangeRateLastDate(DateTime date)
        {
            var query = @"UPDATE systemstate SET ExchangeRateLastDate = @date;";
            Connection.Execute(query, new { date });
        }        
    }
}
