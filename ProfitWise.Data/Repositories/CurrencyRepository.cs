using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Model;

namespace ProfitWise.Data.Repositories
{
    public class CurrencyRepository
    {
        private readonly MySqlConnection _connection;

        public CurrencyRepository(MySqlConnection connection)
        {
            _connection = connection;
        }

        public IList<Currency> RetrieveCurrency()
        {
            var query = @"SELECT * FROM currency ORDER BY CurrencyId ASC;";
            return _connection.Query<Currency>(query).ToList();
        }

        public IList<ExchangeRate> RetrieveExchangeRates()
        {
            var query = @"SELECT * FROM exchangerate 
                        ORDER BY Date, SourceCurrencyId ASC;";
            return _connection.Query<ExchangeRate>(query).ToList();
        }


        public IList<ExchangeRate> RetrieveExchangeRates(DateTime minimumDate)
        {
            var query = @"SELECT * FROM exchangerate 
                        WHERE Date >= {minimumDate}
                        ORDER BY Date, SourceCurrencyId ASC;";
            return _connection.Query<ExchangeRate>(query, new { minimumDate }).ToList();
        }

        public void InsertExchangeRate(ExchangeRate rate)
        {
            var query = @"INSERT INTO exchangerate
                        VALUES ( @SourceCurrencyId, @DestinationCurrencyId, @Date, @Multipler )";
            _connection.Execute(query, rate);
        }

    }
}
