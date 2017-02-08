using Autofac;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.ExchangeRateApis;
using ProfitWise.Data.Factories;
using ProfitWise.Data.HangFire;
using ProfitWise.Data.Processes;
using ProfitWise.Data.ProcessSteps;
using ProfitWise.Data.Repositories;
using ProfitWise.Data.Services;
using Push.Foundation.Utilities.CastleProxies;
using Push.Utilities.CastleProxies;


namespace ProfitWise.Data
{
    public class AutofacRegistration
    {
        public static void Build(ContainerBuilder builder)
        {
            builder.RegisterType<ErrorForensics>();
            builder.RegisterType<ExecutionTime>();

            builder.RegisterType<MultitenantFactory>();

            builder.RegisterType<ShopifyOrderRepository>();
            builder.RegisterType<PwShopRepository>();
            builder.RegisterType<PwBatchStateRepository>();
            builder.RegisterType<PwProductRepository>();
            builder.RegisterType<PwVariantRepository>();
            builder.RegisterType<PwCogsEntryRepository>();
            builder.RegisterType<ExchangeRateRepository>();
            builder.RegisterType<PwPickListRepository>();
            builder.RegisterType<PwReportRepository>();
            builder.RegisterType<PwReportFilterRepository>();
            builder.RegisterType<PwReportQueryRepository>();
            builder.RegisterType<SystemRepository>();
            builder.RegisterType<PwCogsDataUpdateRepository>();
            
            var registry = new InceptorRegistry();
            registry.Add(typeof(ExecutionTime));
            
            builder.RegisterType<OrderRefreshStep>();
            builder.RegisterType<ProductRefreshStep>();
            builder.RegisterType<ProductCleanupStep>();

            builder.RegisterType<ShopRefreshService>();
            builder.RegisterType<CatalogBuilderService>();
            builder.RegisterType<CatalogRetrievalService>();
            builder.RegisterType<CurrencyService>();
            builder.RegisterType<ShopSynchronizationService>();
            builder.RegisterType<CogsService>();
            builder.RegisterType<HangFireService>();

            builder.RegisterType<ShopRefreshProcess>();
            builder.RegisterType<ExchangeRateRefreshProcess>();
            builder.RegisterType<SystemCleanupProcess>();
            
            builder.RegisterType<ShopRequired>();

            builder.RegisterType<FixerApiConfig>();
            builder.RegisterType<FixerApiRepository>();
            builder.RegisterType<FixerApiRequestFactory>();

            // Chicago, by default!
            builder.Register(x => new TimeZoneTranslator(6, 0));
        }
    }
}
