using Autofac;
using Autofac.Extras.DynamicProxy2;
using ProfitWise.Batch.Factory;
using ProfitWise.Batch.Orders;
using ProfitWise.Batch.Products;
using ProfitWise.Data.Repositories;
using Push.Shopify.HttpClient;
using Push.Shopify.Repositories;
using Push.Utilities.Logging;
using MySqlConnectionFactory = MySql.Data.Entity.MySqlConnectionFactory;

namespace ProfitWise.Batch
{
    public class AutofacRegistration
    {
        public IContainer Build()
        {
            var builder = new ContainerBuilder();

            // Push.Shopify classes
            builder.RegisterType<HttpClient>().As<IHttpClient>().EnableClassInterceptors();
            builder.RegisterType<ShopifyHttpClient>().As<IShopifyHttpClient>().EnableClassInterceptors();
            builder.RegisterType<OrderApiRepository>().EnableClassInterceptors();
            builder.RegisterType<ProductApiRepository>().EnableClassInterceptors();

            // ProfitWise.Data classes
            builder.RegisterType<ProductDataRepository>().EnableClassInterceptors();
            builder.RegisterType<VariantDataRepository>().EnableClassInterceptors();

            // ProfitWise.Batch classes
            builder.RegisterType<MySqlConnectionFactory>().EnableClassInterceptors();
            builder.RegisterType<ShopifyHttpClientFactory>().As<IShopifyClientFactory>().EnableClassInterceptors();
            builder.RegisterType<OrderRefreshService>().EnableClassInterceptors();
            builder.RegisterType<ProductRefreshService>().EnableClassInterceptors();


            // Infrastructure setup & configuration binding
            builder.Register(c => new NLoggerImpl("Test.Logger", x => x)).As<ILogger>();
            builder.Register(c => new LoggingInterceptor(c.Resolve<ILogger>()));

            return builder.Build();
        }
    }
}
