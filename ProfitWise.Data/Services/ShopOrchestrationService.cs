using System;
using System.Configuration;
using System.Data;
using ProfitWise.Data.Database;
using ProfitWise.Data.Factories;
using ProfitWise.Data.HangFire;
using ProfitWise.Data.Model.Billing;
using ProfitWise.Data.Model.Shop;
using ProfitWise.Data.Repositories.System;
using ProfitWise.Data.Utility;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Interfaces;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;

namespace ProfitWise.Data.Services
{
    public class ShopOrchestrationService
    {
        private readonly CurrencyService _currencyService;
        private readonly ShopRepository _shopRepository;
        private readonly MultitenantFactory _factory;
        private readonly ApiRepositoryFactory _apifactory;
        private readonly ConnectionWrapper _connectionWrapper;
        private readonly HangFireService _hangFireService;
        private readonly IShopifyCredentialService _credentialService;
        private readonly IPushLogger _logger;


        private readonly int _orderStartOffsetMonths = 
                ConfigurationManager.AppSettings.GetAndTryParseAsInt("InitialOrderStartDateOffsetMonths", 3);

        private static readonly bool TestRecurringCharges =
                ConfigurationManager.AppSettings.GetAndTryParseAsBool("TestRecurringCharges", false);

        private const int DefaultFreeTrial = 14;
        private const decimal ProfitWiseMonthlyPrice = 14.95m;


        public ShopOrchestrationService(
                    CurrencyService currencyService, 
                    ShopRepository shopRepository, 
                    MultitenantFactory factory,
                    ConnectionWrapper connectionWrapper, 
                    ApiRepositoryFactory apifactory, 
                    HangFireService hangFireService, 
                    IShopifyCredentialService credentialService,
                    IPushLogger logger)
        {
            _currencyService = currencyService;
            _shopRepository = shopRepository;
            _factory = factory;
            _logger = logger;
            _connectionWrapper = connectionWrapper;
            _apifactory = apifactory;
            _hangFireService = hangFireService;
            _credentialService = credentialService;
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.InitiateTransaction();
        }
        

        // ProfitWise Shop dawtabase record methods
        public PwShop CreateShop(string shopOwnerUserId, Shop shop, ShopifyCredentials credentials)
        {
            // Create the Shop record in SQL
            var currencyId = _currencyService.AbbreviationToCurrencyId(shop.Currency);
            var newShop = PwShop.Make(
                shopOwnerUserId, shop.Id, currencyId, shop.TimeZone, shop.Domain, _orderStartOffsetMonths);
            newShop.PwShopId = _shopRepository.Insert(newShop);
            _logger.Info($"Created new Shop - UserId: {newShop.ShopOwnerUserId}");

            // Create the Batch State for Shop
            var profitWiseBatchStateRepository = _factory.MakeBatchStateRepository(newShop);
            var state = new PwBatchState
            {
                PwShopId = newShop.PwShopId,
            };
            profitWiseBatchStateRepository.Insert(state);
            _logger.Info($"Created Batch State for Shop - UserId: {newShop.ShopOwnerUserId}");

            // Create the Webhook via Shopify API
            var apiRepository = _apifactory.MakeWebhookApiRepository(credentials);            
            var request = Webhook.MakeUninstallHookRequest();
            var webhook = apiRepository.Subscribe(request);

            // Store the Webhook Id 
            _shopRepository.UpdateShopifyUninstallId(shopOwnerUserId, webhook.Id);
            return newShop;
        }

        public void UpdateShop(string userId, string currencySymbol, string timezone)
        {
            var pwShop = _shopRepository.RetrieveByUserId(userId);
            var currencyId = _currencyService.AbbreviationToCurrencyId(currencySymbol);

            pwShop.CurrencyId = currencyId;
            pwShop.TimeZone = timezone;

            _shopRepository.Update(pwShop);
            _shopRepository.UpdateIsAccessTokenValid(pwShop.PwShopId, true);
            _shopRepository.UpdateIsProfitWiseInstalled(pwShop.PwShopId, true, null);

            _logger.Debug($"Updated Shop - UserId: {pwShop.ShopOwnerUserId}");
        }


