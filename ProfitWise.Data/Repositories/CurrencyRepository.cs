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

        public IList<CurrencyConversion> RetrieveCurrencyConversions()
        {
            var query = @"SELECT * FROM currencyconversion ORDER BY Date, SourceCurrencyId ASC;";
            return _connection.Query<CurrencyConversion>(query).ToList();
        }

    }
}
