using System;
using System.Linq;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Model.System;

namespace ProfitWise.Data.Repositories
{
    public class SystemStateRepository
    {
        private readonly MySqlConnection _connection;

        public SystemStateRepository(MySqlConnection connection)
        {
            _connection = connection;
        }

        public MySqlTransaction InitiateTransaction()
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
