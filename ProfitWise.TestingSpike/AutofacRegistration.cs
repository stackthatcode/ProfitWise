using System.Configuration;
using System.Data.SqlClient;
using Autofac;
using Autofac.Extras.DynamicProxy2;
using Microsoft.AspNet.Identity.EntityFramework;
using MySql.Data.MySqlClient;
using ProfitWise.Batch.RefreshServices;
using Push.Shopify.HttpClient;
using Push.Utilities.Helpers;
using Push.Utilities.Logging;
using Push.Foundation.Web.Identity;
using Push.Shopify.Factories;


namespace ProfitWise.Batch
{
    public class AutofacRegistration
    {
        public static IContainer Build()
        {
            // Autofac registration sequence
            var builder = new ContainerBuilder();

            // ProfitWise.Data registration
            ProfitWise.Data.AutofacRegistration.Build(builder);


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


            // Push.Web Identity Stuff
            builder.RegisterType<ApplicationUserManager>();
            builder.RegisterType<UserStore<ApplicationUser>>();


            // ProfitWise.Batch registration
            builder.RegisterType<OrderRefreshService>().EnableClassInterceptors();
            builder.RegisterType<ProductRefreshService>().EnableClassInterceptors();
            builder.RegisterType<ApiRepositoryFactory>().EnableClassInterceptors();
            builder.RegisterType<ApiRepositoryFactory>().EnableClassInterceptors();
            builder.Register<RefreshServiceConfiguration>(x => new RefreshServiceConfiguration()
            {
                RefreshServiceMaxOrderRate =
                    ConfigurationManager.AppSettings.GetAndTryParseAsInt("RefreshServiceMaxOrderRate", 50),
                RefreshServiceMaxProduceRate =
                    ConfigurationManager.AppSettings.GetAndTryParseAsInt("RefreshServiceMaxProduceRate", 100),
            });


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

            // Infrastructure setup & configuration binding
            builder.Register(c => LoggerSingleton.Get()).As<ILogger>();


            // Fin!
            return builder.Build();
        }
    }
}
