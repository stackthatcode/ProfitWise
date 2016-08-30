using Autofac;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Processes;
using ProfitWise.Data.Steps;
using ProfitWise.Data.Repositories;
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

            builder.RegisterType<MultitenantRepositoryFactory>();
            builder.RegisterType<ShopifyOrderRepository>();
            builder.RegisterType<PwShopRepository>();
            builder.RegisterType<PwBatchStateRepository>();
            builder.RegisterType<PwProductRepository>();
            builder.RegisterType<PwVariantRepository>();
            builder.RegisterType<PwPreferencesRepository>();


            var registry = new InceptorRegistry();
            registry.Add(typeof(ExecutionTime));

            builder.RegisterType<ShopRefreshService>().EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<OrderRefreshService>().EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<ProductRefreshStep>().EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<ProductCleanupService>().EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<RefreshProcess>().EnableClassInterceptorsWithRegistry(registry);

            builder.RegisterType<ShopIdRequired>();
        }
    }
}
