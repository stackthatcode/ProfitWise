using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Autofac;
using Hangfire;
using ProfitWise.Data.ExchangeRateApis;
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
        public static IContainer Build()
        {
            // Autofac registration sequence
            var builder = new ContainerBuilder();


            // Logging and metering interceptor
            builder.Register(c => LoggerSingleton.Get()).As<IPushLogger>();
            var registry = new InceptorRegistry();
            registry.Add(typeof(ErrorForensics));


            // ProfitWise.Data registration - which invokes downstream dependent registrations
            Data.AutofacRegistration.Build(builder);

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
                .InstancePerBackgroundJob();


            // Configure the paging rates of the Refresh Services
            builder.Register(x => new RefreshServiceConfiguration()
            {
                MaxOrderRate =
                    ConfigurationManager.AppSettings.GetAndTryParseAsInt("RefreshServiceMaxOrderRate", 50),
                MaxProductRate =
                    ConfigurationManager.AppSettings.GetAndTryParseAsInt("RefreshServiceMaxProductRate", 100),
            });


            // Push.Shopify API configuration
            Push.Shopify.AutofacRegistration.Build(builder);

            builder.Register(x => new ShopifyClientConfig()
            {
                RetryLimit =
                    ConfigurationManager.AppSettings.GetAndTryParseAsInt("ShopifyRetryLimit", 3),
                Timeout =
                    ConfigurationManager.AppSettings.GetAndTryParseAsInt("ShopifyHttpTimeout", 60000),
                ThrottlingDelay =
                    ConfigurationManager.AppSettings.GetAndTryParseAsInt("ShopifyThrottlingDelay", 500),
                RetriesEnabled = 
                    ConfigurationManager.AppSettings.GetAndTryParseAsBool("ShopifyRetriesEnabled", true)
            });


            // Exchange Rate API configuration
            builder.Register(x => new FixerApiConfig()
            {
                RetryLimit =
                    ConfigurationManager.AppSettings.GetAndTryParseAsInt("FixerApiRetryLimit", 3),
                Timeout =
                    ConfigurationManager.AppSettings.GetAndTryParseAsInt("FixerApiHttpTimeout", 60000),
                ThrottlingDelay =
                    ConfigurationManager.AppSettings.GetAndTryParseAsInt("FixerApiThrottlingDelay", 500),
                RetriesEnabled =
                    ConfigurationManager.AppSettings.GetAndTryParseAsBool("FixerApiRetriesEnabled", true)
            });

            // Timezone
            var machineTimeZone = ConfigurationManager.AppSettings["Machine_TimeZone"];
            builder.Register(x => new TimeZoneTranslator(machineTimeZone));            

            // Push.Foundation.Web Identity Stuff
            var encryption_key = ConfigurationManager.AppSettings["security_aes_key"];
            var encryption_iv = ConfigurationManager.AppSettings["security_aes_iv"];

            Push.Foundation.Web.AutofacRegistration.Build(builder, encryption_key, encryption_iv);


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
}
