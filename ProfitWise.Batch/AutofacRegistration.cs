using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Autofac;
using Autofac.Builder;
using Hangfire;
using ProfitWise.Data.Configuration;
using ProfitWise.Data.Database;
using ProfitWise.Data.ExchangeRateApis;
using ProfitWise.Data.HangFire;
using ProfitWise.Data.ProcessSteps;
using ProfitWise.Data.Services;
using ProfitWise.Data.Utility;
using Push.Foundation.Utilities.CastleProxies;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.HttpClient;
using Push.Utilities.CastleProxies;


namespace ProfitWise.Batch
{
    public class AutofacRegistration
    {
        public static IContainer Build(bool runningHangFire = false)
        {
            // Autofac registration sequence
            var builder = new ContainerBuilder();

            // ProfitWise.Data registration - which invokes downstream dependent registrations
            Data.AutofacRegistration.Build(builder);

            // Sets the logging singleton
            const string loggerName = "ProfitWise.Batch";

            builder.RegisterType<BackgroundJobTraceFormatter>()
                    .As<ILogFormatter>()
                    .InstancePerBackgroundJobIfTrue(runningHangFire);

            builder.Register(x => new NLogger(loggerName, x.Resolve<ILogFormatter>()))
                    .As<IPushLogger>()
                    .InstancePerBackgroundJobIfTrue(runningHangFire);

            // Register a singleton with HangFireLogProvider
            HangFireLogProvider.RegisterInstance(
                    new NLogger(loggerName, new BackgroundJobTraceFormatter()));


            // This registration ensures that within a Background Job, always the same logger will be 
            // used - thus the ScopePrefix need only be set once. :-)            
            builder.Register(ctx =>
                {
                    var connectionstring =
                        ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                    var connection = new SqlConnection(connectionstring);
                    connection.Open();
                    return connection;
                })
                .As<SqlConnection>()
                .As<DbConnection>()
                .As<IDbConnection>()
                .InstancePerBackgroundJobIfTrue(runningHangFire);

            // Critical piece for all database infrastructure to work smoothly
            builder.RegisterType<ConnectionWrapper>().InstancePerBackgroundJobIfTrue(runningHangFire);            

            var registry = new InceptorRegistry();
            registry.Add(typeof(ErrorForensics));
            
            // Configure the paging rates of the Refresh Services
            builder.Register(x => new RefreshServiceConfiguration()
            {
                MaxOrderRate = ConfigurationManager.AppSettings.GetAndTryParseAsInt("RefreshServiceMaxOrderRate", 50),
                MaxProductRate = ConfigurationManager.AppSettings.GetAndTryParseAsInt("RefreshServiceMaxProductRate", 100),
            });

            // Push.Shopify API configuration
            Push.Shopify.AutofacRegistration.Build(builder);

            builder.Register(x => new ShopifyClientConfig()
            {
                RetryLimit = ConfigurationManager.AppSettings.GetAndTryParseAsInt("ShopifyRetryLimit", 3),
                Timeout = ConfigurationManager.AppSettings.GetAndTryParseAsInt("ShopifyHttpTimeout", 60000),
                ThrottlingDelay = ConfigurationManager.AppSettings.GetAndTryParseAsInt("ShopifyThrottlingDelay", 500),
                RetriesEnabled = ConfigurationManager.AppSettings.GetAndTryParseAsBool("ShopifyRetriesEnabled", true)
            });


            // Exchange Rate API configuration
            builder.Register(x => new ExchangeRateApiConfig()
            {
                RetryLimit =
                    ConfigurationManager.AppSettings.GetAndTryParseAsInt("ExchangeRateApiRetryLimit", 3),
                Timeout =
                    ConfigurationManager.AppSettings.GetAndTryParseAsInt("ExchangeRateApiHttpTimeout", 60000),
                ThrottlingDelay =
                    ConfigurationManager.AppSettings.GetAndTryParseAsInt("ExchangeRateApiThrottlingDelay", 500),
                RetriesEnabled =
                    ConfigurationManager.AppSettings.GetAndTryParseAsBool("FixerApiRetriesEnabled", true),

                OerApiKey = ConfigurationManager.AppSettings["OerApiKey"],
                FixerApiKey = ConfigurationManager.AppSettings["FixerApiKey"],
            });

            // Push.Foundation.Web relies on consumers to supply Key and IV for its Encryption Service
            Push.Foundation.Web.AutofacRegistration.Build(builder,
                    ProfitWiseConfiguration.Settings.ClaimKey, 
                    ProfitWiseConfiguration.Settings.ClaimIv,
                    ProfitWiseConfiguration.Settings.ShopifyApiSecret);

            AddDiagnostics(builder);
            
            // Fin!
            return builder.Build();
        }


        private static void AddDiagnostics(ContainerBuilder builder)
        {
            // ProfitWise.Data OrderDiagnostic
            var shop = ConfigurationManager.AppSettings
                .GetAndTryParseAsLongNullable("ProfitWiseDiagonosticShop", -1);
            var orderList = ConfigurationManager.AppSettings
                .GetAndTryParseAsString("ProfitWiseDiagonosticOrders", "");
            var orderIds =
                orderList.IsNullOrEmpty()
                    ? new List<long>()
                    : orderList.Split(',').Select(x => long.Parse(x)).ToList();

            builder.Register(x => new ShopifyOrderDiagnosticShim
            {
                PwShopId = shop,
                OrderIds = orderIds,
            });
        }
    }

    public static class AutofacExtension
    {
        public static IRegistrationBuilder<T1, T2, T3> 
                InstancePerBackgroundJobIfTrue<T1, T2, T3>(
                    this IRegistrationBuilder<T1, T2, T3>  input, bool expression)
        {
            return expression ? input.InstancePerBackgroundJob() : input.InstancePerLifetimeScope();
        }
    }
}
