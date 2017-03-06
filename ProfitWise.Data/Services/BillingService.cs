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
    public class BillingService
    {
        private readonly ApiRepositoryFactory _factory;
        private readonly IShopifyCredentialService _credentialService;
        private readonly ShopRepository _shopRepository;
        private readonly MultitenantFactory _multitenantFactory;

        private static readonly bool TestRecurringCharges = 
            ConfigurationManager.AppSettings.GetAndTryParseAsBool("TestRecurringCharges", false);

        private const int DefaultFreeTrial = 14;
        private const decimal ProfitWiseMonthlyPrice = 14.95m;

        public BillingService(
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

        public static RecurringApplicationCharge MakeProfitWiseCharge()
        {
            var charge = new RecurringApplicationCharge()
            {
                name = "ProfitWise Monthly Charge",
                trial_days = DefaultFreeTrial,
                price = ProfitWiseMonthlyPrice,
                test = TestRecurringCharges ? true : (bool?)null,
            };
            return charge;
        }        

        public PwRecurringCharge CreateCharge(string userId, string returnUrl)
        {
            var shopifyFromClaims = _credentialService.Retrieve(userId);
            var credentials = shopifyFromClaims.ToShopifyCredentials();
            var repository = _factory.MakeRecurringApiRepository(credentials);

            // Get Billing Repository for User
            var shop = _shopRepository.RetrieveByUserId(userId);
            var billingRepository = _multitenantFactory.MakeBillingRepository(shop);

            // Invoke Shopify API to create the Recurring Application Charge
            var chargeParameter = MakeProfitWiseCharge();
            chargeParameter.return_url = returnUrl;

            // If a Free Trial Override has been set, then use that
            if (shop.TempFreeTrialOverride.HasValue)
            {
                chargeParameter.trial_days = shop.TempFreeTrialOverride.Value;
            }
            else if (billingRepository.AnyHistory())
            {
                chargeParameter.trial_days = 0;
            }

            var chargeResult = repository.UpsertCharge(chargeParameter);

            // Write a record in ProfitWise's Recurring Charge table
            using (var transaction = billingRepository.InitiateTransaction())
            {
                _shopRepository.UpdateTempFreeTrialOverride(shop.PwShopId, null);

                var nextChargeId = billingRepository.RetrieveNextKey();
                var charge = new PwRecurringCharge
                {
                    PwShopId = shop.PwShopId,
                    PwChargeId = nextChargeId,
                    IsPrimary = true,
                    ShopifyRecurringChargeId = chargeResult.id,
                    ConfirmationUrl = chargeResult.confirmation_url,
                    LastStatus = chargeResult.status.ToChargeStatus(),
                    LastJson = chargeResult.SerializeToJson(),
                };
                billingRepository.Insert(charge);

                // Important: Update Primary to nuke the previous Charge i.e. because its state went bad
                billingRepository.UpdatePrimary(nextChargeId);

                transaction.Commit();
                return charge;
            }
        }

        public PwRecurringCharge SyncAndRetrieveCurrentCharge(string userId)
        {
            var shopifyFromClaims = _credentialService.Retrieve(userId);
            var credentials = shopifyFromClaims.ToShopifyCredentials();
            var repository = _factory.MakeRecurringApiRepository(credentials);

            // Create Billing Repository and get the current primary Recurring Charge record
            var shop = _shopRepository.RetrieveByUserId(userId);
            var billingRepository = _multitenantFactory.MakeBillingRepository(shop);
            var currentCharge = billingRepository.RetrieveCurrent();
            if (currentCharge == null)
            {
                return null; 
            }

            // Invoke Shopify API to get the latest
            var result = repository.RetrieveCharge(currentCharge.ShopifyRecurringChargeId);

            // Update ProfitWise's local database record
            currentCharge.ConfirmationUrl = result.confirmation_url;
            currentCharge.LastStatus = result.status.ToChargeStatus();
            currentCharge.LastJson = result.SerializeToJson();
            billingRepository.Update(currentCharge);

            return currentCharge;
        }

        public PwRecurringCharge ActivateCharge(string userId, long chargeId)
        {
            var shop = _shopRepository.RetrieveByUserId(userId);
            var billingRepository = _multitenantFactory.MakeBillingRepository(shop);
            var charge = billingRepository.Retrieve(chargeId);
            return ActivateCharge(userId, charge);
        }

        public PwRecurringCharge ActivateCharge(string userId, PwRecurringCharge pwCharge)
        {
            var shop = _shopRepository.RetrieveByUserId(userId);
            var shopifyFromClaims = _credentialService.Retrieve(userId);
            var credentials = shopifyFromClaims.ToShopifyCredentials();
            
            // Invoke Shopify API to get the Charge, the Activate it
            var repository = _factory.MakeRecurringApiRepository(credentials);
            var existingCharge = repository.RetrieveCharge(pwCharge.ShopifyRecurringChargeId);
            var resultCharge = repository.ActivateCharge(existingCharge);

            // Update the ProfitWise database record with Activated Charge data
            var billingRepository = _multitenantFactory.MakeBillingRepository(shop);
            pwCharge.ConfirmationUrl = resultCharge.confirmation_url;
            pwCharge.LastStatus = resultCharge.status.ToChargeStatus();
            pwCharge.LastJson = resultCharge.SerializeToJson();
            billingRepository.Update(pwCharge);

            return pwCharge;
        }

        public void CancelCharge(string userId, long pwChargeId)
        {
            // Create Billing Repository and get the current primary Recurring Charge record
            var shop = _shopRepository.RetrieveByUserId(userId);
            var billingRepository = _multitenantFactory.MakeBillingRepository(shop);
            var charge = billingRepository.Retrieve(pwChargeId);
            if (charge == null)
            {
                return;
            }

            var shopifyFromClaims = _credentialService.Retrieve(userId);
            var credentials = shopifyFromClaims.ToShopifyCredentials();
            var apiRepository = _factory.MakeRecurringApiRepository(credentials);
            apiRepository.CancelCharge(charge.ShopifyRecurringChargeId);

            // Retrieve the Charge from Shopify API
            var result = apiRepository.RetrieveCharge(charge.ShopifyRecurringChargeId);

            // Update ProfitWise's local database record
            charge.ConfirmationUrl = result.confirmation_url;
            charge.LastStatus = result.status.ToChargeStatus();
            charge.LastJson = result.SerializeToJson();
            billingRepository.Update(charge);

            // Eliminate the Primary Charge
            billingRepository.ClearPrimary();

            // Mark Billing Valid to false
            _shopRepository.UpdateIsBillingValid(shop.PwShopId, false);
        }
    }
}

