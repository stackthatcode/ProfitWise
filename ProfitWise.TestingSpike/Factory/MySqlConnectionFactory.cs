using System.Configuration;
using MySql.Data.MySqlClient;

namespace ProfitWise.Batch.Factory
{
    public class MySqlConnectionFactory
    {
        public static MySqlConnection Make()
        {
            var connectionstring = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var connection = new MySqlConnection(connectionstring);
            connection.Open();
            return connection;
        }
    }
}
