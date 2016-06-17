using Autofac;
using Autofac.Extras.DynamicProxy2;
using Push.Shopify.Aspect;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;
using Push.Shopify.Repositories;
using Push.Utilities.Errors;


namespace Push.Shopify
{
    public class AutofacRegistration
    {
        public static void Build(ContainerBuilder builder)
        {
            builder.RegisterType<ErrorForensics>();
            builder.RegisterType<ShopifyCredentialRequired>();

            builder.RegisterType<ShopifyHttpClientConfig>();

            builder.RegisterType<HttpClient.HttpClient>()
                .As<IHttpClient>()
                .EnableClassInterceptors()
                .InterceptedBy(typeof(ErrorForensics));

            builder
                .RegisterType<ShopifyHttpClient>()
                .As<IShopifyHttpClient>()
                .EnableClassInterceptors()
                .InterceptedBy(typeof(ErrorForensics));

            builder
                .RegisterType<ShopifyRequestFactory>()
                .EnableClassInterceptors()
                .InterceptedBy(typeof(ErrorForensics));

            builder
                .RegisterType<OrderApiRepository>()
                .EnableClassInterceptors()
                .InterceptedBy(typeof(ErrorForensics));

            builder
                .RegisterType<ProductApiRepository>()
                .EnableClassInterceptors()
                .InterceptedBy(typeof(ErrorForensics));

            builder.RegisterType<ApiRepositoryFactory>();
        }
        
    }
}
