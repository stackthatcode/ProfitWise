using ProfitWise.Data.Repositories.System;
using ProfitWise.Data.Utility;
using Push.Foundation.Web.Interfaces;
using Push.Shopify.Factories;
using Push.Shopify.Model;

namespace ProfitWise.Data.Services
{
    public class BillingService
    {
        private readonly ApiRepositoryFactory _factory;
        private readonly IShopifyCredentialService _credentialService;
        private readonly ShopRepository _shopRepository;

        public BillingService(
                ApiRepositoryFactory factory, 
                IShopifyCredentialService credentialService,
                ShopRepository shopRepository)
        {
            _factory = factory;
            _credentialService = credentialService;
            _shopRepository = shopRepository;
        }

        public static RecurringApplicationCharge MakeProfitWiseCharge()
        {
            var charge = new RecurringApplicationCharge()
            {
                name = "ProfitWise Monthly Charge",
                trial_days = 0,     //TODO - make configurable
                price = 14.95m,
                test = true,        // HIGH-PRIORITY - make configurable
            };
            return charge;
        }
        


        public RecurringApplicationCharge CreateCharge(string userId, string returnUrl)
        {
            var chargeParameter = MakeProfitWiseCharge();
            chargeParameter.return_url = returnUrl;
            
            var shopifyFromClaims = _credentialService.Retrieve(userId);
            var credentials = shopifyFromClaims.ToShopifyCredentials();

            var repository = _factory.MakeRecurringApiRepository(credentials);
            var chargeResult = repository.UpsertCharge(chargeParameter);

            var shop = _shopRepository.RetrieveByUserId(userId);
            _shopRepository.UpdateRecurringCharge(
                    shop.PwShopId, chargeResult.id, chargeResult.confirmation_url);

            return chargeParameter;
        }

        public RecurringApplicationCharge RetrieveCharge(string userId)
        {
            var shop = _shopRepository.RetrieveByUserId(userId);
            var shopifyFromClaims = _credentialService.Retrieve(userId);
            var credentials = shopifyFromClaims.ToShopifyCredentials();

            var repository = _factory.MakeRecurringApiRepository(credentials);
            var result = repository.RetrieveCharge(shop.ShopifyRecurringChargeId);

            return result;
        }
    }
}

