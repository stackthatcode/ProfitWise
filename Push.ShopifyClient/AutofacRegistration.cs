using Autofac;
using Autofac.Extras.DynamicProxy2;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;
using Push.Shopify.Repositories;


namespace Push.Shopify
{
    public class AutofacRegistration
    {
        public static void Build(ContainerBuilder builder)
        {
            builder.RegisterType<ShopifyHttpClientConfig>();
            builder.RegisterType<HttpClient.HttpClient>().As<IHttpClient>().EnableClassInterceptors();
            builder.RegisterType<ShopifyHttpClient>().As<IShopifyHttpClient>().EnableClassInterceptors();
            builder.RegisterType<ShopifyRequestFactory>().EnableClassInterceptors();
            builder.RegisterType<OrderApiRepository>().EnableClassInterceptors();
            builder.RegisterType<ProductApiRepository>().EnableClassInterceptors();
            builder.RegisterType<ApiRepositoryFactory>();
        }
    }
}
