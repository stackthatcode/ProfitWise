using System.Configuration;
using System.Data.SqlClient;

namespace ProfitWise.TestingSpike
{
    public class ConnectionFactory
    {
        public static SqlConnection Make()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            return new SqlConnection(connectionString);
        }
    }
}
