using System.Configuration;
using System.Data.SqlClient;
using Autofac;
using Autofac.Extras.DynamicProxy2;
using MySql.Data.MySqlClient;
using ProfitWise.Batch.Processes;
using ProfitWise.Batch.RefreshServices;
using Push.Shopify.HttpClient;
using Push.Utilities.Helpers;
using Push.Utilities.Logging;
using Push.Shopify.Factories;
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
            builder.Register(c => LoggerSingleton.Get()).As<ILogger>();
            builder.RegisterType<ErrorForensics>();


            // ProfitWise.Data registration
            ProfitWise.Data.AutofacRegistration.Build(builder);


            // SQL connections
            // ... load configuration into local variables
            var mysqlConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var hangFileConnectionString = ConfigurationManager.ConnectionStrings["HangFireConnection"].ConnectionString;

            // ... and register objects
            builder.Register<MySqlConnection>(ctx =>
            {
                var connectionstring = mysqlConnectionString;
                var connection = new MySqlConnection(connectionstring);
                connection.Open();
                return connection;
            });
            builder.Register<SqlConnection>(ctx =>
            {
                var connectionString = hangFileConnectionString;
                return new SqlConnection(connectionString);
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

            
            // ProfitWise.Batch registration
            builder.RegisterType<OrderRefreshService>()
                .EnableClassInterceptors()
                .InterceptedBy(typeof(ErrorForensics));

            builder.RegisterType<ProductRefreshService>()
                .EnableClassInterceptors()
                .InterceptedBy(typeof(ErrorForensics));

            builder.RegisterType<ApiRepositoryFactory>()
                .EnableClassInterceptors()
                .InterceptedBy(typeof(ErrorForensics));

            builder.RegisterType<ApiRepositoryFactory>()
                .EnableClassInterceptors()
                .InterceptedBy(typeof(ErrorForensics));

            builder.RegisterType<RefreshProcess>()
                .EnableClassInterceptors()
                .InterceptedBy(typeof(ErrorForensics));

            builder.Register<RefreshServiceConfiguration>(x => new RefreshServiceConfiguration()
            {
                RefreshServiceMaxOrderRate =
                    ConfigurationManager.AppSettings.GetAndTryParseAsInt("RefreshServiceMaxOrderRate", 50),
                RefreshServiceMaxProduceRate =
                    ConfigurationManager.AppSettings.GetAndTryParseAsInt("RefreshServiceMaxProduceRate", 100),
            });


            // Fin!
            return builder.Build();
        }
    }
}
