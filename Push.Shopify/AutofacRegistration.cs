using Autofac;
using Push.Foundation.Utilities.CastleProxies;
using Push.Shopify.Aspect;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;
using Push.Shopify.Interfaces;
using Push.Shopify.Repositories;
using Push.Utilities.CastleProxies;


namespace Push.Shopify
{
    public class AutofacRegistration
    {
        public static void Build(ContainerBuilder builder)
        {
            builder.RegisterType<ShopifyCredentialRequired>();

            builder.RegisterType<ShopifyRequestFactory>();

            builder.RegisterType<ErrorForensics>();
            var registry = new InceptorRegistry();
            registry.Add(typeof(ExecutionTime));

            builder.RegisterType<OrderApiRepository>()
                .As<IOrderApiRepository>()
                .EnableClassInterceptorsWithRegistry(registry);

            builder.RegisterType<ProductApiRepository>()
                .As<IProductApiRepository>()
                .EnableClassInterceptorsWithRegistry(registry);

            builder.RegisterType<EventApiRepository>()
                .As<IEventApiRepository>()
                .EnableClassInterceptorsWithRegistry(registry);

            builder.RegisterType<ShopApiRepository>()
                .As<IShopApiRepository>()
                .EnableClassInterceptorsWithRegistry(registry);

            builder.RegisterType<RecurringApiRepository>()
                .As<IRecurringApiRepository>()
                .EnableClassInterceptorsWithRegistry(registry);

            builder.RegisterType<ApiRepositoryFactory>();

        }        
    }
}

