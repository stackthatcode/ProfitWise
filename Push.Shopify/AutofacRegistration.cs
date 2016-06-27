using Autofac;
using Push.Foundation.Utilities.CastleProxies;
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
            builder.RegisterType<ErrorForensics>();
            var registry = new InceptorRegistry();
            //registry.Add(typeof(ErrorForensics));

            builder.RegisterType<ShopifyCredentialRequired>();
            builder.RegisterType<ShopifyHttpClientConfig>();

            builder.RegisterType<HttpClient.HttpClient>()
                .As<IHttpClient>()
                .EnableClassInterceptorsWithRegistry(registry);

            builder
                .RegisterType<ShopifyHttpClient>()
                .As<IShopifyHttpClient>()
                .EnableClassInterceptorsWithRegistry(registry);

            builder
                .RegisterType<ShopifyRequestFactory>()
                .EnableClassInterceptorsWithRegistry(registry);

            builder
                .RegisterType<OrderApiRepository>()
                .EnableClassInterceptorsWithRegistry(registry);

            builder
                .RegisterType<ProductApiRepository>()
                .EnableClassInterceptorsWithRegistry(registry);

            builder.RegisterType<ApiRepositoryFactory>();
        }
        
    }
}
