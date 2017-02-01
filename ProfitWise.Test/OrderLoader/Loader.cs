using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Push.Foundation.Web.Interfaces;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;
using Push.Shopify.Interfaces;
using Push.Shopify.Model;

namespace ProfitWise.Test.OrderLoader
{
    [TestClass]
    public class Loader
    {
        [TestMethod]
        public void Execute()
        {
            var container = ProfitWise.Batch.AutofacRegistration.Build();

            var userId = "7ab46e72-3b1c-4db1-88d4-f8a6b8f3e57a";

            using (var scope = container.BeginLifetimeScope())
            {
                // Step #1 - call Shopify and get our Product catalog
                var factory = scope.Resolve<ApiRepositoryFactory>();
                var credentialService = scope.Resolve<IShopifyCredentialService>();

                var claims = credentialService.Retrieve(userId);
                var credentials = new ShopifyCredentials()
                {
                    ShopOwnerUserId = claims.ShopOwnerUserId,
                    ShopDomain = claims.ShopDomain,
                    AccessToken = claims.AccessToken,
                };

                var productApiRepository = factory.MakeProductApiRepository(credentials);

                var filter = new ProductFilter();
                var products = productApiRepository.Retrieve(filter);

                // Step #2 - prepare a plan to upload random Orders

                

                // Step #3 - Insert Orders into Shopify API
            }
        }
    }
}
