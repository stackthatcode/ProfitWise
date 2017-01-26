﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using ProfitWise.Data.Database;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.System;

namespace ProfitWise.Data.Repositories
{
    public class CurrencyRepository
    {
        private readonly ConnectionWrapper _connection;

        public CurrencyRepository(ConnectionWrapper connection)
        {
            _connection = connection;
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connection.StartTransactionForScope();
        }


        public IList<Currency> RetrieveCurrency()
        {
            var query = @"SELECT * FROM currency ORDER BY CurrencyId ASC;";
            return _connection.DbConn.Query<Currency>(query).ToList();
        }

        public List<ExchangeRate> RetrieveExchangeRates()
        {
            var query = @"SELECT * FROM exchangerate 
                        ORDER BY Date, SourceCurrencyId ASC;";
            return _connection.DbConn.Query<ExchangeRate>(query).ToList();
        }

        public List<ExchangeRate> RetrieveExchangeRateByDate(DateTime date)
        {
            var query = @"SELECT * FROM exchangerate 
                        WHERE Date = @date
                        ORDER BY Date, SourceCurrencyId ASC;";
            return _connection.DbConn.Query<ExchangeRate>(query, new { date }).ToList();
        }

        public List<ExchangeRate> RetrieveExchangeRateFromDate(DateTime minimumDate)
        {
            var query = @"SELECT * FROM exchangerate 
                        WHERE Date >= @minimumDate
                        ORDER BY Date, SourceCurrencyId ASC;";
            return _connection.DbConn.Query<ExchangeRate>(query, new { minimumDate }).ToList();
        }

        public void InsertExchangeRate(ExchangeRate exchangeRate)
        {
            var query = @"INSERT INTO exchangerate
                        VALUES ( @SourceCurrencyId, @DestinationCurrencyId, @Date, @Rate )";
            _connection.DbConn.Execute(query, exchangeRate);
        }

        public void DeleteForDate(DateTime date)
        {
            var query = @"DELETE FROM exchangerate WHERE Date = @date;";
            _connection.DbConn.Execute(query, new { @date });
        }
    }
}
