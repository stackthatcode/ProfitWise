using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.SqlClient;
using Autofac;
using Autofac.Extras.DynamicProxy2;
using Microsoft.AspNet.Identity.EntityFramework;
using MySql.Data.MySqlClient;
using ProfitWise.Batch.MultiTenantFactories;
using ProfitWise.Batch.Orders;
using ProfitWise.Batch.Products;
using ProfitWise.Data.Repositories;
using Push.Shopify.HttpClient;
using Push.Shopify.Repositories;
using Push.Utilities.Logging;
using Push.Utilities.Web.Identity;
using MySqlConnectionFactory = MySql.Data.Entity.MySqlConnectionFactory;

namespace ProfitWise.Batch
{

    public static class ConfigurationExtension
    {
        public static int? AppSettingIntGet(this NameValueCollection collection, string name)
        {
            return collection[name] == null ? (int?)null : Int32.Parse(collection[name]);
        }
    }

    public class AutofacRegistration
    {
        public static IContainer Build()
        {
            // Load configuration into local variables
            var mysqlConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var hangFileConnectionString = ConfigurationManager.ConnectionStrings["HangFireConnection"].ConnectionString;

            var shopifyRetryLimit = ConfigurationManager.AppSettings.AppSettingIntGet("ShopifyRetryLimit") ?? 3;
            var shopifyHttpTimeout = ConfigurationManager.AppSettings.AppSettingIntGet("ShopifyHttpTimeout") ?? 60000;
            var shopifyThrottlingDelay = ConfigurationManager.AppSettings.AppSettingIntGet("ShopifyThrottlingDelay") ?? 500;

            var refreshServiceShopifyOrderLimit =
                ConfigurationManager.AppSettings.AppSettingIntGet("RefreshServiceShopifyOrderLimit") ?? 250;


            // Autofac registration sequence
            var builder = new ContainerBuilder();

            // Push.Shopify classes
            builder.RegisterType<HttpClient>().As<IHttpClient>().EnableClassInterceptors();
            builder.RegisterType<ShopifyHttpClient>().As<IShopifyHttpClient>().EnableClassInterceptors();
            builder.RegisterType<OrderApiRepository>().EnableClassInterceptors();
            builder.RegisterType<ProductApiRepository>().EnableClassInterceptors();

            // ProfitWise.Data classes
            builder.RegisterType<ProductDataRepository>().EnableClassInterceptors();
            builder.RegisterType<VariantDataRepository>().EnableClassInterceptors();

            // ProfitWise.Batch classes
            builder.RegisterType<ApplicationUserManager>();
            builder.RegisterType<UserStore<ApplicationUser>>();

            //builder.Register(c  => 
                    
            //    ).As<IShopifyClientFactory>().EnableClassInterceptors();
            builder.RegisterType<OrderRefreshService>().EnableClassInterceptors();
            builder.RegisterType<ProductRefreshService>().EnableClassInterceptors();
            builder.RegisterType<ApiRepositoryFactory>().EnableClassInterceptors();
            builder.RegisterType<ApiRepositoryFactory>().EnableClassInterceptors();

            // SQL Persistence
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
            builder.RegisterType<LoggingInterceptor>();
            
            return builder.Build();
        }
    }
}
