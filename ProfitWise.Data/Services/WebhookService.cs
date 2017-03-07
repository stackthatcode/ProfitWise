using ProfitWise.Data.Repositories.System;
using ProfitWise.Data.Utility;
using Push.Foundation.Web.Interfaces;
using Push.Shopify.Factories;
using Push.Shopify.Model;

namespace ProfitWise.Data.Services
{
    public class WebhookService
    {
        private readonly ApiRepositoryFactory _factory;
        private readonly IShopifyCredentialService _credentialService;
        private readonly ShopRepository _shopRepository;

        public WebhookService(
                ApiRepositoryFactory factory, 
                IShopifyCredentialService credentialService,
                ShopRepository shopRepository)
        {
            _factory = factory;
            _credentialService = credentialService;
            _shopRepository = shopRepository;
        }

        
        public Webhook RetrieveUninstallHook(string userId)
        {
            var shopifyFromClaims = _credentialService.Retrieve(userId);
            var credentials = shopifyFromClaims.ToShopifyCredentials();
            var apiRepository = _factory.MakeWebhookApiRepository(credentials);

            var shop = _shopRepository.RetrieveByUserId(userId);

            if (shop.ShopifyUninstallId.HasValue)
            {
                var webhook = apiRepository.Retrieve(shop.ShopifyUninstallId.Value);
                return webhook;
            }
            else
            {
                return null;
            }
        }        
    }
}

