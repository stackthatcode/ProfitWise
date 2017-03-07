using System.Configuration;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model.Billing;
using ProfitWise.Data.Repositories.System;
using ProfitWise.Data.Utility;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Json;
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
        private readonly MultitenantFactory _multitenantFactory;
    
        public WebhookService(
                ApiRepositoryFactory factory, 
                IShopifyCredentialService credentialService,
                ShopRepository shopRepository, 
                MultitenantFactory multitenantFactory)
        {
            _factory = factory;
            _credentialService = credentialService;
            _shopRepository = shopRepository;
            _multitenantFactory = multitenantFactory;
        }

        public static Webhook MakeUninstallHookRequest()
        {
            var request = new Webhook()
            {
                Address = "http://requestb.in/1bv0xys1",
                Format = "json",
                Topic = "app/uninstall",
            };
            return request;
        }        

        public Webhook CreateUninstallHook(string userId, string returnUrl)
        {
            var shopifyFromClaims = _credentialService.Retrieve(userId);
            var credentials = shopifyFromClaims.ToShopifyCredentials();
            var apiRepository = _factory.MakeWebhookApiRepository(credentials);

            var request = MakeUninstallHookRequest();
            var webhook = apiRepository.Subscribe(request);

            // TODO - update Shop with Web Hook Id
            //var shop = _shopRepository.RetrieveByUserId(userId);

            return webhook;            
        }

        public Webhook RetrieveUninstallHook(string userId)
        {
            var shopifyFromClaims = _credentialService.Retrieve(userId);
            var credentials = shopifyFromClaims.ToShopifyCredentials();
            var apiRepository = _factory.MakeWebhookApiRepository(credentials);

            //var shop = _shopRepository.RetrieveByUserId(userId);
            var id = 123214559; // TODO - get this from the Shop

            var webhook = apiRepository.Retrieve(id);
            return webhook;
        }
        
    }
}

