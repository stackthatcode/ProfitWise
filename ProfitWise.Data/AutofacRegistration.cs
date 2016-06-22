using Autofac;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Processes;
using ProfitWise.Data.RefreshServices;
using ProfitWise.Data.Repositories;
using Push.Utilities.CastleProxies;
using Push.Utilities.Errors;

namespace ProfitWise.Data
{
    public class AutofacRegistration
    {
        public static void Build(ContainerBuilder builder)
        {
            builder.RegisterType<ErrorForensics>();
            var registry = new InceptorRegistry();
            registry.Add(typeof(ErrorForensics));

            builder.RegisterType<ShopifyProductRepository>().EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<ShopifyVariantRepository>().EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<MultitenantSqlRepositoryFactory>().EnableClassInterceptorsWithRegistry(registry);

            builder.RegisterType<OrderRefreshService>().EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<ProductRefreshService>().EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<RefreshProcess>().EnableClassInterceptorsWithRegistry(registry);

            builder.RegisterType<ShopIdRequired>();
        }
    }
}
