using System;
using System.Data;

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

        public IDbTransaction StartTransactionForScope()
        {
            Transaction = _connection.BeginTransaction();
            return Transaction;
        }

        public void Dispose()
        {
            Transaction?.Dispose();
            _connection.Dispose();
        }
    }
}

