using Autofac;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Processes;
using ProfitWise.Data.RefreshServices;
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
            builder.RegisterType<ShopifyProductRepository>();
            builder.RegisterType<ShopifyVariantRepository>();            
            builder.RegisterType<ShopRepository>();
            builder.RegisterType<ProfitWiseBatchStateRepository>();
            builder.RegisterType<ProfitWiseProductRepository>();


            var registry = new InceptorRegistry();
            registry.Add(typeof(ExecutionTime));

            builder.RegisterType<ShopRefreshService>().EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<OrderRefreshService>().EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<ProductRefreshService>().EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<RefreshProcess>().EnableClassInterceptorsWithRegistry(registry);

            builder.RegisterType<ShopIdRequired>();
        }
    }
}
