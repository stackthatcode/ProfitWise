using Autofac;
using Push.Foundation.Utilities.CastleProxies;
using Push.Foundation.Web.Http;
using Push.Shopify.Aspect;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;
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
                .EnableClassInterceptorsWithRegistry(registry);

            builder.RegisterType<ProductApiRepository>()
                .EnableClassInterceptorsWithRegistry(registry);

            builder.RegisterType<EventApiRepository>()
                .EnableClassInterceptorsWithRegistry(registry);

            builder.RegisterType<ShopApiRepository>()
                .EnableClassInterceptorsWithRegistry(registry);

            builder.RegisterType<ApiRepositoryFactory>();
        }        
    }
}