        // Recurring Application Charge methods
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
            var repository = _apifactory.MakeRecurringApiRepository(credentials);

            // Get Billing Repository for User
            var shop = _shopRepository.RetrieveByUserId(userId);
            var billingRepository = _factory.MakeBillingRepository(shop);

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
            var repository = _apifactory.MakeRecurringApiRepository(credentials);

            // Create Billing Repository and get the current primary Recurring Charge record
            var shop = _shopRepository.RetrieveByUserId(userId);
            var billingRepository = _factory.MakeBillingRepository(shop);
            var currentCharge = billingRepository.RetrieveCurrent();
            if (currentCharge == null)
            {
                return null;
            }

            // Invoke Shopify API to get the Charge
            var result = repository.RetrieveCharge(currentCharge.ShopifyRecurringChargeId);

            // Update ProfitWise's local database record
            currentCharge.ConfirmationUrl = result.confirmation_url;
            currentCharge.LastStatus = result.status.ToChargeStatus();
            currentCharge.LastJson = result.SerializeToJson();
            billingRepository.Update(currentCharge);

            return currentCharge;
        }

        public bool VerifyChargeAcceptedByUser(string userId)
        {
            var charge = SyncAndRetrieveCurrentCharge(userId);
            var shop = _shopRepository.RetrieveByUserId(userId);

            if (charge.IsValid)
            {
                _shopRepository.UpdateIsBillingValid(shop.PwShopId, true);
                _hangFireService.AddOrUpdateInitialShopRefresh(userId);
                return true;
            }
            else
            {
                var billingRepository = _factory.MakeBillingRepository(shop);
                billingRepository.ClearPrimary();

                _shopRepository.UpdateIsBillingValid(shop.PwShopId, false);
                return false;
            }
        }

        public PwRecurringCharge ActivateCharge(string userId, long pwChargeId)
        {
            // Retrieve Credentials from Claims
            var shop = _shopRepository.RetrieveByUserId(userId);
            var shopifyFromClaims = _credentialService.Retrieve(userId);
            var credentials = shopifyFromClaims.ToShopifyCredentials();

            // Get the Charge from the ProfitWise database
            var billingRepository = _factory.MakeBillingRepository(shop);
            var pwCharge = billingRepository.Retrieve(pwChargeId);

            // Invoke Shopify API to get the Charge, the Activate it
            var repository = _apifactory.MakeRecurringApiRepository(credentials);
            var existingCharge = repository.RetrieveCharge(pwCharge.ShopifyRecurringChargeId);
            var resultCharge = repository.ActivateCharge(existingCharge);

            // Update the ProfitWise database record with Activated Charge data
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
            var billingRepository = _factory.MakeBillingRepository(shop);
            var charge = billingRepository.Retrieve(pwChargeId);
            if (charge == null)
            {
                return;
            }

            // Create Shopify API and cancel the Charge in Shopify
            var shopifyFromClaims = _credentialService.Retrieve(userId);
            var credentials = shopifyFromClaims.ToShopifyCredentials();
            var apiRepository = _apifactory.MakeRecurringApiRepository(credentials);
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


        // Uninstallation process
        public void UninstallShop(long shopifyShopId)
        {
            var shop = _shopRepository.RetrieveByShopifyShopId(shopifyShopId);
            _shopRepository.UpdateIsProfitWiseInstalled(shop.PwShopId, false, DateTime.Now);
        }

        public void FinalizeUninstallation(int pwShopId)
        {
            var shop = _shopRepository.RetrieveByShopId(pwShopId);

            // Kill the Recurring Shop Refresh Job
            var batchRepository = _factory.MakeBatchStateRepository(shop);
            var batchState = batchRepository.Retrieve();
            if (batchState.RoutineRefreshJobId != null)
            {
                _hangFireService.KillRecurringJob(batchState.RoutineRefreshJobId);
            }

            // Cancel the Charge
            var billingRepository = _factory.MakeBillingRepository(shop);
            var currentCharge = billingRepository.RetrieveCurrent();
            if (currentCharge != null)
            {
                CancelCharge(shop.ShopOwnerUserId, currentCharge.PwChargeId);
            }
        }
    }
}

