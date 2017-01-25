using System;
using System.Data;
using System.Linq;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Model.System;

namespace ProfitWise.Data.Repositories
{
    public class SystemStateRepository
    {
        private readonly IDbConnection _connection;

        public SystemStateRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connection.BeginTransaction();
        }


        public SystemState Retrieve()
        {
            var query = @"SELECT * FROM systemstate";
            return _connection.Query<SystemState>(query).First();
        }

        public void UpdateExchangeRateLastDate(DateTime date)
        {
            var query = @"UPDATE systemstate SET ExchangeRateLastDate = @date;";
            _connection.Execute(query, new { date });
        }        
    }
}
