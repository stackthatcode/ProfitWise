using Autofac;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.ExchangeRateApis;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Processes;
using ProfitWise.Data.ProcessSteps;
using ProfitWise.Data.Repositories;
using ProfitWise.Data.Services;
using Push.Foundation.Utilities.CastleProxies;
using Push.Shopify.TimeZone;
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
            builder.RegisterType<PwCogsRepository>();
            builder.RegisterType<CurrencyRepository>();
            builder.RegisterType<PwPickListRepository>();

            var registry = new InceptorRegistry();
            registry.Add(typeof(ExecutionTime));

            builder.RegisterType<ShopRefreshService>().EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<OrderRefreshStep>().EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<ProductRefreshStep>().EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<ProductCleanupStep>().EnableClassInterceptorsWithRegistry(registry);

            builder.RegisterType<CatalogBuilderService>().EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<CurrencyService>().EnableClassInterceptorsWithRegistry(registry);

            builder.RegisterType<RefreshProcess>().EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<CurrencyProcess>().EnableClassInterceptorsWithRegistry(registry);

            builder.RegisterType<ShopRequired>();

            builder.RegisterType<FixerApiConfig>();
            builder.RegisterType<FixerApiRepository>();
            builder.RegisterType<FixerApiRequestFactory>();

            // Chicago, by default!
            builder.Register(x => new TimeZoneTranslator(6, 0));
        }
    }
}
