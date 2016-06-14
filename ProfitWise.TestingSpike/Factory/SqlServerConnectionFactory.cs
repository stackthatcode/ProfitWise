using System.Configuration;
using System.Data.SqlClient;

namespace ProfitWise.Batch.Factory
{
    public class SqlServerConnectionFactory
    {
        public static SqlConnection Make()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            return new SqlConnection(connectionString);
        }
    }
}
