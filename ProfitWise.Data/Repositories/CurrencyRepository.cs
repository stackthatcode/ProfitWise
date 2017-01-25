using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.System;

namespace ProfitWise.Data.Repositories
{
    public class CurrencyRepository
    {
        private readonly MySqlConnection _connection;

        public CurrencyRepository(MySqlConnection connection)
        {
            _connection = connection;
        }

        public MySqlTransaction InitiateTransaction()
        {
            return _connection.BeginTransaction();
        }


        public IList<Currency> RetrieveCurrency()
        {
            var query = @"SELECT * FROM currency ORDER BY CurrencyId ASC;";
            return _connection.Query<Currency>(query).ToList();
        }

        public List<ExchangeRate> RetrieveExchangeRates()
        {
            var query = @"SELECT * FROM exchangerate 
                        ORDER BY Date, SourceCurrencyId ASC;";
            return _connection.Query<ExchangeRate>(query).ToList();
        }

        public List<ExchangeRate> RetrieveExchangeRateByDate(DateTime date)
        {
            var query = @"SELECT * FROM exchangerate 
                        WHERE Date = @date
                        ORDER BY Date, SourceCurrencyId ASC;";
            return _connection.Query<ExchangeRate>(query, new { date }).ToList();
        }

        public List<ExchangeRate> RetrieveExchangeRateFromDate(DateTime minimumDate)
        {
            var query = @"SELECT * FROM exchangerate 
                        WHERE Date >= @minimumDate
                        ORDER BY Date, SourceCurrencyId ASC;";
            return _connection.Query<ExchangeRate>(query, new { minimumDate }).ToList();
        }

        public void InsertExchangeRate(ExchangeRate exchangeRate)
        {
            var query = @"INSERT INTO exchangerate
                        VALUES ( @SourceCurrencyId, @DestinationCurrencyId, @Date, @Rate )";
            _connection.Execute(query, exchangeRate);
        }

        public void DeleteForDate(DateTime date)
        {
            var query = @"DELETE FROM exchangerate WHERE Date = @date;";
            _connection.Execute(query, new { @date });
        }
    }
}
