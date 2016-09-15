using Autofac;
using Push.Foundation.Web.Interfaces;
using Push.Shopify.Interfaces;

namespace ProfitWise.DataMocks
{
    public class AutofacRegistration
    {
        public static void Build(ContainerBuilder builder)
        {
            builder.RegisterType<FakeCredentialService>()
                .As<IShopifyCredentialService>();

            builder.RegisterType<FakeOrderApiRepository>()
                .As<IOrderApiRepository>();

            builder.RegisterType<FakeProductApiRepository>()
                .As<IProductApiRepository>();

            builder.RegisterType<FakeEventApiRepository>()
                .As<IEventApiRepository>();

            builder.RegisterType<FakeShopApiRepository>()
                .As<IShopApiRepository>();
        }
    }
}

