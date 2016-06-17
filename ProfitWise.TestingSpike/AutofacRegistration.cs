using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.SqlClient;
using Autofac;
using Autofac.Extras.DynamicProxy2;
using Microsoft.AspNet.Identity.EntityFramework;
using MySql.Data.MySqlClient;
using ProfitWise.Batch.MultiTenantFactories;
using ProfitWise.Batch.RefreshServices;
using ProfitWise.Data.Repositories;
using Push.Shopify.HttpClient;
using Push.Shopify.Repositories;
using Push.Utilities.Helpers;
using Push.Utilities.Logging;
using Push.Utilities.Web.Identity;


namespace ProfitWise.Batch
{
    public class AutofacRegistration
    {
        public static IContainer Build()
        {
            // Load configuration into local variables
            var mysqlConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var hangFileConnectionString = ConfigurationManager.ConnectionStrings["HangFireConnection"].ConnectionString;

            var shopifyRetryLimit = ConfigurationManager.AppSettings.GetAndTryParseAsInt("ShopifyRetryLimit", 3);
            var shopifyHttpTimeout = ConfigurationManager.AppSettings.GetAndTryParseAsInt("ShopifyHttpTimeout", 60000);
            var shopifyThrottlingDelay = ConfigurationManager.AppSettings.GetAndTryParseAsInt("ShopifyThrottlingDelay", 500);
            var refreshServiceShopifyOrderLimit =
                ConfigurationManager.AppSettings.GetAndTryParseAsInt("RefreshServiceShopifyOrderLimit", 250);



            // Autofac registration sequence
            var builder = new ContainerBuilder();

            // Push.Shopify classes
            builder.RegisterType<HttpClient>().As<IHttpClient>().EnableClassInterceptors();
            builder.RegisterType<ShopifyHttpClient>().As<IShopifyHttpClient>().EnableClassInterceptors();
            builder.RegisterType<ShopifyRequestFactory>().EnableClassInterceptors();
            builder.RegisterType<OrderApiRepository>().EnableClassInterceptors();
            builder.RegisterType<ProductApiRepository>().EnableClassInterceptors();
            builder.RegisterType<ApiRepositoryFactory>();

            // ProfitWise.Data classes
            builder.RegisterType<ProductDataRepository>().EnableClassInterceptors();
            builder.RegisterType<VariantDataRepository>().EnableClassInterceptors();
            builder.RegisterType<SqlRepositoryFactory>().EnableClassInterceptors();

            // Push.Web Identity Stuff
            builder.RegisterType<ApplicationUserManager>();
            builder.RegisterType<UserStore<ApplicationUser>>();

            // ProfitWise.Batch classes
            builder.RegisterType<OrderRefreshService>().EnableClassInterceptors();
            builder.RegisterType<ProductRefreshService>().EnableClassInterceptors();
            builder.RegisterType<ApiRepositoryFactory>().EnableClassInterceptors();
            builder.RegisterType<ApiRepositoryFactory>().EnableClassInterceptors();

            // SQL connections
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

            // Proxy Registration
            builder.RegisterType<LoggingInterceptor>();
            
            return builder.Build();
        }
    }
}
