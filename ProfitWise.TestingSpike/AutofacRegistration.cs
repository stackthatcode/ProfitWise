using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Autofac;
using MySql.Data.MySqlClient;
using ProfitWise.Batch.RefreshServices;
using Push.Shopify.HttpClient;
using Push.Utilities.Helpers;
using Push.Utilities.Logging;
using Push.Utilities.CastleProxies;
using Push.Utilities.Errors;


namespace ProfitWise.Batch
{
    public class AutofacRegistration
    {
        public static IContainer Build()
        {
            // Autofac registration sequence
            var builder = new ContainerBuilder();


            // Infrastructure setup & configuration binding
            builder.Register(c => LoggerSingleton.Get()).As<IPushLogger>();
            var registry = new InceptorRegistry();
            registry.Add(typeof(ErrorForensics));


            // ProfitWise.Data registration
            ProfitWise.Data.AutofacRegistration.Build(builder);


            // SQL connections
            // ... load configuration into local variables
            var mysqlConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var hangFileConnectionString = ConfigurationManager.ConnectionStrings["HangFireConnection"].ConnectionString;

            // ... and register configurationa
            builder
                .Register<MySqlConnection>(ctx =>
                {
                    var connectionstring = mysqlConnectionString;
                    var connection = new MySqlConnection(connectionstring);
                    connection.Open();
                    return connection;
                })
                .As<MySqlConnection>()
                .As<DbConnection>()
                .As<IDbConnection>();


            builder.Register<SqlConnection>(ctx =>
            {
                var connectionString = hangFileConnectionString;
                return new SqlConnection(connectionString);
            });

            builder.Register(x => new RefreshServiceConfiguration()
            {
                RefreshServiceMaxOrderRate =
                    ConfigurationManager.AppSettings.GetAndTryParseAsInt("RefreshServiceMaxOrderRate", 50),
                RefreshServiceMaxProduceRate =
                    ConfigurationManager.AppSettings.GetAndTryParseAsInt("RefreshServiceMaxProduceRate", 100),
            });



            // Push.Shopify registration
            Push.Shopify.AutofacRegistration.Build(builder);

            // ... override the default ShopifyHttpClientConfig registration to inject our settings
            builder.Register<ShopifyHttpClientConfig>(x => new ShopifyHttpClientConfig()
            {
                ShopifyRetryLimit =
                    ConfigurationManager.AppSettings.GetAndTryParseAsInt("ShopifyRetryLimit", 3),
                ShopifyHttpTimeout =
                    ConfigurationManager.AppSettings.GetAndTryParseAsInt("ShopifyHttpTimeout", 60000),
                ShopifyThrottlingDelay =
                    ConfigurationManager.AppSettings.GetAndTryParseAsInt("ShopifyThrottlingDelay", 500),
            });


            // Push.Foundation.Web Identity Stuff
            var encryption_key = ConfigurationManager.AppSettings["security_aes_key"];
            var encryption_iv = ConfigurationManager.AppSettings["security_aes_iv"];
            Push.Foundation.Web.AutofacRegistration.Build(builder, encryption_key, encryption_iv);
            

            // Fin!
            return builder.Build();
        }
    }
}
