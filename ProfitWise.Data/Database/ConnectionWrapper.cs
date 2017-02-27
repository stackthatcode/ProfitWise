using System;
using System.Collections.Generic;
using System.Data;
using Dapper;

namespace ProfitWise.Data.Database
{
    public class ConnectionWrapper : IDisposable
    {
        private readonly IDbConnection _connection;
        public IDbTransaction Transaction { get; private set; }
        public Guid Identifier { get; } = Guid.NewGuid();
        public IDbConnection DbConn => _connection;


        public ConnectionWrapper(IDbConnection connection)
        {
            _connection = connection;
        }

        public IDbTransaction InitiateTransaction()
        {
            Transaction = _connection.BeginTransaction();
            return Transaction;
        }

        public int Execute(string sql, object param = null)
        {
            return _connection.Execute(sql, param, this.Transaction);
        }

        public IEnumerable<T> Query<T>(string sql, object param = null)
        {
            return _connection.Query<T>(sql, param, this.Transaction);
        }

        public void CommitTranscation()
        {
            Transaction.Commit();
            Transaction.Dispose();
            Transaction = null;
        }

        public void Dispose()
        {
            Transaction?.Dispose();
            _connection.Dispose();
        }
    }
}

