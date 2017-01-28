using Autofac;
using ProfitWise.DataMocks;
using Push.Foundation.Web.Interfaces;
using Push.Shopify.Interfaces;

namespace ProfitWise.TestData.MockShopifyStore
{
    public class FakeApiAutofacRegistration
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

